using System;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Snapshot inmutable de la selección de provider aplicable a un survivor.
    ///
    /// Esta clase concentra la resolución que antes estaba duplicada entre
    /// MissionAssignmentService y las dos UIs de Mission Library.
    /// No persiste cambios ni toca runtime.
    /// </summary>
    public sealed class MissionProviderSelectionState
    {
        public UnlockProviderKind ActiveProvider { get; internal set; }

        public bool OriginalAvailable { get; internal set; }

        public string ActiveSource
        {
            get;
            internal set;
        } = "";

        public bool CustomAvailable
        {
            get;
            internal set;
        }

        public bool GeneratedAvailable
        {
            get;
            internal set;
        }

        public OfficialMissionPresetDescriptor DirectUsuDescriptor
        {
            get;
            internal set;
        }

        public MissionPreset DirectUsuPreset
        {
            get;
            internal set;
        }

        public string RememberedPresetId
        {
            get;
            internal set;
        } = "";

        public MissionPreset RememberedPreset
        {
            get;
            internal set;
        }

        public string UsuTargetPresetId
        {
            get;
            internal set;
        } = "";

        public MissionPreset UsuTargetPreset
        {
            get;
            internal set;
        }

        /// <summary>
        /// El registry declara explícitamente que este survivor puede alternar
        /// Original &lt;-&gt; USU. Miner es el primer caso real.
        /// </summary>
        public bool DirectUsuAlternativeDeclared =>
            DirectUsuDescriptor != null &&
            DirectUsuDescriptor.AllowDirectUsuWithOriginal;

        /// <summary>
        /// La alternativa directa está declarada y su preset oficial sigue
        /// disponible en la biblioteca.
        /// </summary>
        public bool DirectUsuPresetAvailable =>
            DirectUsuAlternativeDeclared &&
            DirectUsuPreset != null &&
            !string.IsNullOrWhiteSpace(
                DirectUsuDescriptor?.PresetId
            );

        public bool RememberedUsuPresetAvailable =>
            RememberedPreset != null &&
            !string.IsNullOrWhiteSpace(RememberedPresetId);

        public bool UsuTargetAvailable =>
            UsuTargetPreset != null &&
            !string.IsNullOrWhiteSpace(UsuTargetPresetId);

        /// <summary>
        /// La UI fallback muestra USU cuando no existe Original o cuando existe
        /// una excepción oficial declarada. Esto preserva exactamente la regla
        /// histórica de ese panel.
        /// </summary>
        public bool CanOfferUsuProvider =>
            !OriginalAvailable ||
            DirectUsuAlternativeDeclared;

        /// <summary>
        /// La UI embebida sólo habilita el botón cuando puede ejecutar el cambio
        /// en un clic, o cuando USU ya es la familia activa.
        /// </summary>
        public bool CanSelectUsuImmediately =>
            UsuTargetAvailable;

        public bool IsGeneratedActive =>
            UnlockProviderPolicy.IsUsuFamily(ActiveProvider) &&
            string.Equals(
                ActiveSource,
                "Generated",
                StringComparison.OrdinalIgnoreCase
            );

        public bool IsCustomActive =>
            ActiveProvider == UnlockProviderKind.Custom;

        public bool IsUsuActive =>
            UnlockProviderPolicy.IsUsuFamily(ActiveProvider) &&
            !IsGeneratedActive;
    }


    /// <summary>
    /// Política única para resolver selección de misión/provider.
    ///
    /// Responsabilidades:
    /// - proteger Original frente a presets USU no autorizados;
    /// - resolver la alternativa oficial directa (Miner);
    /// - recordar el último preset USU reactivable;
    /// - exponer un snapshot común para RiskOfOptions y el fallback.
    ///
    /// No realiza persistencia ni cambios de runtime.
    /// </summary>
    public static class MissionProviderSelectionPolicy
    {
        public static MissionProviderSelectionState ResolveFromStoredEntry(
            SurvivorJsonEntry entry
        )
        {
            bool originalAvailable =
                UnlockProviderPolicy.HasStoredOriginalUnlock(entry);

            UnlockProviderKind activeProvider =
                UnlockProviderPolicy.ResolveEffectiveProvider(
                    entry,
                    originalAvailable
                );

            return BuildState(
                entry,
                activeProvider,
                originalAvailable
            );
        }


        public static MissionProviderSelectionState ResolveFromContentProfile(
            SurvivorJsonEntry entry,
            SurvivorContentUnlockSource contentSource
        )
        {
            bool originalAvailable =
                UnlockProviderPolicy.IsOriginalAvailable(
                    entry,
                    contentSource
                );

            UnlockProviderKind activeProvider =
                UnlockProviderPolicy.ResolveEffectiveProvider(
                    entry,
                    contentSource
                );

            return BuildState(
                entry,
                activeProvider,
                originalAvailable
            );
        }


        /// <summary>
        /// Devuelve true cuando un preset USU puede asignarse al survivor sin
        /// violar la prioridad del sistema Original del creador.
        /// </summary>
        public static bool CanAssignUsuPreset(
            SurvivorJsonEntry entry,
            string presetId
        )
        {
            if (entry == null)
            {
                return false;
            }

            if (!UnlockProviderPolicy.HasStoredOriginalUnlock(entry))
            {
                return true;
            }

            // Con la fila de providers 0.3.1, una misión Official propia de
            // USU puede coexistir con Original como alternativa explícita del
            // jugador. Seguimos bloqueando presets arbitrarios/cruzados.
            return
                IsOfficialUsuPresetForEntry(
                    entry,
                    presetId
                ) ||
                IsDirectOfficialUsuPreset(
                    entry,
                    presetId
                );
        }


        public static bool IsOfficialUsuPresetForEntry(
            SurvivorJsonEntry entry,
            string presetId
        )
        {
            if (
                entry == null ||
                string.IsNullOrWhiteSpace(presetId) ||
                !OfficialMissionPresetRegistry.TryResolve(
                    entry.BodyName,
                    entry.Source,
                    out OfficialMissionPresetDescriptor descriptor
                ) ||
                descriptor == null
            )
            {
                return false;
            }

            return string.Equals(
                descriptor.PresetId,
                presetId.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }


        /// <summary>
        /// Comprueba si presetId es exactamente la alternativa oficial USU
        /// autorizada para un survivor que también posee Original.
        /// </summary>
        public static bool IsDirectOfficialUsuPreset(
            SurvivorJsonEntry entry,
            string presetId
        )
        {
            if (entry == null)
            {
                return false;
            }

            return OfficialMissionPresetRegistry.AllowsDirectUsuWithOriginal(
                entry.BodyName,
                entry.Source,
                presetId
            );
        }


        /// <summary>
        /// Resuelve el descriptor declarado para Original &lt;-&gt; USU. No exige
        /// que el preset esté disponible; esa validación pertenece a AssignPreset
        /// y permite conservar los mismos códigos de error históricos.
        /// </summary>
        public static bool TryResolveDirectOfficialUsuDescriptor(
            SurvivorJsonEntry entry,
            out OfficialMissionPresetDescriptor descriptor
        )
        {
            descriptor = null;

            if (entry == null)
            {
                return false;
            }

            return
                OfficialMissionPresetRegistry.TryResolve(
                    entry.BodyName,
                    entry.Source,
                    out descriptor
                ) &&
                descriptor != null &&
                descriptor.AllowDirectUsuWithOriginal &&
                !string.IsNullOrWhiteSpace(descriptor.PresetId);
        }


        private static MissionProviderSelectionState BuildState(
            SurvivorJsonEntry entry,
            UnlockProviderKind activeProvider,
            bool originalAvailable
        )
        {
            MissionConfiguration missionConfig =
                entry?.Challenge?.MissionConfig;

            MissionProviderSelectionState state =
                new MissionProviderSelectionState
                {
                    ActiveProvider = activeProvider,
                    OriginalAvailable = originalAvailable,
                    ActiveSource = missionConfig?.Source ?? "",
                    CustomAvailable =
                        MissionProviderSlotStore.HasCustomMission(
                            entry?.Challenge
                        ),
                    GeneratedAvailable =
                        MissionProviderSlotStore.HasGeneratedMission(
                            entry?.Challenge
                        )
                };

            if (entry == null)
            {
                return state;
            }

            // Conserva la excepción histórica Original <-> USU (Miner).
            if (
                TryResolveDirectOfficialUsuDescriptor(
                    entry,
                    out OfficialMissionPresetDescriptor directDescriptor
                )
            )
            {
                state.DirectUsuDescriptor = directDescriptor;

                if (
                    MissionPresetLibraryService.TryGetAssignableMissionPreset(
                        directDescriptor.PresetId,
                        out MissionPreset directPreset
                    ) &&
                    directPreset != null
                )
                {
                    state.DirectUsuPreset = directPreset;
                }
            }

            string rememberedPresetId =
                MissionProviderSlotStore.ResolveRememberedUsuPresetId(
                    entry.Challenge
                );

            state.RememberedPresetId = rememberedPresetId;

            if (
                !string.IsNullOrWhiteSpace(rememberedPresetId) &&
                MissionPresetLibraryService.TryGetAssignableMissionPreset(
                    rememberedPresetId,
                    out MissionPreset rememberedPreset
                ) &&
                rememberedPreset != null
            )
            {
                state.RememberedPreset = rememberedPreset;
            }

            // Cualquier Official propio de USU puede aparecer como opción si
            // no existe Original. Con Original sólo se permite la excepción
            // explícita del registry (por ahora Miner). Esto también cubre el
            // caso futuro Generated -> aparece Official USU.
            if (
                OfficialMissionPresetRegistry.TryResolve(
                    entry.BodyName,
                    entry.Source,
                    out OfficialMissionPresetDescriptor officialDescriptor
                ) &&
                officialDescriptor != null &&
                MissionPresetLibraryService.TryGetAssignableMissionPreset(
                    officialDescriptor.PresetId,
                    out MissionPreset officialPreset
                ) &&
                officialPreset != null
            )
            {
                state.UsuTargetPresetId =
                    officialDescriptor.PresetId?.Trim() ?? "";
                state.UsuTargetPreset = officialPreset;
            }
            else if (state.DirectUsuPresetAvailable)
            {
                state.UsuTargetPresetId =
                    state.DirectUsuDescriptor.PresetId?.Trim() ?? "";
                state.UsuTargetPreset = state.DirectUsuPreset;
            }
            else if (
                !originalAvailable ||
                CanAssignUsuPreset(
                    entry,
                    state.RememberedPresetId
                )
            )
            {
                state.UsuTargetPresetId =
                    state.RememberedPresetId;
                state.UsuTargetPreset =
                    state.RememberedPreset;
            }

            return state;
        }
    }
}
