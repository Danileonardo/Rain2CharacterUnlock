using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Tinkaton 1.1.0.
    ///
    /// Autoridad USU para localización visible ES, sus tres pasivas,
    /// skills, skins, achievement de Maestría, Vista general reparada
    /// ES/EN y traducción del lore nativo del creador.
    ///
    /// La misión oficial USU "Forjada en Chatarra" permanece en el
    /// sistema legacy de misiones y no se migra en esta fase.
    ///
    /// No referencia Tinkaton.dll. Sólo usa Body y tokens públicos.
    /// </summary>
    public sealed class TinkatonDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get { return "com.Dragonyck.Tinkaton"; }
        }

        public override string BodyName
        {
            get { return "TinkatonBody"; }
        }

        public override string DefinitionName
        {
            get { return "Tinkaton"; }
        }

        private static object TinkatonEnglishDescriptionOverlay;

        private static readonly string TinkatonSpanishDescription =
            "Tinkaton es una combatiente versátil que combina golpes pesados de martillo, ataques a distancia " +
            "y apoyo para todo el equipo.<style=cSub>\r\n\r\n" +
            "< ! > Sus tres pasivas cambian su enfoque: Hurto puede generar chatarra al matar, Rompemoldes " +
            "hace que sus ataques ignoren la armadura y Ritmo Propio reduce a la mitad la duración de las " +
            "debilitaciones mientras prolonga los objetos temporales.<style=cSub>\r\n\r\n" +
            "< ! > Antiaéreo castiga especialmente a los enemigos en el aire, mientras Foco Resplandor reduce " +
            "armadura y Metaláser convierte salud en una descarga de múltiples impactos.<style=cSub>\r\n\r\n" +
            "< ! > Protección, Danza Espada, Reflejo y Otra Vez permiten alternar entre defensa, daño de equipo " +
            "y control de tiempos de recarga.<style=cSub>\r\n\r\n" +
            "< ! > Martillo Colosal requiere al menos 3 cargas, consume todas al usarlo e inflige daño adicional " +
            "por cada carga extra.";

        private static readonly string TinkatonEnglishDescription =
            "Tinkaton is a versatile fighter that combines heavy hammer blows, ranged attacks, and support for " +
            "the whole team.<style=cSub>\r\n\r\n" +
            "< ! > Her three passives change her approach: Pickpocket can generate scrap on kills, Mold Breaker " +
            "makes her attacks ignore armor, and Own Tempo halves debuff duration while extending temporary " +
            "item duration.<style=cSub>\r\n\r\n" +
            "< ! > Smack Down especially punishes airborne enemies, while Flash Cannon reduces armor and Steel " +
            "Beam converts health into a multi-hit blast.<style=cSub>\r\n\r\n" +
            "< ! > Protect, Swords Dance, Reflect, and Encore let her switch between defense, team damage, and " +
            "cooldown control.<style=cSub>\r\n\r\n" +
            "< ! > Gigaton Hammer requires at least 3 stocks, consumes all stocks on use, and gains additional " +
            "damage for every extra stock.";

        // Traducción fiel del lore nativo publicado por el creador.
        // El texto inglés original se conserva sin overlay ni reparación.
        private static readonly string TinkatonSpanishLore =
            "Nadie sabe de dónde vino. Lo único que sabemos es que es adorable, es rosa y blande un martillo " +
            "gigantesco. También sabemos que nos causa bastantes problemas, porque a veces vemos piezas " +
            "desaparecidas de la nave incorporadas a su martillo. Si intentamos recuperarlas, o alguien termina " +
            "en la enfermería después de recibir un golpe, o desaparecen todavía más partes de la nave. El " +
            "departamento de investigación informó de que faltaban algunos especímenes. Hace poco vimos una " +
            "pieza de un artilugio de latón unida al martillo, y eso nos dio una idea.\n\n" +
            "Parece entender el lenguaje humano, aunque es discutible que esté dispuesta a escuchar. Vamos a " +
            "enviarla al planeta y esperar lo mejor. Le dijeron que allí abajo podía encontrar más metal. Sus " +
            "ojos se iluminaron de alegría al oírlo.\n\n" +
            "Lo que no sabíamos era la violencia que desataría sobre sus habitantes.";

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
            RegisterTinkatonSpanishTokens();

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | TinkatonBody | " +
                "es-419 / es-ES + vista general USU + 3 pasivas + skills + skins + Maestría + lore nativo"
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Saqueadora"
                : fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return TinkatonSpanishDescription;
            }

            if (IsEnglishLanguage() && IsBrokenDescriptionText(fallback))
            {
                return TinkatonEnglishDescription;
            }

            return fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            // Tinkaton sí publica lore nativo en inglés. USU sólo proporciona
            // su traducción española y nunca reemplaza el original inglés.
            return IsSpanishLanguage()
                ? TinkatonSpanishLore
                : fallback ?? "";
        }

        private static void RegisterEnglishDescriptionOverlay()
        {
            if (TinkatonEnglishDescriptionOverlay != null)
            {
                return;
            }

            // Tinkaton 1.1.0 publica TINKATON_DESCRIPTION, pero su contenido
            // visible son únicamente saltos y marcadores < ! >. USU repara
            // sólo esa Vista general inglesa faltante.
            TinkatonEnglishDescriptionOverlay =
                LanguageAPI.AddOverlay(
                    "TINKATON_DESCRIPTION",
                    TinkatonEnglishDescription,
                    "en"
                );
        }

        private static void RegisterTinkatonSpanishTokens()
        {
            RegisterSpanishToken("TINKATON_SUBTITLE", "Saqueadora");
            RegisterSpanishToken("TINKATON_DESCRIPTION", TinkatonSpanishDescription);
            RegisterSpanishToken("TINKATON_LORE", TinkatonSpanishLore);

            // Primaria
            RegisterSpanishToken("TINKATON_M1", "Giro Vil");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_M1_DESCRIPTION",
                "Amplio barrido con el martillo que inflige <style=cIsDamage>345% de daño</style>."
            );

            // Secundarias
            RegisterSpanishToken("TINKATON_M2", "Antiaéreo");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_M2_DESCRIPTION",
                "Lanza una roca que inflige <style=cIsDamage>150% de daño</style>. Los objetivos " +
                "<style=cIsUtility>en el aire</style> reciben <style=cIsDamage>el doble</style> de daño y sufren " +
                "una debilitación que los hace caer al suelo y les impide volar durante " +
                "<style=cIsUtility>3 s</style>."
            );

            RegisterSpanishToken("TINKATON_M2-2", "Foco Resplandor");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_M2-2_DESCRIPTION",
                "Proyectil <style=cIsDamage>perforante</style> que inflige <style=cIsDamage>240% de daño</style>. " +
                "Aplica una debilitación que <style=cIsDamage>reduce la armadura</style> del objetivo en " +
                "<style=cIsDamage>35</style> durante <style=cIsUtility>4 s</style>."
            );

            RegisterSpanishToken("TINKATON_M2-3", "Metaláser");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_M2-3_DESCRIPTION",
                "Carga un láser que inflige <style=cIsDamage>500% de daño</style>. Cuanto más lo cargues, más " +
                "impactos realizará, hasta un máximo de 10. Cargar esta habilidad sacrifica " +
                "<style=cIsHealth>6% de salud</style> por cada carga de impacto. No puede reducirte por debajo " +
                "de <style=cIsHealth>10%</style> de salud."
            );

            // Utilidades
            RegisterSpanishToken("TINKATON_UTIL", "Protección");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_UTIL_DESCRIPTION",
                "Tinkaton adopta una postura defensiva para <style=cIsDamage>bloquear</style> todo el daño " +
                "recibido. Puede mantener esta postura hasta <style=cIsUtility>10 s</style>."
            );

            RegisterSpanishToken("TINKATON_UTIL-2", "Danza Espada");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_UTIL-2_DESCRIPTION",
                "Tinkaton duplica el daño que infligen ella y los aliados cercanos durante " +
                "<style=cIsUtility>10 s</style>."
            );

            RegisterSpanishToken("TINKATON_UTIL-3", "Reflejo");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_UTIL-3_DESCRIPTION",
                "Tinkaton otorga <style=cIsDamage>75 de armadura</style> a ella y a todos los aliados durante " +
                "<style=cIsUtility>15 s</style>."
            );

            RegisterSpanishToken("TINKATON_UTIL-4", "Otra Vez");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_UTIL-4_DESCRIPTION",
                "Selecciona a un aliado y <style=cIsUtility>restablece</style> los tiempos de recarga de todas " +
                "sus habilidades y de su equipo."
            );

            // Especial
            RegisterSpanishToken("TINKATON_SPEC", "Martillo Colosal");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_SPEC_DESCRIPTION",
                "Tinkaton salta por los aires y cae con su martillo sobre una zona seleccionada, infligiendo " +
                "<style=cIsDamage>1600% de daño</style>. Requiere al menos <style=cIsUtility>3</style> cargas " +
                "para activarse, consume todas las cargas al usarla e inflige <style=cIsDamage>500% de daño</style> " +
                "adicional por cada carga extra."
            );

            // Pasivas seleccionables
            RegisterSpanishToken("TINKATON_PASSIVE_NAME", "Hurto");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_PASSIVE_DESCRIPTION",
                "Al matar a un enemigo, Tinkaton tiene un <style=cIsDamage>1,5%</style> de probabilidad de obtener " +
                "chatarra y un <style=cIsDamage>10%</style> de probabilidad de obtener chatarra al matar jefes."
            );

            RegisterSpanishToken("TINKATON_PASSIVE2_NAME", "Rompemoldes");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_PASSIVE2_DESCRIPTION",
                "Los ataques de Tinkaton <style=cIsDamage>ignoran la armadura</style>."
            );

            RegisterSpanishToken("TINKATON_PASSIVE3_NAME", "Ritmo Propio");
            RegisterSpanishSkillDescriptionToken(
                "TINKATON_PASSIVE3_DESCRIPTION",
                "La duración de las <style=cIsDamage>debilitaciones</style> aplicadas a Tinkaton se reduce a la " +
                "mitad. La duración de los objetos temporales aumenta un <style=cIsUtility>50%</style>."
            );

            // Skins y Maestría original del creador
            RegisterSpanishToken("TINKATONBODY_DEFAULT_SKIN_NAME", "Predeterminado");
            RegisterSpanishToken("TINKATONBODY_MASTERY_SKIN_NAME", "Variocolor");
            RegisterSpanishToken("ACHIEVEMENT_TINKATON_MASTERY_NAME", "Tinkaton: Maestría");
            RegisterSpanishToken(
                "ACHIEVEMENT_TINKATON_MASTERY_DESCRIPTION",
                "Como Tinkaton, completa el juego u oblítérate en Monzón."
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
    }
}
