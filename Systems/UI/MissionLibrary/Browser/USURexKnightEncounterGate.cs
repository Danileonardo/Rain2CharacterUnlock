using System;
using RoR2;
namespace UniversalSurvivorUnlocks {
 internal static class USURexKnightEncounterGate {
  internal static bool CaptainReady() => VanillaBroadcastUSUDiary.IsUnlockedForLocalProfile();
  internal static bool KnightReady() => USUVanillaDiaryVictoryTracker.IsModDiaryIUnlockedForLocalProfile("KnightBody");
  internal static string ResolveKnight(bool es) {
   if (!CaptainReady()) return es ? "[ARCHIVO CORRUPTO — SEÑAL INTERFERIDA]\n▓██▒▓██░▒████\n...a todas las unidades...\n███▒▓██▒████" : "[CORRUPTED FILE — SIGNAL INTERFERENCE]\n▓██▒▓██░▒████\n...to all units...\n███▒▓██▒████";
   if (!KnightReady()) return es ? "[ARCHIVO CORRUPTO — MEMORIA NO RECUPERADA]\n▓██▒░███▒██▓\n...raíces... ▓▓▓ ...un pequeño viajero...\n███▒▓██▒████" : "[CORRUPTED FILE — MEMORY NOT RECOVERED]\n▓██▒░███▒██▓\n...roots... ▓▓▓ ...a small wanderer...\n███▒▓██▒████";
   return es ? KnightCompleteEs : KnightCompleteEn;
  }
  internal static string AppendRexIV(string existing, bool es) {
   string text=existing??"";
   if(text.Contains("Una compañía sin instrucciones") || text.Contains("A Companion Without Orders")) return text;
   string title=es?"Diario IV — Una compañía sin instrucciones (USU)":"Log IV — A Companion Without Orders (USU)";
   string body=!CaptainReady() ? (es ? "[ARCHIVO CORRUPTO — SEÑAL INTERFERIDA]\n▓██░▒████▓██\n...mantengan las comunicaciones...\n▓▓▓▓▓▓" : "[CORRUPTED FILE — SIGNAL INTERFERENCE]\n▓██░▒████▓██\n...keep communications open...\n▓▓▓▓▓▓") : !KnightReady() ? (es ? "[ARCHIVO CORRUPTO — ENCUENTRO NO REGISTRADO]\n▓██▒░███▒██▓\n...entidad pequeña... ▓▓▓ ...sin hostilidad...\n███▒▓██▒████" : "[CORRUPTED FILE — ENCOUNTER NOT RECORDED]\n▓██▒░███▒██▓\n...small entity... ▓▓▓ ...no hostility...\n███▒▓██▒████") : (es ? RexCompleteEs : RexCompleteEn);
   return text+"\n\n<size=18><b>"+title+"</b></size>\n"+body;
  }
  internal static readonly string RexCompleteEs = @"REGISTRO I — SEÑAL CONSERVADA
[UNIDAD MECÁNICA: OPERATIVA] [ORGANISMO VEGETAL: ESTABLE]
Frecuencia UES detectada. Identificación: CAPTAIN. Información recibida: puesto avanzado, comunicaciones, referencias de navegación. Coordenadas almacenadas. No hay orden directa ni necesidad biológica inmediata de desplazamiento. Continuar búsqueda de agua y luz.
[ALERTA] Fluctuación energética. Pérdida de posición. Registro de desplazamiento interrumpido: ▓▓▓▓▓.

REGISTRO II — ENTIDAD C-01
[REACTIVACIÓN] Posición distinta de la última registrada. Traslado: SIN DATOS. Intervalo: DESCONOCIDO.
Una entidad pequeña de caparazón blanco, dos prolongaciones y capa oscura se encuentra junto a las ruinas. Porta un arma cortante. Designación: C-01. C-01 levanta su arma; sistemas defensivos disponibles. Ninguno ataca. C-01 baja el arma y rodea una raíz expuesta sin pisarla.
[RESPUESTA VEGETAL] Actividad defensiva disminuida. [DECISIÓN] Permitir proximidad.

REGISTRO III — COOPERACIÓN SIN INSTRUCCIONES
Una criatura corpulenta se aproxima a REX. C-01 se interpone, esquiva su ataque y utiliza su arma cortante para desviar la amenaza. REX dispara proyectiles vegetales y detiene una embestida. C-01 vuelve al ataque. Enemigo neutralizado.
C-01 examina la carcasa dañada de REX y sus hojas; no intenta extraer materiales. Permanece cercano sin orden ni beneficio identificable.
[CLASIFICACIÓN] NO HOSTIL. [DECISIÓN] Continuar desplazamiento por rutas coincidentes.

REGISTRO IV — CUATRO INCORPORACIONES
C-01 encuentra un contenedor; REX acciona su cierre y recoge un objeto orgánico desconocido. El objeto se integra al equipo de REX. Después de varios combates se registra incremento progresivo de capacidad vital. Hipótesis: efecto vinculado a amenazas derrotadas.
C-01 localiza otros tres contenedores. REX recoge los tres objetos equivalentes.
[ARTEFACTOS TIPO A EQUIPADOS EN REX: 04] [EQUIPADOS EN C-01: 00]
Una sección del camino se derrumba. C-01 alcanza una plataforma segura; REX queda en una ruta inferior bloqueada por una criatura. C-01 regresa y atrae su atención. REX dispara. Ruta liberada. C-01 podía continuar solo; regresó.

REGISTRO V — CUARENTA MANIFESTACIONES
REX encuentra un artefacto de apariencia ósea, origen desconocido. C-01 se aparta y REX lo equipa.
[ARTEFACTO TIPO B: EQUIPADO EN REX]
Durante el combate, REX recibe daños; el organismo vegetal activa recuperación. El artefacto acumula energía y emite una estructura flotante semejante a un cráneo, que se dirige a la amenaza. Hipótesis: energía vinculada a curación del portador.
En enfrentamientos sucesivos REX pierde y recupera integridad. C-01 protege las raíces y abre vías de retirada. Conteo de emisiones de REX: 01... 07... 16... 28... 37... 40.
[MANIFESTACIONES GENERADAS POR REX: 40] No equivalen a cuarenta enemigos eliminados. Comunicación verbal: NINGUNA.

REGISTRO VI — COMPAÑÍA
REX se detiene en un suelo húmedo, con luz apropiada para el crecimiento vegetal. C-01 avanza algunos pasos, se vuelve y regresa para esperar. No existe orden de acompañamiento. Cuando REX termina de absorber recursos, C-01 reanuda la marcha. REX lo sigue.
[CLASIFICACIÓN ANTERIOR] C-01: ENTIDAD DESCONOCIDA.
[CLASIFICACIÓN ACTUAL] C-01: COMPAÑERO.
[PRIORIDADES] Preservar organismo vegetal; preservar soporte mecánico; considerar seguridad de C-01 al seleccionar rutas.
[ARCHIVOS] Transmisión de Captain almacenada. Desplazamiento hacia el puesto avanzado: NO INICIADO.
[DECISIÓN] Continuar exploración conjunta. Ninguna instrucción requerida.";
  internal static readonly string RexCompleteEn = @"LOG I — SIGNAL PRESERVED
[MECHANICAL STATUS: OPERATIONAL]
[BIOLOGICAL STATUS: STABLE]

The unit continues moving along routes chosen according to nutrients, mechanical integrity and nearby threats. Previous logs confirm that the plant organism and its mechanical support must remain functional together.

[INCOMING TRANSMISSION]
Origin: UES FREQUENCY.
Sender: CAPTAIN.
The transmission describes a forward outpost for gathering survivors, conserving resources and coordinating exploration. Navigation references and a communication frequency are supplied.

[ASSESSMENT]
Protocol: RECOGNIZED. Coordinates: STORED. Direct instruction to this unit: NOT DETECTED. Immediate need for assistance: NONE.

[BIOLOGICAL RESPONSE]
Photosynthetic activity: STABLE. Nutrient search: ACTIVE. Relevant changes: NONE.

[DECISION]
Preserve transmission. Maintain survival priorities. Continue exploration.

The unit proceeds toward higher moisture. A fluctuation of unknown energy interferes with spatial sensors.
[WARNING] Position reference: UNSTABLE. Movement record: INTERRUPTED. ▓▓▓▓▓▓▓▓
[UNEXPECTED END OF SEQUENCE]

LOG II — ENTITY C-01
[SYSTEM REACTIVATION]
Mechanical integrity: ACCEPTABLE. Biological integrity: STABLE. Last recorded position: DOES NOT MATCH. Current location: UNREFERENCED. Missing interval: UNKNOWN.

The unit occupies an unfamiliar area. There is no usable record of the transition.
[DETECTION] Small entity, white head surface, two horn-like projections, dark outer covering, cutting weapon.
Provisional designation: C-01.

C-01 stands beside a partially collapsed structure and turns toward REX. The unit aligns its sensors. C-01 raises its weapon slightly; REX prepares its defensive systems without firing. Neither attacks. C-01 lowers the weapon.

[BIOLOGICAL RESPONSE] Defensive activity: HIGH → MODERATE.

Approaching, C-01 stops at an exposed root and deliberately changes course rather than stepping on it.
[OBSERVATION] Contact with living tissue avoided. Intent: UNKNOWN. Hostility: NOT CONFIRMED.
[DECISION] Do not initiate combat. Allow proximity.

LOG III — UNREQUESTED COOPERATION

C-01 continues through the same area. Spoken communication: NONE. Compatible electronic protocol: NONE.

[THREAT EVENT] A large creature emerges from between the structures, shaking the ground. REX calculates a withdrawal route. Before the unit moves, C-01 steps between REX and the hostile, evades its strike and attacks with the cutting weapon. The creature turns toward C-01.

[ASSESSMENT] C-01 has drawn a threat away from REX. Protective instruction issued: NONE.

[BIOLOGICAL RESPONSE] Defense: HIGH. Offensive structures: DEPLOYED.

REX fires plant-based projectiles, interrupting the next charge and giving C-01 time to move aside. C-01 returns to the fight. Threat neutralized.

[RESULT] Mechanical integrity: STABLE. Plant integrity: STABLE. Serious visible injuries to C-01: NONE.

C-01 approaches REX, examines a mark on its casing, then looks toward the leaves emerging from its machinery. The unit adjusts its sensors. C-01 remains nearby after the danger has passed.

[CLASSIFICATION] C-01: NONHOSTILE. Cooperation observed: YES.
[DECISION] Continue along shared routes.

LOG IV — FOUR ACQUISITIONS

[EXPLORATION EVENT] The unit detects a container among structural debris. C-01 investigates the opening mechanism first. REX operates the lock and recovers a small organic-looking object of unknown origin.

[PROVISIONAL CATALOG]
Artifact A-01. Name: UNAVAILABLE. Source: UNKNOWN.
REX incorporates the item into its equipment.

After several hostiles are defeated, sensors report an increase in the combined organism's capacity to withstand damage. This may be associated with the artifact; further observations are required.

C-01 subsequently finds three similar containers. Each time, C-01 waits while REX retrieves and equips their contents.
[INVENTORY]
Type A artifacts carried by REX: 04.
Type A artifacts carried by C-01: 00.
Observed effect: PROGRESSIVE INCREASE IN VITAL CAPACITY.

[UNSTABLE TERRAIN] Part of the path collapses. C-01 reaches a raised platform, while REX finds its lower passage blocked by a large hostile. C-01 has a clear path forward but returns, strikes the creature and draws its attention. REX fires projectiles into the opening. Hostile neutralized. Passage cleared.

[ASSESSMENT] C-01 did not require a return for its own progress. Cause: UNKNOWN.
[BIOLOGICAL RESPONSE] Defensive stress: REDUCED.
[DECISION] Continue together.

LOG V — SPECTRAL PHENOMENON

[ARTIFACT RECOVERY] The unit finds an unfamiliar object among a ruined installation. Its surface resembles bone and emits energy that the instruments cannot classify. C-01 observes it and steps aside while REX examines it.

[PROVISIONAL CATALOG]
Artifact B-01. Name: UNKNOWN. Mechanism: UNIDENTIFIED.
REX incorporates the artifact into its equipment.

[COMBAT EVENT] A hostile strikes the mechanical support. Integrity drops. The plant organism activates its restorative functions and the unit begins recovering.

[ANOMALY] Energy accumulates in Artifact B-01 during recovery. At a threshold, a floating skull-like structure forms and flies toward a nearby hostile. Impact confirmed.

[HYPOTHESIS] Healing received by the wearer powers the artifact. Accumulated energy becomes autonomous projectiles. Confirmation: NOT AVAILABLE.

C-01 follows the skull's path with its head, then turns toward REX. No spoken instruction is detected.

[ADDITIONAL EVENTS] Further battles inflict damage on REX; its biological systems restore integrity. The phenomenon repeats. C-01 intercepts threats approaching the exposed roots and keeps retreat routes clear.

[MANIFESTATION COUNT] 01 / 40. 07 / 40. 16 / 40. 28 / 40. 37 / 40. 40 / 40.
[OBSERVATION COMPLETE] Forty skull-like projectiles generated by the artifact carried by REX. The count does not represent defeated enemies.
[ANALYSIS] REX carries the artifact and supplies its healing. C-01 protects the carrier but does not equip the item. Communication: NONVERBAL. Cooperation: EFFECTIVE.

LOG VI — COMPANION

[MECHANICAL STATUS: OPERATIONAL]
[BIOLOGICAL STATUS: STABLE]
[ENTITY C-01: NEARBY]

REX finds a location with sufficient sunlight and moist soil. It slows and extends roots to absorb resources. C-01 walks ahead several steps, stops, turns back and returns to wait beside the plant organism.

[ASSESSMENT] No nearby threat. No request for assistance. Waiting provides no identifiable immediate advantage to C-01. Reason: UNKNOWN.

[BIOLOGICAL RESPONSE] Defensive activity: MINIMAL. Root growth: STABLE.

When REX can move again, C-01 starts walking. REX adjusts its locomotion and follows.

[RECORD REVIEW]
First contact: NONHOSTILE.
Shared threats: MULTIPLE.
Mutual defensive assistance: CONFIRMED.
Voluntary separation: NOT RECORDED.

[PREVIOUS CLASSIFICATION] C-01: UNKNOWN ENTITY.
[UPDATED CLASSIFICATION] C-01: COMPANION.

[DIRECTIVES]
01. Protect plant organism.
02. Protect mechanical support.
03. Maintain survival resources.
04. Consider C-01's position and integrity when selecting routes.

[ARCHIVES] Captain's broadcast: STORED. Outpost references: STORED. Travel toward outpost: NOT STARTED.
[DECISION] Continue exploration together.

C-01 walks ahead. REX follows. No command to accompany was issued. None was required.";
  internal static readonly string KnightCompleteEs = @"I — UN SUEÑO QUE NO PERTENECÍA A HALLOWNEST
Ah, pequeño portador... Yo había dejado atrás mi existencia y me había dispersado en Esencia. Esto que te acompaña es apenas un eco de la Vidente, una impresión de los recuerdos unidos al Aguijón Onírico. Te vi recorrer una galería silenciosa de Hallownest cuando una luz ajena a nuestros sueños abrió una grieta ante ti. Clavaste tu aguijón en el suelo, intentando resistir la fuerza que te arrastraba. Desapareciste. No conocí el origen de aquella abertura. Mi percepción se interrumpió, y al recuperarla ya estabas en un mundo distinto.

II — UN SER DE RAÍCES Y METAL
No reconocí aquel cielo, ni las estructuras que cubrían el terreno. Una criatura pesada despertó entre las ruinas. Sus mecanismos sostenían un organismo vegetal vivo. Dos existencias reunidas en un solo cuerpo. El ser dirigió sus sensores hacia ti. Levantaste el aguijón; él preparó sus defensas. Ninguno atacó. Bajaste el arma, te aproximaste y rodeaste una raíz tendida sobre la tierra para no dañarla. Las hojas dejaron de agitarse. Era una manera de entenderse sin palabras.

III — UN ACUERDO EN SILENCIO
Seguiste avanzando mientras la criatura de metal recorría una ruta próxima. Entonces una bestia enorme se lanzó contra ella. Podías seguir tu camino, pero te interpusiste, esquivaste sus golpes y la atacaste con el aguijón. Una descarga de proyectiles vegetales detuvo una embestida. Tu extraño compañero había intervenido. Entre ambos derribasteis a la bestia. Te acercaste para examinar su carcasa dañada y las hojas que brotaban de ella. No respondisteis con palabras; cuando retomaste la marcha, los pasos mecánicos continuaron detrás de ti.

IV — CUATRO PEQUEÑAS PROMESAS
Descubriste un contenedor abandonado y te acercaste a él. La criatura vegetal abrió su cierre y recogió un objeto de apariencia orgánica que ninguno de nosotros reconocía. Lo incorporó a su cuerpo. Después de vencer a varias amenazas pareció resistir mejor los daños. Más adelante señalaste otros tres contenedores, y tu compañero recogió los tres objetos semejantes. Eran cuatro objetos en posesión de aquella criatura, no tuyos. Llegaste a una plataforma elevada cuando el camino se derrumbó; abajo, una bestia impedía pasar a tu compañero. Podías marcharte, pero regresaste y atrajiste al enemigo. Los proyectiles vegetales aprovecharon tu abertura. Volvisteis a caminar juntos. Algunas promesas no necesitan pronunciarse.

V — CUARENTA ECOS DESCONOCIDOS
La máquina halló un objeto de apariencia ósea. Te apartaste para observar cómo lo incorporaba a sus mecanismos. Durante el siguiente combate sufrió daños, pero sus raíces comenzaron a restaurar su integridad. Una luz se reunió junto al objeto, tomó la forma de un cráneo flotante y salió disparada hacia el enemigo. Observaste la aparición y dirigiste la mirada a tu compañero. Ni siquiera yo podía nombrar aquella energía. Volvieron los combates y las recuperaciones, y con ellas los cráneos. Uno, siete, dieciséis, veintiocho, treinta y siete, cuarenta. Las cuarenta apariciones nacieron del objeto que llevaba tu compañero. Tú lo protegiste con tu aguijón, interponiéndote cuando las amenazas alcanzaban sus raíces. No conocíais el nombre de esos objetos. Habíais aprendido a sobrevivir juntos.

VI — AQUELLO QUE PERMANECE
Al fin encontrasteis un suelo húmedo bajo una luz apacible. La criatura se detuvo para extender sus raíces y recuperar recursos. Continuaste unos pasos, pero miraste atrás y regresaste. Esperaste hasta que sus mecanismos volvieron a moverse. Ah, pequeño portador, no puedo conocer tus pensamientos; solo contemplar tus decisiones. Ninguna orden te obligaba a esperar. Ningún juramento os unía. Sin embargo, comenzasteis a avanzar al mismo ritmo. No sé si volverás a Hallownest, ni qué mundo es este. Mi recuerdo se desvanece, pero conservo una certeza: a veces la compañía empieza con algo tan pequeño como regresar cuando otro todavía no puede continuar. Que los sueños guarden tus pasos.";
  internal static readonly string KnightCompleteEn = @"I — A DREAM THAT DID NOT BELONG TO HALLOWNEST

Ah, little bearer... how many paths have you walked without speaking a single word? Within Hallownest's ancient stones, dreams linger long after those who dreamed them have departed. I, too, became a memory. My duty ended and my Essence dispersed. What speaks now is not the Seer who welcomed you among the graves, but a faint echo bound to the memories of your Dream Nail.

Through those memories I witnessed the beginning of another journey. You were walking through a silent gallery of Hallownest, its stones scored with old cracks, when an unfamiliar light opened where no opening had stood. It did not resemble the dreams of our kingdom. Your Dream Nail began to vibrate, and the remnants of Essence about it stirred.

I wished I could warn you, little bearer. Echoes cannot halt what is already happening. You drove your nail into the ground as the light pulled at you. For a moment you resisted. Then you disappeared. I did not see the world beyond, know the force that opened the rift or witness your passage. All that remained was silence, until your presence shone again somewhere very far away.

II — A COMPANION OF ROOTS AND METAL

When I sensed you again, the ground and sky were no longer those of Hallownest. Strange constructions stood among unfamiliar vegetation. You adjusted your cloak, took up your nail and examined the ruins. Then heavy mechanical movement echoed nearby.

Before you stood an extraordinary being: metal and machinery woven around living roots and leaves. I could not tell where the machine ended and the plant began. Its sensors lit and turned toward you. You raised your nail. The creature prepared its defenses. Neither advanced.

Then you lowered your weapon. Such a small gesture, little bearer, yet it spared this meeting a violent beginning. You approached. An exposed root lay across the path; you stopped and stepped around it rather than crushing the living tissue. The leaves grew still. Perhaps neither of you understood the other. Yet you had already found a way to show respect without words.

III — AN AGREEMENT WITHOUT WORDS

You continued through the ruins while the metal-and-root creature moved along a nearby route. Its footsteps were heavy; its roots often reached for the soil. For some time neither of you drew too close.

Then the ground shook. A huge beast emerged and charged toward your strange companion. The machine began to retreat. You could have continued without it. You chose otherwise.

You placed yourself in the creature's path, raised your nail and dodged its first strike. Your quick cuts drew its attention. When it prepared another charge, a volley of plant projectiles struck it from the side. Your companion had come to your aid. You evaded, returned to the fight and together brought the beast down.

You inspected the machine's damaged casing, then looked toward the leaves growing through its mechanisms. Its sensors inclined toward you. Neither spoke. Yet when you resumed walking, its heavy movements followed behind. This time you did not try to leave it behind.

IV — FOUR LITTLE PROMISES

This world held objects unlike any charm or relic I could name. You found a container amid the ruins and examined its mechanism. The plant-and-metal being came forward and opened it, recovering a small organic-looking artifact. I did not recognize it. Your companion incorporated it into its machinery.

After several battles, the creature seemed better able to endure harm. Whatever the artifact was, its effect grew more apparent when enemies were defeated. You located three more containers of the same kind. Each time, your companion recovered the object while you waited. Four strange artifacts now belonged to the being of roots and metal. None was carried by you.

Then a section of the path gave way. You jumped onto a raised platform, but your companion was left below, its route blocked by a massive hostile. The way ahead of you was clear. You looked forward, then back toward the creature struggling below. And you returned.

You struck the enemy's forelimb with your nail to draw its attention. The beast turned toward you. A volley of plant projectiles struck it from the side; your companion used the opening you had made. Together you brought it down and cleared the way. No thanks were spoken, no vow sworn. You simply walked on together. Some promises, little bearer, need never be said aloud.

V — FORTY ECHOES OF UNKNOWN ENERGY

Later you discovered another artifact amid the abandoned structures. It looked almost like bone and glimmered with a kind of energy I could not identify. It was no charm from Hallownest. You inspected it and stepped aside while your companion approached, recovered it and incorporated it into its mechanical support.

During another battle, a hostile creature struck the machine. Its casing shook and its vegetation contracted. The being withdrew. Then roots and mechanisms began to restore the damage. Strange energy gathered around the new artifact and took the shape of a little floating skull. It flew toward the enemy. You stood still briefly, following its path, then looked at your companion. Its sensors turned toward you. Neither offered an explanation, nor could I have given one.

The following battles brought further wounds and recoveries. Whenever enough energy had accumulated during the creature's healing, another skull emerged and sought a nearby threat. One. Seven. Sixteen. Twenty-eight. Thirty-seven. Forty. Forty manifestations of a force we did not understand.

All that while, you fought beside the artifact's bearer. You intercepted enemies trying to strike its roots, cleared narrow paths and returned when the machine could not advance. Your companion answered with its plant projectiles and the strange power awakened by its own recovery. The earlier four artifacts strengthened its vitality; the fifth turned a portion of its healing into an attack. You did not know their names, and neither did I. You had nevertheless found a way to survive together.

VI — THAT WHICH REMAINS

At last, the fighting gave way to calm. You reached a place where sunlight touched moist earth. Your companion slowed and lowered its roots into the soil to gather what its living half needed. You walked on a few steps, stopped and turned. The path before you was clear. No enemy waited. No obstacle compelled you to return.

And yet you came back and waited beside it.

Ah, little bearer... Your silence has always guarded secrets even from those who wished to understand you. I cannot tell you what you felt, but I can remember what you did. When the machine moved again, you waited until its steps matched yours. A small shadow with a dark cloak and a creature of metal sustained by living roots continued together among the ruins.

I could not know where your path would end, whether you would ever return to Hallownest, or what power had carried you here. But I had witnessed something that no ancient law could teach: companionship needs neither resemblance nor words. Sometimes all that matters is returning when another cannot go on.

As you disappeared into the distance, the light through which I perceived you faded. My own duty had ended long before this journey. All I could do was preserve the memory. May dreams keep watch over your footsteps, little bearer, and may you find other roads worth walking in company.";
 }
}
