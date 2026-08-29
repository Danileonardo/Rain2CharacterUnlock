using System;
using BepInEx.Logging;
using EntityStates;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class StatusEffectTracker
    {
        private static ManualLogSource logger;

        private static bool initialized;


        public static int CurrentNegativeCount
        {
            get;
            private set;
        }


        public static int CurrentPositiveCount
        {
            get;
            private set;
        }


        public static int CurrentTotalCount
        {
            get;
            private set;
        }


        /*
         * negativeCount, positiveCount, totalCount
         */
        public static event Action<int, int, int>
            ActiveStatusCountChanged;


        // =========================================================
        // INICIALIZAR
        // =========================================================

        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }


            initialized = true;
            logger = log;


            /*
             * Ya no observamos aplicaciones históricas.
             *
             * El desafío necesita saber cuántos efectos
             * VÁLIDOS están ACTIVOS AHORA MISMO.
             */
            RoR2Application.onFixedUpdate +=
                RecalculateActiveStatusEffects;


            logger.LogInfo(
                "StatusEffectTracker inicializado en modo " +
                "de estados activos en tiempo real."
            );
        }


        // =========================================================
        // RECALCULAR ESTADO ACTUAL DEL CAMPO
        // =========================================================

        private static void RecalculateActiveStatusEffects()
        {
            if (!NetworkServer.active)
            {
                return;
            }


            /*
             * Fuera de una run no existe progreso activo.
             */
            if (Run.instance == null)
            {
                SetCounts(
                    0,
                    0
                );

                return;
            }


            int negativeCount = 0;
            int positiveCount = 0;


            TeamMask enemyTeams =
                TeamMask.GetEnemyTeams(
                    TeamIndex.Player
                );


            foreach (
                CharacterBody body
                in CharacterBody.readOnlyInstancesList
            )
            {
                if (!IsValidLivingBody(body))
                {
                    continue;
                }


                TeamIndex teamIndex =
                    body.teamComponent.teamIndex;


                // =================================================
                // ENEMIGOS -> SÓLO EFECTOS NEGATIVOS
                // =================================================

                if (
                    enemyTeams.HasTeam(
                        teamIndex
                    )
                )
                {
                    negativeCount +=
                        CountNegativeBuffs(
                            body
                        );


                    negativeCount +=
                        CountActiveDots(
                            body
                        );


                    negativeCount +=
                        CountActiveCrowdControl(
                            body
                        );


                    continue;
                }


                // =================================================
                // ALIADOS -> SÓLO EFECTOS POSITIVOS
                // =================================================

                if (
                    teamIndex ==
                    TeamIndex.Player
                )
                {
                    positiveCount +=
                        CountPositiveBuffs(
                            body
                        );
                }
            }


            SetCounts(
                negativeCount,
                positiveCount
            );
        }


        // =========================================================
        // VALIDAR BODY
        // =========================================================

        private static bool IsValidLivingBody(
            CharacterBody body
        )
        {
            if (
                body == null ||
                body.teamComponent == null ||
                body.healthComponent == null
            )
            {
                return false;
            }


            return body.healthComponent.alive;
        }


        // =========================================================
        // BUFFS NEGATIVOS ACTIVOS EN ENEMIGOS
        // =========================================================

        private static int CountNegativeBuffs(
            CharacterBody body
        )
        {
            int total = 0;


            BuffIndex[] activeBuffs =
                body.activeBuffsList;


            if (activeBuffs == null)
            {
                return total;
            }


            for (
                int i = 0;
                i < activeBuffs.Length;
                i++
            )
            {
                BuffIndex buffIndex =
                    activeBuffs[i];


                if (
                    !StatusEffectScanner.IsNegative(
                        buffIndex
                    )
                )
                {
                    continue;
                }


                BuffDef buffDef =
                    BuffCatalog.GetBuffDef(
                        buffIndex
                    );


                if (buffDef == null)
                {
                    continue;
                }


                int stackCount =
                    body.GetBuffCount(
                        buffDef
                    );


                if (stackCount <= 0)
                {
                    continue;
                }


                /*
                 * Stackeable:
                 * buff x20 -> 20
                 *
                 * No stackeable:
                 * Weak / Entangle -> 1 por body.
                 */
                total +=
                    buffDef.canStack
                        ? stackCount
                        : 1;
            }


            return total;
        }


        // =========================================================
        // BUFFS POSITIVOS ACTIVOS EN ALIADOS
        // =========================================================

        private static int CountPositiveBuffs(
            CharacterBody body
        )
        {
            int total = 0;


            BuffIndex[] activeBuffs =
                body.activeBuffsList;


            if (activeBuffs == null)
            {
                return total;
            }


            for (
                int i = 0;
                i < activeBuffs.Length;
                i++
            )
            {
                BuffIndex buffIndex =
                    activeBuffs[i];


                if (
                    !StatusEffectScanner.IsPositive(
                        buffIndex
                    )
                )
                {
                    continue;
                }


                BuffDef buffDef =
                    BuffCatalog.GetBuffDef(
                        buffIndex
                    );


                if (buffDef == null)
                {
                    continue;
                }


                int stackCount =
                    body.GetBuffCount(
                        buffDef
                    );


                if (stackCount <= 0)
                {
                    continue;
                }


                total +=
                    buffDef.canStack
                        ? stackCount
                        : 1;
            }


            return total;
        }


        // =========================================================
        // DOTS ACTIVOS EN ENEMIGOS
        // =========================================================

        private static int CountActiveDots(
            CharacterBody body
        )
        {
            DotController dotController =
                DotController.FindDotController(
                    body.gameObject
                );


            if (
                dotController == null ||
                dotController.dotStackList == null
            )
            {
                return 0;
            }


            /*
             * Cada DotStack representa una aplicación
             * activa independiente.
             *
             * Ejemplo:
             *
             * Bleed x50       -> 50
             * Hemorrhage x20  -> 20
             * Burn x10        -> 10
             */
            return
                dotController
                    .dotStackList
                    .Count;
        }


        // =========================================================
        // FREEZE / STUN ACTIVOS EN ENEMIGOS
        // =========================================================

        private static int CountActiveCrowdControl(
            CharacterBody body
        )
        {
            EntityStateMachine stateMachine =
                null;


            SetStateOnHurt setStateOnHurt =
                body.GetComponent<SetStateOnHurt>();


            if (
                setStateOnHurt != null &&
                setStateOnHurt.targetStateMachine != null
            )
            {
                stateMachine =
                    setStateOnHurt.targetStateMachine;
            }


            if (stateMachine == null)
            {
                stateMachine =
                    EntityStateMachine.FindByCustomName(
                        body.gameObject,
                        "Body"
                    );
            }


            if (stateMachine == null)
            {
                return 0;
            }


            if (
                stateMachine.state
                is FrozenState
            )
            {
                return 1;
            }


            if (
                stateMachine.state
                is StunState
            )
            {
                return 1;
            }


            return 0;
        }


        // =========================================================
        // ACTUALIZAR TOTAL GLOBAL
        // =========================================================

        private static void SetCounts(
            int negativeCount,
            int positiveCount
        )
        {
            int totalCount =
                negativeCount +
                positiveCount;


            if (
                negativeCount ==
                    CurrentNegativeCount &&
                positiveCount ==
                    CurrentPositiveCount &&
                totalCount ==
                    CurrentTotalCount
            )
            {
                return;
            }


            CurrentNegativeCount =
                negativeCount;


            CurrentPositiveCount =
                positiveCount;


            CurrentTotalCount =
                totalCount;


            logger?.LogInfo(
                $"ESTADOS ACTIVOS | " +
                $"Negativos en enemigos: {CurrentNegativeCount} | " +
                $"Positivos en aliados: {CurrentPositiveCount} | " +
                $"TOTAL: {CurrentTotalCount}"
            );


            ActiveStatusCountChanged?.Invoke(
                CurrentNegativeCount,
                CurrentPositiveCount,
                CurrentTotalCount
            );
        }
    }
}