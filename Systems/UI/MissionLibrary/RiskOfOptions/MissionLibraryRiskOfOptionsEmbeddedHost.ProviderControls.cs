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
            rootRect.offsetMax = new Vector2(-64f, -156f);

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

            survivorProviderGeneratedButton =
                CreateProviderButton(
                    survivorProviderControlsRoot.transform,
                    "Generated",
                    rowTemplate,
                    out survivorProviderGeneratedText
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

            rect.sizeDelta = new Vector2(104f, 34f);

            LayoutElement element =
                buttonObject.GetComponent<LayoutElement>();

            element.minWidth = 104f;
            element.preferredWidth = 104f;
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
                survivorProviderCustomButton == null ||
                survivorProviderGeneratedButton == null
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

            bool generatedActive =
                selectionState.IsGeneratedActive;

            bool customActive =
                selectionState.IsCustomActive;

            bool usuActive =
                selectionState.IsUsuActive;

            survivorProviderOriginalButton.interactable =
                originalAvailable &&
                activeProvider != UnlockProviderKind.Original;

            survivorProviderUsuButton.interactable =
                selectionState.UsuTargetAvailable &&
                !usuActive;

            // 0.3.1: el editor completo sigue en 5H, pero una Custom ya
            // persistida sí puede reactivarse desde la fila de providers.
            survivorProviderCustomButton.interactable =
                selectionState.CustomAvailable &&
                !customActive;

            bool moddedGeneratedProvider =
                profile.SurvivorInfo != null &&
                profile.SurvivorInfo.IsModded;

            // Generated permanece accesible para cualquier survivor modded.
            // Si aún no existe slot, pulsarlo crea una misión aleatoria y la
            // selecciona; no hace falta pre-generarla al iniciar el juego.
            survivorProviderGeneratedButton.interactable =
                moddedGeneratedProvider &&
                !generatedActive;

            if (survivorProviderOriginalText != null)
            {
                survivorProviderOriginalText.text =
                    (activeProvider == UnlockProviderKind.Original ? "● " : "○ ") +
                    GetProviderLabel(UnlockProviderKind.Original);
            }

            if (survivorProviderUsuText != null)
            {
                survivorProviderUsuText.text =
                    (usuActive ? "● " : "○ ") +
                    GetProviderLabel(UnlockProviderKind.USU);
            }

            if (survivorProviderCustomText != null)
            {
                survivorProviderCustomText.text =
                    (customActive ? "● " : "○ ") +
                    GetProviderLabel(UnlockProviderKind.Custom);
            }

            if (survivorProviderGeneratedText != null)
            {
                survivorProviderGeneratedText.text =
                    (generatedActive ? "● " : "○ ") +
                    L(
                        SurvivorLocalization
                            .MissionLibraryProviderGeneratedToken
                    );
            }

            survivorProviderOriginalButton.onClick.RemoveAllListeners();
            survivorProviderUsuButton.onClick.RemoveAllListeners();
            survivorProviderCustomButton.onClick.RemoveAllListeners();
            survivorProviderGeneratedButton.onClick.RemoveAllListeners();

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

            survivorProviderCustomButton.onClick.AddListener(
                delegate
                {
                    ApplyEmbeddedProviderSelection(
                        capturedProfile,
                        UnlockProviderKind.Custom
                    );
                }
            );

            survivorProviderGeneratedButton.onClick.AddListener(
                delegate
                {
                    ApplyEmbeddedGeneratedSelection(
                        capturedProfile
                    );
                }
            );
        }


        private static bool IsGeneratedProviderActive(
            SurvivorContentProfile profile
        )
        {
            if (profile == null ||
                string.IsNullOrWhiteSpace(profile.BodyName))
            {
                return false;
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(profile.BodyName);

            if (entry == null)
            {
                return false;
            }

            SurvivorContentUnlockSource contentSource =
                profile.SurvivorUnlock?.Source
                    ?? SurvivorContentUnlockSource.Unknown;

            MissionProviderSelectionState selectionState =
                MissionProviderSelectionPolicy.ResolveFromContentProfile(
                    entry,
                    contentSource
                );

            return selectionState != null &&
                selectionState.IsGeneratedActive;
        }


        private void UpdateGeneratedRerollControl(
            SurvivorContentProfile profile
        )
        {
            if (
                survivorGeneratedRerollRoot == null ||
                survivorGeneratedRerollButton == null
            )
            {
                return;
            }

            // La misma politica de visibilidad gobierna el dado y la X.
            // La existencia de una Generated dormida no activa sus acciones.
            bool isGeneratedActive =
                IsGeneratedProviderActive(profile);

            survivorGeneratedRerollRoot.SetActive(
                isGeneratedActive
            );

            if (!isGeneratedActive)
            {
                survivorGeneratedRerollButton.onClick.RemoveAllListeners();
                return;
            }

            // Una misión no debe cambiar en mitad de una run. En menú/lobby
            // sí puede rerollearse; si somos host el snapshot se refresca.
            survivorGeneratedRerollButton.interactable =
                Run.instance == null;

            survivorGeneratedRerollButton.onClick.RemoveAllListeners();

            SurvivorContentProfile capturedProfile = profile;

            survivorGeneratedRerollButton.onClick.AddListener(
                delegate
                {
                    RerollGeneratedMissionFromUi(
                        capturedProfile
                    );
                }
            );
        }


        private void UpdateGeneratedDeleteControl(
            SurvivorContentProfile profile
        )
        {
            if (
                survivorGeneratedDeleteRoot == null ||
                survivorGeneratedDeleteButton == null
            )
            {
                return;
            }

            // Debe obedecer la misma seleccion que el dado: solo cuando
            // el provider activo es Generated y hay un slot eliminable.
            bool canDelete =
                IsGeneratedProviderActive(profile) &&
                GeneratedMissionDeletionService.CanDelete(profile.BodyName);

            survivorGeneratedDeleteRoot.SetActive(canDelete);
            survivorGeneratedDeleteButton.onClick.RemoveAllListeners();

            if (!canDelete)
            {
                return;
            }

            survivorGeneratedDeleteButton.interactable =
                Run.instance == null;

            SurvivorContentProfile capturedProfile = profile;

            survivorGeneratedDeleteButton.onClick.AddListener(
                delegate
                {
                    DeleteGeneratedMissionFromUi(capturedProfile);
                }
            );
        }


        private void DeleteGeneratedMissionFromUi(
            SurvivorContentProfile profile
        )
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.BodyName))
            {
                return;
            }

            if (survivorGeneratedDeleteButton != null)
            {
                survivorGeneratedDeleteButton.interactable = false;
            }

            GeneratedMissionDeletionService.DeleteResult result =
                GeneratedMissionDeletionService.Delete(
                    profile.BodyName,
                    logger
                );

            if (result == null || !result.Success)
            {
                logger?.LogWarning(
                    "[MISSION LIBRARY UI] Borrado Generated rechazado | " +
                    $"Body: {profile.BodyName} | " +
                    $"Code: {result?.Code ?? "null"} | " +
                    (result?.Message ?? "Sin resultado")
                );
            }
            else
            {
                logger?.LogInfo(
                    "[MISSION LIBRARY UI] Misión Generated eliminada | " +
                    $"Body: {profile.BodyName}"
                );
            }

            UpdateSurvivorDetailsPanel(profile);
        }


        private void RerollGeneratedMissionFromUi(
            SurvivorContentProfile profile
        )
        {
            if (
                profile == null ||
                string.IsNullOrWhiteSpace(profile.BodyName)
            )
            {
                return;
            }

            if (survivorGeneratedRerollButton != null)
            {
                survivorGeneratedRerollButton.interactable = false;
            }

            GeneratedMissionRerollService.RerollResult result =
                GeneratedMissionRerollService.Reroll(
                    profile.BodyName,
                    logger
                );

            if (result == null || !result.Success)
            {
                logger?.LogWarning(
                    "[MISSION LIBRARY UI] Reroll Generated rechazado | " +
                    $"Body: {profile.BodyName} | " +
                    $"Code: {result?.Code ?? "null"} | " +
                    $"{result?.Message ?? "Sin resultado"}"
                );
            }
            else
            {
                logger?.LogInfo(
                    "[MISSION LIBRARY UI] Generated rerolleada | " +
                    $"Body: {profile.BodyName} | " +
                    $"{result.OldName} -> {result.NewName}"
                );
            }

            // Vuelve a resolver desde Survivors.json: nombre y descripción de
            // la misión cambian en el mismo panel sin cerrarlo ni reiniciar.
            UpdateSurvivorDetailsPanel(profile);
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


        private void ApplyEmbeddedGeneratedSelection(
            SurvivorContentProfile profile
        )
        {
            if (
                profile == null ||
                string.IsNullOrWhiteSpace(profile.BodyName)
            )
            {
                return;
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(profile.BodyName);

            MissionProviderSelectionState state =
                entry == null
                    ? null
                    : MissionProviderSelectionPolicy.ResolveFromStoredEntry(entry);

            if (state == null || !state.GeneratedAvailable)
            {
                GeneratedMissionManualGenerationService.GenerateResult generated =
                    GeneratedMissionManualGenerationService.GenerateAndSelect(
                        profile.BodyName,
                        logger
                    );

                if (generated == null || !generated.Success)
                {
                    logger?.LogWarning(
                        $"[MISSION LIBRARY UI] Generación manual rechazada | " +
                        $"Body: {profile.BodyName} | " +
                        $"Code: {generated?.Code ?? "null"} | " +
                        $"{generated?.Message ?? "Sin resultado"}"
                    );
                }
                else
                {
                    logger?.LogInfo(
                        $"[MISSION LIBRARY UI] Generated creada desde UI embebida | " +
                        $"Body: {profile.BodyName}"
                    );
                }

                UpdateSurvivorDetailsPanel(profile);
                return;
            }

            MissionAssignmentResult result =
                MissionAssignmentService.SelectGenerated(
                    profile.BodyName,
                    logger
                );

            if (result == null)
            {
                logger?.LogWarning(
                    $"[MISSION LIBRARY UI] No existe una selección Generated aplicable para {profile.BodyName}."
                );

                UpdateSurvivorDetailsPanel(profile);
                return;
            }

            if (!result.Success)
            {
                logger?.LogWarning(
                    $"[MISSION LIBRARY UI] Cambio a Generated rechazado | " +
                    $"Body: {profile.BodyName} | Code: {result.Code} | {result.Message}"
                );
            }
            else
            {
                logger?.LogInfo(
                    $"[MISSION LIBRARY UI] Generated activada desde UI embebida | " +
                    $"Body: {profile.BodyName}"
                );
            }

            UpdateSurvivorDetailsPanel(profile);
        }
    }
}
