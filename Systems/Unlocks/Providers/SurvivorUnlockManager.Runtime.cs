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
        // CREAR UNLOCKABLE + ACHIEVEMENT
        // =========================================================

        private static UnlockableDef RegisterOneUnlockable(
            string bodyName,
            SurvivorJsonEntry entry,
            ManualLogSource logger,
            bool addThroughContentAddition
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                ) ||
                entry == null
            )
            {
                return null;
            }


            if (
                CustomUnlockables.TryGetValue(
                    bodyName,
                    out UnlockableDef existing
                )
            )
            {
                return existing;
            }


            string identifier =
                $"UniversalSurvivorUnlocks.{bodyName}";


            string achievementIdentifier =
                $"UniversalSurvivorUnlocks.{bodyName}.Achievement";


            // Registra una sola vez las traducciones integradas de USU.
            // R2API.Language resolverá después el valor según el idioma
            // actual de cada cliente.
            SurvivorLocalization.EnsureRegistered();


            bool usesBuiltInLocalization =
                SurvivorLocalization.UsesBuiltInLocalization(
                    bodyName,
                    entry.Challenge
                );


            string bodyTokenPrefix =
                $"USU_{MakeToken(bodyName)}";


            string tokenPrefix =
                usesBuiltInLocalization
                    ? SurvivorLocalization.GetOfficialTokenPrefix(bodyName)
                    : SurvivorLocalization.GetCustomTokenPrefix(bodyName);


            string unlockableNameToken =
                $"{bodyTokenPrefix}_UNLOCKABLE_NAME";


            string achievementNameToken =
                $"{tokenPrefix}_ACHIEVEMENT_NAME";


            string achievementDescriptionToken =
                $"{tokenPrefix}_ACHIEVEMENT_DESCRIPTION";


            string challengeName =
                GetChallengeName(
                    entry
                );


            string challengeDescription =
                BuildChallengeDescription(
                    entry
                );


            // =====================================================
            // TEXTOS
            // =====================================================

            LanguageAPI.Add(
                unlockableNameToken,
                entry.DisplayName
            );


            // Los presets oficiales ya fueron registrados por
            // SurvivorLocalization en en / es-419 / es-ES.
            //
            // Las misiones personalizadas conservan el comportamiento
            // anterior: el texto escrito por el usuario se registra como
            // fallback literal y NO se traduce automáticamente.
            if (!usesBuiltInLocalization)
            {
                LanguageAPI.Add(
                    achievementNameToken,
                    challengeName
                );


                LanguageAPI.Add(
                    achievementDescriptionToken,
                    challengeDescription
                );
            }


            // =====================================================
            // UNLOCKABLE DEF
            // =====================================================

            UnlockableDef unlockable =
                ScriptableObject.CreateInstance<
                    UnlockableDef
                >();


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
                    string baseText =
                        Language.GetStringFormatted(
                            "UNLOCK_VIA_ACHIEVEMENT_FORMAT",
                            new object[]
                            {
                                Language.GetString(
                                    achievementNameToken
                                ),

                                Language.GetString(
                                    achievementDescriptionToken
                                )
                            }
                        );


                    string restrictionNotice =
                        BuildRestrictionNotice(
                            entry
                        );


                    if (
                        string.IsNullOrWhiteSpace(
                            restrictionNotice
                        )
                    )
                    {
                        return baseText;
                    }


                    // El aviso NO forma parte de la descripción temática.
                    // Se añade como metadata visual independiente debajo.
                    return
                        baseText +
                        "\n\n" +
                        restrictionNotice;
                };


            unlockable.getUnlockedString =
                () =>
                {
                    return Language.GetStringFormatted(
                        "UNLOCKED_FORMAT",
                        new object[]
                        {
                            Language.GetString(
                                achievementNameToken
                            ),

                            Language.GetString(
                                achievementDescriptionToken
                            )
                        }
                    );
                };


            // =====================================================
            // ACHIEVEMENT DEF
            // =====================================================

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

                    // RoR2 usa este campo para:
                    // 1) mostrar la Moneda lunar +N en la UI vanilla;
                    // 2) conceder la recompensa cuando el achievement
                    //    se obtiene por primera vez mediante AddAchievement().
                    lunarCoinReward =
                        GetLunarCoinReward(
                            entry
                        ),

                    type =
                        ChallengeManager
                        .GetAchievementType(
                            entry.Challenge
                        ),

                    serverTrackerType =
                    ChallengeManager
                        .GetServerTrackerType(
                            entry.Challenge
                        )
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


                return null;
            }


            /*
             * Ruta tradicional:
             *
             * Se utiliza durante Awake antes de que
             * R2API genere sus ContentPacks.
             */
            if (addThroughContentAddition)
            {
                ContentAddition.AddUnlockableDef(
                    unlockable
                );
            }


            CustomAchievements[
                bodyName
            ] =
                achievementDef;


            CustomUnlockables[
                bodyName
            ] =
                unlockable;


            UsuLog.Verbose(
                logger,
                $"Unlock registrado correctamente | " +
                $"Survivor: {bodyName} | " +
                $"Unlockable: {identifier} | " +
                $"Achievement: {achievementIdentifier} | " +
                $"MonedasLunares: {achievementDef.lunarCoinReward} | " +
                $"Ruta: " +
                $"{(addThroughContentAddition ? "R2API" : "DynamicContentPack")}"
            );


            return unlockable;
        }


        // =========================================================
        // APLICAR CONFIGURACIÓN FINAL
        // =========================================================

        public static void ApplyConfiguredUnlockables(
            List<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            if (survivors == null)
            {
                return;
            }


            foreach (
                SurvivorInfo survivorInfo
                in survivors
            )
            {
                if (
                    survivorInfo == null ||
                    survivorInfo.SurvivorDef == null ||
                    !survivorInfo.IsModded ||
                    survivorInfo.Status != SurvivorStatus.Available
                )
                {
                    continue;
                }


                SurvivorDef survivorDef =
                    survivorInfo.SurvivorDef;

                string bodyName =
                    survivorInfo.BodyName;

                SurvivorJsonEntry entry =
                    GetEntry(
                        bodyName
                    );


                UnlockableDef currentUnlock =
                    survivorDef.unlockableDef;


                if (
                    currentUnlock == null ||
                    !IsCustomUnlock(currentUnlock)
                )
                {
                    RememberOriginalUnlock(
                        survivorDef,
                        currentUnlock
                    );
                }


                bool originalAvailable =
                    HasOriginalUnlockAvailable(
                        survivorDef,
                        entry
                    );


                if (
                    !RequiresCustomUnlock(
                        entry,
                        originalAvailable
                    )
                )
                {
                    RestoreOriginalUnlock(
                        survivorDef
                    );


                    UsuLog.Verbose(
                        logger,
                        $"Provider runtime: Original | " +
                        $"{survivorInfo.DisplayName}"
                    );


                    continue;
                }


                if (
                    !CustomUnlockables.TryGetValue(
                        bodyName,
                        out UnlockableDef customUnlock
                    ) ||
                    customUnlock == null
                )
                {
                    logger?.LogWarning(
                        $"No existe UnlockableDef USU preparado para " +
                        $"{bodyName}. El cambio de provider requerirá " +
                        $"reiniciar el juego."
                    );


                    continue;
                }


                AssignEarlyCustomUnlock(
                    survivorDef,
                    customUnlock,
                    logger
                );


                ApplySurvivorAchievementIcon(
                    survivorInfo,
                    customUnlock,
                    logger
                );


                UsuLog.Verbose(
                    logger,
                    $"Provider runtime: " +
                    $"{entry?.Challenge?.MissionConfig?.EffectiveProvider.ToString() ?? "USU"} | " +
                    $"{survivorInfo.DisplayName} | " +
                    $"{customUnlock.cachedName}"
                );
            }
        }
    }
}
