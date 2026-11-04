using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    // Only supplements the USU Notes panel; never alters vanilla Logbook tokens.
    internal static class AcridUSUDiary
    {
        private static readonly Dictionary<string, string> Es = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commando", @"El soldado alzó ambas pistolas cuando la criatura recién reconstruida avanzó sobre sus cuatro extremidades. El animal enseñó los dientes y se detuvo ante el cañón de las armas.
—Quieto. No te acerques.
Acrid respondió con un gruñido y arañó el suelo. Commando retrocedió sin disparar.
—No quiero hacerte daño. Mantén la distancia.
El organismo olfateó el aire y desplazó la mirada hacia una salida luminosa. El soldado reconoció el movimiento y se apartó, dejando libre el paso.
—Bien. Puedes irte.
Acrid avanzó sin dejar de vigilarlo. El humano no bajó las armas hasta que la criatura estuvo lejos." },
            { "Huntress", @"La cazadora retrocedió con el arco preparado. Observó las mandíbulas, las garras y el cuerpo encorvado del animal. Su experiencia le aconsejaba no acercarse a una criatura acorralada.
—Tranquilo. No voy a acercarme.
Acrid gruñó y siguió con la cabeza el movimiento del arco. Huntress dejó espacio entre ambos.
—Solo quieres salir de aquí, ¿verdad?
No hubo respuesta comprensible. El espécimen olfateó una corriente de aire que llegaba desde una abertura. Huntress bajó ligeramente el arco.
—Adelante.
La criatura pasó sin atacarla. Ninguno había iniciado una cacería." },
            { "Bandit", @"Un hombre con sombrero giró al escuchar el gruñido. Retrocedió tan rápido que casi perdió el equilibrio.
—¡Por todos los créditos! ¿De dónde saliste tú?
Acrid mostró los dientes y olfateó las pertenencias del desconocido.
—Tranquilo, grandullón. No soy parte del menú.
Una gota corrosiva cayó cerca de las botas del hombre, que aferró su escopeta.
—¡Y ni se te ocurra derretirme el abrigo!
La criatura inclinó la cabeza. Bandit retrocedió de nuevo.
—Hagamos un trato: tú te quedas con los bichos y yo con cualquier cosa de valor.
Un nuevo gruñido fue toda la respuesta.
—¡Perfecto! Me alegro de que nos entendamos.
El animal perdió interés y se alejó. Ningún trato había sido comprendido." },
            { "MUL-T", @"Una máquina industrial orientó sus sensores hacia la criatura e inició una evaluación de riesgo. Acrid olfateó el metal y se acercó con cautela.
BIP-BIP. [Advertencia: organismo desconocido.]
El animal acercó el hocico a un sensor. MUL-T retiró una herramienta antes de que entrara en contacto con la saliva corrosiva.
BIP. [Distancia de seguridad.]
Acrid respondió con un gruñido y retrocedió. Ambos permanecieron observándose. La unidad señaló con un brazo una abertura luminosa.
El animal siguió la dirección del movimiento y detectó una corriente de aire diferente. Se alejó sin tocar la máquina. MUL-T conservó sus registros químicos; Acrid conservó su libertad." },
            { "Engineer", @"El ingeniero descubrió al organismo cuando una de sus torretas comenzó a seguirlo. Acrid se encorvó ante el movimiento del arma.
—Eh... Esto no estaba en los planos.
La torreta giró un poco más. Un gruñido profundo sacudió el aire. Engineer activó los controles para evitar un disparo precipitado.
—No, no. Nada de disparar todavía.
La criatura mostró los dientes, pero no avanzó.
—Tú no me atacas y yo mantengo mis máquinas lejos de ti. ¿De acuerdo?
Acrid miró hacia la abertura luminosa. El ingeniero mantuvo quietas sus defensas y dejó libre el paso.
—Eso es. Cada uno por su lado.
La criatura se alejó sin comprender las palabras." },
            { "Artificer", @"Una exploradora con traje azul levantó una mano al ver las garras del animal. Una llama creció sobre sus dedos y Acrid retrocedió al sentir el calor.
—No te acerques.
La criatura gruñó; una sustancia verde se acumuló entre sus mandíbulas.
—No quiero hacerte daño, pero puedo defenderme.
Artificer observó sus movimientos. Aquella criatura parecía más preocupada por mantener distancia que por atacar. Redujo lentamente el fuego.
—Puedes marcharte. No voy a detenerte.
El espécimen olfateó la abertura cercana y avanzó con cautela. La exploradora no lo siguió." },
            { "Mercenary", @"Un espadachín levantó su arma al encontrarse frente al organismo. Acrid enseñó los dientes, pero el humano permaneció inmóvil.
El animal dio un paso. El filo se elevó apenas. Ninguno atacó.
—No tienes que pelear.
La criatura gruñó y avanzó de lado, tanteando el espacio libre. El hombre bajó lentamente la punta de su espada.
Acrid volvió la cabeza hacia una abertura luminosa y siguió su camino. Solo entonces el espadachín envainó el arma. Habían evitado un combate sin llegar a entenderse." },
            { "Loader", @"Una mujer de enormes guanteletes mecánicos se sorprendió al ver al animal recuperar el equilibrio.
—¡Vaya! ¡Tú sí que eres grande!
Acrid gruñó y se aproximó para olfatear el metal. Una gota corrosiva cayó sobre el suelo.
—¡Espera! ¡Eso no!
Loader retiró los guanteletes.
—¡He trabajado demasiado en estas maravillas para que terminen cubiertas de ácido!
La criatura mostró los dientes. Ella alzó los brazos, preparada para defenderse, pero no atacó.
—Tranquilo, grandullón. Yo no quiero pelear.
Acrid detectó una corriente de aire distinta y se alejó hacia la salida.
—¡Buen viaje, grandullón!
No hubo respuesta." },
            { "Captain", @"Un hombre veterano con escopeta observó cómo el animal se levantaba. Mantuvo el arma preparada y retrocedió cuando Acrid abrió las mandíbulas.
—Con calma...
El gruñido resonó entre las estructuras.
—Está bien. No voy a acercarme.
La criatura dio un paso hacia él. Captain se desplazó a un lado sin perder de vista las garras.
—Mantengamos la distancia.
Acrid percibió una corriente de aire desde una salida cercana. El hombre siguió su mirada y dejó despejado el camino.
—¿Quieres salir? No voy a impedírtelo.
La criatura pasó con cautela. Captain sostuvo su arma hasta quedar fuera de peligro." },
            { "REX", @"Una máquina cubierta de vegetación examinó al organismo recién reconstruido. Acrid captó el olor de una planta viva y avanzó lentamente. La unidad desplazó una herramienta entre las mandíbulas y el cultivo.
[REX — EVALUACIÓN]
ORGANISMO EXTERNO: DESCONOCIDO
RIESGO QUÍMICO: ELEVADO
DIRECTIVA: PRESERVAR EJEMPLAR 01
Acrid olfateó las hojas desde cierta distancia y dejó escapar un gruñido breve. REX mantuvo la barrera protectora. La criatura no atacó: volvió la cabeza hacia la abertura luminosa y se apartó. La planta permaneció a salvo. No existió reconocimiento confirmado entre ambos." }
        };
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commando", @"The soldier raised both pistols as the newly reconstituted creature advanced on its limbs. The animal bared its teeth and stopped before the weapons.
“Stay there. Do not come closer.”
Acrid growled and scratched the floor. Commando stepped back without firing.
“I do not want to hurt you. Keep your distance.”
The organism sniffed the air and looked toward a glowing exit. The soldier noticed and stepped aside.
“Fine. You can go.”
Acrid passed, still watching him. The human kept his weapons ready until it was far away." },
            { "Huntress", @"The huntress stepped back, bow ready. She studied its jaws, claws, and hunched posture. Experience told her not to approach a cornered animal.
“Easy. I will not come closer.”
Acrid growled and tracked the bow with its head. Huntress gave it room.
“You just want out, do you?”
No answer came. The specimen caught a current of air from an opening. Huntress lowered her bow slightly.
“Go on.”
The creature passed without attacking. Neither had started a hunt." },
            { "Bandit", @"A man in a hat spun around at the growl and almost stumbled backward.
“Sweet credits! Where did you come from?”
Acrid bared its teeth and sniffed his belongings.
“Easy, big fellow. I am not on the menu.”
A corrosive drop landed beside his boots. He grabbed his shotgun.
“And do not melt my coat!”
The creature tilted its head. Bandit backed up again.
“Let us make a deal: you take the monsters, I take anything valuable.”
Another growl was the only reply.
“Perfect! Glad we understand each other.”
The animal lost interest and moved away. No bargain had been understood." },
            { "MUL-T", @"An industrial machine turned its sensors toward the creature and began a risk assessment. Acrid sniffed the metal and approached cautiously.
BEEP-BEEP. [Warning: unknown organism.]
The animal brought its snout close to a sensor. MUL-T withdrew a tool before corrosive saliva could touch it.
BEEP. [Safe distance.]
Acrid growled and stepped back. Both watched one another. The unit pointed an arm toward a glowing opening.
The animal followed the gesture, sensed a different current of air, and moved away without touching the machine. MUL-T kept its chemical readings; Acrid kept its freedom." },
            { "Engineer", @"The engineer noticed the organism when one of his turrets began tracking it. Acrid hunched at the weapon’s movement.
“Uh... this was not in the plans.”
The turret turned farther. A deep growl echoed. Engineer used his controls to prevent an impulsive shot.
“No, no. No shooting yet.”
The creature bared its teeth but did not advance.
“You do not attack me, and I keep my machines away. Deal?”
Acrid looked toward a glowing opening. The engineer held his defenses still and cleared the way.
“That is it. Separate ways.”
The creature departed without understanding his words." },
            { "Artificer", @"An explorer in a blue suit raised a hand as she saw the animal’s claws. Flame gathered above her fingers, and Acrid backed away from the heat.
“Do not come closer.”
The creature growled; green fluid gathered between its jaws.
“I do not want to hurt you, but I can defend myself.”
Artificer watched it. The creature seemed more intent on keeping its distance than on attacking. She slowly reduced the flame.
“You may leave. I will not stop you.”
The specimen sniffed toward a nearby opening and advanced carefully. The explorer did not follow." },
            { "Mercenary", @"A swordsman raised his weapon on encountering the organism. Acrid bared its teeth, but the human stayed still.
The animal took one step. The blade rose slightly. Neither attacked.
“You do not have to fight.”
The creature growled and edged sideways, testing the free space. The man slowly lowered the blade.
Acrid turned toward a glowing opening and moved on. Only then did the swordsman sheath his weapon. They had avoided a fight without understanding each other." },
            { "Loader", @"A woman wearing enormous mechanical gauntlets watched the animal regain its balance.
“Wow! You are a big one!”
Acrid growled and approached to sniff the metal. A corrosive drop struck the floor.
“Wait! Not that!”
Loader drew her gauntlets back.
“I put too much work into these beauties to cover them in acid!”
The creature bared its teeth. She raised her arms, ready to defend herself, but did not strike.
“Easy, big fellow. I do not want a fight.”
Acrid sensed a different current of air and moved toward the exit.
“Safe travels, big fellow!”
There was no response." },
            { "Captain", @"An older man with a shotgun watched the animal rise. He kept the weapon ready and stepped back when Acrid opened its jaws.
“Easy now...”
The growl echoed through the structures.
“All right. I am not coming closer.”
The creature stepped toward him. Captain moved aside, keeping its claws in view.
“Let us keep some distance.”
Acrid caught an air current from a nearby exit. The man followed its gaze and cleared the path.
“Want out? I will not stop you.”
The creature passed warily. Captain kept the weapon ready until it was safely away." },
            { "REX", @"A machine covered in vegetation examined the newly restored organism. Acrid caught the scent of a living plant and moved closer. The unit placed a tool between the jaws and the crop.
[REX — ASSESSMENT]
EXTERNAL ORGANISM: UNKNOWN
CHEMICAL RISK: HIGH
DIRECTIVE: PRESERVE SPECIMEN 01
Acrid sniffed the leaves from a safe distance and gave a low growl. REX held its protective barrier. The creature did not attack: it turned toward a glowing opening and moved away. The plant remained safe. Neither showed confirmed recognition of the other." }
        };
        private const string PrefixEs = @"Antes de aquel encierro hubo otro. Un contenedor, paredes demasiado próximas y olores que ninguna criatura debería recordar. Acrid había conocido el metal de una jaula y, después, el suelo abierto bajo sus garras. La libertad fue breve. Otros seres lo capturaron: criaturas de extremidades duras y movimientos extraños que lo redujeron a una forma que ya no podía correr, morder ni olfatear. No es posible saber cuánto de aquello quedó en su conciencia. Solo subsisten indicios: la reacción de un organismo que, al recuperar sus sentidos, se encuentra rodeado otra vez por estructuras de prisión.

Durante un periodo imposible de medir, sus captores conservaron su cuerpo como una estructura inmóvil. Ninguna presa a la vista. Ninguna salida al alcance. Si sus recuerdos persistieron, no lo hicieron como las frases de un humano. Los animales conservan huellas del dolor, del peligro y de los lugares donde perdieron la posibilidad de escapar. Ese cautiverio había privado a Acrid precisamente de la capacidad de responder al mundo.

Entonces la contención cambió. Hubo alteraciones entre las celdas del Vacío y el organismo comenzó a recuperar su forma. Regresaron el peso, las extremidades y la sensación del suelo bajo las garras. Las mandíbulas se abrieron. Volvió el aire, extraño y lleno de estímulos. Acrid se incorporó con dificultad, mostró los dientes y gruñó. Cerca se encontraba otra figura. Después de una experiencia semejante, la postura defensiva del espécimen no era sorprendente: aún no podía distinguir entre una nueva amenaza y una presencia que simplemente compartía el lugar.";
        private const string PrefixEn = @"Before this prison there had been another: a container, walls too close, and scents no creature should have to remember. Acrid had known a metal cage and, for a time, open ground beneath its claws. Freedom did not last. Other beings captured it—creatures with hard limbs and unfamiliar movements—and reduced its body to something that could neither run nor bite nor smell. How much awareness survived is unknown. Only the signs remain: the response of an animal restored to its senses inside another prison.

For an immeasurable span, its captors kept the body immobile. No prey to pursue. No exit within reach. If memories endured, they were not human sentences. Animals retain traces of pain, danger, and places where escape became impossible. This captivity had taken from Acrid the very ability to respond to its surroundings.

Then containment changed. Disturbances crossed the Void cells, and the organism began to regain its form. Weight returned; then limbs and the feel of ground beneath its claws. Its jaws opened. Air returned, unfamiliar and full of stimuli. Acrid struggled upright, bared its teeth, and growled. Another figure stood nearby. After such captivity, the specimen’s defensive posture was unsurprising: it could not yet distinguish a new captor from someone who merely happened to be present.";
        private const string EndingEs = @"La figura quedó atrás. Acrid percibió una corriente de aire distinta, procedente de una abertura luminosa. Se acercó con cautela, olfateando y observando. Había conocido demasiadas barreras para atravesar una nueva sin desconfiar de ella. Pero nadie lo sujetaba y ninguna garra ajena intentaba devolverlo a su prisión.

El organismo avanzó hasta el portal y lo atravesó. Al otro lado encontró otro terreno, olores desconocidos y posibles fuentes de alimento. No conocía el nombre de aquel lugar. Su atención se dirigió primero al aire, luego al suelo y finalmente a la distancia abierta frente a él. Bajó la cabeza y comenzó a caminar. Por primera vez desde aquel último cautiverio, podía elegir su propia dirección. Y no parecía interesado en compañía.";
        private const string EndingEn = @"The figure remained behind. Acrid sensed a different current of air from a glowing opening. It approached cautiously, sniffing and watching. Too many barriers had taught it to mistrust unfamiliar passages. Yet nothing held it, and no alien claws attempted to return it to its prison.

The organism moved through the portal. Beyond lay another terrain, strange scents, and possible sources of food. It had no name for this place. Its attention went first to the air, then to the ground, and finally to the open distance ahead. It lowered its head and began to walk. For the first time since its last captivity, it could choose its own direction. It did not seem to want company.";

        // Call only after the game actually awards Acrid's original unlock, with
        // the local user profile and the completing survivor's body name.
        internal static void RecordForProfile(string profileId, string survivorBodyName)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return;
            string key = NormalizeBody(survivorBodyName);
            if (key == null) return; // No fabricated identity or accidental overwrite.
            string path = PathForProfile(profileId);
            if (File.Exists(path)) return;
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            File.WriteAllText(path, key, Encoding.UTF8);
        }

        internal static string Append(string nativeNotes, bool spanish)
        {
            string id = LocalProfileIdForRecording();
            if (string.IsNullOrWhiteSpace(id)) return nativeNotes;
            string path = PathForProfile(id);
            if (!File.Exists(path)) return nativeNotes;
            string key;
            try { key = File.ReadAllText(path).Trim().Trim('\uFEFF'); }
            catch { return nativeNotes; }
            string scene;
            if (!(spanish ? Es : En).TryGetValue(key, out scene)) return nativeNotes;
            string title = spanish ? "Diario II — El hambre de ser libre (USU)" : "Log II — The Hunger to Be Free (USU)";
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
            return System.IO.Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "diaries", "acrid-" + safe + ".txt");
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
            if (string.Equals(key, "Treebot", StringComparison.OrdinalIgnoreCase)) key = "REX";
            return Es.ContainsKey(key) ? key : null;
        }
    }
}
