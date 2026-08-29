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

        public static bool RequiresCustomUnlock(
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


            survivorDef.unlockableDef =
                customUnlock;


            logger.LogInfo(
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


            string tokenPrefix =
                $"USU_{MakeToken(bodyName)}";


            string unlockableNameToken =
                $"{tokenPrefix}_UNLOCKABLE_NAME";


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
                    SurvivorJsonEntry current =
                        GetEntry(
                            bodyName
                        )
                        ?? entry;


                    return Language.GetStringFormatted(
                        "UNLOCK_VIA_ACHIEVEMENT_FORMAT",
                        new object[]
                        {
                            GetChallengeName(
                                current
                            ),

                            BuildChallengeDescription(
                                current
                            )
                        }
                    );
                };


            unlockable.getUnlockedString =
                () =>
                {
                    SurvivorJsonEntry current =
                        GetEntry(
                            bodyName
                        )
                        ?? entry;


                    return Language.GetStringFormatted(
                        "UNLOCKED_FORMAT",
                        new object[]
                        {
                            GetChallengeName(
                                current
                            ),

                            BuildChallengeDescription(
                                current
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


            logger.LogInfo(
                $"Unlock registrado correctamente | " +
                $"Survivor: {bodyName} | " +
                $"Unlockable: {identifier} | " +
                $"Achievement: {achievementIdentifier} | " +
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


                /*
                 * Oficial = intocable.
                 */
                if (!survivorInfo.IsModded)
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


                if (
                    !OriginalUnlockables.ContainsKey(
                        survivorDef
                    )
                )
                {
                    OriginalUnlockables[
                        survivorDef
                    ] =
                        IsCustomUnlock(
                            survivorDef.unlockableDef
                        )
                            ? null
                            : survivorDef.unlockableDef;
                }


                /*
                 * Si el autor del survivor tiene
                 * su propio unlock, nunca lo sustituimos.
                 */
                if (survivorInfo.HasOriginalUnlock)
                {
                    /*
                     * Si en este momento vemos directamente
                     * el unlock del autor, lo consideramos
                     * la fuente definitiva.
                     */
                    if (
                        survivorDef.unlockableDef != null &&
                        !IsCustomUnlock(
                            survivorDef.unlockableDef
                        )
                    )
                    {
                        OriginalUnlockables[
                            survivorDef
                        ] =
                            survivorDef.unlockableDef;
                    }


                    RestoreOriginalUnlock(
                        survivorDef
                    );


                    logger.LogInfo(
                        $"Unlock original respetado: " +
                        $"{survivorInfo.DisplayName}"
                    );


                    continue;
                }


                SurvivorJsonEntry entry =
                    GetEntry(
                        bodyName
                    );


                /*
                 * enabled=false:
                 *
                 * el usuario decidió dejar al
                 * personaje libre.
                 */
                if (!RequiresCustomUnlock(entry))
                {
                    RestoreOriginalUnlock(
                        survivorDef
                    );

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


                ApplySurvivorAchievementIcon(
                    survivorInfo,
                    customUnlock,
                    logger
                );


                logger.LogInfo(
                    $"Unlock personalizado asignado: " +
                    $"{survivorInfo.DisplayName} | " +
                    $"{customUnlock.cachedName}"
                );
            }
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


            logger.LogInfo(
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
            return SurvivorJsonManager
                .GetEntryAnywhere(
                    bodyName
                );
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


            // =========================================================
            // DESCRIPCIÓN PERSONALIZADA / PRESET
            // =========================================================
            //
            // Si el challenge contiene una descripción escrita,
            // esa descripción tiene prioridad.
            //
            // Esto permite que SurvivorChallengePresets.cs
            // controle directamente:
            //
            // - Nombre
            // - Descripción
            // - Tipo
            // - Parámetros
            //
            // Si Description está vacío, usamos el sistema
            // automático de abajo como fallback.
            // =========================================================

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