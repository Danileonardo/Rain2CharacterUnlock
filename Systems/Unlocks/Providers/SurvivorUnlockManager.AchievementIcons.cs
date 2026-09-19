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
    }
}
