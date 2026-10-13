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
        private static void HideNativeAnchor(
            GameObject anchor
        )
        {
            if (anchor == null)
            {
                return;
            }

            CanvasGroup canvasGroup =
                anchor.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    anchor.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            LayoutElement layoutElement =
                anchor.GetComponent<LayoutElement>();

            if (layoutElement == null)
            {
                layoutElement =
                    anchor.AddComponent<LayoutElement>();
            }

            // ignoreLayout basta para sacar el ancla del VerticalLayout.
            // NO tocamos minHeight/preferredHeight/flexibleHeight porque esta
            // misma ancla se usa como prefab para clonar las filas visibles.
            // Si colapsamos esas métricas, las copias heredan altura 0.
            layoutElement.ignoreLayout = true;

            Graphic[] graphics =
                anchor.GetComponentsInChildren<Graphic>(
                    true
                );

            for (
                int i = 0;
                i < graphics.Length;
                i++
            )
            {
                if (graphics[i] != null)
                {
                    graphics[i].raycastTarget = false;
                }
            }
        }


        private static GameObject CreateDisplayRow(
            GameObject markerTemplate,
            Transform parent,
            DisplayRow data,
            EmbeddedPage page,
            int rowIndex
        )
        {
            if (
                markerTemplate == null ||
                parent == null ||
                data == null
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
                "USU Embedded " +
                page +
                " Row " +
                rowIndex;

            clone.SetActive(
                true
            );

            CanvasGroup canvasGroup =
                clone.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    clone.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;

            // IMPORTANTE: interactable=false en un CanvasGroup hace que
            // todos los Selectable hijos entren visualmente al estado
            // Disabled (el tono marrón/gris que vimos en 5G.2E-A).
            // Dejamos el grupo en estado normal, pero sin raycasts.
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = false;

            LayoutElement layoutElement =
                clone.GetComponent<LayoutElement>();

            if (layoutElement == null)
            {
                layoutElement =
                    clone.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = false;
            layoutElement.minWidth = 0f;
            layoutElement.preferredWidth = -1f;
            layoutElement.flexibleWidth = 1f;

            RectTransform rowRect =
                clone.transform as RectTransform;

            if (rowRect != null)
            {
                rowRect.localScale = Vector3.one;

                Vector2 size = rowRect.sizeDelta;
                size.x = 0f;
                rowRect.sizeDelta = size;
            }

            DisableOptionBehaviours(
                clone
            );

            PrepareButtonsForReadOnlyDisplay(
                clone,
                data.Value
            );

            ApplyRowText(
                clone,
                data.Label,
                data.Value
            );

            if (data.OnClick != null)
            {
                canvasGroup.blocksRaycasts = true;

                Button[] buttons = clone.GetComponentsInChildren<Button>(true);
                if (buttons != null && buttons.Length > 0 && buttons[0] != null)
                {
                    Button actionButton = buttons[0];
                    actionButton.interactable = true;
                    if (actionButton.targetGraphic != null)
                    {
                        actionButton.targetGraphic.raycastTarget = true;
                    }

                    Action capturedAction = data.OnClick;
                    actionButton.onClick.AddListener(
                        delegate
                        {
                            capturedAction();

                            // Refresca la etiqueta sin abandonar Avanzado.
                            // Los toggles modifican ConfigEntry<bool> real;
                            // el archivo .cfg se guarda desde UsuLog.
                            if (data.Label == "Diagnóstico de logs")
                            {
                                ApplyRowText(clone, data.Label,
                                    UsuLog.DiagnosticEnabled ? "ACTIVADO" : "DESACTIVADO");
                            }
                            else if (data.Label == "Logs detallados")
                            {
                                ApplyRowText(clone, data.Label,
                                    UsuLog.VerboseEnabled ? "ACTIVADO" : "DESACTIVADO");
                            }
                            else if (data.Label == "Auditorías extensas")
                            {
                                ApplyRowText(clone, data.Label,
                                    UsuLog.ExtendedAuditEnabled ? "ACTIVADO" : "DESACTIVADO");
                            }
                        }
                    );
                }
            }

            return clone;
        }


        private static void ConfigureAnchorRow(
            GameObject anchor,
            DisplayRow data
        )
        {
            if (
                anchor == null ||
                data == null
            )
            {
                return;
            }

            anchor.SetActive(
                true
            );

            CanvasGroup canvasGroup =
                anchor.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    anchor.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = false;

            LayoutElement layoutElement =
                anchor.GetComponent<LayoutElement>();

            if (layoutElement == null)
            {
                layoutElement =
                    anchor.AddComponent<LayoutElement>();
            }

            // El ancla participa normalmente en el VerticalLayout. Ya no es
            // un marcador invisible ni necesita colapsarse a altura cero.
            layoutElement.ignoreLayout = false;
            layoutElement.minWidth = 0f;
            layoutElement.preferredWidth = -1f;
            layoutElement.flexibleWidth = 1f;

            RectTransform rowRect =
                anchor.transform as RectTransform;

            if (rowRect != null)
            {
                rowRect.localScale = Vector3.one;

                Vector2 size = rowRect.sizeDelta;
                size.x = 0f;
                rowRect.sizeDelta = size;
            }

            DisableOptionBehaviours(
                anchor
            );

            PrepareButtonsForReadOnlyDisplay(
                anchor,
                data.Value
            );

            ApplyRowText(
                anchor,
                data.Label,
                data.Value
            );
        }


        private static void DisableOptionBehaviours(
            GameObject root
        )
        {
            if (root == null)
            {
                return;
            }

            MonoBehaviour[] behaviours =
                root.GetComponentsInChildren<MonoBehaviour>(
                    true
                );

            for (
                int i = 0;
                i < behaviours.Length;
                i++
            )
            {
                MonoBehaviour behaviour =
                    behaviours[i];

                if (behaviour == null)
                {
                    continue;
                }

                Type type =
                    behaviour.GetType();

                string fullName =
                    type.FullName ?? "";

                if (
                    fullName.StartsWith(
                        "RiskOfOptions.",
                        StringComparison.Ordinal
                    ) ||
                    string.Equals(
                        fullName,
                        "RoR2.UI.LanguageTextMeshController",
                        StringComparison.Ordinal
                    )
                )
                {
                    behaviour.enabled = false;
                }
            }
        }


        private static void DisableButtons(
            GameObject root
        )
        {
            if (root == null)
            {
                return;
            }

            Button[] buttons =
                root.GetComponentsInChildren<Button>(
                    true
                );

            for (
                int i = 0;
                i < buttons.Length;
                i++
            )
            {
                if (buttons[i] == null)
                {
                    continue;
                }

                buttons[i].onClick.RemoveAllListeners();
                buttons[i].interactable = false;
            }
        }


        /// <summary>
        /// Deja las filas generadas en la paleta NORMAL de RiskOfOptions,
        /// aunque sean informativas/no clicables.
        ///
        /// En 5G.2E-A usábamos Button.interactable=false junto con un
        /// CanvasGroup no interactuable. Unity aplicaba disabledColor y por
        /// eso las filas se veían marrón/gris. Este FIX hace que disabledColor
        /// sea igual a normalColor y elimina navegación/raycasts.
        /// </summary>
        private static void PrepareButtonsForReadOnlyDisplay(
            GameObject root,
            string value
        )
        {
            if (root == null)
            {
                return;
            }

            Button[] buttons =
                root.GetComponentsInChildren<Button>(
                    true
                );

            for (
                int i = 0;
                i < buttons.Length;
                i++
            )
            {
                Button button = buttons[i];

                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();

                Navigation navigation = button.navigation;
                navigation.mode = Navigation.Mode.None;
                button.navigation = navigation;

                ColorBlock colors = button.colors;
                colors.disabledColor = colors.normalColor;
                button.colors = colors;

                if (button.targetGraphic != null)
                {
                    button.targetGraphic.color =
                        colors.normalColor;

                    button.targetGraphic.raycastTarget = false;
                }

                // Ajusta el área derecha a textos reales como
                // "Risk of Options" o "Desactivada", evitando el recorte
                // que tenía el botón de valor de ancho mínimo.
                if (button.gameObject != root)
                {
                    LayoutElement buttonLayout =
                        button.GetComponent<LayoutElement>();

                    if (buttonLayout == null)
                    {
                        buttonLayout =
                            button.gameObject.AddComponent<LayoutElement>();
                    }

                    float preferredWidth =
                        CalculateValueWidth(value);

                    buttonLayout.minWidth = preferredWidth;
                    buttonLayout.preferredWidth = preferredWidth;
                    buttonLayout.flexibleWidth = 0f;
                }

                // Conservamos el color normal aunque el control no acepte
                // interacción.
                button.interactable = false;
            }
        }


        private static float CalculateValueWidth(
            string value
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 72f;
            }

            float width =
                36f + (value.Length * 8.5f);

            return Mathf.Clamp(
                width,
                72f,
                205f
            );
        }


        private static void ApplyRowText(
            GameObject root,
            string label,
            string value
        )
        {
            if (root == null)
            {
                return;
            }

            HGTextMeshProUGUI[] texts =
                root.GetComponentsInChildren<HGTextMeshProUGUI>(
                    true
                );

            if (
                texts != null &&
                texts.Length > 0
            )
            {
                ApplyHgText(
                    texts,
                    label,
                    value
                );

                return;
            }

            // Fallback por compatibilidad si RiskOfOptions cambia el prefab
            // a un Text clásico de Unity.
            Text[] legacyTexts =
                root.GetComponentsInChildren<Text>(
                    true
                );

            if (
                legacyTexts == null ||
                legacyTexts.Length == 0
            )
            {
                return;
            }

            for (
                int i = 0;
                i < legacyTexts.Length;
                i++
            )
            {
                if (legacyTexts[i] != null)
                {
                    legacyTexts[i].text = "";
                }
            }

            if (legacyTexts.Length == 1)
            {
                legacyTexts[0].text =
                    string.IsNullOrWhiteSpace(value)
                        ? label
                        : label + ": " + value;

                return;
            }

            int leftIndex;
            int rightIndex;

            FindHorizontalExtremes(
                legacyTexts,
                out leftIndex,
                out rightIndex
            );

            legacyTexts[leftIndex].text =
                label;

            legacyTexts[rightIndex].text =
                value;
        }


        private static void ApplyHgText(
            HGTextMeshProUGUI[] texts,
            string label,
            string value
        )
        {
            for (
                int i = 0;
                i < texts.Length;
                i++
            )
            {
                if (texts[i] != null)
                {
                    texts[i].text = "";
                }
            }

            for (
                int i = 0;
                i < texts.Length;
                i++
            )
            {
                HGTextMeshProUGUI text = texts[i];

                if (text == null)
                {
                    continue;
                }

                text.enableAutoSizing = true;
                text.fontSizeMin = 11f;
                text.fontSizeMax = Mathf.Max(
                    11f,
                    text.fontSize
                );
                text.enableWordWrapping = false;
                text.overflowMode = TextOverflowModes.Ellipsis;
                text.raycastTarget = false;
            }

            if (texts.Length == 1)
            {
                texts[0].text =
                    string.IsNullOrWhiteSpace(value)
                        ? label
                        : label + ": " + value;

                return;
            }

            int leftIndex;
            int rightIndex;

            FindHorizontalExtremes(
                texts,
                out leftIndex,
                out rightIndex
            );

            texts[leftIndex].text =
                label;

            texts[rightIndex].text =
                value;
        }


        private static void FindHorizontalExtremes(
            HGTextMeshProUGUI[] texts,
            out int leftIndex,
            out int rightIndex
        )
        {
            leftIndex = 0;
            rightIndex = texts.Length - 1;

            float leftX = float.PositiveInfinity;
            float rightX = float.NegativeInfinity;

            for (
                int i = 0;
                i < texts.Length;
                i++
            )
            {
                if (
                    texts[i] == null ||
                    texts[i].rectTransform == null
                )
                {
                    continue;
                }

                float x =
                    texts[i]
                        .rectTransform
                        .anchoredPosition
                        .x;

                if (x < leftX)
                {
                    leftX = x;
                    leftIndex = i;
                }

                if (x > rightX)
                {
                    rightX = x;
                    rightIndex = i;
                }
            }

            if (
                leftIndex == rightIndex &&
                texts.Length > 1
            )
            {
                leftIndex = 0;
                rightIndex = texts.Length - 1;
            }
        }


        private static void FindHorizontalExtremes(
            Text[] texts,
            out int leftIndex,
            out int rightIndex
        )
        {
            leftIndex = 0;
            rightIndex = texts.Length - 1;

            float leftX = float.PositiveInfinity;
            float rightX = float.NegativeInfinity;

            for (
                int i = 0;
                i < texts.Length;
                i++
            )
            {
                if (
                    texts[i] == null ||
                    texts[i].rectTransform == null
                )
                {
                    continue;
                }

                float x =
                    texts[i]
                        .rectTransform
                        .anchoredPosition
                        .x;

                if (x < leftX)
                {
                    leftX = x;
                    leftIndex = i;
                }

                if (x > rightX)
                {
                    rightX = x;
                    rightIndex = i;
                }
            }

            if (
                leftIndex == rightIndex &&
                texts.Length > 1
            )
            {
                leftIndex = 0;
                rightIndex = texts.Length - 1;
            }
        }


        // =========================================================
        // 5G.2E-B - NAVEGADOR VISUAL DE PERSONAJES
        // =========================================================

    }
}
