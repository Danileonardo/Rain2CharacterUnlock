using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    // Per-profile unlock. Invoke RecordCaptainVictory only from a CONFIRMED
    // Captain moon-escape OR obelisk-obliteration completion handler.
    internal static class VanillaBroadcastUSUDiary
    {
        private static readonly Dictionary<string, string> Es = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { @"CommandoBody", @"El comunicador militar interrumpió la vigilancia. Reconoció la frecuencia de la Safe Travels y escuchó la orden completa. —Recibido. Mantendré el enlace abierto. Guardó la referencia de navegación. Todavía quedaban supervivientes por encontrar." },
            { @"HuntressBody", @"Una señal de la expedición la alcanzó mientras seguía un rastro. Escuchó inmóvil, con el arco preparado. —Recibido. Memorizó la referencia y reanudó la búsqueda. Saber que había una ruta de regreso no significaba abandonar la cacería." },
            { @"Bandit2Body", @"El comunicador asignado emitió una voz familiar. —¿Suministros y un lugar donde dormir? Empiezo a pensar que el pasaje gratis no estuvo tan mal. Escuchó las instrucciones de registro y vigilancia. —...Retiro lo dicho. Se acomodó el sombrero y guardó la referencia." },
            { @"ToolbotBody", @"[TRANSMISIÓN UES: RECIBIDA]
PUNTO DE OPERACIONES: DISPONIBLE
REFERENCIA DE RETORNO: ARCHIVADA
TAREAS ACTUALES: EN CURSO
La unidad emitió una señal de confirmación y continuó su labor." },
            { @"EngiBody", @"El terminal de campo recibió la transmisión. —Por fin, un lugar donde organizar herramientas y reparar equipos. Guardó las instrucciones y empezó a calcular cuántas torretas serían necesarias. Todavía no conocía las condiciones de aquella posición." },
            { @"MageBody", @"El comunicador recibido de la expedición reprodujo el anuncio. Artificer reconoció al capitán que la había aceptado entre los exploradores. No era la Tierra Prometida ni una respuesta a su prisión, pero quizá allí encontraría información útil para continuar su búsqueda." },
            { @"MercBody", @"Un aviso llegó por el canal del contrato. Mercenary escuchó, registró los datos y aseguró la vaina de su espada. No necesitaba responder todavía. Su trabajo no había terminado." },
            { @"TreebotBody", @"[RECEPTOR: SEÑAL HUMANA INTERMITENTE]
DATOS DE REUNIÓN: ARCHIVADOS
DIRECTIVA PRIORITARIA: PRESERVAR EJEMPLAR 01
La máquina comparó el aviso con sus necesidades de agua y luz. No cambió de rumbo: la planta seguía requiriendo un entorno sostenible." },
            { @"LoaderBody", @"El aviso llegó mientras inspeccionaba un componente enorme. —¡Un lugar con suministros! Espero que hayan reservado espacio para equipo pesado. Guardó la referencia y volvió a sus guanteletes. Primero debía terminar lo que estaba haciendo." },
            { @"CrocoBody", @"Una voz salió de un aparato abandonado entre los restos. Acrid levantó la cabeza, olfateó el objeto y escuchó una sucesión de sonidos sin significado. Lo empujó con una garra; la voz continuó. Después se alejó. No sabía de órdenes ni de campamentos, pero la señal también había llegado hasta él." },
            { @"CaptainBody", @"Tras la transmisión, Captain comprobó que los equipos de comunicación continuaban activos. No sabía quién la recibiría, pero el mensaje había salido. Ahora tocaba mantener la posición y responder a quienes consiguieran regresar." }
        };
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { @"CommandoBody", @"A familiar frequency interrupted his watch. He heard the full message. “Received. I will keep the channel open.” He saved the navigation reference and returned to his mission." },
            { @"HuntressBody", @"The expedition signal reached her while tracking a trail. Bow ready, she listened. “Received.” She saved the reference and continued the hunt. A way back did not mean her search was over." },
            { @"Bandit2Body", @"The assigned communicator crackled. “Supplies and somewhere to sleep? Maybe that free ride was worth it.” Then came the instructions about check-ins and supervision. “...Never mind.” He adjusted his hat and saved the reference." },
            { @"ToolbotBody", @"[UES TRANSMISSION: RECEIVED]
OPERATIONS POINT: AVAILABLE
RETURN REFERENCE: ARCHIVED
CURRENT TASKS: IN PROGRESS
The unit signaled acknowledgment and resumed its work." },
            { @"EngiBody", @"His field terminal received the broadcast. “Finally, somewhere to organize tools and repairs.” He stored the instructions and began estimating the defenses they would need. He had yet to see the site." },
            { @"MageBody", @"The communicator supplied by the expedition played the message. Artificer recognized the captain who had accepted her among the explorers. It was not the Promised Land, nor an answer to her imprisonment, but it might offer the information she needed." },
            { @"MercBody", @"The contract channel carried the announcement. Mercenary listened, saved the data, and checked his sword. There was no need to respond yet. His work was not finished." },
            { @"TreebotBody", @"[RECEIVER: INTERMITTENT HUMAN SIGNAL]
RENDEZVOUS DATA: ARCHIVED
PRIMARY DIRECTIVE: PRESERVE SPECIMEN 01
The machine compared the signal with the plant’s need for water and light, then continued its search." },
            { @"LoaderBody", @"The announcement arrived as she inspected an enormous component. “A place with supplies? Hope they made room for heavy equipment!” She saved the reference and returned to her gauntlets." },
            { @"CrocoBody", @"A voice emerged from an abandoned device. Acrid raised its head, sniffed, and heard a series of sounds without meaning. One claw nudged the object; the voice continued. Then it wandered away, unaware of orders or camps." },
            { @"CaptainBody", @"After broadcasting, Captain checked the communications systems. He did not know who had heard the message. But it was sent. Now he had to keep the position running and answer those who made it back." }
        };
        private const string NoticeEs = @"UES SAFE TRAVELS — COMUNICADO GENERAL
ORIGEN: Mando de expedición
DESTINATARIOS: Equipos desplegados y supervivientes en alcance

Se ha establecido una posición de operaciones en la superficie. Hay suministros, comunicaciones y asistencia para heridos. Transmitan su situación y los datos obtenidos de las regiones exploradas. Las referencias de navegación y rutas disponibles se proporcionarán por canales autorizados. No conocemos aún todo el terreno. Si reciben este mensaje, mantengan abiertas las comunicaciones. Hay un lugar al que regresar.";
        private const string NoticeEn = @"UES SAFE TRAVELS — GENERAL BROADCAST
SOURCE: Expedition command
RECIPIENTS: Deployed teams and survivors within range

An operations position has been established on the surface. Supplies, communications, and aid for the injured are available. Report your status and findings from explored regions. Navigation references and known routes will be provided through authorized channels. Much of the terrain remains unknown. If you receive this message, keep communications open. There is a place to return to.";
        private const string HintEs = @"<color=#888888>REGISTRO III · SEÑAL INTERFERIDA</color>
<color=#666666>▓▒████░▓██▒████▓▒░███▓▒░</color>
<color=#BBBBBB>██▓▒ ...a todas las unidades... ▓██</color>
<color=#666666>▒████▓░▒███▓████▒░▓████</color>
<color=#BBBBBB>▓▒██ ...mantengan... ███▒▓</color>
<color=#666666>██▓▒░████▒▓███░▒████▓▒</color>
<color=#FFFFFF>▒██ ...un lugar al que regresar... ██▓</color>
<color=#666666>▓▒██████░▓██▒████░▓▒███</color>
<color=#BBBBBB>███ ...comunicaciones... ░▓██</color>
<color=#666666>▒▓███░████▓▒██░▓████▒░</color>";
        private const string HintEn = @"<color=#888888>LOG III · SIGNAL INTERFERENCE</color>
<color=#666666>▓▒████░▓██▒████▓▒░███▓▒░</color>
<color=#BBBBBB>██▓▒ ...to all units... ▓██</color>
<color=#666666>▒████▓░▒███▓████▒░▓████</color>
<color=#BBBBBB>▓▒██ ...maintain... ███▒▓</color>
<color=#666666>██▓▒░████▒▓███░▒████▓▒</color>
<color=#FFFFFF>▒██ ...a place to return to... ██▓</color>
<color=#666666>▓▒██████░▓██▒████░▓▒███</color>
<color=#BBBBBB>███ ...communications... ░▓██</color>
<color=#666666>▒▓███░████▓▒██░▓████▒░</color>";

        internal static string Append(SurvivorContentProfile profile, bool hidden, string existing)
        {
            if (profile == null || hidden) return existing;
            return AppendForBody(profile.BodyName, existing);
        }

        internal static string AppendForBody(string bodyName, string existing)
        {
            string body = bodyName ?? "";
            bool es = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
            Dictionary<string,string> scenes = es ? Es : En;
            if (string.Equals(body, "BanditBody", StringComparison.OrdinalIgnoreCase)) body = "Bandit2Body";
            string scene;
            if (!scenes.TryGetValue(body, out scene)) return existing;
            string title = es ? "Diario III — Una señal entre los supervivientes (USU)" : "Log III — A Signal Among Survivors (USU)";
            string content = IsUnlockedForLocalProfile()
                ? (es ? NoticeEs : NoticeEn) + "\n\n" + scene
                : (es ? HintEs : HintEn);
            return (existing ?? "") + "\n\n<size=18><b>" + title + "</b></size>\n" + content;
        }

        // Must be invoked after Captain has ACTUALLY completed one of two allowed endings.
        // Do not invoke on a normal death, non-Captain run, or simply entering the Moon/Obelisk.
        internal static void RecordCaptainVictory(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return;
            string path = PathForProfile(profileId);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            if (!File.Exists(path)) File.WriteAllText(path, "captain-finish-v1", Encoding.UTF8);
        }

        internal static bool IsUnlockedForLocalProfile()
        {
            var local = LocalUserManager.GetFirstLocalUser();
            if (local == null || local.userProfile == null) return false;
            string id = local.userProfile.name;
            return !string.IsNullOrWhiteSpace(id) && File.Exists(PathForProfile(id));
        }

        private static string PathForProfile(string profileId)
        {
            string safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(profileId))
                .Replace('/', '_').Replace('+', '-').TrimEnd('=');
            return System.IO.Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "VanillaDiaryIII", safe + ".flag");
        }
    }
}
