using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    public static partial class SurvivorUnlockManager
    {
        // =========================================================
        // JSON ENTRY
        // =========================================================

        private static SurvivorJsonEntry GetEntry(
            string bodyName
        )
        {
            if (
                SessionMissionRegistry
                    .TryGetEffectiveEntry(
                        bodyName,
                        out SurvivorJsonEntry effectiveEntry
                    ) &&
                effectiveEntry != null
            )
            {
                return effectiveEntry;
            }


            return SurvivorJsonManager
                .GetEntryAnywhere(
                    bodyName
                );
        }


        // =========================================================
        // RECOMPENSA DE MONEDAS LUNARES
        // =========================================================
        //
        // El valor se almacena en Survivors.json para que la recompensa
        // forme parte de la definición del challenge. Se limita a 0..10
        // para mantener una escala similar a la utilizada por RoR2.
        //
        // IMPORTANTE: USU NO suma monedas manualmente.
        // AchievementDef.lunarCoinReward + UserProfile.AddAchievement()
        // dejan la entrega en manos del flujo vanilla y evitan duplicados.
        // =========================================================

        private static uint GetLunarCoinReward(
            SurvivorJsonEntry entry
        )
        {
            SurvivorChallengeJson challenge = entry?.Challenge;
            if (challenge == null)
            {
                return 0;
            }

            MissionConfiguration config = challenge.MissionConfig;
            UnlockProviderKind provider =
                UnlockProviderPolicy.ResolveEffectiveProvider(
                    entry,
                    UnlockProviderPolicy.HasStoredOriginalUnlock(entry)
                );

            // Original pertenece al mod autor: su propio UnlockableDef /
            // AchievementDef y sus monedas son ajenos a USU. Este valor 0
            // es sólo para el AchievementDef USU que queda DORMIDO.
            if (provider == UnlockProviderKind.Original)
            {
                return 0;
            }

            // Regenerar o reactivar Generated nunca hereda monedas de
            // un preset oficial seleccionado anteriormente.
            if (config != null && config.UsesGeneratedMission)
            {
                return 0;
            }

            // Cada vez que volvemos a USU, el catálogo oficial es la
            // fuente de verdad. No leemos 0 del snapshot Generated anterior.
            if (
                provider == UnlockProviderKind.USU &&
                config != null &&
                string.Equals(config.Source, "Preset", StringComparison.OrdinalIgnoreCase) &&
                OfficialMissionPresetRegistry.TryGetByPresetId(
                    config.BasePresetId,
                    out OfficialMissionPresetDescriptor official
                ) &&
                official != null &&
                string.Equals(
                    official.BodyName,
                    entry.BodyName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return (uint)Math.Max(0, Math.Min(10, official.LunarCoinReward));
            }

            // Custom conserva su recompensa configurada. Un preset
            // oficial de otro survivor NO debe dar sus monedas.
            if (
                config != null &&
                string.Equals(config.Source, "Preset", StringComparison.OrdinalIgnoreCase) &&
                OfficialMissionPresetRegistry.IsOfficialPresetId(config.BasePresetId)
            )
            {
                return 0;
            }

            return (uint)Math.Max(0, Math.Min(10, challenge.LunarCoinReward));
        }

        // El popup previo y la entrega al finalizar deben resolver la MISMA
        // recompensa efectiva, incluyendo el snapshot del host en multijugador.
        internal static uint ResolveEffectiveLunarCoinReward(string bodyName)
        {
            return GetLunarCoinReward(GetEntry(bodyName));
        }


        // =========================================================
        // NOMBRE MISIÓN
        // =========================================================

        private static string GetChallengeName(
            SurvivorJsonEntry entry
        )
        {
            if (
                entry?.Challenge != null &&
                !string.IsNullOrWhiteSpace(
                    entry.Challenge.Name
                )
            )
            {
                return entry.Challenge.Name;
            }


            return "Desafío de desbloqueo";
        }


        // =========================================================
        // DESCRIPCIÓN
        // =========================================================

        private static string BuildChallengeDescription(
            SurvivorJsonEntry entry
        )
        {
            if (
                entry == null ||
                entry.Challenge == null
            )
            {
                return
                    "Completa el desafío para desbloquear este personaje.";
            }


            /*
             * Si el creador o usuario escribió una descripción,
             * esa descripción es autoritativa.
             */
            if (
                !string.IsNullOrWhiteSpace(
                    entry.Challenge.Description
                )
            )
            {
                return
                    entry.Challenge.Description;
            }


            JObject parameters =
                entry.Challenge.Parameters;


            // =========================================================
            // DESCRIPCIONES AUTOMÁTICAS
            // =========================================================

            switch (entry.Challenge.Type)
            {
                case "KillEnemies":
                    {
                        int amount =
                            GetInt(
                                parameters,
                                "amount",
                                1
                            );


                        return
                            $"Derrota {amount} enemigos.";
                    }


                case "KillBoss":
                    {
                        int amount =
                            GetInt(
                                parameters,
                                "amount",
                                1
                            );


                        return
                            $"Derrota {amount} jefes.";
                    }


                case "ReachLevel":
                    {
                        int level =
                            GetInt(
                                parameters,
                                "level",
                                1
                            );


                        return
                            $"Alcanza el nivel {level}.";
                    }


                case "ReachStage":
                    {
                        int stage =
                            GetInt(
                                parameters,
                                "stage",
                                1
                            );


                        return
                            $"Alcanza la fase {stage}.";
                    }


                case "ApplyStatusEffects":
                    {
                        int amount =
                            GetInt(
                                parameters,
                                "amount",
                                100
                            );


                        return
                            $"Mantén {amount} efectos de estado válidos activos " +
                            $"simultáneamente en una partida.";
                    }


                case "HealHealth":
                    {
                        int amount =
                            GetInt(
                                parameters,
                                "amount",
                                5000
                            );


                        return
                            $"Restaura un total de {amount} de salud a tu equipo\n" +
                            $"durante una sola partida.";
                    }


                default:
                    {
                        return
                            $"Completa la misión " +
                            $"\"{entry.Challenge.Type}\".";
                    }
            }
        }


        // =========================================================
        // PRESENTACIÓN EFECTIVA DE MISIÓN
        // =========================================================
        //
        // Los AchievementDef de USU se registran antes de G7/G8. Estos
        // helpers permiten que Character Select consulte la misión efectiva
        // actual sin perder la localización integrada de presets oficiales.
        // Para Generated/Custom usamos el texto de la entrada efectiva.
        // =========================================================

        private static string ResolveChallengeDisplayName(
            string bodyName,
            SurvivorJsonEntry entry,
            string fallbackToken
        )
        {
            SurvivorChallengeJson challenge =
                entry?.Challenge;


            if (
                GeneratedMissionPresentationFormatter.TryResolve(
                    entry,
                    out string generatedName,
                    out string generatedDescription
                )
            )
            {
                return generatedName;
            }


            if (
                challenge != null &&
                SurvivorLocalization.UsesBuiltInLocalization(
                    bodyName,
                    challenge
                ) &&
                SurvivorLocalization.TryGetOfficialMissionTokens(
                    bodyName,
                    out string officialNameToken,
                    out string officialDescriptionToken
                )
            )
            {
                return Language.GetString(
                    officialNameToken
                );
            }


            string value =
                GetChallengeName(
                    entry
                );


            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }


            return
                !string.IsNullOrWhiteSpace(fallbackToken)
                    ? Language.GetString(fallbackToken)
                    : "Desafío de desbloqueo";
        }


        private static string ResolveChallengeDisplayDescription(
            string bodyName,
            SurvivorJsonEntry entry,
            string fallbackToken
        )
        {
            SurvivorChallengeJson challenge =
                entry?.Challenge;


            if (
                GeneratedMissionPresentationFormatter.TryResolve(
                    entry,
                    out string generatedName,
                    out string generatedDescription
                )
            )
            {
                return generatedDescription;
            }


            if (
                challenge != null &&
                SurvivorLocalization.UsesBuiltInLocalization(
                    bodyName,
                    challenge
                ) &&
                SurvivorLocalization.TryGetOfficialMissionTokens(
                    bodyName,
                    out string officialNameToken,
                    out string officialDescriptionToken
                )
            )
            {
                return Language.GetString(
                    officialDescriptionToken
                );
            }


            string value =
                BuildChallengeDescription(
                    entry
                );


            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }


            return
                !string.IsNullOrWhiteSpace(fallbackToken)
                    ? Language.GetString(fallbackToken)
                    : "Completa el desafío para desbloquear este personaje.";
        }


        // =========================================================
        // REQUISITOS INFORMATIVOS DEL TOOLTIP (Character Select)
        // =========================================================
        // No son requisitos adicionales: sólo expresan las condiciones
        // realmente definidas en la misión EFECTIVA de USU.
        //
        // * ExcludedSurvivor  -> mantener el aviso vanilla de Jhin.
        // * RequiredSurvivor  -> el jugador que ejecuta el objetivo
        //                        debe usar ese survivor.
        // * PartyHasSurvivor  -> basta UNO en todo el equipo (no cada uno).
        // * Sin restricción   -> Cualquiera.
        //
        // Se recalcula desde la entrada activa, incluyendo rutas OR,
        // personalizaciones y configuración del host. No cambia el unlock.
        // =========================================================

        private static string BuildRestrictionNotice(SurvivorJsonEntry entry)
        {
            List<string> excludedBodies = GetMissionWideExcludedBodies(entry);
            string except = FormatSurvivorNames(excludedBodies);

            // Conservar exactamente el aviso especial aprobado de Jhin.
            if (string.Equals(entry?.BodyName, "JhinBody", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(except))
            {
                return Language.GetStringFormatted(
                    SurvivorLocalization.ExcludedSurvivorsNoticeToken, except);
            }

            MissionDefinition mission = entry?.Challenge?.Mission;
            if (mission?.Routes == null || mission.Routes.Count == 0)
            {
                // Sin una misión V2 no debemos inventar condiciones.
                return string.IsNullOrWhiteSpace(except)
                    ? ""
                    : Language.GetStringFormatted(
                        SurvivorLocalization.ExcludedSurvivorsNoticeToken, except);
            }

            HashSet<string> allowedBodies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> teamBodies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool anyAllowedRoute = false;
            bool anyTeamFreeRoute = false;
            bool foundRoute = false;

            foreach (MissionRoute route in mission.Routes)
            {
                if (route == null) continue;
                foundRoute = true;

                HashSet<string> routeAllowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                HashSet<string> routeTeam = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CollectSurvivorConditionBodies(route.Conditions, "RequiredSurvivor", routeAllowed);
                CollectSurvivorConditionBodies(route.Conditions, "PartyHasSurvivor", routeTeam);

                IReadOnlyList<MissionObjective> objectives = route.GetEffectiveObjectives();
                if (objectives != null)
                {
                    foreach (MissionObjective objective in objectives)
                    {
                        CollectSurvivorConditionBodies(objective?.Conditions, "RequiredSurvivor", routeAllowed);
                        CollectSurvivorConditionBodies(objective?.Conditions, "PartyHasSurvivor", routeTeam);
                    }
                }

                // Rutas alternativas: una ruta sin la condición no la exige a todas.
                if (routeAllowed.Count == 0) anyAllowedRoute = true;
                else allowedBodies.UnionWith(routeAllowed);
                if (routeTeam.Count == 0) anyTeamFreeRoute = true;
                else teamBodies.UnionWith(routeTeam);
            }
            if (!foundRoute) return "";

            bool requiresPerformer = !anyAllowedRoute && allowedBodies.Count > 0;
            bool requiresTeamMember = !anyTeamFreeRoute && teamBodies.Count > 0;
            List<string> lines = new List<string>();

            // Misma etiqueta azul para TODAS las misiones: individuales,
            // mixtas y cooperativas. Solo cambia el valor semántico.
            if (requiresPerformer)
            {
                lines.Add(Language.GetStringFormatted(
                    SurvivorLocalization.CharacterSelectRequiredSurvivorToken,
                    FormatSurvivorChoices(allowedBodies)));
            }
            else if (requiresTeamMember)
            {
                string teamValue = teamBodies.Count == 1
                    ? FormatSurvivorNames(teamBodies) + Language.GetString(
                        SurvivorLocalization.CharacterSelectTeamOneSuffixToken)
                    : Language.GetString(SurvivorLocalization.CharacterSelectTeamChoicesToken) +
                        "\n" + FormatSurvivorChoices(teamBodies);
                lines.Add(Language.GetStringFormatted(
                    SurvivorLocalization.CharacterSelectRequiredSurvivorToken,
                    teamValue));
            }
            else
            {
                lines.Add(Language.GetStringFormatted(
                    SurvivorLocalization.CharacterSelectRequiredSurvivorToken,
                    Language.GetString(SurvivorLocalization.MissionLibraryAnySurvivorToken)));
            }

            // Una misión puede exigir un ejecutor específico Y otro integrante.
            // No se oculta ninguna de las dos restricciones si coexisten.
            if (requiresPerformer && requiresTeamMember)
            {
                lines.Add(Language.GetStringFormatted(
                    SurvivorLocalization.CharacterSelectOneTeamMemberToken,
                    FormatSurvivorChoices(teamBodies)));
            }

            string requiredStages = GetRequiredStageDisplayNames(mission);
            if (!string.IsNullOrWhiteSpace(requiredStages))
            {
                lines.Add(Language.GetStringFormatted(
                    SurvivorLocalization.CharacterSelectRequiredStageToken,
                    requiredStages));
            }

            if (!string.IsNullOrWhiteSpace(except))
            {
                lines.Add(Language.GetStringFormatted(
                    SurvivorLocalization.ExcludedSurvivorsNoticeToken, except));
            }

            if (MissionModeConfiguration.GetMode(mission) == MissionUnlockMode.Mixed)
            {
                if (string.Equals(entry?.BodyName, "WooperBody", StringComparison.OrdinalIgnoreCase) &&
                    HasWooperBiteObjective(mission))
                {
                    lines.Add(Language.GetString(
                        SurvivorLocalization.CharacterSelectWooperContributionToken));
                }
                // El host acredita solo aportes verificados. No prometer la
                // recompensa a todos por estar presentes en la partida.
                lines.Add(Language.GetString(
                    SurvivorLocalization.CharacterSelectMixedModeToken));
            }

            return string.Join("\n", lines.ToArray());
        }

        // Cuando hay varias opciones, dar a cada personaje su propia línea.
        // El renderizado del tooltip de RoR2 acepta saltos de línea y TMP.
        private static string FormatSurvivorChoices(IEnumerable<string> bodyNames)
        {
            List<string> names = new List<string>();
            if (bodyNames == null) return "";
            foreach (string body in bodyNames)
            {
                string name = ResolveSurvivorDisplayName(body);
                if (!string.IsNullOrWhiteSpace(name) && !names.Contains(name))
                    names.Add(name);
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            if (names.Count <= 1) return names.Count == 1 ? names[0] : "";
            return "\n  • " + string.Join("\n  • ", names.ToArray());
        }

        private static bool HasWooperBiteObjective(MissionDefinition mission)
        {
            if (mission?.Routes == null) return false;
            foreach (MissionRoute route in mission.Routes)
            {
                IReadOnlyList<MissionObjective> objectives = route?.GetEffectiveObjectives();
                if (objectives == null) continue;
                foreach (MissionObjective objective in objectives)
                {
                    if (string.Equals(objective?.Id, "poisoned_bite_20",
                        StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
            return false;
        }

        // Escenarios de rutas V2. Cada ruta es alternativa (OR); dentro
        // de una ruta, las condiciones RequiredStage son restricciones AND.
        // Si existe una ruta libre, NO indicamos un escenario obligatorio.
        // El resultado enumera las posibilidades reales sin alterar el unlock.
        private static string GetRequiredStageDisplayNames(MissionDefinition mission)
        {
            if (mission?.Routes == null || mission.Routes.Count == 0)
                return "";

            HashSet<string> possibleStages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool sawRoute = false;

            foreach (MissionRoute route in mission.Routes)
            {
                if (route == null) return "";
                sawRoute = true;
                HashSet<string> routeStages = null;

                // La mayoría de los presets oficiales (Wooper/Miner)
                // especifican RequiredStage en condiciones de ruta.
                if (!IntersectStageConditions(route.Conditions, ref routeStages))
                    return "";

                // Una condición RequiredStage también puede estar asociada
                // a un objetivo de esa ruta en misiones CUSTOM.
                IReadOnlyList<MissionObjective> objectives = route.GetEffectiveObjectives();
                if (objectives != null)
                {
                    foreach (MissionObjective objective in objectives)
                    {
                        if (!IntersectStageConditions(objective?.Conditions, ref routeStages))
                            return "";
                    }
                }

                if (routeStages == null || routeStages.Count == 0)
                    return ""; // Una alternativa sin escenario fijo: no inventarlo.

                possibleStages.UnionWith(routeStages);
            }

            if (!sawRoute || possibleStages.Count == 0) return "";

            // Las cuatro variantes comparten nombre base. El listado largo
            // de SceneCatalog desbordaría Character Select sin aportar nada.
            // Sólo agrupamos si la misión EFECTIVA admite las cuatro;
            // un CUSTOM que exija una sola variante mantiene su precisión.
            if (possibleStages.Count == 4 &&
                possibleStages.Contains("artifactworld") &&
                possibleStages.Contains("artifactworld01") &&
                possibleStages.Contains("artifactworld02") &&
                possibleStages.Contains("artifactworld03"))
            {
                return Language.GetString(
                    SurvivorLocalization.CharacterSelectBulwarksAmbryNameToken);
            }

            List<string> display = new List<string>();
            foreach (string stage in possibleStages)
            {
                string name = ResolveStageDisplayName(stage);
                if (!string.IsNullOrWhiteSpace(name) && !display.Contains(name))
                    display.Add(name);
            }
            display.Sort(StringComparer.OrdinalIgnoreCase);
            return string.Join(" / ", display.ToArray());
        }

        // false = datos inconsistentes o una intersección vacía;
        // jamás anunciar un requisito de escenario inventado.
        private static bool IntersectStageConditions(
            IList<MissionCondition> conditions,
            ref HashSet<string> allowed)
        {
            if (conditions == null) return true;
            foreach (MissionCondition condition in conditions)
            {
                if (condition == null ||
                    !string.Equals(condition.Type, "RequiredStage", StringComparison.OrdinalIgnoreCase))
                    continue;

                JObject parameters = condition.Parameters;
                if (parameters == null) return false;
                HashSet<string> current = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string single = parameters.Value<string>("stage");
                if (!string.IsNullOrWhiteSpace(single)) current.Add(single.Trim());
                if (parameters["stages"] is JArray stages)
                {
                    foreach (JToken token in stages)
                    {
                        string stage = token?.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(stage)) current.Add(stage);
                    }
                }
                if (current.Count == 0) return false;
                if (allowed == null) allowed = current;
                else allowed.IntersectWith(current);
                if (allowed.Count == 0) return false;
            }
            return true;
        }

        private static string ResolveStageDisplayName(string stageName)
        {
            if (string.IsNullOrWhiteSpace(stageName)) return "";

            // Estos dos nombres ya se aprobaron para ES/EN en USU.
            if (string.Equals(stageName, "foggyswamp", StringComparison.OrdinalIgnoreCase))
                return Language.GetString(SurvivorLocalization.CharacterSelectWetlandsNameToken);
            if (string.Equals(stageName, "dampcavesimple", StringComparison.OrdinalIgnoreCase))
                return Language.GetString(SurvivorLocalization.CharacterSelectAbyssalNameToken);

            try
            {
                SceneDef sceneDef = SceneCatalog.GetSceneDefFromSceneName(stageName);
                if (sceneDef != null && !string.IsNullOrWhiteSpace(sceneDef.nameToken))
                {
                    string name = Language.GetString(sceneDef.nameToken);
                    if (!string.IsNullOrWhiteSpace(name) &&
                        !string.Equals(name, sceneDef.nameToken, StringComparison.OrdinalIgnoreCase))
                        return name;
                }
            }
            catch
            {
                // El catálogo puede no estar disponible al abrir Character Select.
            }
            // CUSTOM o escena de mod desconocida: mostrar el identificador
            // real, sin inventar su traducción.
            return stageName;
        }

        private static void CollectSurvivorConditionBodies(
            IList<MissionCondition> conditions,
            string conditionType,
            HashSet<string> destination)
        {
            if (conditions == null || destination == null) return;
            foreach (MissionCondition condition in conditions)
            {
                if (condition == null ||
                    !string.Equals(condition.Type, conditionType, StringComparison.OrdinalIgnoreCase))
                    continue;

                JObject parameters = condition.Parameters;
                if (parameters == null) continue;
                AddExcludedBodiesFromToken(parameters["bodies"], destination);
                AddExcludedBodiesFromToken(parameters["body"], destination);
            }
        }

        private static string FormatSurvivorNames(IEnumerable<string> bodyNames)
        {
            List<string> names = new List<string>();
            if (bodyNames == null) return "";
            foreach (string body in bodyNames)
            {
                string name = ResolveSurvivorDisplayName(body);
                if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return string.Join(" / ", names.ToArray());
        }


        private static List<string> GetMissionWideExcludedBodies(
            SurvivorJsonEntry entry
        )
        {
            MissionDefinition mission =
                entry?.Challenge?.Mission;


            if (
                mission != null &&
                mission.Routes != null &&
                mission.Routes.Count > 0
            )
            {
                HashSet<string> missionWide =
                    null;


                for (int i = 0; i < mission.Routes.Count; i++)
                {
                    MissionRoute route =
                        mission.Routes[i];


                    HashSet<string> routeExcluded =
                        new HashSet<string>(
                            StringComparer.OrdinalIgnoreCase
                        );


                    if (route != null)
                    {
                        AddExcludedBodiesFromConditions(
                            route.Conditions,
                            routeExcluded
                        );


                        IReadOnlyList<MissionObjective> objectives =
                            route.GetEffectiveObjectives();


                        if (objectives != null)
                        {
                            for (
                                int objectiveIndex = 0;
                                objectiveIndex < objectives.Count;
                                objectiveIndex++
                            )
                            {
                                MissionObjective objective =
                                    objectives[objectiveIndex];


                                AddExcludedBodiesFromConditions(
                                    objective?.Conditions,
                                    routeExcluded
                                );
                            }
                        }
                    }


                    if (missionWide == null)
                    {
                        missionWide =
                            new HashSet<string>(
                                routeExcluded,
                                StringComparer.OrdinalIgnoreCase
                            );
                    }
                    else
                    {
                        missionWide.IntersectWith(
                            routeExcluded
                        );
                    }
                }


                if (
                    missionWide != null &&
                    missionWide.Count > 0
                )
                {
                    List<string> result =
                        new List<string>(
                            missionWide
                        );

                    result.Sort(
                        StringComparer.OrdinalIgnoreCase
                    );

                    return result;
                }


                // Hay una misión V2 válida y ninguna exclusión global.
                // No mezclamos aquí el fallback legacy porque podría
                // contradecir rutas OR más nuevas.
                return new List<string>();
            }


            // Compatibilidad con presets legacy todavía sin Mission V2.
            HashSet<string> legacyExcluded =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


            JToken legacyToken =
                entry?.Challenge?.Parameters?[
                    "excludedBodies"
                ];


            AddExcludedBodiesFromToken(
                legacyToken,
                legacyExcluded
            );


            List<string> legacyResult =
                new List<string>(
                    legacyExcluded
                );

            legacyResult.Sort(
                StringComparer.OrdinalIgnoreCase
            );

            return legacyResult;
        }


        private static void AddExcludedBodiesFromConditions(
            IList<MissionCondition> conditions,
            HashSet<string> destination
        )
        {
            if (
                conditions == null ||
                destination == null
            )
            {
                return;
            }


            for (int i = 0; i < conditions.Count; i++)
            {
                MissionCondition condition =
                    conditions[i];


                if (
                    condition == null ||
                    !string.Equals(
                        condition.Type,
                        "ExcludedSurvivor",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                JToken bodiesToken =
                    condition.Parameters?[
                        "bodies"
                    ];


                AddExcludedBodiesFromToken(
                    bodiesToken,
                    destination
                );


                // Compatibilidad con una variante singular futura/manual.
                JToken bodyToken =
                    condition.Parameters?[
                        "body"
                    ];


                AddExcludedBodiesFromToken(
                    bodyToken,
                    destination
                );
            }
        }


        private static void AddExcludedBodiesFromToken(
            JToken token,
            HashSet<string> destination
        )
        {
            if (
                token == null ||
                destination == null
            )
            {
                return;
            }


            if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    AddExcludedBodiesFromToken(
                        child,
                        destination
                    );
                }

                return;
            }


            if (token.Type != JTokenType.String)
            {
                return;
            }


            string bodyName =
                token.Value<string>()?.Trim() ?? "";


            if (!string.IsNullOrWhiteSpace(bodyName))
            {
                destination.Add(
                    bodyName
                );
            }
        }


        private static string ResolveSurvivorDisplayName(
            string bodyName
        )
        {
            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return "";
            }


            SurvivorDef[] survivors =
                SurvivorCatalog.survivorDefs;


            if (survivors != null)
            {
                for (int i = 0; i < survivors.Length; i++)
                {
                    SurvivorDef survivor =
                        survivors[i];


                    if (
                        survivor == null ||
                        survivor.bodyPrefab == null ||
                        !string.Equals(
                            survivor.bodyPrefab.name,
                            bodyName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        continue;
                    }


                    if (
                        !string.IsNullOrWhiteSpace(
                            survivor.displayNameToken
                        )
                    )
                    {
                        string localizedName =
                            Language.GetString(
                                survivor.displayNameToken
                            );


                        if (
                            !string.IsNullOrWhiteSpace(
                                localizedName
                            ) &&
                            !string.Equals(
                                localizedName,
                                survivor.displayNameToken,
                                StringComparison.Ordinal
                            )
                        )
                        {
                            return localizedName;
                        }
                    }


                    break;
                }
            }


            const string bodySuffix =
                "Body";


            if (
                bodyName.EndsWith(
                    bodySuffix,
                    StringComparison.OrdinalIgnoreCase
                ) &&
                bodyName.Length > bodySuffix.Length
            )
            {
                return bodyName.Substring(
                    0,
                    bodyName.Length - bodySuffix.Length
                );
            }


            return bodyName;
        }


        private static int GetInt(
            JObject parameters,
            string key,
            int defaultValue
        )
        {
            if (parameters == null)
            {
                return defaultValue;
            }


            JToken token =
                parameters[
                    key
                ];


            if (
                token == null ||
                token.Type !=
                JTokenType.Integer
            )
            {
                return defaultValue;
            }


            return token.Value<int>();
        }


        private static string MakeToken(
            string value
        )
        {
            return value
                .Replace(".", "_")
                .Replace("-", "_")
                .Replace(" ", "_")
                .ToUpperInvariant();
        }
    }
}
