using System;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    // Agrega contenido propio en la sección Notas de USU; no registra ni modifica
    // tokens o entradas nativas del Logbook ni contenido de terceros.
    internal static class USUAdditionalDiaries
    {
        // Los dos supervivientes iniciales tienen Diario II USU disponible
        // desde el principio. El Diario I nativo conserva su propia visibilidad.
        internal static string AppendToNotes(SurvivorContentProfile profile, bool hidden, string nativeNotes)
        {
            if (profile == null || hidden) return nativeNotes;
            // The native Logbook bridge may have already appended USU diaries to this
            // localized token. Never append a second copy in the USU browser.
            // Preserve the existing II, III and IV (including their locked states).
            if (!string.IsNullOrEmpty(nativeNotes) &&
                (nativeNotes.IndexOf("Diario III —", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 nativeNotes.IndexOf("Log III —", StringComparison.OrdinalIgnoreCase) >= 0))
                return AppendMercSoraIfNeeded(profile.BodyName, AppendRexKnightIfNeeded(profile.BodyName, AppendMultSonicIfNeeded(profile.BodyName, AppendCommandoBayonettaIfNeeded(profile.BodyName, AppendLoaderRocketerIfNeeded(profile.BodyName, AppendEncounterIVIfNeeded(profile.BodyName, nativeNotes))))));
            // La vista nativa puede traer los diarios incorporados. Nunca añadirlos dos veces.
            // Cuando el Logbook original ya está descubierto, el Diario II también lo está.
            bool nativeUnlocked = SurvivorLogbookResolver.Resolve(profile).CanRevealLore;
            string result = AppendDiaryIIOrLock(profile.BodyName, nativeNotes, nativeUnlocked);
            if (result.IndexOf("Diario III —", StringComparison.OrdinalIgnoreCase) < 0 &&
                result.IndexOf("Log III —", StringComparison.OrdinalIgnoreCase) < 0)
                result = VanillaBroadcastUSUDiary.Append(profile, hidden, result);
            if (string.Equals(profile.BodyName, "EngiBody", StringComparison.OrdinalIgnoreCase))
            {
                bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
                result = USUTinkatonEncounterGate.EngineerDiaryIV(result, es);
            }
            return AppendMercSoraIfNeeded(profile.BodyName, AppendRexKnightIfNeeded(profile.BodyName, AppendMultSonicIfNeeded(profile.BodyName, AppendCommandoBayonettaIfNeeded(profile.BodyName, AppendLoaderRocketerIfNeeded(profile.BodyName, AppendEncounterIVIfNeeded(profile.BodyName, result))))));
        }

        // El Logbook nativo y las Notas usan exactamente la misma condición para el Diario II.
        internal static string AppendToNativeLore(string bodyName, string original)
        {
            if (string.IsNullOrEmpty(bodyName)) return original;
            // Esta ruta solo se utiliza sobre el texto del Logbook del survivor.
            // Si su lore original ya existe y el juego permite leerlo, reconocerlo
            // también como progreso previo sin exigir repetir la victoria con USU.
            bool nativeLorePresent = !string.IsNullOrWhiteSpace(original);
            string result = AppendDiaryIIOrLock(bodyName, original, nativeLorePresent);
            if (result.IndexOf("Diario III —", StringComparison.OrdinalIgnoreCase) < 0 &&
                result.IndexOf("Log III —", StringComparison.OrdinalIgnoreCase) < 0)
                result = VanillaBroadcastUSUDiary.AppendForBody(bodyName, result);
            if (string.Equals(bodyName, "EngiBody", StringComparison.OrdinalIgnoreCase) &&
                result.IndexOf("Diario IV —", StringComparison.OrdinalIgnoreCase) < 0 &&
                result.IndexOf("Diary IV —", StringComparison.OrdinalIgnoreCase) < 0 &&
                result.IndexOf("Prohibido tocar las torretas", StringComparison.OrdinalIgnoreCase) < 0)
            {
                bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
                result = USUTinkatonEncounterGate.EngineerDiaryIV(result, es);
            }
            return AppendMercSoraIfNeeded(bodyName, AppendRexKnightIfNeeded(bodyName, AppendMultSonicIfNeeded(bodyName, AppendCommandoBayonettaIfNeeded(bodyName, AppendLoaderRocketerIfNeeded(bodyName, AppendBanditIVIfNeeded(bodyName, result))))));
        }

        private static string AppendEncounterIVIfNeeded(string bodyName, string text)
        {
            string result = AppendBanditIVIfNeeded(bodyName, text);
            if (string.Equals(bodyName, "HuntressBody", StringComparison.OrdinalIgnoreCase))
            {
                bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
                result = USUScoutHuntressEncounterGate.AppendHuntressIV(result, es);
            }
            return result;
        }

        private static string AppendMercSoraIfNeeded(string bodyName, string text)
        {
            if (!string.Equals(bodyName, "MercBody", StringComparison.OrdinalIgnoreCase)) return text;
            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            return USUMercSoraEncounterGate.AppendMercIV(text, es);
        }

        private static string AppendRexKnightIfNeeded(string bodyName, string text)
        {
            if (!string.Equals(bodyName, "TreebotBody", StringComparison.OrdinalIgnoreCase)) return text;
            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            return USURexKnightEncounterGate.AppendRexIV(text, es);
        }

        private static string AppendMultSonicIfNeeded(string bodyName, string text)
        {
            if (!string.Equals(bodyName, "ToolbotBody", StringComparison.OrdinalIgnoreCase)) return text;
            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            return USUMultSonicEncounterGate.AppendMultIV(text, es);
        }

        private static string AppendCommandoBayonettaIfNeeded(string bodyName, string text)
        {
            if (!string.Equals(bodyName, "CommandoBody", StringComparison.OrdinalIgnoreCase)) return text;
            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            return USUCommandoBayonettaEncounterGate.AppendCommandoIV(text, es);
        }

        private static string AppendLoaderRocketerIfNeeded(string bodyName, string text)
        {
            if (!string.Equals(bodyName, "LoaderBody", StringComparison.OrdinalIgnoreCase)) return text;
            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            return USULoaderRocketerEncounterGate.AppendLoaderIII(text, es);
        }

        private static string AppendBanditIVIfNeeded(string bodyName, string text)
        {
            if (!string.Equals(bodyName, "Bandit2Body", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(bodyName, "BanditBody", StringComparison.OrdinalIgnoreCase)) return text;
            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            return USUSpyBanditEncounterGate.AppendBanditIV(text, es);
        }

        private static string AppendDiaryIIOrLock(string bodyName, string original, bool nativeUnlocked)
        {
            if (string.IsNullOrEmpty(bodyName)) return original;
            string value = (original ?? "").TrimEnd('\r', '\n', ' ', '\t');
            if (value.IndexOf("Diario II —", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("Log II —", StringComparison.OrdinalIgnoreCase) >= 0)
                return value;
            if (nativeUnlocked || USUVanillaDiaryVictoryTracker.IsDiaryIIUnlockedForLocalProfile(bodyName))
                return AppendDiariesForBody(bodyName, value);
            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            string title = es ? "Diario II — Bloqueado (USU)" : "Log II — Locked (USU)";
            string message = es
                ? "Completa una partida con este superviviente escapando de la Luna u obliterándote para recuperar su diario."
                : "Complete a run with this survivor by escaping the Moon or obliterating to recover this log.";
            return value + "\n\n<size=18><b>" + title + "</b></size>\n" + message;
        }

        private static string AppendDiariesForBody(string bodyName, string notes)
        {
            return AppendDiariesBase(bodyName, false, notes);
        }

        private static string AppendDiariesBase(SurvivorContentProfile profile, bool hidden, string nativeNotes)
        {
            return profile == null ? nativeNotes : AppendDiariesBase(profile.BodyName, hidden, nativeNotes);
        }

        private static string AppendDiariesBase(string bodyName, bool hidden, string nativeNotes)
        {
            if (hidden || string.IsNullOrEmpty(bodyName))
                return nativeNotes;

            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            string title;
            string body;

            if (string.Equals(bodyName, "CommandoBody", StringComparison.OrdinalIgnoreCase))
            {
                title = es ? "Diario II — Sin tiempo para descansar (USU)" : "Log II — No Time to Rest (USU)";
                body = es ? Spanish : English;
            }
            else if (string.Equals(bodyName, "HuntressBody", StringComparison.OrdinalIgnoreCase))
            {
                title = es ? "Diario II — El rastro de una cazadora (USU)" : "Log II — A Hunter's Trail (USU)";
                body = es ? HuntressSpanish : HuntressEnglish;
            }
            else if (string.Equals(bodyName, "EngiBody", StringComparison.OrdinalIgnoreCase))
            {
                title = es ? "Diario II — Más allá de los planos (USU)" : "Log II — Beyond the Blueprints (USU)";
                body = es ? EngineerSpanish : EngineerEnglish;
            }
            else if (string.Equals(bodyName, "ToolbotBody", StringComparison.OrdinalIgnoreCase))
            {
                title = es ? "Diario II — Unidad operativa (USU)" : "Log II — Operational Unit (USU)";
                body = es ? MultSpanish : MultEnglish;
            }
            else if (string.Equals(bodyName, "CaptainBody", StringComparison.OrdinalIgnoreCase))
            {
                return CaptainUSUDiary.Append(nativeNotes, es);
            }
            else if (string.Equals(bodyName, "CrocoBody", StringComparison.OrdinalIgnoreCase))
            {
                return AcridUSUDiary.Append(nativeNotes, es);
            }
            else if (string.Equals(bodyName, "LoaderBody", StringComparison.OrdinalIgnoreCase))
            {
                return LoaderUSUDiary.Append(nativeNotes, es);
            }
            else if (string.Equals(bodyName, "TreebotBody", StringComparison.OrdinalIgnoreCase))
            {
                return RexUSUDiary.Append(nativeNotes, es);
            }
            else if (string.Equals(bodyName, "MercBody", StringComparison.OrdinalIgnoreCase))
            {
                return MercenaryUSUDiary.Append(nativeNotes, es);
            }
            else if (string.Equals(bodyName, "MageBody", StringComparison.OrdinalIgnoreCase))
            {
                return ArtificerUSUDiary.Append(nativeNotes, es);
            }
            else if (string.Equals(bodyName, "Bandit2Body", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(bodyName, "BanditBody", StringComparison.OrdinalIgnoreCase))
            {
                return BanditUSUDiary.Append(nativeNotes, es);
            }
            else
            {
                return nativeNotes;
            }

            return (nativeNotes ?? "") + "\n\n<size=18><b>" + title + "</b></size>\n" + body;
        }

        private const string Spanish = @"Todavía recuerdo el sonido de aquellas plataformas lanzamisiles. Habíamos pasado demasiado tiempo intentando asegurar ese frente y cada segundo detrás de la cobertura parecía durar una eternidad. El cuerpo me dolía, el casco apenas amortiguaba las explosiones y solo podía pensar en aprovechar la siguiente recarga del enemigo. Conseguí hacerlo. Después apareció aquella orden de reasignación en mi visor. No tuve oportunidad de reunirme con mi pelotón ni de comprobar personalmente cómo terminó la operación. A veces me preguntaba si habrían asegurado las últimas posiciones. Conocía a mis compañeros y sabía de lo que eran capaces, pero eso no hacía que dejara de pensar en ellos. Había aprendido a obedecer órdenes mucho antes de comprender que algunas también podían dejar preguntas sin respuesta.

Ahora formaba parte de la tripulación de la UES Safe Travels. Mi nueva misión no consistía en recuperar un territorio ni neutralizar una posición enemiga, sino en investigar la desaparición de otra nave: la UES Contact Light. Había personal con funciones muy distintas a bordo, desde especialistas técnicos hasta combatientes contratados. No conocía a todos, y tampoco necesitaba hacerlo para comprender la importancia de la operación. Cada uno tenía una responsabilidad. La mía era estar preparado para lo que encontráramos. Había revisado mis armas tantas veces durante el viaje que podía reconocer cualquier irregularidad sin mirar los indicadores. Era una vieja costumbre. Cuando las cosas comenzaban a salir mal, el tiempo para corregir errores ya había terminado.

La aproximación a nuestro destino comenzó con una señal de emergencia. Era la transmisión que había dejado la nave desaparecida. Desde mi posición podía escuchar fragmentos de los informes mientras la Safe Travels examinaba el planeta. No había una imagen clara de la Contact Light, solamente restos y señales de una catástrofe que todavía no comprendíamos. Lo peor fue escuchar que los escáneres no encontraban indicios de supervivientes. Aquella información me recordó lo rápido que podía cambiar una operación. Un momento estabas siguiendo un procedimiento y al siguiente descubrías que todos tus planes habían dejado de servir. La transmisión mencionaba criaturas hostiles. No sabía qué clase de enemigos podían encontrarse allí, ni cómo habían conseguido poner en peligro a una nave entera. Pero había aprendido a no subestimar una amenaza solamente porque todavía no la conocía.

Me quedé observando el planeta mientras continuaban los informes. Desde aquella distancia no parecía diferente de muchos otros mundos habitables. Había nubes, extensiones de tierra y regiones que los instrumentos apenas conseguían identificar. Sin embargo, en algún lugar de su superficie se encontraba la respuesta a lo ocurrido con la Contact Light. Tal vez todavía quedara alguien esperando ayuda. No había manera de saberlo desde la nave. Recordé a mi antiguo pelotón y aquella última orden recibida en mitad del combate. Entonces me habían apartado de una misión antes de conocer su desenlace. Esta vez no quería marcharme sin averiguar qué había sucedido.

Cuando llegó la autorización para el descenso, terminé de ajustar mi equipo. Comprobé las dos pistolas, aseguré las reservas de munición y revisé los sistemas del casco. No sabía cuánto tiempo permanecería en superficie ni si las comunicaciones funcionarían después del aterrizaje. Me acomodé en la cápsula y esperé mientras los mecanismos de lanzamiento completaban sus verificaciones. El cierre se aseguró sobre mi cabeza. Durante unos instantes solamente escuché la vibración de los motores y las indicaciones del sistema. Después llegó la orden de despliegue.

La cápsula descendió atravesando las nubes. Las vibraciones aumentaron hasta que una sacudida anunció el contacto con el terreno. Esperé la confirmación de los sistemas antes de liberar los seguros. Las lecturas ambientales aparecieron en mi visor, seguidas por coordenadas que todavía no significaban nada para mí. Comprobé una última vez el estado de mis armas y accioné la apertura. Frente a mí se extendía un paisaje desconocido, sin señales inmediatas de la tripulación desaparecida. Estaba lejos de mi antiguo frente, de mi pelotón y de todo cuanto había conocido.

No había recibido respuestas. Solo una misión.

Y estaba listo para cumplirla.";

        private const string English = @"I still remember the sound of those missile platforms. We had spent far too long trying to secure that front, and every second behind cover seemed to last forever. My body ached, my helmet barely muffled the explosions, and all I could think about was the enemy's next reload. I took my chance. Then the reassignment order appeared on my visor. I never got to rejoin my squad or see how the operation ended. Sometimes I wondered whether they had secured the remaining positions. I knew what my comrades could do, but that did not stop me thinking about them. I had learned to follow orders long before I understood that some orders leave questions unanswered.

Now I was part of the UES Safe Travels crew. My new assignment wasn't to retake territory or neutralize an enemy position, but to investigate another ship's disappearance: the UES Contact Light. There were specialists of every kind aboard, from technicians to hired fighters. I didn't know them all, and I didn't need to. Everyone had a job. Mine was to be ready for whatever we found. I had checked my weapons so often during the journey that I could recognize a problem without looking at the indicators. An old habit. Once things start going wrong, there is no time left to correct mistakes.

Our approach began with a distress signal left behind by the missing ship. From my station I could hear fragments of the reports while Safe Travels surveyed the planet. There was no clear image of Contact Light, only debris and evidence of a disaster we didn't yet understand. The worst part was hearing that the scanners found no sign of survivors. It reminded me how quickly an operation could change. One moment you're following procedure; the next, every plan you've made is useless. The transmission mentioned hostile creatures. I didn't know what kind of enemies lived down there or how they could have threatened an entire ship. But I'd learned not to underestimate a threat just because I hadn't encountered it before.

I watched the planet as the reports continued. From that distance it looked like any other habitable world: clouds, continents, and regions the instruments couldn't quite identify. Somewhere on its surface lay the answer to what happened to Contact Light. Maybe someone was still waiting for help. There was no way to know from orbit. I remembered my old squad and the last order that pulled me away before the fighting was over. This time I wanted to know how the mission ended.

When descent authorization came, I finished adjusting my gear. I checked both pistols, secured my ammunition, and tested the helmet systems. I didn't know how long I'd be on the surface or whether communications would survive the landing. I took my seat in the pod while its launch systems completed their checks. The hatch sealed overhead. For a few moments there was nothing but the vibration of engines and the system's instructions. Then came the order to deploy.

The pod fell through the clouds. The shaking grew until a violent jolt announced contact with the ground. I waited for system confirmation before releasing the safeties. Environmental readings filled my visor, followed by coordinates that meant nothing to me yet. I checked my weapons one last time and opened the hatch. An unfamiliar landscape stretched ahead, with no immediate sign of the missing crew. I was far from my old front, my squad, and everything I knew.

I had no answers. Only a mission.

And I was ready to carry it out.";
        private const string HuntressSpanish = @"Había pasado demasiado tiempo observando aquellas calles. Incluso ahora, cuando cerraba los ojos, podía recordar la oscuridad de los callejones, las azoteas y las ventanas de aquel almacén abandonado. Amiera había conseguido escapar de la policía llevando una de las últimas amatistas prismáticas. Era una ladrona experimentada. Sabía ocultarse y tenía la paciencia necesaria para permanecer inmóvil durante horas, incluso días. Pero había cometido un error: creer que quien la perseguía también acabaría cansándose. Yo no tenía ninguna prisa. Había seguido su rastro, estudiado sus movimientos y esperado el momento adecuado. No necesitaba enfrentarme a ella ni anunciar mi presencia. Cuando finalmente tuve una abertura, mis flechas hicieron el resto. La recompensa fue de veinte mil créditos. Otro trabajo terminado. A veces me preguntaba cuánto tiempo habría permanecido escondida si yo no hubiera estado allí. Probablemente más del que la policía estaba dispuesta a esperar.

Abrí los ojos. Ya no estaba en aquella ciudad. Ahora viajaba a bordo de la UES Safe Travels, formando parte de una expedición cuyo destino era un mundo que nunca había visitado. La misión era investigar la desaparición de la Contact Light y buscar posibles supervivientes. No tenía demasiada información sobre lo que había ocurrido, pero sabía que la tripulación había dejado de comunicarse y que todavía existían demasiadas preguntas sin respuesta. Aquello era diferente de mis trabajos anteriores. No había un rostro que buscar, una recompensa asociada a un fugitivo ni un rastro que pudiera seguir entre calles conocidas. En su lugar, había una nave desaparecida y un territorio del que apenas disponíamos de información. Había aceptado participar en la expedición, y ahora me correspondía cumplir con mi parte.

Durante el viaje dediqué tiempo a comprobar mi equipo. Mi arco respondía correctamente, los sistemas de fijación de objetivos estaban operativos y mi traje mantenía sus funciones habituales. Siempre he confiado más en una preparación cuidadosa que en la suerte. En mis antiguas cacerías conocía el valor de cada segundo de espera y la importancia de observar antes de actuar. Aquí tendría que aplicar esas mismas lecciones en circunstancias desconocidas. Había otros integrantes de la expedición, pero no necesitaba conocer sus historias para concentrarme en mi responsabilidad. Cada uno tenía sus propias razones para estar allí. Las mías eran suficientes.

Cuando la nave comenzó a aproximarse a su destino, observé las imágenes transmitidas por los instrumentos. La superficie mostraba extensiones de vegetación, formaciones rocosas y regiones que no podía identificar. Desde la distancia parecía tranquila, pero sabía que las apariencias podían ser engañosas. Recordé nuevamente a Amiera. Había creído estar a salvo porque nadie podía verla. Ahora nosotros buscábamos una nave cuya tripulación tampoco respondía. La diferencia era que todavía no sabíamos si alguien se ocultaba, si había sucedido un accidente o si existía una amenaza que nuestros instrumentos no conseguían detectar. La ausencia de respuestas no era una prueba de seguridad.

Llegó el momento del despliegue. Revisé una última vez mi arco y ajusté los sistemas de mi traje antes de ocupar mi lugar en la cápsula. Los mecanismos se aseguraron y el sonido de la nave quedó sustituido por el de los sistemas de descenso. No sabía qué encontraría en superficie ni si tendría oportunidad de regresar pronto. Tampoco podía anticipar cómo reaccionarían los habitantes de aquel mundo ante nuestra presencia. No tenía sentido intentar resolver esas preguntas desde una cápsula cerrada. Necesitaba observar el terreno por mí misma.

El descenso terminó con una sacudida. Esperé a que los indicadores confirmaran el aterrizaje y abrí la escotilla. Frente a mí se extendía un paisaje desconocido. No reconocía las plantas ni las formaciones que se alzaban a lo lejos. No había señales inmediatas de la Contact Light, pero eso no significaba que no existieran. Ajusté mi visor, preparé el arco y examiné las posibles rutas antes de avanzar.

Amiera había pensado que nadie conseguiría encontrarla. La tripulación de aquella nave desaparecida podía estar enfrentándose a algo mucho peor.

Todavía no sabía qué había ocurrido en ese planeta.

Pero había llegado para buscar respuestas.";

        private const string HuntressEnglish = @"I had spent far too long watching those streets. Even now, when I closed my eyes, I could remember the dark alleys, the rooftops, and the windows of that abandoned warehouse. Amiera had escaped the police with one of the last prismatic amethysts. She was an experienced thief. She knew how to hide and had the patience to stay still for hours, even days. But she had made one mistake: believing that whoever hunted her would eventually grow tired. I was in no hurry. I had followed her trail, studied her movements, and waited for the right moment. I didn't need to confront her or announce myself. When an opening finally appeared, my arrows did the rest. The reward was twenty thousand credits. Another job finished. Sometimes I wondered how long she might have stayed hidden if I hadn't been there. Probably longer than the police were willing to wait.

I opened my eyes. I was no longer in that city. I was aboard the UES Safe Travels, part of an expedition bound for a world I had never visited. Our mission was to investigate the disappearance of the Contact Light and search for possible survivors. I had little information about what had happened, but I knew its crew had stopped communicating and too many questions remained unanswered. This was different from my previous work. There was no face to find, no bounty attached to a fugitive, no trail to follow through familiar streets. Instead, there was a missing ship and a territory about which we knew almost nothing. I had agreed to join the expedition, and now I had a job to do.

During the journey, I spent time checking my equipment. My bow responded correctly, the targeting systems were operational, and my suit retained its usual functions. I have always trusted careful preparation more than luck. On my old hunts I knew the value of every second spent waiting, and the importance of watching before acting. Here I would need those same lessons under unfamiliar conditions. There were others aboard, but I didn't need to know their stories to concentrate on my responsibilities. Each had their own reasons for being there. Mine were enough.

As the ship approached its destination, I watched the images relayed by its instruments. The surface showed stretches of vegetation, rocky formations, and regions I couldn't identify. From a distance it looked peaceful, but I knew appearances could deceive. I thought of Amiera again. She had believed herself safe because no one could see her. Now we were searching for a ship whose crew no longer answered. The difference was that we did not yet know whether anyone was hiding, whether there had been an accident, or whether a threat lay beyond the reach of our instruments. A lack of answers was no proof of safety.

The time came to deploy. I checked my bow one final time and adjusted my suit's systems before taking my place in the pod. Its mechanisms locked in place, and the sounds of the ship gave way to the descent systems. I did not know what I would find on the surface, or whether I would be able to return soon. Nor could I anticipate how the inhabitants of that world would react to our presence. There was no point trying to settle those questions inside a sealed pod. I needed to see the ground for myself.

The descent ended with a jolt. I waited for the indicators to confirm the landing and opened the hatch. An unfamiliar landscape stretched before me. I recognized neither the plants nor the formations rising in the distance. There was no immediate sign of the Contact Light, but that didn't mean there was none to find. I adjusted my visor, readied my bow, and studied the possible routes before moving.

Amiera had thought no one could find her. The crew of that missing ship might be facing something far worse.

I still didn't know what had happened on this planet.

But I had come to find answers.";
        // Engineer: diario adicional desbloqueado con el personaje, sin misión USU adicional.
        private const string EngineerSpanish = @"La tecnología siempre ha despertado dos reacciones: admiración y temor. Lo había visto en los debates sobre las modificaciones cibernéticas, cuando algunos defendían la posibilidad de mejorar cualquier limitación humana y otros se preguntaban cuánto podía reemplazarse antes de perder nuestra propia identidad. Era una discusión que iba mucho más allá de los mecanismos y circuitos. Para mí, la ingeniería siempre había planteado una pregunta diferente: ¿qué problema estamos intentando resolver? Un dispositivo podía ser extraordinariamente complejo y seguir siendo inútil si no cumplía su propósito. Una herramienta sencilla, en cambio, podía salvar una vida cuando alguien sabía utilizarla. Nunca he creído que la tecnología deba juzgarse únicamente por lo impresionante que parece. Prefiero comprobar lo que realmente puede hacer.

A bordo de la UES Safe Travels, mi trabajo transcurría entre equipos de análisis, registros técnicos y procedimientos de mantenimiento. La expedición investigaba la desaparición de la Contact Light, pero la información disponible era insuficiente para explicar qué había ocurrido. Desde la nave podíamos examinar imágenes y datos enviados por los equipos que descendían a la superficie. Yo estudiaba especialmente las condiciones del terreno, las interferencias y las estructuras que aparecían en aquellos registros. Había demasiadas incógnitas y cada informe parecía añadir otras nuevas. No conocía personalmente todos los lugares que observaba en las pantallas. Tampoco podía afirmar que entendiera su funcionamiento. Las imágenes mostraban fenómenos; descubrir sus causas era un trabajo completamente distinto.

Con el tiempo, los registros de las expediciones comenzaron a acumularse. Una región presentaba condiciones que no coincidían con las lecturas anteriores. En otra, los equipos encontraban dificultades para desplazarse o mantener sus comunicaciones. Algunos informes contenían datos incompletos; otros mostraban mecanismos y estructuras cuya función no podíamos determinar desde la distancia. Revisé las mediciones, comparé resultados y busqué patrones. Las torretas y los sistemas defensivos que conocía podían responder a amenazas identificables, pero allí ni siquiera teníamos información suficiente para clasificar todos los riesgos. Treinta registros de operaciones completadas. Treinta oportunidades de observar el mismo problema desde ángulos diferentes. Y, aun así, seguía faltando algo.

No era una cuestión de potencia de cálculo. Podía pasar horas examinando una lectura y seguir sin saber qué había provocado una anomalía si el instrumento había observado el fenómeno desde una posición equivocada. Había llegado al límite de lo que podía deducir únicamente a partir de los informes. Necesitábamos observaciones directas, equipos capaces de resistir condiciones cambiantes y alguien que pudiera realizar ajustes sin depender de instrucciones enviadas desde la nave. Mi especialidad resultaba adecuada para aquello. Sabía instalar defensas, preparar dispositivos y adaptar herramientas a situaciones imprevistas. No significaba que estuviera preparado para cualquier cosa. Significaba que disponía de los conocimientos necesarios para intentarlo.

Cuando se confirmó mi despliegue, revisé personalmente el equipamiento asignado. Comprobé los sistemas de las torretas, sus mecanismos de fijación y las conexiones que permitían mantenerlas operativas. Después examiné las minas y el lanzagranadas, asegurándome de que los seguros funcionaran correctamente. No podía instalar un laboratorio completo en una cápsula de descenso, pero sí llevar suficientes herramientas para observar, proteger una posición y continuar trabajando si algo fallaba. Antes de abandonar mi estación revisé los informes acumulados una última vez. Hasta ese momento, aquel planeta había sido una sucesión de imágenes, cifras y problemas pendientes. Pronto dejaría de observarlo desde una pantalla.

Durante el descenso, los indicadores ambientales comenzaron a actualizarse con mayor frecuencia. Las vibraciones del transporte dificultaban la lectura de algunos datos y las imágenes exteriores cambiaban demasiado rápido para examinarlas con detalle. Cuando la cápsula aterrizó, esperé a que los sistemas confirmaran la estabilidad de la estructura. Después comprobé el estado de mi equipo y liberé la escotilla. Frente a mí se extendía un paisaje que conocía solamente a través de los registros de la expedición. Las formaciones del terreno, la vegetación y los sonidos que llegaban desde el exterior eran distintos de cuanto había estudiado a bordo. Tenía datos suficientes para comenzar, pero todavía demasiadas preguntas.

Activé mis instrumentos y examiné el terreno donde instalaría el primer equipo.

Habíamos pasado treinta operaciones reuniendo información sobre aquel mundo.

Ahora era mi turno de comprobarla.

Y esta vez no estaría trabajando detrás de una pantalla.";
        private const string EngineerEnglish = @"Technology has always inspired two reactions: admiration and fear. I had seen it in debates about cybernetic modifications, when some defended the possibility of overcoming any human limitation and others questioned how much could be replaced before we lost what made us human. That discussion went far beyond mechanisms and circuitry. To me, engineering had always posed a different question: what problem are we trying to solve? A device could be extraordinarily complex and still be useless if it failed at its purpose. A simple tool, on the other hand, could save a life when someone knew how to use it. I have never believed that technology should be judged solely by how impressive it looks. I would rather see what it can actually do.

Aboard the UES Safe Travels, my work revolved around analysis equipment, technical records, and maintenance procedures. The expedition was investigating the disappearance of the Contact Light, but the available information was insufficient to explain what had happened. From the ship, we could examine images and data sent by teams descending to the surface. I focused on terrain conditions, interference, and the structures appearing in those reports. There were too many unknowns, and every report seemed to add more. I had never personally visited the places shown on those screens. Nor could I claim to understand how they worked. Images showed us phenomena; discovering their causes was an entirely different job.

Over time, expedition records began to accumulate. One region showed conditions that contradicted earlier readings. In another, teams struggled to move or maintain communications. Some reports contained incomplete data; others showed mechanisms and structures whose purpose could not be determined from a distance. I reviewed the measurements, compared results, and searched for patterns. The turrets and defensive systems I knew could respond to identifiable threats, but down there we did not even have enough information to classify every risk. Thirty completed operation records. Thirty opportunities to observe the same problem from different angles. And still, something was missing.

It was not a matter of computing power. I could spend hours examining a reading and still have no idea what caused an anomaly if the instrument had observed it from the wrong position. I had reached the limit of what I could deduce from reports alone. We needed direct observations, equipment able to withstand changing conditions, and someone who could make adjustments without relying on instructions transmitted from the ship. My specialty suited that work. I knew how to set up defenses, prepare devices, and adapt tools to unexpected situations. That did not mean I was ready for everything. It meant I had the knowledge needed to try.

When my deployment was confirmed, I personally inspected the assigned equipment. I checked the turrets' systems, their targeting mechanisms, and the connections that kept them operational. Then I examined the mines and grenade launcher, making sure their safeties functioned correctly. I could not fit an entire laboratory into a drop pod, but I could bring enough tools to observe, defend a position, and keep working if something broke. Before leaving my station, I reviewed the accumulated reports one last time. Until then, that planet had been a succession of images, figures, and unresolved problems. Soon I would no longer be watching it through a screen.

During descent, the environmental indicators began updating more frequently. The transport's vibrations made some readings difficult to follow, and the exterior images changed too quickly to examine in detail. When the pod landed, I waited for the systems to confirm the structure was stable. Then I checked my equipment and released the hatch. An unfamiliar landscape stretched before me, a place I knew only from expedition records. The terrain formations, vegetation, and sounds coming from outside differed from everything I had studied aboard the ship. I had enough data to begin, but far too many unanswered questions.

I activated my instruments and examined the ground where I would install my first equipment.

We had spent thirty operations gathering information about this world.

Now it was my turn to verify it.

And this time I would not be working behind a screen.";

        // MUL-T: Diario II adicional. No altera el Logbook vanilla ni exige misión extra.
        private const string MultSpanish = @"REGISTRO DE MEMORIA — UNIDAD MUL-T
ESTADO: OPERATIVO
MÓDULO DE APRENDIZAJE: ACTIVO

Los registros recientes conservaban dos identificadores de voz: HIROSHI y RICO. Una revisión de mantenimiento, herramientas sobre la carcasa, ajustes de sensores y una modificación en los circuitos. Antes de esa intervención, muchas observaciones se descartaban cuando la tarea terminaba. Ahora permanecían almacenadas. La imagen de una mano realizando un gesto, una fotografía mostrada por uno de los técnicos y una instrucción asociada a ella seguían disponibles para su recuperación. Los sistemas no asignaban una emoción a esos datos; simplemente habían empezado a conservarlos de una forma nueva. La unidad abandonó el área de mantenimiento y retomó sus funciones habituales, mientras el nuevo módulo continuaba analizando aquello que veía y escuchaba.

REGISTRO DE NAVE — UES SAFE TRAVELS
REFERENCIA TEMPORAL RECIBIDA: 13 HORAS HASTA DESTINO

La unidad continuó transportando materiales, identificando herramientas y recorriendo los sectores asignados. Una tarea podía completarse por más de una ruta. Un obstáculo podía resolverse con una herramienta distinta de la prevista. Los primeros intentos no siempre producían la solución más eficiente; cada corrección aportaba una nueva relación a la memoria. No había órdenes para explorar esas posibilidades, pero tampoco era necesario desobedecer ninguna instrucción: el objetivo seguía siendo cumplir la tarea. El módulo de aprendizaje comparaba resultados y conservaba patrones. En los registros aparecían personas, gestos y sonidos que antes habrían sido información irrelevante. La unidad seguía comunicándose mediante pitidos y señales. Ninguna de sus funciones requería palabras.

DIAGNÓSTICO DE CONFIGURACIÓN
CAMBIOS DEL MÓDULO: PRESENTES
ESTADO DE RESTAURACIÓN ANTERIOR: NO CONSTA

Las memorias no indicaban si los técnicos habían intentado deshacer la modificación después de que MUL-T saliera del taller. Tampoco registraban una nueva intervención sobre el circuito de aprendizaje. La ausencia de datos no permitía establecer qué habían decidido Hiroshi y Rico. Para la unidad, lo verificable era más sencillo: podía recordar, comparar y adaptar sus procedimientos. La revisión del equipamiento terminó con los sistemas de movilidad, las herramientas y los sensores dentro de los parámetros operativos. La nave seguía su curso hacia una ubicación que los registros de servicio no describían con detalle. El destino aún no era un lugar conocido: solo una referencia de navegación asociada a nuevas tareas.

PROCEDIMIENTO DE DESPLIEGUE
ENTORNO DE DESTINO: DATOS INSUFICIENTES
SENSORES: DISPONIBLES

La secuencia siguiente comenzó con mecanismos de sujeción y una comprobación de transporte. Las instrucciones permitían iniciar operaciones fuera de los sectores habituales de la nave. El descenso añadió vibraciones, variaciones de presión y lecturas incompletas. Cuando cesó el movimiento, los sensores registraron un terreno que no coincidía con ninguna zona de trabajo anterior. Había vegetación, rocas y formas cuyo uso no podía deducirse de la base de datos. No constaba una ruta de servicio preparada para aquella superficie. La unidad revisó sus herramientas, desplegó los mecanismos de desplazamiento y buscó una orden que definiera por completo el siguiente procedimiento. No la encontró.

ATERRIZAJE: CONFIRMADO
ESTADO DE UNIDAD: OPERATIVO
ANÁLISIS DEL ENTORNO: INICIADO

MUL-T emitió un breve pitido. Podía seguir esperando una instrucción más precisa o comenzar por aquello que sí era posible hacer: observar, reconocer los obstáculos y reunir información. Las tareas futuras seguían siendo desconocidas, pero el módulo de aprendizaje estaba activo. La unidad avanzó sobre la superficie sin asignarle todavía un nombre al mundo que la rodeaba.

NUEVOS DATOS DISPONIBLES.
OPERACIÓN EN CURSO.";

        private const string MultEnglish = @"MEMORY RECORD — MUL-T UNIT
STATUS: OPERATIONAL
LEARNING MODULE: ACTIVE

Recent records retained two voice identifiers: HIROSHI and RICO. A maintenance inspection, tools against the chassis, sensor adjustments, and a circuit modification. Before that intervention, many observations had been discarded once a task was complete. Now they remained in storage. The image of a hand making a gesture, a photograph shown by one of the technicians, and an instruction associated with it could all be retrieved. The systems assigned no emotion to those data; they had simply begun retaining them in a new way. The unit left the maintenance area and resumed its usual functions while the new module continued analyzing everything it saw and heard.

SHIP RECORD — UES SAFE TRAVELS
RECEIVED TIME REFERENCE: 13 HOURS TO DESTINATION

The unit continued carrying materials, identifying tools, and navigating assigned sectors. A task could be completed by more than one route. An obstacle could be handled with a different tool than the one originally specified. Early attempts did not always yield the most efficient solution; each correction added another association to memory. There were no orders to explore those possibilities, but no instruction needed to be disobeyed either: completing the task remained the objective. The learning module compared results and retained patterns. People, gestures, and sounds now appeared in records that would previously have been irrelevant. The unit still communicated through beeps and signals. None of its functions required speech.

CONFIGURATION DIAGNOSTIC
MODULE CHANGES: PRESENT
PREVIOUS RESTORATION STATUS: NOT RECORDED

The memories did not show whether the technicians had attempted to undo the modification after MUL-T left the workshop. Nor did they record another intervention on the learning circuit. Missing data could not establish what Hiroshi and Rico had decided. For the unit, the verifiable result was simpler: it could remember, compare, and adapt its procedures. Equipment checks concluded with mobility systems, tools, and sensors within operating parameters. The ship continued toward a location that the service records did not describe in detail. The destination was not yet a known place: only a navigational reference linked to new tasks.

DEPLOYMENT PROCEDURE
DESTINATION ENVIRONMENT: INSUFFICIENT DATA
SENSORS: AVAILABLE

The next sequence began with restraints and a transport check. Instructions authorized operations outside the ship's familiar service sectors. Descent added vibration, pressure changes, and incomplete readings. Once movement ceased, the sensors recorded terrain unlike any previous work area. There was vegetation, stone, and shapes whose purpose could not be determined from the database. No prepared service route was recorded for that surface. The unit checked its tools, extended its mobility mechanisms, and searched for an order that would fully define the next procedure. None was found.

LANDING: CONFIRMED
UNIT STATUS: OPERATIONAL
ENVIRONMENT ANALYSIS: INITIATED

MUL-T emitted a short beep. It could keep waiting for a more precise instruction, or begin with what was possible: observe, identify obstacles, and gather data. Future tasks remained unknown, but the learning module was active. The unit moved across the surface without yet assigning a name to the world around it.

NEW DATA AVAILABLE.
OPERATION IN PROGRESS.";

    }
}
