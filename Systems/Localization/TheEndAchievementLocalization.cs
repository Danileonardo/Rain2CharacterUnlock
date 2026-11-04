using System;
using System.Collections.Generic;
using BepInEx.Logging;
using R2API;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Traduce una sola vez el logro/Unlockable compartido de TheEnd y las
    /// skins del Dragón del End. No concede logros ni modifica requisitos.
    /// No necesita referenciar TheEnd.dll.
    /// </summary>
    internal static class TheEndAchievementLocalization
    {
        private static readonly List<object> Overlays = new List<object>();
        private static readonly HashSet<string> Registered =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<UnlockableDef> Wrapped = new HashSet<UnlockableDef>();
        private const string OriginalName = "Free the End";
        private const string SpanishName = "Liberar el End";
        private const string OriginalDescription =
            "Defeat the Ender Dragon and leave The End through its exit portal.";
        private const string SpanishDescription =
            "Derrota al Dragón del End y abandona el End a través de su portal de salida.";

        internal static void Register(ManualLogSource logger)
        {
            try
            {
                // Los títulos/descripciones pueden ser tokens literales.
                // Si el autor usa esos textos como identificadores, quedan cubiertos.
                AddSpanish(OriginalName, SpanishName);
                AddSpanish(OriginalDescription, SpanishDescription);

                var verifiedUnlocks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int achievements = 0;
                IEnumerable<AchievementDef> allAchievements = AchievementManager.allAchievementDefs;
                if (allAchievements != null)
                foreach (AchievementDef achievement in allAchievements)
                {
                    if (achievement == null) continue;
                    string name = Source(achievement.nameToken);
                    string desc = Source(achievement.descriptionToken);
                    if (!IsName(name) || !IsEndDescription(desc)) continue;

                    AddSpanish(achievement.nameToken, SpanishName);
                    AddSpanish(achievement.descriptionToken, SpanishDescription);
                    if (!string.IsNullOrWhiteSpace(achievement.unlockableRewardIdentifier))
                        verifiedUnlocks.Add(achievement.unlockableRewardIdentifier);
                    achievements++;
                }

                // La misma skin de TheEnd existe en numerosos BodyPrefabs,
                // pero todos comparten un mismo requisito de desbloqueo.
                int skins = 0, unlocks = 0;
                IEnumerable<GameObject> allBodies = BodyCatalog.allBodyPrefabs;
                if (allBodies != null)
                foreach (GameObject body in allBodies)
                {
                    if (body == null) continue;
                    foreach (ModelSkinController controller in
                        body.GetComponentsInChildren<ModelSkinController>(true))
                    {
                        if (controller == null || controller.skins == null) continue;
                        foreach (SkinDef skin in controller.skins)
                        {
                            if (skin == null) continue;
                            UnlockableDef unlock = skin.unlockableDef;
                            string skinName = Source(skin.nameToken);
                            string unlockName = unlock == null ? "" : Source(unlock.nameToken);
                            string unlockId = unlock == null ? "" : (unlock.cachedName ?? "");
                            string hint = GetHow(unlock);

                            // Nunca tocar otras mastery skins. Validación por
                            // nombre, logro asociado o texto específico de TheEnd.
                            bool isEndSkin =
                                IsEndSkinName(skinName) ||
                                (!string.IsNullOrEmpty(unlockId) && verifiedUnlocks.Contains(unlockId)) ||
                                (IsName(unlockName) && (IsEndDescription(hint) ||
                                  unlockId.IndexOf("end", StringComparison.OrdinalIgnoreCase) >= 0)) ||
                                (ContainsName(hint) && IsEndDescription(hint));
                            if (!isEndSkin) continue;

                            skins++;
                            if (IsEndSkinName(skinName)) AddSpanish(skin.nameToken, "Dragón del End");
                            if (unlock == null || !Wrapped.Add(unlock)) continue;
                            unlocks++;
                            if (IsName(unlockName)) AddSpanish(unlock.nameToken, SpanishName);
                            WrapHowToUnlock(unlock);
                        }
                    }
                }
                UsuLog.Verbose(logger, "[THEEND ES] Logros: " + achievements +
                    ", skins: " + skins + ", unlockables: " + unlocks +
                    ". Requisito conservado.");
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[THEEND ES] Error localizando TheEnd: " +
                    ex.GetType().Name + " - " + ex.Message);
            }
        }

        private static void WrapHowToUnlock(UnlockableDef unlock)
        {
            // RoR2 puede mostrar texto devuelto por un delegado de UnlockableDef
            // en vez de usar directamente AchievementDef.descriptionToken.
            // Solo cambiamos el TEXTO del tooltip, nunca el logro ni la condición.
            Func<string> original = unlock.getHowToUnlockString;
            if (original != null)
            {
                unlock.getHowToUnlockString = delegate
                {
                    string result = original();
                    return IsSpanish() ? TranslateHowTo(result) : result;
                };
            }
            Func<string> unlocked = unlock.getUnlockedString;
            if (unlocked != null)
            {
                unlock.getUnlockedString = delegate
                {
                    string result = unlocked();
                    return IsSpanish() ? TranslateHowTo(result) : result;
                };
            }
        }

        private static string TranslateHowTo(string english)
        {
            if (string.IsNullOrEmpty(english)) return english;
            return english.Replace(OriginalName, SpanishName)
                .Replace(OriginalDescription, SpanishDescription);
        }

        private static string GetHow(UnlockableDef unlock)
        {
            try { return unlock == null || unlock.getHowToUnlockString == null
                ? "" : (unlock.getHowToUnlockString() ?? ""); }
            catch { return ""; }
        }

        private static bool ContainsName(string text) =>
            !string.IsNullOrEmpty(text) &&
            text.IndexOf(OriginalName, StringComparison.OrdinalIgnoreCase) >= 0;
        private static bool IsName(string text) =>
            string.Equals((text ?? "").Trim(), OriginalName, StringComparison.OrdinalIgnoreCase);
        private static bool IsEndDescription(string text) =>
            !string.IsNullOrEmpty(text) &&
            text.IndexOf("Ender Dragon", StringComparison.OrdinalIgnoreCase) >= 0 &&
            text.IndexOf("portal", StringComparison.OrdinalIgnoreCase) >= 0;
        private static bool IsEndSkinName(string text) =>
            string.Equals((text ?? "").Trim(), "Ender Dragon", StringComparison.OrdinalIgnoreCase) ||
            string.Equals((text ?? "").Trim(), "Ender Dragon Skin", StringComparison.OrdinalIgnoreCase);
        private static bool IsSpanish() =>
            (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        private static string Source(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return "";
            string s = Language.GetString(token);
            return string.IsNullOrWhiteSpace(s) || s == "???" ? token : s;
        }
        private static void AddSpanish(string token, string value)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return;
            foreach (string locale in new[] { "es-419", "es-ES" })
            {
                if (Registered.Add(locale + ":" + token))
                    Overlays.Add(LanguageAPI.AddOverlay(token, value, locale));
            }
        }
    }
}
