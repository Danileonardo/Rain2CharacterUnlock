using System;
using System.IO;
using System.Collections.Generic;
using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    // Diario II USU: no modifica los tokens del Logbook original.
    internal static class ArtificerUSUDiary
    {
        private static readonly Dictionary<string, string> Es = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commando", @"Un soldado con casco y dos pistolas bajó las armas al verme caer. Se arrodilló a cierta distancia.
—¿Puedes oírme?
Intenté responder, pero apenas me salía la voz.
—Está bien. No intentes levantarte todavía.
Esperó hasta que conseguí sostenerme sobre una rodilla.
—...Sí.
—Bien. Tómate tu tiempo.
Poco después pude ponerme en pie y comprobar que mi traje respondía.
—¿Qué le hiciste a esa prisión?
—Activé un mecanismo con unas monedas. No sabía que había alguien dentro.
—No comprendo cómo funcionaba.
—Yo tampoco. Llegué por un portal. Puedo enseñarte la salida.
—¿Quién eres?
—Commando. Estamos investigando la desaparición de una nave y buscando supervivientes.
—Yo también pertenecía a una expedición.
—Entonces hablaremos cuando estemos fuera.
Lo acompañé hasta el portal. No conocía al soldado, pero había esperado a que pudiera caminar antes de pedir respuestas." },
            { "Huntress", @"Cuando conseguí levantar la mirada, distinguí una figura femenina sosteniendo un arco desconocido. Había retrocedido al desaparecer la barrera y me observaba desde unos pasos de distancia.
—¿Puedes moverte?
Intenté responder, pero mi garganta apenas produjo un sonido. Me apoyé sobre una mano y traté de incorporarme. El brazo cedió.
—No te esfuerces.
Mantuvo su posición y vigiló el establecimiento mientras yo recuperaba el aliento.
—...Sí.
—Bien.
No añadió nada más. Cuando conseguí sostenerme sobre ambas piernas, bajó ligeramente el arco.
—¿Estás herida?
—No... lo creo.
—Compruébalo antes de caminar.
Comprobé los indicadores de mi traje ENV. Algunas funciones tardaron en responder.
—¿Has sido tú quien me liberó?
—Sí.
—¿Cómo lo conseguiste?
—Había un mecanismo. Utilicé unas monedas.
Observé el lugar donde había estado el cristal.
—No sé cuánto tiempo permanecí dentro.
—¿Recuerdas cómo llegaste?
—No.
La mujer guardó silencio. No insistió.
—¿Quién eres?
—Huntress.
—Soy una Herald de la Casa del Más Allá.
No pareció reconocer el nombre.
—¿Por qué me liberaste?
La cazadora me observó unos instantes.
—Estoy buscando supervivientes.
—¿Supervivientes?
—Una nave desapareció en este sector. Nos enviaron a investigar.
Miré el cristal vacío.
—No pertenezco a esa nave.
—Ya veo.
—¿Eso cambia algo?
—No. Estabas atrapada.
Huntress señaló un portal luminoso a cierta distancia.
—Por ahí entré. Podemos salir cuando estés preparada.
—Entonces te acompañaré.
Asintió.
—Mantente cerca.
La seguí hacia el portal, recuperando poco a poco el ritmo de mis movimientos. No me había interrogado ni exigido confianza: me dejó decidir." },
            { "Bandit", @"Un hombre con sombrero y escopeta retrocedió de un salto al verme caer.
—¡Por todos los créditos! ¡Había alguien ahí dentro!
No podía sostenerme. Él bajó el arma.
—Eh... ¿señorita? ¿Está viva?
Moví lentamente una mano.
—Bien, buena señal. Ya creía que había tirado diez monedas a la basura. Quiero decir... ¡enhorabuena por su libertad!
—...¿Monedas?
—Y ni siquiera traían instrucciones.
Esperó, incómodo, mientras recuperaba el equilibrio. Cuando pude levantarme, seguía mirando el mecanismo vacío.
—¿Esperabas encontrar algo distinto?
—Por ese precio, un cofre lleno de tesoros.
—¿Me liberaste sin saber qué contenía?
—¡Lo importante es que funcionó! Soy Bandit.
—Artificer. Necesito salir de aquí.
—Eso sí puedo arreglarlo. Hay un portal por donde entré.
—¿Y después?
—Después debo regresar con cierta gente empeñada en vigilarme.
—¿Vigilarte?
—Una confusión sobre un pasaje, unas propiedades ajenas... nada importante.
—Empiezo a comprender.
—¡Me juzgas muy rápido, señorita!
Lo acompañé hacia el portal. No confiaba del todo en aquel hombre, y cada respuesta suya me daba nuevas razones para desconfiar. Pero conocía una salida y, por una vez, parecía decir la verdad." },
            { "MUL-T", @"Una máquina pesada emitió varios pitidos de alarma. Sus sensores me siguieron mientras caía y un brazo mecánico quedó inmóvil a mi alcance.
—...Gracias.
—BIP-BIP.
Me apoyé en la superficie que ofrecía. Permaneció estable hasta que pude ponerme de pie.
—¿Tú me has liberado?
—BIP. [Señala el mecanismo desactivado.]
—¿Puedes comprenderme?
—BIP-BIP. [Inclina sus sensores.]
—No reconozco tu tecnología. ¿Quién te fabricó?
La unidad emitió un sonido breve y se reorientó.
—Perdona. Supongo que puede esperar.
—BIP. [Señala un portal luminoso.]
—¿Por ahí llegaste?
—BIP-BIP.
—Entonces enséñame el camino.
La seguí con pasos cautelosos. No usaba palabras, pero sus movimientos parecían deliberados; sus sensores vigilaban que no perdiera el equilibrio." },
            { "Engineer", @"Un hombre equipado con herramientas se apartó del cristal al desaparecer. Dejó de examinar la estructura al verme caer.
—¡Oh! ¡Había una persona dentro! Espera, no te esfuerces. ¿Puedes mover los dedos?
Los moví lentamente.
—Bien. Eso es bueno. Recupera primero el equilibrio.
—...¿Qué ocurrió?
—Activé un mecanismo. No entiendo bien cómo.
Esperó hasta que conseguí incorporarme y comprobar mi traje.
—¿Eres investigador?
—Ingeniero. Aunque aquí las dos cosas parecen ir de la mano.
—¿Comprendes qué clase de prisión era esa?
—No. Y no pienso fingir que sí.
—Entonces compartimos la misma incertidumbre.
—Tu traje también es interesante. ¿Sigue funcionando?
—Sus sistemas comienzan a responder. Es tecnología de la Alta Corte.
—No conozco ese diseño. Tengo más instrumentos fuera de aquí, pero primero debemos salir.
—Entonces tendremos mucho que estudiar.
Abandonamos el mecanismo atrás y avanzamos hacia el portal, compartiendo preguntas que ninguno podía responder." },
            { "Loader", @"Una mujer con enormes guanteletes metálicos retrocedió al verme caer y se inclinó hacia mí, pero retiró las manos al comprobar su tamaño.
—¡Eh, cuidado! ¡Maldición! No esperaba que saliera alguien.
Intenté incorporarme sin éxito.
—Espera. Mejor no te sujeto con esto. Tómate tu tiempo.
—...No puedo...
—No me voy a ninguna parte.
Cuando recuperé las fuerzas, conseguí sostenerme sobre ambas piernas.
—Gracias por esperar.
—¡De nada! Vaya susto me diste.
—También ha sido inesperado para mí.
—¿Cuánto tiempo llevabas ahí?
—No lo sé. ¿Sabes cómo salir?
—Sí, entré por un portal. ¿Puedes caminar?
—Puedo intentarlo.
—Bien. Y si algo se derrumba, déjame ocuparme.
—Con esos guanteletes, no pienso discutirlo.
Caminé tras ella hasta la salida. Su despreocupación era sorprendente, pero había tenido cuidado de no hacerme daño." },
            { "Mercenary", @"Un espadachín envainó su arma cuando caí. Permaneció inmóvil unos instantes, después se arrodilló y extendió la mano sin intentar sujetarme.
—No te apresures.
Mis brazos temblaban demasiado para aceptarla. Esperó. Cuando recobré el equilibrio, tomé su mano.
—...Gracias.
No respondió.
—¿Por qué me liberaste?
—Estabas atrapada.
—No sabes quién soy.
—No.
—¿Y aun así decidiste intervenir?
—Sí.
Guardamos silencio mientras mi traje completaba sus comprobaciones.
—¿Conoces la salida?
—Por donde llegué.
—¿Puedo acompañarte?
—Si lo deseas.
—Entonces iré contigo.
Caminamos hacia el portal sin intercambiar demasiadas palabras. No necesitaba demostrar nada; simplemente había actuado y respetado mi decisión." },
            { "Captain", @"Un hombre de uniforme y expresión veterana apartó su escopeta y se arrodilló cerca de mí.
—Tranquila. Con calma. ¿Puedes escucharme?
—...Sí.
—Bien. No intentes levantarte todavía.
Esperó mientras recuperaba el aliento y conseguía mover las manos.
—...¿Quién...?
—Puedes llamarme Captain. Lo demás puede esperar.
Cuando el traje respondió y pude caminar, hablamos otra vez.
—¿Ha sido usted quien me liberó?
—Sí. No esperaba encontrar a alguien dentro. ¿Recuerdas cómo llegaste?
—No. Hay un vacío en mis recuerdos.
—Entonces no tienes que resolverlo ahora.
—¿Quién es usted realmente?
—Capitán de una nave de la UES. Estamos buscando supervivientes.
—Yo pertenecía a otra expedición.
—Entonces quizá tengamos información que compartir. Primero salgamos de aquí.
—De acuerdo, capitán.
Lo acompañé hasta el portal. No me exigió explicaciones; prefirió asegurarse de que podía mantenerme en pie." },
            { "Acrid", @"Al levantar la mirada encontré una criatura enorme de garras afiladas y mandíbulas poderosas. Olfateó el suelo donde antes había estado la prisión.
—...No te acerques.
No pude levantarme. La bestia gruñó y giró hacia mí. Los sistemas defensivos de mi traje apenas comenzaban a responder.
—Mantén la distancia.
La criatura me observó, mostró los dientes y volvió su atención hacia los objetos del establecimiento. No sabía si me había liberado deliberadamente o por accidente.
Cuando pude ponerme en pie, permanecí lejos de sus garras.
—No sé si pretendías ayudarme... pero gracias.
Solo respondió otro gruñido. Encontré el portal sin intentar acercarme de nuevo. La criatura y yo no compartíamos idioma ni, aparentemente, intenciones." },
            { "REX", @"Al levantar la mirada distinguí una máquina cubierta de vegetación. Sus sensores seguían examinando el mecanismo desactivado mientras yo luchaba por respirar.
—...¿Una máquina?
La unidad giró hacia mí. Sus componentes vegetales se movieron, pero no comprendí la señal.
—...Espera.
Intenté incorporarme; mis manos temblaban. La máquina permaneció a cierta distancia y después continuó examinando el entorno.
Cuando logré ponerme en pie, observé la integración entre sus piezas artificiales y sus organismos vivos.
—¿Has sido tú quien me liberó?
Orientó sus sensores hacia mí.
—No reconozco tu diseño. ¿Puedes comprender mis palabras?
La unidad emitió sonidos que no pude interpretar.
—No sé si fue tu intención. Pero gracias.
No intentó detenerme. Encontré el portal y avancé hacia él sin saber si aquel encuentro había sido un acto deliberado o un accidente." },
        };
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commando", @"A soldier wearing a helmet and carrying two pistols lowered his weapons when I fell. He knelt a short distance away.
—Can you hear me?
I tried to answer, but could barely speak.
—It's all right. Don't try to stand yet.
He waited until I could support myself on one knee.
—...Yes.
—Good. Take your time.
Once I could stand and check my suit, I asked: 
—What did you do to that prison?
—I activated a mechanism with some coins. Didn't know anyone was inside.
—I don't understand how it worked.
—Neither do I. I came through a portal. I can show you the exit.
—Who are you?
—Commando. We're investigating a missing ship and looking for survivors.
—I belonged to an expedition too.
—Then we can talk once we're outside.
I followed him to the portal. I didn't know the soldier, but he had waited for me to walk before asking questions." },
            { "Huntress", @"When I lifted my head, I saw a woman holding an unfamiliar bow. She had stepped back when the barrier vanished and watched me from several paces away.
—Can you move?
I tried to answer, but barely made a sound. My arm gave way as I tried to rise.
—Don't strain yourself.
She held her position and watched the shop while I caught my breath.
—...Yes.
—Good.
She said nothing more. When I managed to stand, she lowered her bow a little.
—Are you hurt?
—I... don't think so.
—Check before you walk.
I inspected my ENV suit's readings. Some systems were slow to respond.
—Were you the one who freed me?
—Yes.
—How?
—There was a mechanism. I used some coins.
I looked at where the crystal had stood.
—I don't know how long I was inside.
—Do you remember how you got here?
—No.
The woman said nothing and did not press me.
—Who are you?
—Huntress.
—I'm a Herald of the House Beyond.
The name meant nothing to her.
—Why did you free me?
She watched me for a moment.
—I'm looking for survivors.
—Survivors?
—A ship disappeared in this sector. We were sent to investigate.
—I wasn't aboard that ship.
—I see.
—Does that change anything?
—No. You were trapped.
Huntress indicated a glowing portal.
—I came through there. We can leave when you're ready.
—Then I'll come with you.
She nodded.
—Stay close.
I followed her to the portal, slowly finding my stride again. She had not interrogated me or demanded my trust. She let me choose." },
            { "Bandit", @"A man in a hat, carrying a shotgun, jumped back as I fell.
—By all the credits! Someone was inside!
I couldn't support myself. He lowered the weapon.
—Uh... miss? You alive?
I slowly moved one hand.
—Good. I thought I'd wasted ten coins. I mean... congratulations on your freedom!
—...Coins?
—And they didn't even come with instructions.
He waited awkwardly until I could stand. Then I noticed him staring at the empty mechanism.
—Were you expecting something else?
—For that price? A chest of treasure.
—You freed me without knowing what was inside?
—What matters is that it worked! I'm Bandit.
—Artificer. I need to leave.
—That I can arrange. There's a portal I came through.
—And after that?
—I have to get back to some folks who insist on keeping an eye on me.
—Keeping an eye on you?
—A misunderstanding about a ticket, some other people's property... nothing serious.
—I'm beginning to understand.
—You judge too quickly, lady!
I followed him to the portal. I didn't entirely trust him, and each answer gave me fresh reasons not to. But he knew a way out and, for once, seemed to be telling the truth." },
            { "MUL-T", @"A heavy machine sounded several warning beeps. Its sensors followed my fall, and a mechanical arm held steady within my reach.
—...Thank you.
—BEEP-BEEP.
I leaned on the offered surface. It stayed stable until I could stand.
—Did you free me?
—BEEP. [Points at the disabled mechanism.]
—Can you understand me?
—BEEP-BEEP. [Tilts its sensors.]
—I don't recognize your technology. Who made you?
The unit emitted a short sound and turned.
—Sorry. That can wait.
—BEEP. [Points toward a glowing portal.]
—Did you arrive through there?
—BEEP-BEEP.
—Then show me the way.
I followed cautiously. It used no words, but its movements were deliberate, and its sensors watched to make sure I kept my balance." },
            { "Engineer", @"A man carrying engineering tools stepped back as the crystal disappeared. He stopped examining the structure when I fell.
—Oh! There was someone inside! Easy. Can you move your fingers?
I moved them slowly.
—Good. Regain your balance first.
—...What happened?
—I activated a mechanism. I don't really understand how.
He waited until I could stand and inspect my suit.
—Are you a researcher?
—An engineer. Though here those seem to go together.
—Do you know what sort of prison that was?
—No. And I won't pretend I do.
—Then we're equally uncertain.
—Your suit is interesting too. Is it working?
—Its systems are coming back. It's High Court technology.
—I don't know that design. I have more instruments elsewhere, but first we should leave.
—Then we'll have much to study.
We left the mechanism behind and walked toward the portal, sharing questions neither of us could answer." },
            { "Loader", @"A woman wearing enormous metal gauntlets recoiled as I fell and bent toward me, then withdrew her hands when she noticed their size.
—Hey, careful! Damn! Didn't expect someone to come out.
I tried and failed to rise.
—Wait. Better not grab you with these. Take your time.
—I... can't...
—I'm not going anywhere.
After a while I regained enough strength to stand.
—Thanks for waiting.
—No problem! You scared me.
—It was unexpected for me too.
—How long were you in there?
—I don't know. Do you know a way out?
—Yeah, through a portal. Can you walk?
—I can try.
—Good. And if anything falls down, leave it to me.
—With those gauntlets, I won't argue.
I followed her toward the exit. Her easygoing manner was surprising, but she'd taken care not to hurt me." },
            { "Mercenary", @"A swordsman sheathed his weapon when I fell. After a moment he knelt and offered his hand without grabbing me.
—Don't rush.
My arms shook too badly to take it. He waited. When I regained my balance, I accepted his hand.
—...Thank you.
He did not reply.
—Why did you free me?
—You were trapped.
—You don't know who I am.
—No.
—And you intervened anyway?
—Yes.
We remained quiet while my suit checked its systems.
—Do you know the exit?
—The way I came in.
—May I accompany you?
—If you wish.
—Then I'll come.
We walked toward the portal without many words. He had no need to prove anything; he had acted and respected my choice." },
            { "Captain", @"A veteran-looking man in uniform set aside his shotgun and knelt nearby.
—Easy now. Can you hear me?
—...Yes.
—Good. Don't try to stand yet.
He waited until I could catch my breath and move my hands.
—...Who...?
—Call me Captain. The rest can wait.
When my suit responded and I could walk, we spoke again.
—Were you the one who freed me?
—Yes. Didn't expect to find someone inside. Remember how you got here?
—No. There's a gap in my memory.
—Then you don't have to solve it now.
—Who are you really?
—Captain of a UES ship. We're looking for survivors.
—I belonged to another expedition.
—Then perhaps we have information to share. First let's get out of here.
—Agreed, Captain.
I followed him to the portal. He hadn't demanded explanations; he'd made sure I could stand." },
            { "Acrid", @"When I lifted my head I saw an enormous creature with sharp claws and powerful jaws. It sniffed the ground where the prison had stood.
—...Stay back.
I couldn't stand. The beast growled and turned toward me. My suit's defensive systems were only just responding.
—Keep your distance.
It watched me, bared its teeth, then returned its attention to the shop's objects. I couldn't tell whether it had freed me deliberately or by accident.
When I could stand, I kept clear of its claws.
—I don't know whether you meant to help... but thank you.
Another growl was my only answer. I found the portal without approaching it again. We shared neither language nor, apparently, intent." },
            { "REX", @"When I lifted my head I saw a machine covered in vegetation. Its sensors continued examining the inactive mechanism while I struggled to breathe.
—...A machine?
The unit turned toward me. Its plants shifted, but I couldn't understand any signal.
—...Wait.
I tried to rise; my hands trembled. It kept its distance, then resumed surveying the surroundings.
Once I could stand, I examined the combination of artificial components and living organisms.
—Did you free me?
Its sensors turned toward me.
—I don't recognize your design. Can you understand me?
It emitted sounds I couldn't interpret.
—I don't know whether you meant to. But thank you.
It made no effort to stop me. I found the portal and approached it, unsure whether our meeting had been deliberate or accidental." },
        };
        private const string PrefixEs = @"Nos enseñaron a mirar hacia las estrellas sin confundir la distancia con lo imposible. Para la Alta Corte, la Tierra Prometida no era únicamente una esperanza: era un lugar que podía encontrarse. La ciencia nos proporcionaba herramientas; la convicción, la voluntad de seguir buscando. La Casa del Más Allá exploraba regiones que otros apenas se atrevían a considerar. Nuestro traje ENV podía adaptarse a condiciones extremas. Recuerdo nuestro propósito y las posibilidades más allá de los mapas conocidos. Pero no recuerdo el instante exacto en que perdí mi libertad. Entre aquellos recuerdos y mi prisión existe una interrupción que no puedo explicar.

No sé cuánto tiempo permanecí encerrada. Al principio intenté comprender el fenómeno. No sentía dolor y no podía mover un solo músculo, pero mis pensamientos continuaban. Observaba un extraño establecimiento a través de una barrera. Había objetos desconocidos, luces que no sabía interpretar y una criatura singular que parecía formar parte del lugar. Algunas veces distinguía figuras que se aproximaban; otras, el establecimiento permanecía vacío durante intervalos imposibles de medir. Intenté contar esos momentos, pero carecía de una referencia fiable. ¿Habían transcurrido horas? ¿Años? No sabía si el tiempo estaba detenido para mí o si simplemente había dejado de experimentarlo como antes. Pensar era lo único que todavía podía hacer.

Volví una y otra vez a mis recuerdos: las enseñanzas de la Alta Corte, la Casa del Más Allá, los principios de nuestra exploración. Repasaba cuanto sabía sobre energía, materia y fenómenos temporales buscando alguna explicación. No podía utilizar instrumentos, comprobar el estado de mi traje ni siquiera cerrar los ojos cuando quisiera. La incertidumbre terminó siendo más difícil que la inmovilidad. Había dedicado mi existencia a descubrir lo inalcanzable y ahora ni siquiera podía extender una mano. Llegué a temer que mis pensamientos fueran lo último que conservaría de mí misma. Si todavía podía recordar quién era, aquella prisión no me lo había arrebatado todo.

En algún momento una figura se detuvo frente al cristal. La vi examinar el mecanismo cercano. Un resplandor atravesó la barrera. Intenté moverme por reflejo y esta vez mi cuerpo respondió. La prisión estaba cediendo.

La barrera desapareció. Caí.

Mis rodillas y mis manos golpearon el suelo. Sentí la piedra fría, el peso de mi cuerpo y el aire llenando mis pulmones. Intenté levantarme, pero mis extremidades temblaban. Permanecí sobre las manos, respiré y cerré los ojos por voluntad propia. El movimiento más sencillo resultaba extraordinario. Solo entonces levanté la mirada hacia quien estaba frente a mí.";
        private const string PrefixEn = @"We were taught to look toward the stars without mistaking distance for impossibility. To the High Court, the Promised Land was not merely a hope: it was a place to be found. Science gave us the means; conviction gave us the will to seek. The House Beyond explored regions others barely dared consider. Our ENV suits could adapt to hostile conditions. I remember our purpose and the possibilities beyond known maps. But I cannot remember the exact moment I lost my freedom. There is a gap between those memories and my prison that I cannot explain.

I do not know how long I was trapped. At first I tried to understand the phenomenon. I felt no pain and could not move a muscle, yet my thoughts continued. Beyond the barrier was a strange establishment: unfamiliar objects, incomprehensible lights, and an unusual creature that seemed to belong there. Sometimes figures approached; at other times the place was empty for immeasurable intervals. I tried counting, but had no reliable reference. Hours? Years? I could not tell whether time had stopped for me or whether I simply no longer experienced it as before. Thinking was all I could do.

I returned to my memories again and again: the High Court's teachings, the House Beyond, the principles of our exploration. I reviewed all I knew of energy, matter and temporal phenomena, looking for an explanation. I could not use instruments, check my suit or even close my eyes at will. Uncertainty became harder to bear than stillness. I had devoted myself to reaching the unreachable, yet could not stretch out my hand. I feared that my thoughts might be all I had left. If I could still remember who I was, the prison had not taken everything.

At some point a figure stopped before the crystal and examined a nearby mechanism. Light spread across the barrier. I tried to move instinctively—and this time my body obeyed. The prison was giving way.

The barrier vanished. I fell.

My knees and hands struck the floor. I felt cold stone, the weight of my body and air filling my lungs. I tried to stand, but my limbs trembled. I stayed on my hands, breathed, and closed my eyes of my own will. The simplest movement felt extraordinary. Only then did I raise my head toward the figure standing before me.";
        private const string EndingEs = @"Atravesé el portal. Durante un instante perdí toda referencia del espacio, hasta que el suelo volvió a aparecer bajo mis pies. Ante mí se extendía un paisaje desconocido: vegetación extraña, formaciones rocosas y estructuras que no podía identificar. Mi traje ENV volvió a registrar el ambiente. No sabía dónde estaba ni tenía motivos para creer que aquel mundo fuera la Tierra Prometida. Había recuperado mi libertad, pero no las respuestas. Observé el horizonte. Esta vez, al menos, podía explorarlo.";
        private const string EndingEn = @"I stepped through the portal. For a moment I lost every sense of space, until solid ground returned beneath my feet. An unfamiliar landscape stretched before me: strange vegetation, rock formations and structures I could not identify. My ENV suit began recording the environment again. I did not know where I was, nor did I have reason to believe this world was the Promised Land. I had regained my freedom, but not my answers. I looked toward the horizon. This time, at least, I could explore it.";

        // IMPORTANT: invoke ONLY when actual Artificer crystal purchase is confirmed.
        // Set profileId for the unlocked player's UserProfile, not the host's by default.
        // Save the actor's BodyName (CommandoBody, HuntressBody, Bandit2Body ...).
        internal static void RecordForProfile(string profileId, string rescuerBodyName)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return;
            string variant = NormalizeBody(rescuerBodyName);
            if (variant == null) variant = "Unknown";
            string path = PathForProfile(profileId);
            if (File.Exists(path)) return; // Preserve first actual rescuer.
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            File.WriteAllText(path, variant);
        }

        internal static string Append(string nativeNotes, bool spanish)
        {
            string id = LocalProfileIdForRecording();
            if (string.IsNullOrWhiteSpace(id)) return nativeNotes;
            string path = PathForProfile(id);
            if (!File.Exists(path)) return nativeNotes; // No unsupported guess.
            string variant;
            try { variant = File.ReadAllText(path).Trim(); }
            catch { return nativeNotes; }
            var scenes = spanish ? Es : En;
            string encounter;
            if (!scenes.TryGetValue(variant, out encounter))
                encounter = spanish
                    ? "No conseguí distinguir quién activó el mecanismo. Esperé a recuperar mis fuerzas hasta poder localizar el portal de salida."
                    : "I could not make out who had activated the mechanism. I waited until I regained my strength and could find the exit portal.";
            string title = spanish ? "Diario II — Donde el tiempo no transcurre (USU)" : "Log II — Where Time Stands Still (USU)";
            return (nativeNotes ?? "") + "\n\n<size=18><b>" + title + "</b></size>\n" +
                   (spanish ? PrefixEs : PrefixEn) + "\n\n" + encounter + "\n\n" + (spanish ? EndingEs : EndingEn);
        }

        internal static string LocalProfileIdForRecording()
        {
            var local = LocalUserManager.GetFirstLocalUser();
            return local == null || local.userProfile == null ? null : local.userProfile.name;
        }
        private static string PathForProfile(string id)
        {
            string safe = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(id))
                .Replace('/', '_').Replace('+', '-').TrimEnd('=');
            return System.IO.Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "diaries", "artificer-" + safe + ".txt");
        }
        private static string NormalizeBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            string key = body.Trim();
            if (key.EndsWith("Body", StringComparison.OrdinalIgnoreCase)) key = key.Substring(0, key.Length - 4);
            if (string.Equals(key, "Toolbot", StringComparison.OrdinalIgnoreCase)) key = "MUL-T";
            if (string.Equals(key, "Engi", StringComparison.OrdinalIgnoreCase)) key = "Engineer";
            if (string.Equals(key, "Bandit2", StringComparison.OrdinalIgnoreCase)) key = "Bandit";
            if (string.Equals(key, "Treebot", StringComparison.OrdinalIgnoreCase)) key = "REX";
            if (string.Equals(key, "Croco", StringComparison.OrdinalIgnoreCase)) key = "Acrid";
            return Es.ContainsKey(key) ? key : null;
        }
    }
}
