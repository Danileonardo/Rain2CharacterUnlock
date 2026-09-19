using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración de Enforcer / Defensor.
    ///
    /// Enforcer 3.11.9 ya publica localización propia completa para
    /// es-419 y es-ES. USU conserva íntegramente la localización del
    /// creador y sólo migra la autoridad del personaje al sistema de
    /// Definitions.
    ///
    /// Reparación visual puntual de "Motín": el icono original del
    /// achievement no utiliza bien el espacio disponible. En lugar de
    /// recortar o ampliar ese sprite, USU toma el portraitIcon real de
    /// EnforcerBody —el retrato que el personaje aporta para Character
    /// Select— y lo adapta al mismo formato de achievement que ya usa
    /// USU para los portraits de survivors. No genera artwork nuevo ni
    /// modifica Enforcer.dll.
    ///
    /// NemesisEnforcerBody NO forma parte de esta Definition.
    /// </summary>
    public sealed class EnforcerDefinition : SurvivorDefinition
    {
        private const string CharacterUnlockAchievementId =
            "ENFORCER_CHARACTERUNLOCKABLE_ACHIEVEMENT_ID";

        private const string CharacterUnlockRewardIdentifier =
            "ENFORCER_CHARACTERUNLOCKABLE_REWARD_ID";

        private static Sprite characterUnlockPortraitIcon;

        public override string SourceIdentifier
        {
            get { return "Enforcer.EnforcerContent"; }
        }

        public override string BodyName
        {
            get { return "EnforcerBody"; }
        }

        public override string DefinitionName
        {
            get { return "Enforcer"; }
        }

        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            // Enforcer ya tiene localización oficial propia.
            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | EnforcerBody | " +
                "localización nativa del creador (es-419 / es-ES) + lore + skills + keywords + skins + achievements"
            );

            RepairCharacterUnlockAchievementIcon(
                survivor,
                logger
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public override string ResolveOriginalMissionName(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public override string ResolveOriginalMissionDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        // =========================================================
        // REPARACIÓN VISUAL DEL ACHIEVEMENT "MOTÍN"
        // =========================================================

        private static void RepairCharacterUnlockAchievementIcon(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            try
            {
                if (
                    survivor == null ||
                    survivor.SurvivorDef == null ||
                    survivor.SurvivorDef.bodyPrefab == null
                )
                {
                    logger?.LogWarning(
                        "[ENFORCER VISUAL] Motín | SurvivorDef/bodyPrefab no disponible; se conserva el icono original."
                    );

                    return;
                }

                CharacterBody body =
                    survivor
                        .SurvivorDef
                        .bodyPrefab
                        .GetComponent<CharacterBody>();

                Texture portrait =
                    body != null
                        ? body.portraitIcon
                        : null;

                if (portrait == null)
                {
                    logger?.LogWarning(
                        "[ENFORCER VISUAL] Motín | EnforcerBody no tiene portraitIcon disponible; se conserva el icono original."
                    );

                    return;
                }

                if (characterUnlockPortraitIcon == null)
                {
                    characterUnlockPortraitIcon =
                        CreateAchievementIconFromSurvivorPortrait(
                            portrait,
                            logger
                        );
                }

                if (characterUnlockPortraitIcon == null)
                {
                    logger?.LogWarning(
                        "[ENFORCER VISUAL] Motín | no se pudo convertir el portraitIcon de EnforcerBody; se conserva el icono original."
                    );

                    return;
                }

                AchievementDef achievement =
                    FindAchievement(
                        CharacterUnlockAchievementId
                    );

                UnlockableDef unlockable =
                    FindUnlockable(
                        CharacterUnlockRewardIdentifier
                    );

                bool achievementPatched = false;
                bool unlockablePatched = false;

                if (achievement != null)
                {
                    achievement.achievedIcon =
                        characterUnlockPortraitIcon;

                    achievementPatched = true;
                }

                if (unlockable != null)
                {
                    unlockable.achievementIcon =
                        characterUnlockPortraitIcon;

                    unlockablePatched = true;
                }

                logger?.LogInfo(
                    "[ENFORCER VISUAL] Motín | portraitIcon de EnforcerBody (Character Select) aplicado sin crop/zoom | " +
                    "AchievementDef: " + achievementPatched +
                    " | UnlockableDef: " + unlockablePatched + "."
                );
            }
            catch (Exception exception)
            {
                logger?.LogWarning(
                    "[ENFORCER VISUAL] No se pudo aplicar el portraitIcon de Character Select a Motín. " +
                    exception.GetType().Name +
                    ": " +
                    exception.Message
                );
            }
        }

        /// <summary>
        /// Reutiliza exactamente el helper con el que USU adapta los
        /// portraitIcon de survivors a iconos de achievement. Se invoca
        /// por reflexión porque el helper continúa encapsulado dentro de
        /// SurvivorUnlockManager; si en una versión futura cambia, existe
        /// un fallback que copia el portrait completo sin recortarlo.
        /// </summary>
        private static Sprite CreateAchievementIconFromSurvivorPortrait(
            Texture portrait,
            ManualLogSource logger
        )
        {
            if (portrait == null)
            {
                return null;
            }

            try
            {
                MethodInfo helper =
                    typeof(SurvivorUnlockManager)
                        .GetMethod(
                            "CreateFramedAchievementIcon",
                            BindingFlags.Static |
                            BindingFlags.NonPublic
                        );

                if (helper != null)
                {
                    Sprite result =
                        helper.Invoke(
                            null,
                            new object[]
                            {
                                portrait,
                                "EnforcerBody"
                            }
                        ) as Sprite;

                    if (result != null)
                    {
                        result.name =
                            "USU_Enforcer_Mutiny_CharacterSelectPortrait";

                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                logger?.LogWarning(
                    "[ENFORCER VISUAL] No se pudo reutilizar CreateFramedAchievementIcon; se usará copia directa del portrait. " +
                    exception.GetType().Name +
                    ": " +
                    exception.Message
                );
            }

            return CreateDirectPortraitSprite(
                portrait
            );
        }

        /// <summary>
        /// Fallback: copia TODO el portraitIcon a un Sprite, sin crop ni
        /// zoom. Sólo se usa si el helper estándar de USU no está disponible.
        /// </summary>
        private static Sprite CreateDirectPortraitSprite(
            Texture source
        )
        {
            if (
                source == null ||
                source.width <= 1 ||
                source.height <= 1
            )
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
                        0f,
                        0f,
                        source.width,
                        source.height
                    ),
                    0,
                    0
                );

                texture.Apply();

                texture.name =
                    "USU_Enforcer_Mutiny_CharacterSelectTexture";

                texture.wrapMode =
                    TextureWrapMode.Clamp;

                texture.filterMode =
                    FilterMode.Bilinear;

                Sprite result =
                    Sprite.Create(
                        texture,
                        new Rect(
                            0f,
                            0f,
                            texture.width,
                            texture.height
                        ),
                        new Vector2(
                            0.5f,
                            0.5f
                        ),
                        100f
                    );

                result.name =
                    "USU_Enforcer_Mutiny_CharacterSelectPortrait";

                return result;
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

        private static UnlockableDef FindUnlockable(
            string identifier
        )
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            object definitions =
                ReadStaticMember(
                    typeof(UnlockableCatalog),
                    "unlockableDefs"
                ) ??
                ReadStaticMember(
                    typeof(UnlockableCatalog),
                    "allUnlockableDefs"
                );

            IEnumerable enumerable =
                definitions as IEnumerable;

            if (enumerable == null)
            {
                return null;
            }

            foreach (object raw in enumerable)
            {
                UnlockableDef unlockable =
                    raw as UnlockableDef;

                if (
                    unlockable != null &&
                    string.Equals(
                        unlockable.cachedName,
                        identifier,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return unlockable;
                }
            }

            return null;
        }

        private static AchievementDef FindAchievement(
            string identifier
        )
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            object definitions =
                ReadStaticMember(
                    typeof(AchievementManager),
                    "achievementDefs"
                );

            IEnumerable enumerable =
                definitions as IEnumerable;

            if (enumerable == null)
            {
                return null;
            }

            foreach (object raw in enumerable)
            {
                AchievementDef achievement =
                    raw as AchievementDef;

                if (
                    achievement != null &&
                    string.Equals(
                        achievement.identifier,
                        identifier,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return achievement;
                }
            }

            return null;
        }

        private static object ReadStaticMember(
            Type type,
            string memberName
        )
        {
            if (type == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            BindingFlags flags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo field =
                type.GetField(
                    memberName,
                    flags
                );

            if (field != null)
            {
                return field.GetValue(null);
            }

            PropertyInfo property =
                type.GetProperty(
                    memberName,
                    flags
                );

            return property != null
                ? property.GetValue(null, null)
                : null;
        }
    }
}
