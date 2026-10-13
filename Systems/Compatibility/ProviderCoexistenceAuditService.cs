using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// C4 - auditoría de coexistencia de providers.
    ///
    /// Verifica de forma pasiva que Original / USU / Custom / Generated puedan
    /// coexistir sin que una fuente destruya las demás. Nunca persiste cambios
    /// ni altera el provider activo. Para probar restauraciones usa clones en
    /// memoria de cada challenge.
    /// </summary>
    public static class ProviderCoexistenceAuditService
    {
        private sealed class AuditTotals
        {
            public int Entries;
            public int WithOriginal;
            public int WithUsu;
            public int WithCustom;
            public int WithGenerated;
            public int MultiSlot;
            public int GeneratedRestorable;
            public int CustomRestorable;
            public int GeneratedSnapshotMismatch;
            public int InvalidActiveSource;
            public int InvalidProviderSource;
            public int RerollReady;
            public int RerollBlocked;
        }

        public static void Audit(
            ManualLogSource logger
        )
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;

            if (config == null)
            {
                UsuLog.Diagnostic(
                logger,
                    "[PROVIDER COEXISTENCE] C4 sin Survivors.json cargado; auditoría omitida."
                );
                return;
            }

            List<SurvivorJsonEntry> entries =
                EnumerateEntries(config)
                    .Where(entry => entry != null)
                    .ToList();

            AuditTotals totals = new AuditTotals();

            foreach (SurvivorJsonEntry entry in entries)
            {
                AuditEntry(entry, totals, logger);
            }

            UsuLog.Diagnostic(
                logger,
                "[PROVIDER COEXISTENCE] C4 auditado | " +
                $"Entradas: {totals.Entries} | " +
                $"Original: {totals.WithOriginal} | " +
                $"USU: {totals.WithUsu} | " +
                $"Custom: {totals.WithCustom} | " +
                $"Generated: {totals.WithGenerated} | " +
                $"Multi-slot: {totals.MultiSlot} | " +
                $"Generated restaurable: {totals.GeneratedRestorable} | " +
                $"Custom restaurable: {totals.CustomRestorable} | " +
                $"Reroll ready/bloqueado: {totals.RerollReady}/{totals.RerollBlocked} | " +
                $"Mismatch Generated: {totals.GeneratedSnapshotMismatch} | " +
                $"Source inválido: {totals.InvalidActiveSource} | " +
                $"Provider/Source inválido: {totals.InvalidProviderSource} | " +
                "Persistencia modificada: no"
            );
        }

        private static void AuditEntry(
            SurvivorJsonEntry entry,
            AuditTotals totals,
            ManualLogSource logger
        )
        {
            totals.Entries++;

            SurvivorChallengeJson challenge = entry.Challenge;
            MissionConfiguration missionConfig = challenge?.MissionConfig;

            bool original =
                UnlockProviderPolicy.HasStoredOriginalUnlock(entry);

            bool custom =
                MissionProviderSlotStore.HasCustomMission(challenge);

            bool generated =
                MissionProviderSlotStore.HasGeneratedMission(challenge);

            MissionProviderSelectionState state =
                MissionProviderSelectionPolicy.ResolveFromStoredEntry(entry);

            bool usu =
                state.UsuTargetAvailable ||
                state.DirectUsuPresetAvailable ||
                (
                    missionConfig != null &&
                    string.Equals(
                        missionConfig.Source,
                        "Preset",
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (original) totals.WithOriginal++;
            if (usu) totals.WithUsu++;
            if (custom) totals.WithCustom++;
            if (generated) totals.WithGenerated++;

            int slotCount =
                (original ? 1 : 0) +
                (usu ? 1 : 0) +
                (custom ? 1 : 0) +
                (generated ? 1 : 0);

            if (slotCount >= 2)
            {
                totals.MultiSlot++;
            }

            bool generatedRestorable =
                TestGeneratedRestore(challenge);

            bool customRestorable =
                TestCustomRestore(challenge);

            if (generatedRestorable)
            {
                totals.GeneratedRestorable++;
            }

            if (customRestorable)
            {
                totals.CustomRestorable++;
            }

            bool generatedSnapshotMismatch =
                HasGeneratedSnapshotMismatch(challenge);

            if (generatedSnapshotMismatch)
            {
                totals.GeneratedSnapshotMismatch++;
            }

            string source =
                missionConfig?.Source?.Trim() ?? "";

            bool validSource =
                string.IsNullOrWhiteSpace(source) ||
                string.Equals(source, "Preset", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(source, "Generated", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(source, "Custom", StringComparison.OrdinalIgnoreCase);

            if (!validSource)
            {
                totals.InvalidActiveSource++;
            }

            if (!IsProviderSourceConsistent(missionConfig))
            {
                totals.InvalidProviderSource++;
            }

            bool rerollReady =
                IsGeneratedRerollReady(challenge);

            if (generated)
            {
                if (rerollReady)
                {
                    totals.RerollReady++;
                }
                else
                {
                    totals.RerollBlocked++;
                }
            }

            if (
                slotCount >= 2 ||
                generated ||
                custom ||
                state.DirectUsuAlternativeDeclared
            )
            {
                UsuLog.Verbose(
                    logger,
                    "[PROVIDER COEXISTENCE] " +
                    $"{entry.BodyName} | " +
                    $"Activo: {state.ActiveProvider}/{source} | " +
                    $"Slots O/U/C/G: {Bool(original)}/{Bool(usu)}/{Bool(custom)}/{Bool(generated)} | " +
                    $"USU preset: {state.UsuTargetPresetId ?? ""} | " +
                    $"Generated restore: {Bool(generatedRestorable)} | " +
                    $"Custom restore: {Bool(customRestorable)} | " +
                    $"Reroll: {(generated ? (rerollReady ? "READY" : "BLOCKED") : "N/A")} | " +
                    $"Nonce: {ResolveGenerationNonce(challenge)}"
                );
            }
        }

        private static IEnumerable<SurvivorJsonEntry> EnumerateEntries(
            SurvivorJsonFile config
        )
        {
            if (config?.AvailableSurvivors != null)
            {
                foreach (SurvivorJsonEntry entry in config.AvailableSurvivors.Values)
                {
                    yield return entry;
                }
            }

            if (config?.UnavailableSurvivors != null)
            {
                foreach (SurvivorJsonEntry entry in config.UnavailableSurvivors.Values)
                {
                    yield return entry;
                }
            }
        }

        private static bool TestGeneratedRestore(
            SurvivorChallengeJson challenge
        )
        {
            if (!MissionProviderSlotStore.HasGeneratedMission(challenge))
            {
                return false;
            }

            SurvivorChallengeJson clone =
                CloneChallenge(challenge);

            if (clone == null)
            {
                return false;
            }

            MissionProviderSlotStore.CaptureActiveSlots(clone);

            return
                MissionProviderSlotStore.RestoreGenerated(clone) &&
                string.Equals(
                    clone.MissionConfig?.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                clone.Mission != null;
        }

        private static bool TestCustomRestore(
            SurvivorChallengeJson challenge
        )
        {
            if (!MissionProviderSlotStore.HasCustomMission(challenge))
            {
                return false;
            }

            SurvivorChallengeJson clone =
                CloneChallenge(challenge);

            if (clone == null)
            {
                return false;
            }

            MissionProviderSlotStore.CaptureActiveSlots(clone);

            return
                MissionProviderSlotStore.RestoreCustom(clone) &&
                string.Equals(
                    clone.MissionConfig?.Source,
                    "Custom",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                clone.Mission != null;
        }

        private static bool HasGeneratedSnapshotMismatch(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config = challenge?.MissionConfig;

            if (
                config == null ||
                !string.Equals(
                    config.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                challenge.Mission == null ||
                config.GeneratedMission?.Mission == null
            )
            {
                return false;
            }

            string active =
                JsonConvert.SerializeObject(
                    challenge.Mission,
                    Formatting.None
                );

            string snapshot =
                JsonConvert.SerializeObject(
                    config.GeneratedMission.Mission,
                    Formatting.None
                );

            return !string.Equals(
                active,
                snapshot,
                StringComparison.Ordinal
            );
        }

        private static bool IsProviderSourceConsistent(
            MissionConfiguration config
        )
        {
            if (config == null)
            {
                return true;
            }

            string source = config.Source?.Trim() ?? "";

            if (
                string.Equals(source, "Custom", StringComparison.OrdinalIgnoreCase)
            )
            {
                return
                    !config.Provider.HasValue ||
                    config.Provider.Value == UnlockProviderKind.Custom;
            }

            if (
                string.Equals(source, "Generated", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(source, "Preset", StringComparison.OrdinalIgnoreCase)
            )
            {
                return
                    !config.Provider.HasValue ||
                    UnlockProviderPolicy.IsUsuFamily(config.Provider.Value);
            }

            return true;
        }

        private static bool IsGeneratedRerollReady(
            SurvivorChallengeJson challenge
        )
        {
            if (!MissionProviderSlotStore.HasGeneratedMission(challenge))
            {
                return false;
            }

            string nonce = ResolveGenerationNonce(challenge);

            return !string.IsNullOrWhiteSpace(nonce);
        }

        private static string ResolveGenerationNonce(
            SurvivorChallengeJson challenge
        )
        {
            string activeNonce =
                challenge?.Parameters?
                    .Value<string>("generationNonce")?
                    .Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(activeNonce))
            {
                return activeNonce;
            }

            string snapshotNonce =
                challenge?.MissionConfig?
                    .GeneratedMission?
                    .Parameters?
                    .Value<string>("generationNonce")?
                    .Trim() ?? "";

            return snapshotNonce;
        }

        private static SurvivorChallengeJson CloneChallenge(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return null;
            }

            try
            {
                string json =
                    JsonConvert.SerializeObject(
                        challenge,
                        Formatting.None
                    );

                return JsonConvert.DeserializeObject<SurvivorChallengeJson>(json);
            }
            catch
            {
                return null;
            }
        }

        private static string Bool(bool value) => value ? "SI" : "NO";
    }
}
