using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Wooper.
    ///
    /// Autoridad USU para localización visible ES, pasiva, skills, skins,
    /// Vista general reparada ES/EN y lore curado ES/EN.
    ///
    /// La misión oficial USU "De vuelta al agua" permanece en el sistema
    /// legacy de misiones y no se migra en esta fase.
    ///
    /// No referencia Wooper.dll. Sólo usa Body, ContentPack y tokens públicos.
    /// </summary>
    public sealed class WooperDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get { return "com.Dragonyck.Wooper"; }
        }

        public override string BodyName
        {
            get { return "WooperBody"; }
        }

        public override string DefinitionName
        {
            get { return "Wooper"; }
        }

        private static object WooperEnglishDescriptionOverlay;

        private static readonly string WooperSpanishDescription =
            "Wooper es un superviviente flexible que alterna movimientos de <color=#55b8e2>Agua</color> " +
            "y <color=#9b5f41>Tierra</color> para controlar el ritmo del combate.<style=cSub>\r\n\r\n" +
            "< ! > Absorbe Agua hace que las habilidades de Agua apliquen <style=cIsUtility>Empapado</style> " +
            "al golpear durante <style=cIsUtility>4 s</style>.<style=cSub>\r\n\r\n" +
            "< ! > Las habilidades de Tierra consumen Empapado al golpear, <style=cIsUtility>ralentizan</style>, " +
            "infligen <style=cIsDamage>120% de daño</style> y <style=cIsHealth>curan</style> a Wooper un " +
            "<style=cIsHealth>2% de salud</style>.<style=cSub>\r\n\r\n" +
            "< ! > Acua Cola puede acumular Empapado hasta <style=cIsUtility>3</style> veces y repite el ataque " +
            "contra objetivos que todavía tengan menos de 3 acumulaciones.<style=cSub>\r\n\r\n" +
            "< ! > Protección y Excavar ofrecen dos formas distintas de sobrevivir, mientras Surf y Torbellino " +
            "permiten controlar zonas amplias.";

        private static readonly string WooperEnglishDescription =
            "Wooper is a flexible survivor that alternates <color=#55b8e2>Water</color> and " +
            "<color=#9b5f41>Ground</color> moves to control the pace of combat.<style=cSub>\r\n\r\n" +
            "< ! > Water Absorb causes Water skills to inflict <style=cIsUtility>Soaked</style> on hit for " +
            "<style=cIsUtility>4s</style>.<style=cSub>\r\n\r\n" +
            "< ! > Ground skills consume Soaked on hit, <style=cIsUtility>slowing</style>, inflicting " +
            "<style=cIsDamage>120% damage</style> and <style=cIsHealth>healing</style> Wooper for " +
            "<style=cIsHealth>2% health</style>.<style=cSub>\r\n\r\n" +
            "< ! > Aqua Tail can stack Soaked up to <style=cIsUtility>3</style> times and repeats against targets " +
            "that still have fewer than 3 stacks.<style=cSub>\r\n\r\n" +
            "< ! > Protect and Dig provide two different ways to survive, while Surf and Whirlpool help control " +
            "large areas.";

        private static readonly string WooperSpanishLore =
            "Registro de campo recuperado de una zona pantanosa de Petrichor V.\n\n" +
            "Las primeras huellas parecían demasiado pequeñas para pertenecer a algo peligroso. Terminaban junto " +
            "a una poza poco profunda donde una criatura azul observaba el agua con una sonrisa imposible de " +
            "interpretar. No huyó cuando se acercó el equipo de reconocimiento. Tampoco atacó. Simplemente esperó.\n\n" +
            "La calma terminó cuando aparecieron las criaturas locales. Wooper respondió inundando el terreno, " +
            "levantando barro y desapareciendo bajo el suelo antes de surgir entre sus perseguidores. Cada charco " +
            "que dejaba atrás convertía el lugar en algo que parecía conocer de toda la vida.\n\n" +
            "Lo más extraño era su tranquilidad. Entre explosiones, golpes y tierra removida, seguía mostrando la " +
            "misma expresión alegre, como si sobrevivir en un planeta hostil fuese otra tarde jugando junto al agua.\n\n" +
            "Cuando el combate terminó, regresó a la poza. Durante unos segundos no hubo disparos ni alarmas: sólo " +
            "ondas pequeñas extendiéndose sobre la superficie.\n\n" +
            "Después siguió al grupo. Nadie recuerda haberlo invitado.";

        private static readonly string WooperEnglishLore =
            "Field record recovered from a marshland region of Petrichor V.\n\n" +
            "The first tracks seemed far too small to belong to anything dangerous. They ended beside a shallow " +
            "pool where a blue creature watched the water with a smile that was impossible to interpret. It did " +
            "not flee when the survey team approached. It did not attack either. It simply waited.\n\n" +
            "The calm ended when the local creatures arrived. Wooper answered by flooding the ground, throwing up " +
            "mud, and vanishing beneath the soil before surfacing among its pursuers. Every puddle it left behind " +
            "made the area look like somewhere it had known all its life.\n\n" +
            "The strangest part was its composure. Between explosions, impacts, and torn earth, it kept the same " +
            "cheerful expression, as though surviving on a hostile planet were simply another afternoon spent by " +
            "the water.\n\n" +
            "When the fighting ended, it returned to the pool. For a few seconds there were no gunshots or alarms, " +
            "only small ripples spreading across the surface.\n\n" +
            "Then it followed the group. No one remembers inviting it.";

        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            RegisterEnglishDescriptionOverlay();
            RegisterEnglishLoreFallback();
            RegisterWooperSpanishTokens();
            RegisterWooperPassiveSpanish(survivor, logger);

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | WooperBody | " +
                "es-419 / es-ES + vista general USU + pasiva + skills + skins + lore"
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Nadador sonriente"
                : fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return WooperSpanishDescription;
            }

            if (IsEnglishLanguage() && IsBrokenDescriptionText(fallback))
            {
                return WooperEnglishDescription;
            }

            return fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return WooperSpanishLore;
            }

            if (IsEnglishLanguage() && IsMissingLoreText(fallback))
            {
                return WooperEnglishLore;
            }

            return fallback ?? "";
        }

        private static void RegisterEnglishDescriptionOverlay()
        {
            if (WooperEnglishDescriptionOverlay != null)
            {
                return;
            }

            // Wooper 1.0.0 publica WOOPER_WOOPER_DESCRIPTION, pero sólo
            // contiene saltos y marcadores < ! >. La superposición repara
            // únicamente esa Vista general inglesa.
            WooperEnglishDescriptionOverlay =
                LanguageAPI.AddOverlay(
                    "WOOPER_WOOPER_DESCRIPTION",
                    WooperEnglishDescription,
                    "en"
                );
        }

        private static void RegisterEnglishLoreFallback()
        {
            // El audit no expone lore nativo. RoR2 deriva habitualmente el
            // token del Logbook sustituyendo _NAME por _LORE.
            LanguageAPI.Add(
                "WOOPER_WOOPER_LORE",
                WooperEnglishLore,
                "en"
            );
        }

        private static void RegisterWooperSpanishTokens()
        {
            RegisterSpanishToken("WOOPER_WOOPER_SUBTITLE", "Nadador sonriente");
            RegisterSpanishToken("WOOPER_WOOPER_DESCRIPTION", WooperSpanishDescription);
            RegisterSpanishToken("WOOPER_WOOPER_LORE", WooperSpanishLore);

            // Pasiva
            RegisterSpanishToken("WOOPER__PASSIVE", "Absorbe Agua");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__PASSIVE_DESCRIPTION",
                "Las habilidades de <color=#55b8e2>Agua</color> aplican <style=cIsUtility>Empapado</style> " +
                "al golpear durante <style=cIsUtility>4 s</style>. Las habilidades de <color=#9b5f41>Tierra</color> " +
                "consumen <style=cIsUtility>Empapado</style> al golpear, <style=cIsUtility>ralentizan</style>, infligen " +
                "<style=cIsDamage>120% de daño</style> y <style=cIsHealth>curan</style> a Wooper un " +
                "<style=cIsHealth>2% de salud</style>."
            );

            // Primarias
            RegisterSpanishToken("WOOPER__M1", "Pistola Agua");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__M1_DESCRIPTION",
                "<color=#55b8e2>Agua</color>. El objetivo recibe un potente chorro de agua que inflige " +
                "<style=cIsDamage>80% de daño</style>."
            );

            RegisterSpanishToken("WOOPER__M1_ALT", "Disparo Lodo");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__M1_ALT_DESCRIPTION",
                "<color=#9b5f41>Tierra</color>. Wooper lanza una bola de lodo al objetivo, lo " +
                "<style=cIsUtility>ralentiza</style> e inflige <style=cIsDamage>170% de daño</style>."
            );

            RegisterSpanishToken("WOOPER__M1_ALT2", "Acua Cola");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__M1_ALT2_DESCRIPTION",
                "<color=#55b8e2>Agua</color>. Wooper ataca con un coletazo e inflige " +
                "<style=cIsDamage>170% de daño</style>. Cada 2.º golpe inflige " +
                "<style=cIsDamage>200% de daño</style>. El ataque acumula <style=cIsUtility>Empapado</style> " +
                "hasta <style=cIsUtility>3</style> veces y se repite al golpear objetivos que tengan menos de " +
                "<style=cIsUtility>3</style> acumulaciones."
            );

            // Secundarias
            RegisterSpanishToken("WOOPER__M2", "Agua Lodosa");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__M2_DESCRIPTION",
                "<color=#55b8e2>Agua</color>. Wooper dispara agua lodosa a los objetivos, los " +
                "<style=cIsUtility>ralentiza</style> e inflige <style=cIsDamage>260% de daño</style>."
            );

            RegisterSpanishToken("WOOPER__M2_ALT", "Tierra Viva");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__M2_ALT_DESCRIPTION",
                "<color=#9b5f41>Tierra</color>. <style=cIsDamage>Aturde</style>. Wooper hace que el suelo " +
                "estalle con fuerza e inflige <style=cIsDamage>560% de daño</style>."
            );

            RegisterSpanishToken("WOOPER__M2_ALT2", "Torbellino");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__M2_ALT2_DESCRIPTION",
                "<color=#55b8e2>Agua</color>. Wooper atrapa al objetivo dentro de un violento torbellino que " +
                "inflige <style=cIsDamage>140% de daño</style> por segundo durante un máximo de " +
                "<style=cIsUtility>6 s</style>."
            );

            // Utilidades
            RegisterSpanishToken("WOOPER__UTIL", "Excavar");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__UTIL_DESCRIPTION",
                "<color=#9b5f41>Tierra</color>. Wooper se entierra durante un breve periodo, obtiene " +
                "<style=cIsHealth>invulnerabilidad</style> y luego emerge infligiendo " +
                "<style=cIsDamage>280% de daño</style>."
            );

            RegisterSpanishToken("WOOPER__UTIL_ALT", "Protección");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__UTIL_ALT_DESCRIPTION",
                "Wooper adopta una postura defensiva para <style=cIsDamage>bloquear</style> todo el daño " +
                "recibido. Puede mantener esta postura durante un máximo de <style=cIsUtility>10 s</style>."
            );

            RegisterSpanishToken("WOOPER__UTIL_ALT2", "Cascada");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__UTIL_ALT2_DESCRIPTION",
                "<color=#55b8e2>Agua</color>. Wooper asciende en un torrente de agua e inflige " +
                "<style=cIsDamage>270% de daño</style>; luego cae con fuerza e inflige " +
                "<style=cIsDamage>300% de daño</style> al impactar."
            );

            // Especiales
            RegisterSpanishToken("WOOPER__SPEC", "Surf");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__SPEC_DESCRIPTION",
                "<color=#55b8e2>Agua</color>. Wooper inunda sus alrededores con una ola gigante que ataca a " +
                "todo lo cercano e inflige <style=cIsDamage>700% de daño</style>. Deja un charco que inflige " +
                "<style=cIsDamage>70% de daño</style> por segundo durante un máximo de " +
                "<style=cIsUtility>6 s</style>."
            );

            RegisterSpanishToken("WOOPER__SPEC_ALT", "Terremoto");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__SPEC_ALT_DESCRIPTION",
                "<color=#9b5f41>Tierra</color>. Wooper provoca un terremoto que golpea una zona amplia e " +
                "inflige <style=cIsDamage>1200% de daño</style>."
            );

            RegisterSpanishToken("WOOPER__SPEC_ALT2", "Reserva / Tragar / Escupir");
            RegisterSpanishSkillDescriptionToken(
                "WOOPER__SPEC_ALT2_DESCRIPTION",
                "Wooper ejecuta una habilidad según la entrada utilizada." +
                Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName>【Pulsar】Reserva</style><style=cSub> Wooper almacena poder y aumenta su " +
                "<style=cIsDamage>armadura</style> en <style=cIsDamage>5</style>. Este movimiento puede usarse " +
                "hasta <style=cIsUtility>3</style> veces." +
                Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName>【Mantener】Tragar</style><style=cSub> El usuario absorbe el poder almacenado " +
                "con Reserva para <style=cIsHealing>curarse</style> un " +
                "<style=cIsHealing>25%-100%</style>." +
                Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName>【Pulsación doble】Escupir</style><style=cSub> Todo el poder almacenado con Reserva " +
                "se libera de una vez en un ataque que inflige <style=cIsDamage>450%-1350%</style> de daño."
            );

            // Skins
            RegisterSpanishToken("WOOPERBODY_DEFAULT_SKIN_NAME", "Predeterminado");
            RegisterSpanishToken("WOOPERBODY_SKIN01_NAME", "Variocolor");
            RegisterSpanishToken("WOOPERBODY_SKIN02_NAME", "Paldeano");
            RegisterSpanishToken("WOOPERBODY_SKIN03_NAME", "Paldeano variocolor");
        }

        private static void RegisterWooperPassiveSpanish(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (!TryGetPassiveLocalizationTokens(
                    survivor,
                    out string nameToken,
                    out string descriptionToken
                ))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(nameToken))
            {
                RegisterSpanishToken(nameToken, "Absorbe Agua");
            }

            if (!string.IsNullOrWhiteSpace(descriptionToken))
            {
                RegisterSpanishSkillDescriptionToken(
                    descriptionToken,
                    "Las habilidades de <color=#55b8e2>Agua</color> aplican <style=cIsUtility>Empapado</style> " +
                    "al golpear durante <style=cIsUtility>4 s</style>. Las habilidades de <color=#9b5f41>Tierra</color> " +
                    "consumen <style=cIsUtility>Empapado</style> al golpear, <style=cIsUtility>ralentizan</style>, infligen " +
                    "<style=cIsDamage>120% de daño</style> y <style=cIsHealth>curan</style> a Wooper un " +
                    "<style=cIsHealth>2% de salud</style>."
                );
            }

            logger?.LogInfo(
                "[LOCALIZATION] Pasiva localizada | WooperBody | " +
                "NameToken: " + (nameToken ?? "") +
                " | DescriptionToken: " + (descriptionToken ?? "")
            );
        }

        private static void RegisterSpanishSkillDescriptionToken(
            string token,
            string spanish
        )
        {
            RegisterSpanishToken(token, FormatLongSkillText(spanish));
        }

        private static void RegisterSpanishToken(
            string token,
            string spanish
        )
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(spanish))
            {
                return;
            }

            LanguageAPI.Add(token, spanish, "es-419");
            LanguageAPI.Add(token, spanish, "es-ES");
        }

        private static bool IsSpanishLanguage()
        {
            string language = Language.currentLanguageName ?? "";
            return language.StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnglishLanguage()
        {
            string language = Language.currentLanguageName ?? "";
            return language.StartsWith("en", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBrokenDescriptionText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            string withoutTags = "";
            bool insideTag = false;

            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                if (character == '<')
                {
                    insideTag = true;
                    continue;
                }

                if (insideTag)
                {
                    if (character == '>')
                    {
                        insideTag = false;
                    }
                    continue;
                }

                withoutTags += character;
            }

            string useful = withoutTags.Replace("!", "").Trim();
            return string.IsNullOrWhiteSpace(useful);
        }

        private static bool IsMissingLoreText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            string trimmed = text.Trim();
            if (
                trimmed.IndexOf(' ') < 0 &&
                trimmed.IndexOf('_') >= 0 &&
                string.Equals(
                    trimmed,
                    trimmed.ToUpperInvariant(),
                    StringComparison.Ordinal
                )
            )
            {
                return true;
            }

            return false;
        }
    }
}
