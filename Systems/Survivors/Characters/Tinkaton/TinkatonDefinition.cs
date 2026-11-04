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
    /// ES/EN y adaptación narrativa USU del lore del creador.
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

        private static readonly string TinkatonSpanishDescription = TinkatonLocalization.TinkatonSpanishDescription;

        private static readonly string TinkatonEnglishDescription = TinkatonLocalization.TinkatonEnglishDescription;

        // Diario curado USU: registro personal del Ingeniero en primera persona.
        // Seis párrafos EN/ES-419: aprendizaje de Tinkatink, rebeldía de
        // Tinkatuff, misión de recuperación en Canto de la Sirena, dos ojos
        // mecánicos, evolución en combate y reparación conjunta del TC-280.
        // El contenido de la misión de desbloqueo permanece intacto.
        // El token estático jamás contiene el diario completo: se resuelve de forma dinámica en la ficha.
        private static readonly string TinkatonEnglishLore = "[USU] Diary record unavailable from static localization.";

        private static readonly string TinkatonSpanishLore = "[USU] Diario disponible desde la biblioteca de supervivientes.";

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

            UsuLog.Verbose(logger, 
                "[LOCALIZATION] Definition activa | TinkatonBody | " +
                "es-419 / es-ES / en + vista general USU + 3 pasivas + skills + skins + Maestría + lore USU bilingüe"
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
            if (IsSpanishLanguage()) return USUTinkatonEncounterGate.ResolveTinkatonLore(true);
            if (IsEnglishLanguage()) return USUTinkatonEncounterGate.ResolveTinkatonLore(false);
            return fallback ?? "";
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
            LanguageAPI.Add("TINKATON_LORE", TinkatonEnglishLore, "en");

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
