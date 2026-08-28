using System.Collections.Generic;
using BepInEx;
using R2API.ContentManagement;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    [BepInPlugin(
        PluginGuid,
        PluginName,
        PluginVersion
    )]

    [BepInDependency(
        R2APIContentManager.PluginGUID
    )]

    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";

        public const string PluginName =
            "Universal Survivor Unlocks";

        /*
         * Por ahora mantenemos esta versión
         * mientras terminamos la detección
         * automática de survivors modded.
         */
        public const string PluginVersion =
            "0.0.6";


        public static List<SurvivorInfo> Survivors
        {
            get;
            private set;
        }


        // =========================================================
        // AWAKE
        // =========================================================

        private void Awake()
        {
            Logger.LogInfo(
                "Universal Survivor Unlocks cargado correctamente."
            );


            /*
             * Cargamos Survivors.json antes de que
             * terminen de construirse los catálogos.
             *
             * Esto permite registrar UnlockableDefs que
             * ya estaban configurados anteriormente.
             */
            SurvivorJsonManager.LoadForStartup(
                Logger
            );


            /*
             * Registramos UnlockableDef + AchievementDef
             * existentes en el JSON.
             */
            SurvivorUnlockManager
                .RegisterConfiguredUnlockables(
                    Logger
                );


            /*
             * Sistema visual:
             *
             * bloqueado   -> silueta negra
             * desbloqueado -> color original
             */
            SurvivorLockedPortraitManager.Initialize(
                Logger
            );


            /*
             * Esperamos a que Risk of Rain 2
             * termine de cargar todos sus ContentPacks
             * y SurvivorDefs.
             */
            RoR2Application.onLoad +=
                OnGameLoaded;
        }


        // =========================================================
        // ROR2 TERMINÓ DE CARGAR
        // =========================================================

        private void OnGameLoaded()
        {
            Logger.LogInfo(
                "Risk of Rain 2 terminó de cargar."
            );


            // -----------------------------------------------------
            // 1. DETECTAR CONTENT PACKS DE MODS
            // -----------------------------------------------------

            Logger.LogInfo(
                "Analizando ContentPacks pertenecientes a mods..."
            );

            ModdedSurvivorRegistry.Rebuild(
                Logger
            );


            // -----------------------------------------------------
            // 2. DETECTAR TODOS LOS SURVIVORS
            // -----------------------------------------------------

            Logger.LogInfo(
                "Detectando survivors..."
            );

            Survivors =
                SurvivorDetector.DetectSurvivors(
                    Logger
                );


            // -----------------------------------------------------
            // 3. SINCRONIZAR JSON
            // -----------------------------------------------------

            SurvivorJsonManager.Sync(
                Survivors,
                Logger
            );


            // -----------------------------------------------------
            // 4. APLICAR UNLOCKS CONFIGURADOS
            // -----------------------------------------------------

            SurvivorUnlockManager
                .ApplyConfiguredUnlockables(
                    Survivors,
                    Logger
                );


            /*
             * Sólo necesitamos ejecutar esto una vez
             * durante la carga.
             */
            RoR2Application.onLoad -=
                OnGameLoaded;


            Logger.LogInfo(
                "Universal Survivor Unlocks terminó su inicialización."
            );
        }
    }
}