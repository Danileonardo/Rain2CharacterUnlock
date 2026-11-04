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
        private RectTransform cwtCampaignContentRoot;
        private HGTextMeshProUGUI cwtCampaignIntro;
        private HGTextMeshProUGUI cwtCampaignNotes;
        private readonly Image[] cwtCampaignFills = new Image[4];
        private readonly HGTextMeshProUGUI[] cwtCampaignValues = new HGTextMeshProUGUI[4];
        private readonly HGTextMeshProUGUI[] cwtCampaignChecks = new HGTextMeshProUGUI[4];
        private HGTextMeshProUGUI cwtCampaignFinalTitle;
        private HGTextMeshProUGUI cwtCampaignFinalDescription;
        private HGTextMeshProUGUI cwtCampaignFinalCheck;
        private HGTextMeshProUGUI cwtCampaignFinalLock;
        private HGTextMeshProUGUI cwtCampaignHeading;
        private HGTextMeshProUGUI cwtCampaignHeadingHint;
        private HGTextMeshProUGUI cwtCampaignNotesHeading;
        private LayoutElement cwtCampaignMissionCardLayout;
        private LayoutElement cwtCampaignNotesCardLayout;
        private readonly HGTextMeshProUGUI[] cwtCampaignCardTitles = new HGTextMeshProUGUI[4];
        private readonly HGTextMeshProUGUI[] cwtCampaignCardDescriptions = new HGTextMeshProUGUI[4];
        private float cwtCampaignNextRefresh;

        private static readonly Color CWTCardBack = new Color(0.028f, 0.083f, 0.118f, 0.995f);
        private static readonly Color CWTPanelBorder = new Color(0.10f, 0.74f, 0.95f, 0.98f);
        private static readonly Color CWTBarCyan = new Color(0.27f, 0.87f, 0.96f, 1f);
        private static readonly Color CWTCheckGreen = new Color(0.35f, 0.94f, 0.52f, 1f);
        private static readonly Color CWTMuted = new Color(0.69f, 0.78f, 0.85f, 1f);
        private static readonly Color CWTDarkPanel = new Color(0.006f, 0.032f, 0.050f, 0.995f);
        private static readonly Color CWTRedBack = new Color(0.215f, 0.020f, 0.055f, 0.99f);
        private static readonly Color CWTRedBorder = new Color(1.0f, 0.18f, 0.34f, 1f);

        private static GameObject CWTUIObject(string name, Transform parent, bool image = false)
        {
            GameObject go = image
                ? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void CWTRect(RectTransform rect, Vector2 min, Vector2 max,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private HGTextMeshProUGUI CWTText(string name, Transform parent, GameObject template,
            int size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            GameObject obj = CWTUIObject(name, parent);
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

        private static Image CWTColoredImage(GameObject go, Color color)
        {
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void CWTOutline(GameObject go, Color color)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
        }

        private static void CWTAccentLine(Transform parent, string name,
            bool left, float thickness, Color color)
        {
            GameObject line = CWTUIObject(name, parent, true);
            CWTColoredImage(line, color);
            RectTransform rect = line.GetComponent<RectTransform>();
            if (left)
                CWTRect(rect, new Vector2(0f, 0f), new Vector2(0f, 1f),
                    Vector2.zero, new Vector2(thickness, 0f));
            else
                CWTRect(rect, new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(0f, -thickness), Vector2.zero);
        }

        private void CreateCWTCampaignPanel(GameObject rowTemplate, Transform scrollParent)
        {
            GameObject root = new GameObject("CWT Only - Campaign Cards Scroll Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.transform.SetParent(scrollParent, false);
            cwtCampaignContentRoot = root.GetComponent<RectTransform>();
            cwtCampaignContentRoot.anchorMin = new Vector2(0f, 1f);
            cwtCampaignContentRoot.anchorMax = new Vector2(1f, 1f);
            cwtCampaignContentRoot.pivot = new Vector2(0.5f, 1f);
            cwtCampaignContentRoot.anchoredPosition = new Vector2(0f, -2f);
            cwtCampaignContentRoot.sizeDelta = new Vector2(-8f, 0f);

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
            GameObject missionCard = CWTUIObject(
                "CWT Mission Summary Card", root.transform, true);
            CWTColoredImage(missionCard,
                CWTDarkPanel);
            CWTOutline(missionCard, CWTPanelBorder);
            CWTAccentLine(
                missionCard.transform,
                "CWT Mission Top Border",
                false,
                2f,
                CWTPanelBorder
            );

            cwtCampaignMissionCardLayout =
                missionCard.AddComponent<LayoutElement>();
            cwtCampaignMissionCardLayout.preferredHeight = 80f;
            cwtCampaignIntro = CWTText(
                "CWT Mission and Description", missionCard.transform,
                rowTemplate, 17, Color.white);
            CWTRect(cwtCampaignIntro.rectTransform, Vector2.zero,
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
            GameObject campaignGroup = CWTUIObject(
                "CWT Campaign - Unified Outer Frame", root.transform, true);
            CWTColoredImage(campaignGroup, CWTDarkPanel);
            CWTOutline(campaignGroup, CWTPanelBorder);
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

            GameObject header = CWTUIObject("Campaign header", campaignGroup.transform, true);
            CWTColoredImage(header, CWTRedBack);
            CWTOutline(header, CWTRedBorder);
            // Borde rojo izquierdo y superior como en la referencia aprobada.
            CWTAccentLine(header.transform, "Crimson header left", true,
                4f, CWTRedBorder);
            CWTAccentLine(header.transform, "Crimson header top", false,
                3f, CWTRedBorder);
            header.AddComponent<LayoutElement>().preferredHeight = 50f;
            cwtCampaignHeading = CWTText("Heading", header.transform, rowTemplate, 17, Color.white);
            CWTRect(cwtCampaignHeading.rectTransform,
                new Vector2(0f, 0f), new Vector2(0.70f, 1f),
                new Vector2(10f, 1f), new Vector2(-8f, -1f));
            cwtCampaignHeading.alignment = TextAlignmentOptions.MidlineLeft;
            cwtCampaignHeading.enableWordWrapping = true;
            cwtCampaignHeading.overflowMode = TextOverflowModes.Overflow;
            cwtCampaignHeadingHint = CWTText("Heading hint", header.transform, rowTemplate, 14, CWTMuted,
                TextAlignmentOptions.Center);
            CWTRect(cwtCampaignHeadingHint.rectTransform,
                new Vector2(0.70f, 0f), new Vector2(1f, 1f),
                new Vector2(8f, 1f), new Vector2(-10f, -1f));
            cwtCampaignHeadingHint.enableWordWrapping = true;
            cwtCampaignHeadingHint.enableAutoSizing = false;
            cwtCampaignHeadingHint.overflowMode = TextOverflowModes.Overflow;

            for (int i = 0; i < 4; i++)
                CreateCWTObjective(i, rowTemplate, campaignGroup.transform);
            CreateCWTFinal(rowTemplate, campaignGroup.transform);

            // Notas al final, dentro de su tarjeta (nunca se revela
            // un diario bloqueado: se reutiliza ResolveNotesText()).
            GameObject notesBox = CWTUIObject(
                "CWT Notes Card", root.transform, true);
            CWTColoredImage(notesBox,
                CWTDarkPanel);
            CWTOutline(notesBox, CWTPanelBorder);
            cwtCampaignNotesCardLayout =
                notesBox.AddComponent<LayoutElement>();
            cwtCampaignNotesCardLayout.preferredHeight = 86f;
            cwtCampaignNotesHeading = CWTText(
                "Notes Heading", notesBox.transform,
                rowTemplate, 17, Color.white);
            CWTRect(cwtCampaignNotesHeading.rectTransform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(10f, -28f), new Vector2(-10f, -6f));
            cwtCampaignNotes = CWTText(
                "CWT Logbook Text", notesBox.transform,
                rowTemplate, 16, CWTMuted);
            CWTRect(cwtCampaignNotes.rectTransform,
                Vector2.zero, Vector2.one,
                new Vector2(10f, 8f), new Vector2(-10f, -28f));

            root.SetActive(false);
        }

        private void CreateCWTObjective(int index, GameObject template, Transform parent)
        {
            GameObject card = CWTUIObject("CWT Objective " + (index + 1), parent, true);
            CWTColoredImage(card, CWTCardBack);
            CWTOutline(card, CWTPanelBorder);
            // Menos espacio vertical sin comprimir el texto ni la barra.
            card.AddComponent<LayoutElement>().preferredHeight = 84f;

            cwtCampaignCardTitles[index] = CWTText("Objective title", card.transform,
                template, index == 3 ? 16 : 18, Color.white);
            CWTRect(cwtCampaignCardTitles[index].rectTransform, new Vector2(0f, 1f),
                new Vector2(1f, 1f), new Vector2(10f, -27f), new Vector2(-12f, -5f));

            cwtCampaignCardDescriptions[index] = CWTText("Objective description", card.transform,
                template, 14, CWTMuted);
            CWTRect(cwtCampaignCardDescriptions[index].rectTransform, new Vector2(0f, 1f),
                new Vector2(1f, 1f), new Vector2(10f, -49f), new Vector2(-12f, -27f));

            GameObject track = CWTUIObject("Continuous progress track", card.transform, true);
            CWTColoredImage(track, new Color(0.006f, 0.024f, 0.04f, 1f));
            CWTOutline(track, new Color(0.18f, 0.55f, 0.74f, 0.92f));
            CWTRect(track.GetComponent<RectTransform>(), new Vector2(0f, 0f),
                new Vector2(1f, 0f), new Vector2(10f, 16f), new Vector2(-146f, 30f));

            GameObject fill = CWTUIObject("Cyan progress fill", track.transform, true);
            cwtCampaignFills[index] = CWTColoredImage(fill, CWTBarCyan);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            cwtCampaignValues[index] = CWTText("Numeric progress", card.transform,
                template, 15, Color.white, TextAlignmentOptions.MidlineRight);
            CWTRect(cwtCampaignValues[index].rectTransform, new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-144f, 12f), new Vector2(-42f, 34f));

            GameObject checkBox = CWTUIObject("Complete indicator", card.transform, true);
            CWTColoredImage(checkBox, new Color(0.04f, 0.085f, 0.12f, 1f));
            CWTOutline(checkBox, CWTPanelBorder);
            CWTRect(checkBox.GetComponent<RectTransform>(), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-34f, 14f), new Vector2(-8f, 38f));

            cwtCampaignChecks[index] = CWTText("Completion check", checkBox.transform,
                template, 22, CWTCheckGreen, TextAlignmentOptions.Center);
            CWTRect(cwtCampaignChecks[index].rectTransform, Vector2.zero,
                Vector2.one, Vector2.zero, Vector2.zero);
        }

        private void CreateCWTFinal(GameObject template, Transform parent)
        {
            GameObject card = CWTUIObject("Locked final operation", parent, true);
            CWTColoredImage(card, CWTRedBack);
            CWTOutline(card, CWTRedBorder);
            card.AddComponent<RectMask2D>();
            card.AddComponent<LayoutElement>().preferredHeight = 82f;
            // Rayas tenues recortadas dentro de la tarjeta: fondo estático,
            // ningún sprite externo ni texto nuevo que pudiera romper el UI.
            for (int stripe = 0; stripe < 12; stripe++)
            {
                GameObject band = CWTUIObject("Final hatch " + stripe,
                    card.transform, true);
                CWTColoredImage(band,
                    new Color(0.74f, 0.20f, 0.30f, 0.16f));
                RectTransform bandRect = band.GetComponent<RectTransform>();
                bandRect.anchorMin = new Vector2(0f, 0.5f);
                bandRect.anchorMax = new Vector2(0f, 0.5f);
                bandRect.sizeDelta = new Vector2(18f, 210f);
                bandRect.anchoredPosition = new Vector2(stripe * 45f + 10f, 0f);
                bandRect.localRotation = Quaternion.Euler(0f, 0f, -23f);
            }

            cwtCampaignFinalLock = CWTText("Lock icon", card.transform,
                template, 22, new Color(0.9f, 0.92f, 0.96f, 1f), TextAlignmentOptions.Center);
            CWTRect(cwtCampaignFinalLock.rectTransform, new Vector2(0f, 0f),
                new Vector2(0f, 1f), new Vector2(9f, 8f), new Vector2(49f, -8f));

            cwtCampaignFinalTitle = CWTText("Final mission title", card.transform,
                template, 18, new Color(1f, 0.28f, 0.40f, 1f));
            CWTRect(cwtCampaignFinalTitle.rectTransform, new Vector2(0f, 1f),
                new Vector2(1f, 1f), new Vector2(62f, -30f), new Vector2(-44f, -10f));

            cwtCampaignFinalDescription = CWTText("Final mission description", card.transform,
                template, 14, CWTMuted);
            CWTRect(cwtCampaignFinalDescription.rectTransform, new Vector2(0f, 0f),
                new Vector2(1f, 1f), new Vector2(62f, 8f), new Vector2(-44f, -30f));

            GameObject finalBox = CWTUIObject(
                "Final completion checkbox", card.transform, true);
            CWTColoredImage(finalBox,
                new Color(0.04f, 0.085f, 0.12f, 1f));
            CWTOutline(finalBox, CWTPanelBorder);
            CWTRect(finalBox.GetComponent<RectTransform>(),
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-34f, -12f), new Vector2(-8f, 12f));
            cwtCampaignFinalCheck = CWTText(
                "Final completion", finalBox.transform,
                template, 22, CWTCheckGreen,
                TextAlignmentOptions.Center);
            CWTRect(cwtCampaignFinalCheck.rectTransform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Candado simple rehecho desde cero.
            GameObject iconPanel = CWTUIObject("Final lock backdrop", card.transform, true);
            CWTColoredImage(iconPanel, new Color(0.13f, 0.02f, 0.05f, 0.52f));
            CWTRect(iconPanel.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(12f, -23f), new Vector2(50f, 13f));

            GameObject shackleLeft = CWTUIObject("Padlock shackle left", card.transform, true);
            CWTColoredImage(shackleLeft, new Color(0.90f, 0.92f, 0.96f, 0.98f));
            CWTRect(shackleLeft.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(24f, -2f), new Vector2(28f, 10f));

            GameObject shackleRight = CWTUIObject("Padlock shackle right", card.transform, true);
            CWTColoredImage(shackleRight, new Color(0.90f, 0.92f, 0.96f, 0.98f));
            CWTRect(shackleRight.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(34f, -2f), new Vector2(38f, 10f));

            GameObject shackleTop = CWTUIObject("Padlock shackle top", card.transform, true);
            CWTColoredImage(shackleTop, new Color(0.90f, 0.92f, 0.96f, 0.98f));
            CWTRect(shackleTop.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(24f, 8f), new Vector2(38f, 11f));

            GameObject lockBody = CWTUIObject("Padlock body", card.transform, true);
            CWTColoredImage(lockBody, new Color(0.90f, 0.92f, 0.96f, 0.98f));
            CWTRect(lockBody.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(20f, -18f), new Vector2(42f, -2f));

            GameObject keyholeHead = CWTUIObject("Padlock keyhole head", card.transform, true);
            CWTColoredImage(keyholeHead, new Color(0.32f, 0.03f, 0.07f, 0.95f));
            CWTRect(keyholeHead.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(29f, -11f), new Vector2(33f, -7f));

            GameObject keyholeStem = CWTUIObject("Padlock keyhole stem", card.transform, true);
            CWTColoredImage(keyholeStem, new Color(0.32f, 0.03f, 0.07f, 0.95f));
            CWTRect(keyholeStem.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(30f, -16f), new Vector2(32f, -11f));
        }

        private static float CWTTextPreferredHeight(HGTextMeshProUGUI text, float width)
        {
            if (text == null) return 90f;
            text.ForceMeshUpdate();
            return Mathf.Max(36f, text.GetPreferredValues(text.text, Mathf.Max(240f, width), 0f).y + 14f);
        }

        private void UpdateCWTCampaignPanel(SurvivorContentProfile profile)
        {
            if (cwtCampaignContentRoot == null || survivorDetailsScrollRect == null) return;
            survivorDetailsText.gameObject.SetActive(false);
            cwtCampaignContentRoot.gameObject.SetActive(true);
            survivorDetailsScrollRect.content = cwtCampaignContentRoot;
            bool spanish = CWTCampaignTracker.IsSpanish;
            cwtCampaignIntro.text = BuildPresetMissionSummaryText(profile);
            float width = survivorDetailsScrollRect.viewport != null
                ? survivorDetailsScrollRect.viewport.rect.width - 44f : 520f;
            cwtCampaignMissionCardLayout.preferredHeight =
                Mathf.Max(80f, CWTTextPreferredHeight(cwtCampaignIntro, width - 20f) + 4f);
            cwtCampaignHeading.text = spanish
                ? "<b>Progreso de campaña — Acero</b>"
                : "<b>Campaign progress — Steel</b>";
            if (cwtCampaignHeadingHint != null)
                cwtCampaignHeadingHint.text = spanish
                    ? "El progreso se guarda\nentre partidas."
                    : "Progress is saved\nbetween runs.";
            cwtCampaignNotesHeading.text = spanish ? "Notas:" : "Notes:";
            // Se muestra exclusivamente el lore original del creador.
            cwtCampaignNotes.text = ResolveNotesText(profile, profile.ShouldHideIdentity);
            cwtCampaignNotesCardLayout.preferredHeight =
                CWTTextPreferredHeight(cwtCampaignNotes, width - 20f) + 22f;
            string[] names = spanish ? new[] {
                "Dominio del terreno", "Bombardeo implacable",
                "Ruptura de líneas", "Supremacía terrestre"
            } : new[] {
                "Terrain mastery", "Relentless bombardment",
                "Breaking the line", "Land supremacy"
            };
            var s = CWTCampaignProgressStore.Current;
            string stageProgress = string.Format(CultureInfo.InvariantCulture,
                "AQ {0:0.#}/10 · DT {1:0.#}/10 · AB {2:0.#}/10 km",
                s.AqueductMeters / 1000d, s.DeltaMeters / 1000d, s.AbyssMeters / 1000d);
            string[] descriptions = spanish ? new[] {
                "100 km; 10 km/mapa. " + stageProgress,
                "Consigue 10.000 impactos explosivos reales.",
                "Elimina 1.000 enemigos a menos de 12 m.",
                "Derrota 100 jefes de teletransportador."
            } : new[] {
                "100 km; 10 km/stage. " + stageProgress,
                "Land 10,000 real explosive hits.",
                "Defeat 1,000 enemies within 12 m.",
                "Defeat 100 teleporter bosses."
            };
            for (int i = 0; i < 4; i++)
            {
                cwtCampaignCardTitles[i].text = names[i];
                cwtCampaignCardDescriptions[i].text = descriptions[i];
            }
            RefreshCWTCampaignBars(true);
        }

        private void HideCWTCampaignPanel()
        {
            if (cwtCampaignContentRoot != null)
                cwtCampaignContentRoot.gameObject.SetActive(false);
        }

        private void ClearCWTCampaignPanelReferences()
        {
            cwtCampaignContentRoot = null;
            cwtCampaignIntro = null;
            cwtCampaignNotes = null;
            cwtCampaignMissionCardLayout = null;
            cwtCampaignNotesCardLayout = null;
            cwtCampaignFinalTitle = null;
            cwtCampaignFinalDescription = null;
            cwtCampaignFinalCheck = null;
            cwtCampaignFinalLock = null;
            cwtCampaignHeading = null;
            cwtCampaignHeadingHint = null;
            cwtCampaignNotesHeading = null;
            cwtCampaignNextRefresh = 0f;
            for (int i = 0; i < 4; i++)
            {
                cwtCampaignFills[i] = null;
                cwtCampaignValues[i] = null;
                cwtCampaignChecks[i] = null;
                cwtCampaignCardTitles[i] = null;
                cwtCampaignCardDescriptions[i] = null;
            }
        }

        private void RefreshCWTCampaignBarsIfVisible()
        {
            if (cwtCampaignContentRoot == null ||
                !cwtCampaignContentRoot.gameObject.activeInHierarchy ||
                Time.unscaledTime < cwtCampaignNextRefresh) return;
            RefreshCWTCampaignBars(false);
        }

        private void RefreshCWTCampaignBars(bool force)
        {
            if (cwtCampaignContentRoot == null) return;
            if (!force && Time.unscaledTime < cwtCampaignNextRefresh) return;
            cwtCampaignNextRefresh = Time.unscaledTime + 0.4f;
            CWTCampaignProgressStore.SaveData s = CWTCampaignProgressStore.Current;
            double[] amounts = { s.GroundMeters, s.ExplosiveHits, s.CloseKills, s.TeleporterBosses };
            double[] goals = { CWTCampaignProgressStore.GroundGoal,
                CWTCampaignProgressStore.BlastGoal, CWTCampaignProgressStore.CloseGoal,
                CWTCampaignProgressStore.BossGoal };
            bool stageComplete = s.AqueductMeters >= CWTCampaignProgressStore.StageGoal &&
                s.DeltaMeters >= CWTCampaignProgressStore.StageGoal &&
                s.AbyssMeters >= CWTCampaignProgressStore.StageGoal;
            if (cwtCampaignCardDescriptions[0] != null)
                cwtCampaignCardDescriptions[0].text = (CWTCampaignTracker.IsSpanish
                    ? "100 km; 10 km/mapa. " : "100 km; 10 km/stage. ") +
                    string.Format(CultureInfo.InvariantCulture,
                        "AQ {0:0.#}/10 · DT {1:0.#}/10 · AB {2:0.#}/10 km",
                        s.AqueductMeters / 1000d, s.DeltaMeters / 1000d, s.AbyssMeters / 1000d);
            for (int i = 0; i < 4; i++)
            {
                double current = Math.Max(0d, Math.Min(goals[i], amounts[i]));
                bool done = current >= goals[i] && (i != 0 || stageComplete);
                // La barra del terreno no da un falso 100% si falta algún mapa.
                float fraction = (float)(current / goals[i]);
                if (i == 0 && !stageComplete) fraction = Mathf.Min(0.98f, fraction);
                RectTransform fill = cwtCampaignFills[i].rectTransform;
                fill.anchorMax = new Vector2(fraction, 1f);
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
                cwtCampaignFills[i].color = done ? CWTCheckGreen : CWTBarCyan;
                cwtCampaignValues[i].text = i == 0
                    ? (current / 1000d).ToString("0.0", CultureInfo.InvariantCulture) + " / 100 km"
                    : ((int)current).ToString("N0", CultureInfo.InvariantCulture) +
                      " / " + ((int)goals[i]).ToString("N0", CultureInfo.InvariantCulture);
                cwtCampaignValues[i].color = done ? CWTCheckGreen : Color.white;
                cwtCampaignChecks[i].text = done ? "✓" : "";
            }
            bool finalDone = s.FinalScavengerComplete;
            bool finalAvailable = CWTCampaignProgressStore.ObjectivesComplete;
            bool spanish = CWTCampaignTracker.IsSpanish;
            cwtCampaignFinalTitle.text = spanish ? "Operación final" : "Final operation";
            cwtCampaignFinalDescription.text = spanish
                ? (finalDone ? "Completada. Billy recuperado." :
                  "Derrota al Carroñero normal en Monzón o superior. " +
                  (finalAvailable ? "Disponible: cuatro frentes completados." :
                  "Bloqueada hasta completar los cuatro frentes."))
                : (finalDone ? "Complete. Billy recovered." :
                  "Defeat a normal Scavenger on Monsoon or higher. " +
                  (finalAvailable ? "Available: all four fronts complete." :
                  "Locked until all four fronts are complete."));
            cwtCampaignFinalCheck.text = finalDone ? "✓" : "";
            cwtCampaignFinalLock.text = "";
            cwtCampaignFinalLock.color = finalDone || finalAvailable
                ? CWTCheckGreen : CWTMuted;
        }
    }
}
