using System;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Warlock - com.kenko.Warlock / WarlockBody.
    /// Localización curada exclusivamente de tokens verificados en la
    /// auditoría de la instalación del usuario (11/10/2026).
    /// Respeta la misión Original, su logro y el lore acreditado a Plnk.
    /// </summary>
    public sealed class WarlockDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.kenko.Warlock"; } }
        public override string BodyName { get { return "WarlockBody"; } }
        public override string DefinitionName { get { return "Warlock"; } }

        private static readonly string SpanishOverview = WarlockLocalization.SpanishOverview;
        // Traducción completa de las Notas ORIGINALES de Plnk, no un lore nuevo.
        private static readonly string SpanishLore = WarlockLore.SpanishLore;

        // Continuación USU: el relato original del proveedor jamás se edita.
        // ES: el prefijo es SpanishLore; EN: prefijo obtenido del token del creador.
        private const string UsuLoreStartSpanish = WarlockLore.UsuLoreStartSpanish;
        private const string UsuLoreStartEnglish = WarlockLore.UsuLoreStartEnglish;
        private static readonly string SpanishContinuation = WarlockLore.SpanishContinuation;
        private static readonly string EnglishContinuation = WarlockLore.EnglishContinuation;
        private static bool loreHookRegistered;

        private static void EnsureLoreHook()
        {
            if (loreHookRegistered) return;
            loreHookRegistered = true;
            On.RoR2.Language.GetLocalizedStringByToken += AppendNativeWarlockLore;
        }

        // The native Logbook reads a token rather than ResolveLore().
        // Use the original localization first (including EN by Plnk), then
        // append USU once, without replacing the creator's English text.
        private static string AppendNativeWarlockLore(
            On.RoR2.Language.orig_GetLocalizedStringByToken orig,
            Language language, string token)
        {
            string original = orig(language, token);
            // El mismo hook existente también localiza las descripciones que
            // Warlock genera dinámicamente (estadísticas configurables).
            // No altera el inglés ni los datos entregados por el mod.
            if (IsSpanish() && WarlockSkillLocalization.TryLocalizeRuntime(token, original, out string localized))
                return localized;
            if (!string.Equals(token, "KENKO_WARLOCK_LORE", StringComparison.Ordinal))
                return original;
            return AppendLoreIfMissing(original, IsSpanish());
        }

        private static string AppendLoreIfMissing(string original, bool spanish)
        {
            if (string.IsNullOrWhiteSpace(original) ||
                string.Equals(original.Trim(), "KENKO_WARLOCK_LORE", StringComparison.OrdinalIgnoreCase))
                return original ?? "";
            string marker = spanish ? UsuLoreStartSpanish : UsuLoreStartEnglish;
            if (original.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                return original;
            return original + "\n\n<style=cStack>" + marker + "</style>\n\n" +
                (spanish ? SpanishContinuation : EnglishContinuation);
        }

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterSpanishToken("KENKO_WARLOCK_NAME", "Warlock");
            RegisterSpanishToken("KENKO_WARLOCK_SUBTITLE", "Peón maldito");
            RegisterSpanishToken("KENKO_WARLOCK_PRIMARY_SURGE_NAME", "Oleada sobrenatural");
            RegisterSpanishToken("KENKO_WARLOCK_SECONDARY_HEX_NAME", "Maleficio");
            RegisterSpanishToken("KENKO_WARLOCK_UTILITY_BLOOD_DASH_NAME", "Desplazamiento sangriento");
            RegisterSpanishToken("KENKO_WARLOCK_SPECIAL_RITUAL_NAME", "Ritual");
            RegisterSpanishToken("KENKO_WARLOCK_PASSIVE_NAME", "Maná carmesí");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_WARLOCK_UNLOCKACHIEVEMENT_NAME", "Su eminencia");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_WARLOCK_UNLOCKACHIEVEMENT_DESCRIPTION", "Derrota a un Señor de los diablillos dentro de los 5 segundos posteriores a su primer golpe dañino.");
            RegisterSpanishSkill("KENKO_WARLOCK_PRIMARY_SURGE_DESCRIPTION", "Dispara un rayo <style=cIsUtility>perforante</style> que inflige <style=cIsDamage>400% de daño</style>. <color=#981e33>\nMANÁ CARMESÍ: la siguiente activación debilita y repele a los enemigos.</color> <color=#b97f8a>\nMETAMAGIA: mantén pulsado para consumir acumulaciones y disparar 1 rayo adicional por cada una.</color>");
            RegisterSpanishSkill("KENKO_WARLOCK_SECONDARY_HEX_DESCRIPTION", "Maldice a un enemigo durante <style=cIsUtility>7 segundos</style>. Las habilidades le infligen <style=cIsDamage>50% de daño adicional</style> por acumulación. <color=#981e33>\nMANÁ CARMESÍ: el daño adicional se vuelve un ataque en área que inflige el 50% del daño del impacto por acumulación.</color><color=#b97f8a>\nMETAMAGIA: mantén pulsado para consumir acumulaciones: +1 maldición y 2 s de hemorragia al impactar por cada una.</color>");
            RegisterSpanishSkill("KENKO_WARLOCK_UTILITY_BLOOD_DASH_DESCRIPTION", "<style=cIsUtility>Desaparece</style> y <style=cIsUtility>teletranspórtate</style> una distancia corta.<color=#981e33>\nMANÁ CARMESÍ: la siguiente activación inflige 200% de daño con aturdimiento en ambos extremos.</color><color=#b97f8a>\nMETAMAGIA: mantén pulsado para consumir acumulaciones: mayor distancia y 200% de daño al llegar, +100% por cada acumulación.</color>");
            RegisterSpanishSkill("KENKO_WARLOCK_SPECIAL_RITUAL_DESCRIPTION", "Abre un menú para recuperar las cargas de una habilidad elegida y potenciarla. Al volver a activarlo, mantén pulsado <color=#981e33>Ritual</color> para obtener <color=#b97f8a>1 acumulación de Metamagia</color> a cambio de <color=#981e33>1 de Maná carmesí</color>.");
            RegisterSpanishSkill("KENKO_WARLOCK_PASSIVE_DESCRIPTION", "<color=#981e33>Warlock</color> puede potenciar o recuperar determinadas habilidades usando <color=#981e33>Maná carmesí</color>. Obtienes una acumulación de <color=#981e33>Maná carmesí</color> cada <style=cIsDamage>2</style> bajas.");
            RegisterSpanishToken("KENKO_WARLOCK_DESCRIPTION", SpanishOverview);
            // El Diario nativo obtiene el relato desde KENKO_WARLOCK_LORE.
            // ResolveLore() solo cubre las Notas mostradas dentro de USU.
            RegisterSpanishToken("KENKO_WARLOCK_LORE", SpanishLore);
            // Dynamic suffix for native Logbook in EN/ES; no English
            // replacement registration because the creator owns that text.
            WarlockSkillLocalization.Register();
            EnsureLoreHook();
            VerifiedExtraTooltipLocalization.Register(survivor, logger);
            UsuLog.Verbose(logger, "[LOCALIZATION] Definition activa | WarlockBody | " +
                "ES Vista general + pasiva + habilidades potenciadas + Ritual/Scepter + skins + Original + Notas; EN preservado");
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "Peón maldito" : fallback ?? "";
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishOverview : fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            // La visibilidad de las Notas la controla el Logbook nativo.
            // Notes inside USU also get the continuation. If the native
            // hook already appended it, the marker prevents duplicates.
            if (IsSpanish()) return AppendLoreIfMissing(SpanishLore, true);
            return AppendLoreIfMissing(fallback, false);
        }

        public override string ResolveOriginalMissionName(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "Su eminencia" : fallback ?? "";
        }

        public override string ResolveOriginalMissionDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish()
                ? "Derrota a un Señor de los diablillos dentro de los 5 segundos posteriores a su primer golpe dañino."
                : fallback ?? "";
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "")
                .StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static void RegisterSpanishSkill(string token, string value)
        {
            RegisterSpanishToken(token, FormatLongSkillText(value));
        }

        private static void RegisterSpanishToken(string token, string value)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return;
            LanguageAPI.Add(token, value, "es-419");
            LanguageAPI.Add(token, value, "es-ES");
        }
    }
}
