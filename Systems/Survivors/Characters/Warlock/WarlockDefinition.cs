using System;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Warlock - com.kenko.Warlock / WarlockBody.
    /// Localización curada exclusivamente de tokens verificados en la
    /// auditoría de la instalación del usuario (11/10/2026).
    /// Respeta la misión Original, su logro y el lore acreditado a Plnk.
    /// </summary>
    public sealed class WarlockDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.kenko.Warlock"; } }
        public override string BodyName { get { return "WarlockBody"; } }
        public override string DefinitionName { get { return "Warlock"; } }

        private static readonly string SpanishOverview = "Warlock es un superviviente pequeño pero poderoso que recupera y potencia sus habilidades con la sangre de sus enemigos.<color=#CCD3E0>\n\n< ! > Combina <color=#981e33>Maná carmesí</color> con <color=#b97f8a>Metamagia</color> para conseguir efectos todavía más poderosos.\n\n< ! > Oleada sobrenatural sirve para desplazarte por el escenario, pero recuerda que también es tu principal fuente de daño.\n\n< ! > Maleficio permite aumentar considerablemente el daño y los ataques en área.\n\n< ! > Puedes acumular Maná carmesí y Metamagia sin límite. Reservarlos para desatar una gran descarga de habilidades puede ser devastador.\n\n";
        // Traducción completa de las Notas ORIGINALES de Plnk, no un lore nuevo.
        private static readonly string SpanishLore = "<style=cStack>> El informe automatizado 5761726c6f636b ya está disponible en el registro del sitio 47617279204779676178.\n> Consulta el registro 506174726f6e para obtener información adicional durante la revisión.\n> Tipo de informe: transcripción generada automáticamente; perfiles de habla base: «Curly», «Succubus».\n> - Origen: 456c64726974636820546f6d65 (artefacto de material desconocido n.º 4791).\n> Prioridad: alta.\n> Contenido del informe:\n\n-- Inicio del fragmento marcado para revisión --</style>\n\n¡Argh! ¡No puedo creer que [el captor] me haga esto! ¡Sin equipo, sin refuerzos, solo yo, arrojado a [el plano blanco] completamente solo! ¿En qué está pensando [el captor]? ¿Es una misión suicida?\n\nTengo que librarme de esto. Quizá podría decir que estoy enfermo. «Perdón, [mi captor], he caído [enfermo]. No puedo cumplir la misión, lo siento».\n\nComo si eso fuera a funcionar. Tengo que hacer algo grande. Tal vez, si reúno suficientes almas, pueda liberarme de [el contrato]. O quizá... ¡quizá hasta podría acabar con [el captor]! ¡Sí! ¡Eso es! ¡Podría ser libre! Juro por mi ojo que jamás volveré a obedecer una orden de ese maldi—\n\n<style=cDeath>「HOLA, PEQUEÑÍN」.</style>\n\nY-yo... ¡Hola, [mi captor]! ¿Cómo estás en esta hermosa [velada]?\n\n<style=cDeath>「OH, MARAVILLOSAMENTE. AÚN MEJOR AHORA QUE VISITO A MI PEQUEÑA [MASCOTA] FAVORITA. ¿CÓMO VAN TUS PREPARATIVOS PARA LA MISIÓN?」</style>\n\nAh, sí, ¡la misión! Todo está listo para mi excursión a [el plano blanco]. Pero parece que me ha dado algo. Estoy tan enfermo que apenas puedo ponerme de pie. Tal vez podría pospo—\n\n<style=cDeath>「ESPLÉNDIDO. PARTIRÁS CON EL PRIMER DESTELLO DE LA ESTRELLA ROJA」.</style>\n\nP-por supuesto, [mi captor]... ¡Haré cuanto pueda para cumplir tu voluntad!\n\n<style=cDeath>「HARÁS MUCHO MÁS QUE ESO. SI [el plano blanco] NO RECLAMA TU CADÁVER, TENDRÁS ÉXITO... O SERÁS CASTIGADO. NO QUERRÁS UN CASTIGO, ¿VERDAD, [JUGUETE]?」</style>\n\n¡Por supuesto que no, [mi captor]! ¡Regresaré triunfante!\n\n<style=cDeath>「¡OHOHO! ¡POR SUPUESTO QUE SÍ, MI QUERIDA [MASCOTA]! ME HARÁS SENTIR ORGULLO, ¿NO ES ASÍ? DEBO RETIRARME; TENGO OTRAS [MASCOTAS] QUE ATENDER. ¡TA-TA!」</style>\n\n...Triunfante, de pie sobre tu cadáver reseco.\n\n<style=cStack>-- Fin de la grabación --\n\n-- Fin del fragmento marcado para revisión --\n\n> ERRORES DE TRADUCCIÓN: 7\n> 1> No se pudo traducir por completo [el captor].\n> 2> No se pudo traducir por completo [el plano blanco].\n> 3> No se pudo traducir por completo [mi captor].\n> 4> No se pudo traducir por completo [enfermo].\n> 5> No se pudo traducir por completo [el contrato].\n> 6> No se pudo traducir por completo [velada].\n> 7> No se pudo traducir por completo [mascota/juguete].\n\n> Consulta el informe 57687920617265207468657365207265706f72742049447320736f206c6f6e67 para obtener el fragmento completo de audio.\n==============================================================</style>";

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterSpanishToken("KENKO_WARLOCK_NAME", "Warlock");
            RegisterSpanishToken("KENKO_WARLOCK_SUBTITLE", "Peón maldito");
            RegisterSpanishToken("KENKO_WARLOCK_PRIMARY_SURGE_NAME", "Oleada sobrenatural");
            RegisterSpanishToken("KENKO_WARLOCK_SECONDARY_HEX_NAME", "Maleficio");
            RegisterSpanishToken("KENKO_WARLOCK_UTILITY_BLOOD_DASH_NAME", "Desplazamiento sangriento");
            RegisterSpanishToken("KENKO_WARLOCK_SPECIAL_RITUAL_NAME", "Ritual");
            RegisterSpanishToken("KENKO_WARLOCK_PASSIVE_NAME", "Maná carmesí");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_WARLOCK_UNLOCKACHIEVEMENT_NAME", "Su eminencia");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_WARLOCK_UNLOCKACHIEVEMENT_DESCRIPTION", "Derrota a un Señor de los diablillos dentro de los 5 segundos posteriores a su primer golpe dañino.");
            RegisterSpanishSkill("KENKO_WARLOCK_PRIMARY_SURGE_DESCRIPTION", "Dispara un rayo <style=cIsUtility>perforante</style> que inflige <style=cIsDamage>400% de daño</style>. <color=#981e33>\nMANÁ CARMESÍ: la siguiente activación debilita y repele a los enemigos.</color> <color=#b97f8a>\nMETAMAGIA: mantén pulsado para consumir acumulaciones y disparar 1 rayo adicional por cada una.</color>");
            RegisterSpanishSkill("KENKO_WARLOCK_SECONDARY_HEX_DESCRIPTION", "Maldice a un enemigo durante <style=cIsUtility>7 segundos</style>. Las habilidades le infligen <style=cIsDamage>50% de daño adicional</style> por acumulación. <color=#981e33>\nMANÁ CARMESÍ: el daño adicional se vuelve un ataque en área que inflige el 50% del daño del impacto por acumulación.</color><color=#b97f8a>\nMETAMAGIA: mantén pulsado para consumir acumulaciones: +1 maldición y 2 s de hemorragia al impactar por cada una.</color>");
            RegisterSpanishSkill("KENKO_WARLOCK_UTILITY_BLOOD_DASH_DESCRIPTION", "<style=cIsUtility>Desaparece</style> y <style=cIsUtility>teletranspórtate</style> una distancia corta.<color=#981e33>\nMANÁ CARMESÍ: la siguiente activación inflige 200% de daño con aturdimiento en ambos extremos.</color><color=#b97f8a>\nMETAMAGIA: mantén pulsado para consumir acumulaciones: mayor distancia y 200% de daño al llegar, +100% por cada acumulación.</color>");
            RegisterSpanishSkill("KENKO_WARLOCK_SPECIAL_RITUAL_DESCRIPTION", "Abre un menú para recuperar las cargas de una habilidad elegida y potenciarla. Al volver a activarlo, mantén pulsado <color=#981e33>Ritual</color> para obtener <color=#b97f8a>1 acumulación de Metamagia</color> a cambio de <color=#981e33>1 de Maná carmesí</color>.");
            RegisterSpanishSkill("KENKO_WARLOCK_PASSIVE_DESCRIPTION", "<color=#981e33>Warlock</color> puede potenciar o recuperar determinadas habilidades usando <color=#981e33>Maná carmesí</color>. Obtienes una acumulación de <color=#981e33>Maná carmesí</color> cada <style=cIsDamage>2</style> bajas.");
            RegisterSpanishToken("KENKO_WARLOCK_DESCRIPTION", SpanishOverview);
            // El Diario nativo obtiene el relato desde KENKO_WARLOCK_LORE.
            // ResolveLore() solo cubre las Notas mostradas dentro de USU.
            RegisterSpanishToken("KENKO_WARLOCK_LORE", SpanishLore);
            logger?.LogInfo("[LOCALIZATION] Definition activa | WarlockBody | " +
                "ES Vista general + pasiva + 4 habilidades + Original + Notas; EN y Original preservados");
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "Peón maldito" : fallback ?? "";
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishOverview : fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            // La visibilidad de las Notas la controla el Logbook nativo.
            return IsSpanish() ? SpanishLore : fallback ?? "";
        }

        public override string ResolveOriginalMissionName(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "Su eminencia" : fallback ?? "";
        }

        public override string ResolveOriginalMissionDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish()
                ? "Derrota a un Señor de los diablillos dentro de los 5 segundos posteriores a su primer golpe dañino."
                : fallback ?? "";
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "")
                .StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static void RegisterSpanishSkill(string token, string value)
        {
            RegisterSpanishToken(token, FormatLongSkillText(value));
        }

        private static void RegisterSpanishToken(string token, string value)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return;
            LanguageAPI.Add(token, value, "es-419");
            LanguageAPI.Add(token, value, "es-ES");
        }
    }
}
