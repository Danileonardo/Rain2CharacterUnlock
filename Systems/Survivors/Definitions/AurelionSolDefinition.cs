using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Aurelion Sol.
    ///
    /// Esta Definition ya es la autoridad para la localización ES, reparación
    /// EN de tokens visibles faltantes, achievements/skins y lore/Logbook de
    /// Aurelion. No referencia Aurelion.dll: opera sólo con tokens y catálogos
    /// públicos de RoR2/R2API después de que el mod haya sido detectado.
    /// </summary>
    public sealed class AurelionSolDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get
            {
                return "com.Dragonyck.AurelionSol";
            }
        }

        public override string BodyName
        {
            get
            {
                return "AurelionSolBody";
            }
        }

        public override string DefinitionName
        {
            get
            {
                return "Aurelion Sol";
            }
        }

        // Mantiene vivas las superposiciones de presentación en inglés.
        // El contenido usado es el mismo texto de Aurelion 2.0.10 auditado;
        // USU sólo añade espaciado/tamaño y nunca modifica sus mecánicas.
        private static readonly Dictionary<string, object>
            AurelionEnglishSkillPresentationOverlays =
                new Dictionary<string, object>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static readonly string AurelionSpanishDescription =
            "Aurelion Sol es un superviviente poco convencional con herramientas para combatir " +
            "tanto a corta como a larga distancia, aunque prefiere mantenerse a media distancia " +
            "alrededor del radio expandido de sus estrellas.<style=cSub>\r\n\r\n" +
            "< ! > El aliento de Llamarada Solar alcanza el radio expandido; una buena posición " +
            "puede aumentar enormemente tu daño.<style=cSub>\r\n\r\n" +
            "< ! > Explosión Astral es excelente para frenar enemigos cuerpo a cuerpo mientras " +
            "te reposicionas, y también funciona como herramienta de combo e iniciación. Su " +
            "distancia máxima coincide con el radio expandido.<style=cSub>\r\n\r\n" +
            "< ! > Cometa Legendario sirve tanto para llegar rápido a puntos de interés como " +
            "para entrar o salir de combate junto a Explosión Astral.<style=cSub>\r\n\r\n" +
            "< ! > Voz de Luz tiene un enfriamiento alto, pero puede reagrupar enemigos y causar " +
            "un gran daño a larga distancia.<style=cSub>\r\n\r\n" +
            "< ! > Aprender cuándo combinar las habilidades de Aurelion Sol con el impacto de " +
            "una estrella mejora mucho su consistencia.";


        // Lore curado por USU. Aurelion 2.0.10 no aporta una entrada propia,
        // así que usamos un fallback original inspirado en el canon oficial
        // del personaje, con una longitud cercana a la mediana del Logbook
        // Vanilla/DLC medida en runtime.
        private static readonly string AurelionSpanishLore =
            "Transcripción recuperada de un registro astronómico sin procedencia verificada.\n\n" +
            "Durante siglos, algunas culturas observaron estrellas que parecían cambiar de lugar " +
            "sin seguir ninguna órbita conocida. Los registros más antiguos no las describen como " +
            "fenómenos, sino como obra: luces encendidas deliberadamente en el vacío por una " +
            "inteligencia capaz de tratar constelaciones enteras como simples bocetos.\n\n" +
            "El nombre que acompaña a esas historias es Aurelion Sol.\n\n" +
            "En Runaterra, el Forjador de Estrellas fue recibido como una divinidad. Los habitantes " +
            "de Targon le ofrecieron una corona como símbolo de veneración. Cuando la aceptó, " +
            "descubrió demasiado tarde que aquello no era un regalo, sino una cadena. Su poder quedó " +
            "ligado a quienes lo habían engañado, obligado a servir a un imperio diminuto bajo un " +
            "cielo que él mismo podía remodelar.\n\n" +
            "La distancia no apagó su orgullo. Tampoco su paciencia.\n\n" +
            "Ahora esas ataduras se debilitan. Cada estrella creada, cada mundo observado y cada era " +
            "soportada parecen haber alimentado una única certeza: algún día será libre.\n\n" +
            "Y cuando llegue ese día, quizá Targon descubra cuánto pesa una estrella al caer.";


        private static readonly string AurelionEnglishLore =
            "Transcript recovered from an astronomical record of unverified origin.\n\n" +
            "For centuries, some cultures observed stars that seemed to change position without " +
            "following any known orbit. The oldest records describe them not as phenomena, but as " +
            "craftsmanship: lights deliberately kindled in the void by an intelligence capable of " +
            "treating entire constellations as little more than sketches.\n\n" +
            "The name attached to those stories is Aurelion Sol.\n\n" +
            "On Runeterra, the Star Forger was welcomed as a divinity. The people of Targon offered " +
            "him a crown as a symbol of reverence. When he accepted it, he discovered too late that " +
            "it was not a gift, but a chain. His power became bound to those who had deceived him, " +
            "forcing him to serve a tiny empire beneath a sky he himself could reshape.\n\n" +
            "Distance did not diminish his pride. Nor his patience.\n\n" +
            "Now those bindings are weakening. Every star forged, every world observed, and every " +
            "age endured seems to have fed a single certainty: one day, he will be free.\n\n" +
            "And when that day comes, perhaps Targon will learn how heavy a falling star can be.";


        private static readonly string AurelionSpanishPassiveKeyword =
            BuildAurelionPassiveKeywordSpanish();

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

            // Reparaciones EN: sólo cuando el token visible está realmente
            // vacío/sin resolver. El inglés válido del creador no se toca.
            RegisterAurelionEnglishRepairs();

            // ES curado + integración nativa de unlocks/skins/Logbook.
            RegisterAurelionSpanishTokens();
            RegisterAurelionUnlockAchievementSpanish(
                survivor
            );
            RegisterAurelionSkinAchievementsSpanish();

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | " +
                "AurelionSolBody | es-419 / es-ES + EN repair + lore"
            );
        }


        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "El Forjador de Estrellas"
                : fallback ?? "";
        }


        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? AurelionSpanishDescription
                : fallback ?? "";
        }


        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return AurelionSpanishLore;
            }

            if (
                IsEnglishLanguage() &&
                IsMissingLoreText(fallback)
            )
            {
                return AurelionEnglishLore;
            }

            return fallback ?? "";
        }


        public override string ResolveOriginalMissionName(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Grilletes rotos"
                : fallback ?? "";
        }


        public override string ResolveOriginalMissionDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? BuildAurelionUnlockDescriptionSpanish()
                : fallback ?? "";
        }


        /// <summary>
        /// La ficha de USU puede traducir la misión usando el profile, pero
        /// Character Select construye su tooltip directamente desde los
        /// tokens del AchievementDef original. Registramos esos mismos tokens
        /// de forma manual para que ambas interfaces muestren exactamente la
        /// misma localización sin interceptar Language globalmente.
        /// </summary>
        private static void RegisterAurelionUnlockAchievementSpanish(
            SurvivorInfo survivor
        )
        {
            UnlockableDef unlockable =
                survivor?.SurvivorDef?.unlockableDef;

            if (unlockable == null)
            {
                return;
            }

            AchievementDef achievement =
                FindAchievementForUnlockable(
                    unlockable
                );

            if (achievement == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(achievement.nameToken))
            {
                RegisterSpanishToken(
                    achievement.nameToken,
                    "Grilletes rotos"
                );
            }

            if (!string.IsNullOrWhiteSpace(achievement.descriptionToken))
            {
                RegisterSpanishToken(
                    achievement.descriptionToken,
                    BuildAurelionUnlockDescriptionSpanish()
                );
            }
        }


        /// <summary>
        /// Aurelion 2.0.10 registra tres unlockables de skins cuyos tooltips
        /// de logro quedan en inglés cuando el juego está en español.
        ///
        /// No dependemos de nombres internos de clases del mod: recorremos
        /// los AchievementDef y sólo tocamos los que entregan un unlockable
        /// AURELION_SKIN*. Si el creador ya registró español, LanguageAPI.Add
        /// conserva su traducción y USU queda como fallback.
        /// </summary>
        private static void RegisterAurelionSkinAchievementsSpanish()
        {
            try
            {
                FieldInfo field =
                    typeof(AchievementManager).GetField(
                        "achievementDefs",
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                object definitions =
                    field?.GetValue(null);

                if (!(definitions is IEnumerable enumerable))
                {
                    return;
                }

                foreach (object element in enumerable)
                {
                    if (!(element is AchievementDef achievement))
                    {
                        continue;
                    }

                    string rewardIdentifier =
                        achievement.unlockableRewardIdentifier ?? "";

                    if (
                        rewardIdentifier.IndexOf(
                            "AURELION_SKIN",
                            StringComparison.OrdinalIgnoreCase
                        ) < 0
                    )
                    {
                        continue;
                    }

                    string currentName =
                        !string.IsNullOrWhiteSpace(achievement.nameToken)
                            ? Language.GetString(achievement.nameToken) ?? ""
                            : "";

                    string spanishName = "";
                    string spanishDescription = "";

                    if (
                        currentName.IndexOf(
                            "Mastery",
                            StringComparison.OrdinalIgnoreCase
                        ) >= 0
                    )
                    {
                        spanishName =
                            "Aurelion: Maestría";
                        spanishDescription =
                            "Como Aurelion, completa el juego o oblítérate en Monzón.";
                    }
                    else if (
                        currentName.IndexOf(
                            "Carrier Ship",
                            StringComparison.OrdinalIgnoreCase
                        ) >= 0
                    )
                    {
                        spanishName =
                            "Aurelion: Nave nodriza";
                        spanishDescription =
                            "Como Aurelion, mantén 10 drones o más al mismo tiempo.";
                    }
                    else if (
                        currentName.IndexOf(
                            "Asserting Dominance",
                            StringComparison.OrdinalIgnoreCase
                        ) >= 0
                    )
                    {
                        spanishName =
                            "Aurelion: Dominio absoluto";
                        spanishDescription =
                            "Como Aurelion, derrota a Mithrix y al Voidling en Monzón.";
                    }

                    if (
                        !string.IsNullOrWhiteSpace(spanishName) &&
                        !string.IsNullOrWhiteSpace(achievement.nameToken)
                    )
                    {
                        RegisterSpanishToken(
                            achievement.nameToken,
                            spanishName
                        );
                    }

                    if (
                        !string.IsNullOrWhiteSpace(spanishDescription) &&
                        !string.IsNullOrWhiteSpace(achievement.descriptionToken)
                    )
                    {
                        RegisterSpanishToken(
                            achievement.descriptionToken,
                            spanishDescription
                        );
                    }
                }
            }
            catch
            {
                // Si un mod registra achievements fuera del flujo estándar,
                // se conserva el texto original sin afectar el resto de USU.
            }
        }


        private static AchievementDef FindAchievementForUnlockable(
            UnlockableDef unlockable
        )
        {
            if (
                unlockable == null ||
                string.IsNullOrWhiteSpace(unlockable.cachedName)
            )
            {
                return null;
            }

            try
            {
                FieldInfo field =
                    typeof(AchievementManager).GetField(
                        "achievementDefs",
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                object definitions =
                    field?.GetValue(null);

                if (!(definitions is IEnumerable enumerable))
                {
                    return null;
                }

                foreach (object element in enumerable)
                {
                    if (!(element is AchievementDef achievement))
                    {
                        continue;
                    }

                    if (
                        string.Equals(
                            achievement.unlockableRewardIdentifier,
                            unlockable.cachedName,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        return achievement;
                    }
                }
            }
            catch
            {
                // Si un mod registra achievements fuera del flujo estándar,
                // la ficha de USU seguirá usando su traducción manual propia.
            }

            return null;
        }

        /// <summary>
        /// Reparación visual del idioma inglés de Aurelion.
        ///
        /// Aurelion 2.x utiliza algunos tokens que en determinadas versiones
        /// no tienen texto registrado (por ejemplo varios nombres de skin).
        /// USU sólo aporta el original inglés cuando el token está realmente
        /// ausente en el sistema de idioma actual. Si el creador ya entrega
        /// texto válido, no se registra nada y su versión queda intacta.
        ///
        /// También incluimos los tokens visibles principales de habilidades
        /// como red de seguridad. En la versión actual ya existen, por lo que
        /// normalmente estas llamadas no harán nada.
        /// </summary>
        private static void RegisterAurelionEnglishRepairs()
        {
            // Survivor / presentación.
            RegisterEnglishRepairIfMissing(
                "AURELION_NAME",
                "Aurelion Sol"
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_SUBTITLE",
                "The Star Forger"
            );

            // Logbook nativo. RoR2 deriva el lore del baseNameToken
            // AURELION_NAME como AURELION_LORE. El mod de Aurelion no
            // registra este token, por eso el survivor puede existir en
            // Character Select pero no tener contenido de Diario.
            // Sólo lo reparamos si el inglés original está realmente ausente.
            RegisterEnglishRepairIfMissing(
                "AURELION_LORE",
                AurelionEnglishLore
            );

            // Skins. Éste es el caso que ya observamos visualmente: algunas
            // versiones dejan tokens AURELIONBODY_* visibles en la UI.
            RegisterEnglishRepairIfMissing(
                "AURELIONBODY_DEFAULT_SKIN_NAME",
                "Default"
            );

            RegisterEnglishRepairIfMissing(
                "AURELIONBODY_SKIN01_SKIN_NAME",
                "Ashen Lord"
            );

            RegisterEnglishRepairIfMissing(
                "AURELIONBODY_SKIN02_SKIN_NAME",
                "Mecha"
            );

            RegisterEnglishRepairIfMissing(
                "AURELIONBODY_SKIN03_SKIN_NAME",
                "Storm Dragon"
            );

            RegisterEnglishRepairIfMissing(
                "AURELIONBODY_SKIN04_SKIN_NAME",
                "PROJECT"
            );

            RegisterEnglishRepairIfMissing(
                "AURELIONBODY_SKIN05_SKIN_NAME",
                "Inkshadow"
            );

            RegisterEnglishRepairIfMissing(
                "AURELIONBODY_SKIN06_SKIN_NAME",
                "Porcelain Protector"
            );

            // Pasivas / habilidades visibles. No reemplazan el original:
            // sólo actúan si el token aparece sin resolver.
            RegisterEnglishRepairIfMissing(
                "AURELION_PASSIVE_NAME",
                "Center of the Universe"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_PASSIVE_DESCRIPTION",
                "Aurelion Sol can fly at will and is permanently orbited by 3 stars." +
                Environment.NewLine +
                "<style=cKeywordName>Celestial Expansion</style><style=cSub>"
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_PASSIVE2_NAME",
                "Cosmic Creator"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_PASSIVE2_DESCRIPTION",
                "Aurelion Sol can fly at will and his abilities break his foes down to " +
                "<style=cIsUtility>stardust</style>, stacks of <style=cIsUtility>stardust</style> " +
                "<style=cIsDamage>empower</style> his abilities in various ways. " +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>"
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_M1",
                "Solar Flare"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_M1_DESCRIPTION",
                "<style=cIsDamage>Ignite</style>. Aurelion Sol exhales a cluster of starfire close to him, " +
                "dealing <style=cIsDamage>250% damage</style>. This attack also sends a single, weaker but " +
                "long range projectile where aimed dealing <style=cIsDamage>200% damage</style>." +
                Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "Hits with the fire breath collect <style=cIsUtility>stardust</style>. Attack speed and damage " +
                "is increased with <style=cIsUtility>stardust</style>."
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_M1NEW",
                "Breath of Light"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_M1NEW_DESCRIPTION",
                "Exhale a beam of starfire, dealing <style=cIsDamage>20% damage</style> every 0.125 seconds " +
                "to the first enemy hit, with reduced splash damage. Every second the beam hits the same target, " +
                "it'll burst, dealing <style=cIsDamage>150% damage</style> in an aoe. Attack speed increases damage." +
                Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "The bursts collect <style=cIsUtility>stardust</style>. Stardust increases the burst's damage."
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_M2",
                "Starsurge"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_M2_DESCRIPTION",
                "Aurelion Sol channels a newborn star which travels in the target direction. When recast or it " +
                "strays too far away from Aurelion it detonates, <style=cIsDamage>stunning</style> and dealing " +
                "<style=cIsDamage>400%</style>, increasing with distance travelled." +
                Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "Damage with distance is increased."
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_M2NEW",
                "Singularity"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_M2NEW_DESCRIPTION",
                "Aurelion Sol conjures a black hole, dragging enemies towards the centre and dealing " +
                "<style=cIsDamage>20% damage</style> every 0.25 seconds for 5 seconds, increased to 30% in the " +
                "centre. Enemies that die in the black hole are counted as Aurelion Sol killing them." +
                Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "Enemies that die in the black hole grant 1 stardust each. The effect radius increases with " +
                "<style=cIsUtility>stardust</style>."
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_UTIL",
                "Comet of Legend"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_UTIL_DESCRIPTION",
                "Aurelion Sol flies in a burst of speed in the direction aimed, the speed and distance scales " +
                "with <style=cIsUtility>movement speed</style>. If Starsurge is currently active or is cast during " +
                "Comet of Legend, it will center itself on Aurelion Sol while he travels. Can be cancelled at any " +
                "time by pressing Jump." + Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "Maximum travel distance is increased further with <style=cIsUtility>stardust</style>."
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_UTILNEW",
                "Dragonflight"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_UTILNEW_DESCRIPTION",
                "Dash after a brief, while dashing, gain <style=cIsDamage>+20% damage</style>. The trajectory can " +
                "be influenced with directional keys, can be cancelled early by pressing jump." +
                Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "Duration increases with <style=cIsUtility>stardust</style>."
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_SPEC",
                "Voice of Light"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_SPEC_DESCRIPTION",
                "Aurelion Sol <style=cIsUtility>pulls</style> all nearby enemies to Center of the universe's " +
                "expanded radius then exhales a beam of starfire in the target direction dealing " +
                "<style=cIsDamage>1000% damage</style>." + Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "Collects stardust on kill. Damage and length of the blast is increased with " +
                "<style=cIsUtility>stardust</style>."
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_SPECNEW",
                "Falling Star"
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_SPECNEW_DESCRIPTION",
                "Call down a star to impact the target location after 1.25 seconds, dealing " +
                "<style=cIsDamage>500% damage</style> to enemies hit and <style=cIsDamage>stunning</style> them " +
                "for <style=cIsUtility>1s</style>." + Environment.NewLine + Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "Falling Star grants 1 <style=cIsUtility>stardust</style> for each enemy hit. Damage and effect " +
                "radius increase with <style=cIsUtility>stardust</style>, at 900 <style=cIsUtility>stardust</style> " +
                "this skill permanently becomes <style=cKeywordName><style=cIsDamage>The Skies Descend</style></style><style=cSub>"
            );

            RegisterEnglishRepairIfMissing(
                "AURELION_SPECNEW2",
                "The Skies Descend"
            );

            const string skiesDescendEnglish =
                "Aurelion Sol summons an enormous star to impact the target location after 2 seconds, dealing " +
                "<style=cIsDamage>3000% damage</style> damage to enemies hit and <style=cIsDamage>stunning</style> " +
                "them for <style=cIsUtility>1s</style>, upon impact, a massive shockwave rapidly expands through " +
                "the map, dealing <style=cIsDamage>1800% damage</style> to anything caught and slowing them." +
                "\n<style=cKeywordName><style=cIsDamage>Cosmic Creator</style></style><style=cSub>" +
                "Effect radius, shockwave travel distance and damage are all increased with " +
                "<style=cIsUtility>stardust</style>.";

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_SPECNEW2_DESCRIPTION",
                skiesDescendEnglish
            );

            RegisterEnglishSkillDescriptionPresentation(
                "AURELION_SPECNEW2_DESCRIPTION_KEYWORD",
                "<style=cKeywordName><style=cIsDamage>The Skies Descend</style></style><style=cSub>" +
                skiesDescendEnglish
            );
        }


        private static void RegisterEnglishRepairIfMissing(
            string token,
            string english
        )
        {
            if (
                string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(english) ||
                !IsTokenVisiblyMissing(token)
            )
            {
                return;
            }

            LanguageAPI.Add(
                token,
                english,
                "en"
            );
        }


        private static void RegisterEnglishSkillDescriptionPresentation(
            string token,
            string english
        )
        {
            if (
                string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(english)
            )
            {
                return;
            }

            // Conserva la reparación anterior para versiones donde el token
            // realmente falte.
            RegisterEnglishRepairIfMissing(
                token,
                english
            );

            if (
                AurelionEnglishSkillPresentationOverlays.ContainsKey(token)
            )
            {
                return;
            }

            // Aurelion 2.0.10 usa la UI estándar de Loadout. Antes de
            // aplicar formato recuperamos el texto que el juego ya resuelve
            // para el token. Así no reescribimos el contenido del creador:
            // sólo añadimos presentación. El texto de reparación se usa como
            // fallback únicamente si el token realmente no resuelve.
            string presentationText =
                GetResolvedTokenTextOrFallback(
                    token,
                    english
                );

            object overlay =
                LanguageAPI.AddOverlay(
                    token,
                    FormatLongSkillText(
                        presentationText,
                        true
                    ),
                    "en"
                );

            if (overlay != null)
            {
                AurelionEnglishSkillPresentationOverlays[token] = overlay;
            }
        }


        private static string GetResolvedTokenTextOrFallback(
            string token,
            string fallback
        )
        {
            try
            {
                string resolved =
                    Language.GetString(token);

                if (
                    !string.IsNullOrWhiteSpace(resolved) &&
                    !string.Equals(
                        resolved,
                        token,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return resolved;
                }
            }
            catch
            {
                // La reparación EN existente sigue siendo el fallback seguro.
            }

            return fallback ?? "";
        }


        private static bool IsTokenVisiblyMissing(
            string token
        )
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                string current =
                    Language.GetString(token) ?? "";

                return
                    string.IsNullOrWhiteSpace(current) ||
                    string.Equals(
                        current.Trim(),
                        token.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    );
            }
            catch
            {
                return false;
            }
        }



        private static void RegisterAurelionSpanishTokens()
        {
            // Survivor
            RegisterSpanishToken(
                "AURELION_SUBTITLE",
                "El Forjador de Estrellas"
            );

            RegisterSpanishToken(
                "AURELION_DESCRIPTION",
                AurelionSpanishDescription
            );

            // Lore nativo del Logbook. Mantiene el mismo contenido que
            // muestra la sección Notas de USU, pero ahora existe también
            // como token que el Diario de RoR2 puede consumir.
            RegisterSpanishToken(
                "AURELION_LORE",
                AurelionSpanishLore
            );

            RegisterSpanishToken(
                "AURELION_OUTRO",
                "..y así partió, con la libertad a la vista. Targon es el siguiente."
            );

            RegisterSpanishToken(
                "AURELION_FAIL",
                "..y así desapareció, esclavo de los dioses para siempre."
            );

            // Skins. Aurelion 2.0.10 utiliza siete nameTokens; varios no
            // tienen texto registrado por el propio mod, por eso en la UI
            // podían aparecer literalmente como AURELIONBODY_*_SKIN_NAME.
            RegisterSpanishToken(
                "AURELIONBODY_DEFAULT_SKIN_NAME",
                "Predeterminado"
            );

            RegisterSpanishToken(
                "AURELIONBODY_SKIN01_SKIN_NAME",
                "Señor de las Cenizas"
            );

            RegisterSpanishToken(
                "AURELIONBODY_SKIN02_SKIN_NAME",
                "Mecha"
            );

            RegisterSpanishToken(
                "AURELIONBODY_SKIN03_SKIN_NAME",
                "Dragón de la Tormenta"
            );

            RegisterSpanishToken(
                "AURELIONBODY_SKIN04_SKIN_NAME",
                "PROJECT"
            );

            RegisterSpanishToken(
                "AURELIONBODY_SKIN05_SKIN_NAME",
                "Tinta Sombría"
            );

            RegisterSpanishToken(
                "AURELIONBODY_SKIN06_SKIN_NAME",
                "Protector de Porcelana"
            );

            // Pasivas
            RegisterSpanishToken(
                "AURELION_PASSIVE_NAME",
                "Centro del Universo"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_PASSIVE_DESCRIPTION",
                "Aurelion Sol puede volar a voluntad y está orbitado permanentemente por 3 estrellas." +
                Environment.NewLine +
                "<style=cKeywordName>Expansión Celestial</style><style=cSub>"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_PASSIVE_KEYWORD",
                AurelionSpanishPassiveKeyword
            );

            RegisterSpanishToken(
                "AURELION_PASSIVE2_NAME",
                "Creador Cósmico"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_PASSIVE2_DESCRIPTION",
                "Aurelion Sol puede volar a voluntad y sus habilidades reducen a sus enemigos a " +
                "<style=cIsUtility>Polvo Estelar</style>. Las acumulaciones de " +
                "<style=cIsUtility>Polvo Estelar</style> <style=cIsDamage>potencian</style> " +
                "sus habilidades de distintas formas. " +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>"
            );

            // Primarias
            RegisterSpanishToken(
                "AURELION_M1",
                "Llamarada Solar"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_M1_DESCRIPTION",
                "<style=cIsDamage>Incendia</style>. Aurelion Sol exhala fuego estelar a corta " +
                "distancia e inflige <style=cIsDamage>250% de daño</style>. Además dispara hacia " +
                "donde apuntas un proyectil más débil y de largo alcance que inflige " +
                "<style=cIsDamage>200% de daño</style>." +
                Environment.NewLine +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "Los impactos del aliento generan <style=cIsUtility>Polvo Estelar</style>. " +
                "La velocidad de ataque y el daño aumentan con el " +
                "<style=cIsUtility>Polvo Estelar</style>."
            );

            RegisterSpanishToken(
                "AURELION_M1NEW",
                "Aliento Luminoso"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_M1NEW_DESCRIPTION",
                "Exhala un rayo de fuego estelar que inflige <style=cIsDamage>20% de daño</style> " +
                "cada 0,125 s al primer enemigo alcanzado, con daño de área reducido. Cada segundo " +
                "sobre el mismo objetivo, el rayo estalla e inflige <style=cIsDamage>150% de daño</style> " +
                "en área. La velocidad de ataque aumenta el daño." +
                Environment.NewLine +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "Las explosiones generan <style=cIsUtility>Polvo Estelar</style>. El " +
                "<style=cIsUtility>Polvo Estelar</style> aumenta el daño de las explosiones."
            );

            // Secundarias
            RegisterSpanishToken(
                "AURELION_M2",
                "Explosión Astral"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_M2_DESCRIPTION",
                "Aurelion Sol canaliza una estrella recién nacida que viaja hacia donde apuntas. " +
                "Al reactivar la habilidad o alejarse demasiado, detona, " +
                "<style=cIsDamage>aturde</style> e inflige <style=cIsDamage>400% de daño</style>, " +
                "que aumenta con la distancia recorrida." +
                Environment.NewLine +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "El aumento de daño por distancia es mayor."
            );

            RegisterSpanishToken(
                "AURELION_M2NEW",
                "Singularidad"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_M2NEW_DESCRIPTION",
                "Aurelion Sol crea un agujero negro que atrae enemigos al centro e inflige " +
                "<style=cIsDamage>20% de daño</style> cada 0,25 s durante 5 s, aumentado a 30% " +
                "en el centro. Los enemigos que mueren dentro cuentan como bajas de Aurelion Sol." +
                Environment.NewLine +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "Cada enemigo que muere dentro otorga 1 de <style=cIsUtility>Polvo Estelar</style>. " +
                "El radio aumenta con el <style=cIsUtility>Polvo Estelar</style>."
            );

            // Utilidades
            RegisterSpanishToken(
                "AURELION_UTIL",
                "Cometa Legendario"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_UTIL_DESCRIPTION",
                "Aurelion Sol acelera en la dirección apuntada; la velocidad y distancia escalan " +
                "con la <style=cIsUtility>velocidad de movimiento</style>. Si Explosión Astral está " +
                "activa o se lanza durante Cometa Legendario, se centra en Aurelion Sol mientras " +
                "viaja. Puede cancelarse en cualquier momento pulsando Saltar." +
                Environment.NewLine +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "La distancia máxima aumenta aún más con el " +
                "<style=cIsUtility>Polvo Estelar</style>."
            );

            RegisterSpanishToken(
                "AURELION_UTILNEW",
                "Vuelo Dracónico"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_UTILNEW_DESCRIPTION",
                "Tras una breve preparación, embiste hacia delante. Durante el impulso obtienes " +
                "<style=cIsDamage>+20% de daño</style>. Puedes influir en la trayectoria con las " +
                "teclas de dirección y cancelar antes pulsando Saltar." +
                Environment.NewLine +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "La duración aumenta con el <style=cIsUtility>Polvo Estelar</style>."
            );

            // Especiales
            RegisterSpanishToken(
                "AURELION_SPEC",
                "Voz de Luz"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_SPEC_DESCRIPTION",
                "Aurelion Sol <style=cIsUtility>atrae</style> a los enemigos cercanos hacia el " +
                "radio expandido de Centro del Universo y luego exhala un rayo de fuego estelar " +
                "hacia donde apuntas que inflige <style=cIsDamage>1000% de daño</style>." +
                Environment.NewLine +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "Genera <style=cIsUtility>Polvo Estelar</style> al matar. El daño y la longitud " +
                "del rayo aumentan con el <style=cIsUtility>Polvo Estelar</style>."
            );

            RegisterSpanishToken(
                "AURELION_SPECNEW",
                "Estrella Fugaz"
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_SPECNEW_DESCRIPTION",
                "Invoca una estrella que impacta la zona objetivo tras 1,25 s, inflige " +
                "<style=cIsDamage>500% de daño</style> y <style=cIsDamage>aturde</style> durante " +
                "<style=cIsUtility>1 s</style>." +
                Environment.NewLine +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "Estrella Fugaz otorga 1 de <style=cIsUtility>Polvo Estelar</style> por cada enemigo " +
                "alcanzado. El daño y el radio aumentan con el <style=cIsUtility>Polvo Estelar</style>; " +
                "con 900, esta habilidad se convierte permanentemente en " +
                "<style=cKeywordName><style=cIsDamage>Descenso Celestial</style></style><style=cSub>"
            );

            RegisterSpanishToken(
                "AURELION_SPECNEW2",
                "Descenso Celestial"
            );

            string skiesDescendDescription =
                "Aurelion Sol invoca una estrella enorme que impacta la zona objetivo tras 2 s, " +
                "inflige <style=cIsDamage>3000% de daño</style> y <style=cIsDamage>aturde</style> " +
                "durante <style=cIsUtility>1 s</style>. Al impactar, una gran onda expansiva recorre " +
                "rápidamente el mapa, inflige <style=cIsDamage>1800% de daño</style> y ralentiza " +
                "a todo lo alcanzado." +
                Environment.NewLine +
                "<style=cKeywordName><style=cIsDamage>Creador Cósmico</style></style><style=cSub>" +
                "El radio, la distancia de la onda y el daño aumentan con el " +
                "<style=cIsUtility>Polvo Estelar</style>.";

            RegisterSpanishSkillDescriptionToken(
                "AURELION_SPECNEW2_DESCRIPTION",
                skiesDescendDescription
            );

            RegisterSpanishSkillDescriptionToken(
                "AURELION_SPECNEW2_DESCRIPTION_KEYWORD",
                "<style=cKeywordName><style=cIsDamage>Descenso Celestial</style></style><style=cSub>" +
                skiesDescendDescription
            );
        }


        private static void RegisterSpanishSkillDescriptionToken(
            string token,
            string spanish
        )
        {
            RegisterSpanishToken(
                token,
                FormatLongSkillText(
                    spanish,
                    true
                )
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

            // R2API Language conserva la primera entrada registrada para un
            // token+idioma. Como los mods creadores cargan antes de
            // RoR2Application.onLoad, una localización española oficial del
            // creador ya existente no será reemplazada por USU.
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


        private static string BuildAurelionPassiveKeywordSpanish()
        {
            string source =
                Language.GetString(
                    "AURELION_PASSIVE_KEYWORD"
                ) ?? "";

            string keyName =
                ExtractBetween(
                    source,
                    "Pressing ",
                    " toggles"
                );

            if (string.IsNullOrWhiteSpace(keyName))
            {
                keyName =
                    "la tecla configurada";
            }

            return
                "<style=cKeywordName>Expansión Celestial</style><style=cSub>" +
                "Al pulsar " + keyName +
                ", alternas las estrellas entre alcance cercano y expandido. " +
                "Cuando una estrella golpea a un enemigo inflige " +
                "<style=cIsDamage>500% de daño</style>. Su velocidad de rotación " +
                "aumenta con la velocidad de ataque.";
        }


        private static string ExtractBetween(
            string source,
            string startMarker,
            string endMarker
        )
        {
            if (
                string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(startMarker) ||
                string.IsNullOrWhiteSpace(endMarker)
            )
            {
                return "";
            }

            int start =
                source.IndexOf(
                    startMarker,
                    StringComparison.Ordinal
                );

            if (start < 0)
            {
                return "";
            }

            start += startMarker.Length;

            int end =
                source.IndexOf(
                    endMarker,
                    start,
                    StringComparison.Ordinal
                );

            if (end <= start)
            {
                return "";
            }

            return
                source.Substring(
                    start,
                    end - start
                ).Trim();
        }

        private static string BuildAurelionUnlockDescriptionSpanish()
        {
            string sceneName =
                ResolveLocalizedSceneName(
                    "limbo",
                    "A Moment, Whole"
                );

            return
                "Visita " + sceneName +
                " y libera a Aurelion Sol.";
        }


        private static string ResolveLocalizedSceneName(
            string sceneName,
            string fallback
        )
        {
            try
            {
                SceneDef sceneDef =
                    SceneCatalog.GetSceneDefFromSceneName(
                        sceneName
                    );

                if (
                    sceneDef != null &&
                    !string.IsNullOrWhiteSpace(sceneDef.nameToken)
                )
                {
                    string localized =
                        Language.GetString(
                            sceneDef.nameToken
                        );

                    if (
                        !string.IsNullOrWhiteSpace(localized) &&
                        !string.Equals(
                            localized,
                            sceneDef.nameToken,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        return localized;
                    }
                }
            }
            catch
            {
                // Si SceneCatalog todavía no está disponible, conservamos
                // el nombre propio original en vez de inventar una traducción.
            }

            return fallback ?? "";
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
