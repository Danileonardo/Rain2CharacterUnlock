using System;
namespace UniversalSurvivorUnlocks
{
    internal static class CaptainUSUDiary
    {
        internal static string Append(string existing, bool spanish)
        {
            string title = spanish ? "Diario II — Un lugar al que regresar (USU)" : "Log II — A Place to Return To (USU)";
            return (existing ?? "") + "\n\n<size=18><b>" + title + "</b></size>\n" + (spanish ? Spanish : English);
        }
        private const string Spanish = @"Todavía recuerdo aquella conversación. La UES Contact Light había desaparecido y pretendían enviarnos a buscar supervivientes en una nave que ni siquiera contaba con el armamento adecuado. Pregunté por las armas, exigí que resolvieran el problema y miré mi viejo casco de comunicaciones. Había pasado demasiado tiempo retirado. Mis prótesis seguían recordándome los años, pero nada de eso cambiaba la responsabilidad que acababa de aceptar.

A bordo de la UES Safe Travels organizamos la búsqueda. Nuestro destino era Petrichor V. Enviamos soldados, especialistas, equipos técnicos y personal contratado con instrucciones de reconocer el terreno, localizar posibles supervivientes y mantener las comunicaciones. No era necesario que todos se conocieran personalmente; sí que comprendieran sus tareas. Cada informe recibido implicaba que alguien había logrado seguir adelante.

Los primeros despliegues fueron un desastre. Las lanzaderas alcanzaron una región montañosa y nuestros equipos intentaron establecer un punto de reunión, pero la fauna atacó. En menos de un día teníamos bajas, heridos y señales interrumpidas. Algunas patrullas siguieron informando; de otras no supimos más. No bastaba con enviar suministros desde la órbita. Aquella operación necesitaba una posición defendible en la superficie.

Después llegó un registro incompleto del sector lunar. Confirmaba la salida de una nave, pero no identificaba con certeza a quien iba a bordo ni permitía establecer si había sobrevivido. Tampoco explicaba cómo se produjo la evacuación. Entre otros archivos de la Contact Light aparecía una referencia aislada: Providence. Solo un nombre, sin contexto verificable ni prueba de relación con el incidente lunar. Conservé los datos sin inventar respuestas.

Solicité los recursos necesarios para descender y dejé a la Safe Travels operativa, capaz de sostener las comunicaciones y el apoyo desde arriba. En el planeta buscamos una posición que pudiera defenderse. No trasladamos toda la nave ni conocíamos todavía las regiones del mundo. Levantamos refugios provisionales, organizamos suministros, comunicaciones y atención para los heridos. Todavía faltaba mucho trabajo, pero al menos habíamos dejado de improvisar cada regreso.

Cuando el enlace quedó estable, envié un comunicado general. No sabía cuántos lo recibirían, ni quién podría volver. Los equipos que seguían explorando tendrían que transmitir sus hallazgos y solicitar las referencias de navegación por nuestros canales. Miré las primeras luces de la posición y comprendí que por fin habíamos dado un paso que no podía medirse en kilómetros explorados. Habíamos venido a buscar a quienes desaparecieron con la Contact Light. Ahora también debíamos proteger a quienes enviamos a encontrarlos. Teníamos un lugar al que regresar.";
        private const string English = @"I still remember that conversation. UES Contact Light had disappeared, and they intended to send us looking for survivors aboard a ship without suitable armaments. I asked about the weapons, insisted they fix that, and looked at my old communications helmet. I had been retired for too long. My prosthetics still reminded me of the years, but that changed nothing about the responsibility I had accepted.

Aboard UES Safe Travels, we organized the search. Our destination was Petrichor V. Soldiers, scouts, technicians, and contracted specialists received instructions to survey the ground, locate possible survivors, and maintain communications. They did not all need to know one another personally; they did need to know their assignments. Every report meant somebody had managed to keep going.

The initial deployments were a disaster. Shuttles reached a mountainous region and our teams attempted to establish a rally point, but the local wildlife attacked. Within a day we had casualties, wounded personnel, and broken signals. Some patrols kept reporting; others fell silent. Orbital supplies alone would not be enough. This operation needed a defensible position on the surface.

Then came an incomplete record from the lunar sector. It confirmed a ship had departed, but did not reliably identify its occupant or establish whether anyone survived. It offered no explanation of the evacuation. Another fragment from Contact Light records contained a single reference: Providence. One name, without verifiable context or proof of a connection to the lunar incident. I preserved the data without inventing answers.

I requested resources for my descent and left Safe Travels operational, able to maintain communications and support from above. On the planet, we selected a defensible position. We did not move the whole ship, and we still knew little of the world's regions. We set up provisional shelters, organized supplies and communications, and prepared space for the injured. There was much left to do, but at least we were no longer improvising every return.

Once the link stabilized, I issued a general broadcast. I did not know how many would hear it, or who might come back. Teams still exploring would report their findings and request navigation details through our channels. Watching the first lights at our position, I realized we had finally achieved something that could not be measured in distance traveled. We came to find those who disappeared aboard Contact Light. Now we also had to protect those sent to find them. We had a place to return to.";
    }
}
