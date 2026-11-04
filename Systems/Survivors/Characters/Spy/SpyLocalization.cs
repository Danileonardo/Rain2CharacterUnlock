using System;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Datos de descripción y localización propios de Spy.
    /// Extraídos sin alterar contenido ni el orden de sus textos.
    /// </summary>
    internal static class SpyLocalization
    {
        internal const string SpyAgileKeywordEnglish =
            "<style=cKeywordName>Agile</style><style=cSub>The skill can be used while sprinting.</style>";

        internal const string SpyEspionageKeywordEnglish =
            "<style=cKeywordName>Espionage</style><style=cSub>Your next shot has <style=cIsDamage>100% Crit Chance</style>. " +
            "Rolling a <style=cIsDamage>Critical Strike</style> with <color=#62746f>Espionage</color> stacks increases " +
            "<style=cIsDamage>damage by 2x</style> instead. <style=cIsHealth>Backstabs</style> will always grant " +
            "<color=#62746f>Espionage</color> stacks on champion enemies.</style>";

        internal const string SpyOpportunistKeywordEnglish =
            "<style=cKeywordName>Opportunist</style><style=cSub>Your <style=cIsHealth>HP</style> and " +
            "<style=cIsHealing>health regneration</style> are <style=cDeath>permanently reduced by 25%</style>. " +
            "<style=cIsHealth>Backstabs</style> that don't kill champion enemies grant " +
            "<style=cIsUtility>movement speed</style> and <style=cIsHealing>barrier</style>. </style>";

        internal const string SpyBackstabKeywordEnglish =
            "<style=cKeywordName>Backstab</style><style=cSub>Deals <style=cIsDamage>2x damage</style> against champion enemies " +
            "and <style=cIsHealth>30% HP</style> or more to elites.</style>";

        internal const string SpyAgileKeywordSpanish =
            "<style=cKeywordName>Ágil</style><style=cSub>La habilidad puede usarse mientras corres.</style>";

        internal const string SpyEspionageKeywordSpanish =
            "<style=cKeywordName>Espionaje</style><style=cSub>Tu siguiente disparo tiene un " +
            "<style=cIsDamage>100% de probabilidad de crítico</style>. Obtener un " +
            "<style=cIsDamage>golpe crítico</style> con acumulaciones de <color=#62746f>Espionaje</color> " +
            "aumenta el <style=cIsDamage>daño a 2x</style> en su lugar. Las " +
            "<style=cIsHealth>puñaladas por la espalda</style> siempre otorgan acumulaciones de " +
            "<color=#62746f>Espionaje</color> contra enemigos campeones.</style>";

        internal const string SpyOpportunistKeywordSpanish =
            "<style=cKeywordName>Oportunista</style><style=cSub>Tu <style=cIsHealth>salud</style> y tu " +
            "<style=cIsHealing>regeneración de salud</style> se reducen " +
            "<style=cDeath>permanentemente un 25%</style>. Las <style=cIsHealth>puñaladas por la espalda</style> " +
            "que no maten a enemigos campeones otorgan <style=cIsUtility>velocidad de movimiento</style> y " +
            "<style=cIsHealing>barrera</style>.</style>";

        internal const string SpyBackstabKeywordSpanish =
            "<style=cKeywordName>Puñalada por la espalda</style><style=cSub>Inflige " +
            "<style=cIsDamage>2x de daño</style> contra enemigos campeones y al menos un " +
            "<style=cIsHealth>30% de la salud</style> a los élites.</style>";

        internal static readonly string SpySpanishDescription =
            "Spy es un asesino cuerpo a cuerpo frágil que destaca al eliminar con facilidad objetivos de alta prioridad." +
            "<color=#CCD3E0>\r\n\r\n" +
            "< ! > Acumula Espionaje con el cuchillo de Spy y desata golpes críticos con Diamondback.\r\n\r\n" +
            "< ! > Puñalada es una habilidad devastadora capaz de matar al instante a los enemigos más débiles.\r\n\r\n" +
            "< ! > Sabotaje puede ser una gran herramienta para colocarte a alcance de una Puñalada por la espalda mientras aturdes a los enemigos cercanos.\r\n\r\n" +
            "< ! > Camuflaje sirve tanto para iniciar un enfrentamiento como para escapar, pero exige administrar correctamente el recurso.\r\n\r\n";

    }
}
