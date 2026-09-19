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
        private void ShowSurvivorDetails(
            SurvivorContentProfile profile,
            Transform rowsParent,
            GameObject rowTemplate
        )
        {
            if (
                profile == null ||
                rowsParent == null ||
                rowTemplate == null
            )
            {
                return;
            }

            RectTransform rightPanel =
                FindRightDetailsPanel(rowsParent);

            if (rightPanel == null)
            {
                logger?.LogWarning(
                    "[MISSION LIBRARY UI] 5G.2E-B no pudo localizar el panel derecho " +
                    "de RiskOfOptions para mostrar detalles del personaje."
                );

                return;
            }

            bool recreate =
                survivorDetailsRoot == null ||
                survivorDetailsRoot.transform.parent != rightPanel;

            if (recreate)
            {
                DestroySurvivorDetailsPanel();
                CreateSurvivorDetailsPanel(
                    rightPanel,
                    rowTemplate
                );
            }

            UpdateSurvivorDetailsPanel(profile);
        }


        private void CreateSurvivorDetailsPanel(
            RectTransform rightPanel,
            GameObject rowTemplate
        )
        {
            if (rightPanel == null)
            {
                return;
            }

            // 5G.2E-B.1-E:
            // El panel derecho NATIVO de RiskOfOptions es ahora el marco y
            // fondo autoritativo. USU sólo crea una capa de contenido
            // transparente dentro de él. No copiamos altura, no desplazamos
            // Y y no dibujamos un marco paralelo: así permanecemos dentro
            // del preset/layout original de Mod Options.
            survivorDetailsRoot =
                new GameObject(
                    "USU Embedded Survivor Details Content",
                    typeof(RectTransform),
                    typeof(CanvasGroup),
                    typeof(LayoutElement)
                );

            survivorDetailsRoot.transform.SetParent(
                rightPanel,
                false
            );

            LayoutElement rootLayout =
                survivorDetailsRoot.GetComponent<LayoutElement>();

            // Si el host usa un LayoutGroup, no debe recolocar nuestra capa.
            // El RectTransform se estira exactamente al rect nativo.
            rootLayout.ignoreLayout = true;

            RectTransform rootRect =
                survivorDetailsRoot.GetComponent<RectTransform>();

            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;

            // Posición validada por el usuario: mover TODO el contenido
            // exactamente 9 px hacia abajo sin alterar su altura.
            rootRect.offsetMin = new Vector2(0f, -9f);
            rootRect.offsetMax = new Vector2(0f, -9f);
            rootRect.localScale = Vector3.one;

            // Fondo oscuro separado del root de contenido.
            // Se pinta DENTRO del panel nativo y deja 3 px libres en todo
            // el perímetro para que el borde ORIGINAL de Risk of Options
            // permanezca visible. No dibujamos un borde USU.
            survivorDetailsBackground =
                new GameObject(
                    "USU Embedded Survivor Details Background",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(LayoutElement)
                );

            survivorDetailsBackground.transform.SetParent(
                rightPanel,
                false
            );

            LayoutElement backgroundLayout =
                survivorDetailsBackground.GetComponent<LayoutElement>();
            backgroundLayout.ignoreLayout = true;

            RectTransform backgroundRect =
                survivorDetailsBackground.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            // El alto/bajo (-4f / -9f) fue ajustado manualmente y está
            // VALIDADO: NO modificar esos valores. Sólo metemos el fondo
            // 3 px por izquierda/derecha para dejar visible el borde nativo.
            backgroundRect.offsetMin = new Vector2(5f, -4f);
            backgroundRect.offsetMax = new Vector2(-5f, -9f);
            backgroundRect.localScale = Vector3.one;

            Image detailsBackground =
                survivorDetailsBackground.GetComponent<Image>();
            detailsBackground.color =
                new Color(0.025f, 0.055f, 0.075f, 1f);
            detailsBackground.raycastTarget = false;

            // El fondo debe cubrir los hijos visuales nativos del panel,
            // pero el root de contenido debe quedar por encima. El borde
            // original del Image padre sigue visible gracias al inset.
            survivorDetailsBackground.transform.SetAsLastSibling();
            survivorDetailsRoot.transform.SetAsLastSibling();

            CanvasGroup group =
                survivorDetailsRoot.GetComponent<CanvasGroup>();

            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;

            GameObject portraitFrameObject =
                new GameObject(
                    "PortraitFrame",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

            portraitFrameObject.transform.SetParent(
                survivorDetailsRoot.transform,
                false
            );

            RectTransform portraitFrameRect =
                portraitFrameObject.GetComponent<RectTransform>();

            portraitFrameRect.anchorMin = new Vector2(0f, 1f);
            portraitFrameRect.anchorMax = new Vector2(0f, 1f);
            portraitFrameRect.pivot = new Vector2(0f, 1f);
            portraitFrameRect.sizeDelta = new Vector2(132f, 132f);
            portraitFrameRect.anchoredPosition = new Vector2(16f, -16f);

            Image portraitFrame =
                portraitFrameObject.GetComponent<Image>();

            portraitFrame.color =
                new Color(0.12f, 0.18f, 0.23f, 0.98f);
            portraitFrame.raycastTarget = false;

            GameObject portraitObject =
                new GameObject(
                    "Portrait",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage)
                );

            portraitObject.transform.SetParent(
                portraitFrameObject.transform,
                false
            );

            RectTransform portraitRect =
                portraitObject.GetComponent<RectTransform>();

            portraitRect.anchorMin = Vector2.zero;
            portraitRect.anchorMax = Vector2.one;
            portraitRect.offsetMin = new Vector2(3f, 3f);
            portraitRect.offsetMax = new Vector2(-3f, -3f);

            survivorDetailsPortrait =
                portraitObject.GetComponent<RawImage>();

            survivorDetailsPortrait.raycastTarget = false;

            GameObject questionObject =
                new GameObject(
                    "PortraitQuestion",
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

            survivorDetailsQuestion =
                questionObject.AddComponent<HGTextMeshProUGUI>();

            CopyTextStyleFromRow(
                rowTemplate,
                survivorDetailsQuestion
            );

            survivorDetailsQuestion.alignment =
                TextAlignmentOptions.Center;
            survivorDetailsQuestion.fontSize = 54f;
            survivorDetailsQuestion.enableAutoSizing = true;
            survivorDetailsQuestion.fontSizeMin = 24f;
            survivorDetailsQuestion.fontSizeMax = 54f;
            survivorDetailsQuestion.raycastTarget = false;

            GameObject headerObject =
                new GameObject(
                    "HeaderText",
                    typeof(RectTransform),
                    typeof(CanvasRenderer)
                );

            headerObject.transform.SetParent(
                survivorDetailsRoot.transform,
                false
            );

            RectTransform headerRect =
                headerObject.GetComponent<RectTransform>();

            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.offsetMin = new Vector2(164f, -148f);
            headerRect.offsetMax = new Vector2(-18f, -18f);

            survivorDetailsHeaderText =
                headerObject.AddComponent<HGTextMeshProUGUI>();

            CopyTextStyleFromRow(
                rowTemplate,
                survivorDetailsHeaderText
            );

            survivorDetailsHeaderText.alignment =
                TextAlignmentOptions.TopLeft;
            survivorDetailsHeaderText.enableWordWrapping = true;
            survivorDetailsHeaderText.overflowMode =
                TextOverflowModes.Ellipsis;
            // El header combina jerarquías mediante tags <size>. Evitamos
            // auto-size para que una Fuente larga no reduzca también el
            // nombre del personaje.
            survivorDetailsHeaderText.enableAutoSizing = false;
            survivorDetailsHeaderText.fontSize = 16f;
            survivorDetailsHeaderText.raycastTarget = false;

            CreateSurvivorProviderControls(
                rowTemplate
            );

            GameObject scrollObject =
                new GameObject(
                    "DetailsScroll",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(RectMask2D),
                    typeof(ScrollRect)
                );

            scrollObject.transform.SetParent(
                survivorDetailsRoot.transform,
                false
            );

            RectTransform scrollRectTransform =
                scrollObject.GetComponent<RectTransform>();

            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = new Vector2(16f, 14f);
            // Portrait: y -16 .. -148. El selector de provider ocupa la fila
            // inmediatamente inferior. El fondo/borde aprobado NO se toca.
            scrollRectTransform.offsetMax = new Vector2(-16f, -204f);

            Image scrollBackground =
                scrollObject.GetComponent<Image>();

            // Transparente: el fondo liso sin borde pertenece al root de USU.
            // Este Image existe sólo para que el ScrollRect reciba raycasts.
            scrollBackground.color = new Color(1f, 1f, 1f, 0f);
            scrollBackground.raycastTarget = true;

            survivorDetailsScrollRect =
                scrollObject.GetComponent<ScrollRect>();

            survivorDetailsScrollRect.horizontal = false;
            survivorDetailsScrollRect.vertical = true;
            survivorDetailsScrollRect.movementType =
                ScrollRect.MovementType.Clamped;
            survivorDetailsScrollRect.inertia = true;
            survivorDetailsScrollRect.scrollSensitivity = 28f;
            survivorDetailsScrollRect.viewport = scrollRectTransform;

            GameObject textObject =
                new GameObject(
                    "DetailsText",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(ContentSizeFitter)
                );

            textObject.transform.SetParent(
                scrollObject.transform,
                false
            );

            RectTransform textRect =
                textObject.GetComponent<RectTransform>();

            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(0f, -8f);
            textRect.sizeDelta = new Vector2(-24f, 0f);

            survivorDetailsText =
                textObject.AddComponent<HGTextMeshProUGUI>();

            CopyTextStyleFromRow(
                rowTemplate,
                survivorDetailsText
            );

            survivorDetailsText.alignment =
                TextAlignmentOptions.TopLeft;
            survivorDetailsText.enableWordWrapping = true;
            survivorDetailsText.overflowMode =
                TextOverflowModes.Overflow;
            survivorDetailsText.enableAutoSizing = false;
            survivorDetailsText.fontSize = 16f;
            survivorDetailsText.raycastTarget = false;
            survivorDetailsText.margin = new Vector4(0f, 8f, 0f, 12f);

            ContentSizeFitter fitter =
                textObject.GetComponent<ContentSizeFitter>();

            fitter.horizontalFit =
                ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            survivorDetailsScrollRect.content = textRect;

            // El contenido queda por encima del fondo interno. El borde
            // visible sigue siendo el original del panel de RiskOfOptions.
            survivorDetailsRoot.transform.SetAsLastSibling();

            // Sin LogInfo aquí: Risk of Options puede recrear este panel
            // durante su ciclo de vida y ese mensaje terminaba siendo spam.
        }


        private static void CopyTextStyleFromRow(
            GameObject row,
            HGTextMeshProUGUI destination
        )
        {
            if (destination == null)
            {
                return;
            }

            HGTextMeshProUGUI source = null;

            if (row != null)
            {
                HGTextMeshProUGUI[] candidates =
                    row.GetComponentsInChildren<HGTextMeshProUGUI>(true);

                for (int i = 0; i < candidates.Length; i++)
                {
                    if (
                        candidates[i] != null &&
                        candidates[i] != destination &&
                        candidates[i].font != null
                    )
                    {
                        source = candidates[i];
                        break;
                    }
                }
            }

            if (source != null)
            {
                destination.font = source.font;
                destination.fontSharedMaterial =
                    source.fontSharedMaterial;
                destination.color = source.color;
                destination.fontStyle = source.fontStyle;
            }

            destination.richText = true;
        }


        private static RectTransform FindBrowserViewportReference(
            Transform rowsParent
        )
        {
            if (rowsParent == null)
            {
                return null;
            }

            RectMask2D mask =
                rowsParent.GetComponentInParent<RectMask2D>();

            if (mask != null)
            {
                return mask.transform as RectTransform;
            }

            return rowsParent as RectTransform;
        }

        private RectTransform FindRightDetailsPanel(
            Transform rowsParent
        )
        {
            if (rowsParent == null)
            {
                return null;
            }

            RectTransform reference =
                FindBrowserViewportReference(rowsParent);

            if (reference == null)
            {
                return null;
            }

            float refMinX;
            float refMaxX;
            float refMinY;
            float refMaxY;

            GetWorldBounds(
                reference,
                out refMinX,
                out refMaxX,
                out refMinY,
                out refMaxY
            );

            float refWidth =
                Mathf.Max(1f, refMaxX - refMinX);
            float refHeight =
                Mathf.Max(1f, refMaxY - refMinY);
            float refCenterX =
                (refMinX + refMaxX) * 0.5f;

            Transform branch = reference;

            for (int depth = 0; depth < 8; depth++)
            {
                Transform ancestor = branch.parent;

                if (ancestor == null)
                {
                    break;
                }

                RectTransform best = null;
                float bestScore = float.PositiveInfinity;

                for (int i = 0; i < ancestor.childCount; i++)
                {
                    Transform child = ancestor.GetChild(i);

                    if (
                        child == branch ||
                        child == null ||
                        !child.gameObject.activeInHierarchy
                    )
                    {
                        continue;
                    }

                    RectTransform candidate =
                        child as RectTransform;

                    if (candidate == null)
                    {
                        continue;
                    }

                    float minX;
                    float maxX;
                    float minY;
                    float maxY;

                    GetWorldBounds(
                        candidate,
                        out minX,
                        out maxX,
                        out minY,
                        out maxY
                    );

                    float width = maxX - minX;
                    float height = maxY - minY;
                    float centerX = (minX + maxX) * 0.5f;

                    if (
                        centerX <= refCenterX + refWidth * 0.35f ||
                        width < refWidth * 0.55f ||
                        height < refHeight * 0.55f
                    )
                    {
                        continue;
                    }

                    float verticalOverlap =
                        Mathf.Min(maxY, refMaxY) -
                        Mathf.Max(minY, refMinY);

                    if (verticalOverlap < refHeight * 0.35f)
                    {
                        continue;
                    }

                    float score =
                        Mathf.Abs(width - refWidth) +
                        Mathf.Abs(height - refHeight) +
                        Mathf.Abs(centerX - refCenterX);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = candidate;
                    }
                }

                if (best != null)
                {
                    return best;
                }

                branch = ancestor;
            }

            Canvas canvas =
                rowsParent.GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                return null;
            }

            RectTransform[] candidates =
                canvas.GetComponentsInChildren<RectTransform>(true);

            RectTransform fallback = null;
            float fallbackScore = float.PositiveInfinity;

            for (int i = 0; i < candidates.Length; i++)
            {
                RectTransform candidate = candidates[i];

                if (
                    candidate == null ||
                    candidate == reference ||
                    !candidate.gameObject.activeInHierarchy ||
                    reference.IsChildOf(candidate) ||
                    candidate.IsChildOf(reference)
                )
                {
                    continue;
                }

                float minX;
                float maxX;
                float minY;
                float maxY;

                GetWorldBounds(
                    candidate,
                    out minX,
                    out maxX,
                    out minY,
                    out maxY
                );

                float width = maxX - minX;
                float height = maxY - minY;
                float centerX = (minX + maxX) * 0.5f;

                if (
                    centerX <= refCenterX + refWidth * 0.35f ||
                    width < refWidth * 0.65f ||
                    height < refHeight * 0.65f ||
                    width > refWidth * 1.8f ||
                    height > refHeight * 1.8f
                )
                {
                    continue;
                }

                float score =
                    Mathf.Abs(width - refWidth) +
                    Mathf.Abs(height - refHeight) +
                    Mathf.Abs(centerX - refCenterX);

                if (score < fallbackScore)
                {
                    fallbackScore = score;
                    fallback = candidate;
                }
            }

            return fallback;
        }


        private static void GetWorldBounds(
            RectTransform rect,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY
        )
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            minX = corners[0].x;
            maxX = corners[0].x;
            minY = corners[0].y;
            maxY = corners[0].y;

            for (int i = 1; i < corners.Length; i++)
            {
                minX = Mathf.Min(minX, corners[i].x);
                maxX = Mathf.Max(maxX, corners[i].x);
                minY = Mathf.Min(minY, corners[i].y);
                maxY = Mathf.Max(maxY, corners[i].y);
            }
        }


        private void DestroySurvivorDetailsPanel()
        {
            if (survivorDetailsRoot != null)
            {
                UnityEngine.Object.Destroy(
                    survivorDetailsRoot
                );
            }

            if (survivorDetailsBackground != null)
            {
                UnityEngine.Object.Destroy(
                    survivorDetailsBackground
                );
            }

            survivorDetailsBackground = null;
            survivorDetailsRoot = null;
            survivorDetailsPortrait = null;
            survivorDetailsQuestion = null;
            survivorDetailsHeaderText = null;
            survivorDetailsText = null;
            survivorDetailsScrollRect = null;
            survivorProviderControlsRoot = null;
            survivorProviderOriginalButton = null;
            survivorProviderUsuButton = null;
            survivorProviderCustomButton = null;
            survivorProviderOriginalText = null;
            survivorProviderUsuText = null;
            survivorProviderCustomText = null;
        }


    }
}
