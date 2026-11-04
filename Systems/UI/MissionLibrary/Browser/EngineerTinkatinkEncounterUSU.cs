using System;
namespace UniversalSurvivorUnlocks
{
    // Historia de encuentro posterior al comunicado de Captain.
    // NO sustituye el Diario I original ni el Diario II de USU.
    internal static class EngineerTinkatinkEncounterUSU
    {
        internal static string Append(string currentNotes, bool spanish)
        {
            string title = spanish ? @"Encuentro I — Prohibido tocar las torretas" : @"Encounter I — Keep Your Hands Off the Turrets";
            string body = spanish ? Spanish : English;
            return (currentNotes ?? string.Empty) + "\n\n<size=18><b>" + title + " (USU)</b></size>\n" + body;
        }
        private const string Spanish = @"El mensaje de Captain llegó mientras revisaba mis equipos después de otra jornada de reconocimiento. Habían establecido un campamento y solicitaban los registros de los grupos dispersos. Era una buena noticia, aunque todavía necesitaba encontrarlo. La transmisión incluía referencias de navegación, no un mapa completo. Comparé señales, descarté rutas peligrosas y emprendí el camino. No tardé en escuchar pequeños golpes metálicos detrás de mí. Al detenerme, cesaban. Instalé una torreta para descubrir qué me seguía y apareció una criatura rosada, diminuta, sujetando un martillo improvisado. Estaba tan fascinada con los mecanismos que parecía dispuesta a desmontarlos.

—¡Eh! ¡No toques eso!
—¡Tin!
—Tranquila, tranquila. No voy a hacerte daño.
—¿Tin...?
—Mira. Esto puedes quedártelo. Pero las torretas son mías, ¿entendido?

Dio un salto y se escondió detrás del martillo, a punto de llorar. Me agaché, le ofrecí una pieza suelta y esperé. La recogió con cautela y comenzó a estudiarla, olvidándose por un momento de mi torreta. Al reanudar la marcha, volvió a seguirme. Durante el recorrido nos atacaron criaturas mayores que ella; la pequeña se refugió entre las rocas mientras mis defensas mantenían la posición. Cuando terminó el combate, salió a recoger metal y regresó junto a mí. No sabía qué era ni de dónde venía. Pero dejarla sola no parecía una buena idea. Llegamos juntos al campamento. Allí preparé una caja con materiales descartados para que no confundiera nuestras defensas con una cantera. Pronto descubrí que no acumulaba metal al azar: reconocía pesos, probaba uniones y modificaba su herramienta. Comencé a mostrarle cómo reparar componentes. Se frustraba y lloraba cuando algo se rompía, pero siempre volvía a intentarlo. Con el tiempo construyó otro martillo, mayor y más resistente, aunque demasiado pesado para sus brazos.

Durante una de nuestras expediciones, un enemigo enorme se acercó mientras reparaba un dispositivo. La pequeña retrocedió, temblando, y sujetó aquel martillo que todavía no había conseguido levantar bien. Yo intenté preparar las defensas, pero la criatura ya nos tenía encima. Entonces mi acompañante se colocó delante de mí.

—¡Aléjate! ¡Es demasiado grande para ti!
—¡Tin... tink!
—¡TINKAAA!
—¿Qué... acaba de pasar?
—¡Tinka-tuff!
—Bueno... parece que ahora sí puedes utilizarlo.
—¡Tuff!

Alzó el martillo con un esfuerzo desesperado y golpeó al atacante. Una luz la envolvió; cuando se disipó, había crecido y sostenía con firmeza la herramienta que antes arrastraba. No tenía idea de qué acababa de presenciar. Aquel grito me sonó a «Tinkatuff»; terminé usándolo para dirigirme a ella, aunque ignoraba si era realmente su nombre. Nuestra nueva fuerza de trabajo fue una bendición para las reparaciones y una pesadilla para las torretas. Una tarde encontré una desmontada. Mi compañera sostenía orgullosamente una pieza modificada, sin comprender por qué me había enfadado.

—¡Te dije que no tocaras las torretas!
—¡Tinka...!
—¿Lo arreglaste tú?
—...Tuff.
—Ven. Creo que podemos terminarlo juntos.

Bajó el martillo y se marchó del taller. Pasaron horas antes de que regresara. Había reparado como pudo uno de los componentes dañados: el trabajo era torpe, pero funcionaba. No necesitaba comprender sus sonidos para reconocer un intento de enmendar lo ocurrido. Trabajamos juntos para terminar la reparación. Más adelante, durante otra expedición, encontramos restos de naves y numerosos constructos mecánicos. Habíamos recogido seis piezas de chatarra que ella reservaba para un martillo todavía más grande. Las utilizó para acabar su nueva creación mientras mis torretas retenían a los enemigos. Cuando apareció una máquina descomunal, intentó levantar el martillo terminado, pero su peso la venció.

—¡Tenemos que retirarnos! ¡No podemos detener eso!
—¡Tinka... tuff!
—¡Déjalo! ¡Yo me encargo!
—¡TINKATUFF!
—¡Eso sí que es una mejora!
—¡Tinka-ton!

Se negó a abandonar la herramienta. Sujetó el mango otra vez y consiguió alzarla, aunque todo su cuerpo temblaba. La luz volvió a envolverla. Al desaparecer, era más grande, más segura y podía manejar aquel martillo gigantesco sin arrastrarlo. De un golpe derribó al constructo. Sus vocalizaciones habían cambiado y sonaban ahora a «Tinkaton». Después de oírlas varias veces, empecé a llamarla así; parecía responder, aunque nunca supe si estaba pronunciando su nombre. Tiempo después encontramos un dron de transporte averiado. Esta vez trabajó conmigo en lugar de desmantelarlo para conseguir piezas. Volvimos con suministros y preparé una caja rotulada «PROYECTOS COMPARTIDOS». La pequeña desconocida de las rocas se había convertido en mi compañera de taller. Sin embargo, su fascinación por mis defensas seguía intacta.

—Tinkaton.
—¿Tinka?
—Las torretas no se tocan.
—¡Tinka-ton!

Se giró hacia una de mis máquinas con una expresión que ya conocía demasiado bien. No sé si aquella última respuesta fue una promesa o el anuncio de una nueva reparación involuntaria.";
        private const string English = @"Captain's message reached me while I was checking my equipment after another day of reconnaissance. A camp had been established, and the scattered teams were asked to send their reports. Good news, though I still needed to find the place. The transmission contained navigation references, not a complete map. I compared signals, ruled out unsafe routes, and set off. Soon I heard small metallic knocks behind me. Every time I stopped, they stopped too. I deployed a turret to see what was following me. A tiny pink creature appeared, gripping a makeshift hammer and staring at my machinery as though it were about to take it apart.

—Hey! Don’t touch that!
—Tin!
—Easy, easy. I’m not going to hurt you.
—Tin...?
—Look. You can have this. But the turrets are mine, understood?

She jumped and hid behind her hammer, almost crying. I crouched, offered a loose metal scrap, and waited. She took it cautiously and studied it, forgetting the turret for a moment. When I resumed walking, she followed. Larger creatures attacked along the route; the little one sheltered among rocks while my defenses held them back. Afterward she gathered scrap and returned to me. I had no idea what she was or where she came from, but leaving her alone seemed unwise. We reached the camp together. I gave her a box of discarded parts so she wouldn't treat our defenses as a scrapyard. Soon I realized she wasn't collecting metal at random: she tested weights, joints, and improvements to her hammer. I started showing her how to fix components. She would cry when something failed, then try again. Eventually she built a stronger hammer, too heavy for her small arms.

During one of our expeditions, an enormous enemy approached while I was repairing a device. The little creature retreated, shaking, and gripped the hammer she still couldn't lift properly. I tried to deploy our defenses, but the thing was already upon us. Then she stepped in front of me.

—Get back! It’s too big for you!
—Tin... tink!
—TINKAAA!
—What... just happened?
—Tinka-tuff!
—Well... looks like you can use it now.
—Tuff!

She lifted the hammer with a desperate effort and struck the attacker. Light surrounded her. When it faded, she had grown and could hold the tool she had once dragged behind her. I didn't understand what I'd just seen. Her cry sounded something like “Tinkatuff,” so I began calling her that, though I didn't know if it was a name. Her new strength helped with repairs and was a nightmare for my turrets. One afternoon I found one dismantled. She proudly held up a modified part, unable to see why I was angry.

—I told you not to touch the turrets!
—Tinka...!
—Did you fix this yourself?
—...Tuff.
—Come here. I think we can finish it together.

She lowered the hammer and left the workshop. Hours passed before she returned. She had tried to repair one damaged component: rough work, but it functioned. I didn't need to understand her cries to recognize an attempt to make things right. We finished the repair together. Later, on another expedition, we found a wreckage-strewn region filled with mechanical constructs. She had saved six pieces of scrap for an even larger hammer. She used them to finish it as my turrets kept the enemies back. When a colossal machine approached, she tried to lift the completed weapon, but its weight defeated her.

—We need to fall back! We can’t stop that!
—Tinka... tuff!
—Leave it! I’ll handle this!
—TINKATUFF!
—Now that is an upgrade!
—Tinka-ton!

She refused to abandon her work. She gripped the handle again and managed to lift it, though her entire body shook. Light surrounded her once more. When it vanished, she was larger, more confident, and able to wield the enormous hammer without dragging it. One blow brought the construct down. Her cries had changed; they now sounded like “Tinkaton.” After hearing them several times, I began using that name. She seemed to respond, though I never knew whether she was speaking her own name. Later we discovered a broken transport drone. This time she helped repair it rather than stripping it for parts. We returned with supplies, and I labeled a box “SHARED PROJECTS.” The stranger from the rocks had become my workshop partner. Her fascination with my defenses, however, remained.

—Tinkaton.
—Tinka?
—No touching the turrets.
—Tinka-ton!

She turned toward one of my machines with an expression I knew all too well. I couldn't tell whether her reply was a promise or a warning of another accidental repair.";
    }
}
