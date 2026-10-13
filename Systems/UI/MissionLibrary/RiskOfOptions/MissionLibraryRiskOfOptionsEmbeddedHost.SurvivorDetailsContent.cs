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

            UpdateSurvivorProviderControls(
                profile
            );

            // Preset visual común para todos los supervivientes. El provider
            // activo y sus controles siguen viniendo del estado real.
            StyleSurvivorPresetProviderRow(profile);

            UpdateGeneratedRerollControl(
                profile
            );

            UpdateGeneratedDeleteControl(
                profile
            );

            bool ah64 = string.Equals(profile.BodyName, "AH64Body",
                StringComparison.OrdinalIgnoreCase);
            RectTransform detailsTextRect;
            if (ah64)
            {
                HideSurvivorPresetCards();
                UpdateAH64CampaignPanel(profile);
                detailsTextRect = ah64CampaignContentRoot;
            }
            else
            {
                HideAH64CampaignPanel();
                UpdateSurvivorPresetCards(profile);
                detailsTextRect = survivorPresetContentRoot;
            }

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
            SurvivorContentProfile profile,
            bool skipNotes = false,
            bool skipCampaign = false
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

            // ÚNICO PERSONAJE CON CAMPAÑA PERSISTENTE: AH-64.
            // Se ubica después de la misión/Descripción y ANTES de Notas,
            // manteniendo toda la UI de los demás survivors intacta.
            if (!skipCampaign && string.Equals(profile.BodyName, "AH64Body",
                StringComparison.OrdinalIgnoreCase))
            {
                AppendSectionTitle(builder,
                    AH64CampaignTracker.IsSpanish
                        ? "Progreso de campaña — Protocolo 64"
                        : "Campaign progress — Protocol 64");
                builder.Append("\n");
                builder.Append(AH64CampaignTracker.BuildProgressText());
            }

            if (!skipNotes)
            {
                AppendSectionTitle(
                    builder,
                    L(
                        SurvivorLocalization
                            .MissionLibraryBrowserNotesToken
                    )
                );

                builder.Append("\n");
                builder.Append(ResolveNotesText(profile, hidden));
            }

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

            SurvivorJsonEntry entry;

            if (
                !SessionMissionRegistry.TryGetLocalEffectiveEntry(
                    profile.BodyName,
                    out entry
                ) ||
                entry == null
            )
            {
                entry =
                    SurvivorJsonManager.GetEntryAnywhere(
                        profile.BodyName
                    );
            }

            UnlockProviderKind provider =
                GetEffectiveProvider(
                    entry,
                    profile
                );

            result.ProviderLabel =
                provider != UnlockProviderKind.Original &&
                IsGeneratedChallenge(entry?.Challenge)
                    ? L(
                        SurvivorLocalization
                            .MissionLibraryProviderGeneratedToken
                    )
                    : GetProviderLabel(provider);

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


        private static bool IsGeneratedChallenge(
            SurvivorChallengeJson challenge
        )
        {
            return
                challenge?.MissionConfig != null &&
                string.Equals(
                    challenge.MissionConfig.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static UnlockProviderKind GetEffectiveProvider(
            SurvivorJsonEntry entry,
            SurvivorContentProfile profile
        )
        {
            SurvivorContentUnlockSource contentSource =
                profile?.SurvivorUnlock?.Source
                ?? SurvivorContentUnlockSource.Unknown;

            return UnlockProviderPolicy.ResolveEffectiveProvider(
                entry,
                contentSource
            );
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
                GeneratedMissionPresentationFormatter.TryResolve(
                    challenge,
                    out string generatedName,
                    out string generatedDescription
                )
            )
            {
                return generatedName;
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
                GeneratedMissionPresentationFormatter.TryResolve(
                    challenge,
                    out string generatedName,
                    out string generatedDescription
                )
            )
            {
                return generatedDescription;
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


    }
}
