using System;
using System.Collections.Generic;
using System.Text;
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


        /*
         * VERIFICADOR TEMPORAL
         *
         * true:
         *   imprime qué Buff / DoT / CC está contando.
         *
         * false:
         *   sólo imprime los totales.
         *
         * Por ahora lo dejamos en true.
         */
        private const bool DetailedDiagnostics =
            true;


        /*
         * Se usa para no imprimir exactamente
         * la misma composición una y otra vez.
         */
        private static string lastDiagnosticSignature =
            "";


        // =========================================================
        // CONTADORES ACTUALES
        // =========================================================

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
         * negativeCount,
         * positiveCount,
         * totalCount
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
             * El conteo se recalcula constantemente
             * en el servidor.
             *
             * No acumulamos aplicaciones históricas.
             *
             * Siempre queremos saber:
             *
             * "¿Cuántos efectos válidos están
             * activos AHORA MISMO?"
             */
            RoR2Application.onFixedUpdate +=
                RecalculateActiveStatusEffects;


            logger.LogInfo(
                "StatusEffectTracker inicializado en modo " +
                "de estados activos en tiempo real."
            );


            if (DetailedDiagnostics)
            {
                logger.LogInfo(
                    "Verificador detallado de estados ACTIVADO."
                );
            }
        }


        // =========================================================
        // RECALCULAR ESTADO ACTUAL DEL CAMPO
        // =========================================================

        private static void RecalculateActiveStatusEffects()
        {
            /*
             * El servidor es la autoridad.
             *
             * Esto también es importante para multiplayer:
             * todos los enemigos, jugadores y aliados
             * existen en el estado del servidor.
             */
            if (!NetworkServer.active)
            {
                return;
            }


            /*
             * Si no existe una Run:
             *
             * total = 0
             */
            if (Run.instance == null)
            {
                SetCounts(
                    0,
                    0,
                    new List<string>(),
                    new List<string>()
                );

                return;
            }


            int negativeCount =
                0;


            int positiveCount =
                0;


            /*
             * Información utilizada solamente
             * por el verificador.
             */
            List<string> negativeDetails =
                new List<string>();


            List<string> positiveDetails =
                new List<string>();


            /*
             * Todos los equipos considerados
             * enemigos del Team Player.
             */
            TeamMask enemyTeams =
                TeamMask.GetEnemyTeams(
                    TeamIndex.Player
                );


            /*
             * Recorremos TODOS los CharacterBody
             * vivos de la partida.
             */
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
                // ENEMIGO
                //
                // Sólo efectos NEGATIVOS.
                // =================================================

                if (
                    enemyTeams.HasTeam(
                        teamIndex
                    )
                )
                {
                    /*
                     * BuffDef negativos.
                     *
                     * Ejemplos:
                     *
                     * Weak
                     * Entangle
                     * Tangle
                     * Slow
                     * etc.
                     */
                    negativeCount +=
                        CountNegativeBuffs(
                            body,
                            negativeDetails
                        );


                    /*
                     * DoT activos.
                     *
                     * Ejemplos:
                     *
                     * Bleed
                     * Burn
                     * Poison
                     * Hemorrhage
                     */
                    negativeCount +=
                        CountActiveDots(
                            body,
                            negativeDetails
                        );


                    /*
                     * Estados de Crowd Control.
                     *
                     * Freeze
                     * Stun
                     */
                    negativeCount +=
                        CountActiveCrowdControl(
                            body,
                            negativeDetails
                        );


                    continue;
                }


                // =================================================
                // ALIADO
                //
                // Sólo efectos POSITIVOS.
                // =================================================

                if (
                    teamIndex ==
                    TeamIndex.Player
                )
                {
                    positiveCount +=
                        CountPositiveBuffs(
                            body,
                            positiveDetails
                        );
                }
            }


            /*
             * Ordenamos para que el orden de las entidades
             * no provoque falsos cambios de diagnóstico.
             */
            negativeDetails.Sort(
                StringComparer.Ordinal
            );


            positiveDetails.Sort(
                StringComparer.Ordinal
            );


            SetCounts(
                negativeCount,
                positiveCount,
                negativeDetails,
                positiveDetails
            );
        }


        // =========================================================
        // VALIDAR CHARACTER BODY
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


            /*
             * Si murió:
             *
             * sus buffs / DoT ya no deben
             * contribuir al desafío.
             */
            return
                body.healthComponent.alive;
        }


        // =========================================================
        // BUFFS NEGATIVOS ACTIVOS
        //
        // SOLAMENTE EN ENEMIGOS
        // =========================================================

        private static int CountNegativeBuffs(
            CharacterBody body,
            List<string> details
        )
        {
            int total =
                0;


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


                /*
                 * El StatusEffectScanner decide
                 * qué BuffDef consideramos negativo.
                 */
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
                 * REGLA:
                 *
                 * Si stackea:
                 *
                 * x20 = +20
                 *
                 * Si NO stackea:
                 *
                 * presente = +1
                 */
                int contribution =
                    buffDef.canStack
                        ? stackCount
                        : 1;


                total +=
                    contribution;


                if (details != null)
                {
                    details.Add(
                        $"ENEMIGO: {body.name} | " +
                        $"BUFF NEGATIVO: {buffDef.name} | " +
                        $"Stacks reales: {stackCount} | " +
                        $"Stackeable: {(buffDef.canStack ? "SI" : "NO")} | " +
                        $"Aporta: {contribution}"
                    );
                }
            }


            return total;
        }


        // =========================================================
        // BUFFS POSITIVOS ACTIVOS
        //
        // SOLAMENTE EN ALIADOS
        // =========================================================

        private static int CountPositiveBuffs(
            CharacterBody body,
            List<string> details
        )
        {
            int total =
                0;


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


                /*
                 * El StatusEffectScanner decide
                 * qué BuffDef consideramos positivo.
                 */
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


                /*
                 * Igual que los negativos:
                 *
                 * Buff stackeable x5
                 * = +5
                 *
                 * Buff no stackeable
                 * = +1
                 */
                int contribution =
                    buffDef.canStack
                        ? stackCount
                        : 1;


                total +=
                    contribution;


                if (details != null)
                {
                    details.Add(
                        $"ALIADO: {body.name} | " +
                        $"BUFF POSITIVO: {buffDef.name} | " +
                        $"Stacks reales: {stackCount} | " +
                        $"Stackeable: {(buffDef.canStack ? "SI" : "NO")} | " +
                        $"Aporta: {contribution}"
                    );
                }
            }


            return total;
        }


        // =========================================================
        // DOTS ACTIVOS
        //
        // SOLAMENTE EN ENEMIGOS
        // =========================================================

        private static int CountActiveDots(
            CharacterBody body,
            List<string> details
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
             * Cada DotStack activo representa
             * una instancia real actualmente activa.
             *
             * Ejemplo:
             *
             * Bleed x50 = 50
             */
            int total =
                dotController.dotStackList.Count;


            if (
                total <= 0 ||
                details == null
            )
            {
                return total;
            }


            /*
             * Agrupamos por DotIndex.
             *
             * Así Bleed x50 produce UNA línea
             * en el log y no 50 líneas.
             */
            Dictionary<int, int> dotCounts =
                new Dictionary<int, int>();


            Dictionary<int, string> dotNames =
                new Dictionary<int, string>();


            for (
                int i = 0;
                i < dotController.dotStackList.Count;
                i++
            )
            {
                var dotStack =
                    dotController.dotStackList[i];


                int dotIndexValue =
                    (int)dotStack.dotIndex;


                if (
                    !dotCounts.TryGetValue(
                        dotIndexValue,
                        out int currentCount
                    )
                )
                {
                    currentCount =
                        0;
                }


                dotCounts[
                    dotIndexValue
                ] =
                    currentCount + 1;


                /*
                 * Sólo necesitamos resolver
                 * el nombre una vez.
                 */
                if (
                    !dotNames.ContainsKey(
                        dotIndexValue
                    )
                )
                {
                    var dotDef =
                        DotController.GetDotDef(
                            dotStack.dotIndex
                        );


                    string dotName =
                        $"DotIndex_{dotIndexValue}";


                    if (
                        dotDef != null &&
                        dotDef.associatedBuff != null
                    )
                    {
                        dotName =
                            dotDef.associatedBuff.name;
                    }


                    dotNames[
                        dotIndexValue
                    ] =
                        dotName;
                }
            }


            foreach (
                KeyValuePair<int, int> pair
                in dotCounts
            )
            {
                int dotIndexValue =
                    pair.Key;


                int stackCount =
                    pair.Value;


                string dotName =
                    dotNames.TryGetValue(
                        dotIndexValue,
                        out string storedName
                    )
                        ? storedName
                        : $"DotIndex_{dotIndexValue}";


                details.Add(
                    $"ENEMIGO: {body.name} | " +
                    $"DOT: {dotName} | " +
                    $"DotIndex: {dotIndexValue} | " +
                    $"Stacks activos: {stackCount} | " +
                    $"Aporta: {stackCount}"
                );
            }


            return total;
        }


        // =========================================================
        // FREEZE / STUN
        //
        // SOLAMENTE EN ENEMIGOS
        // =========================================================

        private static int CountActiveCrowdControl(
            CharacterBody body,
            List<string> details
        )
        {
            EntityStateMachine stateMachine =
                null;


            /*
             * Primero intentamos utilizar
             * SetStateOnHurt.
             */
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


            /*
             * Fallback:
             *
             * buscar StateMachine "Body".
             */
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


            // =====================================================
            // FREEZE
            // =====================================================

            if (
                stateMachine.state
                is FrozenState
            )
            {
                if (details != null)
                {
                    details.Add(
                        $"ENEMIGO: {body.name} | " +
                        $"CROWD CONTROL: Freeze | " +
                        $"Aporta: 1"
                    );
                }


                return 1;
            }


            // =====================================================
            // STUN
            // =====================================================

            if (
                stateMachine.state
                is StunState
            )
            {
                if (details != null)
                {
                    details.Add(
                        $"ENEMIGO: {body.name} | " +
                        $"CROWD CONTROL: Stun | " +
                        $"Aporta: 1"
                    );
                }


                return 1;
            }


            return 0;
        }


        // =========================================================
        // ACTUALIZAR TOTAL GLOBAL
        // =========================================================

        private static void SetCounts(
            int negativeCount,
            int positiveCount,
            List<string> negativeDetails,
            List<string> positiveDetails
        )
        {
            int totalCount =
                negativeCount +
                positiveCount;


            /*
             * Creamos una firma de todos los efectos
             * actualmente presentes.
             */
            string diagnosticSignature =
                BuildDiagnosticSignature(
                    negativeDetails,
                    positiveDetails
                );


            bool countsChanged =
                negativeCount != CurrentNegativeCount ||
                positiveCount != CurrentPositiveCount ||
                totalCount != CurrentTotalCount;


            /*
             * Puede cambiar la composición
             * aunque el número sea igual.
             *
             * Ejemplo:
             *
             * Bleed x5 = 5
             *
             * después:
             *
             * Bleed x4
             * Burn x1
             *
             * TOTAL sigue siendo 5.
             *
             * Queremos que el verificador
             * nos muestre ese cambio.
             */
            bool compositionChanged =
                diagnosticSignature !=
                lastDiagnosticSignature;


            /*
             * Si absolutamente nada cambió,
             * no imprimimos nada.
             */
            if (
                !countsChanged &&
                !compositionChanged
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


            lastDiagnosticSignature =
                diagnosticSignature;


            // =====================================================
            // RESUMEN GENERAL
            // =====================================================

            logger?.LogInfo(
                $"ESTADOS ACTIVOS | " +
                $"Negativos en enemigos: {CurrentNegativeCount} | " +
                $"Positivos en aliados: {CurrentPositiveCount} | " +
                $"TOTAL: {CurrentTotalCount}"
            );


            // =====================================================
            // VERIFICADOR
            // =====================================================

            if (DetailedDiagnostics)
            {
                LogDetailedState(
                    negativeDetails,
                    positiveDetails
                );
            }


            /*
             * El Achievement sólo necesita reaccionar
             * cuando cambia el número.
             *
             * Si solamente cambió:
             *
             * Bleed x5
             *
             * por:
             *
             * Burn x5
             *
             * el total sigue siendo 5,
             * por lo que no hace falta disparar
             * el evento otra vez.
             */
            if (countsChanged)
            {
                ActiveStatusCountChanged?.Invoke(
                    CurrentNegativeCount,
                    CurrentPositiveCount,
                    CurrentTotalCount
                );
            }
        }


        // =========================================================
        // FIRMA DEL ESTADO ACTUAL
        // =========================================================

        private static string BuildDiagnosticSignature(
            List<string> negativeDetails,
            List<string> positiveDetails
        )
        {
            StringBuilder builder =
                new StringBuilder();


            if (negativeDetails != null)
            {
                for (
                    int i = 0;
                    i < negativeDetails.Count;
                    i++
                )
                {
                    builder.Append(
                        "N|"
                    );


                    builder.AppendLine(
                        negativeDetails[i]
                    );
                }
            }


            if (positiveDetails != null)
            {
                for (
                    int i = 0;
                    i < positiveDetails.Count;
                    i++
                )
                {
                    builder.Append(
                        "P|"
                    );


                    builder.AppendLine(
                        positiveDetails[i]
                    );
                }
            }


            return
                builder.ToString();
        }


        // =========================================================
        // IMPRIMIR VERIFICADOR DETALLADO
        // =========================================================

        private static void LogDetailedState(
            List<string> negativeDetails,
            List<string> positiveDetails
        )
        {
            logger?.LogInfo(
                "========== VERIFICADOR DE ESTADOS ACTIVOS =========="
            );


            // =====================================================
            // NEGATIVOS
            // =====================================================

            logger?.LogInfo(
                "--- NEGATIVOS VÁLIDOS SOBRE ENEMIGOS ---"
            );


            if (
                negativeDetails == null ||
                negativeDetails.Count == 0
            )
            {
                logger?.LogInfo(
                    "Ninguno."
                );
            }
            else
            {
                for (
                    int i = 0;
                    i < negativeDetails.Count;
                    i++
                )
                {
                    logger?.LogInfo(
                        negativeDetails[i]
                    );
                }
            }


            // =====================================================
            // POSITIVOS
            // =====================================================

            logger?.LogInfo(
                "--- POSITIVOS VÁLIDOS SOBRE ALIADOS ---"
            );


            if (
                positiveDetails == null ||
                positiveDetails.Count == 0
            )
            {
                logger?.LogInfo(
                    "Ninguno."
                );
            }
            else
            {
                for (
                    int i = 0;
                    i < positiveDetails.Count;
                    i++
                )
                {
                    logger?.LogInfo(
                        positiveDetails[i]
                    );
                }
            }


            // =====================================================
            // RESUMEN
            // =====================================================

            logger?.LogInfo(
                $"RESUMEN VERIFICADOR | " +
                $"Negativos: {CurrentNegativeCount} | " +
                $"Positivos: {CurrentPositiveCount} | " +
                $"TOTAL: {CurrentTotalCount}"
            );


            logger?.LogInfo(
                "====================================================="
            );
        }
    }
}