using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// C2.6 - Final compatibility invariant for current official content.
    /// Verifies that official/vanilla-DLC survivors never become Generated
    /// targets and that USU did not persist Generated authority for them.
    /// Read-only by design: failures are surfaced loudly rather than silently
    /// rewriting user data.
    /// </summary>
    public static class OfficialContentAuthorityInvariantService
    {
        public static void Audit(
            IReadOnlyList<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            if (survivors == null)
            {
                return;
            }

            IReadOnlyList<GeneratedMissionCandidate> candidates =
                DeterministicMissionGeneratorService.GetAll();

            int official = 0;
            int generatedCandidates = 0;
            int generatedPersisted = 0;
            int originalPreserved = 0;

            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorInfo survivor = survivors[i];
                if (survivor == null || survivor.SurvivorDef == null || survivor.IsModded)
                {
                    continue;
                }

                official++;

                if (survivor.HasOriginalUnlock)
                {
                    originalPreserved++;
                }

                if (HasGeneratedCandidate(candidates, survivor.BodyName))
                {
                    generatedCandidates++;
                    logger?.LogError(
                        "[OFFICIAL AUTHORITY GUARD] VIOLATION | " +
                        survivor.BodyName +
                        " es contenido oficial pero recibió candidato Generated."
                    );
                }

                SurvivorJsonEntry entry =
                    SurvivorJsonManager.GetEntryAnywhere(survivor.BodyName);

                string source =
                    entry?.Challenge?.MissionConfig?.Source ?? "";

                if (
                    string.Equals(
                        source,
                        "Generated",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    generatedPersisted++;
                    logger?.LogError(
                        "[OFFICIAL AUTHORITY GUARD] VIOLATION | " +
                        survivor.BodyName +
                        " es contenido oficial pero Survivors.json conserva Source=Generated."
                    );
                }
            }

            bool ok = generatedCandidates == 0 && generatedPersisted == 0;

            UsuLog.Diagnostic(
                logger,
                "[OFFICIAL AUTHORITY GUARD] C2.6 " +
                (ok ? "OK" : "FAILED") +
                " | Oficiales: " + official +
                " | Con Original preservado: " + originalPreserved +
                " | Generated candidates indebidos: " + generatedCandidates +
                " | Generated persistidos indebidos: " + generatedPersisted +
                " | Corrección automática: no"
            );
        }

        private static bool HasGeneratedCandidate(
            IReadOnlyList<GeneratedMissionCandidate> candidates,
            string bodyName
        )
        {
            if (candidates == null || string.IsNullOrWhiteSpace(bodyName))
            {
                return false;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                GeneratedMissionCandidate candidate = candidates[i];
                if (
                    candidate != null &&
                    string.Equals(
                        candidate.BodyName,
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
    }
}
