using System;
using System.Collections.Generic;
using BepInEx.Logging;
using EntityStates.Bandit2.Weapon;
using EntityStates.Railgunner.Weapon;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class PrecisionExecutionStreakTracker
    {
        private static ManualLogSource logger;
        private static bool initialized;


        // =========================================================
        // TIPO DE DISPARO QUE ESTAMOS OBSERVANDO
        // =========================================================

        private enum TrackedShotType
        {
            RailgunnerM99,
            BanditLightsOut
        }


        // =========================================================
        // CONTEXTO DE UN DISPARO
        // =========================================================

        private sealed class ShotContext
        {
            public CharacterMaster PlayerMaster;

            public TrackedShotType ShotType;

            public bool Success;
        }


        // =========================================================
        // DISPAROS REGISTRADOS
        // =========================================================

        private static readonly Dictionary<
            BulletAttack,
            ShotContext
        > BulletContexts =
            new Dictionary<BulletAttack, ShotContext>();


        /*
         * BulletAttack.Fire() es síncrono.
         *
         * Mientras un disparo registrado se está procesando,
         * guardamos su contexto aquí.
         */
        private static readonly Stack<ShotContext>
            ActiveShotContexts =
                new Stack<ShotContext>();


        // =========================================================
        // CONTADORES INDIVIDUALES
        // =========================================================

        private static readonly Dictionary<
            CharacterMaster,
            int
        > RailgunnerStreaks =
            new Dictionary<CharacterMaster, int>();


        private static readonly Dictionary<
            CharacterMaster,
            int
        > BanditStreaks =
            new Dictionary<CharacterMaster, int>();


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


            // M99 de Railgunner.
            On.EntityStates.Railgunner.Weapon
                .FireSnipeHeavy
                .ModifyBullet +=
                OnRailgunnerModifyBullet;


            // Lights Out de Bandit.
            On.EntityStates.Bandit2.Weapon
                .FireSidearmResetRevolver
                .ModifyBullet +=
                OnLightsOutModifyBullet;


            // Nos permite saber cuándo terminó
            // realmente cada BulletAttack.
            On.RoR2.BulletAttack.Fire +=
                OnBulletAttackFire;


            // Usado para detectar que Lights Out
            // fue quien produjo una muerte.
            GlobalEventManager
                .onCharacterDeathGlobal +=
                OnCharacterDeathGlobal;


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
            orig(
                self,
                bulletAttack
            );


            if (!NetworkServer.active)
            {
                return;
            }


            if (
                self == null ||
                self.characterBody == null ||
                bulletAttack == null
            )
            {
                return;
            }


            CharacterMaster playerMaster =
                self.characterBody.master;


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
            // CREAR CONTEXTO
            // =====================================================

            ShotContext context =
                new ShotContext
                {
                    PlayerMaster =
                        playerMaster,

                    ShotType =
                        TrackedShotType
                            .RailgunnerM99,

                    Success =
                        false
                };


            BulletContexts[
                bulletAttack
            ] =
                context;


            // =====================================================
            // ENVOLVER CALLBACK DEL DISPARO
            // =====================================================
            //
            // Esto NO reemplaza la lógica vanilla.
            //
            // Primero comprobamos si el HurtBox era
            // realmente un sniper weak point y luego
            // dejamos que el callback original continúe.
            // =====================================================

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
            orig(
                self,
                bulletAttack
            );


            if (!NetworkServer.active)
            {
                return;
            }


            if (
                self == null ||
                self.characterBody == null ||
                bulletAttack == null
            )
            {
                return;
            }


            CharacterMaster playerMaster =
                self.characterBody.master;


            if (
                playerMaster == null ||
                !PlayerOwnerResolver.IsPlayerMaster(
                    playerMaster
                )
            )
            {
                return;
            }


            ShotContext context =
                new ShotContext
                {
                    PlayerMaster =
                        playerMaster,

                    ShotType =
                        TrackedShotType
                            .BanditLightsOut,

                    Success =
                        false
                };


            BulletContexts[
                bulletAttack
            ] =
                context;
        }


        // =========================================================
        // BULLET ATTACK
        // =========================================================

        private static void OnBulletAttackFire(
            On.RoR2.BulletAttack.orig_Fire orig,
            BulletAttack self
        )
        {
            if (
                !NetworkServer.active ||
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


            ActiveShotContexts.Push(
                context
            );


            try
            {
                /*
                 * Dentro de esta llamada:
                 *
                 * - Railgunner puede golpear Weak Points.
                 * - Lights Out puede matar enemigos.
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
        // MUERTE DURANTE LIGHTS OUT
        // =========================================================

        private static void OnCharacterDeathGlobal(
            DamageReport damageReport
        )
        {
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
                context.ShotType !=
                TrackedShotType.BanditLightsOut
            )
            {
                return;
            }


            /*
             * El atacante del DamageReport debe ser
             * exactamente el Bandit que realizó
             * este Lights Out.
             */
            if (
                damageReport.attackerMaster !=
                context.PlayerMaster
            )
            {
                return;
            }


            // =====================================================
            // DEBE SER EL DAÑO DE LA HABILIDAD SPECIAL
            // =====================================================
            //
            // Evitamos que un proc de item provocado durante
            // el mismo disparo sea interpretado como la baja
            // directa de Lights Out.
            // =====================================================

            int damageSource =
                (int)damageReport
                    .damageInfo
                    .damageType
                    .damageSource;


            const int SpecialDamageSource =
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
        // RESULTADO RAILGUNNER
        // =========================================================

        private static void ResolveRailgunnerShot(
            ShotContext context
        )
        {
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
        // RESULTADO LIGHTS OUT
        // =========================================================

        private static void ResolveLightsOutShot(
            ShotContext context
        )
        {
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
        // OBTENER PROGRESO
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