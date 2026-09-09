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
        private static readonly Dictionary<string, UnlockableDef>
            CustomUnlockables =
                new Dictionary<string, UnlockableDef>();


        private static readonly Dictionary<string, AchievementDef>
            CustomAchievements =
                new Dictionary<string, AchievementDef>();


        private static readonly Dictionary<string, Sprite>
            CustomAchievementIcons =
                new Dictionary<string, Sprite>();


        private static readonly Dictionary<SurvivorDef, UnlockableDef>
            OriginalUnlockables =
                new Dictionary<SurvivorDef, UnlockableDef>();


        // =========================================================
        // ¿ES NUESTRO UNLOCK?
        // =========================================================

        public static bool IsCustomUnlock(
            UnlockableDef unlockableDef
        )
        {
            if (unlockableDef == null)
            {
                return false;
            }


            return
                unlockableDef.cachedName != null &&
                unlockableDef.cachedName.StartsWith(
                    "UniversalSurvivorUnlocks.",
                    StringComparison.Ordinal
                );
        }


        // =========================================================
        // ¿LA CONFIGURACIÓN REQUIERE UNLOCK?
        // =========================================================

        /// <summary>
        /// Indica si USU debe controlar actualmente el unlock según la
        /// configuración persistida. Esta versión también respeta el metadata
        /// de OriginalUnlock guardado en Survivors.json.
        /// </summary>
        public static bool RequiresCustomUnlock(
            SurvivorJsonEntry entry
        )
        {
            return RequiresCustomUnlock(
                entry,
                HasStoredOriginalUnlock(entry)
            );
        }


        /// <summary>
        /// Variante usada durante la detección temprana. originalUnlockAvailable
        /// permite respetar un unlock del autor incluso antes de que Sync() lo
        /// haya escrito en Survivors.json.
        ///
        /// Regla central 5G.1D-E:
        /// - Original => nunca USU.
        /// - AutomaticFallback + Original disponible => Original.
        /// - UserSelected USU/Community/Custom => la elección del jugador manda.
        /// </summary>
        public static bool RequiresCustomUnlock(
            SurvivorJsonEntry entry,
            bool originalUnlockAvailable
        )
        {
            if (
                entry == null ||
                entry.Challenge == null ||
                !entry.Challenge.Enabled
            )
            {
                return false;
            }


            MissionConfiguration missionConfig =
                entry.Challenge.MissionConfig;


            if (missionConfig != null)
            {
                if (
                    missionConfig.EffectiveProvider ==
                    UnlockProviderKind.Original
                )
                {
                    return false;
                }


                if (
                    missionConfig.EffectiveSelectionMode ==
                        UnlockSelectionMode.AutomaticFallback &&
                    (
                        originalUnlockAvailable ||
                        HasStoredOriginalUnlock(entry)
                    )
                )
                {
                    return false;
                }
            }
            else if (
                originalUnlockAvailable ||
                HasStoredOriginalUnlock(entry)
            )
            {
                // Compatibilidad con Survivors.json anteriores a providers:
                // el comportamiento histórico era respetar el original.
                return false;
            }


            bool hasLegacyType =
                !string.IsNullOrWhiteSpace(
                    entry.Challenge.Type
                ) &&
                !string.Equals(
                    entry.Challenge.Type,
                    "Original",
                    StringComparison.OrdinalIgnoreCase
                );


            bool hasMissionV2 =
                entry.Challenge.Mission != null &&
                entry.Challenge.Mission.Routes != null &&
                entry.Challenge.Mission.Routes.Count > 0;


            return
                hasLegacyType ||
                hasMissionV2;
        }


        /// <summary>
        /// Determina si Survivors.json conserva evidencia de un unlock original
        /// real perteneciente al creador del survivor.
        /// </summary>
        public static bool HasStoredOriginalUnlock(
            SurvivorJsonEntry entry
        )
        {
            if (entry == null)
            {
                return false;
            }


            string value =
                entry.OriginalUnlock?.Trim() ?? "";


            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }


            return
                !string.Equals(
                    value,
                    "Ninguno",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                !string.Equals(
                    value,
                    "None",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                !string.Equals(
                    value,
                    "Null",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        /// <summary>
        /// Un UnlockableDef de USU se prepara en startup aunque esté dormido.
        /// Esto hace posible Original -> USU sin intentar registrar contenido
        /// después de que RoR2 haya cerrado sus catálogos.
        /// </summary>
        private static bool CanPrepareCustomUnlock(
            SurvivorJsonEntry entry
        )
        {
            return
                entry != null &&
                entry.Challenge != null;
        }


        // =========================================================
        // REGISTRAR CONFIGURACIONES YA EXISTENTES
        // =========================================================

        public static void RegisterConfiguredUnlockables(
            ManualLogSource logger
        )
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (config == null)
            {
                return;
            }


            RegisterEntries(
                config.AvailableSurvivors,
                logger
            );


            /*
             * También registramos unavailable.
             *
             * Esto es importante si un mod fue
             * desinstalado y luego vuelve a instalarse.
             */
            RegisterEntries(
                config.UnavailableSurvivors,
                logger
            );
        }


        private static void RegisterEntries(
            Dictionary<
                string,
                SurvivorJsonEntry
            > entries,
            ManualLogSource logger
        )
        {
            if (entries == null)
            {
                return;
            }


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in entries
            )
            {
                string bodyName =
                    pair.Key;


                SurvivorJsonEntry entry =
                    pair.Value;


                if (!CanPrepareCustomUnlock(entry))
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
                    logger,
                    true
                );
            }
        }


        // =========================================================
        // REGISTRAR UNLOCK DINÁMICO
        // =========================================================

        public static UnlockableDef RegisterDynamicUnlockable(
            string bodyName,
            SurvivorJsonEntry entry,
            ManualLogSource logger
        )
        {
            if (
                CustomUnlockables.TryGetValue(
                    bodyName,
                    out UnlockableDef existing
                )
            )
            {
                return existing;
            }


            /*
             * false:
             *
             * NO usar ContentAddition porque en este
             * momento R2API ya construyó sus ContentPacks.
             *
             * UniversalContentPackProvider añadirá este
             * UnlockableDef a nuestro ContentPack dinámico.
             */
            return RegisterOneUnlockable(
                bodyName,
                entry,
                logger,
                false
            );
        }


        // =========================================================
        // BUSCAR UNLOCK
        // =========================================================

        public static bool TryGetCustomUnlockable(
            string bodyName,
            out UnlockableDef unlockable
        )
        {
            return CustomUnlockables.TryGetValue(
                bodyName,
                out unlockable
            );
        }

        // =========================================================
        // BUSCAR ACHIEVEMENT USU
        // =========================================================

        public static bool TryGetCustomAchievement(
            string bodyName,
            out AchievementDef achievementDef
        )
        {
            return CustomAchievements.TryGetValue(
                bodyName,
                out achievementDef
            );
        }

        // =========================================================
        // CONSULTAR UNLOCK ORIGINAL RECORDADO
        // =========================================================

        public static bool TryGetRememberedOriginalUnlock(
            SurvivorDef survivorDef,
            out UnlockableDef originalUnlock
        )
        {
            originalUnlock = null;


            if (survivorDef == null)
            {
                return false;
            }


            return OriginalUnlockables.TryGetValue(
                survivorDef,
                out originalUnlock
            );
        }


        public static bool HasOriginalUnlockAvailable(
            SurvivorDef survivorDef,
            SurvivorJsonEntry entry = null
        )
        {
            if (survivorDef != null)
            {
                UnlockableDef current =
                    survivorDef.unlockableDef;


                if (
                    current != null &&
                    !IsCustomUnlock(current)
                )
                {
                    return true;
                }


                if (
                    OriginalUnlockables.TryGetValue(
                        survivorDef,
                        out UnlockableDef remembered
                    ) &&
                    remembered != null
                )
                {
                    return true;
                }
            }


            return HasStoredOriginalUnlock(entry);
        }


        // =========================================================
        // RECORDAR UNLOCK ORIGINAL
        // =========================================================

        public static void RememberOriginalUnlock(
    SurvivorDef survivorDef,
    UnlockableDef originalUnlock
)
        {
            if (survivorDef == null)
            {
                return;
            }


            /*
             * Nunca almacenamos nuestro propio
             * unlock como "original".
             */
            if (
                IsCustomUnlock(
                    originalUnlock
                )
            )
            {
                return;
            }


            if (
                !OriginalUnlockables.TryGetValue(
                    survivorDef,
                    out UnlockableDef existing
                )
            )
            {
                OriginalUnlockables[
                    survivorDef
                ] =
                    originalUnlock;

                return;
            }


            /*
             * Caso importante:
             *
             * En una primera pasada el mod todavía
             * no había colocado su unlockableDef:
             *
             * original = null
             *
             * Y en una pasada posterior sí aparece.
             *
             * Debemos actualizar null -> unlock real.
             */
            if (
                existing == null &&
                originalUnlock != null
            )
            {
                OriginalUnlockables[
                    survivorDef
                ] =
                    originalUnlock;
            }
        }

        // =========================================================
        // RESTAURAR UNLOCK ORIGINAL
        // =========================================================

        public static void RestoreOriginalUnlock(
            SurvivorDef survivorDef
        )
        {
            if (survivorDef == null)
            {
                return;
            }


            if (
                OriginalUnlockables.TryGetValue(
                    survivorDef,
                    out UnlockableDef originalUnlock
                )
            )
            {
                survivorDef.unlockableDef =
                    originalUnlock;

                return;
            }


            /*
             * Si no conocemos ningún original
             * y actualmente tiene uno nuestro,
             * significa que originalmente era null.
             */
            if (
                IsCustomUnlock(
                    survivorDef.unlockableDef
                )
            )
            {
                survivorDef.unlockableDef =
                    null;
            }
        }


        // =========================================================
        // ASIGNACIÓN TEMPRANA
        // =========================================================

        public static void AssignEarlyCustomUnlock(
            SurvivorDef survivorDef,
            UnlockableDef customUnlock,
            ManualLogSource logger
        )
        {
            if (
                survivorDef == null ||
                customUnlock == null
            )
            {
                return;
            }


            UnlockableDef current =
                survivorDef.unlockableDef;


            if (
                !OriginalUnlockables.ContainsKey(
                    survivorDef
                )
            )
            {
                OriginalUnlockables[
                    survivorDef
                ] =
                    IsCustomUnlock(current)
                        ? null
                        : current;
            }


            // La construcción de ContentPacks puede volver a visitar
            // el mismo SurvivorDef varias veces durante el arranque.
            // Si nuestro unlock ya está aplicado, la operación es
            // idempotente: no reasignamos ni repetimos el mismo log.
            if (
                ReferenceEquals(
                    current,
                    customUnlock
                ) ||
                (
                    current != null &&
                    !string.IsNullOrWhiteSpace(current.cachedName) &&
                    !string.IsNullOrWhiteSpace(customUnlock.cachedName) &&
                    string.Equals(
                        current.cachedName,
                        customUnlock.cachedName,
                        StringComparison.Ordinal
                    )
                )
            )
            {
                return;
            }


            survivorDef.unlockableDef =
                customUnlock;


            UsuLog.Verbose(
                logger,
                $"Unlock temprano asignado | " +
                $"Survivor: {survivorDef.cachedName} | " +
                $"Unlock: {customUnlock.cachedName}"
            );
        }


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


        public static void ApplyCustomAchievementIconForSurvivor(
            SurvivorDef survivorDef,
            string bodyName,
            ManualLogSource logger
        )
        {
            if (
                survivorDef == null ||
                string.IsNullOrWhiteSpace(bodyName) ||
                !CustomUnlockables.TryGetValue(
                    bodyName,
                    out UnlockableDef unlockable
                ) ||
                unlockable == null
            )
            {
                return;
            }


            SurvivorInfo info =
                new SurvivorInfo
                {
                    SurvivorDef = survivorDef,
                    BodyName = bodyName,
                    DisplayName =
                        !string.IsNullOrWhiteSpace(survivorDef.cachedName)
                            ? survivorDef.cachedName
                            : bodyName
                };


            ApplySurvivorAchievementIcon(
                info,
                unlockable,
                logger
            );
        }


        // =========================================================
        // ICONO DEL ACHIEVEMENT
        // =========================================================

        private static void ApplySurvivorAchievementIcon(
            SurvivorInfo survivorInfo,
            UnlockableDef unlockable,
            ManualLogSource logger
        )
        {
            if (
                survivorInfo == null ||
                survivorInfo.SurvivorDef == null ||
                survivorInfo.SurvivorDef.bodyPrefab == null
            )
            {
                return;
            }


            CharacterBody body =
                survivorInfo
                    .SurvivorDef
                    .bodyPrefab
                    .GetComponent<CharacterBody>();


            if (
                body == null ||
                body.portraitIcon == null
            )
            {
                logger.LogWarning(
                    $"No se encontró portraitIcon para " +
                    $"{survivorInfo.BodyName}."
                );


                return;
            }


            Sprite icon;


            if (
                !CustomAchievementIcons.TryGetValue(
                    survivorInfo.BodyName,
                    out icon
                ) ||
                icon == null
            )
            {
                icon =
                    CreateFramedAchievementIcon(
                        body.portraitIcon,
                        survivorInfo.BodyName
                    );


                if (icon == null)
                {
                    logger.LogWarning(
                        $"No se pudo crear el icono con marco para " +
                        $"{survivorInfo.BodyName}."
                    );


                    return;
                }


                CustomAchievementIcons[
                    survivorInfo.BodyName
                ] =
                    icon;
            }


            unlockable.achievementIcon =
                icon;


            if (
                CustomAchievements.TryGetValue(
                    survivorInfo.BodyName,
                    out AchievementDef achievementDef
                )
            )
            {
                achievementDef.achievedIcon =
                    icon;
            }


            UsuLog.Verbose(
                logger,
                $"Icono de achievement con marco actualizado: " +
                $"{survivorInfo.DisplayName}"
            );
        }


        // =========================================================
        // CREAR SPRITE CON MARCO
        // =========================================================

        private static Sprite CreateFramedAchievementIcon(
            Texture source,
            string bodyName
        )
        {
            if (source == null)
            {
                return null;
            }


            RenderTexture temporary =
                RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.ARGB32
                );


            RenderTexture previous =
                RenderTexture.active;


            try
            {
                Graphics.Blit(
                    source,
                    temporary
                );


                RenderTexture.active =
                    temporary;


                Texture2D texture =
                    new Texture2D(
                        source.width,
                        source.height,
                        TextureFormat.RGBA32,
                        false
                    );


                texture.ReadPixels(
                    new Rect(
                        0,
                        0,
                        source.width,
                        source.height
                    ),
                    0,
                    0
                );


                texture.Apply();


                texture.name =
                    $"USU_{bodyName}_AchievementTexture";


                texture.wrapMode =
                    TextureWrapMode.Clamp;


                texture.filterMode =
                    FilterMode.Bilinear;


                /*
                 * IMPORTANTE:
                 *
                 * Los portraits de muchos survivors modded
                 * poseen transparencia.
                 *
                 * El popup de desbloqueo de RoR2 tiene el
                 * gráfico del candado detrás del achievementIcon,
                 * por lo que puede verse a través del portrait.
                 *
                 * Convertimos únicamente ESTE icono de achievement
                 * en una imagen completamente opaca.
                 *
                 * Esto NO modifica el portrait utilizado
                 * en la selección de personajes.
                 */
                FillAchievementBackground(
                    texture
                );


                /*
                 * Después dibujamos nuestro marco
                 * sobre el fondo ya rellenado.
                 */
                DrawAchievementFrame(
                    texture
                );

                Sprite sprite =
                    Sprite.Create(
                        texture,
                        new Rect(
                            0,
                            0,
                            texture.width,
                            texture.height
                        ),
                        new Vector2(
                            0.5f,
                            0.5f
                        ),
                        100f
                    );


                sprite.name =
                    $"USU_{bodyName}_AchievementIcon";


                return sprite;
            }
            finally
            {
                RenderTexture.active =
                    previous;


                RenderTexture.ReleaseTemporary(
                    temporary
                );
            }
        }

        // =========================================================
        // RELLENAR FONDO DEL ICONO DE ACHIEVEMENT
        // =========================================================

        private static void FillAchievementBackground(
            Texture2D texture
        )
        {
            if (texture == null)
            {
                return;
            }


            /*
             * Fondo oscuro similar al utilizado
             * dentro de los iconos de achievements.
             *
             * Lo importante es que alpha = 255,
             * para que el candado del popup
             * no pueda verse por detrás.
             */
            Color32 backgroundColor =
                new Color32(
                    24,
                    40,
                    57,
                    255
                );


            Color32[] pixels =
                texture.GetPixels32();


            for (
                int i = 0;
                i < pixels.Length;
                i++
            )
            {
                Color32 source =
                    pixels[i];


                float alpha =
                    source.a / 255f;


                /*
                 * Componemos el portrait encima
                 * del fondo opaco.
                 *
                 * Si el pixel era:
                 *
                 * alpha 255 → queda igual
                 * alpha 0   → queda fondo
                 * alpha intermedio → mezcla normal
                 */
                byte r =
                    (byte)Mathf.RoundToInt(
                        source.r * alpha +
                        backgroundColor.r *
                        (1f - alpha)
                    );


                byte g =
                    (byte)Mathf.RoundToInt(
                        source.g * alpha +
                        backgroundColor.g *
                        (1f - alpha)
                    );


                byte b =
                    (byte)Mathf.RoundToInt(
                        source.b * alpha +
                        backgroundColor.b *
                        (1f - alpha)
                    );


                pixels[i] =
                    new Color32(
                        r,
                        g,
                        b,
                        255
                    );
            }


            texture.SetPixels32(
                pixels
            );


            texture.Apply();
        }

        // =========================================================
        // MARCO
        // =========================================================

        private static void DrawAchievementFrame(
            Texture2D texture
        )
        {
            int width =
                texture.width;


            int height =
                texture.height;


            int borderThickness =
                Mathf.Max(
                    2,
                    Mathf.RoundToInt(
                        Mathf.Min(
                            width,
                            height
                        ) * 0.018f
                    )
                );


            Color32 frameColor =
                new Color32(
                    73,
                    99,
                    124,
                    255
                );


            Color32 outerColor =
                new Color32(
                    20,
                    25,
                    30,
                    255
                );


            int outerThickness =
                Mathf.Max(
                    1,
                    borderThickness / 2
                );


            DrawTextureBorder(
                texture,
                outerColor,
                borderThickness +
                outerThickness
            );


            DrawTextureBorder(
                texture,
                frameColor,
                borderThickness
            );


            texture.Apply();
        }


        private static void DrawTextureBorder(
            Texture2D texture,
            Color32 color,
            int thickness
        )
        {
            int width =
                texture.width;


            int height =
                texture.height;


            for (
                int y = 0;
                y < height;
                y++
            )
            {
                for (
                    int x = 0;
                    x < width;
                    x++
                )
                {
                    bool border =
                        x < thickness ||
                        x >= width - thickness ||
                        y < thickness ||
                        y >= height - thickness;


                    if (!border)
                    {
                        continue;
                    }


                    texture.SetPixel(
                        x,
                        y,
                        color
                    );
                }
            }
        }


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