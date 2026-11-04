using System;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Seamstress / tsuyoikenko 1.7.4. Source: author Thunderstore page and native runtime token/lore audit.
    /// Registers localization only when the exact source+body pair is installed.
    /// Does not reference the external DLL, change providers or expose locked Logbook entries.
    /// </summary>
    public sealed class SeamstressDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.kenko.Seamstress"; } }
        public override string BodyName { get { return "SeamstressBody"; } }
        public override string DefinitionName { get { return "Seamstress"; } }
        private const string SpanishDescription = SeamstressLocalization.SpanishDescription;
        // El origen del creador permanece intacto como primera parte;
        // USU continúa la historia de Seamstress en Profundidades Abisales.
        private static readonly string SpanishLore = SeamstressLore.SpanishLore;
        private static readonly string EnglishLore = SeamstressLore.EnglishLore;

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterSpanishToken("KENKO_SEAMSTRESS_DESCRIPTION", SpanishDescription);
            // Source audit reports LoreToken empty; ResolveLore supplies the native Logbook translation.
            // Future creator revisions may use this conventional token.
            RegisterSpanishToken("KENKO_SEAMSTRESS_LORE", SpanishLore);
            LanguageAPI.Add("KENKO_SEAMSTRESS_LORE", EnglishLore, "en");
            RegisterSpanishToken("KENKO_SEAMSTRESS_NAME", "Seamstress");
            RegisterSpanishToken("KENKO_SEAMSTRESS_SUBTITLE", "La Doncella Andrajosa");
            RegisterSpanishToken("KENKO_SEAMSTRESS_PASSIVE_NAME", "Tiene hambre");
            RegisterSpanishToken("KENKO_SEAMSTRESS_NEEDLE_NAME", "Corazón marcado por los imps");
            RegisterSpanishToken("KENKO_SEAMSTRESS_PRIMARY_TRIM_NAME", "Recortar");
            RegisterSpanishToken("KENKO_SEAMSTRESS_PRIMARY_FLURRY_NAME", "Ráfaga de cortes");
            RegisterSpanishToken("KENKO_SEAMSTRESS_SECONDARY_CLIP_NAME", "Cercenar");
            RegisterSpanishToken("KENKO_SEAMSTRESS_SECONDARY_PLANMAN_NAME", "Manipulación planar");
            RegisterSpanishToken("KENKO_SEAMSTRESS_UTILITY_HEARTDASH_NAME", "Brutalizar");
            RegisterSpanishToken("KENKO_SEAMSTRESS_UTILITY_PARRY_NAME", "Contraatacar");
            RegisterSpanishToken("KENKO_SEAMSTRESS_SPECIAL_FIRE_NAME", "Ensartar");
            RegisterSpanishToken("KENKO_SEAMSTRESS_MASTERY_SKIN_NAME", "Alternativa");
            RegisterSpanishToken("KENKO_SEAMSTRESS_MASTERY_SKIN_NAME2", "Alternativa 2");
            RegisterSpanishToken("KENKO_SEAMSTRESS_MASTERY_SKIN_NAME3", "Raven");
            RegisterSpanishToken("KENKO_SEAMSTRESS_MASTERY_SKIN_NAME4", "Raven alternativa");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_SEAMSTRESS_MASTERYACHIEVEMENT_NAME", "Seamstress: Maestría");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_SEAMSTRESS_TYPHOONMASTERYACHIEVEMENT_NAME", "Raven: Desbloqueada");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_SEAMSTRESS_MASTERYACHIEVEMENT_DESCRIPTION", "Como Seamstress, completa el juego o sacrifícate en el Obelisco en Monzón.");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_SEAMSTRESS_TYPHOONMASTERYACHIEVEMENT_DESCRIPTION", "Como Seamstress, completa el juego o sacrifícate en el Obelisco en Eclipse o una dificultad superior.");
            RegisterSpanishToken("KENKO_SEAMSTRESS_PASSIVE_DESCRIPTION", FormatLongSkillText("Obtén <style=cIsDamage>daño base</style> y <style=cIsHealing>curación</style> en función de la <style=cIsHealth>salud que te falte</style>."));
            RegisterSpanishToken("KENKO_SEAMSTRESS_NEEDLE_DESCRIPTION", FormatLongSkillText("Consume <color=#9B3737>agujas</color> al saltar en el aire para ejecutar un <color=#9B3737>parpadeo</color> y disparar un proyectil que inflige <style=cIsDamage>40% de daño</style>."));
            RegisterSpanishToken("KENKO_SEAMSTRESS_PRIMARY_TRIM_DESCRIPTION", FormatLongSkillText("<color=#9B3737>Despiadado</color>. Corta frente a ti e inflige <style=cIsDamage>120% + 200% de daño</style>. Cada tercer golpe inflige <style=cIsDamage>150% + 200% de daño</style>."));
            RegisterSpanishToken("KENKO_SEAMSTRESS_PRIMARY_FLURRY_DESCRIPTION", FormatLongSkillText("<color=#9B3737>Despiadado</color>. Corta frente a ti e inflige <style=cIsDamage>140% + 200% de daño</style>."));
            RegisterSpanishToken("KENKO_SEAMSTRESS_SECONDARY_CLIP_DESCRIPTION", FormatLongSkillText("Corta e inflige <style=cIsDamage>2 × 70% de daño</style>. Realiza un corte adicional por cada <color=#9B3737>aguja</color>."));
            RegisterSpanishToken("KENKO_SEAMSTRESS_SECONDARY_PLANMAN_DESCRIPTION", FormatLongSkillText("<color=#9B3737>Aplastamiento</color>. Mantén pulsado para sujetar enemigos. Estrellar al enemigo inflige entre <style=cIsDamage>200% y 1200% de daño</style> en una zona según la cantidad de <color=#9B3737>agujas</color>. Tiene <style=cIsUtility>6 s de enfriamiento</style> contra el mismo objetivo."));
            RegisterSpanishToken("KENKO_SEAMSTRESS_UTILITY_HEARTDASH_DESCRIPTION", FormatLongSkillText("Desplázate rápidamente e inflige <style=cIsDamage>250% de daño</style>; entra en estado <color=#9B3737>Insaciable</color>, que consume <style=cIsHealth>5% de salud y escudo</style> cada 0,2 s. Vuelve a usar la habilidad para convertir la <style=cIsHealing>barrera</style> en <style=cIsHealth>salud</style> y provocar una explosión de <style=cIsDamage>300% a 1200% de daño</style>, que aumenta según la barrera convertida."));
            RegisterSpanishToken("KENKO_SEAMSTRESS_UTILITY_PARRY_DESCRIPTION", FormatLongSkillText("Prepárate para <style=cIsUtility>desviar</style> un ataque. Al recibir un golpe, entra inmediatamente en estado <color=#9B3737>Insaciable</color> y atraviesa a tus enemigos. Tras un breve retraso, inflige <style=cIsDamage>400% de daño</style> a los enemigos alcanzados y atrae hacia ti a los que no sean jefes."));
            RegisterSpanishToken("KENKO_SEAMSTRESS_SPECIAL_FIRE_DESCRIPTION", FormatLongSkillText("<style=cIsHealth>15% de salud</style>. Lanza unas tijeras <color=#9B3737>simbióticas</color> que infligen <style=cIsDamage>400% de daño</style>. Lanzarlas en el aire te impulsa <style=cIsUtility>en dirección contraria al lanzamiento</style>. Recoge las tijeras <color=#9B3737>simbióticas</color> para atacar con ellas e infligir <style=cIsDamage>200% de daño</style>."));
            // Variante opcional registrada únicamente si Ancient Scepter está instalado.
            RegisterSpanishToken("KENKO_SEAMSTRESS_SPECIAL_SCEPTER_NAME", "Desenfreno");
            RegisterSpanishToken("KENKO_SEAMSTRESS_SPECIAL_SCEPTER_DESCRIPTION",
                "Tu arma <color=#9B3737>simbiótica</color> tiene un tiempo de recogida de solo <style=cIsUtility>1,5 segundos</style>.");
            VerifiedExtraTooltipLocalization.Register(survivor, logger);
            UsuLog.Verbose(logger, "[LOCALIZATION] Definition activa | SeamstressBody | ES 7 habilidades + 2 pasivas + 4 skins + 2 logros de skins + Diario I ES/EN (origen autor + continuación USU)");
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "La Doncella Andrajosa" : fallback ?? "";
        }
        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishDescription : fallback ?? "";
        }
        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            // Diario I curado en español e inglés; otros idiomas preservan el fallback.
            if (IsSpanish()) return SpanishLore;
            if ((Language.currentLanguageName ?? "").StartsWith("en", StringComparison.OrdinalIgnoreCase))
                return EnglishLore;
            return fallback ?? EnglishLore;
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static void RegisterSpanishToken(string token, string translation)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(translation)) return;
            LanguageAPI.Add(token, translation, "es-419");
            LanguageAPI.Add(token, translation, "es-ES");
        }
    }
}
