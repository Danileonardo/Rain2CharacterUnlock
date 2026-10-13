using System;

using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Generación manual de un slot Generated desde Mission Library.
    ///
    /// A diferencia del fallback automático, este flujo también puede crear
    /// una Generated para un survivor modded que ya tenga Original/USU. En ese
    /// caso la misión se guarda como slot alternativo y sólo toma autoridad
    /// cuando el jugador la selecciona explícitamente.
    /// </summary>
    public static class GeneratedMissionManualGenerationService
    {
        public sealed class GenerateResult
        {
            public bool Success;
            public string Code = "";
            public string Message = "";
        }

        public static GenerateResult GenerateAndSelect(
            string bodyName,
            ManualLogSource logger = null
        )
        {
            string normalized = bodyName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return Fail("invalid_body", "BodyName vacío.");
            }

            if (Run.instance != null)
            {
                return Fail(
                    "run_active",
                    "No se puede generar una misión durante una run."
                );
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(normalized);

            if (entry == null)
            {
                return Fail(
                    "not_found",
                    "El survivor no está registrado en Survivors.json."
                );
            }

            MissionProviderSelectionState current =
                MissionProviderSelectionPolicy.ResolveFromStoredEntry(entry);

            if (current.GeneratedAvailable)
            {
                MissionAssignmentResult existing =
                    MissionAssignmentService.SelectGenerated(
                        normalized,
                        logger
                    );

                return existing != null && existing.Success
                    ? Success("selected", "Generated seleccionada.")
                    : Fail(
                        existing?.Code ?? "select_failed",
                        existing?.Message ?? "No se pudo seleccionar Generated."
                    );
            }

            if (
                entry.Challenge?.MissionConfig != null &&
                entry.Challenge.MissionConfig.GeneratedSuppressed
            )
            {
                entry.Challenge.MissionConfig.GeneratedSuppressed = false;

                if (!SurvivorJsonManager.TrySaveCurrentConfig(logger))
                {
                    return Fail(
                        "save_failed",
                        "No se pudo reactivar la generación en Survivors.json."
                    );
                }
            }

            if (
                !GeneratedMissionRuntimeMaterializer.TryGenerateForBodyOnDemand(
                    normalized,
                    logger,
                    out string generationError
                )
            )
            {
                return Fail(
                    "generation_failed",
                    string.IsNullOrWhiteSpace(generationError)
                        ? "No se pudo crear una misión Generated válida."
                        : generationError
                );
            }

            MissionAssignmentResult select =
                MissionAssignmentService.SelectGenerated(
                    normalized,
                    logger
                );

            if (select == null || !select.Success)
            {
                return Fail(
                    select?.Code ?? "select_failed",
                    select?.Message ??
                    "La misión se creó, pero no pudo activarse."
                );
            }

            UsuLog.Diagnostic(
                logger,
                "[GENERATED CREATE] Generación manual completada | Body: " +
                normalized
            );

            return Success(
                "generated",
                "Misión Generated creada y seleccionada."
            );
        }

        private static GenerateResult Success(string code, string message)
        {
            return new GenerateResult
            {
                Success = true,
                Code = code ?? "ok",
                Message = message ?? ""
            };
        }

        private static GenerateResult Fail(string code, string message)
        {
            return new GenerateResult
            {
                Success = false,
                Code = code ?? "error",
                Message = message ?? ""
            };
        }
    }
}
