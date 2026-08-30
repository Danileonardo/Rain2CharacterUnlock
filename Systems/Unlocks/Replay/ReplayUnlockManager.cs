using System;
using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;


namespace UniversalSurvivorUnlocks
{
    public static class ReplayUnlockManager
    {
        private static ManualLogSource logger;


        private static bool initialized;


        // =========================================================
        // UNLOCKS QUE HAN SIDO VUELTOS A BLOQUEAR
        // =========================================================
        //
        // Este estado NO reemplaza al Unlockable real.
        //
        // El Unlockable del UserProfile sigue siendo quien decide
        // si el survivor aparece bloqueado o desbloqueado.
        //
        // Esta lista únicamente nos dice:
        //
        // "Este personaje fue revocado durante esta ejecución
        //  del juego, por lo que debemos permitir que su misión
        //  pueda completarse nuevamente."
        //
        // =========================================================

        private static readonly HashSet<string>
            ReplayArmedBodies =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


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
            // BLOQUEAR / REVOCAR
            // -----------------------------------------------------

            On.RoR2.UserProfile
                .RevokeUnlockable +=
                UserProfile_RevokeUnlockable;


            // -----------------------------------------------------
            // DESBLOQUEAR / CONCEDER
            // -----------------------------------------------------

            On.RoR2.UserProfile
                .GrantUnlockable +=
                UserProfile_GrantUnlockable;


            logger?.LogInfo(
                "ReplayUnlockManager inicializado."
            );
        }


        // =========================================================
        // UNLOCK REVOCADO
        // =========================================================

        private static void UserProfile_RevokeUnlockable(
            On.RoR2.UserProfile
                .orig_RevokeUnlockable orig,

            UserProfile self,

            UnlockableDef unlockableDef
        )
        {
            /*
             * Primero dejamos que Risk of Rain 2 /
             * RealerCheatUnlocks hagan normalmente
             * la revocación.
             */
            orig(
                self,
                unlockableDef
            );


            if (
                !SurvivorUnlockManager
                    .TryGetCustomUnlockBodyName(
                        unlockableDef,
                        out string bodyName
                    )
            )
            {
                return;
            }


            bool newlyArmed =
                ReplayArmedBodies.Add(
                    bodyName
                );


            logger?.LogInfo(
                "[REPLAY] Unlock USU revocado | " +
                $"Body: {bodyName} | " +
                $"Replay: ON | " +
                $"Nuevo estado: {newlyArmed}"
            );
        }


        // =========================================================
        // UNLOCK CONCEDIDO
        // =========================================================

        private static void UserProfile_GrantUnlockable(
            On.RoR2.UserProfile
                .orig_GrantUnlockable orig,

            UserProfile self,

            UnlockableDef unlockableDef
        )
        {
            /*
             * Dejamos que el juego conceda primero
             * el Unlockable.
             */
            orig(
                self,
                unlockableDef
            );


            if (
                !SurvivorUnlockManager
                    .TryGetCustomUnlockBodyName(
                        unlockableDef,
                        out string bodyName
                    )
            )
            {
                return;
            }


            bool wasArmed =
                ReplayArmedBodies.Remove(
                    bodyName
                );


            logger?.LogInfo(
                "[REPLAY] Unlock USU concedido | " +
                $"Body: {bodyName} | " +
                $"Replay: OFF | " +
                $"Estaba armado: {wasArmed}"
            );
        }


        // =========================================================
        // ¿ESTÁ ARMADO?
        // =========================================================

        public static bool IsReplayArmed(
            string bodyName
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return false;
            }


            return ReplayArmedBodies.Contains(
                bodyName
            );
        }


        // =========================================================
        // ARMAR MANUALMENTE
        // =========================================================
        //
        // Nos servirá después para multiplayer.
        //
        // Todavía no lo usaremos en esta primera prueba.
        //
        // =========================================================

        public static void ArmReplay(
            string bodyName
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return;
            }


            bool newlyArmed =
                ReplayArmedBodies.Add(
                    bodyName
                );


            logger?.LogInfo(
                "[REPLAY] Armado manualmente | " +
                $"Body: {bodyName} | " +
                $"Nuevo estado: {newlyArmed}"
            );
        }


        // =========================================================
        // DESARMAR MANUALMENTE
        // =========================================================

        public static void DisarmReplay(
            string bodyName
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return;
            }


            bool wasArmed =
                ReplayArmedBodies.Remove(
                    bodyName
                );


            logger?.LogInfo(
                "[REPLAY] Desarmado manualmente | " +
                $"Body: {bodyName} | " +
                $"Estaba armado: {wasArmed}"
            );
        }


        // =========================================================
        // LIMPIAR
        // =========================================================

        public static void Clear()
        {
            ReplayArmedBodies.Clear();


            logger?.LogInfo(
                "[REPLAY] Estados limpiados."
            );
        }
    }
}