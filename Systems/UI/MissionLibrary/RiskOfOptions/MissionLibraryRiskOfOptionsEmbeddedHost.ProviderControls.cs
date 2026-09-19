using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniversalSurvivorUnlocks
{
    internal sealed partial class MissionLibraryRiskOfOptionsEmbeddedHost
    {
        private void CreateSurvivorProviderControls(
            GameObject rowTemplate
        )
        {
            if (
                survivorDetailsRoot == null ||
                rowTemplate == null
            )
            {
                return;
            }

            survivorProviderControlsRoot =
                new GameObject(
                    "ProviderControls",
                    typeof(RectTransform),
                    typeof(HorizontalLayoutGroup)
                );

            survivorProviderControlsRoot.transform.SetParent(
                survivorDetailsRoot.transform,
                false
            );

            RectTransform rootRect =
                survivorProviderControlsRoot.GetComponent<RectTransform>();

            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.offsetMin = new Vector2(16f, -196f);
            rootRect.offsetMax = new Vector2(-16f, -156f);

            HorizontalLayoutGroup layout =
                survivorProviderControlsRoot.GetComponent<HorizontalLayoutGroup>();

            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            survivorProviderOriginalButton =
                CreateProviderButton(
                    survivorProviderControlsRoot.transform,
                    "Original",
                    rowTemplate,
                    out survivorProviderOriginalText
                );

            survivorProviderUsuButton =
                CreateProviderButton(
                    survivorProviderControlsRoot.transform,
                    "USU",
                    rowTemplate,
                    out survivorProviderUsuText
                );

            survivorProviderCustomButton =
                CreateProviderButton(
                    survivorProviderControlsRoot.transform,
                    "Custom",
                    rowTemplate,
                    out survivorProviderCustomText
                );
        }


        private static Button CreateProviderButton(
            Transform parent,
            string objectName,
            GameObject rowTemplate,
            out HGTextMeshProUGUI label
        )
        {
            label = null;

            GameObject buttonObject =
                new GameObject(
                    "Provider " + objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(Button),
                    typeof(LayoutElement)
                );

            buttonObject.transform.SetParent(
                parent,
                false
            );

            RectTransform rect =
                buttonObject.GetComponent<RectTransform>();

            rect.sizeDelta = new Vector2(112f, 34f);

            LayoutElement element =
                buttonObject.GetComponent<LayoutElement>();

            element.minWidth = 112f;
            element.preferredWidth = 112f;
            element.flexibleWidth = 0f;
            element.minHeight = 34f;
            element.preferredHeight = 34f;
            element.flexibleHeight = 0f;

            Image background =
                buttonObject.GetComponent<Image>();

            background.color =
                new Color(0.10f, 0.18f, 0.24f, 0.96f);

            Button button =
                buttonObject.GetComponent<Button>();

            button.targetGraphic = background;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.72f);
            button.colors = colors;

            GameObject textObject =
                new GameObject(
                    "Label",
                    typeof(RectTransform),
                    typeof(CanvasRenderer)
                );

            textObject.transform.SetParent(
                buttonObject.transform,
                false
            );

            RectTransform textRect =
                textObject.GetComponent<RectTransform>();

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6f, 2f);
            textRect.offsetMax = new Vector2(-6f, -2f);

            label =
                textObject.AddComponent<HGTextMeshProUGUI>();

            CopyTextStyleFromRow(
                rowTemplate,
                label
            );

            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.enableAutoSizing = true;
            label.fontSizeMin = 10f;
            label.fontSizeMax = 15f;
            label.raycastTarget = false;

            return button;
        }


        private void UpdateSurvivorProviderControls(
            SurvivorContentProfile profile
        )
        {
            if (
                profile == null ||
                survivorProviderControlsRoot == null ||
                survivorProviderOriginalButton == null ||
                survivorProviderUsuButton == null ||
                survivorProviderCustomButton == null
            )
            {
                return;
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    profile.BodyName
                );

            if (entry == null)
            {
                survivorProviderControlsRoot.SetActive(false);
                return;
            }

            survivorProviderControlsRoot.SetActive(true);

            SurvivorContentUnlockSource contentSource =
                profile.SurvivorUnlock?.Source
                ?? SurvivorContentUnlockSource.Unknown;

            MissionProviderSelectionState selectionState =
                MissionProviderSelectionPolicy.ResolveFromContentProfile(
                    entry,
                    contentSource
                );

            UnlockProviderKind activeProvider =
                selectionState.ActiveProvider;

            bool originalAvailable =
                selectionState.OriginalAvailable;

            bool usuAvailable =
                selectionState.CanSelectUsuImmediately ||
                selectionState.IsUsuActive;

            survivorProviderOriginalButton.interactable =
                originalAvailable &&
                activeProvider != UnlockProviderKind.Original;

            survivorProviderUsuButton.interactable =
                usuAvailable &&
                !UnlockProviderPolicy.IsUsuFamily(
                    activeProvider
                );

            // CUSTOM sigue reservado para 5H. Se muestra para que la jerarquía
            // de providers sea visible, pero aún no se puede seleccionar.
            survivorProviderCustomButton.interactable = false;

            if (survivorProviderOriginalText != null)
            {
                survivorProviderOriginalText.text =
                    (activeProvider == UnlockProviderKind.Original ? "● " : "○ ") +
                    GetProviderLabel(UnlockProviderKind.Original);
            }

            if (survivorProviderUsuText != null)
            {
                bool usuActive =
                    UnlockProviderPolicy.IsUsuFamily(
                        activeProvider
                    );

                survivorProviderUsuText.text =
                    (usuActive ? "● " : "○ ") +
                    GetProviderLabel(UnlockProviderKind.USU);
            }

            if (survivorProviderCustomText != null)
            {
                survivorProviderCustomText.text =
                    (activeProvider == UnlockProviderKind.Custom ? "● " : "○ ") +
                    GetProviderLabel(UnlockProviderKind.Custom);
            }

            survivorProviderOriginalButton.onClick.RemoveAllListeners();
            survivorProviderUsuButton.onClick.RemoveAllListeners();

            SurvivorContentProfile capturedProfile = profile;

            survivorProviderOriginalButton.onClick.AddListener(
                delegate
                {
                    ApplyEmbeddedProviderSelection(
                        capturedProfile,
                        UnlockProviderKind.Original
                    );
                }
            );

            survivorProviderUsuButton.onClick.AddListener(
                delegate
                {
                    ApplyEmbeddedProviderSelection(
                        capturedProfile,
                        UnlockProviderKind.USU
                    );
                }
            );
        }


        private void ApplyEmbeddedProviderSelection(
            SurvivorContentProfile profile,
            UnlockProviderKind provider
        )
        {
            if (
                profile == null ||
                string.IsNullOrWhiteSpace(profile.BodyName)
            )
            {
                return;
            }

            MissionAssignmentResult result =
                MissionAssignmentService.SelectProvider(
                    profile.BodyName,
                    provider,
                    logger
                );

            if (result == null)
            {
                logger?.LogWarning(
                    $"[MISSION LIBRARY UI] No existe una selección de " +
                    $"provider aplicable para {profile.BodyName}."
                );

                UpdateSurvivorDetailsPanel(profile);
                return;
            }

            if (!result.Success)
            {
                logger?.LogWarning(
                    $"[MISSION LIBRARY UI] Cambio de provider rechazado | " +
                    $"Body: {profile.BodyName} | Provider: {provider} | " +
                    $"Code: {result.Code} | {result.Message}"
                );
            }
            else
            {
                logger?.LogInfo(
                    $"[MISSION LIBRARY UI] Provider cambiado desde UI embebida | " +
                    $"Body: {profile.BodyName} | Provider: {provider}"
                );
            }

            UpdateSurvivorDetailsPanel(profile);
        }


    }
}
