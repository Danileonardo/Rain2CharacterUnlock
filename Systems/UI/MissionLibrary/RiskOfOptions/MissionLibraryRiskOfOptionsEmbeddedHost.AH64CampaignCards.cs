using System;
using System.Globalization;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniversalSurvivorUnlocks
{
    // Campaña exclusiva de AH-64 y utilidades visuales del preset común USU.
    // La creación de tarjetas genéricas vive en SurvivorPresetCards.cs.
    internal sealed partial class MissionLibraryRiskOfOptionsEmbeddedHost
    {
        private RectTransform ah64CampaignContentRoot;
        private HGTextMeshProUGUI ah64CampaignIntro;
        private HGTextMeshProUGUI ah64CampaignNotes;
        private readonly Image[] ah64CampaignFills = new Image[4];
        private readonly HGTextMeshProUGUI[] ah64CampaignValues = new HGTextMeshProUGUI[4];
        private readonly HGTextMeshProUGUI[] ah64CampaignChecks = new HGTextMeshProUGUI[4];
        private HGTextMeshProUGUI ah64CampaignFinalTitle;
        private HGTextMeshProUGUI ah64CampaignFinalDescription;
        private HGTextMeshProUGUI ah64CampaignFinalCheck;
        private HGTextMeshProUGUI ah64CampaignFinalLock;
        private HGTextMeshProUGUI ah64CampaignHeading;
        private HGTextMeshProUGUI ah64CampaignHeadingHint;
        private HGTextMeshProUGUI ah64CampaignNotesHeading;
        private RectTransform survivorPresetHeaderFrame;
        private LayoutElement ah64CampaignMissionCardLayout;
        private LayoutElement ah64CampaignNotesCardLayout;
        private readonly HGTextMeshProUGUI[] ah64CampaignCardTitles = new HGTextMeshProUGUI[4];
        private readonly HGTextMeshProUGUI[] ah64CampaignCardDescriptions = new HGTextMeshProUGUI[4];
        private float ah64CampaignNextRefresh;

        private static readonly Color AH64CardBack = new Color(0.028f, 0.083f, 0.118f, 0.995f);
        private static readonly Color AH64PanelBorder = new Color(0.10f, 0.74f, 0.95f, 0.98f);
        private static readonly Color AH64BarCyan = new Color(0.27f, 0.87f, 0.96f, 1f);
        private static readonly Color AH64CheckGreen = new Color(0.35f, 0.94f, 0.52f, 1f);
        private static readonly Color AH64Muted = new Color(0.69f, 0.78f, 0.85f, 1f);
        private static readonly Color AH64DarkPanel = new Color(0.006f, 0.032f, 0.050f, 0.995f);
        private static readonly Color AH64RedBack = new Color(0.215f, 0.020f, 0.055f, 0.99f);
        private static readonly Color AH64RedBorder = new Color(1.0f, 0.18f, 0.34f, 1f);

        private static GameObject AH64UIObject(string name, Transform parent, bool image = false)
        {
            GameObject go = image
                ? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void AH64Rect(RectTransform rect, Vector2 min, Vector2 max,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private HGTextMeshProUGUI AH64Text(string name, Transform parent, GameObject template,
            int size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            GameObject obj = AH64UIObject(name, parent);
            HGTextMeshProUGUI result = obj.AddComponent<HGTextMeshProUGUI>();
            CopyTextStyleFromRow(template, result);
            result.fontSize = size;
            result.enableAutoSizing = false;
            result.enableWordWrapping = true;
            result.overflowMode = TextOverflowModes.Overflow;
            result.alignment = alignment;
            result.color = color;
            result.raycastTarget = false;
            result.margin = Vector4.zero;
            return result;
        }

        private static Image AH64ColoredImage(GameObject go, Color color)
        {
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void AH64Outline(GameObject go, Color color)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
        }

        private static void AH64AccentLine(Transform parent, string name,
            bool left, float thickness, Color color)
        {
            GameObject line = AH64UIObject(name, parent, true);
            AH64ColoredImage(line, color);
            RectTransform rect = line.GetComponent<RectTransform>();
            if (left)
                AH64Rect(rect, new Vector2(0f, 0f), new Vector2(0f, 1f),
                    Vector2.zero, new Vector2(thickness, 0f));
            else
                AH64Rect(rect, new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(0f, -thickness), Vector2.zero);
        }

        private void CreateAH64CampaignPanel(GameObject rowTemplate, Transform scrollParent)
        {
            GameObject root = new GameObject("AH64 Only - Campaign Cards Scroll Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.transform.SetParent(scrollParent, false);
            ah64CampaignContentRoot = root.GetComponent<RectTransform>();
            ah64CampaignContentRoot.anchorMin = new Vector2(0f, 1f);
            ah64CampaignContentRoot.anchorMax = new Vector2(1f, 1f);
            ah64CampaignContentRoot.pivot = new Vector2(0.5f, 1f);
            ah64CampaignContentRoot.anchoredPosition = new Vector2(0f, -2f);
            ah64CampaignContentRoot.sizeDelta = new Vector2(-8f, 0f);

            VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 5f;
            layout.padding = new RectOffset(0, 0, 0, 10);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = root.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Ficha de misión real, conservando Nombre, descripción,
            // restricciones y la eventual Descripción del survivor.
            GameObject missionCard = AH64UIObject(
                "AH64 Mission Summary Card", root.transform, true);
            AH64ColoredImage(missionCard,
                AH64DarkPanel);
            AH64Outline(missionCard, AH64PanelBorder);
            AH64AccentLine(
                missionCard.transform,
                "AH64 Mission Top Border",
                false,
                2f,
                AH64PanelBorder
            );

            ah64CampaignMissionCardLayout =
                missionCard.AddComponent<LayoutElement>();
            ah64CampaignMissionCardLayout.preferredHeight = 80f;
            ah64CampaignIntro = AH64Text(
                "AH64 Mission and Description", missionCard.transform,
                rowTemplate, 17, Color.white);
            AH64Rect(ah64CampaignIntro.rectTransform, Vector2.zero,
                Vector2.one, new Vector2(10f, 10f),
                new Vector2(-10f, -10f));

            // Los objetivos pertenecen a un MISMO recuadro de campaña.
            // La misión de desbloqueo y las Notas permanecen fuera de él.
            // La altura coincide con las alturas reales de sus 6 hijos,
            // los 5 espacios entre ellos y los márgenes interiores.
            const float campaignHeaderHeight = 50f;
            const float campaignObjectiveHeight = 84f;
            const float campaignFinalHeight = 82f;
            const float campaignSpacing = 3f;
            const int campaignPadding = 6;
            GameObject campaignGroup = AH64UIObject(
                "AH64 Campaign - Unified Outer Frame", root.transform, true);
            AH64ColoredImage(campaignGroup, AH64DarkPanel);
            AH64Outline(campaignGroup, AH64PanelBorder);
            campaignGroup.AddComponent<LayoutElement>().preferredHeight =
                campaignHeaderHeight + (4f * campaignObjectiveHeight) +
                campaignFinalHeight + (5f * campaignSpacing) +
                (2f * campaignPadding);
            VerticalLayoutGroup campaignLayout = campaignGroup.AddComponent<VerticalLayoutGroup>();
            campaignLayout.padding = new RectOffset(
                campaignPadding, campaignPadding, campaignPadding, campaignPadding);
            campaignLayout.spacing = campaignSpacing;
            campaignLayout.childAlignment = TextAnchor.UpperLeft;
            campaignLayout.childControlWidth = true;
            campaignLayout.childControlHeight = true;
            campaignLayout.childForceExpandWidth = true;
            campaignLayout.childForceExpandHeight = false;

            GameObject header = AH64UIObject("Campaign header", campaignGroup.transform, true);
            AH64ColoredImage(header, AH64RedBack);
            AH64Outline(header, AH64RedBorder);
            // Borde rojo izquierdo y superior como en la referencia aprobada.
            AH64AccentLine(header.transform, "Crimson header left", true,
                4f, AH64RedBorder);
            AH64AccentLine(header.transform, "Crimson header top", false,
                3f, AH64RedBorder);
            header.AddComponent<LayoutElement>().preferredHeight = 50f;
            ah64CampaignHeading = AH64Text("Heading", header.transform, rowTemplate, 17, Color.white);
            AH64Rect(ah64CampaignHeading.rectTransform,
                new Vector2(0f, 0f), new Vector2(0.70f, 1f),
                new Vector2(10f, 1f), new Vector2(-8f, -1f));
            ah64CampaignHeading.alignment = TextAlignmentOptions.MidlineLeft;
            ah64CampaignHeading.enableWordWrapping = true;
            ah64CampaignHeading.overflowMode = TextOverflowModes.Overflow;
            ah64CampaignHeadingHint = AH64Text("Heading hint", header.transform, rowTemplate, 14, AH64Muted,
                TextAlignmentOptions.Center);
            AH64Rect(ah64CampaignHeadingHint.rectTransform,
                new Vector2(0.70f, 0f), new Vector2(1f, 1f),
                new Vector2(8f, 1f), new Vector2(-10f, -1f));
            ah64CampaignHeadingHint.enableWordWrapping = true;
            ah64CampaignHeadingHint.enableAutoSizing = false;
            ah64CampaignHeadingHint.overflowMode = TextOverflowModes.Overflow;

            for (int i = 0; i < 4; i++)
                CreateAH64Objective(i, rowTemplate, campaignGroup.transform);
            CreateAH64Final(rowTemplate, campaignGroup.transform);

            // Notas al final, dentro de su tarjeta (nunca se revela
            // un diario bloqueado: se reutiliza ResolveNotesText()).
            GameObject notesBox = AH64UIObject(
                "AH64 Notes Card", root.transform, true);
            AH64ColoredImage(notesBox,
                AH64DarkPanel);
            AH64Outline(notesBox, AH64PanelBorder);
            ah64CampaignNotesCardLayout =
                notesBox.AddComponent<LayoutElement>();
            ah64CampaignNotesCardLayout.preferredHeight = 86f;
            ah64CampaignNotesHeading = AH64Text(
                "Notes Heading", notesBox.transform,
                rowTemplate, 17, Color.white);
            AH64Rect(ah64CampaignNotesHeading.rectTransform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(10f, -28f), new Vector2(-10f, -6f));
            ah64CampaignNotes = AH64Text(
                "AH64 Logbook Text", notesBox.transform,
                rowTemplate, 16, AH64Muted);
            AH64Rect(ah64CampaignNotes.rectTransform,
                Vector2.zero, Vector2.one,
                new Vector2(10f, 8f), new Vector2(-10f, -28f));

            root.SetActive(false);
        }

        private void CreateAH64Objective(int index, GameObject template, Transform parent)
        {
            GameObject card = AH64UIObject("AH64 Objective " + (index + 1), parent, true);
            AH64ColoredImage(card, AH64CardBack);
            AH64Outline(card, AH64PanelBorder);
            // Menos espacio vertical sin comprimir el texto ni la barra.
            card.AddComponent<LayoutElement>().preferredHeight = 84f;

            ah64CampaignCardTitles[index] = AH64Text("Objective title", card.transform,
                template, index == 3 ? 16 : 18, Color.white);
            AH64Rect(ah64CampaignCardTitles[index].rectTransform, new Vector2(0f, 1f),
                new Vector2(1f, 1f), new Vector2(10f, -27f), new Vector2(-12f, -5f));

            ah64CampaignCardDescriptions[index] = AH64Text("Objective description", card.transform,
                template, 14, AH64Muted);
            AH64Rect(ah64CampaignCardDescriptions[index].rectTransform, new Vector2(0f, 1f),
                new Vector2(1f, 1f), new Vector2(10f, -49f), new Vector2(-12f, -27f));

            GameObject track = AH64UIObject("Continuous progress track", card.transform, true);
            AH64ColoredImage(track, new Color(0.006f, 0.024f, 0.04f, 1f));
            AH64Outline(track, new Color(0.18f, 0.55f, 0.74f, 0.92f));
            AH64Rect(track.GetComponent<RectTransform>(), new Vector2(0f, 0f),
                new Vector2(1f, 0f), new Vector2(10f, 16f), new Vector2(-146f, 30f));

            GameObject fill = AH64UIObject("Cyan progress fill", track.transform, true);
            ah64CampaignFills[index] = AH64ColoredImage(fill, AH64BarCyan);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            ah64CampaignValues[index] = AH64Text("Numeric progress", card.transform,
                template, 15, Color.white, TextAlignmentOptions.MidlineRight);
            AH64Rect(ah64CampaignValues[index].rectTransform, new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-144f, 12f), new Vector2(-42f, 34f));

            GameObject checkBox = AH64UIObject("Complete indicator", card.transform, true);
            AH64ColoredImage(checkBox, new Color(0.04f, 0.085f, 0.12f, 1f));
            AH64Outline(checkBox, AH64PanelBorder);
            AH64Rect(checkBox.GetComponent<RectTransform>(), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-34f, 14f), new Vector2(-8f, 38f));

            ah64CampaignChecks[index] = AH64Text("Completion check", checkBox.transform,
                template, 22, AH64CheckGreen, TextAlignmentOptions.Center);
            AH64Rect(ah64CampaignChecks[index].rectTransform, Vector2.zero,
                Vector2.one, Vector2.zero, Vector2.zero);
        }

        private void CreateAH64Final(GameObject template, Transform parent)
        {
            GameObject card = AH64UIObject("Locked final operation", parent, true);
            AH64ColoredImage(card, AH64RedBack);
            AH64Outline(card, AH64RedBorder);
            card.AddComponent<RectMask2D>();
            card.AddComponent<LayoutElement>().preferredHeight = 82f;
            // Rayas tenues recortadas dentro de la tarjeta: fondo estático,
            // ningún sprite externo ni texto nuevo que pudiera romper el UI.
            for (int stripe = 0; stripe < 12; stripe++)
            {
                GameObject band = AH64UIObject("Final hatch " + stripe,
                    card.transform, true);
                AH64ColoredImage(band,
                    new Color(0.74f, 0.20f, 0.30f, 0.16f));
                RectTransform bandRect = band.GetComponent<RectTransform>();
                bandRect.anchorMin = new Vector2(0f, 0.5f);
                bandRect.anchorMax = new Vector2(0f, 0.5f);
                bandRect.sizeDelta = new Vector2(18f, 210f);
                bandRect.anchoredPosition = new Vector2(stripe * 45f + 10f, 0f);
                bandRect.localRotation = Quaternion.Euler(0f, 0f, -23f);
            }

            ah64CampaignFinalLock = AH64Text("Lock icon", card.transform,
                template, 22, new Color(0.9f, 0.92f, 0.96f, 1f), TextAlignmentOptions.Center);
            AH64Rect(ah64CampaignFinalLock.rectTransform, new Vector2(0f, 0f),
                new Vector2(0f, 1f), new Vector2(9f, 8f), new Vector2(49f, -8f));

            ah64CampaignFinalTitle = AH64Text("Final mission title", card.transform,
                template, 18, new Color(1f, 0.28f, 0.40f, 1f));
            AH64Rect(ah64CampaignFinalTitle.rectTransform, new Vector2(0f, 1f),
                new Vector2(1f, 1f), new Vector2(62f, -30f), new Vector2(-44f, -10f));

            ah64CampaignFinalDescription = AH64Text("Final mission description", card.transform,
                template, 14, AH64Muted);
            AH64Rect(ah64CampaignFinalDescription.rectTransform, new Vector2(0f, 0f),
                new Vector2(1f, 1f), new Vector2(62f, 8f), new Vector2(-44f, -30f));

            GameObject finalBox = AH64UIObject(
                "Final completion checkbox", card.transform, true);
            AH64ColoredImage(finalBox,
                new Color(0.04f, 0.085f, 0.12f, 1f));
            AH64Outline(finalBox, AH64PanelBorder);
            AH64Rect(finalBox.GetComponent<RectTransform>(),
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-34f, -12f), new Vector2(-8f, 12f));
            ah64CampaignFinalCheck = AH64Text(
                "Final completion", finalBox.transform,
                template, 22, AH64CheckGreen,
                TextAlignmentOptions.Center);
            AH64Rect(ah64CampaignFinalCheck.rectTransform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Candado simple rehecho desde cero.
            GameObject iconPanel = AH64UIObject("Final lock backdrop", card.transform, true);
            AH64ColoredImage(iconPanel, new Color(0.13f, 0.02f, 0.05f, 0.52f));
            AH64Rect(iconPanel.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(12f, -23f), new Vector2(50f, 13f));

            GameObject shackleLeft = AH64UIObject("Padlock shackle left", card.transform, true);
            AH64ColoredImage(shackleLeft, new Color(0.90f, 0.92f, 0.96f, 0.98f));
            AH64Rect(shackleLeft.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(24f, -2f), new Vector2(28f, 10f));

            GameObject shackleRight = AH64UIObject("Padlock shackle right", card.transform, true);
            AH64ColoredImage(shackleRight, new Color(0.90f, 0.92f, 0.96f, 0.98f));
            AH64Rect(shackleRight.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(34f, -2f), new Vector2(38f, 10f));

            GameObject shackleTop = AH64UIObject("Padlock shackle top", card.transform, true);
            AH64ColoredImage(shackleTop, new Color(0.90f, 0.92f, 0.96f, 0.98f));
            AH64Rect(shackleTop.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(24f, 8f), new Vector2(38f, 11f));

            GameObject lockBody = AH64UIObject("Padlock body", card.transform, true);
            AH64ColoredImage(lockBody, new Color(0.90f, 0.92f, 0.96f, 0.98f));
            AH64Rect(lockBody.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(20f, -18f), new Vector2(42f, -2f));

            GameObject keyholeHead = AH64UIObject("Padlock keyhole head", card.transform, true);
            AH64ColoredImage(keyholeHead, new Color(0.32f, 0.03f, 0.07f, 0.95f));
            AH64Rect(keyholeHead.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(29f, -11f), new Vector2(33f, -7f));

            GameObject keyholeStem = AH64UIObject("Padlock keyhole stem", card.transform, true);
            AH64ColoredImage(keyholeStem, new Color(0.32f, 0.03f, 0.07f, 0.95f));
            AH64Rect(keyholeStem.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(30f, -16f), new Vector2(32f, -11f));
        }

        private static float AH64TextPreferredHeight(HGTextMeshProUGUI text, float width)
        {
            if (text == null) return 90f;
            text.ForceMeshUpdate();
            return Mathf.Max(36f, text.GetPreferredValues(text.text, Mathf.Max(240f, width), 0f).y + 14f);
        }

        private void UpdateAH64CampaignPanel(SurvivorContentProfile profile)
        {
            if (ah64CampaignContentRoot == null || survivorDetailsScrollRect == null) return;
            survivorDetailsText.gameObject.SetActive(false);
            ah64CampaignContentRoot.gameObject.SetActive(true);
            survivorDetailsScrollRect.content = ah64CampaignContentRoot;
            bool spanish = AH64CampaignTracker.IsSpanish;

            // La tarjeta muestra la misión EFECTIVA, no una copia vieja
            // grabada en Survivors.json. El nombre/descripcion del preset
            // oficial se resuelven vía SurvivorLocalization; Custom y Generated
            // siguen usando exactamente sus propios textos.
            ah64CampaignIntro.text = BuildPresetMissionSummaryText(profile);
            float width = survivorDetailsScrollRect.viewport != null
                ? survivorDetailsScrollRect.viewport.rect.width - 44f : 520f;
            ah64CampaignMissionCardLayout.preferredHeight =
                Mathf.Max(80f, AH64TextPreferredHeight(
                    ah64CampaignIntro, width - 20f) + 4f);
            ah64CampaignHeading.text = spanish
                ? "<b>Progreso de campaña — Protocolo 64</b>"
                : "<b>Campaign progress — Protocol 64</b>";
            if (ah64CampaignHeadingHint != null)
                ah64CampaignHeadingHint.text = spanish
                    ? "El progreso se guarda\nentre partidas."
                    : "Progress is saved\nbetween runs.";
            ah64CampaignNotesHeading.text = spanish ? "Notas:" : "Notes:";
            ah64CampaignNotes.text = ResolveNotesText(profile, profile.ShouldHideIdentity);
            ah64CampaignNotesCardLayout.preferredHeight =
                AH64TextPreferredHeight(ah64CampaignNotes, width - 20f) + 22f;

            string[] names = spanish ? new[] {
                "Tiempo de vuelo", "Balas disparadas en el aire",
                "Misiles y cohetes en el aire", "Jefes del teletransportador derrotados en el aire"
            } : new[] {
                "Flight time", "Bullets fired airborne", "Airborne missiles and rockets",
                "Teleporter bosses defeated airborne"
            };
            string[] descriptions = spanish ? new[] {
                "Permanece en el aire durante un total de 64 minutos.",
                "Dispara 6400 balas mientras estás en el aire.",
                "Dispara 640 misiles o cohetes estando en el aire.",
                "Derrota a 64 jefes del teletransportador en el aire."
            } : new[] {
                "Accumulate 64 minutes of sustained airtime.",
                "Fire 6400 bullets while airborne.",
                "Launch 640 missiles or rockets while airborne.",
                "Defeat 64 teleporter bosses while airborne."
            };
            for (int i = 0; i < 4; i++)
            {
                ah64CampaignCardTitles[i].text = names[i];
                ah64CampaignCardDescriptions[i].text = descriptions[i];
            }
            RefreshAH64CampaignBars(true);
        }

        private void HideAH64CampaignPanel()
        {
            if (ah64CampaignContentRoot != null)
                ah64CampaignContentRoot.gameObject.SetActive(false);
        }

        private void ClearAH64CampaignPanelReferences()
        {
            ah64CampaignContentRoot = null;
            ah64CampaignIntro = null;
            ah64CampaignNotes = null;
            ah64CampaignMissionCardLayout = null;
            ah64CampaignNotesCardLayout = null;
            ah64CampaignFinalTitle = null;
            ah64CampaignFinalDescription = null;
            ah64CampaignFinalCheck = null;
            ah64CampaignFinalLock = null;
            ah64CampaignHeading = null;
            ah64CampaignHeadingHint = null;
            ah64CampaignNotesHeading = null;
            survivorPresetHeaderFrame = null;
            ah64CampaignNextRefresh = 0f;
            for (int i = 0; i < 4; i++)
            {
                ah64CampaignFills[i] = null;
                ah64CampaignValues[i] = null;
                ah64CampaignChecks[i] = null;
                ah64CampaignCardTitles[i] = null;
                ah64CampaignCardDescriptions[i] = null;
            }
        }

        private void RefreshAH64CampaignBarsIfVisible()
        {
            if (ah64CampaignContentRoot == null ||
                !ah64CampaignContentRoot.gameObject.activeInHierarchy ||
                Time.unscaledTime < ah64CampaignNextRefresh) return;
            RefreshAH64CampaignBars(false);
        }

        private void RefreshAH64CampaignBars(bool force)
        {
            if (ah64CampaignContentRoot == null) return;
            if (!force && Time.unscaledTime < ah64CampaignNextRefresh) return;
            ah64CampaignNextRefresh = Time.unscaledTime + 0.4f;
            AH64CampaignProgressStore.SaveData s = AH64CampaignProgressStore.Current;
            double[] amounts = { s.FlightSeconds, s.BulletShots,
                s.MissilesLaunched, s.TeleporterBosses };
            double[] goals = { AH64CampaignProgressStore.FlightGoal,
                AH64CampaignProgressStore.BulletGoal,
                AH64CampaignProgressStore.MissileGoal,
                AH64CampaignProgressStore.BossGoal };
            for (int i = 0; i < 4; i++)
            {
                double current = Math.Max(0d, Math.Min(goals[i], amounts[i]));
                float fraction = (float)(current / goals[i]);
                RectTransform fill = ah64CampaignFills[i].rectTransform;
                fill.anchorMax = new Vector2(fraction, 1f);
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
                bool done = current >= goals[i];
                ah64CampaignFills[i].color = done ? AH64CheckGreen : AH64BarCyan;
                ah64CampaignValues[i].text = i == 0
                    ? (current / 60d).ToString("0.0", CultureInfo.InvariantCulture) + " / 64 min"
                    : ((int)current).ToString("N0", CultureInfo.InvariantCulture) +
                      " / " + ((int)goals[i]).ToString("N0", CultureInfo.InvariantCulture);
                ah64CampaignValues[i].color = done ? AH64CheckGreen : Color.white;
                ah64CampaignChecks[i].text = done ? "✓" : "";
            }
            bool finalDone = s.FinalEscapeComplete;
            bool finalAvailable = AH64CampaignProgressStore.ObjectivesComplete;
            bool spanish = AH64CampaignTracker.IsSpanish;
            ah64CampaignFinalTitle.text = spanish ? "Operación final" : "Final operation";
            ah64CampaignFinalDescription.text = spanish
                ? (finalDone ? "Completada. Superioridad aérea asegurada." :
                  "Escapa de la Luna en Monzón o superior. " +
                  (finalAvailable ? "Disponible: todos los objetivos completados." :
                  "Disponible al completar los objetivos anteriores."))
                : (finalDone ? "Complete. Air supremacy established." :
                  "Escape the Moon on Monsoon or higher. " +
                  (finalAvailable ? "Available: all objectives completed." :
                  "Available after completing all previous objectives."));
            ah64CampaignFinalCheck.text = finalDone ? "✓" : "";
            ah64CampaignFinalLock.text = "";
            ah64CampaignFinalLock.color = finalDone || finalAvailable
                ? AH64CheckGreen : AH64Muted;
        }

        // Se conservan los cuatro botones originales (con sus listeners).
        // Los cuatro botones existentes usan el mismo preset visual para
        // todos los survivors, sin cambiar listeners ni estado de selección.
        private void StyleSurvivorPresetProviderRow(SurvivorContentProfile profile)
        {
            if (survivorProviderControlsRoot == null) return;
            // El diseño validado de AH-64 es ahora el preset visual común.
            // El progreso de campaña sigue siendo exclusivo de AH64Body.
            bool usePreset = profile != null;
            RectTransform rect =
                survivorProviderControlsRoot.GetComponent<RectTransform>();
            HorizontalLayoutGroup layout =
                survivorProviderControlsRoot.GetComponent<HorizontalLayoutGroup>();
            if (rect == null || layout == null) return;

            // Cabecera del preset común: conserva identidad, estado, fuente,
            // selección de provider y todos los controles nativos existentes.
            // Con identidad visible se conserva la eliminación del renglón
            // vacío aprobada en versiones anteriores.
            bool identityVisible = usePreset && !profile.ShouldHideIdentity;
            if (usePreset && survivorDetailsHeaderText != null)
            {
                string headerText = survivorDetailsHeaderText.text
                    .Replace("<size=25>", "<size=27>")
                    .Replace("<size=16>", "<size=18>")
                    .Replace("<size=14>", "<size=16>");
                if (identityVisible)
                    headerText = headerText.Replace(
                        "\n\n<size=16><b>", "\n<size=16><b>");
                survivorDetailsHeaderText.text = headerText;
            }

            // El contenido de todos los survivors comienza bajo la cabecera fija.
            if (survivorDetailsScrollRect != null)
            {
                RectTransform missionViewport =
                    survivorDetailsScrollRect.GetComponent<RectTransform>();

                if (missionViewport != null)
                {
                    missionViewport.offsetMax = new Vector2(
                        -16f,
                        usePreset ? -170f : -204f
                    );
                }
            }

            // El borde rodea el marco rectangular, no la silueta del retrato.
            RectTransform portraitFrame =
                survivorDetailsPortrait != null
                    ? survivorDetailsPortrait.transform.parent as RectTransform
                    : null;

            if (portraitFrame != null)
            {
                // El marco ya tiene Image: lo crea SurvivorDetails.cs.
                // Outline sobre ese Image dibuja el borde del rectangulo.
                Outline presetFrameOutline =
                    portraitFrame.GetComponent<Outline>();
                if (usePreset && presetFrameOutline == null)
                    presetFrameOutline = portraitFrame.gameObject.AddComponent<Outline>();

                if (presetFrameOutline != null)
                {
                    presetFrameOutline.enabled = usePreset;
                    if (usePreset)
                    {
                        presetFrameOutline.effectColor =
                            new Color32(34, 93, 121, 220);
                        presetFrameOutline.effectDistance =
                            new Vector2(1f, -1f);
                        presetFrameOutline.useGraphicAlpha = true;
                    }
                }

                // Mantener el tamaño y la posicion ya aprobados en el juego.
                portraitFrame.sizeDelta = usePreset
                    ? new Vector2(118f, 118f)
                    : new Vector2(132f, 132f);
                portraitFrame.anchoredPosition = new Vector2(16f, -16f);
            }

            if (survivorDetailsHeaderText != null)
            {
                RectTransform headerRect =
                    survivorDetailsHeaderText.rectTransform;

                headerRect.offsetMin = usePreset
                    ? new Vector2(144f, identityVisible ? -158f : -148f)
                    : new Vector2(164f, -148f);

                headerRect.offsetMax =
                    new Vector2(-80f, identityVisible ? -12f : -18f);
            }

            // El área superior mantiene el retrato, estado y provider;
            // los cuatro botones permanecen delante del scroll.
            // Se conserva la geometría del preset para estados ocultos y visibles.
            rect.offsetMin = usePreset
                ? new Vector2(identityVisible ? 146f : 150f,
                    identityVisible ? -151f : -145f)
                : new Vector2(16f, -196f);

            rect.offsetMax = usePreset
                ? new Vector2(identityVisible ? -14f : -10f,
                    identityVisible ? -116f : -109f)
                : new Vector2(-64f, -156f);
            if (usePreset)
            {
                Transform frameParent = survivorProviderControlsRoot.transform.parent;
                if (frameParent != null)
                {
                    if (survivorPresetHeaderFrame == null)
                    {
                        GameObject frame = AH64UIObject("Survivor Preset Header Frame", frameParent, true);
                        AH64ColoredImage(frame, new Color32(0, 13, 22, 255));
                        AH64Outline(frame, AH64PanelBorder);
                        survivorPresetHeaderFrame = frame.GetComponent<RectTransform>();
                        survivorPresetHeaderFrame.SetAsFirstSibling();
                    }
                    AH64Rect(survivorPresetHeaderFrame,
                        new Vector2(0f, 1f),
                        new Vector2(1f, 1f),
                        new Vector2(10f, identityVisible ? -163f : -150f),
                        new Vector2(-10f, -8f));
                }
            }
            else if (survivorPresetHeaderFrame != null)
                survivorPresetHeaderFrame.gameObject.SetActive(false);
            if (survivorPresetHeaderFrame != null)
                survivorPresetHeaderFrame.gameObject.SetActive(usePreset);
            layout.childControlWidth = true;
            layout.childForceExpandWidth = false;
            layout.spacing = usePreset ? 6f : 8f;
            Button[] buttons = {
                survivorProviderOriginalButton,
                survivorProviderUsuButton,
                survivorProviderCustomButton,
                survivorProviderGeneratedButton
            };
            bool[] selected = new bool[4];
            if (usePreset)
            {
                SurvivorJsonEntry entry =
                    SurvivorJsonManager.GetEntryAnywhere(profile.BodyName);
                if (entry != null)
                {
                    MissionProviderSelectionState state =
                        MissionProviderSelectionPolicy.ResolveFromContentProfile(
                            entry, profile.SurvivorUnlock?.Source ??
                            SurvivorContentUnlockSource.Unknown);
                    selected[0] = state.ActiveProvider == UnlockProviderKind.Original;
                    selected[1] = state.IsUsuActive;
                    selected[2] = state.IsCustomActive;
                    selected[3] = state.IsGeneratedActive;
                }
            }
            for (int i = 0; i < buttons.Length; i++)
            {
                Button b = buttons[i];
                if (b == null) continue;
                LayoutElement element = b.GetComponent<LayoutElement>();
                if (element != null)
                {
                    if (usePreset)
                    {
                        float[] widths = { 82f, 70f, 102f, 84f };
                        float width = i >= 0 && i < widths.Length ? widths[i] : 84f;
                        element.minWidth = width;
                        element.preferredWidth = width;
                        element.flexibleWidth = 0f;
                        element.minHeight = 24f;
                        element.preferredHeight = 24f;
                    }
                    else
                    {
                        element.minWidth = 104f;
                        element.preferredWidth = 104f;
                        element.flexibleWidth = 0f;
                    }
                }
                Image img = b.GetComponent<Image>();
                if (img != null)
                    img.color = usePreset
                        ? (selected[i]
                            ? new Color(0.07f, 0.19f, 0.25f, 0.98f)
                            : new Color(0.03f, 0.08f, 0.12f, 0.96f))
                        : new Color(0.10f, 0.18f, 0.24f, 0.96f);
                if (usePreset)
                {
                    HGTextMeshProUGUI label = b.GetComponentInChildren<HGTextMeshProUGUI>();
                    if (label != null)
                    {
                        label.fontSize = 12.5f;
                        label.enableAutoSizing = false;
                    }
                }
                // Outline creada una sola vez, no en cada refresco.
                Outline outline = b.GetComponent<Outline>();
                if (usePreset && outline == null)
                    outline = b.gameObject.AddComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = usePreset;
                    if (usePreset)
                    {
                        outline.effectColor = selected[i]
                            ? AH64BarCyan : AH64PanelBorder;
                        outline.effectDistance = new Vector2(1f, -1f);
                    }
                }
            }
        }

    }
}
