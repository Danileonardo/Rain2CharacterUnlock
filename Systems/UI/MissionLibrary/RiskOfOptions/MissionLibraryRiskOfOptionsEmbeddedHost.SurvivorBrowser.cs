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
        private void BuildSurvivorBrowserRows(
            PageState state,
            Transform parent
        )
        {
            if (
                state == null ||
                state.MarkerObject == null ||
                parent == null
            )
            {
                return;
            }

            IReadOnlyList<SurvivorContentProfile> sourceProfiles =
                SurvivorContentProfileService.GetAll();

            // La UI NO debe reconstruir los profiles cada vez que se abre
            // Personajes. El catálogo ya los construye durante startup.
            // Sólo hacemos un refresh de emergencia si el snapshot aún está vacío.
            if (
                (sourceProfiles == null || sourceProfiles.Count == 0) &&
                !survivorDiscoveryRefreshedForUiSession
            )
            {
                try
                {
                    SurvivorContentProfileService.RefreshDiscovery();
                    survivorDiscoveryRefreshedForUiSession = true;
                    sourceProfiles =
                        SurvivorContentProfileService.GetAll();
                }
                catch
                {
                    // Si todavía no está listo, seguimos con el snapshot actual.
                }
            }

            List<SurvivorContentProfile> profiles =
                new List<SurvivorContentProfile>();

            if (sourceProfiles != null)
            {
                for (int i = 0; i < sourceProfiles.Count; i++)
                {
                    SurvivorContentProfile profile =
                        sourceProfiles[i];

                    if (
                        profile == null ||
                        profile.SurvivorDef == null ||
                        string.IsNullOrWhiteSpace(profile.BodyName)
                    )
                    {
                        continue;
                    }

                    profiles.Add(profile);
                }
            }

            SurvivorBrowserOrderResolver.Sort(profiles);

            survivorBrowserRows.Clear();

            int siblingIndex =
                state.MarkerObject.transform.GetSiblingIndex() + 1;

            for (int i = 0; i < profiles.Count; i++)
            {
                SurvivorContentProfile profile =
                    profiles[i];

                GameObject row =
                    CreateSurvivorBrowserRow(
                        state.MarkerObject,
                        parent,
                        profile,
                        i
                    );

                if (row == null)
                {
                    continue;
                }

                row.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
                state.GeneratedRows.Add(row);

                RectTransform rowRect =
                    row.transform as RectTransform;

                if (
                    rowRect != null &&
                    !string.IsNullOrWhiteSpace(profile.BodyName)
                )
                {
                    survivorBrowserRows[profile.BodyName] = rowRect;
                }
            }

            HideNativeAnchor(state.MarkerObject);

            SurvivorContentProfile currentSelection =
                selectedSurvivorProfile != null
                    ? FindProfileByBodyName(
                        profiles,
                        selectedSurvivorProfile.BodyName
                    )
                    : null;

            selectedSurvivorProfile =
                currentSelection
                ?? ChooseInitialSurvivorProfile(profiles);

            if (selectedSurvivorProfile != null)
            {
                ShowSurvivorDetails(
                    selectedSurvivorProfile,
                    parent,
                    state.MarkerObject
                );
            }
            else
            {
                DestroySurvivorDetailsPanel();
            }

            if (!state.Logged)
            {
                state.Logged = true;

                logger?.LogInfo(
                    "[MISSION LIBRARY UI] 5G.2E-B navegador de personajes creado | " +
                    "Personajes: " + profiles.Count +
                    " | Modo: Embedded | Ventana externa: no"
                );
            }
        }


        private GameObject CreateSurvivorBrowserRow(
            GameObject markerTemplate,
            Transform parent,
            SurvivorContentProfile profile,
            int rowIndex
        )
        {
            if (
                markerTemplate == null ||
                parent == null ||
                profile == null
            )
            {
                return null;
            }

            GameObject clone =
                UnityEngine.Object.Instantiate(
                    markerTemplate,
                    parent,
                    false
                );

            clone.name =
                "USU Survivor Browser Row " + rowIndex + " " +
                profile.BodyName;

            clone.SetActive(true);

            CanvasGroup canvasGroup =
                clone.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = clone.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            LayoutElement layoutElement =
                clone.GetComponent<LayoutElement>();

            if (layoutElement == null)
            {
                layoutElement = clone.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = false;
            layoutElement.minWidth = 0f;
            layoutElement.preferredWidth = -1f;
            layoutElement.flexibleWidth = 1f;
            layoutElement.minHeight = 68f;
            layoutElement.preferredHeight = 68f;

            RectTransform rowRect =
                clone.transform as RectTransform;

            if (rowRect != null)
            {
                rowRect.localScale = Vector3.one;

                Vector2 size = rowRect.sizeDelta;
                size.x = 0f;
                rowRect.sizeDelta = size;
            }

            DisableOptionBehaviours(clone);

            string label =
                GetSurvivorBrowserLabel(profile);

            string action =
                L(
                    SurvivorLocalization
                        .MissionLibraryBrowserViewToken
                );

            ApplyRowText(
                clone,
                label,
                action
            );

            AddSurvivorPortraitThumbnail(
                clone,
                profile
            );

            PrepareButtonsForSurvivorSelection(
                clone,
                profile
            );

            return clone;
        }


        private void PrepareButtonsForSurvivorSelection(
            GameObject root,
            SurvivorContentProfile profile
        )
        {
            if (
                root == null ||
                profile == null
            )
            {
                return;
            }

            Button[] buttons =
                root.GetComponentsInChildren<Button>(true);

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];

                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                button.interactable = true;

                ColorBlock colors = button.colors;
                colors.disabledColor = colors.normalColor;
                button.colors = colors;

                if (button.targetGraphic != null)
                {
                    button.targetGraphic.raycastTarget = true;
                }

                if (button.gameObject != root)
                {
                    LayoutElement buttonLayout =
                        button.GetComponent<LayoutElement>();

                    if (buttonLayout == null)
                    {
                        buttonLayout =
                            button.gameObject.AddComponent<LayoutElement>();
                    }

                    buttonLayout.minWidth = 86f;
                    buttonLayout.preferredWidth = 86f;
                    buttonLayout.flexibleWidth = 0f;
                }

                SurvivorContentProfile capturedProfile = profile;

                button.onClick.AddListener(
                    delegate
                    {
                        OnSurvivorSelected(capturedProfile);
                    }
                );
            }
        }


        private void OnSurvivorSelected(
            SurvivorContentProfile profile
        )
        {
            if (profile == null)
            {
                return;
            }

            selectedSurvivorProfile = profile;

            if (
                activeState == null ||
                activeState.Page != EmbeddedPage.Survivors ||
                activeState.MarkerObject == null ||
                activeState.MarkerObject.transform.parent == null
            )
            {
                return;
            }

            ShowSurvivorDetails(
                profile,
                activeState.MarkerObject.transform.parent,
                activeState.MarkerObject
            );

            if (
                !string.IsNullOrWhiteSpace(profile.BodyName) &&
                survivorBrowserRows.TryGetValue(
                    profile.BodyName,
                    out RectTransform selectedRow
                )
            )
            {
                SurvivorBrowserScrollHelper.EnsureVisible(
                    selectedRow,
                    activeState.MarkerObject.transform.parent
                );
            }
        }


        private static string GetSurvivorBrowserLabel(
            SurvivorContentProfile profile
        )
        {
            if (profile == null)
            {
                return "";
            }

            if (profile.ShouldHideIdentity)
            {
                return "???";
            }

            if (!string.IsNullOrWhiteSpace(profile.DisplayName))
            {
                return profile.DisplayName;
            }

            if (!string.IsNullOrWhiteSpace(profile.InternalName))
            {
                return profile.InternalName;
            }

            return profile.BodyName ?? "";
        }


        private static void AddSurvivorPortraitThumbnail(
            GameObject row,
            SurvivorContentProfile profile
        )
        {
            if (
                row == null ||
                profile == null
            )
            {
                return;
            }

            HGTextMeshProUGUI[] texts =
                row.GetComponentsInChildren<HGTextMeshProUGUI>(true);

            if (texts != null && texts.Length > 0)
            {
                int leftIndex;
                int rightIndex;

                FindHorizontalExtremes(
                    texts,
                    out leftIndex,
                    out rightIndex
                );

                HGTextMeshProUGUI leftText =
                    texts[leftIndex];

                if (leftText != null)
                {
                    Vector4 margin = leftText.margin;
                    margin.x += 72f;
                    leftText.margin = margin;
                }
            }

            GameObject frameObject =
                new GameObject(
                    "USU Survivor Portrait Frame",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

            frameObject.transform.SetParent(
                row.transform,
                false
            );

            RectTransform frameRect =
                frameObject.GetComponent<RectTransform>();

            frameRect.anchorMin =
                new Vector2(0f, 0.5f);
            frameRect.anchorMax =
                new Vector2(0f, 0.5f);
            frameRect.pivot =
                new Vector2(0f, 0.5f);
            frameRect.sizeDelta =
                new Vector2(58f, 58f);
            frameRect.anchoredPosition =
                new Vector2(7f, 0f);

            Image frame =
                frameObject.GetComponent<Image>();

            frame.color =
                new Color(0.12f, 0.18f, 0.23f, 0.96f);
            frame.raycastTarget = false;

            GameObject portraitObject =
                new GameObject(
                    "USU Survivor Portrait",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage)
                );

            portraitObject.transform.SetParent(
                frameObject.transform,
                false
            );

            RectTransform portraitRect =
                portraitObject.GetComponent<RectTransform>();

            portraitRect.anchorMin = Vector2.zero;
            portraitRect.anchorMax = Vector2.one;
            portraitRect.offsetMin = new Vector2(2f, 2f);
            portraitRect.offsetMax = new Vector2(-2f, -2f);

            RawImage portrait =
                portraitObject.GetComponent<RawImage>();

            portrait.raycastTarget = false;

            SurvivorLockedPortraitManager.ApplyBrowserPortrait(
                portrait,
                profile
            );

            if (profile.Portrait != null)
            {
                return;
            }

            GameObject questionObject =
                new GameObject(
                    "USU Survivor Portrait Question",
                    typeof(RectTransform),
                    typeof(CanvasRenderer)
                );

            questionObject.transform.SetParent(
                portraitObject.transform,
                false
            );

            RectTransform questionRect =
                questionObject.GetComponent<RectTransform>();

            questionRect.anchorMin = Vector2.zero;
            questionRect.anchorMax = Vector2.one;
            questionRect.offsetMin = Vector2.zero;
            questionRect.offsetMax = Vector2.zero;

            HGTextMeshProUGUI question =
                questionObject.AddComponent<HGTextMeshProUGUI>();

            CopyTextStyleFromRow(row, question);

            question.text = "?";
            question.alignment = TextAlignmentOptions.Center;
            question.fontSize = 26f;
            question.enableAutoSizing = true;
            question.fontSizeMin = 14f;
            question.fontSizeMax = 26f;
            question.raycastTarget = false;
        }


    }
}
