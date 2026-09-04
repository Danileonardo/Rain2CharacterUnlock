using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class HoldItemStackTracker
    {
        private static ManualLogSource logger;
        private static bool initialized;
        private static float nextPollTime;


        /*
         * Guardamos el último conteo conocido
         * de cada jugador.
         *
         * IMPORTANTE:
         * cada CharacterMaster tiene su propio valor.
         *
         * Nunca sumamos inventarios de jugadores.
         */
        private static readonly Dictionary<
            CharacterMaster,
            int
        > LastPlayerCounts =
            new Dictionary<CharacterMaster, int>();


        // =========================================================
        // EVENTO
        // =========================================================
        //
        // CharacterMaster = jugador que posee los objetos.
        // int             = cantidad que tiene ESE jugador.
        //
        // NO representa un total del equipo.
        // =========================================================

        public static event Action<
            CharacterMaster,
            int
        > ItemCountChanged;


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


            RoR2Application.onFixedUpdate +=
                FixedUpdate;


            Run.onRunStartGlobal +=
                OnRunStart;


            Run.onRunDestroyGlobal +=
                OnRunEnd;


            logger?.LogInfo(
                "HoldItemStackTracker inicializado."
            );
        }


        // =========================================================
        // INICIO DE RUN
        // =========================================================

        private static void OnRunStart(
            Run run
        )
        {
            LastPlayerCounts.Clear();


            logger?.LogInfo(
                "[HoldItemStack] Nueva run detectada | " +
                "Contadores individuales reiniciados."
            );
        }


        // =========================================================
        // FIN DE RUN
        // =========================================================

        private static void OnRunEnd(
            Run run
        )
        {
            LastPlayerCounts.Clear();
        }


        // =========================================================
        // ACTUALIZACIÓN
        // =========================================================

        private static void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance == null)
            {
                return;
            }


            if (!MissionRuntimeActivityPlan.IsTypeActive("HoldItemStack"))
            {
                return;
            }


            float now = UnityEngine.Time.unscaledTime;
            if (now < nextPollTime)
            {
                return;
            }

            nextPollTime = now + 0.20f;


            /*
             * PlayerCharacterMasterController.instances
             * contiene los masters pertenecientes
             * a jugadores.
             */
            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                if (controller == null)
                {
                    continue;
                }


                CharacterMaster master =
                    controller.master;


                if (
                    master == null ||
                    master.inventory == null
                )
                {
                    continue;
                }


                // =================================================
                // ENERGY DRINK
                // =================================================
                //
                // RoR2Content.Items.SprintBonus
                // corresponde a Energy Drink.
                // =================================================

                int currentCount =
                    master.inventory.GetItemCountEffective(
                        RoR2Content.Items.SprintBonus
                    );


                int previousCount =
                    -1;


                LastPlayerCounts.TryGetValue(
                    master,
                    out previousCount
                );


                /*
                 * Si no cambió, no generamos
                 * ningún evento adicional.
                 */
                if (
                    previousCount ==
                    currentCount
                )
                {
                    continue;
                }


                LastPlayerCounts[
                    master
                ] =
                    currentCount;

                ItemCountChanged?.Invoke(
                    master,
                    currentCount
                );
            }
        }


        // =========================================================
        // OBTENER MAYOR CANTIDAD INDIVIDUAL ACTUAL
        // =========================================================
        //
        // Devuelve la mayor cantidad que tenga
        // UN solo jugador.
        //
        // Ejemplo:
        //
        // Jugador A = 8
        // Jugador B = 7
        //
        // Resultado = 8
        //
        // NO = 15
        // =========================================================

        public static int GetHighestCurrentCount()
        {
            if (!NetworkServer.active)
            {
                return 0;
            }


            int highest =
                0;


            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                if (
                    controller == null ||
                    controller.master == null ||
                    controller.master.inventory == null
                )
                {
                    continue;
                }


                int currentCount =
                    controller
                        .master
                        .inventory
                        .GetItemCountEffective(
                            RoR2Content.Items.SprintBonus
                        );


                if (
                    currentCount >
                    highest
                )
                {
                    highest =
                        currentCount;
                }
            }


            return highest;
        }
    }
}