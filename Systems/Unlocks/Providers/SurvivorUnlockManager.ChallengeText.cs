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
            int configured =
                entry?.Challenge?.LunarCoinReward
                ?? 0;


            configured =
                Math.Max(
                    0,
                    Math.Min(
                        10,
                        configured
                    )
                );


            return (uint)configured;
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
        // AVISOS DE RESTRICCIONES
        // =========================================================
        //
        // La descripción de la misión se mantiene temática. Las
        // restricciones estructurales se muestran debajo como metadata.
        //
        // Para ExcludedSurvivor sólo mostramos cuerpos excluidos en TODAS
        // las rutas de la misión. Así evitamos afirmar que un survivor no
        // es válido cuando otra ruta OR sí permitiría completarla.
        // =========================================================

        private static string BuildRestrictionNotice(
            SurvivorJsonEntry entry
        )
        {
            List<string> excludedBodies =
                GetMissionWideExcludedBodies(
                    entry
                );


            if (excludedBodies.Count == 0)
            {
                return "";
            }


            List<string> displayNames =
                new List<string>();


            for (int i = 0; i < excludedBodies.Count; i++)
            {
                string displayName =
                    ResolveSurvivorDisplayName(
                        excludedBodies[i]
                    );


                if (
                    !string.IsNullOrWhiteSpace(
                        displayName
                    )
                )
                {
                    displayNames.Add(
                        displayName
                    );
                }
            }


            if (displayNames.Count == 0)
            {
                return "";
            }


            string joinedNames =
                string.Join(
                    " / ",
                    displayNames.ToArray()
                );


            return Language.GetStringFormatted(
                SurvivorLocalization.ExcludedSurvivorsNoticeToken,
                new object[]
                {
                    joinedNames
                }
            );
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
