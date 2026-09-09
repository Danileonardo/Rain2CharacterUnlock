using System;
using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Fachada compatible de localización para survivors de terceros.
    ///
    /// Regla:
    /// - El inglés/default conserva el texto que entrega el mod creador.
    /// - es-419 / es-ES usan la traducción manual mantenida y revisada por USU.
    /// - Si USU todavía no tiene traducción de un campo, se conserva el original.
    ///
    /// Este sistema NO modifica los assemblies de otros mods ni sus archivos.
    /// Las Definitions migradas concentran su propio contenido; los demás
    /// personajes siguen usando temporalmente la tabla legacy de este archivo.
    /// </summary>
    internal static class ModdedSurvivorLocalization
    {
        private sealed class Translation
        {
            // Propiedades opcionales: algunos survivors sólo tienen una parte
            // de sus textos revisados por USU. Usar propiedades evita que el
            // compilador marque como CS0649 los campos todavía no poblados
            // (por ejemplo SpanishLore) sin cambiar el comportamiento.
            public string BodyName { get; set; }
            public string SpanishSubtitle { get; set; }
            public string SpanishDescription { get; set; }
            public string SpanishLore { get; set; }
            public string EnglishLore { get; set; }
            public string SpanishOriginalMissionName { get; set; }
            public string SpanishOriginalMissionDescription { get; set; }
        }


        private static readonly Dictionary<string, Translation> translations =
            new Dictionary<string, Translation>(
                StringComparer.OrdinalIgnoreCase
            )
            {
                // Las traducciones de terceros se mantienen de forma manual
                // y revisada por USU. Si un texto no está cubierto aquí, se
                // conserva el original entregado por el mod creador.
                {
                    "EnforcerBody",
                    new Translation
                    {
                        BodyName = "EnforcerBody",
                        SpanishSubtitle = "Agente antidisturbios",
                        SpanishDescription =
                            "Enforcer es un superviviente resistente de corto alcance que combina " +
                            "escopeta, control de masas y un escudo frontal.\n\n" +
                            "< ! > Protect and Serve bloquea el daño que llega de frente, pero limita tu movilidad.\n\n" +
                            "< ! > Riot Shotgun es especialmente eficaz a corta distancia y puede atravesar enemigos.\n\n" +
                            "< ! > Usa tus habilidades de control para mantener a los enemigos delante de tu escudo."
                    }
                },
                {
                    "NemesisEnforcerBody",
                    new Translation
                    {
                        BodyName = "NemesisEnforcerBody",
                        SpanishSubtitle = "Ejecutor Némesis",
                        SpanishDescription =
                            "Nemesis Enforcer cambia la defensa metódica por una ofensiva pesada y constante.\n\n" +
                            "< ! > Mantén la presión sobre grupos de enemigos y aprovecha sus herramientas de control.\n\n" +
                            "< ! > Su tamaño y potencia favorecen los enfrentamientos directos, pero exigen cuidar el posicionamiento."
                    }
                },
                {
                    "JhinBody",
                    new Translation
                    {
                        BodyName = "JhinBody",
                        SpanishSubtitle = "El Virtuoso",
                        SpanishDescription =
                            "Jhin está diseñado para un estilo de juego deliberado, con pocos ataques " +
                            "pero golpes de gran daño. Su daño escala muy bien, aunque dispone de menos " +
                            "herramientas contra grupos numerosos.\n\n" +
                            "< ! > La velocidad de ataque adicional se convierte en daño y movilidad en lugar de aumentar su cadencia.\n\n" +
                            "< ! > Whisper usa un cargador de cuatro disparos; planifica cada bala y cada recarga.\n\n" +
                            "< ! > Aprovecha los críticos y el control de distancia para preparar ejecuciones."
                    }
                },
                {
                    "MinerBody",
                    new Translation
                    {
                        BodyName = "MinerBody",
                        SpanishSubtitle = "Adicto destructivo a las drogas",
                        SpanishDescription =
                            "Miner es un superviviente cuerpo a cuerpo rápido y extremadamente móvil que " +
                            "prioriza mantener largas cadenas de bajas para acumular cargas de su pasiva.\n\n" +
                            "< ! > Cuando tengas una buena cantidad de Adrenalina, Gouge y Crush serán tus mejores fuentes de daño.\n\n" +
                            "< ! > Cargar Drill Charge sólo afecta al daño infligido. Apunta al suelo o hacia los enemigos para concentrar el daño.\n\n" +
                            "< ! > Puedes pulsar Backblast para recorrer una distancia corta. Mantén el botón para llegar más lejos.\n\n" +
                            "< ! > To The Stars puede infligir mucho daño a enemigos grandes.",
                        SpanishOriginalMissionName = "Subidón de adrenalina",
                        SpanishOriginalMissionDescription =
                            "Abre un cofre legendario."
                    }
                },
                {
                    "RobHunkBody",
                    new Translation
                    {
                        BodyName = "RobHunkBody",
                        SpanishSubtitle = "La Parca",
                        SpanishDescription =
                            "HUNK es un superviviente altamente configurable centrado en armas de fuego, " +
                            "munición y supervivencia bajo presión.\n\n" +
                            "< ! > Cada escenario puede incluir un enemigo infectado; derrotarlo permite obtener una muestra de virus.\n\n" +
                            "< ! > Intercambia muestras en terminales Umbrella para conseguir tarjetas y nuevas oportunidades de equipo.\n\n" +
                            "< ! > Administra la munición y adapta tu equipamiento a la situación antes de quedarte sin recursos."
                    }
                },
                {
                    "RocketSurvivorBody",
                    new Translation
                    {
                        BodyName = "RocketSurvivorBody",
                        SpanishSubtitle = "Especialista en cohetes",
                        SpanishDescription =
                            "Rocket convierte las explosiones en movilidad: dispara a tus pies, conserva el impulso " +
                            "y encadena saltos con cohetes.\n\n" +
                            "< ! > Mantén la dirección de tu trayectoria —o suelta el movimiento— para conservar la velocidad tras un rocket jump.\n\n" +
                            "< ! > El retroceso del lanzacohetes depende de la masa del enemigo.\n\n" +
                            "< ! > Remote Detonator elimina la pérdida de daño por distancia de las explosiones."
                    }
                },
                {
                    "ScoutBody",
                    new Translation
                    {
                        BodyName = "ScoutBody",
                        SpanishSubtitle = "Corredor atómico",
                        SpanishDescription =
                            "Scout es un superviviente extremadamente móvil que puede hacer doble salto y acumula " +
                            "Atomic Core al infligir daño.\n\n" +
                            "< ! > Recibir daño reduce Atomic Core; mantener la ofensiva aumenta tu velocidad de movimiento.\n\n" +
                            "< ! > Alterna entre armas a distancia y golpes rápidos para mantener el ritmo.\n\n" +
                            "< ! > Los Atomic Crits aumentan el daño y aplican Weaken."
                    }
                },
                {
                    "SpyBody",
                    new Translation
                    {
                        BodyName = "SpyBody",
                        SpanishSubtitle = "Asesino infiltrado",
                        SpanishDescription =
                            "Spy es un asesino cuerpo a cuerpo frágil que destaca al eliminar objetivos prioritarios con precisión.\n\n" +
                            "< ! > Backstab permite asestar críticos por la espalda y ejecutar instantáneamente a enemigos débiles.\n\n" +
                            "< ! > Busca ángulos seguros antes de comprometerte con un objetivo.\n\n" +
                            "< ! > Usa tus herramientas de engaño y movilidad para entrar, rematar y salir."
                    }
                },
                {
                    "TinkatonBody",
                    new Translation
                    {
                        BodyName = "TinkatonBody",
                        SpanishSubtitle = "La Saqueadora",
                        SpanishDescription =
                            "Tinkaton es un Pokémon inteligente y temerario que usa su enorme martillo para aplastar enemigos " +
                            "y lanzar proyectiles por los aires.\n\n" +
                            "< ! > Aprovecha el alcance y el peso de tu martillo para controlar grupos.\n\n" +
                            "< ! > Pickpocket recompensa un estilo agresivo y orientado a conseguir recursos.\n\n" +
                            "< ! > Sus ataques más pesados requieren colocarte bien antes de comprometerte."
                    }
                },
                {
                    "WooperBody",
                    new Translation
                    {
                        BodyName = "WooperBody",
                        SpanishSubtitle = "El Nadador Sonriente",
                        SpanishDescription =
                            "Wooper es un Pokémon acuático que combina ataques de agua, barro y control de zona.\n\n" +
                            "< ! > Soaked y tus charcos ayudan a mantener presión sobre los enemigos.\n\n" +
                            "< ! > Alterna entre ataques a distancia y movimientos de reposicionamiento para controlar el terreno.\n\n" +
                            "< ! > Aprovecha sus herramientas de agua y tierra según la situación."
                    }
                }
            };



        /// <summary>
        /// Registra únicamente las traducciones manuales de mods que están
        /// realmente instalados. Se ejecuta después del escaneo final de
        /// survivors, cuando los mods creadores ya registraron sus idiomas.
        ///
        /// LanguageAPI.Add no reemplaza una entrada específica de idioma que
        /// ya exista. Por eso, si el creador añade es-419/es-ES, esa versión
        /// conserva prioridad y USU actúa solamente como fallback curado.
        /// </summary>
        public static void RegisterInstalledTranslations(
            IReadOnlyList<SurvivorInfo> installedSurvivors,
            ManualLogSource logger
        )
        {
            if (installedSurvivors == null)
            {
                return;
            }

            // Las Definitions migradas son ahora la autoridad de su propia
            // localización nativa. El resto de survivors continúa usando la
            // tabla legacy de esta fachada hasta migrarse por separado.
            for (int i = 0; i < installedSurvivors.Count; i++)
            {
                SurvivorInfo survivor =
                    installedSurvivors[i];

                if (survivor == null)
                {
                    continue;
                }

                /*
                 * La autoridad para decidir si una Definition puede actuar es
                 * su propio Matches(). Las Definitions normales siguen
                 * exigiendo IsModded=true desde SurvivorDefinition.Matches.
                 *
                 * Esto permite adaptadores explícitos como Moffein-Heretic:
                 * ese mod reutiliza HereticBody/Heretic.asset de RoR2 y, por
                 * tanto, el detector lo clasifica correctamente como oficial.
                 */
                SurvivorDefinition definition =
                    SurvivorDefinitionRegistry.FindFor(
                        survivor
                    );

                if (definition == null)
                {
                    continue;
                }

                definition.RegisterInstalledLocalization(
                    survivor,
                    logger
                );
            }
        }


        public static string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            SurvivorDefinition definition =
                GetDefinition(profile);

            if (definition != null)
            {
                return definition.ResolveSubtitle(
                    profile,
                    fallback
                );
            }

            Translation translation =
                Get(profile);

            return ResolveLegacySpanish(
                profile,
                fallback,
                translation?.SpanishSubtitle
            );
        }


        public static string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            SurvivorDefinition definition =
                GetDefinition(profile);

            if (definition != null)
            {
                return definition.ResolveDescription(
                    profile,
                    fallback
                );
            }

            Translation translation =
                Get(profile);

            return ResolveLegacySpanish(
                profile,
                fallback,
                translation?.SpanishDescription
            );
        }


        public static string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            SurvivorDefinition definition =
                GetDefinition(profile);

            if (definition != null)
            {
                return definition.ResolveLore(
                    profile,
                    fallback
                );
            }

            Translation translation =
                Get(profile);

            if (translation == null)
            {
                return fallback ?? "";
            }

            if (
                IsSpanishLanguage() &&
                !string.IsNullOrWhiteSpace(translation.SpanishLore)
            )
            {
                return translation.SpanishLore;
            }

            return fallback ?? "";
        }


        public static string ResolveOriginalMissionName(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            SurvivorDefinition definition =
                GetDefinition(profile);

            if (definition != null)
            {
                return definition.ResolveOriginalMissionName(
                    profile,
                    fallback
                );
            }

            Translation translation =
                Get(profile);

            return ResolveLegacySpanish(
                profile,
                fallback,
                translation?.SpanishOriginalMissionName
            );
        }


        public static string ResolveOriginalMissionDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            SurvivorDefinition definition =
                GetDefinition(profile);

            if (definition != null)
            {
                return definition.ResolveOriginalMissionDescription(
                    profile,
                    fallback
                );
            }

            Translation translation =
                Get(profile);

            return ResolveLegacySpanish(
                profile,
                fallback,
                translation?.SpanishOriginalMissionDescription
            );
        }


        private static SurvivorDefinition GetDefinition(
            SurvivorContentProfile profile
        )
        {
            if (
                profile == null ||
                string.IsNullOrWhiteSpace(profile.BodyName)
            )
            {
                return null;
            }

            return
                SurvivorDefinitionRegistry.FindByBodyName(
                    profile.BodyName
                );
        }


        private static Translation Get(
            SurvivorContentProfile profile
        )
        {
            if (
                profile == null ||
                string.IsNullOrWhiteSpace(profile.BodyName)
            )
            {
                return null;
            }

            translations.TryGetValue(
                profile.BodyName,
                out Translation translation
            );

            return translation;
        }


        private static string ResolveLegacySpanish(
            SurvivorContentProfile profile,
            string fallback,
            string spanish
        )
        {
            // El texto original del creador sigue siendo la autoridad para
            // cualquier idioma que USU no traduzca de forma explícita.
            if (
                profile == null ||
                string.IsNullOrWhiteSpace(spanish) ||
                !IsSpanishLanguage()
            )
            {
                return fallback ?? "";
            }

            return spanish;
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
    }
}
