using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Localizacion comprobada en BayoMod.dll (com.psyche.BayoMod, PSYCHE_BAYO_*).
    /// Se registran exclusivamente tokens propios de Bayonetta; no modifica la mision ni el lore.
    /// Nombres, Overview, 11 ayudas de habilidad y tres skins presentes en la DLL.
    /// Los porcentajes se obtienen del texto registrado para no fijar valores de otra version.
    /// </summary>
    internal static class BayonettaVerifiedLocalization
    {
        private static readonly List<object> Overlays = new List<object>();
        private static readonly HashSet<string> Seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal const string SpanishOverview =
            "Bayonetta combina ataques cuerpo a cuerpo y a distancia. Puede entrar y salir del peligro para infligir un gran daño.\n\n" +
            "• Si esquivas mientras usas Artes balísticas y mantienes pulsado el botón de ataque, el combo continuará desde donde quedó.\n\n" +
            "• La armadura y ralentización de Tiempo Brujo la protegen de las amenazas cercanas. Aprovecha ese efecto para acercarte y usar sus técnicas de corto alcance.\n\n" +
            "• Muchos movimientos pueden cancelarse antes de tiempo con un salto. Todos pueden cancelarse con Esquiva.\n\n" +
            "• Los enemigos pesados con menos de la mitad de su salud también reciben mayor empuje de los ataques de Bayonetta.\n\n" +
            "• Pulsa repetidamente el ataque principal durante un castigo para acelerar el remate.";

        internal static void RegisterFor(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || survivor.SurvivorDef == null ||
                !string.Equals(survivor.BodyName, "BayoBody", StringComparison.OrdinalIgnoreCase))
                return;

            Overlay("PSYCHE_BAYO_DESCRIPTION", SpanishOverview);
            Overlay("PSYCHE_BAYO_SUBTITLE", "El Ojo Izquierdo de la Oscuridad");
            Overlay("PSYCHE_BAYO_PUNISH_PROMPT", "¡Castigar!");
            // Skin names are verified in BayoMod.dll v0.5.5: Remembrance, Famed, Rogue.
            Overlay("PSYCHE_BAYO_BAYO1_SKIN_NAME", "Reminiscencia");
            Overlay("PSYCHE_BAYO_BAYO2_SKIN_NAME", "Bruja célebre");
            Overlay("PSYCHE_BAYO_ART_SKIN_NAME", "Forajida");

            Overlay("PSYCHE_BAYO_PRIMARY_COMBO_NAME", "Artes balísticas");
            Overlay("PSYCHE_BAYO_SECONDARY_ABK_NAME", "Técnicas de Umbra");
            Overlay("PSYCHE_BAYO_UTILITY_DODGE_NAME", "Esquiva");
            Overlay("PSYCHE_BAYO_SPECIAL_WEAVEIN_NAME", "Tetsuzanko / Pisotón de tacón");
            Overlay("PSYCHE_BAYO_SPECIAL_TETSU_NAME", "Tetsuzanko");
            Overlay("PSYCHE_BAYO_SPECIAL_STOMP_NAME", "Pisotón de tacón");
            Overlay("PSYCHE_BAYO_SPECIAL_WEEAVEOUT_NAME", "Cancelar");

            Overlay("PSYCHE_BAYO_SECONDARY_ABK_DESCRIPTION",
                "Realiza diferentes técnicas según la dirección de movimiento y si Bayonetta está en tierra o en el aire.");
            Overlay("PSYCHE_BAYO_SPECIAL_WEAVEIN_DESCRIPTION",
                "Fija a un enemigo y utiliza el ataque principal o secundario para invocar un Tejido infernal sobre su posición.");

            // Use tokens from the actual DLL, not guessed shared names such as 'Default'.
            Keyword("KEYWORD_BAYO_ABK", "Patada propulsora", "En el aire: mantén adelante + secundaria.",
                "Impúlsate con una patada ardiente que empuja a los enemigos ligeros e inflige {0} de daño.");
            Keyword("KEYWORD_BAYO_SPIN", "Tornado de tacones", "En el aire: mantén neutral + secundaria.",
                "Ejecuta una patada giratoria que inflige {0} de daño continuamente y empuja a los enemigos ligeros.");
            Keyword("KEYWORD_BAYO_DOWN", "Caída mortal", "En el aire: mantén atrás + secundaria.",
                "Desciende rápidamente con una patada que inflige {0} de daño continuamente y derriba a los enemigos.");
            Keyword("KEYWORD_BAYO_HEEL", "Deslizamiento de tacón", "En tierra: mantén adelante + secundaria.",
                "Deslízate hacia delante a gran velocidad e inflige {0} de daño. Mantén pulsado al detenerte para ejecutar una patada ascendente que eleva a los enemigos ligeros e inflige {1} de daño.");
            Keyword("KEYWORD_BAYO_BREAK", "Baile acrobático", "En tierra: mantén neutral + secundaria.",
                "Realiza un baile que inflige {0} de daño por impacto de forma continua.");
            Keyword("KEYWORD_BAYO_RISE", "Disparo de luna llena", "En tierra: mantén atrás + secundaria.",
                "Haz una voltereta hacia atrás que eleva a los enemigos ligeros e inflige {0} de daño. Mantén pulsado para seguir girando e infligir {1} de daño de forma continua.");
            Keyword("KEYWORD_BAYO_WT", "Tiempo Brujo", "",
                "Obtienes 350 de armadura. Los enemigos y proyectiles cercanos se ralentizan considerablemente; las técnicas que normalmente elevan a enemigos ligeros afectan ahora a todos los que no sean jefes.");
            Keyword("KEYWORD_BAYO_TETS", "Tetsuzanko", "Ataque principal (M1).",
                "Invoca un puño demoníaco que empuja a los enemigos e inflige {0} de daño.");
            Keyword("KEYWORD_BAYO_HSTOMP", "Pisotón de tacón", "Ataque secundario (M2).",
                "Invoca un pie demoníaco que derriba a los enemigos, los empuja hacia abajo e inflige {0} de daño.");
            Keyword("KEYWORD_BAYO_KD", "Derribado", "",
                "Los enemigos derribados permanecen aturdidos durante un periodo prolongado. Si Bayonetta está en tierra y suficientemente cerca, puede pulsar Interactuar para ejecutar un ataque de castigo.");

            // These descriptions are complete in the supplied DLL; preserve inline skill colors.
            Overlay("PSYCHE_BAYO_UTILITY_DODGE_DESCRIPTION",
                "Esquiva una corta distancia, obtén una breve invulnerabilidad y <style=cIsUtility>100 de armadura</style>. " +
                "Si recibes un golpe durante la invulnerabilidad, activas <style=cIsUtility>Tiempo Brujo</style>.");

            // Dynamic combo percentages: never replace with hard-coded values.
            string combo = SafeGet("PSYCHE_BAYO_PRIMARY_COMBO_DESCRIPTION");
            if (combo.IndexOf("5 hit punch sequence", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MatchCollection nums = Regex.Matches(combo, @"(?:\d+(?:[.,]\d+)?|\{\d+\})%");
                if (nums.Count >= 4)
                {
                    Overlay("PSYCHE_BAYO_PRIMARY_COMBO_DESCRIPTION",
                        "Ejecuta una combinación de cinco puñetazos. Los cuatro primeros infligen " + Damage(nums[0].Value) +
                        " de daño cada uno. El quinto es una ráfaga de " + Damage(nums[1].Value) +
                        " por golpe; al soltar el botón, invoca un Tejido infernal de " + Damage(nums[2].Value) +
                        " de daño. Sus armas también disparan durante el combo e infligen " + Damage(nums[3].Value) +
                        " de daño por bala.");
                }
            }
            UsuLog.Verbose(logger, "[BAYONETTA] Overview + keywords + skins verificados en BayoMod.dll; preservadas cifras dinamicas.");
        }

        private static string Damage(string value) => "<style=cIsDamage>" + value + "</style>";

        private static void Keyword(string token, string title, string instruction, string body)
        {
            string original = SafeGet(token);
            if (string.IsNullOrWhiteSpace(original) ||
                string.Equals(original, token, StringComparison.OrdinalIgnoreCase))
                return;
            MatchCollection percentages = Regex.Matches(original, @"(?:\d+(?:[.,]\d+)?|\{\d+\})%");
            int requested = Regex.Matches(body, @"\{\d+\}").Count;
            if (percentages.Count < requested) return; // Do not change values if source is unknown.
            for (int i = 0; i < requested; i++)
                body = body.Replace("{" + i + "}", Damage(percentages[i].Value));
            string translated = "<style=cKeywordName>" + title + "</style> ";
            if (!string.IsNullOrEmpty(instruction))
                translated += "<style=cIsUtility>" + instruction + "</style> ";
            translated += "<style=cSub>" + body + "</style>";
            Overlay(token, translated);
        }

        private static string SafeGet(string token)
        {
            try { return Language.GetString(token) ?? ""; }
            catch { return ""; }
        }

        private static void Overlay(string token, string spanish)
        {
            if (string.IsNullOrWhiteSpace(spanish) || !Seen.Add(token)) return;
            Overlays.Add(LanguageAPI.AddOverlay(token, spanish, "es-419"));
            Overlays.Add(LanguageAPI.AddOverlay(token, spanish, "es-ES"));
        }
    }
}
