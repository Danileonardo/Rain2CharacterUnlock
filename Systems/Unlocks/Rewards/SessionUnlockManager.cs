using System;

using BepInEx.Logging;

using R2API.Networking;
using R2API.Networking.Interfaces;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    public static class SessionUnlockManager
    {
        private static ManualLogSource logger;


        private static bool initialized;


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


            logger?.LogInfo(
                "SessionUnlockManager inicializado."
            );
        }


        // =========================================================
        // MISIÓN COMPLETADA
        // =========================================================
        //
        // ESTE MÉTODO SÓLO DEBE SER LLAMADO
        // POR EL SERVIDOR / HOST.
        //
        // Ejemplo:
        //
        // SessionUnlockManager.CompleteMission(
        //     "RobHunkBody"
        // );
        //
        // =========================================================

        public static void CompleteMission(
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


            // -----------------------------------------------------
            // AUTORIDAD DEL HOST
            // -----------------------------------------------------

            if (!NetworkServer.active)
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK] Se intentó completar una " +
                    $"misión fuera del servidor | Body: {bodyName}"
                );


                return;
            }


            // -----------------------------------------------------
            // CONFIRMAR QUE ES UN UNLOCK USU
            // -----------------------------------------------------

            if (
                !SurvivorUnlockManager
                    .TryGetCustomUnlockable(
                        bodyName,
                        out UnlockableDef unlockable
                    ) ||
                unlockable == null
            )
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK] No existe Unlockable USU | " +
                    $"Body: {bodyName}"
                );


                return;
            }


            logger?.LogInfo(
                "[SESSION UNLOCK] Misión completada | " +
                $"Body: {bodyName}"
            );


            // -----------------------------------------------------
            // HOST / JUGADORES LOCALES
            // -----------------------------------------------------

            GrantLocally(
                bodyName
            );


            // -----------------------------------------------------
            // CLIENTES REMOTOS
            // -----------------------------------------------------

            new SessionUnlockGrantMessage(
                bodyName
            )
            .Send(
                NetworkDestination.Clients
            );
        }


        // =========================================================
        // CONCEDER EN ESTE CLIENTE
        // =========================================================

        public static void GrantLocally(
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


            // -----------------------------------------------------
            // UNLOCKABLE
            // -----------------------------------------------------

            if (
                !SurvivorUnlockManager
                    .TryGetCustomUnlockable(
                        bodyName,
                        out UnlockableDef unlockable
                    ) ||
                unlockable == null
            )
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK] Unlockable local no encontrado | " +
                    $"Body: {bodyName}"
                );


                return;
            }


            // -----------------------------------------------------
            // ACHIEVEMENT
            // -----------------------------------------------------

            SurvivorUnlockManager
                .TryGetCustomAchievement(
                    bodyName,
                    out AchievementDef achievementDef
                );


            // -----------------------------------------------------
            // PUEDE HABER MÁS DE UN LOCAL USER
            // -----------------------------------------------------

            foreach (
                LocalUser localUser
                in LocalUserManager
                    .readOnlyLocalUsersList
            )
            {
                UserProfile profile =
                    localUser?.userProfile;


                if (profile == null)
                {
                    continue;
                }


                bool changed =
                    false;


                // =================================================
                // RESTAURAR ACHIEVEMENT
                // =================================================
                //
                // RealerCheatUnlocks utiliza exactamente el sistema
                // de UserProfile para añadir/revocar achievements.
                //
                // Si el achievement fue revocado al volver a
                // bloquear el survivor, lo concedemos nuevamente.
                //
                // =================================================

                if (
                    achievementDef != null &&
                    !profile.HasAchievement(
                        achievementDef.identifier
                    )
                )
                {
                    profile.AddAchievement(
                        achievementDef.identifier,
                        true
                    );


                    changed =
                        true;


                    logger?.LogInfo(
                        "[SESSION UNLOCK] Achievement concedido | " +
                        $"Body: {bodyName} | " +
                        $"Achievement: {achievementDef.identifier}"
                    );
                }


                // =================================================
                // RESTAURAR UNLOCKABLE
                // =================================================

                if (
                    !profile.HasUnlockable(
                        unlockable
                    )
                )
                {
                    profile.GrantUnlockable(
                        unlockable
                    );


                    changed =
                        true;


                    logger?.LogInfo(
                        "[SESSION UNLOCK] Unlockable concedido | " +
                        $"Body: {bodyName} | " +
                        $"Unlock: {unlockable.cachedName}"
                    );
                }


                // =================================================
                // GUARDAR
                // =================================================

                if (changed)
                {
                    profile.RequestEventualSave();
                }
                else
                {
                    logger?.LogInfo(
                        "[SESSION UNLOCK] El perfil ya tenía " +
                        $"la recompensa | Body: {bodyName}"
                    );
                }
            }
        }
    }
}