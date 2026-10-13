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
        private const string SpanishDescription = "Seamstress es una superviviente móvil que convierte su salud en daño. Obtiene menos daño base por nivel, así que acumula objetos de salud para sacar partido a la conversión de salud en daño.<color=#CCD3E0>\n\n< ! > Las agujas aportan daño adicional y movilidad; úsalas con cuidado.\n\n< ! > Utiliza Cercenar con tantas agujas como sea posible. Puede aplicar hemorragia rápidamente durante Insaciable.\n\n< ! > Cúrate tanto como puedas durante Insaciable: la barrera obtenida puede marcar la diferencia entre vivir y morir.\n\n< ! > Brutalizar puede aumentar mucho el daño, pero vigila tu salud.\n\n< ! > Lanza y recupera tus tijeras simbióticas para obtener agujas fuera de Insaciable.\n\n";
        private const string SpanishLore = "Piel pálida y fatiga. Señales de una enfermedad que se extiende entre los hijos de la alta corte. Qué desafortunados son.\n\nUna bebé enfermiza, frágil y débil, llora mientras sus padres también lloran. Un destino trágico para estos nobles ya entrados en años: es su única heredera. Ante un futuro incierto, su <style=cIsHealth>corazón</style> aún late.\n\nLa enfermedad puede curarse con un corazón artificial, pero la operación es peligrosa para los más jóvenes. Solo unos pocos tienen edad suficiente para someterse a ella.\n\nUna adolescente postrada en cama, con la salud deteriorándose rápidamente, se aferra a la vida con todas sus fuerzas. Necesita sobrevivir una semana más. Pendiendo de un hilo, su <color=#292525>corazón</color> aún late.\n\nAunque se curen, la vida de los pacientes sigue siendo la misma. Con el mantenimiento frecuente y el riesgo de fallos, alejarse demasiado del hogar podría significar la muerte. Muchos adoptan pasatiempos tranquilos, como hornear o coser, pero no todos se conforman.\n\nUna costurera inquieta, sostenida por una vitalidad sintética, detesta los muros entre los que vive. No quiere saber nada de su nobleza. Todo es monótono. Cuando cae la oscuridad, se cuela como polizón en una nave civil. Llena de adrenalina, su <style=cIsUtility>corazón</style> aún late.\n\nA muchos de los que huyen se les muestra la realidad. La galaxia fuera de la alta corte no es el país de las maravillas sobre el que leían de niños. Se parece más a un cuento de hadas trágico.\n\nUna criatura corrompida, viviendo de tiempo prestado. ¿Cómo pudo salir todo tan mal? La nave que eligió fue engullida por una grieta creada por imps que deseaban escapar tanto como ella. De pronto, una oleada de sangre se mezcló con el cielo carmesí; humanos e imps por igual fueron seccionados limpiamente. Mientras su visión empezaba a desvanecerse, una figura saltó para alejarse y otra se acercó en silencio. Un susurro, un objetivo, una nueva vida. Su <color=#9B3737>corazón</color> aún late.";

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterSpanishToken("KENKO_SEAMSTRESS_DESCRIPTION", SpanishDescription);
            // Source audit reports LoreToken empty; ResolveLore supplies the native Logbook translation.
            // Future creator revisions may use this conventional token.
            RegisterSpanishToken("KENKO_SEAMSTRESS_LORE", SpanishLore);
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
            logger?.LogInfo("[LOCALIZATION] Definition activa | SeamstressBody | ES 7 habilidades + 2 pasivas + 4 skins + 2 logros de skins + lore original traducido");
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
            // Full translation of the author-provided original; keep original EN unchanged.
            return IsSpanish() ? SpanishLore : fallback ?? "";
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
