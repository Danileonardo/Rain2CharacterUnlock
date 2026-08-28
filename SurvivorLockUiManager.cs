using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorLockUiManager
    {
        private static ManualLogSource Logger;

        private static bool initialized;

        private const string LockedOverlayName =
            "USU_LockedOverlay";

        public static void Initialize(
            ManualLogSource logger
        )
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            Logger = logger;

            On.RoR2.UI.SurvivorIconController.UpdateAvailability +=
                SurvivorIconController_UpdateAvailability;

            On.RoR2.UI.SurvivorIconController.Rebuild +=
                SurvivorIconController_Rebuild;

            Logger.LogInfo(
                "SurvivorLockUiManager inicializado."
            );
        }

        private static void SurvivorIconController_UpdateAvailability(
            On.RoR2.UI.SurvivorIconController.orig_UpdateAvailability orig,
            SurvivorIconController self
        )
        {
            orig(self);

            if (!ShouldCustomLock(
                self.survivorDef
            ))
            {
                return;
            }

            self.survivorIsUnlocked = false;
        }

        private static void SurvivorIconController_Rebuild(
            On.RoR2.UI.SurvivorIconController.orig_Rebuild orig,
            SurvivorIconController self
        )
        {
            orig(self);

            if (!ShouldCustomLock(
                self.survivorDef
            ))
            {
                RemoveLockedOverlay(self);
                return;
            }

            self.survivorIsUnlocked = false;

            ApplyLockedUi(self);
        }

        private static bool ShouldCustomLock(
            SurvivorDef survivor
        )
        {
            if (survivor == null)
            {
                return false;
            }

            if (survivor.bodyPrefab == null)
            {
                return false;
            }

            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;

            if (config == null)
            {
                return false;
            }

            if (config.AvailableSurvivors == null)
            {
                return false;
            }

            string bodyName =
                survivor.bodyPrefab.name;

            if (!config.AvailableSurvivors.TryGetValue(
                bodyName,
                out SurvivorJsonEntry entry
            ))
            {
                return false;
            }

            if (entry == null)
            {
                return false;
            }

            if (entry.Challenge == null)
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
                ) ||
                entry.Challenge.Type == "Original"
            )
            {
                return false;
            }

            return true;
        }

        private static void ApplyLockedUi(
            SurvivorIconController self
        )
        {
            if (self == null)
            {
                return;
            }

            if (self.hgButton == null)
            {
                return;
            }

            SurvivorJsonEntry entry =
                GetEntry(
                    self.survivorDef
                );

            if (entry == null)
            {
                return;
            }

            // Bloqueamos el click,
            // pero NO desactivamos el GameObject.
            // Así el mouse todavía puede detectarlo.
            self.hgButton.disableGamepadClick =
                true;

            self.hgButton.disablePointerClick =
                true;

            ApplyPortraitDarkening(self);

            CreateLockedOverlay(self);

            ApplyTooltip(
                self,
                entry
            );

            Logger.LogInfo(
                $"Bloqueo personalizado aplicado: " +
                $"{entry.DisplayName} | " +
                $"{entry.BodyName}"
            );
        }

        private static void ApplyPortraitDarkening(
            SurvivorIconController self
        )
        {
            if (
                self.survivorDef == null ||
                self.survivorDef.bodyPrefab == null
            )
            {
                return;
            }

            CharacterBody body =
                self.survivorDef
                    .bodyPrefab
                    .GetComponent<CharacterBody>();

            if (body == null)
            {
                return;
            }

            Texture portraitTexture =
                body.portraitIcon;

            if (portraitTexture == null)
            {
                return;
            }

            RawImage[] rawImages =
                self.hgButton
                    .GetComponentsInChildren<RawImage>(
                        true
                    );

            foreach (RawImage rawImage in rawImages)
            {
                if (rawImage == null)
                {
                    continue;
                }

                if (
                    rawImage.texture !=
                    portraitTexture
                )
                {
                    continue;
                }

                Color originalColor =
                    rawImage.color;

                rawImage.color =
                    new Color(
                        originalColor.r * 0.25f,
                        originalColor.g * 0.25f,
                        originalColor.b * 0.25f,
                        originalColor.a
                    );
            }
        }

        private static void CreateLockedOverlay(
            SurvivorIconController self
        )
        {
            Transform buttonTransform =
                self.hgButton.transform;

            Transform existing =
                buttonTransform.Find(
                    LockedOverlayName
                );

            if (existing != null)
            {
                existing.gameObject.SetActive(
                    true
                );

                return;
            }

            GameObject overlayObject =
                new GameObject(
                    LockedOverlayName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

            overlayObject.transform.SetParent(
                buttonTransform,
                false
            );

            RectTransform rect =
                overlayObject
                    .GetComponent<RectTransform>();

            rect.anchorMin =
                Vector2.zero;

            rect.anchorMax =
                Vector2.one;

            rect.offsetMin =
                Vector2.zero;

            rect.offsetMax =
                Vector2.zero;

            Image overlay =
                overlayObject
                    .GetComponent<Image>();

            overlay.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.55f
                );

            // Muy importante:
            // la capa oscura NO intercepta el mouse.
            overlay.raycastTarget =
                false;

            overlayObject
                .transform
                .SetAsLastSibling();
        }

        private static void RemoveLockedOverlay(
            SurvivorIconController self
        )
        {
            if (
                self == null ||
                self.hgButton == null
            )
            {
                return;
            }

            Transform overlay =
                self.hgButton
                    .transform
                    .Find(
                        LockedOverlayName
                    );

            if (overlay != null)
            {
                Object.Destroy(
                    overlay.gameObject
                );
            }
        }

        private static void ApplyTooltip(
            SurvivorIconController self,
            SurvivorJsonEntry entry
        )
        {
            TooltipProvider tooltip =
                self.hgButton
                    .GetComponent<TooltipProvider>();

            if (tooltip == null)
            {
                tooltip =
                    self.hgButton
                        .gameObject
                        .AddComponent<TooltipProvider>();
            }

            tooltip.enabled =
                true;

            tooltip.AllowTooltipOnNavigationSelect =
                true;

            TooltipContent content =
                new TooltipContent
                {
                    overrideTitleText =
                        $"{entry.DisplayName} - BLOQUEADO",

                    overrideBodyText =
                        BuildUnlockDescription(
                            entry
                        ),

                    titleColor =
                        Color.gray,

                    bodyColor =
                        Color.white
                };

            tooltip.SetContent(
                content
            );
        }

        private static SurvivorJsonEntry GetEntry(
            SurvivorDef survivor
        )
        {
            if (
                survivor == null ||
                survivor.bodyPrefab == null
            )
            {
                return null;
            }

            if (
                SurvivorJsonManager.CurrentConfig ==
                null
            )
            {
                return null;
            }

            if (
                SurvivorJsonManager
                    .CurrentConfig
                    .AvailableSurvivors ==
                null
            )
            {
                return null;
            }

            string bodyName =
                survivor.bodyPrefab.name;

            SurvivorJsonManager
                .CurrentConfig
                .AvailableSurvivors
                .TryGetValue(
                    bodyName,
                    out SurvivorJsonEntry entry
                );

            return entry;
        }

        private static string BuildUnlockDescription(
            SurvivorJsonEntry entry
        )
        {
            if (
                entry == null ||
                entry.Challenge == null
            )
            {
                return
                    "Cómo desbloquear:\n" +
                    "Completa su desafío.";
            }

            string type =
                entry.Challenge.Type;

            JObject parameters =
                entry.Challenge.Parameters;

            switch (type)
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
                        "Cómo desbloquear:\n" +
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
                        "Cómo desbloquear:\n" +
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
                        "Cómo desbloquear:\n" +
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
                        "Cómo desbloquear:\n" +
                        $"Alcanza la fase {stage}.";
                }

                default:
                {
                    return
                        "Cómo desbloquear:\n" +
                        $"Completa la misión '{type}'.";
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
                parameters[key];

            if (token == null)
            {
                return defaultValue;
            }

            if (
                token.Type !=
                JTokenType.Integer
            )
            {
                return defaultValue;
            }

            return token.Value<int>();
        }
    }
}