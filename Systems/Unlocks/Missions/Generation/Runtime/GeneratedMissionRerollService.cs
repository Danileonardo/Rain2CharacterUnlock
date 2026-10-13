using System;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G8 Phase 1.6 - reroll manual de una misión Generated.
    ///
    /// Cambia únicamente generationNonce, vuelve a ejecutar G7/G8 y persiste
    /// la nueva Mission V2 en Survivors.json. No convierte la misión a Custom.
    /// Original, Official y Custom quedan fuera de este flujo.
    /// </summary>
    public static class GeneratedMissionRerollService
    {
        private const int MaxPreviewAttempts = 12;


        public sealed class RerollResult
        {
            public bool Success;
            public bool PresentationChanged;
            public string Code = "";
            public string Message = "";
            public string OldName = "";
            public string OldDescription = "";
            public string NewName = "";
            public string NewDescription = "";
            public string OldNonce = "";
            public string NewNonce = "";
        }


        public static RerollResult Reroll(
            string bodyName,
            ManualLogSource logger = null
        )
        {
            RerollResult result =
                new RerollResult();

            string normalizedBodyName =
                bodyName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(normalizedBodyName))
            {
                result.Code = "invalid_body";
                result.Message = "BodyName vacío.";
                return result;
            }

            if (Run.instance != null)
            {
                result.Code = "run_active";
                result.Message =
                    "No se puede rerollear una misión Generated durante una run.";
                return result;
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    normalizedBodyName
                );

            SurvivorChallengeJson challenge =
                entry?.Challenge;

            MissionConfiguration config =
                challenge?.MissionConfig;

            if (
                entry == null ||
                challenge == null ||
                config == null ||
                !string.Equals(
                    config.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                result.Code = "not_generated";
                result.Message =
                    "La misión activa no está administrada como Generated.";
                return result;
            }

            if (UnlockProviderPolicy.HasStoredOriginalUnlock(entry))
            {
                result.Code = "original_available";
                result.Message =
                    "Existe un Original y la Generated no puede rerollearse.";
                return result;
            }

            if (challenge.Parameters == null)
            {
                challenge.Parameters =
                    new JObject();
            }

            result.OldNonce =
                challenge.Parameters
                    .Value<string>("generationNonce")?
                    .Trim()
                ?? "";

            ResolvePresentation(
                challenge,
                out result.OldName,
                out result.OldDescription
            );

            GeneratedMissionCandidate preview = null;
            string selectedNonce = "";
            bool previewChanged = false;

            string oldSignature =
                BuildChallengeSignature(challenge);

            for (int attempt = 0; attempt < MaxPreviewAttempts; attempt++)
            {
                string nonce =
                    Guid.NewGuid().ToString("N");

                if (
                    !DeterministicMissionGeneratorService.TryPreviewForBody(
                        normalizedBodyName,
                        nonce,
                        out GeneratedMissionCandidate candidate
                    ) ||
                    candidate == null
                )
                {
                    continue;
                }

                preview = candidate;
                selectedNonce = nonce;

                if (
                    !string.Equals(
                        oldSignature,
                        BuildCandidateSignature(candidate),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    previewChanged = true;
                    break;
                }
            }

            if (preview == null || string.IsNullOrWhiteSpace(selectedNonce))
            {
                result.Code = "generation_failed";
                result.Message =
                    "G7 no pudo producir un candidato Generated válido.";
                return result;
            }

            challenge.Parameters["generationNonce"] =
                selectedNonce;

            // El cambio del nonce lo realiza USU, no el jugador. Re-sellamos el
            // snapshot intermedio para que el detector Generated -> Custom no lo
            // interprete como una edición manual si otro refresh ocurre antes de
            // que G8 escriba la nueva Mission V2 completa.
            GeneratedMissionPersistenceFingerprint
                .Stamp(challenge);

            if (!DeterministicMissionGeneratorService.RebuildCurrent())
            {
                result.Code = "generator_context_missing";
                result.Message =
                    "El contexto de G7 no está disponible para reroll.";
                return result;
            }

            if (!GeneratedMissionRuntimeMaterializer.RebuildCurrent())
            {
                result.Code = "materializer_context_missing";
                result.Message =
                    "El contexto de G8 no está disponible para reroll.";
                return result;
            }

            GeneratedMissionRuntimeMaterializer
                .PersistGeneratedMissions();

            SurvivorJsonEntry refreshed =
                SurvivorJsonManager.GetEntryAnywhere(
                    normalizedBodyName
                );

            SurvivorChallengeJson refreshedChallenge =
                refreshed?.Challenge;

            if (
                refreshedChallenge == null ||
                refreshedChallenge.MissionConfig == null ||
                !string.Equals(
                    refreshedChallenge.MissionConfig.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                result.Code = "persistence_failed";
                result.Message =
                    "La nueva misión no quedó persistida como Generated.";
                return result;
            }

            result.NewNonce =
                refreshedChallenge.Parameters?
                    .Value<string>("generationNonce")?
                    .Trim()
                ?? selectedNonce;

            ResolvePresentation(
                refreshedChallenge,
                out result.NewName,
                out result.NewDescription
            );

            result.PresentationChanged =
                !string.Equals(
                    result.OldName,
                    result.NewName,
                    StringComparison.Ordinal
                ) ||
                !string.Equals(
                    result.OldDescription,
                    result.NewDescription,
                    StringComparison.Ordinal
                );

            MissionRuntimeRefreshService
                .ApplyBodyConfiguration(
                    normalizedBodyName,
                    logger,
                    "generated reroll"
                );

            result.Success = true;
            result.Code =
                result.PresentationChanged || previewChanged
                    ? "rerolled"
                    : "rerolled_same_presentation";

            result.Message =
                result.PresentationChanged
                    ? "Misión Generated actualizada."
                    : "Reroll aplicado; el perfil sólo ofreció una presentación equivalente.";

            logger?.LogInfo(
                "[GENERATED REROLL] " +
                $"Body: {normalizedBodyName} | " +
                $"Old: {result.OldName} | New: {result.NewName} | " +
                $"PresentationChanged: {(result.PresentationChanged ? "yes" : "no")} | " +
                $"Nonce: {result.OldNonce} -> {result.NewNonce}"
            );

            return result;
        }


        private static string BuildChallengeSignature(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return "";
            }

            string family =
                challenge.Parameters?
                    .Value<string>("generatedFamily")?
                    .Trim()
                ?? "";

            int amount =
                challenge.Parameters?
                    .Value<int?>("amount")
                ?? 0;

            string resetScope =
                challenge.Parameters?
                    .Value<string>("resetScope")?
                    .Trim()
                ?? "";

            return
                family + "|" +
                amount + "|" +
                resetScope;
        }


        private static string BuildCandidateSignature(
            GeneratedMissionCandidate candidate
        )
        {
            if (candidate == null)
            {
                return "";
            }

            return
                candidate.Family + "|" +
                (candidate.Parameters?.Amount ?? 0) + "|" +
                (candidate.Parameters?.ResetScope ?? "");
        }


        private static void ResolvePresentation(
            SurvivorChallengeJson challenge,
            out string name,
            out string description
        )
        {
            name = challenge?.Name ?? "";
            description = challenge?.Description ?? "";

            if (
                challenge != null &&
                GeneratedMissionPresentationFormatter.TryResolve(
                    challenge,
                    out string generatedName,
                    out string generatedDescription
                )
            )
            {
                name = generatedName ?? "";
                description = generatedDescription ?? "";
            }
        }
    }
}
