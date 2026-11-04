using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Curated translations for the installed com.kenko.Banshee (tsuyoikenko) survivor.
    /// Based on native Banshee.txt source tokens from the user runtime log.
    /// No provider changes or assumed skin tokens; ES lore adapted by USU.
    /// </summary>
    public sealed class BansheeDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.kenko.Banshee"; } }
        public override string BodyName { get { return "BansheeBody"; } }
        public override string DefinitionName { get { return "Banshee"; } }
        private const string SpanishDescription = BansheeLocalization.SpanishDescription;
        // USU Diario I autorizado: adaptación del lore del creador (Casa del Más Allá,
        // jerarquía estricta, paraguas FLT, fuegos fatuos y experimentos gravitatorios).
        // Historial de origen conservado en DIARIOS/LORE_ORIGINAL_AUTOR_EN.txt.
        // Narrativa ES/EN publicada en el token nativo del Logbook, sin cambiar misiones.
        private static readonly string SpanishLore = LoreDialogueLineFormatter.Format(BansheeLore.SpanishLore);
        private static readonly string EnglishLore = LoreDialogueLineFormatter.Format(BansheeLore.EnglishLore);
        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterSpanishToken("KENKO_BANSHEE_DESCRIPTION", SpanishDescription);
            RegisterSpanishToken("KENKO_BANSHEE_LORE", SpanishLore);
            RegisterEnglishToken("KENKO_BANSHEE_LORE", EnglishLore);
            RegisterSpanishToken("KENKO_BANSHEE_NAME", "Banshee");
            RegisterSpanishToken("KENKO_BANSHEE_SUBTITLE", "Heraldo de la Casa del Más Allá");
            RegisterSpanishToken("KENKO_BANSHEE_OUTRO_FLAVOR", "...y así partió, con sus susurros resonando por toda la eternidad.");
            RegisterSpanishToken("KENKO_BANSHEE_OUTRO_FAILURE", "...y así desapareció, dejando resonar un silencio ensordecedor.");
            RegisterSpanishToken("KENKO_BANSHEE_PASSIVE_NAME", "Paraguas FLT");
            RegisterSpanishToken("KENKO_BANSHEE_PRIMARY_ATKUMBRELLA_NAME", "Paraguas ATK");
            RegisterSpanishToken("KENKO_BANSHEE_PRIMARY_THRSTUMBRELLA_NAME", "Paraguas THRST");
            RegisterSpanishToken("KENKO_BANSHEE_PRIMARY_WISP_NAME", "Fuego fatuo etéreo");
            RegisterSpanishToken("KENKO_BANSHEE_SECONDARY_DIVE_NAME", "Ataque en picada");
            RegisterSpanishToken("KENKO_BANSHEE_UTILITY_THRUST_NAME", "Propulsores HDN");
            RegisterSpanishToken("KENKO_BANSHEE_UTILITY_SUPERTHRUST_NAME", "Propulsor EXP");
            RegisterSpanishToken("KENKO_BANSHEE_SPECIAL_CALAMITAS_NAME", "Granada CAL");
            RegisterSpanishToken("KENKO_BANSHEE_SPECIAL_SCEP_CALAMITAS_NAME", "Calamitas");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_BANSHEE_UNLOCK_ACHIEVEMENT_NAME", "¡Soy Mary Poppins, maldita sea!");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_BANSHEE_UNLOCK_ACHIEVEMENT_DESCRIPTION", "Completa una etapa sin tocar el suelo más de una vez.");
            RegisterSpanishToken("KENKO_BANSHEE_PASSIVE_DESCRIPTION", FormatLongSkillText("Mantén pulsada la tecla de salto para flotar en el aire. Mientras flotas, tu <style=cIsUtility>habilidad principal</style> se convierte en un ataque dirigido que aplica <color=#ba8fff>Presagio</color>."));
            RegisterSpanishToken("KENKO_BANSHEE_PRIMARY_ATKUMBRELLA_DESCRIPTION", FormatLongSkillText("<color=#ba8fff>Presagio</color>. Corta frente a ti e inflige <style=cIsDamage>250% de daño</style>. Cada tercer corte te impulsa hacia delante y al aire, e inflige <style=cIsDamage>300% de daño</style>."));
            RegisterSpanishToken("KENKO_BANSHEE_PRIMARY_THRSTUMBRELLA_DESCRIPTION", FormatLongSkillText("<color=#ba8fff>Presagio</color>. Corta frente a ti e inflige <style=cIsDamage>250% de daño</style>. Cada tercer corte te impulsa al aire e inflige <style=cIsDamage>300% de daño</style>."));
            RegisterSpanishToken("KENKO_BANSHEE_PRIMARY_WISP_DESCRIPTION", FormatLongSkillText("Dispara un fuego fatuo que inflige <style=cIsDamage>200% de daño</style>."));
            RegisterSpanishToken("KENKO_BANSHEE_SECONDARY_DIVE_DESCRIPTION", FormatLongSkillText("<style=cIsUtility>Aerodinámico</style>. Lánzate en picada e inflige <style=cIsDamage>250% de daño</style> al impactar. <style=cIsHealth>Consume</style> todas las cargas de esta <style=cIsUtility>habilidad</style> y aumenta el <style=cIsDamage>daño infligido</style> por cada carga consumida."));
            RegisterSpanishToken("KENKO_BANSHEE_UTILITY_THRUST_DESCRIPTION", FormatLongSkillText("<color=#ba8fff>Presagio</color>. Desplázate rápidamente en una dirección."));
            RegisterSpanishToken("KENKO_BANSHEE_UTILITY_SUPERTHRUST_DESCRIPTION", FormatLongSkillText("<color=#ba8fff>Presagio</color>. Desplázate rápidamente en una dirección. <color=#ba8fff>Presagio</color> se activa <style=cIsUtility>4</style> veces adicionales."));
            RegisterSpanishToken("KENKO_BANSHEE_SPECIAL_CALAMITAS_DESCRIPTION", FormatLongSkillText("Lanza una granada que crea un agujero negro inestable. Después de <style=cIsUtility>2</style> segundos, el agujero negro colapsa e inflige <style=cIsDamage>700% de daño</style>."));
            RegisterSpanishToken("KENKO_BANSHEE_SPECIAL_SCEP_CALAMITAS_DESCRIPTION", FormatLongSkillText("Lanza 3 granadas que crean agujeros negros inestables. Después de <style=cIsUtility>5</style> segundos, los agujeros negros colapsan e infligen <style=cIsDamage>700% de daño</style>."));
            RegisterBansheeKeywordLocalization(logger);
            UsuLog.Verbose(logger, "[LOCALIZATION] Definition activa | BansheeBody | ES 1 pasiva + 8 habilidades + 1 logro original + 2 keywords + diario USU ES/EN; skins específicas no verificadas");
        }
        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "Heraldo de la Casa del Más Allá" : fallback ?? "";
        }
        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishDescription : fallback ?? "";
        }
        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishLore : EnglishLore;
        }

        // El mod publica keywords como texto literal en SkillDef.keywordTokens.
        // Capturamos los tokens que el propio mod cargado asignó a sus habilidades,
        // sin adivinar nombres ni afectar otras traducciones/personajes.
        private const string OmenSpanishKeyword = BansheeLocalization.OmenSpanishKeyword;

        private const string AerodynamicSpanishKeyword = BansheeLocalization.AerodynamicSpanishKeyword;

        private static void RegisterBansheeKeywordLocalization(ManualLogSource logger)
        {
            try
            {
                // Compatibilidad con versiones que publican el catálogo como campo o propiedad.
                Type catalogType = typeof(RoR2.Skills.SkillCatalog);
                BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                IEnumerable allSkills = null;
                foreach (string memberName in new[] { "allSkillDefs", "skillDefs" })
                {
                    FieldInfo field = catalogType.GetField(memberName, flags);
                    if (field != null) allSkills = field.GetValue(null) as IEnumerable;
                    if (allSkills == null)
                    {
                        PropertyInfo property = catalogType.GetProperty(memberName, flags);
                        if (property != null && property.GetIndexParameters().Length == 0)
                            allSkills = property.GetValue(null, null) as IEnumerable;
                    }
                    if (allSkills != null) break;
                }

                if (allSkills == null)
                {
                    logger?.LogWarning("[BANSHEE LOCALIZATION] SkillCatalog no disponible; keywords sin traducir.");
                    return;
                }

                HashSet<string> translated = new HashSet<string>(StringComparer.Ordinal);
                bool omenFound = false;
                bool aerodynamicFound = false;
                foreach (object item in allSkills)
                {
                    var skill = item as RoR2.Skills.SkillDef;
                    if (skill == null) continue;
                    string name = skill.skillNameToken ?? "";
                    string description = skill.skillDescriptionToken ?? "";
                    if (name.IndexOf("KENKO_BANSHEE", StringComparison.OrdinalIgnoreCase) < 0 &&
                        description.IndexOf("KENKO_BANSHEE", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    if (skill.keywordTokens == null) continue;
                    foreach (string keywordToken in skill.keywordTokens)
                    {
                        if (string.IsNullOrWhiteSpace(keywordToken) || !translated.Add(keywordToken))
                            continue;
                        string original = Language.GetString(keywordToken) ?? "";
                        string combined = keywordToken + " " + original;
                        if (combined.IndexOf("Ethereal Wisp", StringComparison.OrdinalIgnoreCase) >= 0 &&
                            combined.IndexOf("secondary", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            RegisterSpanishToken(keywordToken, OmenSpanishKeyword);
                            omenFound = true;
                        }
                        else if (combined.IndexOf("Aerodynamic", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                 combined.IndexOf("distance", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            RegisterSpanishToken(keywordToken, AerodynamicSpanishKeyword);
                            aerodynamicFound = true;
                        }
                    }
                }
                UsuLog.Verbose(logger, "[BANSHEE LOCALIZATION] Keywords | Presagio: " + omenFound +
                    " | Aerodinámico: " + aerodynamicFound + " | Solo ES-419 / ES-ES.");
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[BANSHEE LOCALIZATION] Keywords: " + ex.GetType().Name + ": " + ex.Message);
            }
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
        private static void RegisterEnglishToken(string token, string translation)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(translation)) return;
            LanguageAPI.Add(token, translation, "en");
        }
    }
}
