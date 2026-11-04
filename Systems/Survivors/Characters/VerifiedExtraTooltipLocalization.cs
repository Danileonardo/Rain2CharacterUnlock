using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Traducción curada de los paneles desplegables (SkillDef.keywordTokens).
    /// Sólo registra textos cuya versión inglesa se observó en el juego.
    /// No modifica SkillDef, los desbloqueos, las estadísticas ni las DLL de los creadores.
    /// </summary>
    internal static class VerifiedExtraTooltipLocalization
    {
        private static readonly List<object> Overlays = new List<object>();
        private static readonly HashSet<string> Applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly string[] Locales = { "es-419", "es-ES" };

        internal static void Register(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || survivor.SurvivorDef == null ||
                survivor.SurvivorDef.bodyPrefab == null) return;
            string body = survivor.BodyName ?? "";
            GameObject prefab = survivor.SurvivorDef.bodyPrefab;
            HashSet<SkillDef> skills = new HashSet<SkillDef>();
            foreach (GenericSkill slot in prefab.GetComponentsInChildren<GenericSkill>(true))
            {
                IEnumerable variants = Get(Get(slot, "skillFamily"), "variants") as IEnumerable;
                if (variants == null) continue;
                foreach (object variant in variants)
                {
                    SkillDef skill = Get(variant, "skillDef") as SkillDef;
                    if (skill != null) skills.Add(skill);
                }
            }
            // Algunos mods crean los SkillDef antes de asignar las variantes al
            // prefab. Añadir las habilidades registradas en SkillCatalog evita
            // omitir esos keywordTokens aunque no estén en SkillFamily.
            if (body.Equals("SeamstressBody", StringComparison.OrdinalIgnoreCase))
            {
                const BindingFlags sf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                Type catalog = typeof(SkillCatalog);
                IEnumerable all = null;
                foreach (string member in new[] { "allSkillDefs", "skillDefs" })
                {
                    FieldInfo f = catalog.GetField(member, sf);
                    if (f != null) all = f.GetValue(null) as IEnumerable;
                    if (all == null)
                    {
                        PropertyInfo prop = catalog.GetProperty(member, sf);
                        if (prop != null) all = prop.GetValue(null, null) as IEnumerable;
                    }
                    if (all != null) break;
                }
                if (all != null)
                    foreach (object item in all)
                    {
                        SkillDef skill = item as SkillDef;
                        if (skill == null) continue;
                        if ((skill.skillNameToken ?? "").IndexOf("KENKO_SEAMSTRESS", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            (skill.skillDescriptionToken ?? "").IndexOf("KENKO_SEAMSTRESS", StringComparison.OrdinalIgnoreCase) >= 0)
                            skills.Add(skill);
                    }
            }
            int translated = 0;
            if (body.Equals("SeamstressBody", StringComparison.OrdinalIgnoreCase))
            {
                // Título literal confirmado en la DLL: algunos paneles consultan
                // el nombre aparte del texto descriptivo del keyword.
                AddSpanish("Insatiable", "Insaciable");
                // Único keyword de la DLL con token fijo confirmado.
                // Los demás se generan dinámicamente y se recorren abajo.
                string plane = Language.GetString("KENKO_SEAMSTRESS_KEYWORD_PLANE_BOUND") ?? "";
                string planePlain = Regex.Replace(plane, "<[^>]*>", " ");
                if (TryTranslate(body, planePlain, out string planeEs) &&
                    AddSpanish("KENKO_SEAMSTRESS_KEYWORD_PLANE_BOUND", planeEs)) translated++;
            }
            foreach (SkillDef skill in skills)
            {
                if (skill.keywordTokens != null)
                    foreach (string token in skill.keywordTokens)
                        if (TranslateKeyword(body, token)) translated++;
                        else if (body.Equals("SeamstressBody", StringComparison.OrdinalIgnoreCase) &&
                                 !string.IsNullOrWhiteSpace(token))
                            UsuLog.Verbose(logger, "[USU KEYWORDS SEAMSTRESS] Sin coincidencia: " +
                                token.Substring(0, Math.Min(token.Length, 85)));
            }
            // Las pasivas pueden no formar parte de ninguna SkillFamily.
            SkillLocator locator = prefab.GetComponent<SkillLocator>();
            object passive = Get(locator, "passiveSkill");
            IEnumerable passiveKeywords = Get(passive, "keywordTokens") as IEnumerable;
            if (passiveKeywords != null)
                foreach (object raw in passiveKeywords)
                    if (TranslateKeyword(body, raw as string)) translated++;
            string singleKeyword = Get(passive, "keywordToken") as string;
            if (TranslateKeyword(body, singleKeyword)) translated++;
            UsuLog.Verbose(logger, "[USU KEYWORDS] " + body + " | traducidos=" + translated +
                " | encontrados=" + skills.Count + " SkillDef.");
        }

        private static bool TranslateKeyword(string body, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            string source = Language.GetString(token) ?? "";
            if (string.IsNullOrWhiteSpace(source) || source == "???") source = token;
            string plain = Regex.Replace(source, "<[^>]*>", " ");
            // En la DLL, KeywordText puede contener solo el cuerpo del efecto
            // mientras su nombre está en un segundo segmento.
            string combined = Regex.Replace(token + " " + plain, "<[^>]*>", " ");
            return TryTranslate(body, combined, out string spanish) && AddSpanish(token, spanish);
        }

        internal static void Overview(SurvivorInfo survivor, string spanish)
        {
            if (survivor == null || survivor.SurvivorDef == null) return;
            AddSpanish(survivor.SurvivorDef.descriptionToken, spanish);
        }

        private static bool AddSpanish(string token, string value)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return false;
            bool any = false;
            foreach (string lang in Locales)
            {
                if (!Applied.Add(lang + ":" + token)) continue;
                Overlays.Add(LanguageAPI.AddOverlay(token, value, lang));
                any = true;
            }
            return any;
        }

        private static bool Has(string text, string key)
        {
            return text.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // Sólo coincidencias verificadas en las capturas del 29/10, sin
        // sustituir cifras del mod por otras ni alterar tooltips desconocidos.
        private static bool TryTranslate(string body, string text, out string translated)
        {
            translated = null;
            if (body.Equals("SeamstressBody", StringComparison.OrdinalIgnoreCase))
            {
                if (Has(text, "Plane Bound") && Has(text, "missing health"))
                {
                    // La pasiva usa valores configurables: conservar los
                    // números exactos publicados por el mod cargado.
                    Match baseDamage = Regex.Match(text,
                        @"Gain\s+([0-9.,]+)\s+base\s+damage",
                        RegexOptions.IgnoreCase);
                    Match heal = Regex.Match(text,
                        @"Heal\s+up\s+to\s+([0-9.,]+)%",
                        RegexOptions.IgnoreCase);
                    if (baseDamage.Success && heal.Success)
                        translated = "[ Vinculada al plano ]\nObtén <style=cIsDamage>" +
                            baseDamage.Groups[1].Value + " de daño base</style> " +
                            "por cada punto de <style=cIsHealth>salud faltante</style>. " +
                            "Al golpear, recuperas hasta el <style=cIsHealing>" +
                            heal.Groups[1].Value + "%</style> de la salud que te falte.";
                }
                else if (Has(text, "Needles") && Has(text, "picking up") && Has(text, "Insatiable"))
                    translated = "[ Agujas ]\nObtén <color=#9B3737>agujas</color> al recoger tus armas <color=#9B3737>simbióticas</color> o golpear enemigos durante <color=#9B3737>Insaciable</color>.";
                else if (Has(text, "Agile") && Has(text, "used while sprinting"))
                    translated = "[ Ágil ]\nPuedes utilizar esta habilidad mientras corres.";
                else if (Has(text, "Ruthless") && Has(text, "additional strike"))
                    translated = "[ Despiadado ]\nCuando tienes disponibles tus tijeras <color=#9B3737>simbióticas</color>, realizas un golpe adicional.";
                else if (Has(text, "Attacks") && Has(text, "bleed") && Has(text, "Healing") && Has(text, "converted") && Has(text, "Barrier"))
                    translated = "[ Insaciable ]\nLos ataques causan <style=cIsHealth>hemorragia</style> y otorgan <color=#9B3737>agujas</color>. La <style=cIsHealing>curación</style> se transforma en <style=cIsHealing>barrera</style>.";
                else if (Has(text, "Symbiotic") && Has(text, "each stock missing"))
                    translated = "[ Simbiótico ]\nObtienes <style=cIsDamage>velocidad de ataque</style> y <style=cIsUtility>velocidad de movimiento</style> por cada carga que te falte. Las cargas adicionales se convierten en cargas faltantes.";
                else if (Has(text, "Lost and Found") && Has(text, "Scissors return"))
                    translated = "[ Perdido y recuperado ]\nAl recoger unas tijeras, ejecutas un corte y obtienes una <color=#9B3737>aguja</color>. Las tijeras regresan cuando su objetivo muere.";
                else if (Has(text, "Crush") && Has(text, "velocity") && Has(text, "max health"))
                    translated = "[ Aplastamiento ]\nInflige daño adicional según la velocidad y la <style=cIsHealth>salud máxima</style> del enemigo.";
            }
            else if (body.Equals("AurielBody", StringComparison.OrdinalIgnoreCase))
            {
                if (Has(text, "Divine Beam") && Has(text, "100%") && Has(text, "Energy"))
                    translated = "[ Rayo divino (25) ]\nAuriel canaliza su poder divino y atraviesa a los enemigos con un potente haz de luz que inflige <style=cIsDamage>100% + Energía</style> por impacto y los quema.";
                else if (Has(text, "Resurrect") && Has(text, "2s") && Has(text, "full health"))
                    translated = "[ Resurrección (100) ]\nAuriel canaliza el espíritu de un aliado caído. Tras <style=cIsUtility>2 s</style>, lo devuelve a la vida. Si no hay ningún aliado que resucitar, Auriel recupera toda su salud.";
            }
            else if (body.Equals("WooperBody", StringComparison.OrdinalIgnoreCase))
            {
                if (Has(text, "Stockpile") && Has(text, "armor by 5"))
                    translated = "[ Pulsar: Reserva ]\nWooper almacena poder y aumenta su armadura en <style=cIsUtility>5</style>. Puede usar este movimiento hasta <style=cIsUtility>3</style> veces.";
                else if (Has(text, "Swallow") && Has(text, "25%-100%"))
                    translated = "[ Mantener: Tragar ]\nAbsorbe el poder almacenado mediante Reserva para curarse entre un <style=cIsHealing>25% y un 100%</style>.";
                else if (Has(text, "Spit Up") && Has(text, "450%-1350%"))
                    translated = "[ Pulsación doble: Escupir ]\nLibera de golpe el poder almacenado mediante Reserva en un ataque que inflige entre <style=cIsDamage>450% y 1350%</style> de daño.";
            }
            else if (body.Equals("WarlockBody", StringComparison.OrdinalIgnoreCase))
            {
                if (Has(text, "Crimson Mana Empowerment") && Has(text, "Primary: 1 cast"))
                    translated = "[ Potenciación de Maná carmesí ]\nLas activaciones potenciadas se acumulan hasta que se usan.\nPrincipal: 1 activación.\nSecundaria: la mitad de las cargas máximas.\nUtilidad: 1 activación.";
            }
            else if (body.Equals("DarthVaderBody", StringComparison.OrdinalIgnoreCase))
            {
                if (Has(text, "Cooldown Passive") && Has(text, "reduces all cooldowns"))
                    translated = "[ Pasiva de recargas ]\nGolpear enemigos reduce todos los tiempos de reutilización en <style=cIsUtility>1 segundo</style>.";
                else if (Has(text, "Energy Passive") && Has(text, "regens 10 energy"))
                    translated = "[ Pasiva de energía ]\nGolpear enemigos regenera <style=cIsHealing>10 de energía</style>, cantidad que aumenta con la velocidad de ataque.";
            }
            return translated != null;
        }

        private static object Get(object instance, string name)
        {
            if (instance == null) return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                FieldInfo f = instance.GetType().GetField(name, flags);
                if (f != null) return f.GetValue(instance);
                PropertyInfo p = instance.GetType().GetProperty(name, flags);
                if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(instance, null);
            }
            catch { }
            return null;
        }
    }
}
