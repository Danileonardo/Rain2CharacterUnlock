using BepInEx.Logging;
using RoR2.UI;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// 5G.2E-A - Entrada de la UI de USU.
    ///
    /// Prioridad:
    /// 1) Risk of Options: USU vive EMBEBIDO dentro de Mod Options.
    /// 2) Sin Risk of Options: conserva el launcher probado de Character Select.
    ///
    /// IMPORTANTE:
    /// Cuando Risk of Options está disponible ya NO se crea la ventana
    /// persistente OnGUI de 5G.2B/5G.2C.
    /// </summary>
    public static class MissionLibraryUIManager
    {
        private static bool initialized;
        private static ManualLogSource logger;

        private static GameObject persistentRoot;
        private static MissionLibraryPanelController persistentPanel;

        private static bool riskOfOptionsRegistered;


        public static void Initialize(
            ManualLogSource activeLogger
        )
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            logger = activeLogger;

            SurvivorLocalization.EnsureRegistered();

            riskOfOptionsRegistered =
                MissionLibraryRiskOfOptionsIntegration
                    .TryRegister(
                        logger
                    );

            if (riskOfOptionsRegistered)
            {
                logger?.LogInfo(
                    "[MISSION LIBRARY UI] 5G.2E-A inicializada | " +
                    "Modo: RiskOfOptions EMBEDDED | " +
                    "Sin ventana OnGUI independiente"
                );

                return;
            }

            // Fallback sólo cuando Risk of Options no está disponible.
            EnsurePersistentPanel();

            On.RoR2.UI.CharacterSelectController.Awake +=
                CharacterSelectController_Awake;

            int attachedExisting =
                AttachToExistingControllers();

            logger?.LogInfo(
                "[MISSION LIBRARY UI] 5G.2E-A inicializada | " +
                "RiskOfOptions no disponible; launcher de Character Select activo | " +
                "Controladores existentes: " +
                attachedExisting
            );
        }


        /// <summary>
        /// Conservado por compatibilidad con llamadas antiguas.
        /// Con RiskOfOptions activo NO abre una ventana externa.
        /// </summary>
        public static void OpenFromSettings()
        {
            if (riskOfOptionsRegistered)
            {
                logger?.LogDebug(
                    "[MISSION LIBRARY UI] OpenFromSettings ignorado: " +
                    "5G.2E-A usa interfaz embebida en RiskOfOptions."
                );

                return;
            }

            EnsurePersistentPanel();
            persistentPanel?.OpenExternal();
        }


        private static void EnsurePersistentPanel()
        {
            if (persistentPanel != null)
            {
                return;
            }

            persistentRoot =
                new GameObject(
                    "USU_MissionLibrary_Fallback"
                );

            Object.DontDestroyOnLoad(
                persistentRoot
            );

            persistentPanel =
                persistentRoot.AddComponent<
                    MissionLibraryPanelController
                >();

            persistentPanel.Initialize(
                logger
            );

            // La instancia fallback se abre desde el launcher de Character Select.
            persistentPanel.SetLauncherVisible(
                false
            );
        }


        private static void CharacterSelectController_Awake(
            On.RoR2.UI.CharacterSelectController.orig_Awake orig,
            CharacterSelectController self
        )
        {
            orig(self);

            AttachFallbackLauncher(
                self
            );
        }


        private static int AttachToExistingControllers()
        {
            CharacterSelectController[] controllers =
                Resources.FindObjectsOfTypeAll<
                    CharacterSelectController
                >();

            if (controllers == null)
            {
                return 0;
            }

            int attached = 0;

            for (
                int i = 0;
                i < controllers.Length;
                i++
            )
            {
                CharacterSelectController controller =
                    controllers[i];

                if (
                    controller == null ||
                    controller.gameObject == null ||
                    !controller.gameObject.scene.IsValid()
                )
                {
                    continue;
                }

                if (
                    AttachFallbackLauncher(
                        controller
                    )
                )
                {
                    attached++;
                }
            }

            return attached;
        }


        private static bool AttachFallbackLauncher(
            CharacterSelectController controller
        )
        {
            if (
                controller == null ||
                controller.gameObject == null
            )
            {
                return false;
            }

            MissionLibraryPanelController existing =
                controller.GetComponent<
                    MissionLibraryPanelController
                >();

            if (existing != null)
            {
                existing.Initialize(
                    logger
                );

                existing.SetLauncherVisible(
                    true
                );

                return false;
            }

            MissionLibraryPanelController panel =
                controller.gameObject.AddComponent<
                    MissionLibraryPanelController
                >();

            panel.Initialize(
                logger
            );

            panel.SetLauncherVisible(
                true
            );

            logger?.LogInfo(
                "[MISSION LIBRARY UI] Launcher fallback adjuntado | " +
                "Object: " +
                controller.gameObject.name
            );

            return true;
        }
    }
}
