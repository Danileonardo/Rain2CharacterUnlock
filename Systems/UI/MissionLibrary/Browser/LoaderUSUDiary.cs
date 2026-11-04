using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    // Loader Diary II is a USU-only supplement, not a replacement for the original Logbook.
    internal static class LoaderUSUDiary
    {
        private static readonly Dictionary<string, string> Es = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commando", @"Entre los restos encontré a un soldado con armadura y dos pistolas. Reconocí el equipamiento de la expedición, aunque no recordaba haber conversado con él antes. Todavía vigilaba el cielo.
—¡Oye! ¡Menudo espectáculo! ¿Has derribado tú solo esa cosa?
—Sí. Apártate de los restos. Podrían seguir activos.
—¡Descuida! Trabajo con maquinaria pesada. Aunque nunca había visto un aparato así.
—¿Eres parte del equipo de recuperación?
—Loader. Operadora de carga. ¿Tú?
—Commando. Estoy reconociendo la zona.
—¡Pues encontraste algo que merece un informe enorme!
—Y todavía pueden quedar enemigos.
—¡Está bien, está bien! Primero aseguramos el lugar. Después miramos la chatarra.
—Tenemos que continuar al siguiente sector.
—¡Perfecto! A ver si puedes seguirme el ritmo.

El soldado no parecía compartir mi entusiasmo por las piezas recuperables, pero sabía mantener la calma y vigilar el terreno. Cuando completamos el evento del teletransportador, seguimos avanzando juntos." },
            { "Huntress", @"Una figura con un arco luminoso observaba los restos desde una plataforma. Su equipo me resultó familiar por los registros de la expedición, aunque no sabía si ella me reconocería.
—¡Eh! ¡Eso sí que fue una buena cacería!
—Hizo bastante ruido.
—¡Imagínate si hubiera caído encima de una nave entera! ¿Eres de Safe Travels?
—Sí.
—¡Soy Loader, del equipo de carga!
—Huntress.
—¿Sabes qué hizo aparecer a esa cosa?
—Los nidos. Algo reaccionó cuando fueron destruidos.
—¿Un robot gigantesco protegiendo huevos?
—Eso parece.
—¡Este planeta es fantástico!
—También es peligroso. Hay movimiento a nuestra izquierda.
—¡Entonces terminemos lo que falta y salgamos de aquí!

Mientras yo revisaba las estructuras cercanas, ella vigiló las rutas de aproximación. Después activamos el teletransportador y avanzamos." },
            { "Bandit", @"Un hombre con sombrero y abrigo rebuscaba entre los restos. Su escopeta y su interés por el botín me hicieron sospechar quién era.
—¡Vaya! ¿Fuiste tú quien desmontó esa chatarra?
—Señorita, algunos tenemos talento para resolver problemas costosos.
—¡Pues has dejado piezas por todas partes!
—¿Cuánto crees que pagarían por ellas?
—¡Yo quiero descubrir cómo funcionan!
—Qué desperdicio de espíritu comercial.
Levanté un componente con mis guanteletes; retrocedió de un salto.
—¡Avisa antes de mover media montaña!
—¿Te preocupa que te aplaste el sombrero?
—Es un sombrero excelente. Merece respeto.
—¿Tú eres el polizón del que hablan algunos?
—Prefiero «especialista independiente en adquisición de recursos».
—¡Pues ven a adquirir unos cuantos enemigos menos mientras terminamos el teletransportador!
—¿Puedo quedarme con lo que encontremos?
—¡Solo si puedes cargarlo!

Vigilé mis pertenencias mientras terminábamos el evento. Cuando cruzamos el siguiente portal, él seguía hablando de dinero y yo de maquinaria." },
            { "MUL-T", @"Junto a los restos había una unidad industrial con herramientas y armamento incorporados.
—¡Eso sí que fue un trabajo bien hecho! ¿Fuiste tú?
BIP-BIP. [Señala los restos.]
—¡Sabía que eras tú! ¿Qué te parecen esos motores?
BIP. [Analiza una pieza.]
—¿Podrías cargar eso? Yo sí puedo. ¡Mira!
Enganché un componente enorme con el cabrestante y lo alcé.
BIP-BIP-BIP. [Advertencia de estabilidad.]
—¡Tranquilo! Conozco los límites de mi equipo.
Una sujeción crujió. Bajé la pieza inmediatamente.
—...Bueno, casi todos.
BIP.
—¡No te rías! ¿O sí estabas riéndote?
La máquina señaló el camino hacia el teletransportador.
—Está bien, compañero. ¡Vamos a ver qué más encontramos!

Terminamos de preparar el teletransportador y partimos hacia el siguiente lugar." },
            { "Engineer", @"Un hombre con equipamiento técnico de la expedición examinaba los restos.
—¡Oye! ¡Espero que hayas dejado algo para mí!
—¡Hay suficientes piezas para estudiar durante días!
—¡Por fin alguien que entiende lo importante! ¿Viste los propulsores?
—No pude obtener lecturas completas. Estaba intentando sobrevivir.
—¡Podríamos usar esos motores para levantar contenedores!
—Primero habría que comprobar que no exploten.
—Detalles, detalles.
Ambos miramos una pieza que todavía desprendía chispas.
—...Aunque sí sería interesante desmontarla.
—¡Lo sabía! Soy Loader. Equipo de carga.
—Engineer. Supongo que ya lo habrás deducido.
—¡Entonces sabes por qué no quiero dejar esto aquí!
—Y tú sabes por qué tenemos que hacerlo. El siguiente sector nos espera.
—¡Está bien! Pero recordaré dónde están las piezas.

Seguimos intercambiando ideas mientras terminábamos el teletransportador." },
            { "Artificer", @"Una mujer con traje azul flotaba sobre el terreno. Su equipo coincidía con las descripciones de una exploradora incorporada recientemente.
—¡Eso sí que fue una forma espectacular de desarmarla!
—Su estructura resultó menos resistente de lo que parecía.
—¡Era gigantesca!
—El tamaño no siempre determina la resistencia.
—¡Eso diría alguien capaz de derretir un robot!
Artificer descendió y examinó un fragmento.
—No reconozco todos sus materiales.
—¡Entonces llevémonos algunos! Tengo equipo para moverlos.
—Antes comprobaremos que no conservan energía residual.
—Seguridad primero. Luego levantamos cosas.
—Parece que disfrutas especialmente esa parte.
—¡Es la mejor! Soy Loader, por cierto.
—Artificer.
—¡Espera a verme usar los ganchos!

Después de examinar los restos, ambas teníamos motivos para atravesar el siguiente portal." },
            { "Mercenary", @"Un espadachín estaba junto a los restos, con la espada envainada.
—¡Espera! ¿Derribaste esa cosa con una espada?
—Sí.
—¡Increíble! ¿Sabes cuánto metal llevaba encima?
—Suficiente.
—¡No eres muy conversador!
—No especialmente.
Señalé una de las piezas más grandes.
—¿Me ayudas a mover eso?
—¿Para qué?
—¡Para ver qué hay debajo!
El espadachín miró el teletransportador.
—Tenemos que continuar.
—¡Me debes una visita a este lugar!
—No prometí nada.
—¡Lo tomaré como un quizá!

Lo acompañé hacia el siguiente portal, todavía preguntándome de qué más era capaz su espada." },
            { "Captain", @"Reconocí el uniforme de mando y la escopeta del capitán.
—¡Capitán! ¿Ha visto el tamaño de esa cosa?
—La he visto bastante de cerca, gracias.
—¡Tiene que dejarme revisar los motores!
—Primero asegurémonos de que no queda nada intentando dispararnos.
—¡Ah, vamos! ¿Cuándo encontraremos otra máquina así?
—Con nuestra suerte, antes de lo que quisiera.
Me reí.
—¡Entonces necesitará a alguien que mueva las piezas grandes!
—Para eso te contrataron. Entre otras cosas.
—¡Una misión perfecta!
—No dije que pudieras llevarte la máquina entera.
—...¿Y la mitad?
—Loader.
—¡Está bien! Primero terminamos el teletransportador.

Cuando el portal estuvo listo, seguimos adelante. Yo aún pensaba en aquellos motores." },
            { "Acrid", @"Entre los fragmentos encontré una criatura enorme de piel violeta. Olfateaba componentes y de sus garras caían gotas verdosas.
—¡Santo cielo! ¿Tú derribaste esa cosa?
La criatura respondió con un gruñido.
—¡Vaya dientes! No me extraña que el robot terminara hecho pedazos.
Una gota verde cayó sobre el metal. Retrocedí.
—¡Eh! ¡Cuidado con eso!
Acrid se acercó a olfatear un guantelete.
—¡Ni se te ocurra! ¡He trabajado demasiado en estas maravillas para que acaben cubiertas de ácido!
Mostró los dientes. Activé los guanteletes, lista para defenderme.
—Tranquilo, grandullón. Solo mantén eso lejos de mi equipo.
La criatura volvió hacia los restos.
—Tú inspecciona con los dientes y yo con las herramientas.

Mantuve la distancia y crucé el portal cuando terminé el evento. No intenté convencer a aquella criatura de acompañarme." },
            { "REX", @"Entre los restos encontré una máquina pequeña con vegetación viva integrada en su estructura.
—¡Vaya! ¿Fuiste tú quien derribó a ese monstruo?
La unidad orientó sus sensores hacia mí y emitió una señal.
—¿Eso es una planta de verdad?
Me incliné para observarla. Una herramienta apareció entre mis guanteletes y el cultivo.
—¡Tranquilo! No voy a quitártela.
La máquina comprobó sus sistemas hidropónicos.
—He transportado cargas enormes, pero nunca había visto una máquina tan preocupada por algo tan pequeño.
REX orientó sus sensores hacia las hojas.
—Ah... Ya entiendo. Para ti es lo más importante que llevas.
Me aparté para dejarle espacio.
—Está bien, jardinero. No tocaré tu carga.
La unidad emitió una señal breve.
—¡Lo tomaré como un gracias!

REX parecía tener prioridades propias. Se alejó por otra ruta; yo regresé al teletransportador. No todas las cargas necesitaban que yo las transportara." },
        };
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commando", @"An armored soldier with two pistols was watching the sky. I recognized expedition equipment, though we had never spoken.
—Hey! That was quite a show! Did you take it down yourself?
—Yes. Keep clear of the wreckage. It might still be active.
—Don't worry. I work with heavy machinery. Never seen one quite like that.
—Recovery crew?
—Loader. Cargo operator. And you?
—Commando. Reconnaissance.
—Then you've found something worth a huge report!
—There could be more enemies.
—Fine, fine! Secure the site first, inspect the scrap later.
—We need to move on.
—Perfect. Try to keep up!

He knew how to stay alert, even if he didn't share my excitement. We finished the teleporter event and continued together." },
            { "Huntress", @"A woman with a glowing bow watched the wreck from a platform. I recognized her equipment from the expedition's records.
—Hey! That was quite a hunt!
—It made a lot of noise.
—Imagine it landing on a whole ship! Safe Travels?
—Yes.
—I'm Loader. Cargo crew!
—Huntress.
—What made that thing appear?
—The nests. Something reacted when they were destroyed.
—A gigantic robot defending eggs?
—Looks that way.
—This planet is incredible!
—Also dangerous. Movement on our left.
—Then let's finish up and get out of here!

I inspected the structures while she watched for threats. Then we activated the teleporter and moved on." },
            { "Bandit", @"A man in a hat was searching the wreck for valuables. His shotgun and his interest in loot made me suspect who he was.
—Wow! Did you dismantle that pile of scrap?
—Miss, some of us have a gift for solving expensive problems.
—You left pieces everywhere!
—How much could they sell for?
—I want to know how they work!
—What a waste of commercial spirit.
I lifted a component with my gauntlets; he jumped backward.
—Warn me before moving half a mountain!
—Worried about your hat?
—An excellent hat. Deserves respect.
—Are you that stowaway people have mentioned?
—I prefer independent resource acquisition specialist.
—Come acquire fewer enemies while we finish the teleporter!
—Can I keep whatever we find?
—Only if you can carry it!

I kept an eye on my belongings as we finished up. Through the next portal, he kept talking money and I kept talking machinery." },
            { "MUL-T", @"An industrial unit with integrated tools and weapons stood beside the wreck.
—Now that's a job well done! Was that you?
BEEP-BEEP. [Points at the wreck.]
—I knew it! What do you think of those engines?
BEEP. [Scans a component.]
—Could you lift that? I can. Watch!
I hoisted a huge piece with my winch.
BEEP-BEEP-BEEP. [Stability warning.]
—Relax! I know my equipment's limits.
A restraint creaked. I lowered the piece.
—...Well, almost all of them.
BEEP.
—Don't laugh! Were you laughing?
The machine pointed to the teleporter.
—All right, partner. Let's see what else is out there!

We finished the teleporter and moved on." },
            { "Engineer", @"A man in expedition engineering gear was inspecting the wreck.
—Hey! Hope you left something for me!
—Enough pieces here to study for days!
—Someone who understands! Did you see those thrusters?
—Couldn't take readings. I was busy surviving.
—We could lift containers with those engines!
—First we'd need to check they don't explode.
—Details, details.
We stared at a sparking fragment.
—...It would be interesting to dismantle.
—Knew it! Loader. Cargo team.
—Engineer. Probably guessed that.
—Then you know why I don't want to leave!
—And why we have to. Next sector awaits.
—Fine! I'll remember where we left the pieces.

We traded ideas as we finished the teleporter." },
            { "Artificer", @"A woman in a blue suit hovered over the ground. Her gear matched descriptions of an explorer who had recently joined the expedition.
—That was a spectacular way to take it apart!
—Its structure was less durable than it appeared.
—It was gigantic!
—Size isn't always strength.
—Spoken like someone who just melted a robot!
She descended to examine a fragment.
—I don't recognize all its materials.
—Then let's take some! I've got the equipment.
—First we check for residual energy.
—Safety first. Then we lift things.
—You really like lifting things.
—Best part of the job! Loader, by the way.
—Artificer.
—Wait until you see my grappling hooks!

After inspecting the wreck, we both had reasons to cross the next portal." },
            { "Mercenary", @"A swordsman stood by the wreck, blade sheathed.
—Wait! Did you take that down with a sword?
—Yes.
—Incredible! Know how much metal it carried?
—Enough.
—You're not much of a talker!
—Not especially.
I pointed to a huge fragment.
—Help me shift that?
—Why?
—To see what's underneath!
He looked toward the teleporter.
—We need to move on.
—You owe me another visit here!
—I promised nothing.
—I'll take that as a maybe!

I followed him toward the portal, wondering what else his sword could do." },
            { "Captain", @"I recognized the command uniform and shotgun.
—Captain! Did you see the size of that thing?
—I saw it rather close up, thank you.
—You have to let me inspect those engines!
—First let's make sure nothing else wants to shoot at us.
—When will we ever see another machine like that?
—With our luck, sooner than I'd like.
I laughed.
—Then you'll need someone to move the big pieces!
—That's why you were hired. Among other things.
—Perfect mission!
—I didn't say you could take the whole machine.
—...Half?
—Loader.
—Fine! Teleporter first.

When the portal was ready we moved on. I was still thinking about those engines." },
            { "Acrid", @"Among the fragments I found a huge creature with violet skin. It sniffed the wreckage, green droplets falling from its claws.
—Good grief! Did you take that thing down?
It growled.
—Those teeth! No wonder the robot's in pieces.
A green drop fell on the metal. I stepped back.
—Hey! Careful with that!
Acrid moved to sniff a gauntlet.
—Don't even think about it! I worked too hard on these things to coat them in acid!
It bared its teeth. I readied my gauntlets.
—Easy, big guy. Just keep that stuff away from my gear.
The creature returned to the debris.
—You inspect with your teeth, I'll use my tools.

I kept my distance and took the portal after the event, without trying to make it follow me." },
            { "REX", @"I found a small machine with living vegetation integrated into its chassis.
—Wow! Did you take that monster down?
The unit turned its sensors toward me and signaled.
—Is that a real plant?
I leaned closer. A tool moved between my gauntlets and the plant.
—Easy! I'm not taking it.
The machine checked its hydroponics.
—I've carried enormous loads, but never seen a machine care so much about such a small one.
REX looked at the leaves.
—Ah... I get it. It's the most important thing you carry.
I gave it room.
—All right, gardener. I won't touch your cargo.
The unit chirped.
—I'll take that as thanks!

REX had its own priorities and left by another route. I headed back to the teleporter. Not every load needed me to carry it." },
        };
        private const string PrefixEs = @"Siempre me ha parecido extraño que algunas personas consideren aburrido mover mercancías. ¿Aburrido? ¡Por favor! Prueba a levantar un contenedor enorme, acomodarlo entre otras veinte cargas y dejarlo exactamente donde tiene que estar sin aplastar nada. Eso requiere habilidad. Y un buen equipo, claro. Mi exoesqueleto de titanio puede levantar hasta doscientas cincuenta toneladas. Le añadí unos ganchos personalizados con un cabrestante capaz de arrastrar otras cien. No porque alguien me lo pidiera, sino porque quería comprobar hasta dónde podía llevarlo. Trabajar en los muelles de la UES también tiene sus ventajas: conoces tripulaciones de todas partes, escuchas historias increíbles y siempre hay alguna pieza nueva que examinar. Estaba bastante contenta con mi trabajo. Hasta que apareció una oportunidad imposible de rechazarla.

Una misión clasificada hacia los Territorios Inexplorados. Eso fue lo que me ofrecieron. No voy a fingir que conocía todos los detalles, pero formar parte de una expedición semejante con mi propio exoesqueleto sonaba fantástico. Terminé a bordo de la UES Safe Travels, rumbo a Petrichor V. Nuestra misión estaba relacionada con otra nave desaparecida y la búsqueda de posibles supervivientes. Durante el viaje revisé los motores de mis guanteletes, ajusté los ganchos y comprobé las sujeciones del cabrestante tantas veces que probablemente podría hacerlo con los ojos cerrados. Había trabajado con maquinaria enorme toda mi vida, pero nunca había participado en una expedición así. Cuando llegó la oportunidad de descender, estaba más que preparada.

El planeta no me decepcionó. Había criaturas desconocidas, estructuras extrañas y lugares donde una operadora de carga podía encontrar trabajo para toda una vida. Uno de ellos me llamó especialmente la atención. Entre acantilados y enormes estructuras metálicas descansaban los restos de numerosas naves. Algunas estaban prácticamente partidas por la mitad. Otras parecían haberse estrellado contra formaciones rocosas que sobresalían del terreno. No sabía qué había ocurrido allí ni cómo llamaban a aquel lugar, pero me habría encantado disponer de un equipo completo de recuperación para examinarlo todo. Utilicé mis ganchos para alcanzar las plataformas más altas y aparté algunos fragmentos con los guanteletes. Estaba tratando de identificar el origen de una pieza cuando escuché un zumbido que hizo vibrar el metal bajo mis pies.

El sonido aumentó hasta convertirse en un estruendo. Desde las alturas apareció una máquina gigantesca, suspendida sobre las estructuras destruidas. No reconocí su diseño. Tampoco parecía interesada en que alguien la estudiara: sus sistemas de armamento comenzaron a disparar contra una figura que se movía entre los restos. Me preparé para intervenir y busqué un lugar desde el cual pudiera acercarme sin quedar expuesta a los ataques. Pero la batalla avanzó demasiado rápido. La figura desconocida evitó otra descarga, respondió con sus propias armas y finalmente consiguió derribar a aquella monstruosidad. Observé cómo la unidad perdía estabilidad y terminaba desplomándose entre los escombros. Por un momento olvidé incluso que tenía los ganchos preparados. ¡Jamás había visto algo semejante!

Esperé unos instantes, observando los restos de la enorme máquina para asegurarme de que no seguía funcionando. Después descendí hacia el lugar del combate, procurando no perder de vista las piezas que todavía desprendían chispas. Mi primera intención era examinar los restos, por supuesto. Pero antes quería conocer a quien había conseguido derrotarla. Ajusté mis guanteletes, guardé los ganchos y me acerqué a la figura que permanecía junto a la maquinaria destruida.";
        private const string PrefixEn = @"Some people call cargo work boring. Boring? Try lifting a massive container, settling it between twenty more, and not crushing a thing. That takes skill, and excellent equipment. My titanium exoskeleton can lift two hundred and fifty tons. I fitted custom grappling hooks and a winch capable of hauling another hundred, because I wanted to see what it could do. Working at UES docks meant meeting crews, hearing stories, and examining all sorts of machinery. I was perfectly happy until an offer came along that I could not refuse.

A classified expedition into the Uncharted Territories. I did not know every detail, but joining it with my own equipment sounded fantastic. I boarded the UES Safe Travels bound for Petrichor V, investigating the loss of another ship and looking for possible survivors. I spent the journey checking my gauntlets, hooks, and winch. When I finally had the chance to descend, I was more than ready.

The planet did not disappoint. There were unfamiliar creatures, mysterious structures, and places where a cargo operator could find a lifetime's work. One place in particular caught my eye: wrecked ships scattered across cliffs and twisted metal. I did not know what had happened there or what anyone called the place. I climbed the platforms with my hooks and shifted wreckage with my gauntlets. Then a humming noise began to shake the metal beneath my feet.

The noise grew into a roar. A gigantic machine rose over the wrecks and fired on someone moving between the structures. I prepared to intervene, searching for a safe approach, but the fight was already moving too fast. The stranger evaded another discharge, struck back, and brought the machine crashing down among the debris. I almost forgot that I was holding my grappling hooks. I had never seen anything like it!

I watched the wreck for a moment to make sure it was no longer active. Then I descended toward the battle, keeping an eye on the pieces still sparking. Naturally I wanted to examine the remains. First, however, I wanted to meet whoever had taken that monster down. I adjusted my gauntlets, stowed my hooks, and approached the figure standing beside the wreckage.";
        private const string EndingEs = @"Antes de marcharme, volví a mirar los restos de aquella máquina gigantesca. Todavía me costaba creer que algo semejante pudiera mantenerse suspendido sobre aquel cementerio de naves. Había encontrado motores, materiales y sistemas que no conocía, además de alguien capaz de enfrentarse a un enemigo extraordinario. No conseguí examinar tantas piezas como habría querido, pero guardé suficientes observaciones para intentar comprenderlas más adelante. Tal vez algún día pudiera regresar con herramientas apropiadas y descubrir cómo funcionaba aquella monstruosidad.

Comprobé los mecanismos de mis guanteletes y ajusté los ganchos del cabrestante. El siguiente destino seguía siendo un misterio, pero eso era precisamente lo que me había hecho aceptar la expedición. No había llegado hasta Petrichor V para quedarme mirando una montaña de chatarra. El teletransportador estaba listo. Sonreí y avancé hacia la luz.

Todavía quedaba muchísimo por descubrir.";
        private const string EndingEn = @"Before leaving, I looked back at the immense wreck. It was still hard to believe something that large could hover over a graveyard of ships. I had found strange engines, materials, and mechanisms, and met someone capable of defeating a remarkable enemy. I had not examined nearly as much as I wanted, but I would remember what I had seen. Perhaps one day I could return with proper tools.

I checked my gauntlets and adjusted my grappling winch. I did not know what waited in the next area, and that was exactly why I had joined the expedition. I had not come all the way to Petrichor V just to stare at a heap of scrap. The teleporter was ready. I smiled and stepped into the light.

There was still so much to discover.";

        // Call after original Alloy Worship Unit victory has been confirmed.
        // Not wired automatically: attribution must be resolved by the game event integration.
        internal static void RecordForProfile(string profileId, string victorBodyName)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return;
            string key = NormalizeBody(victorBodyName) ?? "Unknown";
            string path = PathForProfile(profileId);
            if (File.Exists(path)) return;
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            File.WriteAllText(path, key);
        }
        internal static string Append(string nativeNotes, bool spanish)
        {
            string id = LocalProfileIdForRecording();
            if (string.IsNullOrWhiteSpace(id)) return nativeNotes;
            string file = PathForProfile(id);
            if (!File.Exists(file)) return nativeNotes;
            string key;
            try { key = File.ReadAllText(file).Trim(); }
            catch { return nativeNotes; }
            string scene;
            if (!(spanish ? Es : En).TryGetValue(key, out scene))
                scene = spanish ? "No se pudo identificar al superviviente que derrotó a la máquina." : "The survivor who defeated the machine could not be identified.";
            string title = spanish ? "Diario II — Siempre queda algo por cargar (USU)" : "Log II — There's Always More to Carry (USU)";
            return (nativeNotes ?? "") + "\n\n<size=18><b>" + title + "</b></size>\n"
                 + (spanish ? PrefixEs : PrefixEn) + "\n\n" + scene + "\n\n" + (spanish ? EndingEs : EndingEn);
        }
        internal static string LocalProfileIdForRecording()
        {
            var local = LocalUserManager.GetFirstLocalUser();
            return local == null || local.userProfile == null ? null : local.userProfile.name;
        }
        private static string PathForProfile(string id)
        {
            string safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(id)).Replace('/', '_').Replace('+', '-').TrimEnd('=');
            return System.IO.Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "diaries", "loader-" + safe + ".txt");
        }
        private static string NormalizeBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            string key = body.Trim();
            if (key.EndsWith("Body", StringComparison.OrdinalIgnoreCase)) key = key.Substring(0, key.Length - 4);
            if (string.Equals(key, "Toolbot", StringComparison.OrdinalIgnoreCase)) key = "MUL-T";
            if (string.Equals(key, "Engi", StringComparison.OrdinalIgnoreCase)) key = "Engineer";
            if (string.Equals(key, "Mage", StringComparison.OrdinalIgnoreCase)) key = "Artificer";
            if (string.Equals(key, "Merc", StringComparison.OrdinalIgnoreCase)) key = "Mercenary";
            if (string.Equals(key, "Bandit2", StringComparison.OrdinalIgnoreCase)) key = "Bandit";
            if (string.Equals(key, "Croco", StringComparison.OrdinalIgnoreCase)) key = "Acrid";
            if (string.Equals(key, "Treebot", StringComparison.OrdinalIgnoreCase)) key = "REX";
            return Es.ContainsKey(key) ? key : null;
        }
    }
}
