using System;
using System.Collections.Generic;

using BepInEx.Logging;

using EntityStates.Bandit2.Weapon;
using EntityStates.Railgunner.Weapon;

using R2API.Networking;
using R2API.Networking.Interfaces;

using RoR2;

using UnityEngine;
using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    public static class PrecisionExecutionStreakTracker
    {
        private static ManualLogSource logger;

        private static bool initialized;


        // =========================================================
        // TIPOS DE DISPARO
        // =========================================================

        private enum TrackedShotType
        {
            RailgunnerM99,

            BanditLightsOut
        }


        // =========================================================
        // CONTEXTO DE DISPARO
        // =========================================================

        private sealed class ShotContext
        {
            public CharacterMaster PlayerMaster;

            public GameObject BodyObject;

            public TrackedShotType ShotType;

            public bool Success;

            /*
             * false:
             * el disparo está siendo procesado por el servidor/host.
             *
             * true:
             * el disparo pertenece a un cliente remoto y,
             * cuando termine BulletAttack.Fire(), debemos enviar
             * el resultado al servidor.
             *
             * Actualmente esto se usa para Railgunner.
             */
            public bool SendResultToServer;
        }


        // =========================================================
        // BANDIT CLIENTE
        // =========================================================

        /*
         * A diferencia de Railgunner, no necesitamos determinar
         * directamente qué enemigo murió.
         *
         * Lights Out tiene una propiedad muy útil:
         *
         *     una baja restablece los cooldowns.
         *
         * Por tanto:
         *
         * 1. El cliente dispara Lights Out.
         * 2. Guardamos cuánto stock quedó después del disparo.
         * 3. Esperamos una ventana corta.
         * 4. Si el stock aumenta antes del cooldown normal,
         *    sabemos que hubo un reset.
         * 5. Mandamos true/false al host.
         *
         * Esto evita que el host tenga que intentar emparejar
         * un mensaje de red con un DamageReport.
         */

        private sealed class PendingBanditClientCheck
        {
            public CharacterMaster PlayerMaster;

            public GameObject BodyObject;

            public GenericSkill SpecialSkill;

            public int StockAfterShot;

            public float Deadline;
        }


        /*
         * Sólo puede existir normalmente un Lights Out pendiente
         * por jugador.
         */
        private static readonly Dictionary<
            CharacterMaster,
            PendingBanditClientCheck
        > PendingBanditClientChecks =
            new Dictionary<
                CharacterMaster,
                PendingBanditClientCheck
            >();


        /*
         * Lights Out tiene un cooldown bastante superior
         * a esta ventana.
         *
         * Por ello, si el stock vuelve antes de 1.25 segundos,
         * interpretamos que el juego restableció el cooldown
         * debido a una baja.
         */
        private const float
            BanditClientConfirmWindow =
                1.25f;


        // =========================================================
        // BULLET CONTEXTS
        // =========================================================

        /*
         * Relaciona un BulletAttack concreto con el contexto
         * de HUNK correspondiente.
         */
        private static readonly Dictionary<
            BulletAttack,
            ShotContext
        > BulletContexts =
            new Dictionary<
                BulletAttack,
                ShotContext
            >();


        /*
         * BulletAttack.Fire() es síncrono.
         *
         * Mientras un disparo del host/servidor se está ejecutando,
         * colocamos aquí su contexto.
         *
         * Esto permite que OnCharacterDeathGlobal sepa que una
         * muerte ocurrió exactamente durante ese Lights Out.
         */
        private static readonly Stack<
            ShotContext
        > ActiveShotContexts =
            new Stack<
                ShotContext
            >();


        // =========================================================
        // RACHAS INDIVIDUALES
        // =========================================================

        /*
         * IMPORTANTE:
         *
         * Las rachas NO se comparten entre jugadores.
         *
         * Cada CharacterMaster tiene su propia racha.
         *
         * Ejemplo:
         *
         * Bandit A = 12
         * Bandit B = 12
         *
         * NO significa 24.
         */

        private static readonly Dictionary<
            CharacterMaster,
            int
        > RailgunnerStreaks =
            new Dictionary<
                CharacterMaster,
                int
            >();


        private static readonly Dictionary<
            CharacterMaster,
            int
        > BanditStreaks =
            new Dictionary<
                CharacterMaster,
                int
            >();


        // =========================================================
        // EVENTOS
        // =========================================================

        public static event Action<
            CharacterMaster,
            int
        > RailgunnerWeakPointStreakChanged;


        public static event Action<
            CharacterMaster,
            int
        > BanditLightsOutStreakChanged;


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


            // -----------------------------------------------------
            // RAILGUNNER — M99
            // -----------------------------------------------------

            On.EntityStates.Railgunner.Weapon
                .FireSnipeHeavy
                .ModifyBullet +=
                OnRailgunnerModifyBullet;


            // -----------------------------------------------------
            // BANDIT — LIGHTS OUT
            // -----------------------------------------------------

            On.EntityStates.Bandit2.Weapon
                .FireSidearmResetRevolver
                .ModifyBullet +=
                OnLightsOutModifyBullet;


            // -----------------------------------------------------
            // BULLET ATTACK
            // -----------------------------------------------------

            On.RoR2.BulletAttack.Fire +=
                OnBulletAttackFire;


            // -----------------------------------------------------
            // MUERTES
            //
            // Sólo sigue siendo necesario para Bandit cuando
            // el disparo está siendo ejecutado en servidor/host.
            // -----------------------------------------------------

            GlobalEventManager
                .onCharacterDeathGlobal +=
                OnCharacterDeathGlobal;


            // -----------------------------------------------------
            // BANDIT CLIENTE
            //
            // Aquí observamos si Lights Out recuperó su stock.
            // -----------------------------------------------------

            RoR2Application.onFixedUpdate +=
                OnBanditClientFixedUpdate;


            // -----------------------------------------------------
            // RUN
            // -----------------------------------------------------

            Run.onRunStartGlobal +=
                OnRunStart;


            Run.onRunDestroyGlobal +=
                OnRunEnd;


            logger?.LogInfo(
                "PrecisionExecutionStreakTracker inicializado."
            );
        }


        // =========================================================
        // RUN
        // =========================================================

        private static void OnRunStart(
            Run run
        )
        {
            ResetAll();
        }


        private static void OnRunEnd(
            Run run
        )
        {
            ResetAll();
        }


        private static void ResetAll()
        {
            BulletContexts.Clear();

            ActiveShotContexts.Clear();

            PendingBanditClientChecks.Clear();

            RailgunnerStreaks.Clear();

            BanditStreaks.Clear();
        }


        // =========================================================
        // RAILGUNNER — M99
        // =========================================================

        private static void OnRailgunnerModifyBullet(
            On.EntityStates.Railgunner.Weapon
                .FireSnipeHeavy
                .orig_ModifyBullet orig,

            FireSnipeHeavy self,

            BulletAttack bulletAttack
        )
        {
            if (!MissionRuntimeActivityPlan.IsTypeActive("PrecisionExecutionStreak"))
            {
                orig(self, bulletAttack);
                return;
            }

            /*
             * Primero dejamos que la habilidad vanilla configure
             * normalmente el BulletAttack.
             */
            orig(
                self,
                bulletAttack
            );


            if (
                self == null ||
                self.characterBody == null ||
                bulletAttack == null
            )
            {
                return;
            }


            CharacterBody body =
                self.characterBody;


            CharacterMaster playerMaster =
                body.master;


            if (
                playerMaster == null ||
                !PlayerOwnerResolver.IsPlayerMaster(
                    playerMaster
                )
            )
            {
                return;
            }


            // =====================================================
            // HOST / SERVIDOR
            // =====================================================

            if (NetworkServer.active)
            {
                CreateRailgunnerContext(
                    body,
                    bulletAttack,
                    false
                );


                return;
            }


            // =====================================================
            // CLIENTE REMOTO
            // =====================================================

            /*
             * Sólo el cliente que realmente controla este cuerpo
             * debe informar su resultado.
             */
            if (
                !NetworkClient.active ||
                !Util.HasEffectiveAuthority(
                    body.gameObject
                )
            )
            {
                return;
            }


            CreateRailgunnerContext(
                body,
                bulletAttack,
                true
            );
        }


        // =========================================================
        // CREAR CONTEXTO RAILGUNNER
        // =========================================================

        private static void CreateRailgunnerContext(
            CharacterBody body,
            BulletAttack bulletAttack,
            bool sendResultToServer
        )
        {
            if (
                body == null ||
                body.master == null ||
                bulletAttack == null
            )
            {
                return;
            }


            ShotContext context =
                new ShotContext
                {
                    PlayerMaster =
                        body.master,

                    BodyObject =
                        body.gameObject,

                    ShotType =
                        TrackedShotType
                            .RailgunnerM99,

                    Success =
                        false,

                    SendResultToServer =
                        sendResultToServer
                };


            BulletContexts[
                bulletAttack
            ] =
                context;


            /*
             * Guardamos el callback original.
             *
             * No queremos reemplazar comportamiento vanilla.
             */
            var originalCallback =
                bulletAttack.hitCallback
                ??
                BulletAttack.defaultHitCallback;


            bulletAttack.hitCallback =
                delegate (
                    BulletAttack attack,
                    ref BulletAttack.BulletHit hitInfo
                )
                {
                    /*
                     * El propio juego sabe determinar
                     * si el HurtBox golpeado es un Weak Point
                     * válido para Railgunner.
                     */
                    if (
                        BulletAttack.IsSniperTargetHit(
                            in hitInfo
                        )
                    )
                    {
                        context.Success =
                            true;
                    }


                    return originalCallback(
                        attack,
                        ref hitInfo
                    );
                };
        }


        // =========================================================
        // BANDIT — LIGHTS OUT
        // =========================================================

        private static void OnLightsOutModifyBullet(
            On.EntityStates.Bandit2.Weapon
                .FireSidearmResetRevolver
                .orig_ModifyBullet orig,

            FireSidearmResetRevolver self,

            BulletAttack bulletAttack
        )
        {
            if (!MissionRuntimeActivityPlan.IsTypeActive("PrecisionExecutionStreak"))
            {
                orig(self, bulletAttack);
                return;
            }

            /*
             * Conservamos completamente la configuración vanilla.
             */
            orig(
                self,
                bulletAttack
            );


            if (
                self == null ||
                self.characterBody == null ||
                bulletAttack == null
            )
            {
                return;
            }


            CharacterBody body =
                self.characterBody;


            CharacterMaster playerMaster =
                body.master;


            if (
                playerMaster == null ||
                !PlayerOwnerResolver.IsPlayerMaster(
                    playerMaster
                )
            )
            {
                return;
            }


            // =====================================================
            // HOST / SERVIDOR
            // =====================================================

            /*
             * El host puede seguir usando el sistema síncrono
             * original.
             *
             * En ese caso:
             *
             * BulletAttack.Fire()
             *      ↓
             * OnCharacterDeathGlobal
             *      ↓
             * context.Success = true
             */
            if (NetworkServer.active)
            {
                ShotContext context =
                    new ShotContext
                    {
                        PlayerMaster =
                            playerMaster,

                        BodyObject =
                            body.gameObject,

                        ShotType =
                            TrackedShotType
                                .BanditLightsOut,

                        Success =
                            false,

                        SendResultToServer =
                            false
                    };


                BulletContexts[
                    bulletAttack
                ] =
                    context;


                return;
            }


            // =====================================================
            // CLIENTE REMOTO
            // =====================================================

            if (
                !NetworkClient.active ||
                !Util.HasEffectiveAuthority(
                    body.gameObject
                )
            )
            {
                return;
            }


            /*
             * IMPORTANTE:
             *
             * Ya NO enviamos:
             *
             *     "disparé Lights Out"
             *
             * al servidor.
             *
             * Ahora esperamos localmente el resultado y luego
             * enviamos:
             *
             *     true  = hubo reset / baja
             *     false = no hubo reset
             */
            BeginBanditClientCheck(
                body
            );
        }


        // =========================================================
        // BANDIT CLIENTE — INICIAR COMPROBACIÓN
        // =========================================================

        private static void BeginBanditClientCheck(
            CharacterBody body
        )
        {
            if (
                body == null ||
                body.master == null ||
                body.skillLocator == null ||
                body.skillLocator.special == null
            )
            {
                return;
            }


            CharacterMaster master =
                body.master;


            GenericSkill specialSkill =
                body.skillLocator.special;


            /*
             * Normalmente el FixedUpdate resolverá el disparo
             * antes de que podamos usar Lights Out nuevamente.
             *
             * Si por alguna razón todavía existe uno pendiente,
             * lo retiramos para evitar que dos comprobaciones
             * diferentes manipulen la misma racha.
             */
            if (
                PendingBanditClientChecks.ContainsKey(
                    master
                )
            )
            {
                logger?.LogWarning(
                    "[HUNK] Bandit cliente | " +
                    "Existía una comprobación anterior pendiente. " +
                    "Será reemplazada."
                );


                PendingBanditClientChecks.Remove(
                    master
                );
            }


            PendingBanditClientCheck check =
                new PendingBanditClientCheck
                {
                    PlayerMaster =
                        master,

                    BodyObject =
                        body.gameObject,

                    SpecialSkill =
                        specialSkill,

                    /*
                     * Esto es MUY importante.
                     *
                     * No comprobamos simplemente:
                     *
                     *     stock > 0
                     *
                     * porque podrían existir efectos que aumenten
                     * el máximo de cargas de la habilidad.
                     *
                     * Guardamos exactamente el stock que quedó
                     * después de disparar.
                     *
                     * Si después aumenta:
                     *
                     *     hubo un reset.
                     */
                    StockAfterShot =
                        specialSkill.stock,

                    Deadline =
                        Time.fixedTime +
                        BanditClientConfirmWindow
                };


            PendingBanditClientChecks[
                master
            ] =
                check;


            logger?.LogInfo(
                "[HUNK] Bandit cliente | " +
                "Lights Out disparado | " +
                $"Stock después del disparo: " +
                $"{check.StockAfterShot} | " +
                "Esperando reset local."
            );
        }


        // =========================================================
        // BANDIT CLIENTE — OBSERVAR RESET
        // =========================================================

        private static void OnBanditClientFixedUpdate()
        {
            if (!MissionRuntimeActivityPlan.IsTypeActive("PrecisionExecutionStreak"))
            {
                return;
            }

            /*
             * El host no necesita esta lógica.
             *
             * Sólo queremos ejecutarla en clientes remotos.
             */
            if (
                NetworkServer.active ||
                !NetworkClient.active ||
                PendingBanditClientChecks.Count <= 0
            )
            {
                return;
            }


            /*
             * Copiamos las claves porque vamos a retirar
             * elementos del Dictionary durante el recorrido.
             */
            List<CharacterMaster> masters =
                new List<CharacterMaster>(
                    PendingBanditClientChecks.Keys
                );


            foreach (
                CharacterMaster master
                in masters
            )
            {
                if (
                    !PendingBanditClientChecks
                        .TryGetValue(
                            master,
                            out PendingBanditClientCheck check
                        )
                )
                {
                    continue;
                }


                if (
                    check == null ||
                    check.PlayerMaster == null ||
                    check.BodyObject == null ||
                    check.SpecialSkill == null
                )
                {
                    PendingBanditClientChecks.Remove(
                        master
                    );


                    continue;
                }


                /*
                 * Si dejamos de controlar ese cuerpo,
                 * no debemos mandar resultados en su nombre.
                 */
                if (
                    !Util.HasEffectiveAuthority(
                        check.BodyObject
                    )
                )
                {
                    PendingBanditClientChecks.Remove(
                        master
                    );


                    continue;
                }


                int currentStock =
                    check.SpecialSkill.stock;


                // =================================================
                // ÉXITO
                // =================================================

                /*
                 * Lights Out había dejado el Special con X stock.
                 *
                 * Si repentinamente tenemos más stock antes
                 * del cooldown normal:
                 *
                 *     Lights Out provocó un reset.
                 */
                if (
                    currentStock >
                    check.StockAfterShot
                )
                {
                    SendBanditClientResult(
                        check,
                        true
                    );


                    PendingBanditClientChecks.Remove(
                        master
                    );


                    continue;
                }


                // =================================================
                // FALLO
                // =================================================

                /*
                 * Si llegamos al límite sin recuperar stock,
                 * interpretamos que Lights Out no consiguió
                 * la baja.
                 */
                if (
                    Time.fixedTime >=
                    check.Deadline
                )
                {
                    SendBanditClientResult(
                        check,
                        false
                    );


                    PendingBanditClientChecks.Remove(
                        master
                    );
                }
            }
        }


        // =========================================================
        // BANDIT CLIENTE — ENVIAR RESULTADO
        // =========================================================

        private static void SendBanditClientResult(
            PendingBanditClientCheck check,
            bool success
        )
        {
            if (
                check == null ||
                check.BodyObject == null
            )
            {
                return;
            }


            int currentStock =
                check.SpecialSkill != null
                    ? check.SpecialSkill.stock
                    : -1;


            logger?.LogInfo(
                "[HUNK] Bandit cliente | " +
                $"Resultado local: {success} | " +
                $"Stock inicial: {check.StockAfterShot} | " +
                $"Stock actual: {currentStock}"
            );


            new HunkBanditShotResultMessage(
                check.BodyObject,
                success
            )
            .Send(
                NetworkDestination.Server
            );
        }


        // =========================================================
        // BULLET ATTACK
        // =========================================================

        private static void OnBulletAttackFire(
            On.RoR2.BulletAttack.orig_Fire orig,
            BulletAttack self
        )
        {
            if (!MissionRuntimeActivityPlan.IsTypeActive("PrecisionExecutionStreak"))
            {
                orig(self);
                return;
            }

            if (
                self == null ||
                !BulletContexts.TryGetValue(
                    self,
                    out ShotContext context
                )
            )
            {
                orig(
                    self
                );


                return;
            }


            // =====================================================
            // CLIENTE REMOTO — RAILGUNNER
            // =====================================================

            if (context.SendResultToServer)
            {
                try
                {
                    /*
                     * Durante Fire() se ejecutará nuestro
                     * hitCallback y se decidirá si hubo Weak Point.
                     */
                    orig(
                        self
                    );
                }
                finally
                {
                    BulletContexts.Remove(
                        self
                    );
                }


                if (
                    context.ShotType ==
                    TrackedShotType.RailgunnerM99
                )
                {
                    logger?.LogInfo(
                        "[HUNK] Railgunner cliente | " +
                        $"Weak Point enviado al servidor: " +
                        $"{context.Success}"
                    );


                    new HunkRailgunnerShotResultMessage(
                        context.BodyObject,
                        context.Success
                    )
                    .Send(
                        NetworkDestination.Server
                    );
                }


                return;
            }


            // =====================================================
            // SERVIDOR / HOST
            // =====================================================

            if (!NetworkServer.active)
            {
                BulletContexts.Remove(
                    self
                );


                orig(
                    self
                );


                return;
            }


            ActiveShotContexts.Push(
                context
            );


            try
            {
                /*
                 * Dentro de esta llamada:
                 *
                 * Railgunner:
                 *     nuestro hitCallback puede marcar
                 *     Weak Point.
                 *
                 * Bandit:
                 *     OnCharacterDeathGlobal puede detectar
                 *     una muerte producida durante Lights Out.
                 */
                orig(
                    self
                );
            }
            finally
            {
                if (
                    ActiveShotContexts.Count >
                    0
                )
                {
                    ActiveShotContexts.Pop();
                }


                BulletContexts.Remove(
                    self
                );
            }


            ResolveShotResult(
                context
            );
        }


        // =========================================================
        // BANDIT HOST — MUERTE DURANTE LIGHTS OUT
        // =========================================================

        private static void OnCharacterDeathGlobal(
            DamageReport damageReport
        )
        {
            if (!MissionRuntimeActivityPlan.IsTypeActive("PrecisionExecutionStreak"))
            {
                return;
            }

            if (!NetworkServer.active)
            {
                return;
            }


            if (
                damageReport == null ||
                ActiveShotContexts.Count <= 0
            )
            {
                return;
            }


            ShotContext context =
                ActiveShotContexts.Peek();


            if (
                context == null ||
                context.ShotType !=
                TrackedShotType.BanditLightsOut
            )
            {
                return;
            }


            /*
             * La muerte debe pertenecer exactamente
             * al CharacterMaster que disparó este Lights Out.
             */
            if (
                damageReport.attackerMaster !=
                context.PlayerMaster
            )
            {
                return;
            }


            /*
             * Además, exigimos que el golpe mortal venga
             * directamente de la habilidad Special.
             *
             * Esto evita contar:
             *
             * - proc de objetos;
             * - fuego;
             * - efectos secundarios;
             * - otras habilidades.
             */
            int damageSource =
                (int)
                damageReport
                    .damageInfo
                    .damageType
                    .damageSource;


            const int
                SpecialDamageSource =
                    8;


            if (
                (
                    damageSource &
                    SpecialDamageSource
                )
                ==
                0
            )
            {
                return;
            }


            context.Success =
                true;
        }


        // =========================================================
        // RESULTADO REMOTO — RAILGUNNER
        // =========================================================

        /*
         * Este método es llamado por:
         *
         * HunkRailgunnerShotResultMessage.OnReceived()
         *
         * en el servidor.
         */
        public static void RegisterRemoteRailgunnerShot(
            GameObject bodyObject,
            bool success
        )
        {
            if (
                !NetworkServer.active ||
                bodyObject == null
            )
            {
                return;
            }


            CharacterBody body =
                bodyObject.GetComponent<
                    CharacterBody
                >();


            if (
                body == null ||
                body.master == null
            )
            {
                return;
            }


            CharacterMaster master =
                body.master;


            if (
                !PlayerOwnerResolver.IsPlayerMaster(
                    master
                )
            )
            {
                return;
            }


            logger?.LogInfo(
                "[HUNK] Railgunner remoto | " +
                $"Weak Point recibido: {success}"
            );


            ShotContext context =
                new ShotContext
                {
                    PlayerMaster =
                        master,

                    BodyObject =
                        bodyObject,

                    ShotType =
                        TrackedShotType
                            .RailgunnerM99,

                    Success =
                        success,

                    SendResultToServer =
                        false
                };


            ResolveRailgunnerShot(
                context
            );
        }


        // =========================================================
        // RESULTADO REMOTO — BANDIT
        // =========================================================

        /*
         * Este es el cambio principal.
         *
         * El servidor YA NO recibe:
         *
         *     "Bandit disparó Lights Out"
         *
         * y espera una muerte.
         *
         * Ahora recibe directamente:
         *
         *     Success = true / false
         *
         * igual que Railgunner.
         */
        public static void RegisterRemoteBanditShotResult(
            GameObject bodyObject,
            bool success
        )
        {
            if (
                !NetworkServer.active ||
                bodyObject == null
            )
            {
                return;
            }


            CharacterBody body =
                bodyObject.GetComponent<
                    CharacterBody
                >();


            if (
                body == null ||
                body.master == null
            )
            {
                return;
            }


            CharacterMaster master =
                body.master;


            if (
                !PlayerOwnerResolver.IsPlayerMaster(
                    master
                )
            )
            {
                return;
            }


            logger?.LogInfo(
                "[HUNK] Bandit remoto | " +
                "Resultado recibido por servidor | " +
                $"Baja: {success}"
            );


            ShotContext context =
                new ShotContext
                {
                    PlayerMaster =
                        master,

                    BodyObject =
                        bodyObject,

                    ShotType =
                        TrackedShotType
                            .BanditLightsOut,

                    Success =
                        success,

                    SendResultToServer =
                        false
                };


            ResolveLightsOutShot(
                context
            );
        }


        // =========================================================
        // RESOLVER RESULTADO
        // =========================================================

        private static void ResolveShotResult(
            ShotContext context
        )
        {
            if (
                context == null ||
                context.PlayerMaster == null
            )
            {
                return;
            }


            switch (
                context.ShotType
            )
            {
                case TrackedShotType
                    .RailgunnerM99:


                    ResolveRailgunnerShot(
                        context
                    );


                    break;


                case TrackedShotType
                    .BanditLightsOut:


                    ResolveLightsOutShot(
                        context
                    );


                    break;
            }
        }


        // =========================================================
        // RAILGUNNER — RESOLVER RACHA
        // =========================================================

        private static void ResolveRailgunnerShot(
            ShotContext context
        )
        {
            if (
                context == null ||
                context.PlayerMaster == null
            )
            {
                return;
            }


            CharacterMaster master =
                context.PlayerMaster;


            int streak =
                GetDictionaryValue(
                    RailgunnerStreaks,
                    master
                );


            if (context.Success)
            {
                streak++;
            }
            else
            {
                streak =
                    0;
            }


            RailgunnerStreaks[
                master
            ] =
                streak;


            logger?.LogInfo(
                "[HUNK] Railgunner M99 | " +
                $"Weak Point: {context.Success} | " +
                $"Racha: {streak}"
            );


            RailgunnerWeakPointStreakChanged
                ?.Invoke(
                    master,
                    streak
                );
        }


        // =========================================================
        // BANDIT — RESOLVER RACHA
        // =========================================================

        private static void ResolveLightsOutShot(
            ShotContext context
        )
        {
            if (
                context == null ||
                context.PlayerMaster == null
            )
            {
                return;
            }


            CharacterMaster master =
                context.PlayerMaster;


            int streak =
                GetDictionaryValue(
                    BanditStreaks,
                    master
                );


            if (context.Success)
            {
                streak++;
            }
            else
            {
                streak =
                    0;
            }


            BanditStreaks[
                master
            ] =
                streak;


            logger?.LogInfo(
                "[HUNK] Bandit Lights Out | " +
                $"Baja: {context.Success} | " +
                $"Racha: {streak}"
            );


            BanditLightsOutStreakChanged
                ?.Invoke(
                    master,
                    streak
                );
        }


        // =========================================================
        // PROGRESO
        // =========================================================

        public static int GetHighestRailgunnerStreak()
        {
            return GetHighestValue(
                RailgunnerStreaks
            );
        }


        public static int GetHighestBanditStreak()
        {
            return GetHighestValue(
                BanditStreaks
            );
        }


        // =========================================================
        // HELPERS
        // =========================================================

        private static int GetDictionaryValue(
            Dictionary<
                CharacterMaster,
                int
            > dictionary,

            CharacterMaster master
        )
        {
            if (
                dictionary.TryGetValue(
                    master,
                    out int value
                )
            )
            {
                return value;
            }


            return 0;
        }


        private static int GetHighestValue(
            Dictionary<
                CharacterMaster,
                int
            > dictionary
        )
        {
            int highest =
                0;


            foreach (
                KeyValuePair<
                    CharacterMaster,
                    int
                > pair
                in dictionary
            )
            {
                if (
                    pair.Value >
                    highest
                )
                {
                    highest =
                        pair.Value;
                }
            }


            return highest;
        }
    }
}