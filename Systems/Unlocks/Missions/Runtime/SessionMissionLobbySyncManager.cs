using System;
using System.Reflection;
using BepInEx.Logging;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * SESSION MISSION LOBBY SYNC MANAGER
     * =============================================================
     *
     * Sin polling.
     *
     * Evento usado:
     *     CharacterSelectController.Awake
     *
     * HOST:
     *     construye LobbySnapshot desde su configuración local
     *     y lo envía a clientes.
     *
     * CLIENTE:
     *     solicita el snapshot al host cuando entra a selección.
     *
     * Cuando cambia el snapshot efectivo:
     *     refrescamos los iconos visibles de la selección.
     * =============================================================
     */
    public static class SessionMissionLobbySyncManager
    {
        private static ManualLogSource logger;

        private static bool initialized;


        private static MethodInfo
            survivorIconRebuildMethod;


        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;

            logger =
                log;


            On.RoR2.UI.CharacterSelectController.Awake +=
                CharacterSelectController_Awake;


            SessionMissionRegistry
                .EffectiveSnapshotChanged +=
                OnEffectiveSnapshotChanged;


            survivorIconRebuildMethod =
                typeof(
                    SurvivorIconController
                )
                .GetMethod(
                    "Rebuild",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );


            logger?.LogInfo(
                "[MISSION LOBBY] Sincronización de lobby inicializada."
            );
        }


        private static void CharacterSelectController_Awake(
            On.RoR2.UI.CharacterSelectController.orig_Awake orig,
            CharacterSelectController self
        )
        {
            if (
                NetworkServer.active &&
                Run.instance == null
            )
            {
                SessionMissionRegistry
                    .RefreshLobbySnapshotAndBroadcast(
                        "CharacterSelectController.Awake"
                    );
            }
            else if (
                !NetworkServer.active &&
                NetworkClient.active &&
                Run.instance == null
            )
            {
                new SessionMissionLobbyRequestMessage()
                    .Send(
                        NetworkDestination.Server
                    );


                logger?.LogInfo(
                    "[MISSION LOBBY] Cliente solicitó snapshot al host."
                );
            }


            orig(
                self
            );
        }


        public static void
            ReceiveClientLobbySnapshotRequest()
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance != null)
            {
                logger?.LogInfo(
                    "[MISSION LOBBY] Petición de cliente ignorada: " +
                    "la run ya comenzó."
                );

                return;
            }


            logger?.LogInfo(
                "[MISSION LOBBY] Petición de snapshot recibida desde cliente."
            );


            SessionMissionRegistry
                .RefreshLobbySnapshotAndBroadcast(
                    "solicitud de cliente"
                );
        }


        private static void OnEffectiveSnapshotChanged()
        {
            RefreshVisibleSurvivorIcons();
        }


        private static void RefreshVisibleSurvivorIcons()
        {
            try
            {
                SurvivorIconController[] icons =
                    UnityEngine.Object
                        .FindObjectsOfType<
                            SurvivorIconController
                        >();


                if (
                    icons == null ||
                    icons.Length == 0
                )
                {
                    return;
                }


                if (survivorIconRebuildMethod == null)
                {
                    logger?.LogWarning(
                        "[MISSION UI] No se encontró " +
                        "SurvivorIconController.Rebuild."
                    );

                    return;
                }


                int refreshed =
                    0;


                foreach (
                    SurvivorIconController icon
                    in icons
                )
                {
                    if (icon == null)
                    {
                        continue;
                    }


                    try
                    {
                        survivorIconRebuildMethod
                            .Invoke(
                                icon,
                                null
                            );


                        refreshed++;
                    }
                    catch (Exception exception)
                    {
                        logger?.LogWarning(
                            "[MISSION UI] No se pudo refrescar " +
                            "un SurvivorIconController."
                        );

                        logger?.LogWarning(
                            exception.Message
                        );
                    }
                }


                logger?.LogInfo(
                    $"[MISSION UI] Iconos de selección refrescados | " +
                    $"Cantidad: {refreshed}"
                );
            }
            catch (Exception exception)
            {
                logger?.LogError(
                    "[MISSION UI] Error refrescando selección de personajes."
                );

                logger?.LogError(
                    exception
                );
            }
        }
    }
}
