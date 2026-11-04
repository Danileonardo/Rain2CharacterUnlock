using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada y no invasiva de EnforcerGang / Moffein SniperClassic.
    ///
    /// Origen verificado (log 2026-10-15):
    /// Body: SniperClassicBody; ContentPack: SniperClassic.content;
    /// unlock original: SNIPERCLASSIC_CHARACTERUNLOCKABLE_REWARD_ID.
    /// Traducción curada ESTÁTICA al español latino (es-419) y España (es-ES):
    /// identidad, vista general, habilidades, alternativas, palabras clave,
    /// skins y desbloqueables originales. Los textos de habilidad observados
    /// en capturas del usuario tienen prioridad para evitar cifras inventadas.
    /// NO toca logros ni cambia el desbloqueo Original individual. El lore
    /// original permanece intacto en el mod; USU registra narrativa propia y alias de presentación.
    /// </summary>
    public sealed class SniperClassicDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "SniperClassic.content"; } }
        public override string BodyName { get { return "SniperClassicBody"; } }
        public override string DefinitionName { get { return "Sniper Classic"; } }

        private static readonly string SpanishOverview = SniperClassicLocalization.SpanishOverview;

        private static readonly string EnglishOverview = SniperClassicLocalization.EnglishOverview;

        // Relato curado USU a partir de las referencias del lore original de Sniper Classic.
        // Texto independiente: no altera SniperClassic.dll ni su archivo de idiomas.
        private const string UsUContentLoreToken = "USU_SNIPERCLASSIC_LORE";
        private static readonly string SpanishLore = SniperClassicLore.SpanishLore;

        private static readonly string EnglishLore = SniperClassicLore.EnglishLore;

        public override bool Matches(SurvivorInfo survivor)
        {
            if (survivor == null || !survivor.IsModded ||
                !string.Equals(survivor.BodyName, BodyName, StringComparison.OrdinalIgnoreCase))
                return false;

            // SniperClassic 2.x utiliza un ContentPack independiente en algunas
            // versiones; aceptamos los dos identificadores concretos del creador.
            string pack = survivor.ContentPackIdentifier ?? "";
            return string.Equals(pack, "SniperClassic.content", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(pack, "com.Moffein.SniperClassic", StringComparison.OrdinalIgnoreCase);
        }

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (!Matches(survivor)) return;

            // Las traducciones son CURADAS y ESTÁTICAS, no dependen de la
            // carpeta externa language del proveedor. No tocar su DLL.
            RegisterSniperSpanishTokens();
            // USU mantiene su propia narración, separada del token original del mod.
            RegisterCuratedLore(survivor, logger);

            UsuLog.Verbose(logger, 
                "[SNIPER LOCALIZATION] es-419/es-ES | " +
                "identidad + vista general + habilidades base y alternativas + " +
                "4 keywords + skins + 3 desbloqueables originales | " +
                "Body: SniperClassicBody | Original individual conservado."
            );
        }

        private static void RegisterSniperSpanishTokens()
        {
            // Identidad y descripción. El lore nativo NO se sustituye por
            // textos no verificados; véanse las notas del parche.
            RegisterSpanishToken("SNIPERCLASSIC_BODY_NAME", "Sniper");
            RegisterSpanishToken("SNIPERCLASSIC_BODY_SUBTITLE", "Ojo de halcón");
            RegisterSpanishToken("SNIPERCLASSIC_DESCRIPTION", SpanishOverview);

            RegisterSpanishToken("SNIPERCLASSIC_OUTRO_FLAVOR",
                "...y así se marcharon, sin prestar atención a lo que oían.");
            RegisterSpanishToken("SNIPERCLASSIC_MAIN_ENDING_ESCAPE_FAILURE_FLAVOR",
                "...y así desaparecieron, sin que nadie los recordara.");

            // Primaria, recarga y variantes.
            RegisterSpanishToken("SNIPERCLASSIC_PRIMARY_NAME", "Disparo de precisión");
            RegisterSpanishToken("SNIPERCLASSIC_PRIMARY_DESCRIPTION",
                "Dispara un proyectil penetrante que inflige <style=cIsDamage>430 % de daño</style>. " +
                "Tras disparar, <style=cIsUtility>recarga</style> para obtener hasta " +
                "<style=cIsDamage>1,5× de daño adicional</style> si sincronizas la recarga correctamente.");
            RegisterSpanishToken("SNIPERCLASSIC_RELOAD_NAME", "Recargar");
            RegisterSpanishToken("SNIPERCLASSIC_RELOAD_DESCRIPTION", "Recarga tu arma.");

            RegisterSpanishToken("SNIPERCLASSIC_PRIMARY_ALT_NAME", "Marca");
            RegisterSpanishToken("SNIPERCLASSIC_PRIMARY_ALT_DESCRIPTION",
                "Dispara una ráfaga de <style=cIsDamage>5 proyectiles penetrantes</style> " +
                "que infligen <style=cIsDamage>360 % de daño</style>. " +
                "Después, <style=cIsUtility>recarga</style> en el momento preciso para " +
                "<style=cIsUtility>reducir el tiempo de recarga de Spotter</style>.");

            RegisterSpanishToken("SNIPERCLASSIC_PRIMARY_ALT2_NAME", "Impacto contundente");
            RegisterSpanishToken("SNIPERCLASSIC_PRIMARY_ALT2_DESCRIPTION",
                "Dispara un proyectil de <style=cIsDamage>mortero</style> que inflige " +
                "<style=cIsDamage>540 % de daño</style>. Tras disparar, " +
                "<style=cIsUtility>recarga</style> para obtener hasta " +
                "<style=cIsDamage>1,5× de daño adicional</style> si sincronizas la recarga correctamente.");

            // Secundaria.
            RegisterSpanishToken("SNIPERCLASSIC_SECONDARY_NAME", "Puntería firme");
            RegisterSpanishToken("SNIPERCLASSIC_SECONDARY_DESCRIPTION",
                "Activa la <style=cIsUtility>mira telescópica</style> y " +
                "<style=cIsDamage>carga</style> tu arma hasta infligir " +
                "<style=cIsDamage>3× de daño</style>. Con la carga completa puedes " +
                "apuntar a los <style=cIsDamage>puntos débiles</style>.");

            // Utilidad base y alternativa vista en el loadout.
            RegisterSpanishToken("SNIPERCLASSIC_UTILITY_NAME", "Entrenamiento de combate");
            RegisterSpanishToken("SNIPERCLASSIC_UTILITY_DESCRIPTION",
                "Vuélvete <style=cIsUtility>invisible</style>, rueda una distancia corta " +
                "y <style=cIsUtility>recarga</style> tu arma.");
            RegisterSpanishToken("SNIPERCLASSIC_UTILITY_BACKFLIP_NAME", "Entrenamiento militar");
            RegisterSpanishToken("SNIPERCLASSIC_UTILITY_BACKFLIP_DESCRIPTION",
                "<style=cIsDamage>Aturde</style>. Da una voltereta hacia atrás en el aire " +
                "y <style=cIsUtility>recarga</style> tu arma.");

            // Especial, variantes y mejoras por Cetro antiguo.
            RegisterSpanishToken("SNIPERCLASSIC_SPECIAL_NAME", "Spotter: RETROALIMENTACIÓN");
            RegisterSpanishToken("SNIPERCLASSIC_SPECIAL_DESCRIPTION",
                "<style=cIsDamage>Analiza</style> a un enemigo. Al golpearlo por más del " +
                "<style=cIsDamage>1000 % de daño</style>, electrocuta a los enemigos cercanos " +
                "por el <style=cIsDamage>50 % del daño total</style>. Después, " +
                "<style=cIsHealth>Spotter debe recargarse</style>.");
            RegisterSpanishToken("SNIPERCLASSIC_SPECIAL_SCEPTER_NAME", "Spotter: SOBRECARGA");
            RegisterSpanishToken("SNIPERCLASSIC_SPECIAL_SCEPTER_DESCRIPTION",
                "<style=cIsDamage>Analiza</style> a un enemigo. Al golpearlo por más del " +
                "<style=cIsDamage>1000 % de daño</style>, electrocuta a los enemigos cercanos " +
                "por el <style=cIsDamage>100 % del daño total</style>. Después, " +
                "<style=cIsHealth>Spotter debe recargarse</style>.");
            RegisterSpanishToken("SNIPERCLASSIC_SPECIAL_ALT_NAME", "Spotter: DISTRACCIÓN");
            RegisterSpanishToken("SNIPERCLASSIC_SPECIAL_ALT_DESCRIPTION",
                "<style=cIsDamage>Analiza</style> a un enemigo durante " +
                "<style=cIsUtility>7 s</style>, distrae a los enemigos cercanos " +
                "y causa <style=cIsDamage>7×100 % de daño</style> al objetivo.");
            RegisterSpanishToken("SNIPERCLASSIC_SPECIAL_ALT_SCEPTER_NAME", "Spotter: RÁFAGA");
            RegisterSpanishToken("SNIPERCLASSIC_SPECIAL_ALT_SCEPTER_DESCRIPTION",
                "<style=cIsDamage>Analiza</style> a un enemigo durante " +
                "<style=cIsUtility>7 s</style>, distrae a los enemigos cercanos " +
                "y causa <style=cIsDamage>7×200 % de daño</style> al objetivo.");

            // Tooltips de palabras clave. Mantener <style=cSub> y porcentajes.
            RegisterSpanishToken("KEYWORD_SNIPERCLASSIC_WEAKPOINT",
                "<style=cKeywordName>Punto débil</style><style=cSub>Algunas partes de los " +
                "enemigos contienen <style=cIsDamage>puntos débiles</style>. " +
                "Acertar en uno de ellos inflige <style=cIsDamage>50 % más de daño</style>.</style>");
            RegisterSpanishToken("KEYWORD_SNIPERCLASSIC_OVERCHARGE",
                "<style=cKeywordName>Sobrecarga</style><style=cSub>Las cargas adicionales de " +
                "la <style=cIsUtility>habilidad secundaria</style> permiten seguir cargando " +
                "el arma por encima del máximo para causar más daño.</style>");
            RegisterSpanishToken("KEYWORD_SNIPERCLASSIC_ANALYZED",
                "<style=cKeywordName>Analizado</style><style=cSub>Reduce la " +
                "<style=cIsUtility>velocidad de movimiento un 40 %</style> y " +
                "la <style=cIsDamage>armadura en 30</style>.</style>");
            RegisterSpanishToken("KEYWORD_SNIPERCLASSIC_MORTAR",
                "<style=cKeywordName>Mortero</style><style=cSub>Proyectil explosivo " +
                "cuyo radio de explosión aumenta con la distancia recorrida.</style>");

            // Skins originales: nombres de aspecto, NO nombres de sus logros.
            RegisterSpanishToken("SNIPERCLASSIC_MASTERY_SKIN_NAME", "Agente");
            RegisterSpanishToken("SNIPERCLASSIC_GRANDMASTERY_SKIN_NAME", "Expedición");

            // Desbloqueables nativos. Se conservan las condiciones, el estado
            // individual y el registro de achievements proporcionado por Sniper.
            RegisterSpanishToken("SNIPERCLASSIC_CHARACTERUNLOCKABLE_ACHIEVEMENT_NAME", "¡Detectado!");
            RegisterSpanishToken("SNIPERCLASSIC_CHARACTERUNLOCKABLE_ACHIEVEMENT_DESC",
                "Repara un dron de equipo usando un Escáner de radar.");
            RegisterSpanishToken("SNIPERCLASSIC_CHARACTERUNLOCKABLE_UNLOCKABLE_NAME", "¡Detectado!");

            RegisterSpanishToken("SNIPERCLASSIC_MASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Sniper: Maestría");
            RegisterSpanishToken("SNIPERCLASSIC_MASTERYUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Sniper, completa el juego u oblítérate en dificultad Monzón.");
            RegisterSpanishToken("SNIPERCLASSIC_MASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Sniper: Maestría");

            RegisterSpanishToken("SNIPERCLASSIC_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_NAME",
                "Sniper: Gran maestría");
            RegisterSpanishToken("SNIPERCLASSIC_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Sniper, completa el juego u oblítérate en Eclipse o Tifón. " +
                "<color=#8888>(Tifón requiere Starstorm 2).</color>");
            RegisterSpanishToken("SNIPERCLASSIC_GRANDMASTERYUNLOCKABLE_UNLOCKABLE_NAME",
                "Sniper: Gran maestría");
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            if (IsSpanish()) return SpanishOverview;
            if (IsEnglish()) return EnglishOverview;
            return fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            // La sección Notas usa el relato USU para los idiomas admitidos.
            // Su visibilidad sigue siendo responsabilidad de SurvivorLogbookResolver:
            // no se desbloquea ni se inventa una nueva entrada del Diario.
            if (IsSpanish()) return SpanishLore;
            if (IsEnglish()) return EnglishLore;
            return fallback ?? "";
        }

        private static readonly List<object> CuratedNativeLoreOverlays = new List<object>();
        private static readonly HashSet<string> RegisteredCuratedLoreOverlays =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static void RegisterCuratedLore(SurvivorInfo survivor, ManualLogSource logger)
        {
            // Fuente de verdad propia de USU. No editamos la historia publicada
            // por el creador ni sus archivos language.
            LanguageAPI.Add(UsUContentLoreToken, SpanishLore, "es-419");
            LanguageAPI.Add(UsUContentLoreToken, SpanishLore, "es-ES");
            LanguageAPI.Add(UsUContentLoreToken, EnglishLore, "en");

            // El Logbook nativo usa un token de lore que puede diferir entre
            // versiones del mod. Conservar la fuente USU separada nos permite
            // ofrecer el mismo relato también en la entrada nativa mediante
            // los alias conocidos, SIN alterar SniperClassic.dll ni los perfiles.
            // Esto solo cambia los textos localizados en memoria. No cambia el
            // estado de descubrimiento del Diario ni concede el personaje.
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SNIPERCLASSIC_BODY_LORE",
                "SNIPERCLASSIC_LORE"
            };

            SurvivorDef def = survivor != null ? survivor.SurvivorDef : null;
            if (def != null)
            {
                AddDerivedLoreToken(candidates, ReadStringMember(def, "displayNameToken"));
                CharacterBody body = def.bodyPrefab != null
                    ? def.bodyPrefab.GetComponent<CharacterBody>() : null;
                AddDerivedLoreToken(candidates, ReadStringMember(body, "baseNameToken"));
                AddDirectLoreToken(candidates, ReadStringMember(def, "loreToken"));
                AddDirectLoreToken(candidates, ReadStringMember(body, "loreToken"));
            }

            int aliases = 0;
            foreach (string token in candidates)
            {
                // Registrar alias para ambos idiomas, incluso cuando el mod
                // original haya definido el suyo, porque el usuario ha elegido
                // expresamente el relato curado de USU. Conservar el original
                // íntegro en la instalación permite recuperarlo al desactivar USU.
                if (string.IsNullOrWhiteSpace(token) ||
                    string.Equals(token, UsUContentLoreToken, StringComparison.OrdinalIgnoreCase))
                    continue;

                RegisterNativeAlias(token, SpanishLore, "es-419", logger);
                RegisterNativeAlias(token, SpanishLore, "es-ES", logger);
                RegisterNativeAlias(token, EnglishLore, "en", logger);
                aliases++;
            }

            UsuLog.Verbose(logger, "[SNIPER LORE USU] Registrado token independiente: " +
                UsUContentLoreToken + " | alias nativos candidatos: " +
                string.Join(",", candidates) + " | total: " + aliases +
                " | sin alterar desbloqueos ni estado del Diario.");
        }

        private static void RegisterNativeAlias(
            string token, string value, string language, ManualLogSource logger)
        {
            string key = language + ":" + token;
            if (!RegisteredCuratedLoreOverlays.Add(key)) return;

            try
            {
                object overlay = LanguageAPI.AddOverlay(token, value, language);
                if (overlay != null) CuratedNativeLoreOverlays.Add(overlay);
            }
            catch (Exception ex)
            {
                RegisteredCuratedLoreOverlays.Remove(key);
                logger?.LogWarning("[SNIPER LORE USU] No fue posible registrar " +
                    token + " (" + language + "): " + ex.Message);
            }
        }

        private static void AddDirectLoreToken(HashSet<string> candidates, string token)
        {
            if (!string.IsNullOrWhiteSpace(token) && token.IndexOf(' ') < 0)
                candidates.Add(token.Trim());
        }

        private static void AddDerivedLoreToken(HashSet<string> candidates, string nameToken)
        {
            if (string.IsNullOrWhiteSpace(nameToken)) return;
            if (nameToken.EndsWith("_NAME", StringComparison.OrdinalIgnoreCase))
                candidates.Add(nameToken.Substring(0, nameToken.Length - 5) + "_LORE");
            if (nameToken.EndsWith("_DISPLAYNAME", StringComparison.OrdinalIgnoreCase))
                candidates.Add(nameToken.Substring(0, nameToken.Length - 12) + "_LORE");
        }

        public override string ResolveOriginalMissionName(SurvivorContentProfile profile, string fallback)
        {
            // "Spotted!" existe en documentación de Sniper. No sustituir el
            // nombre nativo (puede estar ya traducido por el creador).
            if (!string.IsNullOrWhiteSpace(fallback)) return fallback;
            return IsSpanish() ? "¡Detectado!" : "Spotted!";
        }

        public override string ResolveOriginalMissionDescription(SurvivorContentProfile profile, string fallback)
        {
            // La condición original no está verificada al nivel de los tokens:
            // preservar el texto exacto del creador, sin inventar objetivos.
            return fallback ?? "";
        }

        private static string ReadStringMember(object instance, string member)
        {
            if (instance == null) return "";
            try
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                FieldInfo field = instance.GetType().GetField(member, flags);
                if (field != null) return field.GetValue(instance) as string ?? "";
                PropertyInfo property = instance.GetType().GetProperty(member, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(instance, null) as string ?? "";
            }
            catch { }
            return "";
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnglish()
        {
            return (Language.currentLanguageName ?? "").StartsWith("en", StringComparison.OrdinalIgnoreCase);
        }

        private static void RegisterSpanishToken(string token, string translation)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(translation)) return;
            LanguageAPI.Add(token, translation, "es-419");
            LanguageAPI.Add(token, translation, "es-ES");
        }
    }
}
