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


                // =========================================================
                // ESTADO ACTUAL DEL PERFIL
                // =========================================================

                bool hasAchievement =
                    achievementDef != null &&
                    profile.HasAchievement(
                        achievementDef.identifier
                    );


                bool hasUnlockable =
                    profile.HasUnlockable(
                        unlockable
                    );


                // =========================================================
                // YA TIENE TODO
                // =========================================================

                if (
                    hasAchievement &&
                    hasUnlockable
                )
                {
                    logger?.LogInfo(
                        "[SESSION UNLOCK] El perfil ya tenía " +
                        $"la recompensa | Body: {bodyName}"
                    );


                    continue;
                }


                // =========================================================
                // CASO 1:
                // NO TIENE EL ACHIEVEMENT
                // =========================================================
                //
                // Esta es la ruta normal de desbloqueo.
                //
                // Sólo añadimos el Achievement.
                //
                // NO llamamos también a GrantUnlockable() en este mismo
                // instante, porque el AchievementDef ya está vinculado
                // al Unlockable mediante unlockableRewardIdentifier.
                //
                // Esto evita:
                // Achievement popup
                // +
                // Unlockable popup
                //
                // =========================================================

                if (
                    achievementDef != null &&
                    !hasAchievement
                )
                {
                    profile.AddAchievement(
                        achievementDef.identifier,
                        true
                    );


                    profile.RequestEventualSave();


                    logger?.LogInfo(
                        "[SESSION UNLOCK] Recompensa concedida mediante Achievement | " +
                        $"Body: {bodyName} | " +
                        $"Achievement: {achievementDef.identifier}"
                    );


                    continue;
                }


                // =========================================================
                // CASO 2:
                // TIENE ACHIEVEMENT, PERO LE FALTA EL UNLOCKABLE
                // =========================================================
                //
                // Esto cubre mods/herramientas que hayan revocado
                // solamente el Unlockable.
                //
                // Como el Achievement ya existe, no podemos volver a
                // añadirlo.
                //
                // Restauramos solamente el Unlockable.
                //
                // =========================================================

                if (!hasUnlockable)
                {
                    profile.GrantUnlockable(
                        unlockable
                    );


                    profile.RequestEventualSave();


                    logger?.LogInfo(
                        "[SESSION UNLOCK] Unlockable restaurado | " +
                        $"Body: {bodyName} | " +
                        $"Unlock: {unlockable.cachedName}"
                    );
                }
            }
        }
    }
}