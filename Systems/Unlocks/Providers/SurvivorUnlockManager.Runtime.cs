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

        /// <summary>
        /// Nombre nativo del desbloqueable, separado del nombre de misión.
        /// Registro explícito para en, es-419 y es-ES, más el idioma activo.
        /// Si la fuente contiene otro token aún no disponible durante Awake,
        /// usar el nombre del survivor y refrescar al cambiar de idioma.
        /// </summary>
        private static void RegisterUnlockableDisplayNameTokens(
            string bodyName,
            SurvivorJsonEntry entry
        )
        {
            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return;
            }

            string token = $"USU_{MakeToken(bodyName)}_UNLOCKABLE_NAME";
            string rawName = entry?.DisplayName?.Trim() ?? "";
            string name = rawName;

            if (!string.IsNullOrWhiteSpace(rawName))
            {
                string localized = Language.GetString(rawName);
                if (!string.IsNullOrWhiteSpace(localized) &&
                    !string.Equals(localized, rawName, StringComparison.Ordinal))
                {
                    name = localized;
                }
            }

            // Si DisplayName es otro token (GWENGWEN_NAME, etc.) que aún
            // no se ha registrado, no propagamos ese token al chat vanilla.
            if (string.IsNullOrWhiteSpace(name) ||
                (string.Equals(name, rawName, StringComparison.Ordinal) &&
                 name.IndexOf('_') >= 0 &&
                 string.Equals(name, name.ToUpperInvariant(),
                     StringComparison.Ordinal)))
            {
                name = ResolveSurvivorDisplayName(bodyName);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = bodyName;
            }

            LanguageAPI.Add(token, name);
            LanguageAPI.Add(token, name, "en");
            LanguageAPI.Add(token, name, "es-419");
            LanguageAPI.Add(token, name, "es-ES");

            string activeLanguage = Language.currentLanguageName ?? "";
            if (!string.IsNullOrWhiteSpace(activeLanguage))
            {
                LanguageAPI.Add(token, name, activeLanguage);
            }
        }


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


            string sourceTokenPrefix =
                usesBuiltInLocalization
                    ? SurvivorLocalization.GetOfficialTokenPrefix(bodyName)
                    : SurvivorLocalization.GetCustomTokenPrefix(bodyName);


            string sourceAchievementNameToken =
                $"{sourceTokenPrefix}_ACHIEVEMENT_NAME";


            string sourceAchievementDescriptionToken =
                $"{sourceTokenPrefix}_ACHIEVEMENT_DESCRIPTION";


            string unlockableNameToken =
                $"{bodyTokenPrefix}_UNLOCKABLE_NAME";


            /*
             * G8 Phase 1.3:
             * AchievementDef usa tokens EFFECTIVE estables. Así un popup
             * vanilla/right-click puede actualizar su presentación cuando
             * G7/G8 sustituye el placeholder histórico o cuando el host
             * aporta otra misión para la sesión.
             */
            string achievementNameToken =
                GetEffectiveAchievementNameToken(
                    bodyName
                );


            string achievementDescriptionToken =
                GetEffectiveAchievementDescriptionToken(
                    bodyName
                );


            string challengeName =
                usesBuiltInLocalization
                    ? Language.GetString(
                        sourceAchievementNameToken
                    )
                    : GetChallengeName(
                        entry
                    );


            string challengeDescription =
                usesBuiltInLocalization
                    ? Language.GetString(
                        sourceAchievementDescriptionToken
                    )
                    : BuildChallengeDescription(
                        entry
                    );


            // =====================================================
            // TEXTOS
            // =====================================================

            // El juego usa este token en su propio chat al conceder el
            // UnlockableDef. Sólo Add(token, value) dejaba el token crudo
            // en es-419/es-ES (p. ej. USU_SPYBODY_UNLOCKABLE_NAME).
            RegisterUnlockableDisplayNameTokens(bodyName, entry);


            // Los presets oficiales ya fueron registrados por
            // SurvivorLocalization en en / es-419 / es-ES.
            //
            // Conservamos también los tokens CUSTOM históricos por
            // compatibilidad con configuraciones/UI previas, pero el
            // AchievementDef nuevo apunta siempre a EFFECTIVE.
            if (!usesBuiltInLocalization)
            {
                LanguageAPI.Add(
                    sourceAchievementNameToken,
                    challengeName
                );


                LanguageAPI.Add(
                    sourceAchievementDescriptionToken,
                    challengeDescription
                );
            }


            LanguageAPI.Add(
                achievementNameToken,
                challengeName
            );


            LanguageAPI.Add(
                achievementDescriptionToken,
                challengeDescription
            );


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
                    /*
                     * G8 Phase 1.2:
                     * el UnlockableDef se registra durante Awake, antes de que
                     * G7/G8 construyan una Generated Mission. Por eso NO
                     * debemos capturar para siempre el texto persistido del
                     * fallback histórico (KillEnemies 100).
                     *
                     * Resolvemos la entrada efectiva en el momento en que la
                     * UI pregunta el tooltip. SessionMissionRegistry aplica
                     * entonces el overlay Generated runtime-only si corresponde.
                     */
                    SurvivorJsonEntry displayEntry =
                        GetEntry(bodyName)
                        ?? entry;


                    string displayName =
                        ResolveChallengeDisplayName(
                            bodyName,
                            displayEntry,
                            achievementNameToken
                        );


                    string displayDescription =
                        ResolveChallengeDisplayDescription(
                            bodyName,
                            displayEntry,
                            achievementDescriptionToken
                        );


                    string baseText =
                        Language.GetStringFormatted(
                            "UNLOCK_VIA_ACHIEVEMENT_FORMAT",
                            new object[]
                            {
                                displayName,
                                displayDescription
                            }
                        );


                    string restrictionNotice =
                        BuildRestrictionNotice(
                            displayEntry
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
                    SurvivorJsonEntry displayEntry =
                        GetEntry(bodyName)
                        ?? entry;


                    return Language.GetStringFormatted(
                        "UNLOCKED_FORMAT",
                        new object[]
                        {
                            ResolveChallengeDisplayName(
                                bodyName,
                                displayEntry,
                                achievementNameToken
                            ),

                            ResolveChallengeDisplayDescription(
                                bodyName,
                                displayEntry,
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
