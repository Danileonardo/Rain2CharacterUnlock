using System;
using System.Text;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniversalSurvivorUnlocks
{
    // Plantilla visual de la biblioteca USU, estrenada y aprobada con AH-64.
    // Estas tarjetas se aplican a los demás survivors. No fabrican trackers:
    // la campaña real AH64 sigue renderizándose en AH64CampaignCards.cs.
    internal sealed partial class MissionLibraryRiskOfOptionsEmbeddedHost
    {
        private RectTransform survivorPresetContentRoot;
        private HGTextMeshProUGUI survivorPresetMissionText;
        private HGTextMeshProUGUI survivorPresetNotesTitle;
        private HGTextMeshProUGUI survivorPresetNotesText;
        private LayoutElement survivorPresetMissionLayout;
        private LayoutElement survivorPresetNotesLayout;

        private static string BuildPresetMissionSummaryText(SurvivorContentProfile profile)
        {
            if (profile == null) return "";

            // Resolver SIEMPRE la misión activa del provider elegido. Esto
            // preserva Original, USU, Custom y Generated, sus restricciones
            // y las localizaciones actuales; no crea/copias misiones nuevas.
            MissionDisplayInfo mission = ResolveMissionDisplay(profile);
            string name = mission.MissionName ?? "";
            string description = (mission.MissionDescription ?? "")
                .Replace("\r\n", " ").Replace("\n", " ")
                .Replace("\r", " ").Trim();

            StringBuilder builder = new StringBuilder();
            builder.Append("<size=18><b>");
            builder.Append(L(SurvivorLocalization.MissionLibraryBrowserUnlockMissionToken));
            builder.Append("</b></size>");

            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.Append("\n<size=17><b>");
                builder.Append(name);
                builder.Append("</b></size>");
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.Append("\n<size=14><color=#C0CDD9>");
                builder.Append(description);
                builder.Append("</color></size>");
            }
            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(description))
            {
                builder.Append("\n<size=14>");
                builder.Append(L(SurvivorLocalization.MissionLibraryNoActiveMissionToken));
                builder.Append("</size>");
            }
            if (!string.IsNullOrWhiteSpace(mission.Restrictions))
            {
                builder.Append("\n<size=14>");
                builder.Append(mission.Restrictions);
                builder.Append("</size>");
            }

            // Igual que en AH-64: Vista general sólo con identidad revelada.
            if (!profile.ShouldHideIdentity)
            {
                string overview = ModdedSurvivorLocalization.ResolveDescription(
                    profile, profile.Description ?? "");
                if (!string.IsNullOrWhiteSpace(overview))
                {
                    builder.Append("\n\n<size=15><b>");
                    builder.Append(L(SurvivorLocalization.MissionLibraryBrowserDescriptionToken));
                    builder.Append("</b></size>\n<size=13><color=#C0CDD9>");
                    builder.Append(overview);
                    builder.Append("</color></size>");
                }
            }
            return builder.ToString();
        }

        private void CreateSurvivorPresetCards(GameObject rowTemplate, Transform scrollParent)
        {
            GameObject root = new GameObject("USU Survivor Preset - Cards",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.transform.SetParent(scrollParent, false);
            survivorPresetContentRoot = root.GetComponent<RectTransform>();
            survivorPresetContentRoot.anchorMin = new Vector2(0f, 1f);
            survivorPresetContentRoot.anchorMax = new Vector2(1f, 1f);
            survivorPresetContentRoot.pivot = new Vector2(0.5f, 1f);
            survivorPresetContentRoot.anchoredPosition = new Vector2(0f, -2f);
            survivorPresetContentRoot.sizeDelta = new Vector2(-8f, 0f);

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

            // Tarjeta de misión / descripción: exactamente la composición
            // visual validada con AH-64, pero con datos del perfil actual.
            GameObject missionCard = AH64UIObject(
                "USU Preset - Unlock Mission Card", root.transform, true);
            AH64ColoredImage(missionCard, AH64DarkPanel);
            AH64Outline(missionCard, AH64PanelBorder);
            AH64AccentLine(missionCard.transform,
                "USU Preset - Mission Top Border", false, 2f, AH64PanelBorder);
            survivorPresetMissionLayout = missionCard.AddComponent<LayoutElement>();
            survivorPresetMissionLayout.preferredHeight = 80f;
            survivorPresetMissionText = AH64Text(
                "USU Preset - Mission and Overview", missionCard.transform,
                rowTemplate, 17, Color.white);
            AH64Rect(survivorPresetMissionText.rectTransform, Vector2.zero,
                Vector2.one, new Vector2(10f, 10f), new Vector2(-10f, -10f));

            // Notas nativas: se respetan los estados reales del Logbook.
            GameObject notesCard = AH64UIObject(
                "USU Preset - Logbook Card", root.transform, true);
            AH64ColoredImage(notesCard, AH64DarkPanel);
            AH64Outline(notesCard, AH64PanelBorder);
            survivorPresetNotesLayout = notesCard.AddComponent<LayoutElement>();
            survivorPresetNotesLayout.preferredHeight = 86f;
            survivorPresetNotesTitle = AH64Text(
                "USU Preset - Notes Heading", notesCard.transform,
                rowTemplate, 17, Color.white);
            AH64Rect(survivorPresetNotesTitle.rectTransform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(10f, -28f), new Vector2(-10f, -6f));
            survivorPresetNotesText = AH64Text(
                "USU Preset - Notes Text", notesCard.transform,
                rowTemplate, 16, AH64Muted);
            AH64Rect(survivorPresetNotesText.rectTransform,
                Vector2.zero, Vector2.one,
                new Vector2(10f, 8f), new Vector2(-10f, -28f));
            root.SetActive(false);
        }

        private void UpdateSurvivorPresetCards(SurvivorContentProfile profile)
        {
            if (profile == null || survivorPresetContentRoot == null ||
                survivorDetailsScrollRect == null) return;

            survivorDetailsText.gameObject.SetActive(false);
            survivorPresetContentRoot.gameObject.SetActive(true);
            survivorDetailsScrollRect.content = survivorPresetContentRoot;

            survivorPresetMissionText.text = BuildPresetMissionSummaryText(profile);
            survivorPresetNotesTitle.text =
                L(SurvivorLocalization.MissionLibraryBrowserNotesToken) + ":";
            survivorPresetNotesText.text =
                ResolveNotesText(profile, profile.ShouldHideIdentity);

            float width = survivorDetailsScrollRect.viewport != null
                ? survivorDetailsScrollRect.viewport.rect.width - 44f : 520f;
            survivorPresetMissionLayout.preferredHeight =
                Mathf.Max(80f, AH64TextPreferredHeight(
                    survivorPresetMissionText, width - 20f) + 4f);
            survivorPresetNotesLayout.preferredHeight =
                AH64TextPreferredHeight(survivorPresetNotesText, width - 20f) + 22f;
        }

        private void HideSurvivorPresetCards()
        {
            if (survivorPresetContentRoot != null)
                survivorPresetContentRoot.gameObject.SetActive(false);
        }

        private void ClearSurvivorPresetCardReferences()
        {
            survivorPresetContentRoot = null;
            survivorPresetMissionText = null;
            survivorPresetNotesTitle = null;
            survivorPresetNotesText = null;
            survivorPresetMissionLayout = null;
            survivorPresetNotesLayout = null;
        }
    }
}
