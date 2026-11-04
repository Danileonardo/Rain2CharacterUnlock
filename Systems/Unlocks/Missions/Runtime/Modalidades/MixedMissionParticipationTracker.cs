using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Modos mixtos: el equipo completa la ruta Shared, pero solo reciben
    /// el personaje los usuarios conectados que realizaron el aporte personal.
    /// Evidencia tomada exclusivamente de eventos validos del evaluador servidor.
    /// Sin permisos de cliente para declararse participante.
    /// </summary>
    internal static class MixedMissionParticipationTracker
    {
        private static bool initialized;
        private static ManualLogSource logger;
        private static readonly HashSet<string> WarnedUnresolvedPoisonOwners =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, HashSet<string>> Participants =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        // MissionRuntimeCatalog assigns MissionId = BodyName, not the preset ID.
        // Use that exact key to discard Wooper contributors between stages.
        private const string WooperRuntimeMissionId = "WooperBody";

        internal static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            MissionRunLifecycle.RunStarted += Clear;
            MissionRunLifecycle.RunDestroyed += Clear;
            // Wooper exige 20 presas en un mismo sector: tampoco deben
            // sobrevivir los participantes de una etapa anterior.
            MissionStageRuntimeTracker.StageStarted += OnStageStarted;
        }

        private static void Clear(Run run)
        {
            Participants.Clear();
            WarnedUnresolvedPoisonOwners.Clear();
        }

        private static void OnStageStarted(MissionStageEventContext stage)
        {
            // Las otras misiones mixtas conservan su evidencia a nivel de run.
            // Solo Wooper reinicia su elegibilidad al reiniciar el contador Stage.
            Participants.Remove(WooperRuntimeMissionId);
            WarnedUnresolvedPoisonOwners.Remove(WooperRuntimeMissionId);
        }

        private static bool IsPersonalEvidence(string body, string objective)
        {
            // Gwen se verifica por inventario ACTUAL en el momento final.
            // Kindred: muerte de un jefe distinto con Marca de muerte.
            // Wooper: mordida letal a enemigo envenenado en Humedales.
            // Seamstress: habilidad/cura de REX y remate posterior del jefe.
            switch (body)
            {
                // A diferencia de los eventos compartidos de escenario, estas
                // acciones acreditan exclusivamente a su autor verificable.
                case "DarthVaderBody": return objective == "vader_cube_66";
                case "KnightBody": return objective == "knight_skulls_40";
                case "HornetBody": return objective == "hornet_flying_9";
                case "SpyBody": return objective == "spy_boss";
                case "MinerBody": return objective == "miner_abyssal_boss";
                case "RalseiBody": return objective == "devotion_friends_3";
                case "TinkatonBody": return objective == "scrap_6" ||
                    objective == "mechanical_eye";
                case "KindredBody": return objective == "kindred_unique_boss_hunt";
                case "WooperBody": return objective == "poisoned_bite_20";
                case "SeamstressBody": return objective == "seamstress_rex_spend_health" ||
                    objective == "seamstress_rex_heal_1000" ||
                    objective == "seamstress_boss_after_healing";
                // Warlock: only validated kills in the 500-count route,
                // or the final valid Imp Overlord kill, confer participation.
                case "WarlockBody": return objective == "warlock_five_hundred_souls" ||
                    objective == "warlock_overlord_after_500";
                default: return false;
            }
        }

        internal static void Record(
            MissionObjectiveRuntimeBinding binding,
            MissionCompositionProgressResult result,
            CharacterBody killedBody = null)
        {
            if (!NetworkServer.active || Run.instance == null ||
                binding == null || result == null || !result.Accepted ||
                result.CurrentValue <= result.PreviousValue ||
                result.CompletingNetworkUser == null ||
                !IsPersonalEvidence(binding.BodyName, result.ObjectiveId))
                return;

            // Wooper es Mixed: el contador es Shared (20 presas validas),
            // pero solo los autores verificables de Mordida o veneno de una
            // presa valida reciben la recompensa al completarse la meta.

            MissionPlayerIdentity identity;
            if (!MissionPlayerIdentity.TryCreate(result.CompletingNetworkUser, out identity) ||
                string.IsNullOrWhiteSpace(identity.Key)) return;

            string key = binding.MissionId ?? binding.BodyName;
            HashSet<string> players;
            if (!Participants.TryGetValue(key, out players))
            {
                players = new HashSet<string>(StringComparer.Ordinal);
                Participants[key] = players;
            }
            if (players.Add(identity.Key) && UsuLog.VerboseEnabled)
                UsuLog.Verbose(logger, "[MISSION PARTICIPATION] Acción validada | Body: " +
                    binding.BodyName + " | Objective: " + result.ObjectiveId +
                    " | User: " + result.CompletingNetworkUser.userName +
                    " | Identity: " + identity.Key);

            if (binding.BodyName == "WooperBody" &&
                result.ObjectiveId == "poisoned_bite_20" && killedBody != null)
            {
                List<string> poisonOwners =
                    MissionPreDamageStatusTracker.GetPoisonAppliersBeforeCurrentDamage(killedBody);
                if (poisonOwners.Count == 0 && WarnedUnresolvedPoisonOwners.Add(key))
                    logger?.LogWarning("[MISSION CREDIT AUDIT] Wooper Mordida aceptada pero " +
                        "sin propietario de veneno recuperable; no se fabrican identidades | " +
                        "Target: " + killedBody.name + " | Objective: " + result.ObjectiveId);
                foreach (string poisonOwnerKey in poisonOwners)
                {
                    if (players.Add(poisonOwnerKey) && UsuLog.VerboseEnabled)
                        UsuLog.Verbose(logger, "[MISSION PARTICIPATION] Wooper Veneno | Identity: " + poisonOwnerKey);
                }
            }
        }

        /// <summary>
        /// Crédito adicional exclusivamente para el Mercenary que generó una
        /// atracción física comprobada del Cubo, aunque otro Mercenary remate.
        /// Solo invocable tras que la muerte haya pasado el evaluador host.
        /// </summary>
        internal static void RecordVaderCubeOwner(
            MissionObjectiveRuntimeBinding binding,
            MissionCompositionProgressResult result, CharacterMaster cubeOwner)
        {
            if (!NetworkServer.active || Run.instance == null || binding == null ||
                binding.BodyName != "DarthVaderBody" || cubeOwner == null ||
                result == null || !result.Accepted ||
                result.CurrentValue <= result.PreviousValue ||
                result.ObjectiveId != "vader_cube_66") return;
            NetworkUser user = MissionPlayerIdentity.GetNetworkUser(cubeOwner);
            MissionPlayerIdentity identity;
            if (user == null || !MissionPlayerIdentity.TryCreate(user, out identity) ||
                string.IsNullOrWhiteSpace(identity.Key)) return;
            string key = binding.MissionId ?? binding.BodyName;
            HashSet<string> participants;
            if (!Participants.TryGetValue(key, out participants))
            {
                participants = new HashSet<string>(StringComparer.Ordinal);
                Participants[key] = participants;
            }
            participants.Add(identity.Key);
        }

        // Solo se incorporan participantes de una oleada Sora COMPLETA
        // cuando el evaluador acepta el avance. No se atribuyen muertes
        // sin dueño verificado ni se acredita al primer jugador por defecto.
        internal static void RecordSoraWave(
            MissionObjectiveRuntimeBinding binding,
            MissionCompositionProgressResult result,
            IEnumerable<string> verifiedContributorKeys)
        {
            if (!NetworkServer.active || Run.instance == null ||
                binding == null || binding.BodyName != "SoraBody" ||
                result == null || !result.Accepted ||
                result.ObjectiveId != "vengeance_waves_3" ||
                result.CurrentValue <= result.PreviousValue ||
                verifiedContributorKeys == null)
                return;

            string key = binding.MissionId ?? binding.BodyName;
            HashSet<string> players;
            if (!Participants.TryGetValue(key, out players))
            {
                players = new HashSet<string>(StringComparer.Ordinal);
                Participants[key] = players;
            }
            foreach (string contributor in verifiedContributorKeys)
            {
                if (!string.IsNullOrWhiteSpace(contributor))
                    players.Add(contributor);
            }
        }

        internal static List<NetworkUser> EligibleRecipients(
            MissionObjectiveRuntimeBinding binding)
        {
            var recipients = new List<NetworkUser>();
            if (!NetworkServer.active || binding == null || Run.instance == null)
                return recipients;

            // Fail closed: no premiar espectadores ni misiones sin política
            // de atribución explícita. AH-64 posee progresión de campaña propia.
            string body = binding.BodyName;
            if (body != "DarthVaderBody" && body != "KnightBody" &&
                body != "HornetBody" && body != "GwenBody" && body != "KindredBody" &&
                body != "WooperBody" && body != "SeamstressBody" &&
                body != "WarlockBody" && body != "SpyBody" &&
                body != "RobHunkBody" && body != "RalseiBody" &&
                body != "SoraBody" && body != "MinerBody" &&
                body != "TinkatonBody")
                return recipients;

            HashSet<string> participants;
            Participants.TryGetValue(binding.MissionId ?? body, out participants);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (CharacterMaster master in MissionConnectedPlayerEnumerator.GetMasters())
            {
                NetworkUser user = master == null ? null :
                    MissionPlayerIdentity.GetNetworkUser(master);
                if (user?.gameObject == null) continue;
                MissionPlayerIdentity identity;
                if (!MissionPlayerIdentity.TryCreate(user, out identity) ||
                    string.IsNullOrWhiteSpace(identity.Key) ||
                    !seen.Add(identity.Key)) continue;

                bool eligible;
                string reason;
                if (body == "GwenBody")
                {
                    eligible = HoldsGwenDaggers(master);
                    reason = eligible ? "holds_two_daggers" : "missing_two_daggers";
                }
                else if (body == "RobHunkBody")
                {
                    // Dos rutas OR: únicamente el portador que NO perdió
                    // la batería ni murió en la ruta que acaba de terminar.
                    eligible = HoldsValidHunkBattery(binding, master);
                    reason = eligible ? "valid_route_battery_carrier" : "invalid_or_no_battery";
                }
                else
                {
                    eligible = participants != null && participants.Contains(identity.Key);
                    // HoldItemStack es un estado actual; una segunda persona
                    // puede aportar el martillo sin causar un aumento del
                    // contador Shared si otra ya tenía el mismo objeto.
                    if (body == "TinkatonBody" && HoldsTinkatonHammer(master))
                        eligible = true;
                    reason = eligible ? "validated_contribution" : "no_personal_contribution_recorded";
                }
                if (UsuLog.VerboseEnabled)
                    UsuLog.Verbose(logger, "[MISSION CREDIT AUDIT] Mixed | Body: " + body +
                    " | User: " + user.userName + " | Identity: " + identity.Key +
                    " | Eligible: " + eligible + " | Reason: " +
                    reason);
                if (eligible) recipients.Add(user);
            }
            return recipients;
        }

        private static bool HoldsValidHunkBattery(
            MissionObjectiveRuntimeBinding endingBinding,
            CharacterMaster master)
        {
            if (endingBinding == null || master == null) return false;
            // La elegibilidad no se reutiliza entre las rutas de Luna y
            // Obelisco, aun cuando pertenezcan a una misma misión.
            foreach (MissionObjectiveRuntimeBinding carryBinding in
                MissionRuntimeCatalog.GetObjectives("CarryEquipment"))
            {
                if (carryBinding == null ||
                    carryBinding.MissionId != endingBinding.MissionId ||
                    carryBinding.RouteIndex != endingBinding.RouteIndex)
                    continue;

                if (CarryEquipmentObjectiveHandler.IsValidCarrier(
                    carryBinding, master))
                    return true;
            }
            return false;
        }

        private static bool HoldsTinkatonHammer(CharacterMaster master)
        {
            if (master?.inventory == null) return false;
            ItemIndex item = ItemCatalog.FindItemIndex("ArmorReductionOnHit");
            return item != ItemIndex.None &&
                master.inventory.GetItemCountPermanent(item) > 0;
        }

        private static bool HoldsGwenDaggers(CharacterMaster master)
        {
            if (master?.inventory == null) return false;
            ItemIndex dagger = ItemCatalog.FindItemIndex("Dagger");
            return dagger != ItemIndex.None &&
                master.inventory.GetItemCountPermanent(dagger) >= 2;
        }
    }
}
