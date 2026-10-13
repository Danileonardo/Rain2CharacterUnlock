using System;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Borra exclusivamente el slot Generated cuando el survivor conserva una
    /// alternativa segura Original o USU. Los survivors cuyo único desbloqueo
    /// es Generated no pueden quedarse sin misión desde esta acción.
    /// </summary>
    public static class GeneratedMissionDeletionService
    {
        public sealed class DeleteResult
        {
            public bool Success;
            public string Code = "";
            public string Message = "";
        }

        public static bool CanDelete(string bodyName)
        {
            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(bodyName);

            if (entry == null || !MissionProviderSlotStore.HasGeneratedMission(entry.Challenge))
            {
                return false;
            }

            MissionProviderSelectionState state =
                MissionProviderSelectionPolicy.ResolveFromStoredEntry(entry);

            return state.OriginalAvailable || state.UsuTargetAvailable;
        }

        public static DeleteResult Delete(
            string bodyName,
            ManualLogSource logger = null
        )
        {
            string normalized = bodyName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return Fail("invalid_body", "BodyName vacío.");
            }

            if (RoR2.Run.instance != null)
            {
                return Fail("run_active", "No se puede borrar una misión Generated durante una run.");
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(normalized);

            if (entry?.Challenge?.MissionConfig == null)
            {
                return Fail("not_found", "No existe una misión Generated guardada para este survivor.");
            }

            MissionProviderSelectionState state =
                MissionProviderSelectionPolicy.ResolveFromStoredEntry(entry);

            if (!state.GeneratedAvailable)
            {
                return Fail("no_generated", "No existe un slot Generated para borrar.");
            }

            if (!state.OriginalAvailable && !state.UsuTargetAvailable)
            {
                return Fail(
                    "required_fallback",
                    "Generated es la única misión disponible y se conserva como fallback."
                );
            }

            // Si Generated está activa, primero cambia a una fuente segura.
            if (state.IsGeneratedActive)
            {
                MissionAssignmentResult switchResult =
                    state.OriginalAvailable
                        ? MissionAssignmentService.SelectOriginal(normalized, logger)
                        : MissionAssignmentService.SelectProvider(
                            normalized,
                            UnlockProviderKind.USU,
                            logger
                        );

                if (switchResult == null || !switchResult.Success)
                {
                    return Fail(
                        "provider_switch_failed",
                        switchResult?.Message ?? "No se pudo activar la misión alternativa."
                    );
                }

                entry = SurvivorJsonManager.GetEntryAnywhere(normalized);
            }

            MissionConfiguration config =
                entry?.Challenge?.MissionConfig;

            if (config == null)
            {
                return Fail("missing_config", "MissionConfig no está disponible.");
            }

            config.GeneratedMission = null;
            config.GeneratedSuppressed = true;

            if (!SurvivorJsonManager.TrySaveCurrentConfig(logger))
            {
                return Fail("save_failed", "No se pudo guardar Survivors.json.");
            }

            UsuLog.Diagnostic(
                logger,
                "[GENERATED DELETE] Slot Generated eliminado | Body: " + normalized
            );

            return new DeleteResult
            {
                Success = true,
                Code = "deleted",
                Message = "Misión Generated eliminada."
            };
        }

        private static DeleteResult Fail(string code, string message)
        {
            return new DeleteResult
            {
                Success = false,
                Code = code ?? "error",
                Message = message ?? ""
            };
        }
    }
}
