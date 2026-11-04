using System;
using System.Reflection;
using BepInEx.Logging;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// HEL-P 2.3.0 (rob / com.rob.Ultron): localization-only integration.
    /// No external assembly references; retains PlNK origin, then adds the USU sequel in both languages.
    /// New official unlocks live in their own MissionPreset, not in this Definition.
    /// </summary>
    public sealed class HelPDefinition : SurvivorDefinition
    {
        private const string NativeOverviewToken = "USU_HELP_BODY_DESCRIPTION";

        public override string SourceIdentifier => "com.rob.Ultron";
        public override string BodyName => "RobUltronBody";
        public override string DefinitionName => "HEL-P";

        private const string SpanishOverview = HelPLocalization.SpanishOverview;
        private const string EnglishOverview = HelPLocalization.EnglishOverview;
        private static readonly string SpanishLore = HelPLore.SpanishLore;
        private static readonly string EnglishLore = HelPLore.EnglishLore;

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterSpanishTokens();
            RegisterEnglishLore();
            RegisterNativeOverview(survivor, logger);
            // NO se instala un parche global sobre Language.GetLocalizedStringByToken.
            UsuLog.Verbose(logger, "[HEL-P RECOVERY] Vista general activa; hook global de recarga DESHABILITADO.");
            RegisterHelPPlaceholder(survivor);
            RegisterHelPSkins(survivor, logger);
            UsuLog.Verbose(logger, "[HEL-P LOCALIZATION] com.rob.Ultron / RobUltronBody | 2.3.0 | " +
                "identidad, pasiva, habilidades, skins y Diario I PlNK + USU ES-419 / ES-ES / EN");
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "Ayudante ligeramente confundido" : (fallback ?? "");
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishOverview :
                IsEnglish() ? EnglishOverview : (fallback ?? "");
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            // HEL-P Diario I: the creator's origin (PlNK) followed by USU's sequel.
            // Unlock state and native Logbook visibility are owned elsewhere.
            return IsSpanish() ? SpanishLore : IsEnglish() ? EnglishLore : (fallback ?? "");
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnglish()
        {
            return string.Equals(Language.currentLanguageName ?? "", "en", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddSpanish(string token, string translated)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            LanguageAPI.Add(token, translated, "es-419");
            LanguageAPI.Add(token, translated, "es-ES");
        }

        private static void RegisterEnglishLore()
        {
            // The original scene remains at the beginning; USU's sequel follows.
            LanguageAPI.Add("ROB_ULTRON_BODY_LORE", EnglishLore, "en");
        }

        private static void RegisterSpanishTokens()
        {
            AddSpanish("ROB_ULTRON_BODY_NAME", "HEL-P");
            AddSpanish("ROB_ULTRON_BODY_SUBTITLE", "Ayudante ligeramente confundido");
            AddSpanish("ROB_ULTRON_BODY_LORE", SpanishLore);
            AddSpanish("DYNAMIC FLIGHT", "Vuelo dinámico");
            AddSpanish("HEL-P IS ABLE TO <style=cIsUtility>FLY FREELY</style>, BUT IS <style=cIsHealth>EXTREMELY FRAGILE</style>.", "HEL-P puede <style=cIsUtility>volar libremente</style>, pero es <style=cIsHealth>extremadamente frágil</style>.");
            AddSpanish("ENCEPHALO-RAY", "Rayo encefálico");
            AddSpanish("UNLEASH A BURNING ENERGY BEAM THAT DEALS <style=cIsDamage>350% DAMAGE</style>, ENDING IN AN <style=cIsUtility>EXPLOSION</style> DEALING <style=cIsDamage>500% DAMAGE</style>.", "Dispara un haz de energía abrasadora que inflige <style=cIsDamage>350% de daño</style> y termina en una <style=cIsUtility>explosión</style> que inflige <style=cIsDamage>500% de daño</style>.");
            AddSpanish("REPULSOR BLAST", "Explosión repulsora");
            AddSpanish("FIRE NANO PULSE CANNONS THAT <style=cIsHealth>HARM HOSTILE COMBATANTS</style> AND <style=cIsHealing>HEAL FRIENDLY COMBATANTS</style> FOR <style=cIsDamage>220%</style>.", "Dispara cañones de nanopulsos que <style=cIsHealth>dañan a los enemigos</style> y <style=cIsHealing>curan a los aliados</style> por un <style=cIsDamage>220%</style>.");
            AddSpanish("NANO-BEAM", "Nanorrayo");
            AddSpanish("IMPERATIVE: PATCH", "IMPERATIVO: REPARAR");
            AddSpanish("TARGET AN <style=cIsUtility>ENTITY</style> TO DEPLOY A <style=cIsUtility>DRONE</style> TO THEM. ENEMIES SUFFER A CHAINING <style=cIsDamage>ELECTRIC SHOCK FOR <style=cIsDamage>100% DAMAGE</style>, WHILE ALLIES <style=cIsHealing>HEAL 25% MAX HEALTH</style>.", "Selecciona una <style=cIsUtility>entidad</style> para enviarle un <style=cIsUtility>dron</style>. Los enemigos reciben una <style=cIsDamage>descarga eléctrica encadenada del 100% de daño</style>; los aliados <style=cIsHealing>recuperan un 25% de su salud máxima</style>.");
            AddSpanish("IMPERATIVE: NEUTRALIZE", "IMPERATIVO: NEUTRALIZAR");
            AddSpanish("ZAP A HOSTILE COMBATANT TO <style=cIsUtility>SHOCK</style> THEM FOR <style=cIsDamage>30%</style> DAMAGE. <style=cIsUtility>HOLD UP TO 2 CHARGES.</style>", "Electrocuta a un enemigo para aplicarle <style=cIsUtility>Conmoción</style> e infligir <style=cIsDamage>30% de daño</style>. <style=cIsUtility>Almacena hasta 2 cargas.</style>");
            AddSpanish("IMPERATIVE: FLEE", "IMPERATIVO: HUIR");
            AddSpanish("DASH, GAINING <style=cIsHealing>25% BARRIER</style>, AND THEN ENTER AN <style=cIsUtility>ACCELERATED</style> STATE FOR THE NEXT <style=cIsDamage>7 SECONDS</style>.", "Desplázate rápidamente, obtén <style=cIsHealing>25% de barrera</style> y entra en un estado <style=cIsUtility>acelerado</style> durante los siguientes <style=cIsDamage>7 segundos</style>.");
            AddSpanish("IMPERATIVE: EXTERMINATE", "IMPERATIVO: EXTERMINAR");
            AddSpanish("FIRE A BARRAGE OF ENERGY THAT <style=cIsHealth>HARMS HOSTILE COMBATANTS</style> AND <style=cIsHealing>HEALS FRIENDLY COMBATANTS</style> FOR <style=cIsDamage>5x600%</style>. <style=cIsUtility>HOLD UP TO 3 CHARGES.</style>", "Dispara una ráfaga de energía que <style=cIsHealth>daña a los enemigos</style> y <style=cIsHealing>cura a los aliados</style> por <style=cIsDamage>5 × 600%</style>. <style=cIsUtility>Almacena hasta 3 cargas.</style>");
            AddSpanish("INVINCIBLE PULSE CANNON", "Cañón de pulsos invencible");
            AddSpanish("FIRE A DEVASTATING PULSE CANNON THAT <style=cIsHealth>HARMS HOSTILE COMBATANTS</style> AND <style=cIsHealing>HEALS FRIENDLY COMBATANTS</style> FOR <style=cIsDamage>4000%</style>.", "Dispara un devastador cañón de pulsos que <style=cIsHealth>daña a los enemigos</style> y <style=cIsHealing>cura a los aliados</style> por <style=cIsDamage>4000%</style>.");
            AddSpanish("USU_HELP_WIP_SKILL", "Habilidad en desarrollo");
            AddSpanish("USU_HELP_SKIN_DEFAULT", "Original");
            AddSpanish("USU_HELP_SKIN_ULTRON", "Ultron");
            // The original generic English strings must not be replaced globally.
            LanguageAPI.Add("USU_HELP_WIP_SKILL", "WIP SKILL", "en");
            LanguageAPI.Add("USU_HELP_SKIN_DEFAULT", "Default", "en");
            LanguageAPI.Add("USU_HELP_SKIN_ULTRON", "Ultron", "en");
            LanguageAPI.Add("USU_HELP_WIP_SKILL", "WIP SKILL");
            LanguageAPI.Add("USU_HELP_SKIN_DEFAULT", "Default");
            LanguageAPI.Add("USU_HELP_SKIN_ULTRON", "Ultron");
        }

        /// <summary>
        /// El panel "Vista general" usa el SurvivorDef.descriptionToken nativo;
        /// ResolveDescription sólo alimenta las fichas internas de USU.
        /// HEL-P 2.3.0 no proporciona un texto visible allí. Registramos un
        /// token propio en EN/ES y se lo asignamos exclusivamente a HEL-P.
        /// </summary>
        private static void RegisterNativeOverview(SurvivorInfo survivor, ManualLogSource logger)
        {
            LanguageAPI.Add(NativeOverviewToken, SpanishOverview, "es-419");
            LanguageAPI.Add(NativeOverviewToken, SpanishOverview, "es-ES");
            LanguageAPI.Add(NativeOverviewToken, EnglishOverview, "en");
            LanguageAPI.Add(NativeOverviewToken, EnglishOverview);

            try
            {
                SurvivorDef def = survivor?.SurvivorDef;
                if (def == null) return;
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                FieldInfo field = def.GetType().GetField("descriptionToken", flags);
                if (field != null && field.FieldType == typeof(string))
                {
                    field.SetValue(def, NativeOverviewToken);
                    UsuLog.Verbose(logger, "[HEL-P OVERVIEW] SurvivorDef.descriptionToken -> " + NativeOverviewToken);
                    return;
                }
                PropertyInfo property = def.GetType().GetProperty("descriptionToken", flags);
                if (property != null && property.CanWrite && property.PropertyType == typeof(string))
                {
                    property.SetValue(def, NativeOverviewToken, null);
                    UsuLog.Verbose(logger, "[HEL-P OVERVIEW] SurvivorDef.descriptionToken -> " + NativeOverviewToken);
                    return;
                }
                logger?.LogWarning("[HEL-P OVERVIEW] descriptionToken no disponible en este build.");
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[HEL-P OVERVIEW] No se pudo asignar descripción nativa: " + ex.Message);
            }
        }

        private static void RegisterHelPPlaceholder(SurvivorInfo survivor)
        {
            try
            {
                GameObject prefab = survivor?.SurvivorDef?.bodyPrefab;
                SkillLocator locator = prefab != null ? prefab.GetComponent<SkillLocator>() : null;
                if (locator == null) return;
                GenericSkill[] slots = { locator.primary, locator.secondary, locator.utility, locator.special };
                foreach (GenericSkill slot in slots)
                {
                    SkillFamily family = slot != null ? slot.skillFamily : null;
                    if (family == null || family.variants == null) continue;
                    foreach (SkillFamily.Variant variant in family.variants)
                    {
                        SkillDef skill = variant.skillDef;
                        if (skill == null) continue;
                        if (string.Equals(skill.skillNameToken, "NANO-BEAM", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(skill.skillDescriptionToken, "WIP SKILL", StringComparison.OrdinalIgnoreCase))
                        {
                            skill.skillDescriptionToken = "USU_HELP_WIP_SKILL";
                        }
                    }
                }
            }
            catch
            {
                // The creator's skill setup must remain usable even when a variant is absent.
            }
        }

        private static void RegisterHelPSkins(SurvivorInfo survivor, ManualLogSource logger)
        {
            try
            {
                GameObject prefab = survivor?.SurvivorDef?.bodyPrefab;
                ModelLocator locator = prefab != null ? prefab.GetComponent<ModelLocator>() : null;
                Transform model = locator != null ? locator.modelTransform : null;
                ModelSkinController controller = model != null ? model.GetComponent<ModelSkinController>() : null;
                if (controller == null || controller.skins == null) return;
                int localized = 0;
                foreach (SkinDef skin in controller.skins)
                {
                    if (skin == null || string.IsNullOrWhiteSpace(skin.nameToken)) continue;
                    string name = skin.nameToken.Trim();
                    if (string.Equals(name, "Default", StringComparison.OrdinalIgnoreCase))
                    {
                        skin.nameToken = "USU_HELP_SKIN_DEFAULT";
                        localized++;
                    }
                    else if (string.Equals(name, "Ultron", StringComparison.OrdinalIgnoreCase))
                    {
                        skin.nameToken = "USU_HELP_SKIN_ULTRON";
                        localized++;
                    }
                }
                UsuLog.Verbose(logger, "[HEL-P SKINS] Nombres propios identificados: " + localized);
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[HEL-P SKINS] No fue posible localizar los nombres de aspecto: " + ex.Message);
            }
        }
    }
}
