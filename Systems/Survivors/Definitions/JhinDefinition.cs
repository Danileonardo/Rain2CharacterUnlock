using System;
using System.Collections;
using System.Reflection;

using BepInEx.Logging;
using R2API;
using RoR2;
using RoR2.Skills;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Jhin 1.5.1.
    ///
    /// El mod creador publica correctamente su contenido inglés, pero no una
    /// localización española. USU aporta es-419 y es-ES para identidad visible,
    /// Vista general, pasiva, las cuatro habilidades, skins y lore nativo.
    ///
    /// Los nombres de habilidades y skins usan, cuando existe equivalente,
    /// la terminología regional oficial de Jhin en League of Legends.
    /// Las mecánicas y valores proceden del contenido real auditado del mod.
    ///
    /// La misión oficial USU "El Cuarto Acto" permanece en el sistema legacy
    /// con su requisito actual de 4.444 de daño crítico mortal a un jefe.
    /// No se migra ni modifica en esta fase.
    ///
    /// No referencia JhinMod.dll. Sólo usa Body y tokens públicos del mod.
    /// </summary>
    public sealed class JhinDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get { return "com.seroronin.JhinMod"; }
        }

        public override string BodyName
        {
            get { return "JhinBody"; }
        }

        public override string DefinitionName
        {
            get { return "Jhin"; }
        }

        private static readonly string JhinSpanishDescriptionEs419 =
            "Jhin es un superviviente de gran daño explosivo contra un solo objetivo, centrado en disparos " +
            "contundentes en lugar de una cadencia rápida.<color=#CCD3E0>\r\n\r\n" +
            "< ! > Jhin sólo puede obtener velocidad de ataque al subir de nivel. Cualquier otra fuente aumenta " +
            "en su lugar la eficacia de daño de sus demás habilidades.\r\n\r\n" +
            "< ! > Los golpes críticos de Jhin le otorgan velocidad de movimiento adicional. Usa el crítico " +
            "garantizado de su cuarto disparo para salir de situaciones peligrosas.\r\n\r\n" +
            "< ! > Brote Mortal puede interrumpir ataques que de otro modo no podrías evitar; resulta especialmente " +
            "útil contra enemigos como los gólems.\r\n\r\n" +
            "< ! > Granada Bailarina y Llamado a Escena ayudan a controlar grupos, uno de los puntos débiles de " +
            "Jhin durante las primeras etapas.";

        private static readonly string JhinSpanishDescriptionEsEs =
            "Jhin es un superviviente de gran daño explosivo contra un solo objetivo, centrado en disparos " +
            "contundentes en lugar de una cadencia rápida.<color=#CCD3E0>\r\n\r\n" +
            "< ! > Jhin sólo puede obtener velocidad de ataque al subir de nivel. Cualquier otra fuente aumenta " +
            "en su lugar la eficacia de daño de sus demás habilidades.\r\n\r\n" +
            "< ! > Los golpes críticos de Jhin le otorgan velocidad de movimiento adicional. Usa el crítico " +
            "garantizado de su cuarto disparo para salir de situaciones peligrosas.\r\n\r\n" +
            "< ! > Florecer mortal puede interrumpir ataques que de otro modo no podrías evitar; resulta especialmente " +
            "útil contra enemigos como los gólems.\r\n\r\n" +
            "< ! > Granada danzante y Abajo el telón ayudan a controlar grupos, uno de los puntos débiles de " +
            "Jhin durante las primeras etapas.";

        // Traducción fiel del lore que publica el creador del mod.
        // El original inglés se conserva intacto.
        private static readonly string JhinSpanishLoreEs419 =
            "Jhin es un psicópata meticuloso que cree que el asesinato es un arte. Antiguamente fue un prisionero " +
            "jonio, pero elementos sombríos del consejo gobernante de Jonia lo liberaron y ahora el asesino en serie " +
            "trabaja como sicario para su camarilla. Utilizando su arma como pincel, Jhin crea obras de brutalidad " +
            "artística que horrorizan a víctimas y espectadores. Obtiene un placer cruel al montar su macabro teatro, " +
            "lo que lo convierte en la opción ideal para enviar el más poderoso de los mensajes: terror.";

        private static readonly string JhinSpanishLoreEsEs =
            "Jhin es un meticuloso criminal psicópata que considera el asesinato un arte. Antiguo prisionero jonio, " +
            "fue liberado por elementos sombríos del consejo gobernante de Jonia y ahora el asesino en serie trabaja " +
            "como sicario para su camarilla. Utilizando su arma como pincel, Jhin crea obras de brutalidad artística " +
            "que horrorizan a víctimas y espectadores. Obtiene un placer cruel al escenificar su macabro teatro, " +
            "lo que lo convierte en la elección ideal para transmitir el más poderoso de los mensajes: terror.";

        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            RegisterJhinSpanishTokens();
            RegisterJhinKeywordTranslations(logger);

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | JhinBody | " +
                "es-419 / es-ES + pasiva + skills + keywords + skins + lore nativo"
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "El Virtuoso"
                : fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (!IsSpanishLanguage())
            {
                return fallback ?? "";
            }

            return IsSpanishSpainLanguage()
                ? JhinSpanishDescriptionEsEs
                : JhinSpanishDescriptionEs419;
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (!IsSpanishLanguage())
            {
                return fallback ?? "";
            }

            return IsSpanishSpainLanguage()
                ? JhinSpanishLoreEsEs
                : JhinSpanishLoreEs419;
        }

        private static void RegisterJhinSpanishTokens()
        {
            // Identidad / Vista general / lore del Logbook nativo.
            RegisterRegionalToken(
                "SERORONIN_JHIN_BODY_SUBTITLE",
                "El Virtuoso",
                "El Virtuoso"
            );

            RegisterRegionalToken(
                "SERORONIN_JHIN_BODY_DESCRIPTION",
                JhinSpanishDescriptionEs419,
                JhinSpanishDescriptionEsEs
            );

            // SurvivorContentProfileService deriva este token desde
            // SERORONIN_JHIN_BODY_NAME para leer el lore del Logbook.
            RegisterRegionalToken(
                "SERORONIN_JHIN_BODY_LORE",
                JhinSpanishLoreEs419,
                JhinSpanishLoreEsEs
            );

            // Pasiva propia del mod.
            RegisterRegionalToken(
                "SERORONIN_JHIN_BODY_PASSIVE_NAME",
                "Cada momento cuenta",
                "Cada momento cuenta"
            );
            RegisterRegionalSkillDescription(
                "SERORONIN_JHIN_BODY_PASSIVE_DESCRIPTION",
                "Jhin sólo obtiene <style=cIsDamage>Velocidad de ataque</style> al subir de nivel. El " +
                "<style=cIsDamage>60% de la Velocidad de ataque</style> obtenida de otras fuentes se convierte " +
                "en <style=cDeath>Daño adicional porcentual</style>. Además, los <style=cDeath>golpes críticos</style> " +
                "le otorgan un <style=cIsUtility>10%</style> + (<style=cIsUtility>0,4%</style> por cada " +
                "<style=cIsDamage>1% de velocidad de ataque adicional</style>) de " +
                "<style=cIsUtility>velocidad de movimiento adicional</style> durante <style=cIsUtility>2 s</style>.",
                "Jhin sólo obtiene <style=cIsDamage>Velocidad de ataque</style> al subir de nivel. El " +
                "<style=cIsDamage>60% de la Velocidad de ataque</style> obtenida de otras fuentes se convierte " +
                "en <style=cDeath>Daño adicional porcentual</style>. Además, los <style=cDeath>golpes críticos</style> " +
                "le otorgan un <style=cIsUtility>10%</style> + (<style=cIsUtility>0,4%</style> por cada " +
                "<style=cIsDamage>1% de velocidad de ataque adicional</style>) de " +
                "<style=cIsUtility>velocidad de movimiento adicional</style> durante <style=cIsUtility>2 s</style>."
            );

            // Primaria. Riot usa nombres regionales distintos.
            RegisterRegionalToken(
                "SERORONIN_JHIN_BODY_PRIMARY_WHISPER_NAME",
                "Murmullo",
                "Susurro"
            );
            RegisterRegionalSkillDescription(
                "SERORONIN_JHIN_BODY_PRIMARY_WHISPER_DESCRIPTION",
                "<style=cIsUtility>Ágil.</style> Dispara una bala que inflige <style=cIsDamage>600% de daño</style>. " +
                "El cuarto disparo <style=cDeath>asesta un golpe crítico</style> y es " +
                "<color=#ff5078>ejecutor</color>. Puede disparar hasta 4 veces antes de tener que " +
                "<style=cIsUtility>recargar</style>.",
                "<style=cIsUtility>Ágil.</style> Dispara una bala que inflige <style=cIsDamage>600% de daño</style>. " +
                "El cuarto disparo <style=cDeath>asesta un golpe crítico</style> y es " +
                "<color=#ff5078>ejecutor</color>. Puede disparar hasta 4 veces antes de tener que " +
                "<style=cIsUtility>recargar</style>."
            );

            // Secundaria.
            RegisterRegionalToken(
                "SERORONIN_JHIN_BODY_SECONDARY_GRENADE_NAME",
                "Granada Bailarina",
                "Granada danzante"
            );
            RegisterRegionalSkillDescription(
                "SERORONIN_JHIN_BODY_SECONDARY_GRENADE_DESCRIPTION",
                "<style=cIsUtility>Ágil.</style> Dispara una granada dirigida que inflige " +
                "<style=cIsDamage>444% de daño</style>. La granada rebota hacia un enemigo cercano hasta " +
                "<style=cIsDamage>3</style> veces adicionales. Cada rebote obtiene un " +
                "<style=cIsDamage>30% de daño TOTAL</style> adicional si el enemigo " +
                "<style=cDeath>muere</style>.",
                "<style=cIsUtility>Ágil.</style> Dispara una granada dirigida que inflige " +
                "<style=cIsDamage>444% de daño</style>. La granada rebota hacia un enemigo cercano hasta " +
                "<style=cIsDamage>3</style> veces adicionales. Cada rebote obtiene un " +
                "<style=cIsDamage>30% de daño TOTAL</style> adicional si el enemigo " +
                "<style=cDeath>muere</style>."
            );

            // Utilidad.
            RegisterRegionalToken(
                "SERORONIN_JHIN_BODY_UTILITY_FLOURISH_NAME",
                "Brote Mortal",
                "Florecer mortal"
            );
            RegisterRegionalSkillDescription(
                "SERORONIN_JHIN_BODY_UTILITY_FLOURISH_DESCRIPTION",
                "<color=#ff5078>Cautivador.</color> <style=cIsUtility>Aturdidor.</style> Dispara un haz " +
                "<style=cIsDamage>perforante</style> que inflige <style=cIsDamage>800% de daño</style>. " +
                "Dañar a un enemigo activa <color=#ff5078>Cada momento cuenta</color> como si Jhin hubiera " +
                "asestado un golpe crítico, durante <style=cIsUtility>4 s</style>.",
                "<color=#ff5078>Cautivador.</color> <style=cIsUtility>Aturdidor.</style> Dispara un haz " +
                "<style=cIsDamage>perforante</style> que inflige <style=cIsDamage>800% de daño</style>. " +
                "Dañar a un enemigo activa <color=#ff5078>Cada momento cuenta</color> como si Jhin hubiera " +
                "asestado un golpe crítico, durante <style=cIsUtility>4 s</style>."
            );

            // Especial.
            RegisterRegionalToken(
                "SERORONIN_JHIN_BODY_SPECIAL_ULT_NAME",
                "Llamado a Escena",
                "Abajo el telón"
            );
            RegisterRegionalSkillDescription(
                "SERORONIN_JHIN_BODY_SPECIAL_ULT_DESCRIPTION",
                "<color=#ff5078>Ejecutor.</color> <style=cIsUtility>Recarga</style> al instante y potencia tus " +
                "próximos 4 disparos. Durante los siguientes <style=cIsUtility>10 s</style>, usar tu habilidad " +
                "primaria dispara proyectiles <style=cIsDamage>explosivos</style> que infligen " +
                "<style=cIsDamage>900% de daño</style>. <style=cDeath>El cuarto disparo asesta un golpe crítico</style>. " +
                "<style=cIsUtility>Reduce</style> el tiempo de recarga un <style=cIsUtility>15%</style> por cada " +
                "disparo restante cuando termina Llamado a Escena.",
                "<color=#ff5078>Ejecutor.</color> <style=cIsUtility>Recarga</style> al instante y potencia tus " +
                "próximos 4 disparos. Durante los siguientes <style=cIsUtility>10 s</style>, usar tu habilidad " +
                "primaria dispara proyectiles <style=cIsDamage>explosivos</style> que infligen " +
                "<style=cIsDamage>900% de daño</style>. <style=cDeath>El cuarto disparo asesta un golpe crítico</style>. " +
                "<style=cIsUtility>Reduce</style> el tiempo de recarga un <style=cIsUtility>15%</style> por cada " +
                "disparo restante cuando termina Abajo el telón."
            );

            // Skins presentes en Jhin 1.5.1.
            RegisterRegionalToken("SERORONIN_JHIN_BODY_DEFAULT_SKIN_NAME", "Jhin", "Jhin");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_HIGHNOON_SKIN_NAME", "Jhin el Forajido", "Jhin solo ante el peligro");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_BLOODMOON_SKIN_NAME", "Jhin Luna de Sangre", "Jhin luna sangrienta");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_SKTT1_SKIN_NAME", "Jhin SKT T1", "Jhin SKT T1");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_PROJECT_SKIN_NAME", "PROYECTO: Jhin", "PROYECTO: Jhin");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_SHANHAI_SKIN_NAME", "Jhin Pergaminos Shan Hai", "Jhin pergaminos de Shan Hai");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_DWG_SKIN_NAME", "Jhin DWG", "Jhin DWG");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_EMPYREAN_SKIN_NAME", "Jhin Empíreo", "Jhin empíreo");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_SOULFIGHTER_SKIN_NAME", "Jhin Peleador Álmico", "Jhin luchador de almas");
            RegisterRegionalToken("SERORONIN_JHIN_BODY_ARCANA_SKIN_NAME", "Jhin Arcana", "Jhin arcano");
        }


        /// <summary>
        /// Jhin 1.5.1 mantiene varias explicaciones del panel lateral en
        /// keywordTokens separados de las descripciones principales:
        /// Executing: Primary, Reload, Every Moment Matters, Captivating y
        /// Executing: Special. El auditor estándar no expone sus nombres de
        /// token, así que los resolvemos desde los SkillDef reales de Jhin.
        ///
        /// No hay referencia a JhinMod.dll: se inspecciona SkillCatalog y se
        /// registra la traducción sobre el token exacto que ya usa el mod.
        /// </summary>
        private static void RegisterJhinKeywordTranslations(
            ManualLogSource logger
        )
        {
            // Jhin 1.5.1 declara estos tokens literalmente en sus SkillDef.
            // Registrarlos de forma directa evita depender del texto inglés
            // que Language.GetString() haya resuelto en ese momento.
            RegisterRegionalToken(
                "KEYWORD_EXECUTING_WHISPER",
                "<style=cKeywordName>Ejecutor: Primaria</style>" +
                "<style=cSub>Inflige daño adicional equivalente al <style=cIsDamage>30%</style> " +
                "de la salud que le falte al objetivo. Este daño adicional no puede superar el " +
                "<style=cIsDamage>100%</style> del daño original.</style>",
                "<style=cKeywordName>Ejecutor: Primaria</style>" +
                "<style=cSub>Inflige daño adicional equivalente al <style=cIsDamage>30%</style> " +
                "de la salud que le falte al objetivo. Este daño adicional no puede superar el " +
                "<style=cIsDamage>100%</style> del daño original.</style>"
            );

            RegisterRegionalToken(
                "KEYWORD_SCALING_WHISPER",
                "<style=cKeywordName>Cada momento cuenta</style>" +
                "<style=cSub>Obtiene coeficiente de activación adicional en función del " +
                "<style=cIsDamage>100%</style> de la velocidad de ataque adicional previa a la conversión.</style>",
                "<style=cKeywordName>Cada momento cuenta</style>" +
                "<style=cSub>Obtiene coeficiente de activación adicional en función del " +
                "<style=cIsDamage>100%</style> de la velocidad de ataque adicional previa a la conversión.</style>"
            );

            RegisterRegionalToken(
                "KEYWORD_EXECUTING_SPECIAL",
                "<style=cKeywordName>Ejecutor: Especial</style>" +
                "<style=cSub>Inflige hasta un <style=cIsDamage>300%</style> de daño adicional " +
                "según la salud que le falte al objetivo.</style>",
                "<style=cKeywordName>Ejecutor: Especial</style>" +
                "<style=cSub>Inflige hasta un <style=cIsDamage>300%</style> de daño adicional " +
                "según la salud que le falte al objetivo.</style>"
            );

            RegisterRegionalToken(
                "KEYWORD_RELOAD",
                "<style=cKeywordName>Recarga</style>" +
                "<style=cSub>Entra en estado de recarga después de disparar 4 tiros de " +
                "<color=#ff5078>Murmullo</color> o después de <style=cIsUtility>10 segundos</style>. " +
                "<i>El temporizador se reinicia al usar cualquier habilidad.</i></style>",
                "<style=cKeywordName>Recarga</style>" +
                "<style=cSub>Entra en estado de recarga después de disparar 4 tiros de " +
                "<color=#ff5078>Susurro</color> o después de <style=cIsUtility>10 segundos</style>. " +
                "<i>El temporizador se reinicia al usar cualquier habilidad.</i></style>"
            );

            RegisterRegionalToken(
                "KEYWORD_CAPTIVATING",
                "<style=cKeywordName>Cautivador</style>" +
                "<style=cSub>Las demás habilidades de Jhin <style=cDeath>marcan</style> a los enemigos durante " +
                "<style=cIsUtility>4 segundos</style>. Golpear a un enemigo marcado con " +
                "<color=#ff5078>Brote Mortal</color> consume la marca y lo " +
                "<style=cIsDamage>inmoviliza</style> durante <style=cIsUtility>2 segundos</style>.</style>",
                "<style=cKeywordName>Cautivador</style>" +
                "<style=cSub>Las demás habilidades de Jhin <style=cDeath>marcan</style> a los enemigos durante " +
                "<style=cIsUtility>4 segundos</style>. Golpear a un enemigo marcado con " +
                "<color=#ff5078>Florecer mortal</color> consume la marca y lo " +
                "<style=cIsDamage>inmoviliza</style> durante <style=cIsUtility>2 segundos</style>.</style>"
            );

            logger?.LogInfo(
                "[JHIN LOCALIZATION] Tokens exactos de Jhin 1.5.1 registrados | " +
                "EXECUTING_WHISPER / SCALING_WHISPER / EXECUTING_SPECIAL / RELOAD / CAPTIVATING."
            );
        }

        private static bool LooksLikeJhinSkill(
            string nameToken,
            string descriptionToken,
            string internalName
        )
        {
            return
                ContainsIgnoreCase(nameToken, "SERORONIN_JHIN") ||
                ContainsIgnoreCase(descriptionToken, "SERORONIN_JHIN") ||
                ContainsIgnoreCase(internalName, "Jhin") ||
                ContainsIgnoreCase(internalName, "Whisper") ||
                ContainsIgnoreCase(internalName, "Flourish") ||
                ContainsIgnoreCase(internalName, "Curtain");
        }

        private static JhinKeywordTranslation ResolveJhinKeywordSpanish(
            string token,
            string resolved
        )
        {
            string source = (token ?? "") + "\n" + (resolved ?? "");

            if (
                ContainsIgnoreCase(source, "Executing: Primary") ||
                (
                    ContainsIgnoreCase(source, "30%") &&
                    ContainsIgnoreCase(source, "missing health") &&
                    ContainsIgnoreCase(source, "original damage")
                )
            )
            {
                string text =
                    "<style=cKeywordName>Ejecutor: Primaria</style>" +
                    "<style=cSub>Inflige daño adicional equivalente al <style=cIsDamage>30%</style> " +
                    "de la salud que le falte al objetivo. Este daño adicional no puede superar el " +
                    "<style=cIsDamage>100%</style> del daño original.</style>";

                return new JhinKeywordTranslation(text, text);
            }

            if (
                ContainsIgnoreCase(source, "Executing: Special") ||
                (
                    ContainsIgnoreCase(source, "300%") &&
                    ContainsIgnoreCase(source, "missing health")
                )
            )
            {
                string text =
                    "<style=cKeywordName>Ejecutor: Especial</style>" +
                    "<style=cSub>Inflige hasta un <style=cIsDamage>300%</style> de daño adicional " +
                    "según la salud que le falte al objetivo.</style>";

                return new JhinKeywordTranslation(text, text);
            }

            if (
                ContainsIgnoreCase(source, "Reload") &&
                ContainsIgnoreCase(source, "4") &&
                ContainsIgnoreCase(source, "10 seconds")
            )
            {
                return new JhinKeywordTranslation(
                    "<style=cKeywordName>Recarga</style>" +
                    "<style=cSub>Entra en estado de recarga después de disparar " +
                    "<style=cIsDamage>4</style> tiros de Murmullo o después de " +
                    "<style=cIsUtility>10 segundos</style>. El temporizador se reinicia al usar cualquier habilidad.</style>",
                    "<style=cKeywordName>Recarga</style>" +
                    "<style=cSub>Entra en estado de recarga después de disparar " +
                    "<style=cIsDamage>4</style> tiros de Susurro o después de " +
                    "<style=cIsUtility>10 segundos</style>. El temporizador se reinicia al usar cualquier habilidad.</style>"
                );
            }

            if (
                ContainsIgnoreCase(source, "Every Moment Matters") &&
                ContainsIgnoreCase(source, "proc coefficient")
            )
            {
                string text =
                    "<style=cKeywordName>Cada momento cuenta</style>" +
                    "<style=cSub>Obtiene coeficiente de activación adicional en función del " +
                    "<style=cIsDamage>100%</style> de la velocidad de ataque adicional previa a la conversión.</style>";

                return new JhinKeywordTranslation(text, text);
            }

            if (
                ContainsIgnoreCase(source, "Captivating") &&
                ContainsIgnoreCase(source, "mark enemies")
            )
            {
                return new JhinKeywordTranslation(
                    "<style=cKeywordName>Cautivador</style>" +
                    "<style=cSub>Las demás habilidades de Jhin marcan a los enemigos durante " +
                    "<style=cIsUtility>4 segundos</style>. Golpear a un enemigo marcado con " +
                    "<style=cIsDamage>Brote Mortal</style> consume la marca y lo inmoviliza durante " +
                    "<style=cIsUtility>2 segundos</style>.</style>",
                    "<style=cKeywordName>Cautivador</style>" +
                    "<style=cSub>Las demás habilidades de Jhin marcan a los enemigos durante " +
                    "<style=cIsUtility>4 segundos</style>. Golpear a un enemigo marcado con " +
                    "<style=cIsDamage>Florecer mortal</style> consume la marca y lo inmoviliza durante " +
                    "<style=cIsUtility>2 segundos</style>.</style>"
                );
            }

            return null;
        }

        private sealed class JhinKeywordTranslation
        {
            public readonly string Es419;
            public readonly string EsEs;

            public JhinKeywordTranslation(string es419, string esEs)
            {
                Es419 = es419 ?? "";
                EsEs = esEs ?? es419 ?? "";
            }
        }

        private static object ReadStaticMember(Type type, string memberName)
        {
            if (type == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static;

            FieldInfo field = type.GetField(memberName, flags);
            if (field != null)
            {
                return field.GetValue(null);
            }

            PropertyInfo property = type.GetProperty(memberName, flags);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(null, null);
            }

            return null;
        }

        private static object ReadMember(object instance, string memberName)
        {
            if (instance == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            Type type = instance.GetType();
            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;

            FieldInfo field = type.GetField(memberName, flags);
            if (field != null)
            {
                return field.GetValue(instance);
            }

            PropertyInfo property = type.GetProperty(memberName, flags);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(instance, null);
            }

            return null;
        }

        private static string ReadStringMember(object instance, string memberName)
        {
            object value = ReadMember(instance, memberName);
            return value as string ?? "";
        }

        private static bool ContainsIgnoreCase(string text, string value)
        {
            return
                !string.IsNullOrEmpty(text) &&
                !string.IsNullOrEmpty(value) &&
                text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void RegisterRegionalSkillDescription(
            string token,
            string spanish419,
            string spanishSpain
        )
        {
            RegisterRegionalToken(
                token,
                FormatLongSkillText(spanish419),
                FormatLongSkillText(spanishSpain)
            );
        }

        private static void RegisterRegionalToken(
            string token,
            string spanish419,
            string spanishSpain
        )
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(spanish419))
            {
                LanguageAPI.Add(token, spanish419, "es-419");
            }

            if (!string.IsNullOrWhiteSpace(spanishSpain))
            {
                LanguageAPI.Add(token, spanishSpain, "es-ES");
            }
        }

        private static bool IsSpanishLanguage()
        {
            string language = Language.currentLanguageName ?? "";
            return language.StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSpanishSpainLanguage()
        {
            return string.Equals(
                Language.currentLanguageName ?? "",
                "es-ES",
                StringComparison.OrdinalIgnoreCase
            );
        }
    }
}
