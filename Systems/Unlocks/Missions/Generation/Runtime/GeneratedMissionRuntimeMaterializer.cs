using System;
using System.Collections.Generic;

using BepInEx.Logging;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G8 - materialización de los blueprints Generated de G7.
    ///
    /// Desde Phase 1.5 la Mission V2 Generated se guarda en Survivors.json
    /// con Source=Generated. Nunca se guarda como CustomMission salvo que el
    /// jugador la edite mediante el flujo Custom.
    ///
    /// Reglas de autoridad:
    /// - Official/CUSTOM/otra misión autorada siempre la suprimen;
    /// - el overlay se aplica sólo sobre snapshots/lecturas efectivas;
    /// - el target jamás se añade como performer.
    ///
    /// Las familias materializables actuales son CombatKills, Execution,
    /// MeleeCombat, ProjectileCombat, ResourceMastery y HealingSupport. G7
    /// limita la selección a ellas para que todo candidato pueda convertirse
    /// inmediatamente en Mission V2.
    /// </summary>
    public static class GeneratedMissionRuntimeMaterializer
    {
        private sealed class RuntimeMaterialization
        {
            public GeneratedMissionCandidate Candidate;
            public MissionDefinition Mission;
            public int PerformerCount;
            public int PerformerSkillCount;
        }


        private static readonly Dictionary<
            string,
            RuntimeMaterialization
        > MaterializationsByBody =
            new Dictionary<
                string,
                RuntimeMaterialization
            >(StringComparer.OrdinalIgnoreCase);


        private static IReadOnlyList<SurvivorInfo> survivors;
        private static IReadOnlyList<MissionProfile> currentProfiles;
        private static ManualLogSource logger;


        public static int Count =>
            MaterializationsByBody.Count;


        public static void Initialize(
            IReadOnlyList<SurvivorInfo> installedSurvivors,
            IReadOnlyList<MissionProfile> profiles,
            ManualLogSource activeLogger
        )
        {
            survivors = installedSurvivors;
            currentProfiles = profiles;
            logger = activeLogger;

            MaterializationsByBody.Clear();

            IReadOnlyList<GeneratedMissionCandidate> candidates =
                DeterministicMissionGeneratorService.GetAll();

            int unsupported = 0;
            int invalid = 0;

            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    GeneratedMissionCandidate candidate =
                        candidates[i];

                    if (candidate == null)
                    {
                        continue;
                    }

                    RuntimeMaterialization materialization =
                        BuildMaterialization(
                            candidate,
                            profiles
                        );

                    if (materialization == null)
                    {
                        if (IsSupportedFamily(candidate.Family))
                        {
                            invalid++;
                        }
                        else
                        {
                            unsupported++;
                        }

                        continue;
                    }

                    MaterializationsByBody[
                        candidate.BodyName ?? ""
                    ] = materialization;
                }
            }

            UsuLog.Diagnostic(
                logger,
                "[GENERATED RUNTIME] G8 Phase 1 construido | " +
                $"Candidates: {candidates?.Count ?? 0} | " +
                $"Materialized: {MaterializationsByBody.Count} | " +
                $"Unsupported families: {unsupported} | " +
                $"Invalid materializations: {invalid} | " +
                "Snapshot overlay: yes | " +
                "Authored authority recheck: yes | " +
                "Target usage: no | " +
                "Persistencia: Generated JSON"
            );

            LogVerboseMaterializations();
        }


        public static bool RebuildCurrent()
        {
            if (currentProfiles == null)
            {
                return false;
            }

            Initialize(
                survivors,
                currentProfiles,
                logger
            );

            return true;
        }


        /// <summary>
        /// Crea de forma explícita una Generated para un único survivor modded.
        /// Se usa desde Mission Library cuando el jugador pulsa Generada y aún
        /// no existe ese slot. Original/USU/Custom se conservan como slots; la
        /// selección activa se realiza después mediante SelectGenerated.
        /// </summary>
        public static bool TryGenerateForBodyOnDemand(
            string bodyName,
            ManualLogSource activeLogger,
            out string error
        )
        {
            error = "";

            string normalized = bodyName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(normalized))
            {
                error = "BodyName vacío.";
                return false;
            }

            if (currentProfiles == null)
            {
                error = "El contexto de perfiles G8 todavía no está disponible.";
                return false;
            }

            string nonce = Guid.NewGuid().ToString("N");

            if (
                !DeterministicMissionGeneratorService.TryPreviewForBody(
                    normalized,
                    nonce,
                    out GeneratedMissionCandidate candidate,
                    true
                ) ||
                candidate == null
            )
            {
                error = "G7 no encontró una misión compatible para este survivor.";
                return false;
            }

            RuntimeMaterialization materialization =
                BuildMaterialization(candidate, currentProfiles);

            if (materialization == null || materialization.Mission == null)
            {
                error = "G8 no pudo materializar la misión generada.";
                return false;
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(normalized);

            if (entry == null)
            {
                error = "El survivor no existe en Survivors.json.";
                return false;
            }

            SurvivorChallengeJson generated =
                BuildPersistedChallenge(
                    candidate,
                    materialization.Mission,
                    entry.Challenge
                );

            GeneratedMissionSnapshot snapshot =
                generated?.MissionConfig?.GeneratedMission;

            if (generated == null || snapshot == null)
            {
                error = "No se pudo crear el snapshot Generated.";
                return false;
            }

            string authorityDetail;
            GeneratedMissionAuthorityKind authority =
                GeneratedMissionAuthorityProbe.Detect(
                    survivors,
                    candidate.SurvivorId,
                    candidate.BodyName,
                    out authorityDetail
                );

            if (authority == GeneratedMissionAuthorityKind.None)
            {
                entry.Challenge = generated;
            }
            else
            {
                if (entry.Challenge == null)
                {
                    entry.Challenge =
                        new SurvivorChallengeJson
                        {
                            Enabled = true,
                            Name = "",
                            Description = "",
                            LocalizationKey = "",
                            LunarCoinReward = 0,
                            Type = "MissionV2",
                            Mission = null,
                            MissionConfig =
                                new MissionConfiguration
                                {
                                    Provider =
                                        authority == GeneratedMissionAuthorityKind.OriginalUnlock
                                            ? UnlockProviderKind.Original
                                            : UnlockProviderKind.USU,
                                    SelectionMode = UnlockSelectionMode.AutomaticFallback,
                                    Source = "Preset",
                                    GeneratedMission = snapshot,
                                    GeneratedSuppressed = false
                                }
                        };
                }
                else
                {
                    MissionConfiguration config =
                        entry.Challenge.MissionConfig ??
                        new MissionConfiguration();

                    entry.Challenge.MissionConfig = config;
                    MissionProviderSlotStore.CaptureActiveSlots(entry.Challenge);
                    config.GeneratedMission = snapshot;
                    config.GeneratedSuppressed = false;
                }
            }

            if (!SurvivorJsonManager.TrySaveCurrentConfig(activeLogger ?? logger))
            {
                error = "No se pudo guardar Survivors.json.";
                return false;
            }

            return true;
        }


        /// <summary>
        /// Persiste las Generated materializadas en la misma configuración
        /// Mission V2 que usa Custom. Source=Generated las mantiene editables
        /// como base pero actualizables automáticamente por G7/G8.
        /// </summary>
        public static int PersistGeneratedMissions()
        {
            int updated = 0;

            foreach (
                KeyValuePair<string, RuntimeMaterialization> pair
                in MaterializationsByBody
            )
            {
                RuntimeMaterialization materialization =
                    pair.Value;

                GeneratedMissionCandidate candidate =
                    materialization?.Candidate;

                if (
                    candidate == null ||
                    materialization.Mission == null
                )
                {
                    continue;
                }

                SurvivorJsonEntry entry =
                    SurvivorJsonManager.GetEntryAnywhere(
                        candidate.BodyName
                    );

                if (entry == null)
                {
                    continue;
                }

                string authorityDetail;

                GeneratedMissionAuthorityKind authority =
                    GeneratedMissionAuthorityProbe.Detect(
                        survivors,
                        candidate.SurvivorId,
                        candidate.BodyName,
                        out authorityDetail
                    );

                if (authority != GeneratedMissionAuthorityKind.None)
                {
                    if (!TemporaryGeneratedTestOptions.CanGenerateAlongside(authority))
                    {
                        continue;
                    }

                    SurvivorChallengeJson generatedDormant =
                        BuildPersistedChallenge(
                            candidate,
                            materialization.Mission,
                            entry.Challenge
                        );

                    GeneratedMissionSnapshot dormantSnapshot =
                        generatedDormant?.MissionConfig?.GeneratedMission;

                    if (dormantSnapshot == null)
                    {
                        continue;
                    }

                    // Un survivor con Original puede no tener Challenge porque
                    // USU no lo controlaba. Creamos sólo un contenedor de slots
                    // con Provider=Original; la Generated queda DORMIDA y no
                    // toma autoridad hasta que el jugador la seleccione.
                    if (entry.Challenge == null)
                    {
                        entry.Challenge =
                            new SurvivorChallengeJson
                            {
                                Enabled = true,
                                Name = "",
                                Description = "",
                                LocalizationKey = "",
                                LunarCoinReward = 0,
                                Type = "MissionV2",
                                Mission = null,
                                MissionConfig =
                                    new MissionConfiguration
                                    {
                                        Provider = UnlockProviderKind.Original,
                                        SelectionMode = UnlockSelectionMode.AutomaticFallback,
                                        Source = "Preset",
                                        GeneratedMission = dormantSnapshot
                                    }
                            };

                        updated++;
                        continue;
                    }

                    MissionConfiguration activeConfig =
                        entry.Challenge.MissionConfig;

                    if (activeConfig == null)
                    {
                        activeConfig = new MissionConfiguration();

                        if (authority == GeneratedMissionAuthorityKind.OriginalUnlock)
                        {
                            activeConfig.Provider = UnlockProviderKind.Original;
                            activeConfig.SelectionMode = UnlockSelectionMode.AutomaticFallback;
                        }

                        entry.Challenge.MissionConfig = activeConfig;
                    }

                    string beforeDormant =
                        activeConfig.GeneratedMission == null
                            ? ""
                            : Newtonsoft.Json.JsonConvert.SerializeObject(
                                activeConfig.GeneratedMission,
                                Newtonsoft.Json.Formatting.None
                            );

                    string afterDormant =
                        Newtonsoft.Json.JsonConvert.SerializeObject(
                            dormantSnapshot,
                            Newtonsoft.Json.Formatting.None
                        );

                    if (!string.Equals(beforeDormant, afterDormant, StringComparison.Ordinal))
                    {
                        activeConfig.GeneratedMission = dormantSnapshot;
                        updated++;
                    }

                    continue;
                }

                SurvivorChallengeJson generated =
                    BuildPersistedChallenge(
                        candidate,
                        materialization.Mission,
                        entry.Challenge
                    );

                string before =
                    entry.Challenge == null
                        ? ""
                        : Newtonsoft.Json.JsonConvert.SerializeObject(
                            entry.Challenge,
                            Newtonsoft.Json.Formatting.None
                        );

                string after =
                    Newtonsoft.Json.JsonConvert.SerializeObject(
                        generated,
                        Newtonsoft.Json.Formatting.None
                    );

                if (string.Equals(before, after, StringComparison.Ordinal))
                {
                    continue;
                }

                entry.Challenge = generated;
                updated++;

                UsuLog.Verbose(
                    logger,
                    "[GENERATED PERSISTED] " +
                    candidate.DisplayName +
                    " | Id: " +
                    candidate.CandidateId +
                    " | Family: " +
                    candidate.Family +
                    " | Nonce: " +
                    candidate.GenerationNonce
                );
            }

            if (
                updated > 0 &&
                !SurvivorJsonManager.TrySaveCurrentConfig(logger)
            )
            {
                logger?.LogWarning(
                    "[GENERATED PERSISTENCE] No se pudo guardar Survivors.json. " +
                    "Las Generated seguirán activas sólo durante esta sesión."
                );
            }

            UsuLog.Diagnostic(
                logger,
                "[GENERATED PERSISTENCE] G8 Phase 1.5 | " +
                $"Materialized: {MaterializationsByBody.Count} | " +
                $"Updated JSON: {updated} | Source: Generated | " +
                "Legacy KillEnemies100: disabled"
            );

            return updated;
        }


        private static SurvivorChallengeJson BuildPersistedChallenge(
            GeneratedMissionCandidate candidate,
            MissionDefinition mission,
            SurvivorChallengeJson previousChallenge
        )
        {
            MissionConfiguration previousConfig =
                previousChallenge?.MissionConfig;

            MissionDefinition preservedCustomMission =
                previousConfig?.CustomMission == null
                    ? null
                    : CloneMission(previousConfig.CustomMission);

            CustomMissionSnapshot preservedCustomSnapshot =
                previousConfig?.CustomMissionSnapshot == null
                    ? null
                    : JToken
                        .FromObject(previousConfig.CustomMissionSnapshot)
                        .ToObject<CustomMissionSnapshot>();

            string preservedUsuPresetId =
                MissionProviderSlotStore.ResolveRememberedUsuPresetId(
                    previousChallenge
                );

            UnlockSelectionMode selectionMode =
                previousConfig != null &&
                string.Equals(
                    previousConfig.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? previousConfig.EffectiveSelectionMode
                    : UnlockSelectionMode.AutomaticFallback;

            SurvivorChallengeJson challenge =
                new SurvivorChallengeJson
                {
                    Enabled = true,
                    Name = BuildChallengeName(candidate),
                    Description = BuildChallengeDescription(candidate),
                    LocalizationKey = "",
                    LunarCoinReward = 0,
                    Type = "GeneratedMission",
                    Parameters = BuildGeneratedMetadata(candidate),
                    Mission = CloneMission(mission),
                    MissionConfig =
                        new MissionConfiguration
                        {
                            Provider = UnlockProviderKind.USU,
                            SelectionMode = selectionMode,
                            Source = "Generated",
                            BasePresetId = candidate?.CandidateId ?? "",
                            UsuPresetId = preservedUsuPresetId,
                            CustomMissionSnapshot = preservedCustomSnapshot,
                            CustomMission = preservedCustomMission
                        }
                };

            if (
                GeneratedMissionPresentationFormatter.TryResolve(
                    challenge,
                    out string localizedName,
                    out string localizedDescription
                )
            )
            {
                challenge.Name = localizedName;
                challenge.Description = localizedDescription;
            }

            GeneratedMissionPersistenceFingerprint
                .Stamp(challenge);

            // Mantiene una copia dormida independiente para poder alternar
            // Generated <-> USU/Custom/Original sin destruir esta misión.
            MissionProviderSlotStore
                .CaptureActiveGenerated(challenge);

            return challenge;
        }


        /// <summary>
        /// Aplica la definición Generated sobre una COPIA JObject de la entrada
        /// efectiva. Nunca debe recibir el JObject persistido como destino.
        /// </summary>
        public static bool TryOverlay(
            string bodyName,
            JObject entryJson
        )
        {
            if (
                string.IsNullOrWhiteSpace(bodyName) ||
                entryJson == null ||
                !MaterializationsByBody.TryGetValue(
                    bodyName,
                    out RuntimeMaterialization materialization
                ) ||
                materialization == null ||
                materialization.Candidate == null ||
                materialization.Mission == null
            )
            {
                return false;
            }

            GeneratedMissionCandidate candidate =
                materialization.Candidate;

            string authorityDetail;

            GeneratedMissionAuthorityKind authority =
                GeneratedMissionAuthorityProbe.Detect(
                    survivors,
                    candidate.SurvivorId,
                    candidate.BodyName,
                    out authorityDetail
                );

            if (authority != GeneratedMissionAuthorityKind.None)
            {
                UsuLog.Verbose(
                    logger,
                    "[GENERATED RUNTIME SUPPRESSED] " +
                    $"{candidate.DisplayName} | " +
                    $"Id: {candidate.CandidateId} | " +
                    $"Authority: {authority} | " +
                    $"Detail: {authorityDetail}"
                );

                return false;
            }

            SurvivorJsonEntry sourceEntry;

            try
            {
                sourceEntry =
                    entryJson.ToObject<SurvivorJsonEntry>();
            }
            catch (Exception exception)
            {
                logger?.LogWarning(
                    "[GENERATED RUNTIME] No se pudo leer la entrada " +
                    $"temporal de {bodyName}; Generated no se aplicó."
                );

                UsuLog.Verbose(
                    logger,
                    exception.ToString()
                );

                return false;
            }

            if (sourceEntry == null)
            {
                return false;
            }

            MissionConfiguration missionConfig =
                sourceEntry.Challenge?.MissionConfig;

            if (
                missionConfig != null &&
                missionConfig.EffectiveProvider ==
                    UnlockProviderKind.Original
            )
            {
                return false;
            }

            SurvivorChallengeJson challenge =
                CloneChallenge(
                    sourceEntry.Challenge
                )
                ?? new SurvivorChallengeJson();

            challenge.Enabled = true;
            challenge.Name = BuildChallengeName(candidate);
            challenge.Description = BuildChallengeDescription(candidate);
            challenge.LocalizationKey = "";
            challenge.LunarCoinReward = 0;

            // Evita que un tracker Schema v1 interprete el fallback histórico
            // (por ejemplo KillEnemies 100) como la misión activa.
            challenge.Type = "GeneratedMission";

            challenge.Parameters =
                BuildGeneratedMetadata(candidate);

            challenge.Mission =
                CloneMission(
                    materialization.Mission
                );

            /*
             * MissionConfig se conserva EXACTAMENTE como autoridad/provider
             * persistidos. Source=Generated sigue administrado por G8 y no se
             * convierte en Custom sin una edición explícita.
             */

            entryJson["challenge"] =
                JObject.FromObject(challenge);


            UsuLog.Verbose(
                logger,
                "[GENERATED RUNTIME ACTIVE] " +
                $"{candidate.DisplayName} | " +
                $"Id: {candidate.CandidateId} | " +
                $"Family: {candidate.Family} | " +
                $"Amount: {candidate.Parameters?.Amount} | " +
                $"Reset: {candidate.Parameters?.ResetScope} | " +
                $"Performers: {materialization.PerformerCount} | " +
                $"Performer skills: {materialization.PerformerSkillCount} | " +
                "TargetUsage: no | Shared: yes | " +
                "Source: Generated | Persistencia: JSON+snapshot"
            );

            return true;
        }


        public static bool IsMaterializedForBody(
            string bodyName
        )
        {
            return
                !string.IsNullOrWhiteSpace(bodyName) &&
                MaterializationsByBody.ContainsKey(bodyName);
        }


        private static bool IsSupportedFamily(
            MissionTemplateFamily family
        )
        {
            switch (family)
            {
                case MissionTemplateFamily.CombatKills:
                case MissionTemplateFamily.Execution:
                case MissionTemplateFamily.MeleeCombat:
                case MissionTemplateFamily.ProjectileCombat:
                case MissionTemplateFamily.ResourceMastery:
                case MissionTemplateFamily.HealingSupport:
                    return true;

                default:
                    return false;
            }
        }


        private static RuntimeMaterialization BuildMaterialization(
            GeneratedMissionCandidate candidate,
            IReadOnlyList<MissionProfile> profiles
        )
        {
            if (
                candidate == null ||
                string.IsNullOrWhiteSpace(candidate.BodyName) ||
                candidate.RequiresTargetUsage ||
                candidate.Parameters == null
            )
            {
                return null;
            }

            switch (candidate.Family)
            {
                case MissionTemplateFamily.CombatKills:
                    return BuildCombatKillsMaterialization(candidate);

                case MissionTemplateFamily.Execution:
                    return BuildSkillDrivenMaterialization(
                        candidate,
                        profiles,
                        objectiveType: "Kill",
                        objectiveSuffix: "executions",
                        targetCategory: "AnyEnemy"
                    );

                case MissionTemplateFamily.MeleeCombat:
                    return BuildSkillDrivenMaterialization(
                        candidate,
                        profiles,
                        objectiveType: "Kill",
                        objectiveSuffix: "melee_kills",
                        targetCategory: "AnyEnemy"
                    );

                case MissionTemplateFamily.ProjectileCombat:
                    return BuildSkillDrivenMaterialization(
                        candidate,
                        profiles,
                        objectiveType: "Kill",
                        objectiveSuffix: "projectile_kills",
                        targetCategory: "AnyEnemy"
                    );

                case MissionTemplateFamily.ResourceMastery:
                    return BuildSkillDrivenMaterialization(
                        candidate,
                        profiles,
                        objectiveType: "UseSkill",
                        objectiveSuffix: "resource_actions",
                        targetCategory: "Player"
                    );

                case MissionTemplateFamily.HealingSupport:
                    return BuildSkillDrivenMaterialization(
                        candidate,
                        profiles,
                        objectiveType: "Heal",
                        objectiveSuffix: "healing_total",
                        targetCategory: "Ally"
                    );

                default:
                    return null;
            }
        }


        private static RuntimeMaterialization BuildCombatKillsMaterialization(
            GeneratedMissionCandidate candidate
        )
        {
            int amount =
                Math.Max(
                    1,
                    candidate?.Parameters?.Amount ?? 1
                );

            string resetScope =
                NormalizeResetScope(
                    candidate?.Parameters?.ResetScope
                );

            MissionObjective objective =
                new MissionObjective
                {
                    Id =
                        candidate.CandidateId +
                        ".objective.combat_kills",

                    Type = "Kill",
                    Amount = amount,
                    ResetScope = resetScope,

                    Target =
                        new MissionTarget
                        {
                            Category = "AnyEnemy",
                            Id = ""
                        },

                    Conditions =
                        new List<MissionCondition>
                        {
                            new MissionCondition
                            {
                                Type = "ExcludedSurvivor",
                                Parameters =
                                    new JObject
                                    {
                                        ["bodies"] =
                                            new JArray(
                                                candidate?.BodyName ?? ""
                                            )
                                    }
                            }
                        },

                    Parameters =
                        BuildGeneratedMetadata(candidate)
                };

            MissionRoute route =
                new MissionRoute
                {
                    Id =
                        candidate.CandidateId +
                        ".route.combatkills",

                    Objectives =
                        new List<MissionObjective>
                        {
                            objective
                        },

                    Conditions =
                        new List<MissionCondition>(),

                    Rules =
                        new MissionRules
                        {
                            SingleRun = true,
                            ResetOnDeath = false,
                            IsStreak = false
                        }
                };

            return new RuntimeMaterialization
            {
                Candidate = candidate,
                Mission =
                    new MissionDefinition
                    {
                        SchemaVersion = 2,
                        ProgressScope = "Shared",
                        RewardScope = "Session",
                        Routes =
                            new List<MissionRoute>
                            {
                                route
                            }
                    },
                PerformerCount = 0,
                PerformerSkillCount = 0
            };
        }


        private static RuntimeMaterialization BuildSkillDrivenMaterialization(
            GeneratedMissionCandidate candidate,
            IReadOnlyList<MissionProfile> profiles,
            string objectiveType,
            string objectiveSuffix,
            string targetCategory
        )
        {
            List<JObject> performerOptions =
                BuildPerformerOptions(
                    candidate,
                    profiles,
                    out int performerCount
                );

            if (performerOptions.Count == 0)
            {
                logger?.LogWarning(
                    "[GENERATED RUNTIME] No existen performers/skills " +
                    $"{candidate.Family} utilizables para {candidate.DisplayName}. " +
                    "Se conserva el fallback anterior."
                );

                return null;
            }

            JArray options =
                new JArray();

            for (int i = 0; i < performerOptions.Count; i++)
            {
                options.Add(
                    performerOptions[i]
                );
            }

            MissionCondition performerSkillCondition =
                new MissionCondition
                {
                    Type = "AllowedPerformerSkill",
                    Parameters =
                        new JObject
                        {
                            ["options"] = options
                        }
                };

            int amount =
                Math.Max(
                    1,
                    candidate.Parameters.Amount
                );

            string resetScope =
                NormalizeResetScope(
                    candidate.Parameters.ResetScope
                );

            MissionObjective objective =
                new MissionObjective
                {
                    Id =
                        candidate.CandidateId +
                        ".objective." +
                        objectiveSuffix,

                    Type = objectiveType,
                    Amount = amount,
                    ResetScope = resetScope,

                    Target =
                        new MissionTarget
                        {
                            Category = targetCategory,
                            Id = ""
                        },

                    Conditions =
                        new List<MissionCondition>
                        {
                            performerSkillCondition
                        },

                    Parameters =
                        BuildGeneratedMetadata(candidate)
                };

            MissionRoute route =
                new MissionRoute
                {
                    Id =
                        candidate.CandidateId +
                        ".route." +
                        candidate.Family.ToString().ToLowerInvariant(),

                    Objectives =
                        new List<MissionObjective>
                        {
                            objective
                        },

                    Conditions =
                        new List<MissionCondition>(),

                    Rules =
                        new MissionRules
                        {
                            SingleRun = true,
                            ResetOnDeath = false,
                            IsStreak = false
                        }
                };

            MissionDefinition mission =
                new MissionDefinition
                {
                    SchemaVersion = 2,

                    // Todos los performers compatibles suman al mismo contador
                    // de la misión que el host congeló para la sesión.
                    ProgressScope = "Shared",

                    RewardScope = "Session",

                    Routes =
                        new List<MissionRoute>
                        {
                            route
                        }
                };

            return new RuntimeMaterialization
            {
                Candidate = candidate,
                Mission = mission,
                PerformerCount = performerCount,
                PerformerSkillCount = performerOptions.Count
            };
        }


        private static List<JObject> BuildPerformerOptions(
            GeneratedMissionCandidate candidate,
            IReadOnlyList<MissionProfile> profiles,
            out int performerCount
        )
        {
            performerCount = 0;

            List<JObject> options =
                new List<JObject>();

            if (profiles == null)
            {
                return options;
            }

            HashSet<string> performerBodies =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            HashSet<string> optionKeys =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            for (int i = 0; i < profiles.Count; i++)
            {
                MissionProfile profile =
                    profiles[i];

                if (
                    profile == null ||
                    !profile.IsUnlocked ||
                    string.IsNullOrWhiteSpace(profile.BodyName) ||
                    IsTarget(candidate, profile) ||
                    profile.Skills == null
                )
                {
                    continue;
                }

                bool bodyAdded = false;

                for (int skillIndex = 0;
                    skillIndex < profile.Skills.Count;
                    skillIndex++)
                {
                    MissionSkillProfile skill =
                        profile.Skills[skillIndex];

                    if (
                        skill == null ||
                        !IsUsableSkill(skill) ||
                        !SkillMatchesFamily(
                            skill,
                            candidate.Family
                        ) ||
                        !TryGetRuntimeSlot(
                            skill.Slot,
                            out string slot
                        )
                    )
                    {
                        continue;
                    }

                    string skillToken =
                        skill.NameToken?.Trim() ?? "";

                    string skillName =
                        skill.InternalName?.Trim() ?? "";

                    if (
                        string.IsNullOrWhiteSpace(skillToken) &&
                        string.IsNullOrWhiteSpace(skillName)
                    )
                    {
                        continue;
                    }

                    string key =
                        profile.BodyName.Trim() + "|" +
                        slot + "|" +
                        skillToken + "|" +
                        skillName;

                    if (!optionKeys.Add(key))
                    {
                        continue;
                    }

                    JObject option =
                        new JObject
                        {
                            ["body"] =
                                profile.BodyName.Trim(),

                            ["slot"] =
                                slot
                        };

                    if (!string.IsNullOrWhiteSpace(skillToken))
                    {
                        option["skillToken"] =
                            skillToken;
                    }
                    else if (!string.IsNullOrWhiteSpace(skillName))
                    {
                        option["skillName"] =
                            skillName;
                    }

                    options.Add(option);
                    bodyAdded = true;
                }

                if (bodyAdded)
                {
                    performerBodies.Add(
                        profile.BodyName.Trim()
                    );
                }
            }

            options.Sort(
                delegate(JObject a, JObject b)
                {
                    string left =
                        BuildOptionSortKey(a);

                    string right =
                        BuildOptionSortKey(b);

                    return string.Compare(
                        left,
                        right,
                        StringComparison.OrdinalIgnoreCase
                    );
                }
            );

            performerCount =
                performerBodies.Count;

            return options;
        }


        private static bool SkillMatchesFamily(
            MissionSkillProfile skill,
            MissionTemplateFamily family
        )
        {
            if (skill == null)
            {
                return false;
            }

            switch (family)
            {
                case MissionTemplateFamily.Execution:
                    return
                        HasSkillCapability(
                            skill,
                            MissionProfileCapability.Execution
                        ) ||
                        HasSkillEffect(
                            skill,
                            MissionSkillEffectKind.Execution
                        );

                case MissionTemplateFamily.HealingSupport:
                    return
                        HasSkillCapability(
                            skill,
                            MissionProfileCapability.Healing
                        ) ||
                        HasSkillEffect(
                            skill,
                            MissionSkillEffectKind.Healing
                        );

                case MissionTemplateFamily.ResourceMastery:
                    return
                        HasSkillCapability(
                            skill,
                            MissionProfileCapability.ResourceMechanic
                        ) ||
                        HasSkillEffect(
                            skill,
                            MissionSkillEffectKind.Resource
                        );

                case MissionTemplateFamily.MeleeCombat:
                    return
                        HasSkillEffect(
                            skill,
                            MissionSkillEffectKind.Melee
                        );

                case MissionTemplateFamily.ProjectileCombat:
                    return
                        HasSkillEffect(
                            skill,
                            MissionSkillEffectKind.Projectile
                        );

                default:
                    return false;
            }
        }


        private static bool HasSkillCapability(
            MissionSkillProfile skill,
            MissionProfileCapability capability
        )
        {
            if (skill?.Capabilities == null)
            {
                return false;
            }

            for (int i = 0; i < skill.Capabilities.Count; i++)
            {
                if (skill.Capabilities[i] == capability)
                {
                    return true;
                }
            }

            return false;
        }


        private static bool HasSkillEffect(
            MissionSkillProfile skill,
            MissionSkillEffectKind effectKind
        )
        {
            if (skill?.Effects == null)
            {
                return false;
            }

            for (int i = 0; i < skill.Effects.Count; i++)
            {
                MissionSkillEffectProfile effect =
                    skill.Effects[i];

                if (
                    effect != null &&
                    effect.Kind == effectKind
                )
                {
                    return true;
                }
            }

            return false;
        }


        private static bool IsTarget(
            GeneratedMissionCandidate candidate,
            MissionProfile profile
        )
        {
            if (
                candidate == null ||
                profile == null
            )
            {
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(candidate.SurvivorId) &&
                !string.IsNullOrWhiteSpace(profile.SurvivorId) &&
                string.Equals(
                    candidate.SurvivorId,
                    profile.SurvivorId,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return true;
            }

            return
                !string.IsNullOrWhiteSpace(candidate.BodyName) &&
                !string.IsNullOrWhiteSpace(profile.BodyName) &&
                string.Equals(
                    candidate.BodyName,
                    profile.BodyName,
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static bool IsUsableSkill(
            MissionSkillProfile skill
        )
        {
            if (skill == null)
            {
                return false;
            }

            return
                !skill.HasUnlockRequirement ||
                skill.IsUnlocked;
        }


        private static bool TryGetRuntimeSlot(
            SurvivorContentSkillSlot slot,
            out string value
        )
        {
            value = "";

            switch (slot)
            {
                case SurvivorContentSkillSlot.Primary:
                    value = "Primary";
                    return true;

                case SurvivorContentSkillSlot.Secondary:
                    value = "Secondary";
                    return true;

                case SurvivorContentSkillSlot.Utility:
                    value = "Utility";
                    return true;

                case SurvivorContentSkillSlot.Special:
                    value = "Special";
                    return true;

                default:
                    return false;
            }
        }


        private static string BuildOptionSortKey(
            JObject option
        )
        {
            if (option == null)
            {
                return "";
            }

            return
                (option.Value<string>("body") ?? "") + "|" +
                (option.Value<string>("slot") ?? "") + "|" +
                (option.Value<string>("skillToken") ?? "") + "|" +
                (option.Value<string>("skillName") ?? "");
        }


        private static string NormalizeResetScope(
            string value
        )
        {
            return string.Equals(
                value,
                "Stage",
                StringComparison.OrdinalIgnoreCase
            )
                ? "Stage"
                : "Run";
        }


        private static JObject BuildGeneratedMetadata(
            GeneratedMissionCandidate candidate
        )
        {
            return new JObject
            {
                ["generatedMissionId"] =
                    candidate?.CandidateId ?? "",

                ["generatorVersion"] =
                    candidate?.GeneratorVersion ?? 0,

                ["generationNonce"] =
                    candidate?.GenerationNonce ?? "",

                ["generatedTemplateId"] =
                    candidate?.TemplateId ?? "",

                ["generatedFingerprint"] =
                    candidate?.ProfileFingerprint ?? "",

                ["generatedFamily"] =
                    candidate?.Family.ToString() ?? "Unknown",

                ["amount"] =
                    candidate?.Parameters?.Amount ?? 0,

                ["resetScope"] =
                    NormalizeResetScope(
                        candidate?.Parameters?.ResetScope
                    ),

                ["temporary"] = false,
                ["managed"] = true,
                ["fallbackOnly"] = true,
                ["targetUsage"] = false
            };
        }


        private static string BuildChallengeName(
            GeneratedMissionCandidate candidate
        )
        {
            switch (candidate?.Family)
            {
                case MissionTemplateFamily.CombatKills:
                    return "Cacería calculada";

                case MissionTemplateFamily.Execution:
                    return "Ejecución calculada";

                case MissionTemplateFamily.HealingSupport:
                    return "Apoyo calculado";

                case MissionTemplateFamily.ResourceMastery:
                    return "Dominio calculado";

                case MissionTemplateFamily.MeleeCombat:
                    return "Combate cercano calculado";

                case MissionTemplateFamily.ProjectileCombat:
                    return "Tiro calculado";

                default:
                    return "Misión generada";
            }
        }


        private static string BuildChallengeDescription(
            GeneratedMissionCandidate candidate
        )
        {
            int amount =
                Math.Max(
                    1,
                    candidate?.Parameters?.Amount ?? 1
                );

            bool stageReset =
                string.Equals(
                    candidate?.Parameters?.ResetScope,
                    "Stage",
                    StringComparison.OrdinalIgnoreCase
                );

            switch (candidate?.Family)
            {
                case MissionTemplateFamily.CombatKills:
                    return
                        $"Derrota {amount} enemigos;\n" +
                        (stageReset
                            ? "hazlo en un sector."
                            : "hazlo en una partida.");

                case MissionTemplateFamily.Execution:
                    return
                        $"Consigue {amount} bajas con habilidades de ejecución;\n" +
                        (
                            stageReset
                                ? "hazlas en un sector."
                                : "hazlas en una partida."
                        );

                case MissionTemplateFamily.HealingSupport:
                    return
                        $"Restaura {amount} de salud con habilidades de apoyo;\n" +
                        (
                            stageReset
                                ? "hazlo en un sector."
                                : "hazlo en una partida."
                        );

                case MissionTemplateFamily.ResourceMastery:
                    return
                        $"Activa {amount} veces habilidades de recurso;\n" +
                        (
                            stageReset
                                ? "hazlo en un sector."
                                : "hazlo en una partida."
                        );

                case MissionTemplateFamily.MeleeCombat:
                    return
                        $"Consigue {amount} bajas con ataques cuerpo a cuerpo;\n" +
                        (
                            stageReset
                                ? "hazlas en un sector."
                                : "hazlas en una partida."
                        );

                case MissionTemplateFamily.ProjectileCombat:
                    return
                        $"Consigue {amount} bajas con habilidades de proyectil;\n" +
                        (
                            stageReset
                                ? "hazlas en un sector."
                                : "hazlas en una partida."
                        );

                default:
                    return
                        "Completa la misión temporal generada por USU.";
            }
        }


        private static SurvivorChallengeJson CloneChallenge(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return null;
            }

            return
                JObject
                    .FromObject(challenge)
                    .ToObject<SurvivorChallengeJson>();
        }


        private static MissionDefinition CloneMission(
            MissionDefinition mission
        )
        {
            if (mission == null)
            {
                return null;
            }

            return
                JObject
                    .FromObject(mission)
                    .ToObject<MissionDefinition>();
        }


        private static void LogVerboseMaterializations()
        {
            if (!UsuLog.VerboseEnabled)
            {
                return;
            }

            List<RuntimeMaterialization> ordered =
                new List<RuntimeMaterialization>(
                    MaterializationsByBody.Values
                );

            ordered.Sort(
                delegate(
                    RuntimeMaterialization a,
                    RuntimeMaterialization b
                )
                {
                    return string.Compare(
                        a?.Candidate?.DisplayName,
                        b?.Candidate?.DisplayName,
                        StringComparison.CurrentCultureIgnoreCase
                    );
                }
            );

            for (int i = 0; i < ordered.Count; i++)
            {
                RuntimeMaterialization item =
                    ordered[i];

                GeneratedMissionCandidate candidate =
                    item?.Candidate;

                if (candidate == null)
                {
                    continue;
                }

                UsuLog.Diagnostic(
                logger,
                    "[GENERATED RUNTIME MATERIALIZED] " +
                    $"{candidate.DisplayName} | " +
                    $"Id: {candidate.CandidateId} | " +
                    $"Family: {candidate.Family} | " +
                    $"Objective: {candidate.Parameters?.ObjectiveKind} | " +
                    $"Amount: {candidate.Parameters?.Amount} | " +
                    $"Reset: {candidate.Parameters?.ResetScope} | " +
                    $"Performers: {item.PerformerCount} | " +
                    $"Performer skills: {item.PerformerSkillCount} | " +
                    "Progress: Shared | TargetUsage: no | " +
                    "Managed Generated: yes | Persistencia: JSON"
                );
            }
        }
    }
}
