using System;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Datos de una notificación opcional. No representan un logro ni
    /// modifican el estado persistente de una misión.
    /// </summary>
    internal sealed class MissionMedalNotification
    {
        internal string Category;
        internal string Title;
        internal string Description;
        internal string Symbol;
        internal Color Accent;
        internal float DurationSeconds;
    }

    /// <summary>
    /// Interfaz de medallas reutilizable para futuras misiones.
    /// Solo vive en el cliente local y no requiere prefabs externos.
    /// Posición: inferior-centro, por encima de la barra de salud.
    /// </summary>
    internal static class MissionMedalNotificationManager
    {
        private static MissionMedalNotificationView view;

        internal static void Show(MissionMedalNotification medal)
        {
            if (medal == null) return;

            if (view == null)
            {
                var gameObject = new GameObject(
                    "USU Mission Medals Overlay",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler));
                UnityEngine.Object.DontDestroyOnLoad(gameObject);

                Canvas canvas = gameObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.overrideSorting = true;
                canvas.sortingOrder = 30000;
                canvas.pixelPerfect = false;

                CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                view = gameObject.AddComponent<MissionMedalNotificationView>();
            }

            view.Present(medal);
        }
    }

    /// <summary>
    /// Componente visual autónomo. Usa tiempo no escalado para continuar
    /// visible al finalizar la partida, incluso si el HUD es destruido.
    /// </summary>
    public sealed class MissionMedalNotificationView : MonoBehaviour
    {
        private const float BottomOffset = 205f;
        private const float EntryDelay = 0.65f;
        private const float FadeIn = 0.30f;
        private const float FadeOut = 0.55f;

        private GameObject card;
        private CanvasGroup group;
        private RectTransform cardRect;
        private Image accentStrip;
        private Image seal;
        private HGTextMeshProUGUI category;
        private HGTextMeshProUGUI title;
        private HGTextMeshProUGUI description;
        private HGTextMeshProUGUI symbol;
        private float elapsed;
        private float life;
        private bool playing;

        internal void Present(MissionMedalNotification medal)
        {
            if (card == null) BuildCard();

            category.text = medal.Category ?? string.Empty;
            title.text = medal.Title ?? string.Empty;
            description.text = medal.Description ?? string.Empty;
            symbol.text = medal.Symbol ?? "★";
            accentStrip.color = medal.Accent;
            seal.color = new Color(medal.Accent.r * 0.24f,
                medal.Accent.g * 0.24f, medal.Accent.b * 0.24f, 0.98f);
            symbol.color = medal.Accent;
            elapsed = 0f;
            life = Mathf.Max(2.0f, medal.DurationSeconds);
            playing = true;
            card.SetActive(false);
            group.alpha = 0f;
        }

        private void Update()
        {
            if (!playing || card == null) return;

            elapsed += Time.unscaledDeltaTime;
            if (elapsed < EntryDelay) return;
            if (!card.activeSelf) card.SetActive(true);

            float presentationTime = elapsed - EntryDelay;
            if (presentationTime > life)
            {
                playing = false;
                card.SetActive(false);
                return;
            }

            float fadeIn = Mathf.Clamp01(presentationTime / FadeIn);
            float fadeOut = Mathf.Clamp01((life - presentationTime) / FadeOut);
            float visibility = Mathf.Min(fadeIn, fadeOut);
            group.alpha = visibility;
            cardRect.anchoredPosition = new Vector2(0f,
                BottomOffset - (1f - fadeIn) * 28f);
        }

        private void BuildCard()
        {
            card = new GameObject("USU Medal Card",
                typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(CanvasGroup));
            card.transform.SetParent(transform, false);
            cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0f);
            cardRect.anchorMax = new Vector2(0.5f, 0f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(625f, 108f);
            cardRect.anchoredPosition = new Vector2(0f, BottomOffset);

            Image panel = card.GetComponent<Image>();
            panel.color = new Color(0.045f, 0.075f, 0.105f, 0.95f);
            panel.raycastTarget = false;

            group = card.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0f;

            accentStrip = AddImage("Accent", card.transform,
                new Vector2(-309f, 0f), new Vector2(7f, 108f));
            seal = AddImage("Medal Seal", card.transform,
                new Vector2(-259f, 0f), new Vector2(71f, 71f));

            symbol = AddText("Medal Symbol", card.transform,
                new Vector2(-259f, 0f), new Vector2(68f, 68f), 36f,
                TextAlignmentOptions.Center, FontStyles.Bold);
            category = AddText("Medal Category", card.transform,
                new Vector2(49f, 29f), new Vector2(480f, 21f), 16f,
                TextAlignmentOptions.Left, FontStyles.Normal);
            category.color = new Color(0.74f, 0.81f, 0.85f, 1f);
            title = AddText("Medal Title", card.transform,
                new Vector2(49f, 2f), new Vector2(480f, 34f), 25f,
                TextAlignmentOptions.Left, FontStyles.Bold);
            title.color = Color.white;
            description = AddText("Medal Description", card.transform,
                new Vector2(49f, -31f), new Vector2(480f, 28f), 17f,
                TextAlignmentOptions.Left, FontStyles.Normal);
            description.color = new Color(0.87f, 0.90f, 0.92f, 1f);

            card.SetActive(false);
        }

        private static Image AddImage(string name, Transform parent,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private static HGTextMeshProUGUI AddText(string name,
            Transform parent, Vector2 position, Vector2 size,
            float textSize, TextAlignmentOptions align, FontStyles style)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            HGTextMeshProUGUI result = go.AddComponent<HGTextMeshProUGUI>();
            result.fontSize = textSize;
            result.fontStyle = style;
            result.alignment = align;
            result.enableAutoSizing = false;
            result.enableWordWrapping = false;
            result.overflowMode = TextOverflowModes.Ellipsis;
            result.raycastTarget = false;
            return result;
        }
    }
}
