using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Sora.
    ///
    /// Autoridad USU para:
    /// - localización visible es-419 / es-ES;
    /// - pasiva y skills publicadas por el mod creador;
    /// - nombres visibles de skins;
    /// - reparación de la descripción general en la ficha de USU cuando el
    ///   texto original inglés llega vacío/malformado;
    /// - lore / Logbook porque el creador no publica una entrada de lore.
    ///
    /// La misión oficial USU "Elegido de la Llave Espada" continúa en el
    /// sistema de misiones actual y se migrará en una fase posterior.
    ///
    /// No referencia Sora.dll. Todo se aplica mediante identificadores y
    /// tokens públicos una vez que el Registry confirma que el survivor está
    /// realmente instalado.
    /// </summary>
    public sealed class SoraDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get
            {
                return "com.Dragonyck.Sora";
            }
        }

        public override string BodyName
        {
            get
            {
                return "SoraBody";
            }
        }

        public override string DefinitionName
        {
            get
            {
                return "Sora";
            }
        }


        private static readonly string SoraSpanishDescription =
            "Sora combina ataques rápidos con la Llave Espada, la versatilidad de la Magia y una gran movilidad aérea.<style=cSub>\r\n\r\n" +
            "< ! > Administra tus MP con cuidado; al agotarlos entrarás en Estado de recuperación y no podrás lanzar Magia temporalmente.<style=cSub>\r\n\r\n" +
            "< ! > Durante el Estado de recuperación, Ataque se vuelve mucho más agresivo y permite encadenar golpes con mayor facilidad.<style=cSub>\r\n\r\n" +
            "< ! > Usa Esquivar para reposicionarte rápidamente o mantén la habilidad para recorrer distancias mayores con Deslizarse.<style=cSub>\r\n\r\n" +
            "< ! > Ataque veloz puede atravesar grupos enteros y recuperar MP al eliminar enemigos.";


        private static readonly string SoraEnglishDescription =
            "Sora combines swift Keyblade attacks, versatile Magic, and strong aerial mobility.<style=cSub>\r\n\r\n" +
            "< ! > Manage MP carefully; exhausting it puts Sora into Recovery State and temporarily prevents him from casting Magic.<style=cSub>\r\n\r\n" +
            "< ! > During Recovery State, Attack becomes much more aggressive and can chain hits together more freely.<style=cSub>\r\n\r\n" +
            "< ! > Tap Dodge to reposition quickly or hold the skill to cover longer distances with Slide.<style=cSub>\r\n\r\n" +
            "< ! > Strike Raid can pierce entire groups and recover MP when it kills enemies.";


        private static readonly string SoraSpanishLore =
            "Registro recuperado de una transmisión sin origen identificado.\n\n" +
            "El joven apareció armado con una llave demasiado grande para una cerradura cualquiera. " +
            "No parecía saber cómo había llegado hasta aquí, aunque tampoco parecía especialmente sorprendido. " +
            "Según él, no era la primera vez que terminaba en un mundo desconocido.\n\n" +
            "Se hace llamar Sora.\n\n" +
            "Habla de mundos conectados por caminos invisibles, de corazones capaces de caer en la oscuridad y de " +
            "criaturas nacidas cuando esa oscuridad los consume. La espada con forma de llave que empuña parece " +
            "responder a su voluntad y le permite canalizar una forma de magia poco común entre los supervivientes registrados.\n\n" +
            "Lo más extraño no es el arma.\n\n" +
            "Es su actitud.\n\n" +
            "Incluso rodeado de ruinas, monstruos y señales de una catástrofe que no comprende, Sora continúa " +
            "avanzando como si estuviera seguro de que cada camino conduce finalmente a alguien a quien vale la pena encontrar.\n\n" +
            "Quizá ésa sea la verdadera razón por la que la Llave Espada lo eligió.\n\n" +
            "No porque sea el más fuerte, sino porque, incluso entre mundos perdidos, todavía cree que ningún corazón está realmente solo.";


        private static readonly string SoraEnglishLore =
            "Record recovered from a transmission of unidentified origin.\n\n" +
            "The young man appeared carrying a key far too large for any ordinary lock. He did not seem to know " +
            "how he had arrived here, though he did not appear particularly surprised either. According to him, " +
            "it was not the first time he had ended up in an unfamiliar world.\n\n" +
            "He calls himself Sora.\n\n" +
            "He speaks of worlds connected by invisible paths, of hearts capable of falling into darkness, and of " +
            "creatures born when that darkness consumes them. The key-shaped sword he carries appears to answer his " +
            "will and allows him to channel a form of magic uncommon among the recorded survivors.\n\n" +
            "The strangest thing is not the weapon.\n\n" +
            "It is his attitude.\n\n" +
            "Even surrounded by ruins, monsters, and signs of a catastrophe he does not understand, Sora keeps moving " +
            "forward as though he is certain every path eventually leads to someone worth finding.\n\n" +
            "Perhaps that is the real reason the Keyblade chose him.\n\n" +
            "Not because he is the strongest, but because even among lost worlds he still believes that no heart is ever truly alone.";


        private static readonly string SoraSpanishPassiveDescription =
            "La <color=#0172eb>Cadena del Reino</color> concede a Sora habilidades mágicas especiales." +
            Environment.NewLine +
            "Sora puede mantenerse suspendido en el aire de forma pasiva. Además, mantener pulsado Salto hace que Sora " +
            "<style=cIsUtility>se deslice hacia delante</style>." +
            Environment.NewLine +
            "Sora posee una <color=#0172eb>barra de MP</color> que le permite lanzar Magia a cambio de " +
            "<color=#0172eb>MP</color>. Cuando los <color=#0172eb>MP</color> llegan a 0, Sora entra en " +
            "<color=#e1279f>Estado de recuperación</color> durante <style=cIsUtility>20 s</style>; durante ese tiempo " +
            "no puede usar Magia y <style=cIsDamage>Ataque</style> queda considerablemente potenciado.";


        private static readonly string SoraSpanishMagicKeyword =
            BuildSoraMagicKeywordSpanish();


        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (
                survivor == null ||
                !Matches(survivor)
            )
            {
                return;
            }

            // El creador no publica lore. USU sólo completa ese hueco en EN.
            RegisterEnglishLoreFallback();

            RegisterSoraSpanishTokens();
            RegisterSoraPassiveSpanish(
                survivor,
                logger
            );

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | " +
                "SoraBody | es-419 / es-ES + pasiva + skills + Magia keyword + skins + lore"
            );
        }


        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Corazón bondadoso"
                : fallback ?? "";
        }


        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return SoraSpanishDescription;
            }

            // El audit actual muestra SORA_DESCRIPTION formado sólo por tags
            // y marcadores < ! >. Reparamos únicamente la ficha de USU en EN
            // cuando el contenido visible no contiene texto útil.
            if (
                IsEnglishLanguage() &&
                IsBrokenDescriptionText(fallback)
            )
            {
                return SoraEnglishDescription;
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
                return SoraSpanishLore;
            }

            if (
                IsEnglishLanguage() &&
                IsMissingLoreText(fallback)
            )
            {
                return SoraEnglishLore;
            }

            return fallback ?? "";
        }


        private static void RegisterEnglishLoreFallback()
        {
            // RoR2 deriva el lore nativo desde SORA_NAME como SORA_LORE.
            // Si una versión futura del creador lo aporta, su registro conserva
            // prioridad y USU queda solamente como fallback.
            LanguageAPI.Add(
                "SORA_LORE",
                SoraEnglishLore,
                "en"
            );
        }


        private static void RegisterSoraSpanishTokens()
        {
            // Survivor / Logbook
            RegisterSpanishToken(
                "SORA_SUBTITLE",
                "Corazón bondadoso"
            );

            RegisterSpanishToken(
                "SORA_DESCRIPTION",
                SoraSpanishDescription
            );

            RegisterSpanishToken(
                "SORA_LORE",
                SoraSpanishLore
            );

            // Pasiva. La descripción también se registra dinámicamente usando
            // los tokens reales publicados por SkillLocator.passiveSkill.
            RegisterSpanishToken(
                "SORA_PASSIVE_NAME",
                "<color=#0172eb>Cadena del Reino</color>"
            );

            RegisterSpanishToken(
                "SORA_PASSIVE_DESCRIPTION",
                FormatLongSkillText(
                    SoraSpanishPassiveDescription
                )
            );

            // Primaria
            RegisterSpanishToken(
                "SORA_M1",
                "Ataque"
            );

            RegisterSpanishToken(
                "SORA_M1_DESCRIPTION",
                FormatLongSkillText(
                    "Ataca hacia delante con tu Llave Espada e inflige <style=cIsDamage>275%</style>/<color=#e1279f>325%</color> " +
                    "de <style=cIsDamage>daño</style>. Cada tercer ataque remata el combo e inflige " +
                    "<style=cIsDamage>325%</style>/<color=#e1279f>385%</color> de <style=cIsDamage>daño</style>. " +
                    "Las eliminaciones restauran <color=#0172eb>7 MP</color>. Mientras estás en " +
                    "<color=#e1279f>Estado de recuperación</color>, todos los ataques pueden encadenarse entre sí; además, " +
                    "aumenta el <style=cIsDamage>retroceso</style> contra enemigos terrestres, los enemigos voladores tienen " +
                    "un <style=cIsDamage>50% de probabilidad al golpearlos</style> de quedar <style=cIsDamage>Aturdidos</style> " +
                    "durante <style=cIsUtility>0,5 s</style>, y Sora se <style=cIsDamage>abalanza</style> rápidamente a través " +
                    "de ellos mientras están <style=cIsDamage>fijados</style>."
                )
            );

            // Secundaria
            RegisterSpanishToken(
                "SORA_M2",
                "Ataque veloz"
            );

            RegisterSpanishToken(
                "SORA_M2_DESCRIPTION",
                FormatLongSkillText(
                    "<style=cIsDamage>Verdugo</style>. Lanza tu Llave Espada, <style=cIsDamage>atravesando</style> a los " +
                    "enemigos en su trayectoria e infligiendo <style=cIsDamage>460% de daño</style>. El daño aumenta un " +
                    "<style=cIsDamage>40%</style> por cada enemigo atravesado. Los enemigos eliminados dejan caer un orbe " +
                    "de <color=#0172eb>MP</color> que restaura <color=#0172eb>12 MP</color>."
                )
            );

            // Utilidad
            RegisterSpanishToken(
                "SORA_UTIL",
                "Esquivar/Deslizarse"
            );

            RegisterSpanishToken(
                "SORA_UTIL_DESCRIPTION",
                FormatLongSkillText(
                    "Pulsa para <style=cIsDamage>Esquivar</style> una distancia corta y volverte " +
                    "<style=cIsDamage>invulnerable</style> durante <style=cIsUtility>0,31 s</style>. El " +
                    "<style=cIsUtility>tiempo de recarga</style> se <style=cIsDamage>reduce</style> en " +
                    "<style=cIsUtility>1 s</style>." + Environment.NewLine +
                    "Mantén pulsado para <style=cIsDamage>Deslizarte</style> una distancia larga y volverte " +
                    "<style=cIsDamage>invulnerable</style> durante <style=cIsUtility>0,18 s</style>."
                )
            );

            // Especial
            RegisterSpanishToken(
                "SORA_SPEC",
                "<color=#0172eb>Magia</color>"
            );

            RegisterSpanishToken(
                "SORA_SPEC_DESCRIPTION",
                "Lanza Magia con el poder de los elementos a cambio de <color=#0172eb>MP</color>."
            );

            // Keyword expandido de Magia. El propio mod de Sora lo publica
            // mediante keywordTokens; USU sólo aporta su equivalente español.
            RegisterSpanishToken(
                "SORA_MAGIC_KEYWORD",
                SoraSpanishMagicKeyword
            );

            // Skins. Sora 1.0.6 usa estos nombres literales como nameToken.
            // Sólo añadimos entradas ES; el inglés del creador no se modifica.
            RegisterSpanishToken(
                "Default",
                "Predeterminado"
            );

            RegisterSpanishToken(
                "Valor",
                "Valor"
            );

            RegisterSpanishToken(
                "Wisdom",
                "Sabiduría"
            );

            RegisterSpanishToken(
                "Limit",
                "Límite"
            );

            RegisterSpanishToken(
                "Master",
                "Maestro"
            );

            RegisterSpanishToken(
                "Final",
                "Final"
            );

            RegisterSpanishToken(
                "Anti",
                "Anti"
            );

            RegisterSpanishToken(
                "Christmas",
                "Navidad"
            );

            RegisterSpanishToken(
                "Halloween",
                "Halloween"
            );

            RegisterSpanishToken(
                "Riku",
                "Riku"
            );
        }


        private static string BuildSoraMagicKeywordSpanish()
        {
            const string bodySize = "82%";

            return
                "<style=cKeywordName><color=#0172eb>Firaga</color></style>" +
                "<style=cSub><size=" + bodySize + ">" +
                "<style=cIsDamage>Incendia</style>. Impúlsate hacia delante y desata un tornado de fuego que rodea a Sora e inflige " +
                "<style=cIsDamage>1000% de daño</style>.</size></style>" +
                Environment.NewLine +
                "<style=cKeywordName><color=#0172eb>Blizzaga</color></style>" +
                "<style=cSub><size=" + bodySize + ">" +
                "<style=cIsUtility>Ralentiza</style>. Dispara una ráfaga <style=cIsDamage>perforante</style> de cristales de hielo que inflige " +
                "<style=cIsDamage>900% de daño</style>.</size></style>" +
                Environment.NewLine +
                "<style=cKeywordName><color=#0172eb>Thundaga</color></style>" +
                "<style=cSub><size=" + bodySize + ">" +
                "Deja caer un rayo sobre un enemigo objetivo e inflige <style=cIsDamage>750% de daño</style> a los enemigos cercanos en un radio amplio.</size></style>" +
                Environment.NewLine +
                "<style=cKeywordName><color=#0172eb>Waterga</color></style>" +
                "<style=cSub><size=" + bodySize + ">" +
                "Dispara un torrente lento de agua que explota al contacto e inflige <style=cIsDamage>1000% de daño</style> a los enemigos en un radio amplio.</size></style>" +
                Environment.NewLine +
                "<style=cKeywordName><color=#0172eb>Aeroga</color></style>" +
                "<style=cSub><size=" + bodySize + ">" +
                "Invoca un huracán en la ubicación de un enemigo objetivo e inflige <style=cIsDamage>1375% de daño</style> a todos los enemigos cercanos en un área enorme.</size></style>" +
                Environment.NewLine +
                "<style=cKeywordName><color=#0172eb>Reflega</color></style>" +
                "<style=cSub><size=" + bodySize + ">" +
                "Sora queda rodeado por una barrera de luz durante <style=cIsUtility>0,85 s</style> que refleja todo el daño recibido de vuelta al atacante, mientras lo <style=cIsDamage>aturde</style> y <style=cIsDamage>empuja</style>. Todos los proyectiles cercanos dentro de un radio pequeño son devueltos a sus atacantes y <style=cIsDamage>aturden</style> al contacto.</size></style>" +
                Environment.NewLine +
                "<style=cKeywordName><color=#0172eb>Magnega</color></style>" +
                "<style=cSub><size=" + bodySize + ">" +
                "Crea un vórtice de fuerza magnética que atrae a todos los enemigos cercanos dentro de un radio medio durante <style=cIsUtility>6 s</style>.</size></style>" +
                Environment.NewLine +
                "<style=cKeywordName><color=#0172eb>Curaga</color></style>" +
                "<style=cSub><size=" + bodySize + ">" +
                "<style=cIsHealing>Restaura salud</style> según la cantidad de <color=#0172eb>MP</color> utilizados. Los aliados cercanos se <style=cIsHealing>curan</style> un 50% de la misma cantidad.</size></style>";
        }


        private static void RegisterSoraPassiveSpanish(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            string nameToken;
            string descriptionToken;

            bool found =
                TryGetPassiveLocalizationTokens(
                    survivor,
                    out nameToken,
                    out descriptionToken
                );

            if (!found)
            {
                logger?.LogWarning(
                    "[LOCALIZATION] Pasiva no resuelta | " +
                    "SoraBody | SkillLocator.passiveSkill no disponible"
                );

                return;
            }

            if (!string.IsNullOrWhiteSpace(nameToken))
            {
                RegisterSpanishToken(
                    nameToken,
                    "<color=#0172eb>Cadena del Reino</color>"
                );
            }

            if (!string.IsNullOrWhiteSpace(descriptionToken))
            {
                RegisterSpanishToken(
                    descriptionToken,
                    FormatLongSkillText(
                        SoraSpanishPassiveDescription
                    )
                );
            }

            logger?.LogInfo(
                "[LOCALIZATION] Pasiva localizada | " +
                "SoraBody | NameToken: " +
                (nameToken ?? "") +
                " | DescriptionToken: " +
                (descriptionToken ?? "")
            );
        }


        private static void RegisterSpanishToken(
            string token,
            string spanish
        )
        {
            if (
                string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(spanish)
            )
            {
                return;
            }

            LanguageAPI.Add(
                token,
                spanish,
                "es-419"
            );

            LanguageAPI.Add(
                token,
                spanish,
                "es-ES"
            );
        }


        private static bool IsSpanishLanguage()
        {
            string language =
                Language.currentLanguageName ?? "";

            return
                language.StartsWith(
                    "es",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static bool IsEnglishLanguage()
        {
            string language =
                Language.currentLanguageName ?? "";

            return
                language.StartsWith(
                    "en",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static bool IsMissingLoreText(
            string text
        )
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            string trimmed =
                text.Trim();

            return
                trimmed.IndexOf(' ') < 0 &&
                trimmed.IndexOf('_') >= 0 &&
                string.Equals(
                    trimmed,
                    trimmed.ToUpperInvariant(),
                    StringComparison.Ordinal
                );
        }


        private static bool IsBrokenDescriptionText(
            string text
        )
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            string visible =
                text
                    .Replace("<style=cSub>", "")
                    .Replace("</style>", "")
                    .Replace("< ! >", "")
                    .Replace("\\n", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Trim();

            return string.IsNullOrWhiteSpace(visible);
        }
    }
}
