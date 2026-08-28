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
        /*
         * Unlockables creados por nuestro mod.
         *
         * Ejemplo:
         * UniversalSurvivorUnlocks.CommandoBody
         */
        private static readonly Dictionary<string, UnlockableDef>
            CustomUnlockables =
                new Dictionary<string, UnlockableDef>();

        /*
         * Achievements asociados a nuestros UnlockableDef.
         */
        private static readonly Dictionary<string, AchievementDef>
            CustomAchievements =
                new Dictionary<string, AchievementDef>();

        /*
         * Iconos generados en runtime para los achievements.
         */
        private static readonly Dictionary<string, Sprite>
            CustomAchievementIcons =
                new Dictionary<string, Sprite>();

        /*
         * Guardamos el unlock original de cada survivor
         * para poder restaurarlo si la misión personalizada
         * se desactiva.
         */
        private static readonly Dictionary<SurvivorDef, UnlockableDef>
            OriginalUnlockables =
                new Dictionary<SurvivorDef, UnlockableDef>();


        // =========================================================
        // DETECTAR SI UN UNLOCK PERTENECE A NUESTRO MOD
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
        // REGISTRAR LOS UNLOCKABLES CONFIGURADOS
        // =========================================================

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
                KeyValuePair<string, SurvivorJsonEntry> pair
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


        // =========================================================
        // CREAR UN UNLOCKABLE + ACHIEVEMENT
        // =========================================================

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


            // -----------------------------------------------------
            // TEXTOS
            // -----------------------------------------------------

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


            // -----------------------------------------------------
            // UNLOCKABLE DEF
            // -----------------------------------------------------

            UnlockableDef unlockable =
                ScriptableObject.CreateInstance<UnlockableDef>();

            unlockable.cachedName =
                identifier;

            unlockable.nameToken =
                unlockableNameToken;

            unlockable.sortScore =
                200;

            unlockable.hidden =
                false;


            /*
             * Icono provisional.
             *
             * Después, cuando SurvivorCatalog ya esté disponible,
             * lo reemplazaremos por el retrato del personaje
             * con nuestro marco generado en runtime.
             */
            unlockable.achievementIcon =
                LegacyResourcesAPI.Load<Sprite>(
                    "Textures/MiscIcons/texUnlockIcon"
                );


            /*
             * Texto mostrado al poner el mouse encima
             * del survivor bloqueado.
             *
             * Ejemplo:
             *
             * Requiere «Exterminador»
             * Derrota 100 enemigos.
             */
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


            ContentAddition.AddUnlockableDef(
                unlockable
            );


            // -----------------------------------------------------
            // ACHIEVEMENT DEF
            // -----------------------------------------------------

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


            /*
             * Guardamos referencias.
             */
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
                $"Achievement: {achievementIdentifier}"
            );
        }


        // =========================================================
        // ASIGNAR LOS UNLOCKS A LOS SURVIVORS
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
                 * Guardamos el UnlockableDef original
                 * una sola vez.
                 */
                if (
                    !OriginalUnlockables.ContainsKey(
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
                 * Si la misión personalizada está apagada,
                 * restauramos el unlock original.
                 */
                if (!RequiresCustomUnlock(entry))
                {
                    survivorDef.unlockableDef =
                        OriginalUnlockables[
                            survivorDef
                        ];

                    continue;
                }


                /*
                 * Buscamos nuestro UnlockableDef.
                 */
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


                /*
                 * Asignamos nuestro desbloqueo.
                 */
                survivorDef.unlockableDef =
                    customUnlock;


                /*
                 * Ahora que tenemos acceso al SurvivorDef
                 * completo, tomamos su portraitIcon y creamos
                 * el icono del achievement con marco.
                 */
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
        // CREAR ICONO DEL ACHIEVEMENT DESDE EL PORTRAIT
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


            /*
             * Si todavía no generamos el sprite,
             * lo creamos.
             */
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


            /*
             * Icono del UnlockableDef.
             */
            unlockable.achievementIcon =
                icon;


            /*
             * Icono del AchievementDef.
             */
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
        // CREAR COPIA DEL RETRATO EN MEMORIA
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


            /*
             * Creamos una RenderTexture temporal.
             *
             * De esta forma podemos copiar incluso texturas
             * que no tengan Read/Write habilitado.
             */
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
                /*
                 * Copiamos el retrato original.
                 */
                Graphics.Blit(
                    source,
                    temporary
                );


                RenderTexture.active =
                    temporary;


                /*
                 * Creamos nuestra propia Texture2D.
                 *
                 * Esta textura sólo existe en memoria.
                 */
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
                 * Dibujamos el recuadro.
                 */
                DrawAchievementFrame(
                    texture
                );


                /*
                 * Convertimos la textura a Sprite.
                 */
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
                /*
                 * Restauramos el RenderTexture anterior
                 * y liberamos el temporal.
                 */
                RenderTexture.active =
                    previous;


                RenderTexture.ReleaseTemporary(
                    temporary
                );
            }
        }


        // =========================================================
        // DIBUJAR MARCO DEL ACHIEVEMENT
        // =========================================================

        private static void DrawAchievementFrame(
            Texture2D texture
        )
        {
            int width =
                texture.width;

            int height =
                texture.height;


            /*
             * El grosor se adapta al tamaño del retrato.
             *
             * 128x128 -> aproximadamente 2-3 px
             * 256x256 -> aproximadamente 4-5 px
             */
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


            /*
             * Parte principal del marco.
             *
             * Es un azul/gris parecido al usado
             * por los achievements vanilla.
             */
            Color32 frameColor =
                new Color32(
                    73,
                    99,
                    124,
                    255
                );


            /*
             * Borde exterior oscuro.
             */
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


            /*
             * Primero dibujamos el borde exterior.
             */
            DrawTextureBorder(
                texture,
                outerColor,
                borderThickness +
                outerThickness
            );


            /*
             * Después dibujamos el borde principal.
             */
            DrawTextureBorder(
                texture,
                frameColor,
                borderThickness
            );


            texture.Apply();
        }


        // =========================================================
        // DIBUJAR UN BORDE RECTANGULAR
        // =========================================================

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
        // SABER SI EL JSON PIDE NUESTRO DESBLOQUEO
        // =========================================================

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


        // =========================================================
        // OBTENER SURVIVOR DESDE EL JSON
        // =========================================================

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


        // =========================================================
        // NOMBRE DE LA MISIÓN
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


            return "Desafío personalizado";
        }


        // =========================================================
        // DESCRIPCIÓN DE LA MISIÓN
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
                {
                    return
                        $"Completa la misión " +
                        $"\"{entry.Challenge.Type}\".";
                }
            }
        }


        // =========================================================
        // LEER ENTEROS DEL JSON
        // =========================================================

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


        // =========================================================
        // CONVERTIR NOMBRE A TOKEN
        // =========================================================

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