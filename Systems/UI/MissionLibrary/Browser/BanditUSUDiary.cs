using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    // Registro complementario de USU. Nunca altera tokens ni entradas del Logbook original.
    // Registro por perfil local: llamar a RecordForProfile al completar realmente el desafío.
    internal static class BanditUSUDiary
    {
        private static readonly Dictionary<string, string[]> Es = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
                { "Commando", new [] { @"Escuché el sonido de dos seguros a mi espalda. Me giré y vi a un soldado con una pistola en cada mano. Aquella mirada bajo el casco no auguraba nada bueno.", @"—Identificación. Ahora.
—Vaya, socio. ¿Siempre apuntas a los pasajeros?
—No estás en la lista. Eres un polizón.
—Prefiero «pasajero independiente».", @"—¡Enemigos acercándose! ¡Cubre el flanco izquierdo!
—¡Qué rápido pasamos de las amenazas al trabajo en equipo!", @"—Regresas con nosotros. El mando decidirá qué hacer contigo.
—¿Y si me niego?
—Te quedas aquí solo.
—...Admiro la hospitalidad militar." } },
                { "Artificer", new [] { @"Estaba demasiado ocupado examinando un objeto que había encontrado como para advertir a quien se acercaba. Una voz femenina me hizo detenerme. Al darme vuelta vi a una mujer con un traje azul cuyos mecanismos parecían demasiado caros para dejarlos sin vigilancia.", @"—¿Puedo saber qué estás haciendo?
—¡Ah! Una compañera de expedición. Solo revisaba unos suministros.
—No recuerdo haberte visto antes.
—Bandit, señorita. Me asignaron a otra zona de exploración.
—Es posible. No conozco a todos los equipos desplegados. Aun así, confirmaré tu identificación.
[Su comunicador emitió una señal.]
—Aquí Artificer. ¿Tenemos a un Bandit asignado a los equipos de exploración?
—Negativo. No hay ningún Bandit registrado en la expedición.
—¿Algo que quieras aclarar?
—...¿Problemas con el sistema de personal?
[Una pequeña llama apareció sobre sus dedos.]
—Aleja las manos de esos suministros.
—¡Está bien! ¡Solo estaba mirando!", @"—¡Bandit! ¡Hay criaturas acercándose por el flanco derecho!
—¡Pensé que todavía estabas enfadada conmigo!
[Una explosión de fuego alcanzó a los enemigos que intentaban rodearnos.]
—¡Lo estoy! ¡Ahora dispara!
—¡Ja! ¡Sabía que terminaríamos llevándonos bien!
—¡Concéntrate!
—¡Sí, señora!", @"—Regresarás conmigo. Los responsables de la expedición decidirán qué hacer contigo.
—¿Y si prefiero marcharme por mi cuenta?
—Has sobrevivido hasta aquí, pero no creo que sea prudente continuar solo.
—¿Eso es una amenaza o una invitación?
[Una pequeña llama apareció sobre sus dedos.]
—Una recomendación.
—...Siempre he apreciado los buenos consejos.
—Entonces empieza por decir la verdad cuando lleguemos.
—¿Toda la verdad?
—Bandit.
—¡Está bien! ¡No hace falta mirarme así!" } },
                { "Huntress", new [] { @"Una flecha luminosa pasó junto a mi sombrero. Arriba, una cazadora mantenía su arco apuntándome. No tenía el menor temblor.", @"—Sal de tu escondite.
—¿Y perderme una entrada tan elegante?
—Llevas tiempo siguiendo nuestra ruta. No perteneces al equipo.
—Solo admiro su trabajo desde lejos.", @"—Tres a la derecha. No dejes que nos rodeen.
—¡Yo me encargo! Pero me debes un sombrero nuevo.", @"—Regresarás con el grupo.
—¿Eso es una invitación?
—Es una advertencia.
—Las mejores invitaciones lo son." } },
                { "MUL-T", new [] { @"Una máquina enorme me bloqueó la salida. En su carcasa decía MUL-T. Sus sensores recorrieron mi identificación y emitieron una alerta.", @"—Eh, grandullón. ¿Tenemos algún problema?
BIP-BIP. [Señala la identificación falsa.]
—Qué acusación tan fea. Apenas nos conocemos.
BIIIIIP. [Bloquea el camino.]
—Entiendo. No me dejas pasar.", @"BIP-BIP. [Marca a los enemigos y despliega el lanzaclavos.]
—¡Sí, los veo! ¡No hace falta que me los señales todos!", @"BIP. [Proyecta una ruta hacia el punto de reunión.]
—¿Quieres que te siga? ¿Qué eres, seguridad con ruedas?
BIP-BIP. [Indica la escasa munición que me queda.]
—...Está bien. Pero deja de revisar mis bolsillos." } },
                { "Engineer", new [] { @"Una torreta empezó a seguir mis movimientos. Su dueño apareció con un lanzagranadas y una segunda torreta cubría mi única salida.", @"—Alto. Zona restringida. ¿A qué unidad perteneces?
—Una división muy pequeña. Casi secreta.
—Tan secreta que tu identificación no existe.
—Exacto. Me alegra que nos entendamos.", @"—¡No cruces las minas! Quédate detrás de las torretas.
—¡Perfecto! Me encantan los planes donde otro hace el trabajo peligroso.", @"—Regresarás con nosotros. Allí comprobaremos tu identidad.
—¿Y si prefiero seguir explorando?
—Mis torretas pueden acompañarte hasta que cambies de opinión.
—...Qué considerado." } },
                { "Loader", new [] { @"Un gancho se clavó en la estructura junto a mí. Una figura cruzó el aire y aterrizó bloqueándome la ruta. Sus guanteletes eran más grandes que mi cabeza.", @"—¡Eh! ¿Qué haces aquí?
—Disfrutando del paisaje. ¿Algún problema?
—Esta ruta es para el personal. Muéstrame tu identificación.
—¿Y si mejor te invito a una bebida?", @"—¡Agáchate! ¡Voy a atravesar a ese grandullón!
—¡Por todos los créditos! ¡Avísame antes de salir volando!", @"—Vienes conmigo. No dejaré a un ladrón suelto entre el equipo.
—Prefiero «coleccionista».
—Colecciona tus cosas y camina.
—...Me agrada tu humor. Creo." } },
                { "Mercenary", new [] { @"No oí pasos. Solo el zumbido de una espada detenida junto a mi cuello. Su dueño parecía haber surgido de la nada.", @"—No te muevas.
—Si querías hablar, bastaba con un saludo.
—Te he visto seguirnos. No perteneces al equipo. Tu identificación.
—Está en otra chaqueta. Una muy lejana.", @"—Cuando avance, cubre mi espalda.
—¡Con gusto! Siempre que no confundas mi sombrero con un enemigo.", @"—Regresarás con la expedición.
—¿Y quién va a obligarme?
—Yo.
—...Qué convincente. ¿Has pensado en las ventas?" } },
                { "Captain", new [] { @"Reconocí la escopeta antes que al hombre que la empuñaba. Un veterano con uniforme de mando estudió mi identificación. Era el capitán de la nave a la que me había colado.", @"—No recuerdo haber autorizado tu presencia.
—¡Capitán! Justo a quien esperaba encontrar.
—Entraste en mi nave sin permiso.
—Técnicamente la puerta estaba abierta... después de que conseguí una tarjeta.", @"—¡Mantén la posición! ¡Solicitaré apoyo orbital!
—¡Ahora sí hablamos! Pero que caiga bien lejos de mí.", @"—Cuando volvamos, hablaremos de tu pasaje.
—¿No podemos olvidarlo después de trabajar tan bien juntos?
—No.
—...Tenía que intentarlo." } },
                { "Acrid", new [] { @"Escuché un gruñido entre la maleza. Una bestia cubierta de cicatrices, garras y saliva corrosiva apareció frente a mí. No parecía interesada en comprar nada.", @"—Tranquilo, grandullón. No quiero problemas.
[La criatura gruñe y escupe veneno contra la roca.]
—¡SANTO CIELO! ¡QUÉDATE CON TODO!", @"Salí disparado entre las rocas. Aquella criatura me siguió lo suficiente para convencerme de que cualquier fortuna podía esperar.", @"Cuando creí haberla perdido, alguien armado con equipo de la expedición me interceptó cerca del siguiente teletransportador. No conseguí averiguar quién era: una nueva horda nos obligó a combatir. Después me dejó claro, sin aceptar excusas, que debía regresar con el grupo." } },
                { "REX", new [] { @"Me encontré una máquina inmóvil con plantas creciendo sobre la carcasa. Sus componentes parecían valiosos y, por un instante, pensé que nadie los echaría de menos.", @"—Solo tomaré prestado un par de...
[Los sistemas de la máquina se activan y apuntan hacia mí.]
—¡Está bien! ¡No necesito tus malditas piezas!", @"Un proyectil golpeó el terreno junto a mis botas. Salí corriendo antes de descubrir qué otras sorpresas guardaba aquella maceta con patas.", @"Más adelante, cerca de un teletransportador, alguien con equipamiento de la expedición me descubrió. No llegué a identificarlo en medio del ataque de las criaturas. Nos tocó luchar juntos y, una vez sobrevivimos, me obligó a acompañar al grupo." } },
        };
        private static readonly Dictionary<string, string[]> En = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
                { "Commando", new [] { @"I heard two weapons click behind me. A soldier stood there with a pistol in each hand. His helmet gave nothing away.", @"—Identification. Now.
—Easy, partner. Do you greet all your passengers like that?
—You are not on the manifest. You are a stowaway.
—I prefer ""independent traveler.""", @"—Enemies closing in! Cover the left flank!
—That was a quick change from threats to teamwork!", @"—You are coming with us. Command will decide what to do with you.
—And if I refuse?
—You stay here alone.
—...Love that military hospitality." } },
                { "Artificer", new [] { @"I was too busy examining something I had found to notice anyone approaching. A woman's voice stopped me. I turned to find a woman in a blue suit whose equipment looked far too expensive to leave unattended.", @"—Mind telling me what you're doing?
—Ah! A fellow expedition member. Just checking supplies.
—I don't recall seeing you before.
—Bandit, ma'am. Assigned to another exploration team.
—Possible. I don't know everyone deployed. I'll verify your identification anyway.
[Her communicator chirped.]
—Artificer here. Do we have a Bandit assigned to our exploration teams?
—Negative. Nobody named Bandit is registered with the expedition.
—Anything you'd like to explain?
—...Trouble with the personnel database?
[A small flame appeared over her fingers.]
—Hands away from those supplies.
—All right! I was only looking!", @"—Bandit! Creatures approaching from the right flank!
—I thought you were still mad at me!
[A burst of fire engulfed the creatures closing in.]
—I am! Now shoot!
—Ha! I knew we'd end up getting along!
—Focus!
—Yes, ma'am!", @"—You're coming back with me. Expedition command will decide what to do with you.
—What if I prefer going my own way?
—You've survived this far, but continuing alone hardly seems wise.
—Is that a threat or an invitation?
[A little flame flickered around her fingers.]
—A recommendation.
—...I've always appreciated good advice.
—Then start telling the truth when we get back.
—The whole truth?
—Bandit.
—All right! No need to look at me like that!" } },
                { "Huntress", new [] { @"A glowing arrow struck beside my hat. Above me, a hunter kept her bow trained on my hiding place.", @"—Come out.
—And spoil such an elegant entrance?
—You have been following our route. You are not part of the team.
—I am simply an admirer.", @"—Three on the right. Do not let them surround us.
—I got it! You owe me a new hat, though.", @"—You are returning with the group.
—Is that an invitation?
—A warning.
—Best invitations usually are." } },
                { "MUL-T", new [] { @"A huge robot blocked my path. Its chassis read MUL-T. The sensors scanned my badge and flashed a warning.", @"—Hey, big guy. Do we have a problem?
BEEP-BEEP. [Points at the fake badge.]
—That is a nasty accusation. We barely know each other.
BEEEEEP. [Blocks the path.]
—Okay. No getting past you.", @"BEEP-BEEP. [Marks enemies and deploys the nailgun.]
—I see them! You do not have to point out every single one!", @"BEEP. [Displays a route back to the rendezvous.]
—You want me to follow you? What are you, security on wheels?
BEEP-BEEP. [Indicates my dwindling ammunition.]
—...Fine. But stop checking my pockets." } },
                { "Engineer", new [] { @"A turret started tracking me. Its owner stepped out carrying a grenade launcher. A second turret watched my only exit.", @"—Stop. Restricted area. What unit are you with?
—A very small one. Almost secret.
—So secret your identification does not exist.
—Exactly. Glad we understand each other.", @"—Stay clear of the mines! Behind the turrets!
—Perfect! I love plans where somebody else does the dangerous work.", @"—Back to camp. We will check your identity there.
—And if I would rather keep exploring?
—My turrets can escort you until you change your mind.
—...How thoughtful." } },
                { "Loader", new [] { @"A grapple struck the structure beside me. Somebody swung through the air and landed in my path. Her gauntlets were bigger than my head.", @"—Hey! What are you doing here?
—Enjoying the scenery. Problem?
—This route is for personnel. Show me your ID.
—How about I buy you a drink instead?", @"—Duck! I am going through that big one!
—For the love of credits! Warn me before you start flying!", @"—You are coming with me. I am not leaving a thief loose among our gear.
—I prefer ""collector.""
—Collect your things and move.
—...I think I like your humor." } },
                { "Mercenary", new [] { @"I heard no footsteps, only the hum of a sword stopping beside my throat. Its owner seemed to have appeared out of nowhere.", @"—Do not move.
—A simple hello would have worked.
—I saw you following us. You are not on the team. Your ID.
—It is in my other coat. Very far away.", @"—When I advance, cover my back.
—Happily! Just do not mistake my hat for an enemy.", @"—You are returning with the expedition.
—And who is making me?
—I am.
—...Persuasive. Ever thought of sales?" } },
                { "Captain", new [] { @"I recognized the shotgun before its owner. An old officer studied my badge. The captain of the very ship I had boarded without permission.", @"—I do not recall authorizing you.
—Captain! Just the man I hoped to meet.
—You came aboard my ship without permission.
—Technically the door was open... after I got a card.", @"—Hold this position! Calling in orbital support!
—Now we are talking! Just keep it well away from me.", @"—When we return, we will discuss your passage.
—Could we forget it after working so well together?
—No.
—...Had to try." } },
                { "Acrid", new [] { @"Something growled in the brush. A scarred beast emerged, all claws and corrosive spit. It did not seem interested in buying anything.", @"—Easy there, big guy. I do not want trouble.
[The beast snarls and spits poison into the rock.]
—GOOD HEAVENS! KEEP EVERYTHING!", @"I ran through the rocks. The beast chased me long enough to remind me that fortunes can wait.", @"Just when I thought I had lost it, an armed stranger wearing expedition gear caught me near the next teleporter. I never learned who it was: a fresh horde forced us to fight. Afterward, they made it very clear that I was coming back with the group." } },
                { "REX", new [] { @"I found a motionless machine with plants growing across its chassis. Its parts looked valuable, and for a moment I thought nobody would miss them.", @"—I am just borrowing a couple of...
[The machine activates its weapons and turns toward me.]
—All right! Keep your damn parts!", @"A shot hit the ground beside my boots. I ran before I found out what else that walking flowerpot could do.", @"Later, near a teleporter, somebody wearing expedition equipment discovered me. The creatures attacked before I could learn who it was. We fought together, and once we survived, they made me come back to the group." } },
        };
        private const string PrefixEs = @"Nunca he sido amigo de pagar por algo que pudiera conseguir gratis. Unas bebidas, una tarjeta de acceso que cambió de dueño y una tripulación demasiado ocupada: así acabé de polizón en la Safe Travels. La Contact Light llevaba tiempo desaparecida, y su cargamento prometía armas y equipo que podrían valer una fortuna. Yo no buscaba una misión heroica. Quería encontrar algo valioso y desaparecer sin que nadie hiciera preguntas. Y claro que me llevé el abrigo.

El planeta complicó las cosas. Criaturas extrañas, terreno hostil y teletransportadores que parecían atraer a todos los monstruos del lugar. Seguí adelante escondiéndome, hasta que el tercer traslado dejó de parecer una salida sencilla.";
        private const string PrefixEn = @"I have never liked paying for what I could get for free. A few drinks, an access card that changed owners, and a distracted crew: that was all it took to stow away aboard the Safe Travels. The Contact Light had gone missing, and its cargo promised valuable weapons and equipment. I was not there to be a hero. I wanted something worth stealing and a clean exit. And of course I brought my coat.

The planet complicated matters. Strange creatures, hostile terrain, and teleporters that seemed to attract every monster around. I kept out of sight until the third trip stopped looking like an easy way out.";
        private const string EndingEs = @"Al terminar, conservaba mis armas, mi abrigo y, por suerte, la vida. Pero también había perdido el anonimato. La gente de la expedición tenía ahora una razón para vigilarme y pocas ganas de escuchar mis excusas. Había venido por un cargamento perdido y había terminado trabajando con la misma tripulación a la que intenté engañar.

Supongo que incluso un ladrón sabe cuándo aceptar un trato que no le ofrecen dos veces.";
        private const string EndingEn = @"When it was over, I still had my weapons, my coat and, fortunately, my life. But I had lost my anonymity. The expedition now had every reason to keep an eye on me and no patience left for excuses. I came looking for missing cargo and ended up working with the very crew I had tried to fool.

I guess even a thief knows when to accept a deal that will not be offered twice.";

        // Usar solo cuando se confirma el desbloqueo original (tercer portal sin morir).
        // El primer encuentro se mantiene, incluso si el jugador vuelve a cumplir el reto.
        internal static void RecordForProfile(string profileId, string survivorBodyName)
        {
            if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(survivorBodyName)) return;
            string key = NormalizeBodyName(survivorBodyName);
            if (key == null) key = "Unknown"; // No adivinamos al descubridor.
            string path = PathForProfile(profileId);
            if (File.Exists(path)) return;
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            File.WriteAllText(path, key);
        }
        internal static string Append(string nativeNotes, bool spanish)
        {
            string profile = GetLocalProfileIdentifier();
            if (profile == null) return nativeNotes;
            string file = PathForProfile(profile);
            if (!File.Exists(file)) return nativeNotes;
            string variant;
            try { variant = File.ReadAllText(file).Trim(); }
            catch { return nativeNotes; }
            var map = spanish ? Es : En;
            string[] scene;
            if (!map.TryGetValue(variant, out scene)) scene = map["Acrid"]; // fallback safety below
            if (!map.ContainsKey(variant))
            {
                // Variante anónima sin atribuir encuentros con Acrid ni REX.
                scene = spanish ? new [] {
                    "Alguien de la expedición acabó descubriéndome cerca del tercer teletransportador. No logré identificarlo entre el caos.",
                    "—Eh, quizá podamos hablarlo después.\nNo hubo tiempo: los enemigos ya nos habían rodeado.",
                    "Nos tocó defendernos juntos hasta activar el traslado.",
                    "Después quedó claro que no me permitirían seguir solo. Me llevaron de vuelta con la expedición." }
                    : new [] {
                    "Somebody from the expedition caught me near the third teleporter. In the chaos, I never learned who.",
                    "—Hey, perhaps we can talk about this later.\nThere was no time: enemies had already surrounded us.",
                    "We fought together long enough to activate the teleporter.",
                    "Afterward it was clear they would not let me go alone. They took me back to the expedition." };
            }
            string title = spanish ? "Diario II — El precio de un pasaje gratis (USU)" : "Log II — The Price of a Free Ride (USU)";
            return (nativeNotes ?? "") + "\n\n<size=18><b>" + title + "</b></size>\n" +
                   (spanish ? PrefixEs : PrefixEn) + "\n\n" +
                   string.Join("\n\n", scene) + "\n\n" + (spanish ? EndingEs : EndingEn);
        }
        internal static string LocalProfileIdForRecording() { return GetLocalProfileIdentifier(); }
        private static string GetLocalProfileIdentifier()
        {
            var local = LocalUserManager.GetFirstLocalUser();
            if (local == null || local.userProfile == null) return null;
            // UserProfile.name can be changed; use platform profile file name where available in future.
            return local.userProfile.name;
        }
        private static string PathForProfile(string id)
        {
            // Cannot escape the config directory using arbitrary player input.
            string safe = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(id))
                .Replace('/', '_').Replace('+', '-').TrimEnd('=');
            return System.IO.Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "diaries", "bandit-" + safe + ".txt");
        }
        private static string NormalizeBodyName(string body)
        {
            string key = body.Trim();
            if (key.EndsWith("Body", StringComparison.OrdinalIgnoreCase)) key = key.Substring(0, key.Length - 4);
            if (string.Equals(key, "Mage", StringComparison.OrdinalIgnoreCase)) key = "Artificer";
            if (string.Equals(key, "Toolbot", StringComparison.OrdinalIgnoreCase)) key = "MUL-T";
            if (string.Equals(key, "Engi", StringComparison.OrdinalIgnoreCase)) key = "Engineer";
            if (string.Equals(key, "Merc", StringComparison.OrdinalIgnoreCase)) key = "Mercenary";
            if (string.Equals(key, "Treebot", StringComparison.OrdinalIgnoreCase)) key = "REX";
            if (string.Equals(key, "Croco", StringComparison.OrdinalIgnoreCase)) key = "Acrid";
            if (string.Equals(key, "Bandit2", StringComparison.OrdinalIgnoreCase)) key = "Bandit";
            return Es.ContainsKey(key) ? key : null;
        }
    }
}
