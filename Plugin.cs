using System.Collections.Generic;
using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    [BepInPlugin(
        PluginGuid,
        PluginName,
        PluginVersion
    )]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";

        public const string PluginName =
            "Universal Survivor Unlocks";

        public const string PluginVersion =
            "0.0.5";

        public static List<SurvivorInfo> Survivors
        {
            get;
            private set;
        }

        private void Awake()
        {
            Logger.LogInfo(
                "Universal Survivor Unlocks cargado correctamente."
            );

            /*
             * 1. Cargamos el JSON existente al iniciar.
             */
            SurvivorJsonManager.LoadForStartup(
                Logger
            );

            /*
             * 2. Registramos los UnlockableDef y
             * AchievementDef personalizados.
             */
            SurvivorUnlockManager
                .RegisterConfiguredUnlockables(
                    Logger
                );

            /*
             * 3. Activamos únicamente el sistema
             * visual para convertir el retrato
             * bloqueado en una silueta negra.
             */
            SurvivorLockedPortraitManager.Initialize(
                Logger
            );

            /*
             * 4. Esperamos a que Risk of Rain 2
             * termine de cargar sus catálogos.
             */
            RoR2Application.onLoad +=
                OnGameLoaded;
        }

        private void OnGameLoaded()
        {
            Logger.LogInfo(
                "Risk of Rain 2 terminó de cargar. Detectando survivors..."
            );

            /*
             * Detectamos todos los survivors.
             */
            Survivors =
                SurvivorDetector.DetectSurvivors(
                    Logger
                );

            /*
             * Actualizamos Survivors.json.
             */
            SurvivorJsonManager.Sync(
                Survivors,
                Logger
            );

            /*
             * Asignamos nuestros UnlockableDef
             * a los survivors que tengan
             * challenge.enabled = true.
             */
            SurvivorUnlockManager
                .ApplyConfiguredUnlockables(
                    Survivors,
                    Logger
                );

            RoR2Application.onLoad -=
                OnGameLoaded;
        }
    }
}