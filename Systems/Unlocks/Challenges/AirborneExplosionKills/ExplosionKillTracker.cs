using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class ExplosionKillTracker
    {
        private static ManualLogSource logger;
        private static bool initialized;


        /*
         * Un BlastAttack puede generar otro BlastAttack
         * durante el procesamiento de procs.
         *
         * Por ejemplo:
         *
         * ataque
         *   -> explosión
         *       -> Behemoth
         *           -> otra explosión
         *
         * Por eso usamos una pila y no una sola variable.
         */
        private static readonly Stack<BlastAttack>
            ActiveBlasts =
                new Stack<BlastAttack>();


        // =========================================================
        // EVENTO
        // =========================================================
        //
        // IMPORTANTE:
        //
        // Este evento significa:
        //
        // "un BlastAttack fue el golpe mortal"
        //
        // Todavía NO significa:
        //
        // "esta era una explosión válida para Rocket"
        //
        // La clasificación la haremos después.
        // =========================================================

        public static event Action<
            CharacterMaster,
            DamageReport,
            BlastAttack
        > LethalBlastDetected;


        // =========================================================
        // CONTEXTO DE BLAST ACTIVO PARA MISSION SCHEMA V2
        // =========================================================
        //
        // Permite que el runtime genérico clasifique una baja como
        // explosiva SIN duplicar la detección de BlastAttack.
        //
        // La pila sólo contiene un BlastAttack mientras Fire() está
        // procesando su daño. Si onCharacterDeathGlobal se dispara
        // dentro de esa llamada, la muerte fue causada directamente
        // por ese blast.
        // =========================================================

        public static bool TryGetActiveBlast(
            out BlastAttack blastAttack
        )
        {
            blastAttack =
                null;


            if (
                !NetworkServer.active ||
                ActiveBlasts.Count <= 0
            )
            {
                return false;
            }


            blastAttack =
                ActiveBlasts.Peek();


            return blastAttack != null;
        }


        // =========================================================
        // INICIALIZAR
        // =========================================================

        public static void Initialize(
            ManualLogSource pluginLogger
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;


            logger =
                pluginLogger;


            On.RoR2.BlastAttack.Fire +=
                OnBlastAttackFire;


            GlobalEventManager
                .onCharacterDeathGlobal +=
                OnCharacterDeathGlobal;


            Run.onRunStartGlobal +=
                OnRunStart;


            Run.onRunDestroyGlobal +=
                OnRunEnd;


            logger?.LogInfo(
                "ExplosionKillTracker inicializado."
            );
        }


        // =========================================================
        // INICIO DE RUN
        // =========================================================

        private static void OnRunStart(
            Run run
        )
        {
            ActiveBlasts.Clear();
        }


        // =========================================================
        // FIN DE RUN
        // =========================================================

        private static void OnRunEnd(
            Run run
        )
        {
            ActiveBlasts.Clear();
        }


        // =========================================================
        // BLAST ATTACK
        // =========================================================

        private static BlastAttack.Result OnBlastAttackFire(
            On.RoR2.BlastAttack.orig_Fire orig,
            BlastAttack self
        )
        {
            /*
             * En cliente simplemente dejamos pasar
             * el BlastAttack.
             *
             * Toda nuestra detección será
             * autoritativa en el servidor.
             */
            if (!NetworkServer.active)
            {
                return orig(
                    self
                );
            }


            ActiveBlasts.Push(
                self
            );


            try
            {
                /*
                 * El daño del BlastAttack ocurre
                 * dentro de esta llamada.
                 *
                 * Si un enemigo muere aquí,
                 * onCharacterDeathGlobal se ejecutará
                 * mientras este blast siga en la pila.
                 */
                return orig(
                    self
                );
            }
            finally
            {
                /*
                 * Protección adicional por si algún
                 * otro mod provoca una excepción.
                 */
                if (
                    ActiveBlasts.Count >
                    0
                )
                {
                    ActiveBlasts.Pop();
                }
            }
        }


        // =========================================================
        // MUERTE
        // =========================================================

        private static void OnCharacterDeathGlobal(
            DamageReport damageReport
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance == null)
            {
                return;
            }


            if (damageReport == null)
            {
                return;
            }


            /*
             * Si no estamos dentro de BlastAttack.Fire(),
             * esta muerte NO fue causada directamente
             * por el blast que estamos observando.
             */
            if (
                ActiveBlasts.Count <=
                0
            )
            {
                return;
            }


            BlastAttack activeBlast =
                ActiveBlasts.Peek();


            if (activeBlast == null)
            {
                return;
            }


            // =====================================================
            // RESOLVER MASTER QUE PROVOCÓ LA MUERTE
            // =====================================================

            CharacterMaster sourceMaster =
                damageReport.attackerMaster;


            /*
             * Algunos BlastAttack pueden no entregar
             * attackerMaster correctamente en el
             * DamageReport.
             *
             * Intentamos obtenerlo desde attacker
             * como fallback.
             */
            if (
                sourceMaster == null &&
                activeBlast.attacker != null
            )
            {
                CharacterBody attackerBody =
                    activeBlast
                        .attacker
                        .GetComponent<CharacterBody>();


                if (attackerBody != null)
                {
                    sourceMaster =
                        attackerBody.master;
                }
            }


            if (sourceMaster == null)
            {
                return;
            }


            // =====================================================
            // RESOLVER JUGADOR PROPIETARIO
            // =====================================================
            //
            // Jugador
            //     -> jugador
            //
            // Missile Drone
            //     -> ownerMaster
            //     -> jugador
            //
            // Minion
            //     -> owner
            //     -> jugador
            // =====================================================

            CharacterMaster playerOwner =
                PlayerOwnerResolver
                    .ResolveOwningPlayerMaster(
                        sourceMaster
                    );


            if (playerOwner == null)
            {
                return;
            }


            // =====================================================
            // INFORMACIÓN DE DIAGNÓSTICO
            // =====================================================

            string attackerName =
                activeBlast.attacker != null
                    ? activeBlast.attacker.name
                    : "<null>";


            string inflictorName =
                activeBlast.inflictor != null
                    ? activeBlast.inflictor.name
                    : "<null>";


            string victimName =
                damageReport.victimBody != null
                    ? damageReport.victimBody.name
                    : "<desconocido>";


            string playerName =
                GetPlayerName(
                    playerOwner
                );


            string damageSourceName =
    GetDamageSourceName(
        activeBlast
            .damageType
            .damageSource
    );


            CharacterBody sourceBody =
                sourceMaster.GetBody();


            string sourceBodyName =
                sourceBody != null
                    ? sourceBody.name
                    : "<sin body>";


            CharacterBody ownerBody =
                playerOwner.GetBody();


            string ownerBodyName =
                ownerBody != null
                    ? ownerBody.name
                    : "<sin body>";


            float lethalDamage =
                damageReport
                    .damageInfo
                    .damage;


            bool wasCritical =
                damageReport
                    .damageInfo
                    .crit;


            // El detalle de cada Blast mortal generaba una línea por baja.
            // BombingRun/AirborneExplosionKills ya reportan progreso por
            // hitos, por lo que aquí no escribimos diagnóstico de release.



            // =====================================================
            // AVISAR
            // =====================================================

            LethalBlastDetected?.Invoke(
                playerOwner,
                damageReport,
                activeBlast
            );
        }

        // =========================================================
        // CLASIFICAR DAMAGE SOURCE
        // =========================================================
        //
        // DamageSource funciona como flags.
        //
        // Esto NO determina todavía si una explosión
        // es válida para Rocket.
        //
        // Sólo nos permite saber de qué slot de habilidad
        // parece provenir el BlastAttack.
        //
        // 1 = Primary
        // 2 = Secondary
        // 4 = Utility
        // 8 = Special
        // =========================================================

        private static string GetDamageSourceName(
            DamageSource damageSource
        )
        {
            int rawValue =
                (int)damageSource;


            if (rawValue == 0)
            {
                return "NoneSpecified / Proc / Item / desconocido";
            }


            List<string> sources =
                new List<string>();


            if (
                (rawValue & 1) !=
                0
            )
            {
                sources.Add(
                    "Primary"
                );
            }


            if (
                (rawValue & 2) !=
                0
            )
            {
                sources.Add(
                    "Secondary"
                );
            }


            if (
                (rawValue & 4) !=
                0
            )
            {
                sources.Add(
                    "Utility"
                );
            }


            if (
                (rawValue & 8) !=
                0
            )
            {
                sources.Add(
                    "Special"
                );
            }


            if (sources.Count <= 0)
            {
                return
                    $"Otro / Raw={rawValue}";
            }


            return string.Join(
                " + ",
                sources
            );
        }

        // =========================================================
        // NOMBRE DEL JUGADOR
        // =========================================================

        private static string GetPlayerName(
            CharacterMaster master
        )
        {
            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                if (
                    controller == null ||
                    controller.master != master
                )
                {
                    continue;
                }


                string displayName =
                    controller.GetDisplayName();


                if (
                    !string.IsNullOrWhiteSpace(
                        displayName
                    )
                )
                {
                    return displayName;
                }
            }


            return
                master.name;
        }
    }
}