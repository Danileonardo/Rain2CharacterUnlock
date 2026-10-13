using System;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// AH-64 by JohnstonStu. Native log audit + published author documentation.
    /// No external assembly coupling, no changes to equipment/game mechanics.
    /// </summary>
    public sealed class AH64Definition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.JohnstonStu.AH64"; } }
        public override string BodyName { get { return "AH64Body"; } }
        public override string DefinitionName { get { return "AH-64"; } }
        private const string SpanishDescription = "El AH-64 es una aeronave de ataque que nunca aterriza: sacrifica movilidad terrestre a cambio de altura y armamento completo.<color=#CCD3E0>\n\n< ! > El cañón M230 lleva un tambor fijo y recarga toda la munición a la vez. Inflige más daño a más de 30 m y solo un 75% a menos de 10 m. Dispara ráfagas cortas a distancia; recarga antes de comprometerte.\n\n< ! > Las cápsulas Hydra disparan una salva escalonada: mantén la puntería durante los cohetes. Infligen un 75% del daño antes de recorrer 8 m y el daño completo a partir de 25 m. Pueden dispararse mientras recargas el cañón.\n\n< ! > El tonel evasivo asciende en diagonal hacia delante, izquierda o derecha. Concede invulnerabilidad durante la primera mitad y armadura adicional durante la maniobra. Mantén salto para ganar altura y descender para perderla; suelta ambos para mantenerla. El tiempo por encima de la altura de reposo es limitado.\n\n< ! > El radar de control de tiro marca la amenaza más importante. Mantén Longbow para fijar objetivos mientras sigues usando el cañón y los Hydra; suelta para lanzar los misiles. Hellfire es una variante del equipamiento.\n\n";
        private const string SpanishLore = "<style=cSub>Registro recuperado de la aeronave. Fragmento incompleto. Marcas temporales inconsistentes.</style>\n\nCICLO 0041 — Misión de escolta. Pasada a baja altura sobre la línea de restos. Sin contactos. Bancada del rotor en condiciones nominales.\n\nCICLO 0042 — Contacto. Contacto. Ninguna entrada de la base de datos de amenazas coincide. Agoté el tambor, recargué y volví a agotarlo. El radar seguía detectando nuevas señales más deprisa de lo que el lanzador podía eliminarlas.\n\nCICLO 0058 — Solicité relevo. Ninguna respuesta en ninguna frecuencia. Solicité permiso para aterrizar. Ninguna respuesta.\n\nCICLO 0061 — Modificación de campaña: patines de aterrizaje retirados. Noventa kilos menos y, de todos modos, ya no queda ningún lugar donde aterrizar. La autonomía ha dejado de ser una cuestión de combustible.\n\nCICLO 0103 — El cañón sigue funcionando. Las lanzaderas siguen funcionando. El radar todavía encuentra el objetivo más grande del campo y lo mantiene fijado. Fuera cual fuera la misión, ha sido sustituida por la única parte que todavía funciona.\n\n<style=cSub>El registro continúa. El número de entradas supera en nueve veces la vida útil certificada de la aeronave.</style>";

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            // Only creator-specific language keys: never overwrite DEFAULT_SKIN.
            RegisterSpanishToken("AH64_PASSIVE_NAME", "Radar de control de tiro");
            RegisterSpanishToken("AH64_PASSIVE_DESCRIPTION", "El radar superior marca al <style=cIsUtility>enemigo más fuerte cercano</style>. Infliges un <style=cIsUtility>12% más de daño</style> al objetivo marcado, obtienes un <style=cIsUtility>15% de velocidad de movimiento</style> mientras lo encaras y <style=cIsUtility>30 de armadura</style> cuando está cerca.");
            RegisterSpanishToken("AH64_PRIMARY_CHAINGUN_NAME", "Cañón automático M230");
            RegisterSpanishToken("AH64_PRIMARY_CHAINGUN_DESCRIPTION", FormatLongSkillText("<style=cIsUtility>Ágil.</style> Dispara la torreta frontal: <style=cIsDamage>52% de daño</style> por proyectil y una pequeña explosión de <style=cIsDamage>26%</style>. Ambos infligen un <style=cIsUtility>75%</style> a menos de <style=cIsUtility>10 m</style> y el daño completo a <style=cIsUtility>30 m</style>. Tambor de <style=cIsUtility>20 proyectiles</style> y recarga de <style=cIsUtility>1,7 s</style>. Pierde precisión al mantener el disparo."));
            RegisterSpanishToken("AH64_PRIMARY_GATLING_NAME", "Cañón rotativo XM301");
            RegisterSpanishToken("AH64_PRIMARY_GATLING_DESCRIPTION", FormatLongSkillText("<style=cIsUtility>Ágil.</style> Acelera un cañón de seis tubos para disparar proyectiles de <style=cIsDamage>34% de daño</style> con explosiones de <style=cIsDamage>13,5%</style>. Inflige un <style=cIsUtility>75%</style> a menos de <style=cIsUtility>10 m</style> y el daño completo a <style=cIsUtility>30 m</style>. <style=cIsUtility>La cadencia aumenta al girar los tubos.</style> Tambor de <style=cIsUtility>60 disparos</style>; recarga de <style=cIsUtility>2,4 s</style>. Menor precisión que el M230."));
            RegisterSpanishToken("AH64_PRIMARY_CANNON_NAME", "Cañón pesado M789");
            RegisterSpanishToken("AH64_PRIMARY_CANNON_DESCRIPTION", FormatLongSkillText("<style=cIsUtility>Ágil.</style> Dispara proyectiles pesados y lentos que infligen <style=cIsDamage>290% de daño</style>, con una <style=cIsUtility>gran</style> explosión de <style=cIsDamage>100%</style>. Tambor de <style=cIsUtility>8 proyectiles</style>; recarga de <style=cIsUtility>2,8 s</style>. Muy preciso: cada disparo cuenta."));
            RegisterSpanishToken("AH64_SECONDARY_ROCKETPODS_NAME", "Cápsulas Hydra-70");
            RegisterSpanishToken("AH64_SECONDARY_ROCKETPODS_DESCRIPTION", FormatLongSkillText("<style=cIsUtility>Ágil.</style> Dispara una salva de cohetes no guiados desde los soportes laterales, cada uno con <style=cIsDamage>180% de daño</style>. Infligen un <style=cIsUtility>75%</style> antes de recorrer <style=cIsUtility>8 m</style> y el daño completo desde <style=cIsUtility>25 m</style>. Capacidad de <style=cIsUtility>6 cohetes</style>; recarga de <style=cIsUtility>5,5 s</style>. Cada <style=cIsUtility>Cargador de reserva</style> añade 1 cohete y <style=cIsUtility>0,8 s</style> de recarga."));
            RegisterSpanishToken("AH64_UTILITY_DASH_NAME", "Tonel evasivo");
            RegisterSpanishToken("AH64_UTILITY_DASH_DESCRIPTION", FormatLongSkillText("Realiza un tonel <style=cIsUtility>hacia delante, la izquierda o la derecha</style> ascendiendo en diagonal según tu movimiento. Refuerza el blindaje con <style=cIsUtility>200 de armadura</style> durante <style=cIsUtility>0,95 s</style>. <style=cIsUtility>Eres invulnerable durante la primera mitad.</style> Mantén salto después para ganar altura: el tonel solo te eleva ligeramente."));
            RegisterSpanishToken("AH64_UTILITY_BACKFLIP_NAME", "Voltereta con humo");
            RegisterSpanishToken("AH64_UTILITY_BACKFLIP_DESCRIPTION", FormatLongSkillText("Realiza una <style=cIsUtility>voltereta hacia atrás</style> en ascenso, retrocede y libera una nube de humo. Obtienes <style=cIsUtility>camuflaje</style> brevemente antes de volver al combate."));
            RegisterSpanishToken("AH64_UTILITY_BRAKING_TURN_NAME", "Viraje inclinado");
            RegisterSpanishToken("AH64_UTILITY_BRAKING_TURN_DESCRIPTION", FormatLongSkillText("Inclínate en un amplio giro de 90 grados sin perder vuelo. Elige izquierda o derecha con el movimiento; sin introducir una dirección, gira a la derecha. Puedes apuntar y disparar durante todo el viraje."));
            RegisterSpanishToken("AH64_SPECIAL_LONGBOW_NAME", "AGM-114L Longbow");
            RegisterSpanishToken("AH64_SPECIAL_LONGBOW_DESCRIPTION", FormatLongSkillText("<style=cIsUtility>Mantén pulsado</style> para fijar objetivos bajo la retícula y <style=cIsUtility>suelta para lanzar</style> hasta <style=cIsUtility>6 misiles guiados</style> desde soportes alternados. Puedes usar la principal y la secundaria mientras fijas objetivos. Los últimos objetivos reciben más daño (<style=cIsDamage>400%</style>–<style=cIsDamage>675%</style>) y los misiles adicionales mantienen el máximo. Cada <style=cIsUtility>Célula de lisato</style> añade un misil."));
            RegisterSpanishToken("AH64_SPECIAL_HELLFIRE_NAME", "AGM-114 Hellfire");
            RegisterSpanishToken("AH64_SPECIAL_HELLFIRE_DESCRIPTION", FormatLongSkillText("Pulsa para lanzar un misil lento desde un soporte lateral. Mantén la especial para mostrar el láser y acelerar el misil activo hacia la mira; suelta para frenarlo y vuelve a mantener para continuar sin consumir otra carga. Puedes lanzar uno nuevo cuando termine el anterior. La principal y secundaria siguen disponibles. Explota al tocar un objetivo por <style=cIsDamage>1350% de daño</style>."));
            RegisterSpanishToken("AH64_SPECIAL_BOMBING_NAME", "Bombardeo de pasada");
            RegisterSpanishToken("AH64_SPECIAL_BOMBING_DESCRIPTION", FormatLongSkillText("Suelta 6 bombas a lo largo de tu trayectoria, cada una con <style=cIsDamage>300% de daño</style>. Cada enemigo puede recibir como máximo 3 impactos de bombas por pasada. Se pierden las bombas restantes si interrumpes la maniobra."));
            RegisterSpanishToken("AH64_DESERT_SKIN_NAME", "Arena desértica");
            RegisterSpanishToken("AH64_ARCTIC_SKIN_NAME", "Ártico");
            RegisterSpanishToken("AH64_ARMY_SKIN_NAME", "Verde militar");
            RegisterSpanishToken("AH64_NIGHT_SKIN_NAME", "Acechador nocturno");
            RegisterSpanishToken("ACHIEVEMENT_AH64_MASTERYACHIEVEMENT_NAME", "AH-64: Maestría");
            RegisterSpanishToken("ACHIEVEMENT_AH64_MASTERYACHIEVEMENT_DESCRIPTION", "Como AH-64, vence la partida o sacrifícate en el Obelisco en dificultad Monzón.");
            RegisterSpanishToken("AH64_SUBTITLE", "La aeronave de ataque");
            RegisterSpanishToken("AH64_DESCRIPTION", "El AH-64 es una aeronave de ataque que nunca aterriza: sacrifica movilidad terrestre a cambio de altura y armamento completo.<color=#CCD3E0>\n\n< ! > El cañón M230 lleva un tambor fijo y recarga toda la munición a la vez. Inflige más daño a más de 30 m y solo un 75% a menos de 10 m. Dispara ráfagas cortas a distancia; recarga antes de comprometerte.\n\n< ! > Las cápsulas Hydra disparan una salva escalonada: mantén la puntería durante los cohetes. Infligen un 75% del daño antes de recorrer 8 m y el daño completo a partir de 25 m. Pueden dispararse mientras recargas el cañón.\n\n< ! > El tonel evasivo asciende en diagonal hacia delante, izquierda o derecha. Concede invulnerabilidad durante la primera mitad y armadura adicional durante la maniobra. Mantén salto para ganar altura y descender para perderla; suelta ambos para mantenerla. El tiempo por encima de la altura de reposo es limitado.\n\n< ! > El radar de control de tiro marca la amenaza más importante. Mantén Longbow para fijar objetivos mientras sigues usando el cañón y los Hydra; suelta para lanzar los misiles. Hellfire es una variante del equipamiento.\n\n");
            RegisterSpanishToken("AH64_LORE", "<style=cSub>Registro recuperado de la aeronave. Fragmento incompleto. Marcas temporales inconsistentes.</style>\n\nCICLO 0041 — Misión de escolta. Pasada a baja altura sobre la línea de restos. Sin contactos. Bancada del rotor en condiciones nominales.\n\nCICLO 0042 — Contacto. Contacto. Ninguna entrada de la base de datos de amenazas coincide. Agoté el tambor, recargué y volví a agotarlo. El radar seguía detectando nuevas señales más deprisa de lo que el lanzador podía eliminarlas.\n\nCICLO 0058 — Solicité relevo. Ninguna respuesta en ninguna frecuencia. Solicité permiso para aterrizar. Ninguna respuesta.\n\nCICLO 0061 — Modificación de campaña: patines de aterrizaje retirados. Noventa kilos menos y, de todos modos, ya no queda ningún lugar donde aterrizar. La autonomía ha dejado de ser una cuestión de combustible.\n\nCICLO 0103 — El cañón sigue funcionando. Las lanzaderas siguen funcionando. El radar todavía encuentra el objetivo más grande del campo y lo mantiene fijado. Fuera cual fuera la misión, ha sido sustituida por la única parte que todavía funciona.\n\n<style=cSub>El registro continúa. El número de entradas supera en nueve veces la vida útil certificada de la aeronave.</style>");
            logger?.LogInfo("[LOCALIZATION] Definition activa | AH64Body | 11 skills, 4 skins, logro de Maestría, Vista general y Notas ES");
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "La aeronave de ataque" : fallback ?? "";
        }
        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishDescription : fallback ?? "";
        }
        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishLore : fallback ?? "";
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static void RegisterSpanishToken(string token, string value)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return;
            LanguageAPI.Add(token, value, "es-419");
            LanguageAPI.Add(token, value, "es-ES");
        }
    }
}
