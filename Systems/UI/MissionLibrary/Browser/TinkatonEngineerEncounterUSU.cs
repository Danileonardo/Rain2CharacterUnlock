using System;
namespace UniversalSurvivorUnlocks
{
    // Historia de encuentro posterior al comunicado de Captain.
    // NO sustituye el Diario I original ni el Diario II de USU.
    internal static class TinkatonEngineerEncounterUSU
    {
        internal static string GetBody(bool spanish) { return spanish ? Spanish : English; }
        internal static string Append(string currentNotes, bool spanish)
        {
            string title = spanish ? @"Encuentro I — El martillo de una nueva aventura" : @"Encounter I — A Hammer for a New Adventure";
            string body = spanish ? Spanish : English;
            return (currentNotes ?? string.Empty) + "\n\n<size=18><b>" + title + " (USU)</b></size>\n" + body;
        }
        private const string Spanish = @"Hay Pokémon que consideran su martillo tan importante como cualquier otra parte de su vida. Tinkatink fabrica el suyo reuniendo fragmentos de metal y puede angustiarse cuando una creación se rompe. En un planeta desconocido, una Tinkatink observaba a un humano cubierto de herramientas. Él escuchó un dispositivo del que salían voces y comenzó a caminar siguiendo sus instrumentos. Ella no comprendía el mensaje, pero sus máquinas móviles resultaban irresistibles. Lo siguió entre las rocas, golpeando accidentalmente el suelo con su pequeño martillo. Cuando el humano desplegó una de aquellas máquinas, se acercó a examinarla.

—¡Eh! ¡No toques eso!
—¡Tin!
—Tranquila, tranquila. No voy a hacerte daño.
—¿Tin...?
—Mira. Esto puedes quedártelo. Pero las torretas son mías, ¿entendido?

La voz la sobresaltó. Se escondió tras su martillo, temblando y al borde del llanto. El humano le ofreció un fragmento de metal que no pertenecía a la máquina. Tinkatink lo olfateó, lo tomó y lo probó con pequeños golpes. Cuando él se marchó, continuó detrás de sus herramientas. Un ataque la hizo esconderse, pero las defensas del desconocido mantuvieron alejadas a las criaturas. Después recogió algunas piezas y regresó junto a él. Llegaron a un asentamiento lleno de generadores y materiales. Tinkatink comenzó a examinar todo cuanto brillaba. El humano le proporcionó una caja de restos y le mostró cómo reforzar uniones y mantener mecanismos en funcionamiento. Ella practicaba, se frustraba cuando una pieza se rompía y volvía a intentarlo. Con el tiempo consiguió construir un martillo más resistente. Había un problema: todavía no podía levantarlo con seguridad.

Durante una excursión, un enemigo enorme sorprendió al humano mientras reparaba un dispositivo. Tinkatink se asustó y retrocedió, pero vio que las defensas aún no estaban listas. El martillo nuevo y demasiado pesado yacía a su lado. Se colocó entre el atacante y el humano.

—¡Aléjate! ¡Es demasiado grande para ti!
—¡Tin... tink!
—¡TINKAAA!
—¿Qué... acaba de pasar?
—¡Tinka-tuff!
—Bueno... parece que ahora sí puedes utilizarlo.
—¡Tuff!

Con todas sus fuerzas levantó su creación y consiguió hacer retroceder al enemigo. Una luz envolvió su cuerpo. Al desaparecer, Tinkatuff sostenía el martillo con estabilidad. Su nueva fuerza le permitió trabajar con metales mayores, pero también despertó nuevas dificultades. En una ocasión desmontó una máquina defensiva para mejorar uno de sus mecanismos. Cuando el humano la encontró, ella le mostró orgullosa el resultado; la reacción no fue la que esperaba.

—¡Te dije que no tocaras las torretas!
—¡Tinka...!
—¿Lo arreglaste tú?
—...Tuff.
—Ven. Creo que podemos terminarlo juntos.

El entusiasmo de Tinkatuff desapareció. Se retiró con el martillo bajo y, durante las horas siguientes, intentó reparar uno de los componentes que había dañado. Regresó para dejarlo ante el humano. Su comportamiento sugería que empezaba a comprender que una pieza también podía valer por la función que cumplía para su compañero. Él aceptó ayudarla a terminar el trabajo. Tiempo después, en una región cubierta de restos metálicos, Tinkatuff utilizó seis piezas de chatarra para completar un martillo gigantesco. Las máquinas defensivas del humano los protegían mientras ella lo terminaba. Cuando una enorme máquina enemiga se acercó, quiso levantar su obra, pero era demasiado pesada.

—¡Tenemos que retirarnos! ¡No podemos detener eso!
—¡Tinka... tuff!
—¡Déjalo! ¡Yo me encargo!
—¡TINKATUFF!
—¡Eso sí que es una mejora!
—¡Tinka-ton!

Aun así, Tinkatuff no abandonó el martillo. Volvió a intentarlo, tembló por el esfuerzo y finalmente lo elevó. Una nueva luz la envolvió: al desaparecer era Tinkaton, fuerte y capaz de blandir su creación sin dificultad. El constructo cayó ante su golpe. Su entusiasmo por el metal seguía siendo el mismo, pero sus acciones habían cambiado. Más adelante, al encontrar un dron de transporte estropeado, eligió piezas adecuadas y ayudó al humano a repararlo en lugar de desmontarlo. Al volver al asentamiento vio una caja etiquetada «PROYECTOS COMPARTIDOS». No podemos afirmar que comprendiera aquellas palabras, pero dejó su martillo junto a las herramientas de su compañero con una expresión satisfecha. Aun así, las máquinas defensivas continuaban despertando su curiosidad.

—Tinkaton.
—¿Tinka?
—Las torretas no se tocan.
—¡Tinka-ton!

El humano volvió a advertirle que no tocara sus torretas. Tinkaton emitió un sonido alegre y miró de reojo la máquina más cercana. Su comportamiento no permitía asegurar si había comprendido la prohibición, pero sí que aquel taller todavía tenía mucho que ofrecerle.";
        private const string English = @"Some Pokémon treat their hammers as an essential part of life. Tinkatink gathers metal scraps to build her own and can become distressed when her work breaks. On an unfamiliar planet, a Tinkatink watched a human carrying many tools. He listened to a device speaking in unfamiliar voices and walked off, following his instruments. She didn't understand the message, but his moving machines were irresistible. She followed him through the rocks, occasionally tapping the ground with her small hammer. When he deployed one of those machines, she approached to investigate.

—Hey! Don’t touch that!
—Tin!
—Easy, easy. I’m not going to hurt you.
—Tin...?
—Look. You can have this. But the turrets are mine, understood?

His voice startled her. She hid behind her hammer, trembling and nearly in tears. The human offered a scrap of metal that didn't belong to the machine. Tinkatink inspected it, took it, and tried a few light taps. When he walked on, she followed the tools. An attack sent her into hiding, but the stranger's defenses held the creatures off. Afterward she collected scraps and returned to him. They reached a settlement full of generators and materials. Tinkatink wanted to inspect everything that shone. The human gave her a box of scrap and showed her how to strengthen joints and keep mechanisms working. She practiced, grew frustrated when a piece broke, and tried again. In time she built a stronger hammer. One problem remained: she still couldn't lift it safely.

On an outing, a huge enemy surprised the human while he was repairing a device. Tinkatink recoiled, frightened, but saw that his defenses were not ready. Her new, overly heavy hammer lay nearby. She stepped between the attacker and the human.

—Get back! It’s too big for you!
—Tin... tink!
—TINKAAA!
—What... just happened?
—Tinka-tuff!
—Well... looks like you can use it now.
—Tuff!

With all her strength she lifted her creation and forced the enemy back. Light surrounded her. When it faded, Tinkatuff could hold the hammer steadily. Her new strength made larger materials accessible, but brought fresh trouble. One day she dismantled a defensive machine to improve one of its mechanisms. When the human found her, she proudly showed him the result; his reaction wasn't what she expected.

—I told you not to touch the turrets!
—Tinka...!
—Did you fix this yourself?
—...Tuff.
—Come here. I think we can finish it together.

Tinkatuff's enthusiasm vanished. She retreated, hammer lowered, and spent the following hours trying to repair a component she had damaged. She returned and placed it before the human. Her behavior suggested she was beginning to appreciate that a part mattered for what it did for her companion as well as for its metal. He helped her finish the repair. Later, in a region filled with wreckage, Tinkatuff used six pieces of scrap to finish a gigantic hammer. The human's defenses protected them while she worked. When a huge enemy machine approached, she tried to lift her creation, but it was too heavy.

—We need to fall back! We can’t stop that!
—Tinka... tuff!
—Leave it! I’ll handle this!
—TINKATUFF!
—Now that is an upgrade!
—Tinka-ton!

Still, Tinkatuff did not abandon the hammer. She tried again, trembling with effort, and finally raised it. Light surrounded her once more: when it faded, she was Tinkaton, strong enough to wield her creation easily. The construct fell to her strike. Her love of metal remained, but her actions had changed. Later, they found a damaged transport drone, and she chose suitable pieces to help the human repair it instead of taking it apart. Back at the settlement, she saw a box labeled “SHARED PROJECTS.” We cannot know whether she understood those words, but she placed her hammer beside her companion's tools with evident satisfaction. His defensive machines still fascinated her.

—Tinkaton.
—Tinka?
—No touching the turrets.
—Tinka-ton!

The human reminded her once more not to touch the turrets. Tinkaton chirped happily and glanced at the nearest one. Her behavior did not reveal whether she understood the warning, only that the workshop still held countless possibilities.";
    }
}
