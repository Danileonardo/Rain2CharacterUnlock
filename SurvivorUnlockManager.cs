using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorUnlockManager
    {
        private static readonly Dictionary<
            string,
            UnlockableDef
        > CustomUnlockables =
            new Dictionary<
                string,
                UnlockableDef
            >();

        private static readonly Dictionary<
            SurvivorDef,
            UnlockableDef
        > OriginalUnlockables =
            new Dictionary<
                SurvivorDef,
                UnlockableDef
            >();

        public static bool IsCustomUnlock(
            UnlockableDef unlockableDef
        )
        {
            if (unlockableDef == null)
            {
                return false;
            }

            return unlockableDef.cachedName != null &&
                   unlockableDef.cachedName.StartsWith(
                       "UniversalSurvivorUnlocks.",
                       StringComparison.Ordinal
                   );
        }

        public static void RegisterConfiguredUnlockables(
            ManualLogSource logger
        )
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;

            if (
                config == null ||
                config.AvailableSurvivors == null
            )
            {
                return;
            }

            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in config.AvailableSurvivors
            )
            {
                string bodyName =
                    pair.Key;

                SurvivorJsonEntry entry =
                    pair.Value;

                if (!RequiresCustomUnlock(entry))
                {
                    continue;
                }

                if (
                    CustomUnlockables.ContainsKey(
                        bodyName
                    )
                )
                {
                    continue;
                }

                RegisterOneUnlockable(
                    bodyName,
                    entry,
                    logger
                );
            }
        }

        private static void RegisterOneUnlockable(
            string bodyName,
            SurvivorJsonEntry entry,
            ManualLogSource logger
        )
        {
            string identifier =
                $"UniversalSurvivorUnlocks.{bodyName}";

            string achievementIdentifier =
                $"UniversalSurvivorUnlocks.{bodyName}.Achievement";

            string tokenPrefix =
                $"USU_{MakeToken(bodyName)}";

            string unlockableNameToken =
                $"{tokenPrefix}_UNLOCKABLE_NAME";

            string achievementNameToken =
                $"{tokenPrefix}_ACHIEVEMENT_NAME";

            string achievementDescriptionToken =
                $"{tokenPrefix}_ACHIEVEMENT_DESCRIPTION";

            string challengeName =
                GetChallengeName(entry);

            string challengeDescription =
                BuildChallengeDescription(entry);

            /*
             * Textos usados por RoR2.
             */
            LanguageAPI.Add(
                unlockableNameToken,
                entry.DisplayName
            );

            LanguageAPI.Add(
                achievementNameToken,
                challengeName
            );

            LanguageAPI.Add(
                achievementDescriptionToken,
                challengeDescription
            );

            /*
             * Creamos el UnlockableDef real.
             */
            UnlockableDef unlockable =
                ScriptableObject
                    .CreateInstance<UnlockableDef>();

            unlockable.cachedName =
                identifier;

            unlockable.nameToken =
                unlockableNameToken;

            unlockable.sortScore =
                200;

            unlockable.hidden =
                false;

            unlockable.achievementIcon =
                LegacyResourcesAPI.Load<Sprite>(
                    "Textures/MiscIcons/texUnlockIcon"
                );

            unlockable.getHowToUnlockString =
                () =>
                {
                    SurvivorJsonEntry current =
                        GetEntry(bodyName)
                        ?? entry;

                    return Language.GetStringFormatted(
                        "UNLOCK_VIA_ACHIEVEMENT_FORMAT",
                        new object[]
                        {
                            GetChallengeName(current),
                            BuildChallengeDescription(current)
                        }
                    );
                };

            unlockable.getUnlockedString =
                () =>
                {
                    SurvivorJsonEntry current =
                        GetEntry(bodyName)
                        ?? entry;

                    return Language.GetStringFormatted(
                        "UNLOCKED_FORMAT",
                        new object[]
                        {
                            GetChallengeName(current),
                            BuildChallengeDescription(current)
                        }
                    );
                };

            /*
             * Añadimos el UnlockableDef al contenido.
             */
            ContentAddition.AddUnlockableDef(
                unlockable
            );

            /*
             * ÉSTA era la pieza que nos faltaba.
             *
             * Creamos un AchievementDef asociado al
             * UnlockableDef.
             */
            AchievementDef achievementDef =
                new AchievementDef
                {
                    identifier =
                        achievementIdentifier,

                    unlockableRewardIdentifier =
                        identifier,

                    prerequisiteAchievementIdentifier =
                        null,

                    nameToken =
                        achievementNameToken,

                    descriptionToken =
                        achievementDescriptionToken,

                    achievedIcon =
                        unlockable.achievementIcon,

                    type =
                        typeof(
                            UniversalSurvivorAchievement
                        ),

                    serverTrackerType =
                        null
                };

#pragma warning disable CS0618

            bool achievementAdded =
                UnlockableAPI.AddAchievement(
                    achievementDef
                );

#pragma warning restore CS0618

            if (!achievementAdded)
            {
                logger.LogError(
                    $"No se pudo registrar el achievement " +
                    $"para {bodyName}."
                );

                return;
            }

            CustomUnlockables[
                bodyName
            ] =
                unlockable;

            logger.LogInfo(
                $"Unlock registrado correctamente | " +
                $"Survivor: {bodyName} | " +
                $"Unlockable: {identifier} | " +
                $"Achievement: {achievementIdentifier}"
            );
        }

        public static void ApplyConfiguredUnlockables(
            List<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            foreach (
                SurvivorInfo survivorInfo
                in survivors
            )
            {
                if (
                    survivorInfo == null ||
                    survivorInfo.SurvivorDef == null
                )
                {
                    continue;
                }

                if (
                    survivorInfo.Status !=
                    SurvivorStatus.Available
                )
                {
                    continue;
                }

                SurvivorDef survivorDef =
                    survivorInfo.SurvivorDef;

                string bodyName =
                    survivorInfo.BodyName;

                /*
                 * Guardamos el unlock original
                 * una sola vez.
                 */
                if (
                    !OriginalUnlockables
                        .ContainsKey(
                            survivorDef
                        )
                )
                {
                    OriginalUnlockables[
                        survivorDef
                    ] =
                        survivorDef.unlockableDef;
                }

                SurvivorJsonEntry entry =
                    GetEntry(bodyName);

                /*
                 * Si nuestra misión NO está activa,
                 * dejamos/restauramos el desbloqueo
                 * original.
                 */
                if (!RequiresCustomUnlock(entry))
                {
                    survivorDef.unlockableDef =
                        OriginalUnlockables[
                            survivorDef
                        ];

                    continue;
                }

                if (
                    !CustomUnlockables.TryGetValue(
                        bodyName,
                        out UnlockableDef customUnlock
                    )
                )
                {
                    logger.LogWarning(
                        $"No existe UnlockableDef registrado " +
                        $"para {bodyName}."
                    );

                    continue;
                }

                survivorDef.unlockableDef =
                    customUnlock;

                logger.LogInfo(
                    $"Unlock personalizado asignado: " +
                    $"{survivorInfo.DisplayName} | " +
                    $"{customUnlock.cachedName}"
                );
            }
        }

        private static bool RequiresCustomUnlock(
            SurvivorJsonEntry entry
        )
        {
            if (
                entry == null ||
                entry.Challenge == null
            )
            {
                return false;
            }

            if (!entry.Challenge.Enabled)
            {
                return false;
            }

            if (
                string.IsNullOrWhiteSpace(
                    entry.Challenge.Type
                )
            )
            {
                return false;
            }

            return !string.Equals(
                entry.Challenge.Type,
                "Original",
                StringComparison.OrdinalIgnoreCase
            );
        }

        private static SurvivorJsonEntry GetEntry(
            string bodyName
        )
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;

            if (
                config == null ||
                config.AvailableSurvivors == null
            )
            {
                return null;
            }

            config
                .AvailableSurvivors
                .TryGetValue(
                    bodyName,
                    out SurvivorJsonEntry entry
                );

            return entry;
        }

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

            return "Desafío personalizado";
        }

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

            JObject parameters =
                entry.Challenge.Parameters;

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

                default:
                    return
                        $"Completa la misión " +
                        $"\"{entry.Challenge.Type}\".";
            }
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
                parameters[key];

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