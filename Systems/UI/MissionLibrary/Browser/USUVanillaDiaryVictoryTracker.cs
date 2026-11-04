using System;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Diario II: victoria individual del perfil con el survivor vanilla.
    /// Diario III: victoria del perfil con Captain (todo el elenco vanilla).
    /// Ambos: MainEnding moon2 u ObliterationEnding en el Obelisco.
    /// El host valida el resultado; cada cliente guarda solamente su perfil local.
    /// </summary>
    internal static class USUVanillaDiaryVictoryTracker
    {
        private static bool installed;
        private static ManualLogSource logger;
        private static readonly HashSet<string> Vanilla = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CommandoBody", "HuntressBody", "Bandit2Body", "ToolbotBody", "EngiBody",
            "MageBody", "MercBody", "TreebotBody", "LoaderBody", "CrocoBody", "CaptainBody"
        };

        internal static void Initialize(ManualLogSource log)
        {
            if (installed) return;
            installed = true;
            logger = log;
            USUVanillaNativeLogbookBridge.Initialize();
            Run.onServerGameOver += OnServerGameOver;
        }

        internal static string NormalizeBody(string body) =>
            string.Equals(body, "BanditBody", StringComparison.OrdinalIgnoreCase) ? "Bandit2Body" : body;

        internal static bool IsValidEnding(GameEndingDef ending)
        {
            if (ending == null) return false;
            if (MoonEscapeEndingMatcher.IsMoonEscape(ending)) return true;
            string name = ending.cachedName ?? "";
            return name.IndexOf("Obliter", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void OnServerGameOver(Run run, GameEndingDef ending)
        {
            if (!NetworkServer.active || run == null || !IsValidEnding(ending)) return;
            var seen = new HashSet<NetworkUser>();
            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                CharacterMaster master = controller?.master;
                if (master == null) continue;
                NetworkUser user = MissionPlayerIdentity.GetNetworkUser(master);
                if (user == null || !seen.Add(user)) continue;
                CharacterBody currentBody = master.GetBody();
                // bodyPrefab permanece disponible cuando el body murió en esta run.
                string body = currentBody != null ? currentBody.baseNameToken : null;
                string prefabName = master.bodyPrefab != null ? master.bodyPrefab.name : "";
                string bodyName = NormalizeBody(prefabName.Replace("(Clone)", ""));
                if (string.IsNullOrWhiteSpace(bodyName)) continue;
                new USUVanillaDiaryVictoryMessage(user.gameObject, bodyName).Send(NetworkDestination.Clients);
                // El host procesa su propio perfil sin depender del loopback de red.
                ApplyToLocalUser(user.gameObject, bodyName);
            }
        }

        internal static void ApplyToLocalUser(GameObject recipient, string bodyName)
        {
            bodyName = NormalizeBody(bodyName);
            if (recipient == null || string.IsNullOrWhiteSpace(bodyName)) return;
            foreach (LocalUser local in LocalUserManager.readOnlyLocalUsersList)
            {
                if (local == null || local.userProfile == null ||
                    local.currentNetworkUser == null || local.currentNetworkUser.gameObject != recipient) continue;
                string profileId = local.userProfile.name;
                if (string.IsNullOrWhiteSpace(profileId)) return;
                if (Vanilla.Contains(bodyName))
                {
                    MarkDiaryII(profileId, bodyName);
                    if (bodyName == "CaptainBody") VanillaBroadcastUSUDiary.RecordCaptainVictory(profileId);
                }
                else
                {
                    MarkModDiaryI(profileId, bodyName);
                }
            }
        }

        internal static bool IsDiaryIIUnlockedForLocalProfile(string body)
        {
            var local = LocalUserManager.GetFirstLocalUser();
            string profileId = local?.userProfile?.name;
            if (string.IsNullOrWhiteSpace(profileId)) return false;
            return File.Exists(DiaryIIPath(profileId, NormalizeBody(body)));
        }

        internal static bool IsModDiaryIUnlockedForLocalProfile(string body)
        {
            var local = LocalUserManager.GetFirstLocalUser();
            string profileId = local?.userProfile?.name;
            return !string.IsNullOrWhiteSpace(profileId) &&
                   !string.IsNullOrWhiteSpace(body) &&
                   File.Exists(ModDiaryIPath(profileId, NormalizeBody(body)));
        }

        private static void MarkModDiaryI(string profileId, string body)
        {
            try
            {
                string path = ModDiaryIPath(profileId, body);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (!File.Exists(path)) File.WriteAllText(path, "mod-diary-i-v1", Encoding.UTF8);
            }
            catch (Exception ex) { logger?.LogWarning("[MOD DIARIO I] No se pudo guardar: " + ex.Message); }
        }

        private static string ModDiaryIPath(string profileId, string body)
        {
            string safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(profileId))
                .Replace('/', '_').Replace('+', '-').TrimEnd('=');
            string safeBody = body.Replace('/', '_').Replace('\\', '_').Replace(':', '_');
            return Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "ModDiaryI", safe, safeBody + ".flag");
        }

        private static void MarkDiaryII(string profileId, string body)
        {
            try
            {
                string path = DiaryIIPath(profileId, body);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (!File.Exists(path)) File.WriteAllText(path, "diary-ii-v1", Encoding.UTF8);
            }
            catch (Exception ex) { logger?.LogWarning("[DIARIO II] No se pudo guardar: " + ex.Message); }
        }

        private static string DiaryIIPath(string profileId, string body)
        {
            string safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(profileId))
                .Replace('/', '_').Replace('+', '-').TrimEnd('=');
            return Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "VanillaDiaryII", safe, body + ".flag");
        }
    }

    public sealed class USUVanillaDiaryVictoryMessage : INetMessage
    {
        public GameObject Recipient;
        public string BodyName;
        public USUVanillaDiaryVictoryMessage() { }
        public USUVanillaDiaryVictoryMessage(GameObject recipient, string bodyName)
        { Recipient = recipient; BodyName = bodyName; }
        public void Serialize(NetworkWriter writer)
        { writer.Write(Recipient); writer.Write(BodyName ?? ""); }
        public void Deserialize(NetworkReader reader)
        { Recipient = reader.ReadGameObject(); BodyName = reader.ReadString(); }
        public void OnReceived()
        { USUVanillaDiaryVictoryTracker.ApplyToLocalUser(Recipient, BodyName); }
    }
}
