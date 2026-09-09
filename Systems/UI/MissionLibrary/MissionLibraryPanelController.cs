using System;
using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// 5G.2C FIX1 - Biblioteca de Misiones + política Original/USU/Custom.
    ///
    /// Esta iteración conserva el backend ya probado de 5G.1D/5G.2A y
    /// mejora únicamente la experiencia de uso:
    /// - selector de proveedor Original / USU / Custom;
    /// - preview de la misión activa;
    /// - restricciones mostradas debajo de la descripción;
    /// - cards de presets con "Diseñado para" y estado activo;
    /// - restauración del personaje separada del cambio de provider.
    ///
    /// CUSTOM sigue siendo sólo informativo hasta 5H/5I.
    /// </summary>
    public sealed class MissionLibraryPanelController : MonoBehaviour
    {
        private sealed class SurvivorRow
        {
            public string BodyName = "";
            public string DisplayName = "";
            public bool Available;
        }


        private ManualLogSource logger;
        private bool initialized;
        private bool isOpen;
        private bool launcherVisible = true;

        private Rect windowRect;

        private Vector2 survivorScroll;
        private Vector2 presetScroll;

        private readonly List<SurvivorRow> survivors =
            new List<SurvivorRow>();

        private List<MissionPreset> presets =
            new List<MissionPreset>();

        private string selectedBodyName = "";
        private string selectedPresetId = "";

        private string statusMessage = "";
        private bool statusSuccess = true;
        private bool guiFirstFrameLogged;

        private GUIStyle titleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle normalStyle;
        private GUIStyle smallStyle;
        private GUIStyle providerStyle;
        private GUIStyle activeProviderStyle;
        private GUIStyle warningStyle;
        private GUIStyle successStyle;
        private GUIStyle errorStyle;


        public void Initialize(
            ManualLogSource activeLogger
        )
        {
            logger = activeLogger;

            if (initialized)
            {
                return;
            }

            initialized = true;

            RebuildLists();
        }


        private void OnGUI()
        {
            if (!initialized)
            {
                return;
            }

            GUI.depth = -10000;

            if (!guiFirstFrameLogged)
            {
                guiFirstFrameLogged = true;

                logger?.LogInfo(
                    "[MISSION LIBRARY UI] 5G.2B OnGUI activo | " +
                    "Object: " +
                    gameObject.name
                );
            }

            EnsureStyles();

            float scale = GetUiScale();

            if (!isOpen)
            {
                if (launcherVisible)
                {
                    DrawLauncher(scale);
                }

                return;
            }

            EnsureWindowRect(scale);

            windowRect = GUI.ModalWindow(
                GetInstanceID(),
                windowRect,
                DrawWindow,
                Language.GetString(
                    SurvivorLocalization.MissionLibraryWindowTitleToken
                )
            );
        }


        private void DrawLauncher(
            float scale
        )
        {
            float width = 110f * scale;
            float height = 42f * scale;

            Rect buttonRect =
                new Rect(
                    Screen.width - width - (20f * scale),
                    20f * scale,
                    width,
                    height
                );

            if (
                GUI.Button(
                    buttonRect,
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryButtonToken
                    )
                )
            )
            {
                Open();
            }
        }


        private void DrawWindow(
            int windowId
        )
        {
            GUILayout.BeginVertical();

            DrawHeader();

            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();

            DrawSurvivorColumn();

            GUILayout.Space(12f);

            DrawDetailsColumn();

            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            DrawFooter();

            GUILayout.EndVertical();

            GUI.DragWindow(
                new Rect(
                    0f,
                    0f,
                    windowRect.width - 42f,
                    28f
                )
            );
        }


        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                Language.GetString(
                    SurvivorLocalization.MissionLibraryHeadingToken
                ),
                titleStyle
            );

            GUILayout.FlexibleSpace();

            if (
                GUILayout.Button(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryRefreshToken
                    ),
                    GUILayout.Width(110f)
                )
            )
            {
                RebuildLists(
                    preserveSelection: true
                );
            }

            if (
                GUILayout.Button(
                    "X",
                    GUILayout.Width(34f)
                )
            )
            {
                isOpen = false;
            }

            GUILayout.EndHorizontal();
        }


        private void DrawSurvivorColumn()
        {
            GUILayout.BeginVertical(
                GUI.skin.box,
                GUILayout.Width(270f),
                GUILayout.ExpandHeight(true)
            );

            GUILayout.Label(
                Language.GetString(
                    SurvivorLocalization.MissionLibrarySurvivorsToken
                ),
                sectionStyle
            );

            survivorScroll =
                GUILayout.BeginScrollView(
                    survivorScroll,
                    GUILayout.ExpandHeight(true)
                );

            if (survivors.Count == 0)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryNoSurvivorsToken
                    ),
                    normalStyle
                );
            }

            for (
                int i = 0;
                i < survivors.Count;
                i++
            )
            {
                SurvivorRow row =
                    survivors[i];

                if (row == null)
                {
                    continue;
                }

                SurvivorJsonEntry entry =
                    SurvivorJsonManager.GetEntryAnywhere(
                        row.BodyName
                    );

                UnlockProviderKind provider =
                    GetEffectiveProvider(
                        entry
                    );

                bool selected =
                    string.Equals(
                        selectedBodyName,
                        row.BodyName,
                        StringComparison.OrdinalIgnoreCase
                    );

                string label =
                    selected
                        ? "> " + row.DisplayName
                        : row.DisplayName;

                label +=
                    "  [" +
                    GetProviderShortLabel(provider) +
                    "]";

                if (!row.Available)
                {
                    label +=
                        " " +
                        Language.GetString(
                            SurvivorLocalization
                                .MissionLibraryUnavailableSuffixToken
                        );
                }

                if (
                    GUILayout.Button(
                        label,
                        GUILayout.MinHeight(30f)
                    )
                )
                {
                    selectedBodyName =
                        row.BodyName;

                    selectedPresetId = "";
                    statusMessage = "";
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }


        private void DrawDetailsColumn()
        {
            GUILayout.BeginVertical(
                GUI.skin.box,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );

            SurvivorJsonEntry entry =
                GetSelectedEntry();

            if (entry == null)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibrarySelectSurvivorToken
                    ),
                    normalStyle
                );

                GUILayout.EndVertical();
                return;
            }

            DrawCharacterHeader(entry);

            GUILayout.Space(8f);

            DrawProviderSelector(entry);

            GUILayout.Space(8f);

            DrawActiveMission(entry);

            GUILayout.Space(8f);

            DrawPresetLibrary(entry);

            GUILayout.EndVertical();
        }


        private void DrawCharacterHeader(
            SurvivorJsonEntry entry
        )
        {
            string displayName =
                !string.IsNullOrWhiteSpace(entry.DisplayName)
                    ? entry.DisplayName
                    : selectedBodyName;

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                displayName,
                titleStyle,
                GUILayout.ExpandWidth(true)
            );

            if (!entry.Available)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryUnavailableSuffixToken
                    ),
                    warningStyle,
                    GUILayout.ExpandWidth(false)
                );
            }

            GUILayout.EndHorizontal();
        }


        private void DrawProviderSelector(
            SurvivorJsonEntry entry
        )
        {
            GUILayout.Label(
                Language.GetString(
                    SurvivorLocalization.MissionLibraryProviderSectionToken
                ),
                sectionStyle
            );

            UnlockProviderKind activeProvider =
                GetEffectiveProvider(
                    entry
                );

            bool canMutate =
                CanMutateConfiguration() &&
                entry.Available;

            bool originalAvailable =
                SurvivorUnlockManager.HasStoredOriginalUnlock(
                    entry
                );

            MissionConfiguration missionConfig =
                entry.Challenge?.MissionConfig;

            string rememberedPresetId =
                missionConfig?.BasePresetId ?? "";

            bool canReactivateUsu =
                !string.IsNullOrWhiteSpace(
                    rememberedPresetId
                ) &&
                MissionPresetLibraryService
                    .TryGetAssignableMissionPreset(
                        rememberedPresetId,
                        out MissionPreset rememberedPreset
                    ) &&
                rememberedPreset != null;

            GUILayout.BeginHorizontal();

            DrawProviderButton(
                UnlockProviderKind.Original,
                activeProvider,
                originalAvailable,
                canMutate && originalAvailable,
                delegate
                {
                    ApplyResult(
                        MissionAssignmentService.SelectOriginal(
                            selectedBodyName,
                            logger
                        )
                    );
                }
            );

            // Nueva política de 5G.2C FIX1:
            // si el creador ya posee un sistema Original, USU NO ofrece un
            // provider alternativo directo. Cualquier modificación futura se
            // hará como CUSTOM, preservando intacto el sistema del creador.
            //
            // Si no existe Original, USU sigue siendo el fallback y puede
            // cambiarse entre presets oficiales como hasta ahora.
            bool usuSelectable =
                !originalAvailable;

            DrawProviderButton(
                UnlockProviderKind.USU,
                activeProvider,
                usuSelectable || activeProvider == UnlockProviderKind.USU,
                canMutate && usuSelectable,
                delegate
                {
                    if (canReactivateUsu)
                    {
                        ApplyResult(
                            MissionAssignmentService.AssignPreset(
                                selectedBodyName,
                                rememberedPresetId,
                                logger
                            )
                        );

                        return;
                    }

                    statusSuccess = true;
                    statusMessage =
                        Language.GetString(
                            SurvivorLocalization
                                .MissionLibraryChoosePresetActionToken
                        );

                    presetScroll = Vector2.zero;
                }
            );

            DrawProviderButton(
                UnlockProviderKind.Custom,
                activeProvider,
                false,
                false,
                null
            );

            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            if (activeProvider == UnlockProviderKind.Original)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryOriginalDescriptionToken
                    ),
                    smallStyle
                );
            }
            else if (
                activeProvider == UnlockProviderKind.USU ||
                activeProvider == UnlockProviderKind.Community
            )
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryUsuDescriptionToken
                    ),
                    smallStyle
                );
            }
            else
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryCustomDescriptionToken
                    ),
                    smallStyle
                );
            }

            if (originalAvailable)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization
                            .MissionLibraryUsuUnavailableWhenOriginalToken
                    ),
                    smallStyle
                );

                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization
                            .MissionLibraryCustomFromOriginalSoonToken
                    ),
                    smallStyle
                );
            }
            else if (
                activeProvider != UnlockProviderKind.USU &&
                !canReactivateUsu
            )
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryChoosePresetForUsuToken
                    ),
                    smallStyle
                );
            }

            if (!originalAvailable)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryOriginalUnavailableToken
                    ),
                    smallStyle
                );
            }

            if (!canMutate)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryHostOnlyToken
                    ),
                    errorStyle
                );
            }
        }


        private void DrawProviderButton(
            UnlockProviderKind provider,
            UnlockProviderKind activeProvider,
            bool available,
            bool canClick,
            Action action
        )
        {
            bool active =
                provider == activeProvider ||
                (
                    provider == UnlockProviderKind.USU &&
                    activeProvider == UnlockProviderKind.Community
                );

            string label =
                (active ? "● " : "○ ") +
                GetProviderShortLabel(provider);

            if (!available && provider != UnlockProviderKind.Custom)
            {
                label +=
                    " " +
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryUnavailableSuffixToken
                    );
            }

            if (provider == UnlockProviderKind.Custom)
            {
                label +=
                    " " +
                    Language.GetString(
                        SurvivorLocalization.MissionLibrarySoonSuffixToken
                    );
            }

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                canClick &&
                !active;

            if (
                GUILayout.Button(
                    label,
                    active
                        ? activeProviderStyle
                        : providerStyle,
                    GUILayout.MinHeight(38f),
                    GUILayout.ExpandWidth(true)
                ) &&
                action != null
            )
            {
                action();
            }

            GUI.enabled =
                previousEnabled;
        }


        private void DrawActiveMission(
            SurvivorJsonEntry entry
        )
        {
            GUILayout.BeginVertical(
                GUI.skin.box
            );

            GUILayout.Label(
                Language.GetString(
                    SurvivorLocalization.MissionLibraryActiveMissionToken
                ),
                sectionStyle
            );

            UnlockProviderKind provider =
                GetEffectiveProvider(
                    entry
                );

            if (provider == UnlockProviderKind.Original)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryOriginalMissionTitleToken
                    ),
                    normalStyle
                );

                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryOriginalMissionDescriptionToken
                    ),
                    smallStyle
                );

                GUILayout.EndVertical();
                DrawRestoreSection(entry);
                return;
            }

            SurvivorChallengeJson challenge =
                entry.Challenge;

            string missionName =
                ResolveChallengeName(
                    entry
                );

            string missionDescription =
                ResolveChallengeDescription(
                    entry
                );

            if (
                string.IsNullOrWhiteSpace(missionName) &&
                string.IsNullOrWhiteSpace(missionDescription)
            )
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryNoActiveMissionToken
                    ),
                    normalStyle
                );
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(missionName))
                {
                    GUILayout.Label(
                        missionName,
                        titleStyle
                    );
                }

                if (!string.IsNullOrWhiteSpace(missionDescription))
                {
                    GUILayout.Label(
                        missionDescription,
                        normalStyle
                    );
                }

                DrawMissionRestrictionNotices(
                    challenge?.Mission
                );

                MissionConfiguration missionConfig =
                    challenge?.MissionConfig;

                if (
                    missionConfig != null &&
                    string.Equals(
                        missionConfig.Source,
                        "Preset",
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    !string.IsNullOrWhiteSpace(
                        missionConfig.BasePresetId
                    ) &&
                    MissionPresetLibraryService
                        .TryGetAssignableMissionPreset(
                            missionConfig.BasePresetId,
                            out MissionPreset activePreset
                        ) &&
                    activePreset != null &&
                    !string.IsNullOrWhiteSpace(
                        activePreset.TargetBody
                    )
                )
                {
                    GUILayout.Space(3f);

                    GUILayout.Label(
                        Language.GetStringFormatted(
                            SurvivorLocalization.MissionLibraryDesignedForToken,
                            ResolveDisplayName(
                                activePreset.TargetBody
                            )
                        ),
                        smallStyle
                    );
                }
            }

            GUILayout.EndVertical();

            DrawRestoreSection(entry);
        }


        private void DrawRestoreSection(
            SurvivorJsonEntry entry
        )
        {
            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                Language.GetString(
                    SurvivorLocalization.MissionLibraryRestoreHintToken
                ),
                smallStyle,
                GUILayout.ExpandWidth(true)
            );

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                CanMutateConfiguration() &&
                entry.Available;

            if (
                GUILayout.Button(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryRestoreToken
                    ),
                    GUILayout.Width(180f),
                    GUILayout.MinHeight(30f)
                )
            )
            {
                ApplyResult(
                    MissionAssignmentService.RestoreCharacter(
                        selectedBodyName,
                        logger
                    )
                );
            }

            GUI.enabled =
                previousEnabled;

            GUILayout.EndHorizontal();
        }


        private void DrawPresetLibrary(
            SurvivorJsonEntry entry
        )
        {
            GUILayout.Label(
                Language.GetString(
                    SurvivorLocalization.MissionLibraryPresetsToken
                ),
                sectionStyle
            );

            bool originalAvailable =
                SurvivorUnlockManager.HasStoredOriginalUnlock(
                    entry
                );

            GUILayout.Label(
                Language.GetString(
                    originalAvailable
                        ? SurvivorLocalization.MissionLibraryPresetHintOriginalToken
                        : SurvivorLocalization.MissionLibraryPresetHintToken
                ),
                smallStyle
            );

            presetScroll =
                GUILayout.BeginScrollView(
                    presetScroll,
                    GUILayout.ExpandHeight(true)
                );

            if (presets == null || presets.Count == 0)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryNoPresetsToken
                    ),
                    normalStyle
                );
            }

            for (
                int i = 0;
                presets != null && i < presets.Count;
                i++
            )
            {
                MissionPreset preset =
                    presets[i];

                if (preset == null)
                {
                    continue;
                }

                DrawPresetCard(
                    entry,
                    preset
                );
            }

            GUILayout.EndScrollView();
        }


        private void DrawPresetCard(
            SurvivorJsonEntry entry,
            MissionPreset preset
        )
        {
            MissionConfiguration missionConfig =
                entry.Challenge?.MissionConfig;

            UnlockProviderKind provider =
                GetEffectiveProvider(
                    entry
                );

            bool activePreset =
                provider == UnlockProviderKind.USU &&
                missionConfig != null &&
                string.Equals(
                    missionConfig.Source,
                    "Preset",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                string.Equals(
                    missionConfig.BasePresetId,
                    preset.PresetId,
                    StringComparison.OrdinalIgnoreCase
                );

            bool lastActionPreset =
                string.Equals(
                    selectedPresetId,
                    preset.PresetId,
                    StringComparison.OrdinalIgnoreCase
                );

            GUILayout.BeginVertical(
                GUI.skin.box
            );

            GUILayout.BeginHorizontal();

            string presetName =
                ResolvePresetName(
                    preset
                );

            GUILayout.Label(
                (activePreset ? "● " : "") +
                presetName,
                sectionStyle,
                GUILayout.ExpandWidth(true)
            );

            bool originalAvailable =
                SurvivorUnlockManager.HasStoredOriginalUnlock(
                    entry
                );

            bool canMutate =
                CanMutateConfiguration() &&
                entry.Available &&
                !activePreset &&
                !originalAvailable;

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                canMutate;

            string actionText;

            if (originalAvailable)
            {
                actionText =
                    Language.GetString(
                        SurvivorLocalization
                            .MissionLibraryPresetCustomSoonToken
                    );
            }
            else
            {
                actionText =
                    activePreset
                        ? Language.GetString(
                            SurvivorLocalization.MissionLibraryInUseToken
                        )
                        : Language.GetString(
                            SurvivorLocalization.MissionLibraryUseThisMissionToken
                        );
            }

            if (
                GUILayout.Button(
                    actionText,
                    GUILayout.Width(150f),
                    GUILayout.MinHeight(28f)
                )
            )
            {
                selectedPresetId =
                    preset.PresetId ?? "";

                ApplyResult(
                    MissionAssignmentService.AssignPreset(
                        selectedBodyName,
                        preset.PresetId,
                        logger
                    )
                );
            }

            GUI.enabled =
                previousEnabled;

            GUILayout.EndHorizontal();

            string presetDescription =
                ResolvePresetDescription(
                    preset
                );

            if (
                !string.IsNullOrWhiteSpace(
                    presetDescription
                )
            )
            {
                GUILayout.Label(
                    presetDescription,
                    normalStyle
                );
            }

            DrawMissionRestrictionNotices(
                preset.Mission
            );

            if (
                !string.IsNullOrWhiteSpace(
                    preset.TargetBody
                )
            )
            {
                GUILayout.Space(3f);

                GUILayout.Label(
                    Language.GetStringFormatted(
                        SurvivorLocalization.MissionLibraryDesignedForToken,
                        ResolveDisplayName(
                            preset.TargetBody
                        )
                    ),
                    smallStyle
                );
            }

            if (lastActionPreset && !activePreset)
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryLastActionPresetToken
                    ),
                    smallStyle
                );
            }

            GUILayout.EndVertical();
        }


        private void DrawMissionRestrictionNotices(
            MissionDefinition mission
        )
        {
            List<string> excludedBodies =
                GetGloballyExcludedSurvivors(
                    mission
                );

            if (
                excludedBodies == null ||
                excludedBodies.Count == 0
            )
            {
                return;
            }

            List<string> displayNames =
                new List<string>();

            for (
                int i = 0;
                i < excludedBodies.Count;
                i++
            )
            {
                string displayName =
                    ResolveDisplayName(
                        excludedBodies[i]
                    );

                if (
                    string.IsNullOrWhiteSpace(
                        displayName
                    )
                )
                {
                    continue;
                }

                displayNames.Add(
                    displayName
                );
            }

            if (displayNames.Count == 0)
            {
                return;
            }

            GUILayout.Space(4f);

            GUILayout.Label(
                Language.GetStringFormatted(
                    SurvivorLocalization.MissionLibraryNotValidWithToken,
                    string.Join(
                        " / ",
                        displayNames.ToArray()
                    )
                ),
                warningStyle
            );
        }


        private static List<string> GetGloballyExcludedSurvivors(
            MissionDefinition mission
        )
        {
            List<string> result =
                new List<string>();

            if (
                mission == null ||
                mission.Routes == null ||
                mission.Routes.Count == 0
            )
            {
                return result;
            }

            HashSet<string> global =
                null;

            bool foundUsableRoute =
                false;

            for (
                int routeIndex = 0;
                routeIndex < mission.Routes.Count;
                routeIndex++
            )
            {
                MissionRoute route =
                    mission.Routes[routeIndex];

                if (route == null)
                {
                    continue;
                }

                HashSet<string> routeExcluded =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase
                    );

                AddExcludedBodiesFromConditions(
                    route.Conditions,
                    routeExcluded
                );

                IReadOnlyList<MissionObjective> objectives =
                    route.GetEffectiveObjectives();

                if (objectives != null)
                {
                    for (
                        int objectiveIndex = 0;
                        objectiveIndex < objectives.Count;
                        objectiveIndex++
                    )
                    {
                        MissionObjective objective =
                            objectives[objectiveIndex];

                        if (objective == null)
                        {
                            continue;
                        }

                        AddExcludedBodiesFromConditions(
                            objective.Conditions,
                            routeExcluded
                        );
                    }
                }

                if (!foundUsableRoute)
                {
                    global =
                        new HashSet<string>(
                            routeExcluded,
                            StringComparer.OrdinalIgnoreCase
                        );

                    foundUsableRoute =
                        true;
                }
                else
                {
                    global.IntersectWith(
                        routeExcluded
                    );
                }
            }

            if (
                !foundUsableRoute ||
                global == null ||
                global.Count == 0
            )
            {
                return result;
            }

            foreach (string body in global)
            {
                result.Add(
                    body
                );
            }

            result.Sort(
                StringComparer.OrdinalIgnoreCase
            );

            return result;
        }


        private static void AddExcludedBodiesFromConditions(
            IList<MissionCondition> conditions,
            HashSet<string> destination
        )
        {
            if (
                conditions == null ||
                destination == null
            )
            {
                return;
            }

            for (
                int i = 0;
                i < conditions.Count;
                i++
            )
            {
                MissionCondition condition =
                    conditions[i];

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

                Newtonsoft.Json.Linq.JToken bodiesToken =
                    condition.Parameters["bodies"];

                if (
                    bodiesToken is Newtonsoft.Json.Linq.JArray bodiesArray
                )
                {
                    for (
                        int bodyIndex = 0;
                        bodyIndex < bodiesArray.Count;
                        bodyIndex++
                    )
                    {
                        string body =
                            bodiesArray[bodyIndex]?.ToString();

                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            destination.Add(
                                body.Trim()
                            );
                        }
                    }
                }

                string singleBody =
                    condition.Parameters["body"]?.ToString();

                if (!string.IsNullOrWhiteSpace(singleBody))
                {
                    destination.Add(
                        singleBody.Trim()
                    );
                }
            }
        }


        private string ResolveChallengeName(
            SurvivorJsonEntry entry
        )
        {
            if (entry?.Challenge == null)
            {
                return "";
            }

            SurvivorChallengeJson challenge =
                entry.Challenge;

            if (
                SurvivorLocalization.UsesBuiltInLocalization(
                    selectedBodyName,
                    challenge
                ) &&
                SurvivorLocalization.TryGetOfficialMissionTokens(
                    selectedBodyName,
                    out string ownNameToken,
                    out string ownDescriptionToken
                )
            )
            {
                return Language.GetString(
                    ownNameToken
                );
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
                MissionPresetLibraryService
                    .TryGetAssignableMissionPreset(
                        missionConfig.BasePresetId,
                        out MissionPreset preset
                    ) &&
                preset != null
            )
            {
                return ResolvePresetName(
                    preset
                );
            }

            return challenge.Name ?? "";
        }


        private string ResolveChallengeDescription(
            SurvivorJsonEntry entry
        )
        {
            if (entry?.Challenge == null)
            {
                return "";
            }

            SurvivorChallengeJson challenge =
                entry.Challenge;

            if (
                SurvivorLocalization.UsesBuiltInLocalization(
                    selectedBodyName,
                    challenge
                ) &&
                SurvivorLocalization.TryGetOfficialMissionTokens(
                    selectedBodyName,
                    out string ownNameToken,
                    out string ownDescriptionToken
                )
            )
            {
                return Language.GetString(
                    ownDescriptionToken
                );
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
                MissionPresetLibraryService
                    .TryGetAssignableMissionPreset(
                        missionConfig.BasePresetId,
                        out MissionPreset preset
                    ) &&
                preset != null
            )
            {
                return ResolvePresetDescription(
                    preset
                );
            }

            return challenge.Description ?? "";
        }


        private string ResolvePresetName(
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
                return Language.GetString(
                    nameToken
                );
            }

            return
                !string.IsNullOrWhiteSpace(preset.Name)
                    ? preset.Name
                    : preset.PresetId ?? "";
        }


        private string ResolvePresetDescription(
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
                return Language.GetString(
                    descriptionToken
                );
            }

            return preset.Description ?? "";
        }


        private void DrawFooter()
        {
            if (
                !string.IsNullOrWhiteSpace(
                    statusMessage
                )
            )
            {
                GUILayout.Label(
                    statusMessage,
                    statusSuccess
                        ? successStyle
                        : errorStyle
                );
            }
            else
            {
                GUILayout.Label(
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryFooterToken
                    ),
                    smallStyle
                );
            }
        }


        private void ApplyResult(
            MissionAssignmentResult result
        )
        {
            if (result == null)
            {
                statusSuccess = false;
                statusMessage =
                    "MissionAssignmentService returned null.";

                return;
            }

            statusSuccess =
                result.Success;

            statusMessage =
                result.Message ?? "";

            RebuildLists(
                preserveSelection: true
            );
        }


        public void SetLauncherVisible(
            bool visible
        )
        {
            launcherVisible = visible;
        }


        public void OpenExternal()
        {
            Open();
        }


        private void Open()
        {
            RebuildLists(
                preserveSelection: true
            );

            isOpen = true;
        }


        private void RebuildLists(
            bool preserveSelection = false
        )
        {
            string previousBody =
                preserveSelection
                    ? selectedBodyName
                    : "";

            survivors.Clear();

            SurvivorJsonFile file =
                SurvivorJsonManager.CurrentConfig;

            if (file != null)
            {
                AddEntries(
                    file.AvailableSurvivors,
                    true
                );

                AddEntries(
                    file.UnavailableSurvivors,
                    false
                );
            }

            survivors.Sort(
                delegate(
                    SurvivorRow first,
                    SurvivorRow second
                )
                {
                    return string.Compare(
                        first?.DisplayName ?? "",
                        second?.DisplayName ?? "",
                        StringComparison.OrdinalIgnoreCase
                    );
                }
            );

            presets =
                MissionPresetLibraryService
                    .GetAssignableMissionPresets()
                ?? new List<MissionPreset>();

            if (
                !string.IsNullOrWhiteSpace(previousBody) &&
                ContainsBody(previousBody)
            )
            {
                selectedBodyName =
                    previousBody;
            }
            else if (
                !string.IsNullOrWhiteSpace(selectedBodyName) &&
                ContainsBody(selectedBodyName)
            )
            {
                // Conserva selección existente.
            }
            else
            {
                selectedBodyName =
                    survivors.Count > 0
                        ? survivors[0].BodyName
                        : "";
            }
        }


        private void AddEntries(
            Dictionary<string, SurvivorJsonEntry> entries,
            bool available
        )
        {
            if (entries == null)
            {
                return;
            }

            foreach (
                KeyValuePair<string, SurvivorJsonEntry> pair
                in entries
            )
            {
                SurvivorJsonEntry entry =
                    pair.Value;

                if (entry == null)
                {
                    continue;
                }

                string bodyName =
                    !string.IsNullOrWhiteSpace(entry.BodyName)
                        ? entry.BodyName
                        : pair.Key;

                if (
                    string.IsNullOrWhiteSpace(
                        bodyName
                    )
                )
                {
                    continue;
                }

                string displayName =
                    !string.IsNullOrWhiteSpace(entry.DisplayName)
                        ? entry.DisplayName
                        : GetReadableBodyName(bodyName);

                survivors.Add(
                    new SurvivorRow
                    {
                        BodyName = bodyName,
                        DisplayName = displayName,
                        Available = available
                    }
                );
            }
        }


        private SurvivorJsonEntry GetSelectedEntry()
        {
            if (
                string.IsNullOrWhiteSpace(
                    selectedBodyName
                )
            )
            {
                return null;
            }

            return SurvivorJsonManager.GetEntryAnywhere(
                selectedBodyName
            );
        }


        private bool ContainsBody(
            string bodyName
        )
        {
            for (
                int i = 0;
                i < survivors.Count;
                i++
            )
            {
                if (
                    string.Equals(
                        survivors[i]?.BodyName,
                        bodyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }


        private string ResolveDisplayName(
            string bodyName
        )
        {
            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    bodyName
                );

            if (
                entry != null &&
                !string.IsNullOrWhiteSpace(
                    entry.DisplayName
                )
            )
            {
                return entry.DisplayName;
            }

            return GetReadableBodyName(
                bodyName
            );
        }


        private static string GetReadableBodyName(
            string bodyName
        )
        {
            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return "";
            }

            switch (bodyName)
            {
                case "CommandoBody":
                    return "Commando";
                case "HuntressBody":
                    return "Huntress";
                case "Bandit2Body":
                    return "Bandit";
                case "ToolbotBody":
                    return "MUL-T";
                case "EngiBody":
                    return "Engineer";
                case "MageBody":
                    return "Artificer";
                case "MercBody":
                    return "Mercenary";
                case "TreebotBody":
                    return "REX";
                case "LoaderBody":
                    return "Loader";
                case "CrocoBody":
                    return "Acrid";
                case "CaptainBody":
                    return "Captain";
                case "RailgunnerBody":
                    return "Railgunner";
                case "VoidSurvivorBody":
                    return "Void Fiend";
                case "SeekerBody":
                    return "Seeker";
            }

            string result =
                bodyName.Trim();

            if (
                result.EndsWith(
                    "Body",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                result =
                    result.Substring(
                        0,
                        result.Length - 4
                    );
            }

            return result;
        }


        private static UnlockProviderKind GetEffectiveProvider(
            SurvivorJsonEntry entry
        )
        {
            MissionConfiguration missionConfig =
                entry?.Challenge?.MissionConfig;

            if (missionConfig != null)
            {
                // La UI debe reflejar el provider REAL, no sólo el valor
                // persistido. AutomaticFallback siempre cede al Original
                // cuando el creador dispone de su propio unlock.
                if (
                    missionConfig.EffectiveSelectionMode ==
                        UnlockSelectionMode.AutomaticFallback &&
                    SurvivorUnlockManager.HasStoredOriginalUnlock(
                        entry
                    )
                )
                {
                    return UnlockProviderKind.Original;
                }

                return missionConfig.EffectiveProvider;
            }

            return SurvivorUnlockManager.HasStoredOriginalUnlock(entry)
                ? UnlockProviderKind.Original
                : UnlockProviderKind.USU;
        }


        private string GetProviderShortLabel(
            UnlockProviderKind provider
        )
        {
            switch (provider)
            {
                case UnlockProviderKind.Original:
                    return Language.GetString(
                        SurvivorLocalization.MissionLibraryProviderOriginalToken
                    );

                case UnlockProviderKind.Custom:
                    return Language.GetString(
                        SurvivorLocalization.MissionLibraryProviderCustomToken
                    );

                case UnlockProviderKind.Community:
                    return Language.GetString(
                        SurvivorLocalization.MissionLibraryProviderCommunityToken
                    );

                default:
                    return Language.GetString(
                        SurvivorLocalization.MissionLibraryProviderUsuToken
                    );
            }
        }


        private bool CanMutateConfiguration()
        {
            return !(
                NetworkClient.active &&
                !NetworkServer.active
            );
        }


        private float GetUiScale()
        {
            return Mathf.Clamp(
                Screen.height / 1080f,
                0.75f,
                1.35f
            );
        }


        private void EnsureWindowRect(
            float scale
        )
        {
            float width =
                Mathf.Min(
                    1120f * scale,
                    Screen.width - (40f * scale)
                );

            float height =
                Mathf.Min(
                    760f * scale,
                    Screen.height - (60f * scale)
                );

            if (
                windowRect.width <= 0f ||
                windowRect.height <= 0f
            )
            {
                windowRect =
                    new Rect(
                        (Screen.width - width) * 0.5f,
                        (Screen.height - height) * 0.5f,
                        width,
                        height
                    );
            }
            else
            {
                windowRect.width =
                    width;

                windowRect.height =
                    height;

                windowRect.x =
                    Mathf.Clamp(
                        windowRect.x,
                        0f,
                        Screen.width - width
                    );

                windowRect.y =
                    Mathf.Clamp(
                        windowRect.y,
                        0f,
                        Screen.height - height
                    );
            }
        }


        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle =
                new GUIStyle(
                    GUI.skin.label
                )
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    richText = true
                };

            sectionStyle =
                new GUIStyle(
                    GUI.skin.label
                )
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    richText = true
                };

            normalStyle =
                new GUIStyle(
                    GUI.skin.label
                )
                {
                    fontSize = 13,
                    wordWrap = true,
                    richText = true
                };

            smallStyle =
                new GUIStyle(
                    GUI.skin.label
                )
                {
                    fontSize = 11,
                    wordWrap = true,
                    richText = true
                };

            providerStyle =
                new GUIStyle(
                    GUI.skin.button
                )
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Normal,
                    wordWrap = true
                };

            activeProviderStyle =
                new GUIStyle(
                    GUI.skin.button
                )
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };

            warningStyle =
                new GUIStyle(
                    normalStyle
                )
                {
                    fontStyle = FontStyle.Bold
                };

            successStyle =
                new GUIStyle(
                    normalStyle
                )
                {
                    fontStyle = FontStyle.Bold
                };

            errorStyle =
                new GUIStyle(
                    normalStyle
                )
                {
                    fontStyle = FontStyle.Bold
                };
        }
    }
}
