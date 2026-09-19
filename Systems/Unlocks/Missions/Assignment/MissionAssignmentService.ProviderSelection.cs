using System;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    public static partial class MissionAssignmentService
    {
        /// <summary>
        /// Punto único para cambiar entre los providers seleccionables desde la
        /// Mission Library actual. CUSTOM se habilitará en su fase propia.
        ///
        /// La resolución del preset objetivo vive en
        /// MissionProviderSelectionPolicy; este método sólo ejecuta la acción.
        /// </summary>
        public static MissionAssignmentResult SelectProvider(
            string bodyName,
            UnlockProviderKind provider,
            ManualLogSource logger = null
        )
        {
            string normalizedBodyName =
                bodyName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(normalizedBodyName))
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.InvalidBodyName,
                    "El BodyName del survivor no puede estar vacío."
                );
            }

            if (provider == UnlockProviderKind.Original)
            {
                return SelectOriginal(
                    normalizedBodyName,
                    logger
                );
            }

            if (
                provider != UnlockProviderKind.USU &&
                provider != UnlockProviderKind.Community
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.ProviderNotSelectable,
                    $"El provider '{provider}' todavía no puede seleccionarse desde la biblioteca.",
                    normalizedBodyName
                );
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    normalizedBodyName
                );

            if (entry == null)
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.SurvivorNotFound,
                    $"No existe una configuración de USU para '{normalizedBodyName}'.",
                    normalizedBodyName
                );
            }

            MissionProviderSelectionState state =
                MissionProviderSelectionPolicy.ResolveFromStoredEntry(
                    entry
                );

            if (state.DirectUsuAlternativeDeclared)
            {
                return SelectOfficialUsuAlternative(
                    normalizedBodyName,
                    logger
                );
            }

            if (string.IsNullOrWhiteSpace(state.UsuTargetPresetId))
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.PresetNotAssignable,
                    "No existe un preset USU recordado para reactivar. Selecciona una misión de la biblioteca.",
                    normalizedBodyName
                );
            }

            return AssignPreset(
                normalizedBodyName,
                state.UsuTargetPresetId,
                logger
            );
        }
    }
}
