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
                    $"v{Plugin.PluginVersion}"
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
                new DisplayRow(
                    "Diagnóstico de logs",
                    UsuLog.DiagnosticEnabled ? "ACTIVADO" : "DESACTIVADO",
                    UsuLog.ToggleDiagnostic
                ),
                new DisplayRow(
                    "Logs detallados",
                    UsuLog.VerboseEnabled ? "ACTIVADO" : "DESACTIVADO",
                    UsuLog.ToggleVerbose
                ),
                new DisplayRow(
                    "Auditorías extensas",
                    UsuLog.ExtendedAuditEnabled ? "ACTIVADO" : "DESACTIVADO",
                    UsuLog.ToggleExtendedAudit
                ),
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
