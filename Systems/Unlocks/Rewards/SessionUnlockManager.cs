using System;
using System.Collections.Generic;

using BepInEx.Logging;

using R2API.Networking;
using R2API.Networking.Interfaces;

using RoR2;

using UnityEngine;
using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    public static class SessionUnlockManager
    {
        private static ManualLogSource logger;


        private static bool initialized;


        /*
         * Sólo aceptamos confirmaciones para rewards cuya misión
         * fue completada realmente por el host durante esta run.
         */
        private static readonly HashSet<string>
            CompletedBodiesThisRun =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


        /*
         * Evita repetir el mensaje de chat si una confirmación
         * llega más de una vez.
         */
        private static readonly HashSet<string>
            AnnouncedAchievementsThisRun =
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


            Run.onRunStartGlobal +=
                OnRunStart;


            Run.onRunDestroyGlobal +=
                OnRunEnd;


            logger?.LogInfo(
                "SessionUnlockManager inicializado."
            );
        }


        private static void OnRunStart(
            Run run
        )
        {
            CompletedBodiesThisRun.Clear();
            AnnouncedAchievementsThisRun.Clear();
        }


        private static void OnRunEnd(
            Run run
        )
        {
            CompletedBodiesThisRun.Clear();
            AnnouncedAchievementsThisRun.Clear();
        }


        // =========================================================
        // ESTADO LOCAL DE DESBLOQUEO
        // =========================================================
        //
        // Se usa únicamente para decidir si vale la pena mostrar
        // progreso de una misión en ESTE cliente/host.
        //
        // IMPORTANTE:
        // no desactiva la misión en red. Si el host ya tiene al
        // personaje pero un cliente remoto todavía no, el runtime
        // continúa procesando la misión y ese cliente puede recibir
        // el desbloqueo normalmente.
        // =========================================================

        public static bool AreAllLocalUsersUnlocked(
            string bodyName
        )
        {
            if (
                string.IsNullOrWhiteSpace(bodyName) ||
                !SurvivorUnlockManager.TryGetCustomUnlockable(
                    bodyName,
                    out UnlockableDef unlockable
                ) ||
                unlockable == null
            )
            {
                return false;
            }


            bool foundLocalProfile = false;


            foreach (
                LocalUser localUser
                in LocalUserManager.readOnlyLocalUsersList
            )
            {
                UserProfile profile = localUser?.userProfile;


                if (profile == null)
                {
                    continue;
                }


                foundLocalProfile = true;


                if (!profile.HasUnlockable(unlockable))
                {
                    return false;
                }
            }


            return foundLocalProfile;
        }


        // =========================================================
        // MISIÓN COMPLETADA
        // =========================================================
        //
        // SÓLO HOST / SERVIDOR.
        //
        // El host valida la misión.
        // El host concede su recompensa local.
        // Los clientes reciben SessionUnlockGrantMessage y
        // comprueban/conceden la recompensa en SU perfil.
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


            if (!NetworkServer.active)
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK] Se intentó completar una " +
                    $"misión fuera del servidor | Body: {bodyName}"
                );

                return;
            }


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


            bool suppressLocalMissionLogs =
                AreAllLocalUsersUnlocked(
                    bodyName
                );


            CompletedBodiesThisRun.Add(
                bodyName
            );


            if (!suppressLocalMissionLogs)
            {
                logger?.LogInfo(
                    "[SESSION UNLOCK] Misión completada | " +
                    $"Body: {bodyName}"
                );
            }


            // -----------------------------------------------------
            // HOST / JUGADORES LOCALES
            // -----------------------------------------------------

            List<SessionUnlockGrantResult> localResults =
                GrantLocally(
                    bodyName
                );


            foreach (
                SessionUnlockGrantResult result
                in localResults
            )
            {
                NetworkUser networkUser =
                    result
                        .LocalUser?
                        .currentNetworkUser;


                HandleGrantResult(
                    networkUser,
                    result.BodyName,
                    result.AchievementBefore,
                    result.UnlockableBefore,
                    result.AchievementAfter,
                    result.UnlockableAfter,
                    "HOST LOCAL"
                );
            }


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
        // CONCEDER / VERIFICAR EN ESTE CLIENTE
        // =========================================================
        //
        // IMPORTANTE:
        // Este método NO decide si la misión se completó.
        // Sólo trabaja con el UserProfile local.
        //
        // =========================================================

        public static List<SessionUnlockGrantResult>
            GrantLocally(
                string bodyName
            )
        {
            List<SessionUnlockGrantResult> results =
                new List<SessionUnlockGrantResult>();


            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return results;
            }


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

                return results;
            }


            if (
                !SurvivorUnlockManager
                    .TryGetCustomAchievement(
                        bodyName,
                        out AchievementDef achievementDef
                    ) ||
                achievementDef == null
            )
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK] Achievement local no encontrado | " +
                    $"Body: {bodyName}"
                );

                return results;
            }


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


                bool achievementBefore =
                    profile.HasAchievement(
                        achievementDef.identifier
                    );


                bool unlockableBefore =
                    profile.HasUnlockable(
                        unlockable
                    );


                bool wasAlreadyUnlocked =
                    unlockableBefore;


                if (!wasAlreadyUnlocked)
                {
                    logger?.LogInfo(
                        "[SESSION UNLOCK LOCAL] ANTES | " +
                        $"Body: {bodyName} | " +
                        $"Achievement: {achievementBefore} | " +
                        $"Unlockable: {unlockableBefore}"
                    );
                }


                // -------------------------------------------------
                // 1. ACHIEVEMENT
                // -------------------------------------------------
                //
                // Conservamos la ruta actual porque el popup superior
                // ya funciona correctamente.
                //
                // -------------------------------------------------

                if (!achievementBefore)
                {
                    profile.AddAchievement(
                        achievementDef.identifier,
                        true
                    );
                }


                bool achievementAfterAdd =
                    profile.HasAchievement(
                        achievementDef.identifier
                    );


                bool unlockableAfterAdd =
                    profile.HasUnlockable(
                        unlockable
                    );


                // -------------------------------------------------
                // 2. VERIFICAR EL UNLOCKABLE REAL
                // -------------------------------------------------
                //
                // Ya no asumimos que AddAchievement() siempre dejó
                // la recompensa real en el perfil.
                //
                // Sólo reparamos el Unlockable si DESPUÉS del
                // Achievement sigue faltando.
                //
                // -------------------------------------------------

                if (
                    achievementAfterAdd &&
                    !unlockableAfterAdd
                )
                {
                    logger?.LogWarning(
                        "[SESSION UNLOCK LOCAL] Achievement concedido " +
                        "pero Unlockable ausente; reparando | " +
                        $"Body: {bodyName}"
                    );


                    profile.GrantUnlockable(
                        unlockable
                    );
                }


                bool achievementAfter =
                    profile.HasAchievement(
                        achievementDef.identifier
                    );


                bool unlockableAfter =
                    profile.HasUnlockable(
                        unlockable
                    );


                bool changed =
                    achievementBefore !=
                        achievementAfter ||
                    unlockableBefore !=
                        unlockableAfter;


                if (changed)
                {
                    profile.RequestEventualSave();
                }


                if (!wasAlreadyUnlocked || changed)
                {
                    logger?.LogInfo(
                        "[SESSION UNLOCK LOCAL] DESPUÉS | " +
                        $"Body: {bodyName} | " +
                        $"Achievement: {achievementAfter} | " +
                        $"Unlockable: {unlockableAfter} | " +
                        $"AchievementNuevo: {!achievementBefore && achievementAfter} | " +
                        $"UnlockableNuevo: {!unlockableBefore && unlockableAfter}"
                    );
                }


                results.Add(
                    new SessionUnlockGrantResult(
                        bodyName,
                        localUser,
                        achievementBefore,
                        unlockableBefore,
                        achievementAfter,
                        unlockableAfter
                    )
                );
            }


            return results;
        }


        // =========================================================
        // RESULTADO CLIENTE -> HOST
        // =========================================================

        public static void ReceiveClientGrantResult(
            GameObject networkUserObject,
            string bodyName,
            bool achievementBefore,
            bool unlockableBefore,
            bool achievementAfter,
            bool unlockableAfter
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return;
            }


            if (
                !CompletedBodiesThisRun.Contains(
                    bodyName
                )
            )
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK HOST] Se rechazó una confirmación " +
                    "para una misión no completada por el host | " +
                    $"Body: {bodyName}"
                );

                return;
            }


            if (networkUserObject == null)
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK HOST] ACK sin NetworkUserObject | " +
                    $"Body: {bodyName}"
                );

                return;
            }


            NetworkUser networkUser =
                networkUserObject
                    .GetComponent<NetworkUser>();


            if (networkUser == null)
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK HOST] ACK sin NetworkUser | " +
                    $"Body: {bodyName}"
                );

                return;
            }


            HandleGrantResult(
                networkUser,
                bodyName,
                achievementBefore,
                unlockableBefore,
                achievementAfter,
                unlockableAfter,
                "CLIENTE REMOTO"
            );
        }


        // =========================================================
        // PROCESAR RESULTADO EN HOST
        // =========================================================

        private static void HandleGrantResult(
            NetworkUser networkUser,
            string bodyName,
            bool achievementBefore,
            bool unlockableBefore,
            bool achievementAfter,
            bool unlockableAfter,
            string source
        )
        {
            bool achievementWasNew =
                !achievementBefore &&
                achievementAfter;


            bool unlockableWasNew =
                !unlockableBefore &&
                unlockableAfter;


            bool success =
                achievementAfter &&
                unlockableAfter;


            string playerName =
                networkUser != null
                    ? networkUser.userName
                    : "<sin NetworkUser>";


            bool wasAlreadyComplete =
                achievementBefore &&
                unlockableBefore &&
                achievementAfter &&
                unlockableAfter;


            if (!wasAlreadyComplete)
            {
                logger?.LogInfo(
                    "[SESSION UNLOCK HOST] Resultado recibido | " +
                    $"Fuente: {source} | " +
                    $"Jugador: {playerName} | " +
                    $"Body: {bodyName} | " +
                    $"Achievement: {achievementBefore}->{achievementAfter} | " +
                    $"Unlockable: {unlockableBefore}->{unlockableAfter} | " +
                    $"Success: {success}"
                );
            }


            if (!success)
            {
                logger?.LogError(
                    "[SESSION UNLOCK HOST] La recompensa NO quedó " +
                    "completa en el perfil del jugador | " +
                    $"Jugador: {playerName} | " +
                    $"Body: {bodyName}"
                );

                return;
            }


            /*
             * Si sólo reparamos un Unlockable antiguo pero el achievement
             * ya existía, NO debemos fingir que el jugador acaba de ganar
             * el achievement otra vez.
             */
            if (!achievementWasNew)
            {
                if (unlockableWasNew)
                {
                    logger?.LogInfo(
                        "[SESSION UNLOCK HOST] Unlockable reparado sin " +
                        "nuevo achievement; no se anuncia en chat | " +
                        $"Jugador: {playerName} | " +
                        $"Body: {bodyName}"
                    );
                }

                return;
            }


            if (networkUser == null)
            {
                return;
            }


            string announcementKey =
                networkUser
                    .gameObject
                    .GetInstanceID()
                    .ToString() +
                "|" +
                bodyName;


            if (
                !AnnouncedAchievementsThisRun.Add(
                    announcementKey
                )
            )
            {
                logger?.LogInfo(
                    "[SESSION UNLOCK HOST] Anuncio duplicado ignorado | " +
                    $"Jugador: {playerName} | " +
                    $"Body: {bodyName}"
                );

                return;
            }


            SessionUnlockAnnouncementManager
                .AnnounceAchievement(
                    networkUser,
                    bodyName,
                    logger
                );
        }
    }
}
