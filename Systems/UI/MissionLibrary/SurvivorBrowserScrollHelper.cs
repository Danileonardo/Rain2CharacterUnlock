using UnityEngine;
using UnityEngine.UI;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Mantiene la fila seleccionada completamente dentro del viewport del
    /// ScrollRect. Sólo desplaza la cantidad mínima necesaria; nunca centra
    /// la selección de forma forzada.
    /// </summary>
    internal static class SurvivorBrowserScrollHelper
    {
        public static void EnsureVisible(
            RectTransform row,
            Transform rowsParent
        )
        {
            if (row == null || rowsParent == null)
            {
                return;
            }

            ScrollRect scrollRect =
                rowsParent.GetComponentInParent<ScrollRect>();

            if (scrollRect == null)
            {
                return;
            }

            RectTransform viewport =
                scrollRect.viewport != null
                    ? scrollRect.viewport
                    : scrollRect.transform as RectTransform;

            RectTransform content = scrollRect.content;

            if (viewport == null || content == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            Bounds bounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(
                    viewport,
                    row
                );

            Rect viewRect = viewport.rect;
            const float edgePadding = 3f;
            float topEdge = viewRect.yMax - edgePadding;
            float bottomEdge = viewRect.yMin + edgePadding;
            float correction = 0f;

            if (bounds.max.y > topEdge)
            {
                // La fila sobresale por arriba: mover contenido hacia abajo.
                correction = -(bounds.max.y - topEdge);
            }
            else if (bounds.min.y < bottomEdge)
            {
                // La fila sobresale por abajo: mover contenido hacia arriba.
                correction = bottomEdge - bounds.min.y;
            }

            if (Mathf.Abs(correction) < 0.01f)
            {
                return;
            }

            scrollRect.StopMovement();
            scrollRect.velocity = Vector2.zero;

            Vector2 position = content.anchoredPosition;
            position.y += correction;
            content.anchoredPosition = position;

            Canvas.ForceUpdateCanvases();
        }
    }
}
