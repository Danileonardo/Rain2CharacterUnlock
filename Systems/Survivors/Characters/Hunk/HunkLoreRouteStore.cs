using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// La PRIMERA ruta que concedió HUNK queda fijada por perfil local.
    /// Nunca se guarda una segunda ruta ni se modifica el Achievement nativo.
    /// El host valida la ruta y cada cliente guarda únicamente SU perfil.
    /// </summary>
    internal static class HunkLoreRouteStore
    {
        internal const string Escape = "escape";
        internal const string Sacrifice = "sacrifice";
        private const string LoreToken = "ROB_HUNK_BODY_LORE";

        private sealed class SaveData
        {
            public int SchemaVersion = 1;
            public Dictionary<string, string> RoutesByProfile =
                new Dictionary<string, string>(StringComparer.Ordinal);
        }

        private static SaveData state = new SaveData();
        private static bool loaded;
        private static bool writable = true;
        private static bool hookInstalled;
        private static ManualLogSource logger;

        private static string FilePath => System.IO.Path.Combine(
            Paths.ConfigPath, "UniversalSurvivorUnlocks", "HunkLoreRoutes.json");

        internal static void RegisterNativeLoreHook(ManualLogSource log)
        {
            if (logger == null && log != null) logger = log;
            if (hookInstalled) return;
            hookInstalled = true;
            // El Logbook NATIVO lee el token, no Definition.ResolveLore().
            // Resolverlo al leer permite detectar cambios de perfil/idioma
            // sin sobrescribir los tokens globales de otros jugadores.
            On.RoR2.Language.GetLocalizedStringByToken += GetNativeHunkLore;
        }

        private static string GetNativeHunkLore(
            On.RoR2.Language.orig_GetLocalizedStringByToken orig,
            Language language, string token)
        {
            string original = orig(language, token);
            if (!string.Equals(token, LoreToken, StringComparison.Ordinal))
                return original;
            try
            {
                string culture = Language.currentLanguageName ?? "";
                bool spanish = culture.StartsWith("es", StringComparison.OrdinalIgnoreCase);
                bool english = culture.StartsWith("en", StringComparison.OrdinalIgnoreCase);
                if (!spanish && !english) return original;
                UserProfile profile = GetActiveLocalProfile();
                // Nunca mostrar anexos si aún no se identificó el perfil.
                return HunkLore.BuildForProfile(profile, spanish);
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[HUNK LORE] Lectura nativa: " + ex.Message);
                return original;
            }
        }

        internal static UserProfile GetActiveLocalProfile()
        {
            try
            {
                var locals = LocalUserManager.readOnlyLocalUsersList;
                if (locals == null || locals.Count == 0) return null;
                return locals[0]?.userProfile;
            }
            catch { return null; }
        }

        private static void LoadIfNeeded()
        {
            if (loaded) return;
            loaded = true;
            if (!File.Exists(FilePath)) return;
            try
            {
                SaveData data = JsonConvert.DeserializeObject<SaveData>(
                    File.ReadAllText(FilePath));
                if (data == null || data.SchemaVersion != 1 ||
                    data.RoutesByProfile == null)
                {
                    writable = false;
                    logger?.LogWarning("[HUNK LORE] Guardado incompatible; no se sobrescribirá.");
                    return;
                }
                foreach (string route in data.RoutesByProfile.Values)
                {
                    if (route != Escape && route != Sacrifice)
                    {
                        writable = false;
                        logger?.LogWarning("[HUNK LORE] Ruta desconocida: JSON protegido.");
                        return;
                    }
                }
                state = data;
            }
            catch (Exception ex)
            {
                writable = false;
                logger?.LogWarning("[HUNK LORE] Guardado ilegible, no se sobrescribirá: " + ex.Message);
            }
        }

        // UserProfile.fileName es la identidad local de la partida. Por
        // compatibilidad con diferentes versiones de RoR2, se lee por reflexión.
        // NO se usa el nombre visible del jugador como identificador único.
        private static string GetProfileKey(UserProfile profile)
        {
            if (profile == null) return null;
            Type t = profile.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (string name in new[] { "fileName", "FileName", "profileFileName" })
            {
                try
                {
                    object value = null;
                    PropertyInfo property = t.GetProperty(name, flags);
                    if (property != null && property.GetIndexParameters().Length == 0)
                        value = property.GetValue(profile, null);
                    else
                    {
                        FieldInfo field = t.GetField(name, flags);
                        if (field != null) value = field.GetValue(profile);
                    }
                    string text = value as string;
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                        return BitConverter.ToString(digest).Replace("-", "").ToLowerInvariant();
                    }
                }
                catch { /* Otra versión de la API */ }
            }
            return null; // No arriesgar mezclar perfiles sin identidad persistente.
        }

        internal static string GetRoute(UserProfile profile)
        {
            LoadIfNeeded();
            string key = GetProfileKey(profile);
            if (key == null) return null;
            string route;
            return state.RoutesByProfile.TryGetValue(key, out route) ? route : null;
        }

        internal static bool RecordFirstUnlock(UserProfile profile, string route)
        {
            if (route != Escape && route != Sacrifice) return false;
            LoadIfNeeded();
            if (!writable) return false;
            string key = GetProfileKey(profile);
            if (key == null)
            {
                logger?.LogWarning("[HUNK LORE] El perfil no expone fileName; no se atribuye una ruta insegura.");
                return false;
            }
            if (state.RoutesByProfile.ContainsKey(key)) return false;

            state.RoutesByProfile.Add(key, route);
            if (Persist())
            {
                logger?.LogInfo("[HUNK LORE] Primer desenlace guardado: " + route);
                return true;
            }
            // Si falló el disco, no aparentar que quedó fijado en memoria.
            state.RoutesByProfile.Remove(key);
            return false;
        }

        private static bool Persist()
        {
            string target = FilePath;
            string temp = target + ".tmp";
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target));
                File.WriteAllText(temp,
                    JsonConvert.SerializeObject(state, Formatting.Indented));
                if (File.Exists(target)) File.Replace(temp, target, target + ".bak");
                else File.Move(temp, target);
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[HUNK LORE] No se pudo guardar el primer desenlace: " + ex.Message);
                return false;
            }
        }
    }
}
