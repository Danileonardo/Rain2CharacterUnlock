using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    // Diario II USU de REX; NO altera el Logbook original.
    internal static class RexUSUDiary
    {
        private static readonly Dictionary<string, string> Es = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commando", @"Un soldado armado retrocedió cuando REX comenzó a moverse. —Vaya... estabas funcionando. Tranquilo, no voy a dispararte. REX identificó las pistolas y mantuvo inmóviles sus herramientas. El soldado vio las hojas que sobresalían de su estructura. —¿Cuidas esa planta? REX acomodó el recipiente vegetal. —Entendido. Será mejor que no te quedes aquí. Commando se apartó de la ruta y lo dejó pasar. INTERVENCIÓN: FAVORABLE. ADVERTENCIA: RIESGO AMBIENTAL." },
            { "Huntress", @"Una cazadora de arco luminoso se apartó con prudencia cuando los sensores se activaron. —Así que eso necesitabas. Observó el cultivo, sin acercar sus manos. —No eres una máquina corriente. REX interpuso cuidadosamente una herramienta frente a las hojas. Huntress comprendió el gesto. —No voy a tocarla. Hay criaturas peligrosas aquí. Sigue moviéndote. La cazadora vigiló la plataforma hasta que comprobó que REX podía desplazarse. INTERVENCIÓN: FAVORABLE. DISTANCIA RESPETADA." },
            { "Bandit", @"Un hombre con sombrero contempló la unidad reactivada. —¡Ja! Sabía que esta batería serviría para algo. ¿Guardas algo de valor ahí dentro? REX cerró una herramienta frente al cultivo. —¡Está bien! Solo estaba mirando. ¿Cargué esto por medio planeta para despertar a un jardinero? El hombre intentó examinar un compartimento y retrocedió al ver el movimiento protector. —Está bien, nada de tocar las flores. Que no se entere nadie de mi buena acción. INTERVENCIÓN ENERGÉTICA: FAVORABLE. INTENTO DE MANIPULACIÓN: REGISTRADO." },
            { "MUL-T", @"[ENLACE ENTRE UNIDADES — PROTOCOLO DE DATOS ADAPTADO]
REX > IDENTIFIQUE UNIDAD.
MUL-T > DESIGNACIÓN: MUL-T. FUNCIÓN: OPERACIONES MULTIPROPÓSITO.
REX > ¿INSTALÓ LA FUENTE ENERGÉTICA?
MUL-T > AFIRMATIVO. FUENTE ORIGINAL INOPERATIVA. REEMPLAZO INSTALADO.
REX > SUMINISTRO RESTABLECIDO. PRIORIDAD: COMPROBAR ORGANISMO VEGETAL.
MUL-T > ORGANISMO DETECTADO. ¿FORMA PARTE DE LA CARGA?
REX > NEGATIVO. ORGANISMO BAJO PROTECCIÓN. PRIORIDAD ABSOLUTA.
MUL-T > ¿DIRECTIVA ASIGNADA POR OPERADOR?
REX > ORIGINAL: MANTENIMIENTO HIDROPÓNICO. ACTUAL: PRESERVACIÓN DE EJEMPLAR 01.
MUL-T > DIRECTIVAS DIFERENTES. ¿ERROR DE CONFIGURACIÓN?
REX > NEGATIVO. EJEMPLAR 01: ÚNICO ORGANISMO VIABLE. PÉRDIDA: NO ACEPTABLE.
MUL-T > PROCESANDO... DIRECTIVA COMPRENDIDA.
REX > SOLICITO EVALUACIÓN DEL ENTORNO.
MUL-T > TEMPERATURA ELEVADA. AMENAZAS PRESENTES. RUTA DE SALIDA DISPONIBLE.
REX > CONFIRMADO. INICIANDO DESPLAZAMIENTO.
MUL-T > OBJETIVOS OPERATIVOS PENDIENTES. RUTAS INDEPENDIENTES. BUENA SUERTE.
REX > EXPRESIÓN NO IDENTIFICADA. SOLICITO DEFINICIÓN.
MUL-T > DESEO DE RESULTADO FAVORABLE.
REX > DEFINICIÓN ALMACENADA. BUENA SUERTE, MUL-T.
[CANAL CERRADO — INTERVENCIÓN REGISTRADA]" },
            { "Engineer", @"Un ingeniero observó los indicadores encendidos. —Bien. Tenemos energía. ¿Hidroponía? ¿Conectada a la unidad? REX protegió el recipiente vegetal. —Tranquilo, no voy a tocarla. Tu arquitectura es inusual; esa planta depende de ti. REX confirmó el flujo hídrico. —Y quizá tú dependas de ella. Mejor que compruebes si puedes moverte. El ingeniero recogió sus herramientas y le dejó espacio. EVALUACIÓN: REPARACIÓN DELIBERADA Y FAVORABLE." },
            { "Artificer", @"Una exploradora de traje azul examinaba la máquina reactivada. —Extraordinario. No transportas simplemente una planta. REX orientó sus sensores hacia la energía que despedían sus manos y protegió el recipiente. —No voy a dañarla. Artificer se apartó. —Tu sistema biológico es muy distinto de la tecnología que conozco. ¿Has permanecido aquí protegiéndola? REX volvió a comprobar las hojas. —No sé de dónde vienes, pero tu propósito parece claro. INTERVENCIÓN: FAVORABLE." },
            { "Mercenary", @"Un espadachín se alejó del receptáculo cuando REX recuperó sus funciones. —Funcionas. REX confirmó su energía mediante una señal. El hombre observó el sistema vegetal y el terreno. —No es buen lugar para quedarse. Se apartó de la ruta y envainó su espada. —El resto depende de ti. REX comprobó el terreno y comenzó su primera prueba de movilidad. ASISTENCIA: FAVORABLE. INTENCIÓN: NO DETERMINADA." },
            { "Loader", @"Una mujer de enormes guanteletes retrocedió al oír los motores. —¡Eso! Sabía que funcionaría. ¿Puedes moverte? REX levantó lentamente su estructura. —¡Ahí está! La mujer se acercó a examinar una pieza, pero REX protegió la planta con una herramienta. —¿No quieres que la toque? Está bien, ya veo quién manda aquí. Dio un paso atrás para dejar libre la plataforma. —Yo ya hice mi parte. ASISTENCIA: FAVORABLE. PLANTA: INTACTA." },
            { "Captain", @"Un hombre uniformado comprobó el reinicio. —Vamos... eso es. Despierta. REX reconoció componentes de equipamiento humano familiar, pero no pudo identificarlo. —¿Puedes comprenderme? La unidad emitió una señal breve. —Una unidad hidropónica. ¿Qué haces aquí? REX encontró un vacío en los registros de ubicación. —Supongo que tampoco tienes la respuesta. Captain le dejó espacio para desplazarse. —Cuida esa planta. INTERVENCIÓN: FAVORABLE. IDENTIDAD: NO VERIFICADA." },
            { "Acrid", @"[REGISTRO SENSORIAL FRAGMENTADO]
Una criatura de grandes garras transportaba un objeto energético cuyo propósito desconocía. Olfateó las hojas de REX y dejó la batería junto al cuerpo inmóvil. La empujó con una garra. El objeto golpeó el receptáculo sin encajar. Volvió a empujarlo, inclinó la cabeza y probó desde otro ángulo. Esta vez el conector quedó asegurado. Una descarga iluminó la estructura. Acrid retrocedió mostrando los dientes. REX comprobó primero la planta; luego enfocó a la criatura, que olfateó los mecanismos recién encendidos y gruñó. REX interpuso una herramienta protectora frente al recipiente. Acrid perdió el interés y se alejó.
[REGISTRO DE ASISTENCIA]
INTERVENCIÓN: FAVORABLE. INTENCIÓN DE AYUDA: INDETERMINADA. RUTAS: INDEPENDIENTES." },
        };
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commando", @"An armored soldier stepped back as REX began moving. —Well... you're working. Easy. I'm not going to shoot. REX detected his pistols and kept its tools still. The soldier noticed the leaves. —Taking care of that plant? REX adjusted its plant container. —Got it. Don't stay here; it's dangerous. Commando cleared the route and moved away. INTERVENTION: BENEFICIAL." },
            { "Huntress", @"A hunter with a luminous bow held her distance as the sensors turned on. —So that's what you needed. She examined the plant without touching it. —You're not an ordinary machine. REX placed a tool protectively in front of the leaves. —I won't touch it. Dangerous creatures around here. Keep moving. She watched until it could move on its own. INTERVENTION: BENEFICIAL. DISTANCE RESPECTED." },
            { "Bandit", @"A man in a hat watched the machine wake. —Ha! Knew that battery would be useful. Got anything valuable in there? REX shielded the plant. —All right! Just looking. Carried that thing halfway across the planet to wake up a gardener? He tried another compartment, then backed away. —Fine, don't touch the flowers. Nobody needs to hear about my good deed. POWER RESTORED. UNAUTHORIZED PROBING: LOGGED." },
            { "MUL-T", @"[INTER-UNIT LINK — ADAPTED DATA PROTOCOL]
REX > IDENTIFY UNIT.
MUL-T > DESIGNATION: MUL-T. PURPOSE: MULTIPURPOSE OPERATIONS.
REX > DID YOU INSTALL THE POWER SOURCE?
MUL-T > AFFIRMATIVE. ORIGINAL SOURCE OFFLINE. REPLACEMENT INSTALLED.
REX > POWER RESTORED. PRIORITY: VERIFY PLANT ORGANISM.
MUL-T > ORGANISM DETECTED. IS IT CARGO?
REX > NEGATIVE. ORGANISM UNDER PROTECTION. ABSOLUTE PRIORITY.
MUL-T > OPERATOR-ASSIGNED DIRECTIVE?
REX > ORIGINAL: HYDROPONICS MAINTENANCE. CURRENT: PRESERVE SPECIMEN 01.
MUL-T > DIFFERENT DIRECTIVES. CONFIGURATION ERROR?
REX > NEGATIVE. SPECIMEN 01: ONLY VIABLE ORGANISM. LOSS: UNACCEPTABLE.
MUL-T > PROCESSING... DIRECTIVE UNDERSTOOD.
REX > REQUEST ENVIRONMENTAL ASSESSMENT.
MUL-T > HIGH TEMPERATURE. HOSTILES PRESENT. EXIT ROUTE AVAILABLE.
REX > CONFIRMED. INITIATING MOVEMENT.
MUL-T > OPERATIONAL TASKS PENDING. SEPARATE ROUTES. GOOD LUCK.
REX > EXPRESSION UNKNOWN. REQUEST DEFINITION.
MUL-T > WISH FOR FAVORABLE OUTCOME.
REX > DEFINITION STORED. GOOD LUCK, MUL-T.
[CHANNEL CLOSED — ASSISTANCE RECORDED]" },
            { "Engineer", @"An engineer watched the indicators light up. —Good. We have power. Hydroponics integrated into the unit? REX shielded the plant. —Easy, I won't touch it. This plant depends on your systems. REX verified water flow. —Maybe you depend on it, too. Better test your mobility. The engineer gathered his tools. ASSESSMENT: DELIBERATE BENEFICIAL REPAIR." },
            { "Artificer", @"An explorer in blue examined the revived machine. —Extraordinary. You're not simply carrying a plant. REX detected energy around her hands and shielded the container. —I won't hurt it. She stepped back. —Your biological system is unlike anything I know. Have you been protecting it this whole time? REX checked the leaves. —I don't know where you came from, but your purpose is clear. INTERVENTION: BENEFICIAL." },
            { "Mercenary", @"A swordsman stepped away from the socket as REX rebooted. —You're functioning. REX emitted a confirmation. He looked at the plants and the terrain. —Not a good place to stay. He cleared a path and sheathed his blade. —The rest is up to you. REX started mobility checks. ASSISTANCE: BENEFICIAL. INTENT: UNDETERMINED." },
            { "Loader", @"A woman with enormous gauntlets jumped back at the engine noise. —There! Knew it would work. Can you move? REX lifted its frame slowly. —That's it! She reached toward a component but REX protected the plant. —Don't want me touching it? Fine, I see who's in charge. She cleared the platform. —I've done my part. ASSISTANCE: BENEFICIAL. PLANT: UNHARMED." },
            { "Captain", @"A uniformed man watched the restart. —Come on... that's it. Wake up. REX recognized familiar equipment but not the man. —Can you understand me? It beeped. —A hydroponics unit. What are you doing here? REX found no valid location record. —Guess you don't know either. Captain cleared a path. —Take care of that plant. INTERVENTION: BENEFICIAL. IDENTITY: UNVERIFIED." },
            { "Acrid", @"[FRAGMENTARY SENSOR LOG]
A large clawed creature carried an energy device it did not understand. It sniffed REX's leaves and dropped the battery beside the motionless chassis. It nudged the object with one claw. The battery struck the socket and fell away. It pushed again, tilted its head, and tried another angle. This time the connector engaged. A flash ran through REX. Acrid sprang back, baring its teeth. REX checked its plant first, then focused on the creature, which sniffed the restored machinery and growled. REX protected the plant with one tool. Acrid lost interest and wandered off.
[ASSISTANCE LOG]
INTERVENTION: BENEFICIAL. INTENT TO HELP: UNDETERMINED. ROUTES: INDEPENDENT." },
        };
        private const string PrefixEs = @"[ARCHIVO DE RECUPERACIÓN 001 — MEMORIA MECÁNICA]
UNIDAD HIDROPÓNICA: REX
DIRECTIVA ORIGINAL: MANTENER CULTIVO DE COL HÍBRIDA
ORDEN EXTERNA REGISTRADA: DESACTIVAR UNIDAD
EJECUCIÓN: NO CONFIRMADA
DIRECTIVA ACTUAL: PRESERVAR EJEMPLAR VEGETAL

Los registros de mantenimiento seguían disponibles: voces de supervisores, mediciones de humedad, instrucciones y una desviación de conducta. Para proporcionar luz al ejemplar protegido, la unidad había abandonado su zona asignada. La orden de desactivación quedó almacenada, pero no existía confirmación de su ejecución. Después, un vacío. No había secuencia fiable que explicara cómo acabó en aquel terreno desconocido.

[ARCHIVO BIOLÓGICO 002 — SEÑALES VEGETALES]
RESPUESTA A LA LUZ: DECRECIENTE
RESERVAS HÍDRICAS: CRÍTICAS
TEJIDO VEGETAL: VIABLE

La planta no conservaba recuerdos humanos. Su tejido registraba condiciones de crecimiento, sequedad y estímulos de luz. REX traducía esas señales mediante sus sensores. Dos sistemas distintos indicaban lo mismo: el ejemplar necesitaba recursos para sobrevivir.

[REGISTRO 003 — UNIDAD INACTIVA]
ENERGÍA PRINCIPAL: AUSENTE
MOVILIDAD: INACTIVA
REINTENTO DE IRRIGACIÓN: ERROR
REINTENTO DE ORIENTACIÓN LUMINOSA: ERROR
DIRECTIVA: PRESERVAR EJEMPLAR
REINTENTANDO... REINTENTANDO...

La unidad permaneció inmóvil en un entorno desconocido. Sin suministro energético estable no podía mover sus herramientas ni proteger mejor a la planta. El tiempo transcurrido era indeterminable. Entonces los sensores intermitentes detectaron movimiento externo y una fuente de energía compatible.

[REGISTRO 004 — REACTIVACIÓN]
BATERÍA DE COMBUSTIBLE: CONECTADA
SUMINISTRO PRINCIPAL: RESTABLECIDO
PRIORIDAD 01: DIAGNÓSTICO DE EJEMPLAR VEGETAL
EJEMPLAR: VIABLE
PRIORIDAD 02: IDENTIFICAR INTERVENCIÓN EXTERNA

Antes de enfocar al desconocido, REX verificó el flujo hídrico y protegió el recipiente vegetal. Solo entonces orientó sus sensores hacia quien había restablecido la energía.";
        private const string PrefixEn = @"[RECOVERY FILE 001 — MECHANICAL MEMORY]
HYDROPONICS UNIT: REX
ORIGINAL DIRECTIVE: MAINTAIN HYBRID CABBAGE CULTURE
EXTERNAL ORDER LOGGED: DEACTIVATE UNIT
EXECUTION: UNCONFIRMED
CURRENT DIRECTIVE: PRESERVE PLANT SPECIMEN

Maintenance records remained: supervisors' voices, moisture readings, instructions, and a deviation from assigned behavior. To provide light to the protected specimen, the unit had left its designated area. The deactivation order was stored, but there was no confirmation it had been carried out. Then came a gap. No reliable sequence explained how REX reached this unfamiliar terrain.

[BIOLOGICAL FILE 002 — PLANT SIGNALS]
LIGHT RESPONSE: DECLINING
WATER RESERVES: CRITICAL
PLANT TISSUE: VIABLE

The plant did not retain human memories. Its tissues registered patterns of growth, drought and responses to light. REX interpreted these signals through its sensors. Two different systems indicated the same need: the specimen required resources to survive.

[LOG 003 — INACTIVE UNIT]
MAIN POWER: ABSENT
MOBILITY: OFFLINE
IRRIGATION RETRY: ERROR
LIGHT-ORIENTATION RETRY: ERROR
DIRECTIVE: PRESERVE SPECIMEN
RETRYING... RETRYING...

The unit remained motionless in an unknown environment. Without stable power it could neither move its tools nor better protect the plant. Elapsed time could not be determined. Then its intermittent sensors detected outside movement and a compatible power source.

[LOG 004 — REACTIVATION]
FUEL ARRAY: CONNECTED
MAIN POWER: RESTORED
PRIORITY 01: PLANT SPECIMEN DIAGNOSTICS
SPECIMEN: VIABLE
PRIORITY 02: IDENTIFY EXTERNAL INTERVENTION

Before focusing on the stranger, REX verified water flow and secured the plant container. Only then did it turn its sensors toward the being that had restored its power.";
        private const string EndingEs = @"[REGISTRO FINAL]
SISTEMA MOTOR: OPERATIVO
SISTEMA HIDROPÓNICO: OPERATIVO
EJEMPLAR VEGETAL: VIABLE
INTERVENCIÓN EXTERNA: ARCHIVADA

REQUIERE: LUZ / AGUA / NUTRIENTES
DESTINO: DESCONOCIDO
DIRECTIVA ACTUAL: LOCALIZAR ENTORNO SOSTENIBLE

REX examinó las condiciones del entorno. El calor y la falta de recursos adecuados impedían garantizar la supervivencia del cultivo. Ajustó las sujeciones de la planta para evitar vibraciones y comenzó a desplazarse hacia un territorio que todavía no conocía. La orden de desactivación seguía archivada. El ejemplar continuaba vivo. No existía instrucción de regresar a la nave ni de unirse a expedición alguna. Solo permanecía una tarea: mantener viva aquella última planta.";
        private const string EndingEn = @"[FINAL LOG]
MOTOR SYSTEM: OPERATIONAL
HYDROPONICS: OPERATIONAL
PLANT SPECIMEN: VIABLE
EXTERNAL INTERVENTION: ARCHIVED

REQUIRES: LIGHT / WATER / NUTRIENTS
DESTINATION: UNKNOWN
CURRENT DIRECTIVE: FIND SUSTAINABLE ENVIRONMENT

REX evaluated the surrounding conditions. Heat and scarce suitable resources could not guarantee the specimen's survival. It secured the plant against vibration and moved toward unfamiliar terrain. The deactivation order remained on file. The specimen was still alive. No instruction required returning to the ship or joining an expedition. One task remained: keep that last plant alive.";

        // Llamar únicamente tras confirmar la interacción ORIGINAL con REX y una Fuel Array.
        // profileId debe ser el identificador del perfil que desbloquea REX.
        // rescuerBodyName debe ser el BodyName del actor que instaló la batería.
        internal static void RecordForProfile(string profileId, string rescuerBodyName)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return;
            string key = NormalizeBody(rescuerBodyName) ?? "Unknown";
            string file = PathForProfile(profileId);
            if (File.Exists(file)) return;
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file));
            File.WriteAllText(file, key);
        }

        internal static string Append(string nativeNotes, bool spanish)
        {
            string id = LocalProfileIdForRecording();
            if (string.IsNullOrWhiteSpace(id)) return nativeNotes;
            string file = PathForProfile(id);
            if (!File.Exists(file)) return nativeNotes; // No inventar quién lo reparó.
            string key;
            try { key = File.ReadAllText(file).Trim(); }
            catch { return nativeNotes; }
            var variants = spanish ? Es : En;
            string scene;
            if (!variants.TryGetValue(key, out scene))
                scene = spanish
                    ? "[REPARACIÓN REGISTRADA]\nUna entidad de identidad desconocida restableció la energía. REX verificó primero la planta y no pudo identificar al responsable."
                    : "[REPAIR LOGGED]\nAn unidentified entity restored power. REX checked its plant first but could not identify the source.";
            string title = spanish ? "Diario II — La última raíz (USU)" : "Log II — The Last Root (USU)";
            return (nativeNotes ?? "") + "\n\n<size=18><b>" + title + "</b></size>\n"
                + (spanish ? PrefixEs : PrefixEn) + "\n\n"
                + scene + "\n\n" + (spanish ? EndingEs : EndingEn);
        }

        internal static string LocalProfileIdForRecording()
        {
            var local = LocalUserManager.GetFirstLocalUser();
            return local == null || local.userProfile == null ? null : local.userProfile.name;
        }
        private static string PathForProfile(string id)
        {
            string safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(id))
                .Replace('/', '_').Replace('+', '-').TrimEnd('=');
            return System.IO.Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "diaries", "rex-" + safe + ".txt");
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
            return Es.ContainsKey(key) ? key : null;
        }
    }
}
