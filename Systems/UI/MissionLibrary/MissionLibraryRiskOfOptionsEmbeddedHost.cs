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
    /// <summary>
    /// 5G.2E-A FIX4 - Host embebido sin filas fantasma y sin colapsar las filas visibles
    /// de categoría de Risk of Options.
    ///
    /// RiskOfOptions sigue necesitando una GenericButtonOption por categoría
    /// para construir su panel nativo. En FIX2 esa opción también era la
    /// primera fila visible; al cambiar de pestaña, RiskOfOptions podía mostrar
    /// el ancla nueva durante unas décimas antes de que USU limpiara las filas
    /// de la página anterior.
    ///
    /// FIX4 conserva esas opciones únicamente como ANCLAS TÉCNICAS: permanecen
    /// activas para que RiskOfOptions mantenga su ciclo de vida, pero USU las
    /// vuelve transparentes, sin raycast y fuera del Layout en LateUpdate.
    /// Todas las filas visibles, incluida la primera, son copias administradas
    /// por USU. Así no aparece "Presets asignables", "Catálogo runtime" ni otra
    /// fila de la página entrante debajo de la página anterior.
    ///
    /// No se crea Canvas, ventana OnGUI ni overlay externo.
    /// </summary>
    internal sealed class MissionLibraryRiskOfOptionsEmbeddedHost : MonoBehaviour
    {
        private const string GenericButtonObjectPrefix =
            "Mod Option GenericButton, ";

        private ManualLogSource logger;
        private bool initialized;
        private PageState activeState;
        private GameObject activeAnchor;

        private readonly Dictionary<string, PageState> pageStates =
            new Dictionary<string, PageState>(StringComparer.Ordinal);

        // 5G.2E-B
        // El navegador de personajes vive dentro del panel nativo de
        // Risk of Options. No se crea Canvas/OnGUI/overlay externo.
        private SurvivorContentProfile selectedSurvivorProfile;
        private readonly Dictionary<string, RectTransform> survivorBrowserRows =
            new Dictionary<string, RectTransform>(StringComparer.OrdinalIgnoreCase);
        private GameObject survivorDetailsBackground;
        private GameObject survivorDetailsRoot;
        private RawImage survivorDetailsPortrait;
        private HGTextMeshProUGUI survivorDetailsQuestion;
        private HGTextMeshProUGUI survivorDetailsHeaderText;
        private HGTextMeshProUGUI survivorDetailsText;
        private ScrollRect survivorDetailsScrollRect;

        // UI/log cleanup: evita reconstrucciones informativas repetidas
        // dentro de la misma instancia de Mod Options.
        private bool survivorDiscoveryRefreshedForUiSession;

        // El estado de unlock puede cambiar fuera de USU (por ejemplo con
        // right-click unlock/relock). El Browser compara el UserProfile real
        // a baja frecuencia y sólo se reconstruye cuando detecta un cambio.
        private float nextSurvivorDiscoveryCheckTime;
        private const float SurvivorDiscoveryCheckInterval = 0.25f;

        private enum EmbeddedPage
        {
            General,
            Survivors,
            Missions,
            Advanced
        }


        private sealed class PageState
        {
            public string MarkerName;
            public EmbeddedPage Page;
            public GameObject MarkerObject;
            public readonly List<GameObject> GeneratedRows =
                new List<GameObject>();
            public bool Logged;
        }


        private sealed class DisplayRow
        {
            public string Label;
            public string Value;

            public DisplayRow(
                string label,
                string value
            )
            {
                Label = label ?? "";
                Value = value ?? "";
            }
        }


        public void Initialize(
            ManualLogSource activeLogger
        )
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            logger = activeLogger;

            RegisterPage(
                MissionLibraryRiskOfOptionsIntegration.GeneralAnchorName,
                EmbeddedPage.General
            );

            RegisterPage(
                MissionLibraryRiskOfOptionsIntegration.SurvivorsAnchorName,
                EmbeddedPage.Survivors
            );

            RegisterPage(
                MissionLibraryRiskOfOptionsIntegration.MissionsAnchorName,
                EmbeddedPage.Missions
            );

            RegisterPage(
                MissionLibraryRiskOfOptionsIntegration.AdvancedAnchorName,
                EmbeddedPage.Advanced
            );

        }


        /// <summary>
        /// Se usa LateUpdate deliberadamente. RiskOfOptions reconstruye sus
        /// opciones durante el frame del cambio de categoría; al ejecutar al
        /// final del frame ocultamos el ancla nueva y retiramos las filas de la
        /// página anterior antes del render.
        /// </summary>
        private void LateUpdate()
        {
            if (!initialized)
            {
                return;
            }

            PageState incomingState = null;
            GameObject incomingAnchor = null;
            PageState currentStateStillPresent = null;
            GameObject currentAnchorStillPresent = null;

            foreach (
                KeyValuePair<string, PageState> pair
                in pageStates
            )
            {
                PageState state = pair.Value;

                if (state == null)
                {
                    continue;
                }

                GameObject anchor =
                    GameObject.Find(
                        GenericButtonObjectPrefix +
                        state.MarkerName
                    );

                if (anchor == null)
                {
                    state.MarkerObject = null;
                    continue;
                }

                bool newAnchorInstance =
                    state.MarkerObject == null ||
                    state.MarkerObject != anchor;

                state.MarkerObject = anchor;

                // Siempre ocultamos el objeto nativo ANTES de decidir qué
                // página construir. Se mantiene activo para RiskOfOptions,
                // pero jamás debe participar visualmente en el layout.
                HideNativeAnchor(anchor);

                if (state == activeState)
                {
                    currentStateStillPresent = state;
                    currentAnchorStillPresent = anchor;
                }

                // Durante una transición pueden coexistir durante un frame el
                // ancla vieja y la nueva. La nueva instancia gana prioridad.
                if (
                    state != activeState &&
                    newAnchorInstance
                )
                {
                    incomingState = state;
                    incomingAnchor = anchor;
                }
                else if (incomingState == null && activeState == null)
                {
                    incomingState = state;
                    incomingAnchor = anchor;
                }
            }

            PageState desiredState =
                incomingState ?? currentStateStillPresent;

            GameObject desiredAnchor =
                incomingAnchor ?? currentAnchorStillPresent;

            if (
                desiredState == null ||
                desiredAnchor == null
            )
            {
                if (activeState != null)
                {
                    DestroyAllGeneratedRows();
                }

                DestroySurvivorDetailsPanel();

                activeState = null;
                activeAnchor = null;
                return;
            }

            bool survivorDiscoveryChanged = false;

            if (
                desiredState.Page == EmbeddedPage.Survivors &&
                Time.unscaledTime >= nextSurvivorDiscoveryCheckTime
            )
            {
                nextSurvivorDiscoveryCheckTime =
                    Time.unscaledTime +
                    SurvivorDiscoveryCheckInterval;

                try
                {
                    survivorDiscoveryChanged =
                        SurvivorContentProfileService
                            .RefreshDiscoveryIfChanged();
                }
                catch
                {
                    survivorDiscoveryChanged = false;
                }
            }

            bool pageChanged =
                desiredState != activeState ||
                desiredAnchor != activeAnchor;

            bool rowsMissing =
                !AreGeneratedRowsAlive(
                    desiredState
                );

            if (
                pageChanged ||
                rowsMissing ||
                survivorDiscoveryChanged
            )
            {
                // Limpieza GLOBAL: evita que las filas de Personajes convivan
                // durante un frame con el ancla de Misiones, etc.
                DestroyAllGeneratedRows();

                activeState = desiredState;
                activeAnchor = desiredAnchor;

                BuildPageRows(
                    desiredState
                );
            }
            else
            {
                HideNativeAnchor(
                    desiredAnchor
                );
            }

        }


        private void OnDestroy()
        {
            foreach (
                KeyValuePair<string, PageState> pair
                in pageStates
            )
            {
                DestroyGeneratedRows(
                    pair.Value
                );
            }

            DestroySurvivorDetailsPanel();
            pageStates.Clear();
        }


        private void RegisterPage(
            string markerName,
            EmbeddedPage page
        )
        {
            pageStates[markerName] =
                new PageState
                {
                    MarkerName = markerName,
                    Page = page
                };
        }


        private static bool AreGeneratedRowsAlive(
            PageState state
        )
        {
            if (
                state == null ||
                state.GeneratedRows.Count == 0
            )
            {
                return false;
            }

            for (
                int i = 0;
                i < state.GeneratedRows.Count;
                i++
            )
            {
                if (state.GeneratedRows[i] == null)
                {
                    return false;
                }
            }

            return true;
        }


        private void BuildPageRows(
            PageState state
        )
        {
            if (
                state == null ||
                state.MarkerObject == null ||
                state.MarkerObject.transform == null ||
                state.MarkerObject.transform.parent == null
            )
            {
                return;
            }

            Transform parent =
                state.MarkerObject.transform.parent;

            if (state.Page == EmbeddedPage.Survivors)
            {
                BuildSurvivorBrowserRows(
                    state,
                    parent
                );

                return;
            }

            DestroySurvivorDetailsPanel();

            List<DisplayRow> rows =
                GetRowsForPage(
                    state.Page
                );

            if (
                rows == null ||
                rows.Count == 0
            )
            {
                return;
            }

            int siblingIndex =
                state.MarkerObject.transform.GetSiblingIndex() + 1;

            // El ancla nativa queda completamente oculta. TODAS las filas
            // visibles, incluida rows[0], son copias administradas por USU.
            for (
                int i = 0;
                i < rows.Count;
                i++
            )
            {
                GameObject row =
                    CreateDisplayRow(
                        state.MarkerObject,
                        parent,
                        rows[i],
                        state.Page,
                        i
                    );

                if (row == null)
                {
                    continue;
                }

                row.transform.SetSiblingIndex(
                    siblingIndex
                );

                siblingIndex++;

                state.GeneratedRows.Add(
                    row
                );
            }

            HideNativeAnchor(
                state.MarkerObject
            );

            if (!state.Logged)
            {
                state.Logged = true;

                logger?.LogInfo(
                    "[MISSION LIBRARY UI] 5G.2E-B página embebida creada | " +
                    "Página: " +
                    GetPageDebugName(state.Page) +
                    " | Filas: " +
                    rows.Count +
                    " | Parent: " +
                    parent.name
                );
            }
        }


        /// <summary>
        /// Mantiene la GenericButtonOption nativa como ancla técnica sin
        /// destruirla. No ocupa espacio, no recibe input y no puede dibujarse.
        /// Se ejecuta cada LateUpdate para cubrir reconstrucciones de RoO.
        /// </summary>
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
            // Portrait: y -16 .. -148. Dejamos sólo 8 px antes de la
            // sección de misión para que el header se lea como un bloque.
            scrollRectTransform.offsetMax = new Vector2(-16f, -156f);

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


        private void UpdateSurvivorDetailsPanel(
            SurvivorContentProfile profile
        )
        {
            if (
                profile == null ||
                survivorDetailsRoot == null ||
                survivorDetailsHeaderText == null ||
                survivorDetailsText == null ||
                survivorDetailsPortrait == null
            )
            {
                return;
            }

            SurvivorLockedPortraitManager.ApplyBrowserPortrait(
                survivorDetailsPortrait,
                profile
            );

            if (survivorDetailsQuestion != null)
            {
                survivorDetailsQuestion.text =
                    profile.Portrait == null
                        ? "?"
                        : "";
            }

            survivorDetailsHeaderText.text =
                BuildSurvivorDetailsHeaderText(profile);

            survivorDetailsText.text =
                BuildSurvivorDetailsText(profile);

            RectTransform detailsTextRect =
                survivorDetailsText.rectTransform;

            if (detailsTextRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    detailsTextRect
                );
            }

            Canvas.ForceUpdateCanvases();

            if (survivorDetailsScrollRect != null)
            {
                survivorDetailsScrollRect.verticalNormalizedPosition = 1f;
            }
        }


        private static string BuildSurvivorDetailsHeaderText(
            SurvivorContentProfile profile
        )
        {
            if (profile == null)
            {
                return "";
            }

            bool hidden = profile.ShouldHideIdentity;

            string name =
                hidden
                    ? "???"
                    : GetSurvivorBrowserLabel(profile);

            string subtitle =
                hidden
                    ? ""
                    : ModdedSurvivorLocalization.ResolveSubtitle(
                        profile,
                        profile.Subtitle ?? ""
                    );

            string status =
                GetDiscoveryLabel(profile.Discovery);

            string source =
                hidden
                    ? ""
                    : GetProfileSourceLabel(profile);

            MissionDisplayInfo mission =
                ResolveMissionDisplay(profile);

            System.Text.StringBuilder builder =
                new System.Text.StringBuilder();

            builder.Append("<b><size=25>");
            builder.Append(name);
            builder.Append("</size></b>");

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                builder.Append("\n<size=16>");
                builder.Append(subtitle);
                builder.Append("</size>");
            }

            // Estado / provider / fuente pertenecen a la identidad de la
            // ficha, no al cuerpo de la misión. Se mantienen bajo el título
            // y terminan aproximadamente a la altura inferior del portrait.
            builder.Append("\n\n<size=14><b>");
            builder.Append(
                L(
                    SurvivorLocalization
                        .MissionLibraryBrowserStatusToken
                )
            );
            builder.Append(":</b> ");
            builder.Append(status);

            builder.Append("\n<b>");
            builder.Append(
                L(
                    SurvivorLocalization
                        .MissionLibraryProviderSectionToken
                )
            );
            builder.Append(":</b> ");
            builder.Append(mission.ProviderLabel);

            if (!string.IsNullOrWhiteSpace(source))
            {
                builder.Append("\n<b>");
                builder.Append(
                    L(
                        SurvivorLocalization
                            .MissionLibraryBrowserSourceToken
                    )
                );
                builder.Append(":</b> ");
                builder.Append(source);
            }

            builder.Append("</size>");

            return builder.ToString();
        }

        private static string BuildSurvivorDetailsText(
            SurvivorContentProfile profile
        )
        {
            if (profile == null)
            {
                return "";
            }

            bool hidden = profile.ShouldHideIdentity;

            MissionDisplayInfo mission =
                ResolveMissionDisplay(profile);

            System.Text.StringBuilder builder =
                new System.Text.StringBuilder();

            AppendSectionTitle(
                builder,
                L(
                    SurvivorLocalization
                        .MissionLibraryBrowserUnlockMissionToken
                )
            );

            if (!string.IsNullOrWhiteSpace(mission.MissionName))
            {
                builder.Append("\n<b>");
                builder.Append(mission.MissionName);
                builder.Append("</b>");
            }

            if (!string.IsNullOrWhiteSpace(mission.MissionDescription))
            {
                builder.Append("\n");
                builder.Append(mission.MissionDescription);
            }

            if (
                string.IsNullOrWhiteSpace(mission.MissionName) &&
                string.IsNullOrWhiteSpace(mission.MissionDescription)
            )
            {
                builder.Append("\n");
                builder.Append(
                    L(
                        SurvivorLocalization
                            .MissionLibraryNoActiveMissionToken
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(mission.Restrictions))
            {
                builder.Append("\n");
                builder.Append(mission.Restrictions);
            }

            string localizedDescription =
                hidden
                    ? ""
                    : ModdedSurvivorLocalization.ResolveDescription(
                        profile,
                        profile.Description ?? ""
                    );

            if (
                !hidden &&
                !string.IsNullOrWhiteSpace(localizedDescription)
            )
            {
                AppendSectionTitle(
                    builder,
                    L(
                        SurvivorLocalization
                            .MissionLibraryBrowserDescriptionToken
                    )
                );

                builder.Append("\n");
                builder.Append(localizedDescription);
            }

            AppendSectionTitle(
                builder,
                L(
                    SurvivorLocalization
                        .MissionLibraryBrowserNotesToken
                )
            );

            builder.Append("\n");
            builder.Append(
                ResolveNotesText(
                    profile,
                    hidden
                )
            );

            return builder.ToString();
        }


        private static void AppendSectionTitle(
            System.Text.StringBuilder builder,
            string title
        )
        {
            if (builder == null)
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append("\n\n");
            }

            builder.Append("<size=18><b>");
            builder.Append(title ?? "");
            builder.Append("</b></size>");
        }


        private static string ResolveNotesText(
            SurvivorContentProfile profile,
            bool hidden
        )
        {
            if (profile == null)
            {
                return "???";
            }

            if (hidden)
            {
                return
                    L(
                        SurvivorLocalization
                            .MissionLibraryBrowserNotesLockedToken
                    );
            }

            SurvivorLogbookResolver.Result logbook =
                SurvivorLogbookResolver.Resolve(profile);

            if (
                logbook != null &&
                logbook.CanRevealLore
            )
            {
                string localizedLore =
                    ModdedSurvivorLocalization.ResolveLore(
                        profile,
                        profile.Lore ?? ""
                    );

                return
                    !string.IsNullOrWhiteSpace(localizedLore)
                        ? localizedLore
                        : L(
                            SurvivorLocalization
                                .MissionLibraryBrowserNotesUnavailableToken
                        );
            }

            if (
                logbook != null &&
                logbook.State ==
                    SurvivorLogbookResolver.NativeEntryState.Missing
            )
            {
                // El fallback sólo existe para contenido modded que realmente
                // no publica una Entry nativa. En vanilla/DLC un Missing nunca
                // debe convertirse en permiso para revelar lore: si el match
                // fallara por una diferencia de versión, preferimos ocultarlo.
                bool isModded =
                    profile.SurvivorInfo != null &&
                    profile.SurvivorInfo.IsModded;

                if (isModded)
                {
                    // Algunos mods aportan un token *_LORE aunque no creen una
                    // Entry de Logbook. Ese texto sigue siendo del creador.
                    string localizedLore =
                        ModdedSurvivorLocalization.ResolveLore(
                            profile,
                            profile.Lore ?? ""
                        );

                    if (!string.IsNullOrWhiteSpace(localizedLore))
                    {
                        return localizedLore;
                    }

                    return
                        L(
                            SurvivorLocalization
                                .MissionLibraryBrowserNotesFallbackDemoToken
                        );
                }

                return
                    L(
                        SurvivorLocalization
                            .MissionLibraryBrowserNotesUnavailableToken
                    );
            }

            // Locked y Unknown se mantienen ocultos. Unknown es importante
            // para no revelar lore si el Logbook aún no terminó de construir
            // sus Entry cuando se abre Mod Options.
            return
                L(
                    SurvivorLocalization
                        .MissionLibraryBrowserNotesLockedToken
                );
        }


        private sealed class MissionDisplayInfo
        {
            public string ProviderLabel = "";
            public string MissionName = "";
            public string MissionDescription = "";
            public string Restrictions = "";
        }


        private static MissionDisplayInfo ResolveMissionDisplay(
            SurvivorContentProfile profile
        )
        {
            MissionDisplayInfo result =
                new MissionDisplayInfo();

            if (profile == null)
            {
                return result;
            }

            SurvivorContentUnlockInfo unlock =
                profile.SurvivorUnlock;

            if (
                unlock != null &&
                unlock.Source ==
                    SurvivorContentUnlockSource.UnlockedByDefault
            )
            {
                result.ProviderLabel =
                    L(
                        SurvivorLocalization
                            .MissionLibraryProviderOriginalToken
                    );

                result.MissionName =
                    L(
                        SurvivorLocalization
                            .MissionLibraryBrowserUnlockedByDefaultToken
                    );

                result.MissionDescription =
                    L(
                        SurvivorLocalization
                            .MissionLibraryBrowserNoMissionRequiredToken
                    );

                return result;
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    profile.BodyName
                );

            UnlockProviderKind provider =
                GetEffectiveProvider(
                    entry,
                    profile
                );

            result.ProviderLabel =
                GetProviderLabel(provider);

            if (provider == UnlockProviderKind.Original)
            {
                if (unlock != null)
                {
                    result.MissionName =
                        ModdedSurvivorLocalization.ResolveOriginalMissionName(
                            profile,
                            unlock.MissionName ?? ""
                        );

                    result.MissionDescription =
                        ModdedSurvivorLocalization.ResolveOriginalMissionDescription(
                            profile,
                            unlock.MissionDescription ?? ""
                        );
                }

                if (
                    string.IsNullOrWhiteSpace(result.MissionName) &&
                    string.IsNullOrWhiteSpace(result.MissionDescription)
                )
                {
                    result.MissionName =
                        L(
                            SurvivorLocalization
                                .MissionLibraryOriginalMissionTitleToken
                        );

                    result.MissionDescription =
                        L(
                            SurvivorLocalization
                                .MissionLibraryOriginalMissionDescriptionToken
                        );
                }

                return result;
            }

            SurvivorChallengeJson challenge =
                entry?.Challenge;

            if (challenge != null)
            {
                result.MissionName =
                    ResolveChallengeName(
                        profile.BodyName,
                        challenge
                    );

                result.MissionDescription =
                    ResolveChallengeDescription(
                        profile.BodyName,
                        challenge
                    );

                result.Restrictions =
                    ResolveMissionRestrictions(
                        ResolveActiveMissionDefinition(
                            challenge
                        )
                    );
            }

            if (
                string.IsNullOrWhiteSpace(result.MissionName) &&
                unlock != null
            )
            {
                result.MissionName =
                    ModdedSurvivorLocalization.ResolveOriginalMissionName(
                        profile,
                        unlock.MissionName ?? ""
                    );
            }

            if (
                string.IsNullOrWhiteSpace(result.MissionDescription) &&
                unlock != null
            )
            {
                result.MissionDescription =
                    ModdedSurvivorLocalization.ResolveOriginalMissionDescription(
                        profile,
                        unlock.MissionDescription ?? ""
                    );
            }

            return result;
        }


        private static MissionDefinition ResolveActiveMissionDefinition(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return null;
            }

            MissionConfiguration config =
                challenge.MissionConfig;

            if (
                config != null &&
                config.UsesCustomMission
            )
            {
                return config.CustomMission;
            }

            if (
                config != null &&
                string.Equals(
                    config.Source,
                    "Preset",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                MissionPresetLibraryService.TryGetAssignableMissionPreset(
                    config.BasePresetId,
                    out MissionPreset preset
                ) &&
                preset != null &&
                preset.Mission != null
            )
            {
                return preset.Mission;
            }

            return challenge.Mission;
        }


        private static string ResolveMissionRestrictions(
            MissionDefinition mission
        )
        {
            if (
                mission == null ||
                mission.Routes == null ||
                mission.Routes.Count == 0
            )
            {
                return "";
            }

            List<string> excludedBodies =
                new List<string>();

            for (int routeIndex = 0;
                routeIndex < mission.Routes.Count;
                routeIndex++)
            {
                MissionRoute route = mission.Routes[routeIndex];

                if (
                    route == null ||
                    route.Conditions == null
                )
                {
                    continue;
                }

                for (int conditionIndex = 0;
                    conditionIndex < route.Conditions.Count;
                    conditionIndex++)
                {
                    MissionCondition condition =
                        route.Conditions[conditionIndex];

                    if (
                        condition == null ||
                        !string.Equals(
                            condition.Type,
                            "ExcludedSurvivor",
                            StringComparison.OrdinalIgnoreCase
                        ) ||
                        condition.Parameters == null
                    )
                    {
                        continue;
                    }

                    JToken bodiesToken =
                        condition.Parameters["bodies"];

                    if (bodiesToken is JArray bodies)
                    {
                        for (int i = 0; i < bodies.Count; i++)
                        {
                            AddExcludedBodyLabel(
                                excludedBodies,
                                bodies[i]?.ToString()
                            );
                        }
                    }
                    else
                    {
                        AddExcludedBodyLabel(
                            excludedBodies,
                            condition.Parameters["body"]?.ToString()
                        );
                    }
                }
            }

            if (excludedBodies.Count == 0)
            {
                return "";
            }

            string format =
                L(
                    SurvivorLocalization
                        .MissionLibraryNotValidWithToken
                );

            return string.Format(
                format,
                string.Join(" / ", excludedBodies)
            );
        }


        private static void AddExcludedBodyLabel(
            List<string> labels,
            string bodyName
        )
        {
            if (
                labels == null ||
                string.IsNullOrWhiteSpace(bodyName)
            )
            {
                return;
            }

            string label = bodyName;

            if (
                SurvivorContentProfileService.TryGetByBodyName(
                    bodyName,
                    out SurvivorContentProfile profile
                ) &&
                profile != null
            )
            {
                if (!string.IsNullOrWhiteSpace(profile.DisplayName))
                {
                    label = profile.DisplayName;
                }
                else if (!string.IsNullOrWhiteSpace(profile.InternalName))
                {
                    label = profile.InternalName;
                }
            }

            for (int i = 0; i < labels.Count; i++)
            {
                if (
                    string.Equals(
                        labels[i],
                        label,
                        StringComparison.CurrentCultureIgnoreCase
                    )
                )
                {
                    return;
                }
            }

            labels.Add(label);
        }


        private static UnlockProviderKind GetEffectiveProvider(
            SurvivorJsonEntry entry,
            SurvivorContentProfile profile
        )
        {
            if (
                profile?.SurvivorUnlock != null &&
                profile.SurvivorUnlock.Source ==
                    SurvivorContentUnlockSource.UnlockedByDefault
            )
            {
                return UnlockProviderKind.Original;
            }

            MissionConfiguration missionConfig =
                entry?.Challenge?.MissionConfig;

            if (missionConfig != null)
            {
                if (
                    missionConfig.EffectiveSelectionMode ==
                        UnlockSelectionMode.AutomaticFallback &&
                    entry != null &&
                    SurvivorUnlockManager.HasStoredOriginalUnlock(entry)
                )
                {
                    return UnlockProviderKind.Original;
                }

                return missionConfig.EffectiveProvider;
            }

            if (
                entry != null &&
                SurvivorUnlockManager.HasStoredOriginalUnlock(entry)
            )
            {
                return UnlockProviderKind.Original;
            }

            if (
                profile?.SurvivorUnlock != null &&
                profile.SurvivorUnlock.Source ==
                    SurvivorContentUnlockSource.Original
            )
            {
                return UnlockProviderKind.Original;
            }

            return UnlockProviderKind.USU;
        }


        private static string GetProviderLabel(
            UnlockProviderKind provider
        )
        {
            switch (provider)
            {
                case UnlockProviderKind.Original:
                    return L(
                        SurvivorLocalization
                            .MissionLibraryProviderOriginalToken
                    );

                case UnlockProviderKind.Custom:
                    return L(
                        SurvivorLocalization
                            .MissionLibraryProviderCustomToken
                    );

                case UnlockProviderKind.Community:
                    return L(
                        SurvivorLocalization
                            .MissionLibraryProviderCommunityToken
                    );

                default:
                    return L(
                        SurvivorLocalization
                            .MissionLibraryProviderUsuToken
                    );
            }
        }


        private static string ResolveChallengeName(
            string bodyName,
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return "";
            }

            if (
                SurvivorLocalization.UsesBuiltInLocalization(
                    bodyName,
                    challenge
                ) &&
                SurvivorLocalization.TryGetOfficialMissionTokens(
                    bodyName,
                    out string ownNameToken,
                    out string ownDescriptionToken
                )
            )
            {
                return Language.GetString(ownNameToken);
            }

            MissionConfiguration missionConfig =
                challenge.MissionConfig;

            if (
                missionConfig != null &&
                string.Equals(
                    missionConfig.Source,
                    "Preset",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                MissionPresetLibraryService.TryGetAssignableMissionPreset(
                    missionConfig.BasePresetId,
                    out MissionPreset preset
                ) &&
                preset != null
            )
            {
                return ResolvePresetName(preset);
            }

            return challenge.Name ?? "";
        }


        private static string ResolveChallengeDescription(
            string bodyName,
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return "";
            }

            if (
                SurvivorLocalization.UsesBuiltInLocalization(
                    bodyName,
                    challenge
                ) &&
                SurvivorLocalization.TryGetOfficialMissionTokens(
                    bodyName,
                    out string ownNameToken,
                    out string ownDescriptionToken
                )
            )
            {
                return Language.GetString(ownDescriptionToken);
            }

            MissionConfiguration missionConfig =
                challenge.MissionConfig;

            if (
                missionConfig != null &&
                string.Equals(
                    missionConfig.Source,
                    "Preset",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                MissionPresetLibraryService.TryGetAssignableMissionPreset(
                    missionConfig.BasePresetId,
                    out MissionPreset preset
                ) &&
                preset != null
            )
            {
                return ResolvePresetDescription(preset);
            }

            return challenge.Description ?? "";
        }


        private static string ResolvePresetName(
            MissionPreset preset
        )
        {
            if (preset == null)
            {
                return "";
            }

            if (
                string.Equals(
                    preset.Category,
                    MissionPresetCategories.CharacterRecipe,
                    StringComparison.OrdinalIgnoreCase
                ) &&
                SurvivorLocalization.TryGetOfficialMissionTokens(
                    preset.TargetBody,
                    out string nameToken,
                    out string descriptionToken
                )
            )
            {
                return Language.GetString(nameToken);
            }

            return
                !string.IsNullOrWhiteSpace(preset.Name)
                    ? preset.Name
                    : preset.PresetId ?? "";
        }


        private static string ResolvePresetDescription(
            MissionPreset preset
        )
        {
            if (preset == null)
            {
                return "";
            }

            if (
                string.Equals(
                    preset.Category,
                    MissionPresetCategories.CharacterRecipe,
                    StringComparison.OrdinalIgnoreCase
                ) &&
                SurvivorLocalization.TryGetOfficialMissionTokens(
                    preset.TargetBody,
                    out string nameToken,
                    out string descriptionToken
                )
            )
            {
                return Language.GetString(descriptionToken);
            }

            return preset.Description ?? "";
        }


        private static string GetDiscoveryLabel(
            ContentCatalogDiscoveryState discovery
        )
        {
            switch (discovery)
            {
                case ContentCatalogDiscoveryState.Undiscovered:
                    return L(
                        SurvivorLocalization
                            .MissionLibraryBrowserLockedToken
                    );

                case ContentCatalogDiscoveryState.Discovered:
                case ContentCatalogDiscoveryState.NotApplicable:
                    return L(
                        SurvivorLocalization
                            .MissionLibraryBrowserUnlockedToken
                    );

                default:
                    return L(
                        SurvivorLocalization
                            .MissionLibraryBrowserUnknownToken
                    );
            }
        }


        private static string GetProfileSourceLabel(
            SurvivorContentProfile profile
        )
        {
            if (profile == null)
            {
                return "";
            }

            if (!string.IsNullOrWhiteSpace(profile.SourceAssembly))
            {
                return profile.SourceAssembly;
            }

            if (!string.IsNullOrWhiteSpace(profile.SourceIdentifier))
            {
                return profile.SourceIdentifier;
            }

            return "Risk of Rain 2";
        }


        private static SurvivorContentProfile ChooseInitialSurvivorProfile(
            List<SurvivorContentProfile> profiles
        )
        {
            if (
                profiles == null ||
                profiles.Count == 0
            )
            {
                return null;
            }

            for (int i = 0; i < profiles.Count; i++)
            {
                if (
                    profiles[i] != null &&
                    !profiles[i].ShouldHideIdentity
                )
                {
                    return profiles[i];
                }
            }

            return profiles[0];
        }


        private static SurvivorContentProfile FindProfileByBodyName(
            List<SurvivorContentProfile> profiles,
            string bodyName
        )
        {
            if (
                profiles == null ||
                string.IsNullOrWhiteSpace(bodyName)
            )
            {
                return null;
            }

            for (int i = 0; i < profiles.Count; i++)
            {
                SurvivorContentProfile profile = profiles[i];

                if (
                    profile != null &&
                    string.Equals(
                        profile.BodyName,
                        bodyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return profile;
                }
            }

            return null;
        }


        private static int CompareSurvivorProfiles(
            SurvivorContentProfile left,
            SurvivorContentProfile right
        )
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            int sourceCompare =
                GetSurvivorSourceOrder(left)
                    .CompareTo(
                        GetSurvivorSourceOrder(right)
                    );

            if (sourceCompare != 0)
            {
                return sourceCompare;
            }

            return string.Compare(
                GetSurvivorBrowserLabel(left),
                GetSurvivorBrowserLabel(right),
                StringComparison.CurrentCultureIgnoreCase
            );
        }


        private static int GetSurvivorSourceOrder(
            SurvivorContentProfile profile
        )
        {
            if (profile == null)
            {
                return 99;
            }

            ContentCatalogEntry entry;

            if (
                ContentCatalogService.TryGet(
                    "survivor:" + profile.BodyName,
                    out entry
                ) &&
                entry?.Source != null
            )
            {
                switch (entry.Source.Kind)
                {
                    case ContentCatalogSourceKind.Vanilla:
                        return 0;

                    case ContentCatalogSourceKind.Dlc:
                        return 1;

                    case ContentCatalogSourceKind.Mod:
                        return 2;

                    case ContentCatalogSourceKind.Usu:
                        return 3;
                }
            }

            string source =
                (profile.SourceIdentifier ?? "") + " " +
                (profile.SourceAssembly ?? "");

            if (
                source.IndexOf(
                    "DLC",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0
            )
            {
                return 1;
            }

            if (
                source.IndexOf(
                    "RoR2",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0
            )
            {
                return 0;
            }

            return 2;
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
        }


        private static List<DisplayRow> GetRowsForPage(
            EmbeddedPage page
        )
        {
            switch (page)
            {
                case EmbeddedPage.General:
                    return BuildGeneralRows();

                case EmbeddedPage.Survivors:
                    return BuildSurvivorRows();

                case EmbeddedPage.Missions:
                    return BuildMissionRows();

                case EmbeddedPage.Advanced:
                    return BuildAdvancedRows();

                default:
                    return new List<DisplayRow>();
            }
        }


        private static List<DisplayRow> BuildGeneralRows()
        {
            return new List<DisplayRow>
            {
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedTitleToken,
                    "v0.2.0"
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedInterfaceToken,
                    "Risk of Options"
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedLibraryToken,
                    L(
                        SurvivorLocalization.MissionLibraryEmbeddedIntegratedToken
                    )
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedExternalWindowToken,
                    L(
                        SurvivorLocalization.MissionLibraryEmbeddedDisabledToken
                    )
                )
            };
        }


        private static List<DisplayRow> BuildSurvivorRows()
        {
            ProfileStats stats =
                CollectProfileStats();

            return new List<DisplayRow>
            {
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedProfilesToken,
                    stats.Profiles.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedLoreToken,
                    stats.LoreResolved.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedSkillGroupsToken,
                    stats.SkillGroups.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedSkillVariantsToken,
                    stats.SkillVariants.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedLockedSkillsToken,
                    stats.LockedSkills.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedSkinsToken,
                    stats.Skins.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedLockedSkinsToken,
                    stats.LockedSkins.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedVisualBrowserToken,
                    L(
                        SurvivorLocalization.MissionLibraryEmbeddedNextBrowserToken
                    )
                )
            };
        }


        private static List<DisplayRow> BuildMissionRows()
        {
            int presetCount = 0;

            try
            {
                List<MissionPreset> presets =
                    MissionPresetLibraryService
                        .GetAssignableMissionPresets();

                if (presets != null)
                {
                    presetCount =
                        presets.Count;
                }
            }
            catch
            {
                presetCount = 0;
            }

            return new List<DisplayRow>
            {
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedAssignablePresetsToken,
                    presetCount.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedProvidersToken,
                    "Original / USU / Custom"
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedLibraryToken,
                    L(
                        SurvivorLocalization.MissionLibraryEmbeddedIntegratedToken
                    )
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedCustomToken,
                    L(
                        SurvivorLocalization.MissionLibraryEmbeddedSoon5HToken
                    )
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedEditorToken,
                    L(
                        SurvivorLocalization.MissionLibraryEmbeddedSoon5IToken
                    )
                )
            };
        }


        private static List<DisplayRow> BuildAdvancedRows()
        {
            int unknownDiscovery = 0;
            int discovered = 0;
            int undiscovered = 0;

            IReadOnlyList<ContentCatalogEntry> all =
                ContentCatalogService.GetAll();

            if (all != null)
            {
                for (
                    int i = 0;
                    i < all.Count;
                    i++
                )
                {
                    ContentCatalogEntry entry =
                        all[i];

                    if (entry == null)
                    {
                        continue;
                    }

                    switch (entry.Discovery)
                    {
                        case ContentCatalogDiscoveryState.Unknown:
                            unknownDiscovery++;
                            break;

                        case ContentCatalogDiscoveryState.Discovered:
                            discovered++;
                            break;

                        case ContentCatalogDiscoveryState.Undiscovered:
                            undiscovered++;
                            break;
                    }
                }
            }

            return new List<DisplayRow>
            {
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedRuntimeCatalogToken,
                    ContentCatalogService.Count.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedDetectedSurvivorsToken,
                    CountCatalog(ContentCatalogKind.Survivor).ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedItemsToken,
                    CountCatalog(ContentCatalogKind.Item).ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedEquipmentToken,
                    CountCatalog(ContentCatalogKind.Equipment).ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedEnemiesToken,
                    CountCatalog(ContentCatalogKind.Enemy).ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedBossesToken,
                    CountCatalog(ContentCatalogKind.Boss).ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedStagesToken,
                    CountCatalog(ContentCatalogKind.Stage).ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedDiscoveredToken,
                    discovered.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedUndiscoveredToken,
                    undiscovered.ToString()
                ),
                Row(
                    SurvivorLocalization.MissionLibraryEmbeddedUnknownDiscoveryToken,
                    unknownDiscovery.ToString()
                )
            };
        }


        private static int CountCatalog(
            ContentCatalogKind kind
        )
        {
            IReadOnlyList<ContentCatalogEntry> entries =
                ContentCatalogService.GetEntries(
                    kind
                );

            return
                entries != null
                    ? entries.Count
                    : 0;
        }


        private static DisplayRow Row(
            string labelToken,
            string value
        )
        {
            return new DisplayRow(
                L(labelToken),
                value
            );
        }


        private static string L(
            string token
        )
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return "";
            }

            return
                Language.GetString(
                    token
                );
        }


        private sealed class ProfileStats
        {
            public int Profiles;
            public int LoreResolved;
            public int SkillGroups;
            public int SkillVariants;
            public int LockedSkills;
            public int Skins;
            public int LockedSkins;
            public int OriginalUnlocks;
            public int UsuUnlocks;
            public int FreeUnlocks;
        }


        private static ProfileStats CollectProfileStats()
        {
            ProfileStats result =
                new ProfileStats();

            IReadOnlyList<SurvivorContentProfile> profiles =
                SurvivorContentProfileService.GetAll();

            if (profiles == null)
            {
                return result;
            }

            result.Profiles =
                profiles.Count;

            for (
                int i = 0;
                i < profiles.Count;
                i++
            )
            {
                SurvivorContentProfile profile =
                    profiles[i];

                if (profile == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(profile.Lore))
                {
                    result.LoreResolved++;
                }

                if (profile.SurvivorUnlock != null)
                {
                    switch (profile.SurvivorUnlock.Source)
                    {
                        case SurvivorContentUnlockSource.Original:
                            result.OriginalUnlocks++;
                            break;

                        case SurvivorContentUnlockSource.UsuManaged:
                            result.UsuUnlocks++;
                            break;

                        case SurvivorContentUnlockSource.UnlockedByDefault:
                            result.FreeUnlocks++;
                            break;
                    }
                }

                result.SkillGroups +=
                    profile.SkillGroups.Count;

                for (
                    int groupIndex = 0;
                    groupIndex < profile.SkillGroups.Count;
                    groupIndex++
                )
                {
                    SurvivorContentSkillGroup group =
                        profile.SkillGroups[groupIndex];

                    if (group == null)
                    {
                        continue;
                    }

                    result.SkillVariants +=
                        group.Variants.Count;

                    for (
                        int variantIndex = 0;
                        variantIndex < group.Variants.Count;
                        variantIndex++
                    )
                    {
                        SurvivorContentSkillVariant variant =
                            group.Variants[variantIndex];

                        if (
                            variant != null &&
                            variant.Discovery ==
                                ContentCatalogDiscoveryState.Undiscovered
                        )
                        {
                            result.LockedSkills++;
                        }
                    }
                }

                result.Skins +=
                    profile.Skins.Count;

                for (
                    int skinIndex = 0;
                    skinIndex < profile.Skins.Count;
                    skinIndex++
                )
                {
                    SurvivorContentSkinEntry skin =
                        profile.Skins[skinIndex];

                    if (
                        skin != null &&
                        skin.Discovery ==
                            ContentCatalogDiscoveryState.Undiscovered
                    )
                    {
                        result.LockedSkins++;
                    }
                }
            }

            return result;
        }


        private void DestroyAllGeneratedRows()
        {
            foreach (
                KeyValuePair<string, PageState> pair
                in pageStates
            )
            {
                DestroyGeneratedRows(
                    pair.Value
                );
            }

            if (
                activeState == null ||
                activeState.Page != EmbeddedPage.Survivors
            )
            {
                DestroySurvivorDetailsPanel();
            }
        }


        private void DestroyGeneratedRows(
            PageState state
        )
        {
            if (state == null)
            {
                return;
            }

            for (
                int i = 0;
                i < state.GeneratedRows.Count;
                i++
            )
            {
                GameObject row =
                    state.GeneratedRows[i];

                if (row != null)
                {
                    UnityEngine.Object.Destroy(
                        row
                    );
                }
            }

            state.GeneratedRows.Clear();
        }


        private static string GetPageDebugName(
            EmbeddedPage page
        )
        {
            switch (page)
            {
                case EmbeddedPage.General:
                    return "General";

                case EmbeddedPage.Survivors:
                    return "Personajes";

                case EmbeddedPage.Missions:
                    return "Misiones";

                case EmbeddedPage.Advanced:
                    return "Avanzado";

                default:
                    return page.ToString();
            }
        }
    }
}
