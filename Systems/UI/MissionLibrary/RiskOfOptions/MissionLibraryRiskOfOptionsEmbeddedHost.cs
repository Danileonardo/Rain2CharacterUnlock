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
    internal sealed partial class MissionLibraryRiskOfOptionsEmbeddedHost : MonoBehaviour
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

        // 5G.2E-B provider switch real dentro del host EMBEDDED.
        // MissionLibraryPanelController pertenece al panel legacy y no es la
        // UI que el usuario ve cuando RiskOfOptions está en modo Embedded.
        private GameObject survivorProviderControlsRoot;
        private Button survivorProviderOriginalButton;
        private Button survivorProviderUsuButton;
        private Button survivorProviderCustomButton;
        private Button survivorProviderGeneratedButton;
        private HGTextMeshProUGUI survivorProviderOriginalText;
        private HGTextMeshProUGUI survivorProviderUsuText;
        private HGTextMeshProUGUI survivorProviderCustomText;
        private HGTextMeshProUGUI survivorProviderGeneratedText;

        // G8 Phase 1.6: el reroll Generated vive junto al bloque de misión.
        // No aparece para Original, Official USU ni Custom.
        private GameObject survivorGeneratedRerollRoot;
        private Button survivorGeneratedRerollButton;
        private GameObject survivorGeneratedDeleteRoot;
        private Button survivorGeneratedDeleteButton;
        private HGTextMeshProUGUI survivorGeneratedDeleteText;
        private static Texture2D generatedRerollIconTexture;
        private static Sprite generatedRerollIconSprite;

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
            public Action OnClick;

            public DisplayRow(
                string label,
                string value,
                Action onClick = null
            )
            {
                Label = label ?? "";
                Value = value ?? "";
                OnClick = onClick;
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

            RefreshAH64CampaignBarsIfVisible();
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
    }
}
