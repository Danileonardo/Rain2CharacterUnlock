using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Foto de jugadores conectados desde la autoridad del host, usada en
    /// eventos discretos (teleporter, recompensa). Evita depender solo de
    /// PlayerCharacterMasterController.instances en una transición de stage.
    /// Un master sin NetworkUser enlazado NO se considera un jugador válido.
    /// </summary>
    internal static class MissionConnectedPlayerEnumerator
    {
        private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        internal static List<CharacterMaster> GetMasters()
        {
            var masters = new List<CharacterMaster>();
            var seen = new HashSet<CharacterMaster>();
            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
                Add(controller?.master, seen, masters);

            // Algunas sesiones tienen NetworkUser activo mientras se termina de
            // poblar el índice global de PlayerCharacterMasterController.
            foreach (NetworkUser user in GetNetworkUsers())
                Add(GetLinkedMaster(user), seen, masters);
            return masters;
        }

        internal static NetworkUser FindNetworkUser(CharacterMaster master)
        {
            if (master == null) return null;
            foreach (NetworkUser user in GetNetworkUsers())
            {
                if (ReferenceEquals(GetLinkedMaster(user), master)) return user;
            }
            return null;
        }

        private static CharacterMaster GetLinkedMaster(NetworkUser user)
        {
            if (user == null) return null;
            object value = GetMember(typeof(NetworkUser), user, "master") ??
                           GetMember(typeof(NetworkUser), user, "masterObject");
            if (value is CharacterMaster master) return master;
            if (value is GameObject go) return go.GetComponent<CharacterMaster>();
            return null;
        }

        private static IEnumerable<NetworkUser> GetNetworkUsers()
        {
            object users = GetMember(typeof(NetworkUser), null, "readOnlyInstancesList") ??
                           GetMember(typeof(NetworkUser), null, "instancesList");
            if (!(users is IEnumerable collection)) yield break;
            foreach (object user in collection)
            {
                if (user is NetworkUser nu) yield return nu;
            }
        }

        private static void Add(CharacterMaster master, HashSet<CharacterMaster> seen, List<CharacterMaster> result)
        {
            if (master != null && MissionPlayerIdentity.GetNetworkUser(master) != null && seen.Add(master))
                result.Add(master);
        }

        private static object GetMember(Type type, object instance, string name)
        {
            try
            {
                FieldInfo f = type.GetField(name, All);
                if (f != null) return f.GetValue(instance);
                PropertyInfo prop = type.GetProperty(name, All);
                return prop?.GetValue(instance, null);
            }
            catch (Exception) { return null; }
        }
    }
}
