using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Contribuciones verificadas por el host para misiones PerPlayer.
    /// Una contribución debe haberse aceptado por el evaluador. Los asistentes
    /// de un jefe se añaden únicamente cuando su muerte satisface el objetivo.
    /// La recompensa se restringe a la misma misión y la misma ruta OR.
    /// </summary>
    internal static class MissionContributionLedger
    {
        private static ManualLogSource logger;
        private static bool initialized;

        private sealed class Contribution
        {
            internal string DisplayName;
            internal readonly HashSet<string> Evidence = new HashSet<string>(StringComparer.Ordinal);
        }

        private static readonly Dictionary<string, Dictionary<string, Contribution>> ByRoute =
            new Dictionary<string, Dictionary<string, Contribution>>(StringComparer.Ordinal);

        internal static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            MissionRunLifecycle.RunStarted += Clear;
            MissionRunLifecycle.RunDestroyed += Clear;
        }

        private static void Clear(Run run) { ByRoute.Clear(); }

        private static string RouteKey(MissionObjectiveRuntimeBinding binding, string routeId)
        {
            return (binding?.MissionId ?? binding?.BodyName ?? "") + "::" + (routeId ?? "");
        }

        private static void AddIdentity(MissionObjectiveRuntimeBinding binding, string routeId,
            string identityKey, string displayName, string evidence)
        {
            if (string.IsNullOrWhiteSpace(identityKey)) return;
            string key = RouteKey(binding, routeId);
            Dictionary<string, Contribution> group;
            if (!ByRoute.TryGetValue(key, out group))
            {
                group = new Dictionary<string, Contribution>(StringComparer.Ordinal);
                ByRoute.Add(key, group);
            }
            Contribution existing;
            if (!group.TryGetValue(identityKey, out existing))
            {
                existing = new Contribution { DisplayName = displayName ?? identityKey };
                group.Add(identityKey, existing);
            }
            if (existing.Evidence.Add(evidence) && UsuLog.VerboseEnabled)
                UsuLog.Verbose(logger, "[MISSION CREDIT] Contribución validada | Body: " + binding.BodyName +
                    " | Route: " + routeId + " | Identity: " + identityKey +
                    " | User: " + existing.DisplayName + " | Evidence: " + evidence);
        }

        internal static void Record(MissionObjectiveRuntimeBinding binding,
            MissionCompositionProgressResult result)
        {
            if (!NetworkServer.active || Run.instance == null || binding == null ||
                result == null || !result.Accepted ||
                result.ProgressScope != MissionProgressScope.PerPlayer ||
                result.CompletingNetworkUser == null ||
                result.CurrentValue <= result.PreviousValue) return;

            // Scout: tener una bebida no es ayudar a completar una ruta alternativa.
            // El requisito de inventario o teleporter debe cumplirse por jugador.
            if (binding.BodyName == "ScoutBody" && !result.ObjectiveCompleted) return;

            MissionPlayerIdentity identity;
            if (!MissionPlayerIdentity.TryCreate(result.CompletingNetworkUser, out identity)) return;
            AddIdentity(binding, result.RouteId, identity.Key,
                result.CompletingNetworkUser.userName, "objective:" + result.ObjectiveId);
        }

        /// <summary>
        /// Atribuye la atraccion del Cubo a otro Mercenary, pero solo si
        /// el evaluador del host acepto efectivamente la baja de Vader.
        /// Nunca concede progreso ni recompensas por si mismo.
        /// </summary>
        internal static void RecordValidatedVaderCubeOwner(
            MissionObjectiveRuntimeBinding binding,
            MissionCompositionProgressResult result,
            CharacterMaster owner)
        {
            if (!NetworkServer.active || Run.instance == null || owner == null ||
                binding == null || binding.BodyName != "DarthVaderBody" ||
                result == null || !result.Accepted ||
                result.CurrentValue <= result.PreviousValue ||
                result.ProgressScope != MissionProgressScope.PerPlayer ||
                result.ObjectiveId != "vader_cube_66") return;

            NetworkUser user = MissionPlayerIdentity.GetNetworkUser(owner);
            MissionPlayerIdentity identity;
            if (user == null || !MissionPlayerIdentity.TryCreate(user, out identity) ||
                string.IsNullOrWhiteSpace(identity.Key)) return;
            AddIdentity(binding, result.RouteId, identity.Key, user.userName,
                "validated_primordial_cube_field:vader_cube_66");
        }

        internal static void RecordBossAssist(MissionObjectiveRuntimeBinding binding,
            MissionCompositionProgressResult result, IEnumerable<string> assisterKeys)
        {
            if (!NetworkServer.active || binding == null || result == null ||
                !result.Accepted || result.CurrentValue <= result.PreviousValue ||
                result.ProgressScope != MissionProgressScope.PerPlayer ||
                assisterKeys == null || binding.BodyName == "ScoutBody") return;

            foreach (string assisterKey in assisterKeys)
                AddIdentity(binding, result.RouteId, assisterKey, assisterKey,
                    "boss_damage_before_valid_kill:" + result.ObjectiveId);
        }

        internal static List<NetworkUser> EligibleRecipients(MissionObjectiveRuntimeBinding binding,
            string completedRouteId, NetworkUser completingUser)
        {
            var result = new List<NetworkUser>();
            if (!NetworkServer.active || Run.instance == null || binding == null) return result;
            Dictionary<string, Contribution> contributors;
            ByRoute.TryGetValue(RouteKey(binding, completedRouteId), out contributors);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (CharacterMaster master in MissionConnectedPlayerEnumerator.GetMasters())
            {
                NetworkUser user = master == null ? null : MissionPlayerIdentity.GetNetworkUser(master);
                if (user?.gameObject == null) continue;
                MissionPlayerIdentity identity;
                if (!MissionPlayerIdentity.TryCreate(user, out identity) ||
                    string.IsNullOrWhiteSpace(identity.Key) || !seen.Add(identity.Key)) continue;
                Contribution evidence = null;
                bool eligible = contributors != null && contributors.TryGetValue(identity.Key, out evidence);
                // Fallback seguro: el evaluador ya confirmó que esta persona
                // completó el objetivo, aunque falle el almacenamiento auxiliar.
                if (ReferenceEquals(user, completingUser)) eligible = true;
                if (UsuLog.VerboseEnabled)
                    UsuLog.Verbose(logger, "[MISSION CREDIT AUDIT] Individual | Body: " + binding.BodyName +
                    " | Route: " + completedRouteId + " | User: " + user.userName +
                    " | Identity: " + identity.Key + " | Eligible: " + eligible +
                    " | Reason: " + (eligible ? (evidence != null ?
                        string.Join(",", evidence.Evidence) : "completer_verified") :
                        "no_validated_contribution_in_route"));
                if (eligible) result.Add(user);
            }
            // Si el índice de controllers se quedó desfasado en la transición,
            // preservar al completador ya autenticado por el evaluador host.
            if (completingUser?.gameObject != null)
            {
                MissionPlayerIdentity finisherIdentity;
                if (MissionPlayerIdentity.TryCreate(completingUser, out finisherIdentity) &&
                    !string.IsNullOrWhiteSpace(finisherIdentity.Key) &&
                    seen.Add(finisherIdentity.Key))
                {
                    result.Add(completingUser);
                    logger?.LogWarning("[MISSION CREDIT AUDIT] Recuperado completador " +
                        "fuera del enumerador de jugadores | Body: " + binding.BodyName +
                        " | User: " + completingUser.userName +
                        " | Identity: " + finisherIdentity.Key);
                }
            }
            return result;
        }
    }
}
