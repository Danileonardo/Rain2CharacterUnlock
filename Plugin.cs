using System.Collections.Generic;
using BepInEx;
using R2API.ContentManagement;
using R2API.Networking;
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

    [BepInDependency(
    NetworkingAPI.PluginGUID
    )]

    [BepInDependency(
        "com.rune580.riskofoptions",
        BepInDependency.DependencyFlags.SoftDependency
    )]

    public class Plugin :
        BaseUnityPlugin
    {
        public const string PluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";


        public const string PluginName =
            "Universal Survivor Unlocks";


        public const string PluginVersion =
            "0.2.2";


        public static List<SurvivorInfo> Survivors
        {
            get;
            private set;
        }


        private void Awake()
        {
            // Logging.VerboseLogging=false por defecto:
            // mantiene la consola limpia sin perder warnings/errores.
            UsuLog.Initialize(
                Config
            );


            NetworkingAPI
                .RegisterMessageType<
                    HunkBanditShotResultMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionUnlockResultMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    HunkRailgunnerShotResultMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionUnlockGrantMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionMissionSyncMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionMissionLobbyRequestMessage
                >();

            Logger.LogInfo(
                $"Universal Survivor Unlocks " +
                $"{PluginVersion} cargado."
            );


            // =====================================================
            // CONFIGURACIÓN
            // =====================================================

            SurvivorJsonManager
                .LoadForStartup(
                    Logger
                );


            // =====================================================
            // UNLOCKS CONOCIDOS DE ARRANQUES ANTERIORES
            // =====================================================

            SurvivorUnlockManager
                .RegisterConfiguredUnlockables(
                    Logger
                );


            // =====================================================
            // PROVIDER UNIVERSAL
            // =====================================================

            UniversalContentPackProvider
                .Initialize(
                    Logger
                );


            // =====================================================
            // VISUAL DE BLOQUEO
            // =====================================================

            SurvivorLockedPortraitManager
                .Initialize(
                    Logger
                );


            // =====================================================
            // FASE FINAL
            // =====================================================

            RoR2Application.onLoad +=
                OnGameLoaded;
        }


        private void OnGameLoaded()
        {
            Logger.LogInfo(
                "Risk of Rain 2 terminó de cargar."
            );

            /*
             * Runtime gameplay centralizado.
             *
             * Conserva el orden histórico de trackers Legacy, Mission v2,
             * autoridad de sesión y refresh sin exponer cada hook en Plugin.cs.
             */
            GameplayRuntimeBootstrap.Initialize(
                Logger
            );

            MissionLibraryUIManager.Initialize(
                Logger
            );

            /*
             * NO hacemos Rebuild.
                         *
                         * Conservamos toda la información que
                         * recopilamos desde peerLoadInfos.
                         *
                         * Sólo cruzamos los BodyPrefabs contra
                         * el catálogo final como fallback.
                         */
            ModdedSurvivorRegistry
                .ReconcileCatalog(
                    Logger
                );


            Survivors =
                SurvivorDetector
                    .DetectSurvivors(
                        Logger
                    );


            /*
             * Registro NUEVO de definiciones curadas.
             *
             * Fase 1: completamente pasivo. Sólo reconoce definitions cuyo
             * mod/body ya fue detectado por USU. No modifica localización,
             * providers, unlocks, misiones, JSON ni multiplayer.
             */
            SurvivorDefinitionRegistry
                .Initialize(
                    Survivors,
                    Logger
                );


            /*
             * Localización manual/curada de mods instalados.
             *
             * Los mods creadores ya ejecutaron Awake antes de este onLoad.
             * R2API conserva una traducción específica que ya exista, por lo
             * que Creator es-419/es-ES mantiene prioridad y USU sólo completa
             * los idiomas que el mod no registró.
             *
             * Se hace antes de construir Content Profiles para que la ficha de
             * USU capture desde el principio el mismo texto localizado que ve
             * la UI nativa del juego.
             */
            ModdedSurvivorLocalization
                .RegisterInstalledTranslations(
                    Survivors,
                    Logger
                );


            /*
             * 5G.2D-A
             *
             * Catálogo universal de contenido instalado. Esta capa es de
             * sólo lectura: no cambia providers, unlocks ni progreso.
             * Alimentará el navegador visual y el editor de misiones.
             */
            ContentCatalogService
                .Initialize(
                    Logger
                );


            /*
             * Persistimos en JSON las entradas
             * que fueron creadas en memoria.
             */
            SurvivorJsonManager
                .Sync(
                    Survivors,
                    Logger
                );


            /*
             * 5G.1D-F
             *
             * BasePresetId es la autoridad de cualquier misión cuya
             * Source sea Preset. Si un preset oficial cambió desde la
             * última ejecución, actualizamos su snapshot persistido antes
             * de aplicar providers/runtime.
             */
            MissionAssignmentService
                .RefreshAssignedPresetSnapshots(
                    Logger
                );


            /*
             * Aplicación final.
             */
            SurvivorUnlockManager
                .ApplyConfiguredUnlockables(
                    Survivors,
                    Logger
                );


            /*
             * 5G.2D-C
             *
             * Construye la ficha contextual de cada survivor DESPUÉS de
             * aplicar el provider final. De este modo el navegador futuro
             * conoce el unlock runtime real y, a partir del personaje,
             * relaciona únicamente sus skills, skins, lore y requisitos.
             */
            SurvivorContentProfileService
                .Initialize(
                    Survivors,
                    Logger
                );


            /*
             * Diagnóstico de cobertura de localización.
             *
             * Es deliberadamente opt-in porque recorre por reflexión los
             * perfiles, skills, skins y tokens visibles. Durante desarrollo
             * puede habilitarse con Logging.VerboseLogging=true; en una
             * ejecución normal no añade trabajo ni spam al arranque.
             */
            if (UsuLog.VerboseEnabled)
            {
                LocalizationCoverageDiagnostics.Log(
                    SurvivorContentProfileService.GetAll(),
                    Logger
                );
            }


            RoR2Application.onLoad -=
                OnGameLoaded;


            Logger.LogInfo(
                "Universal Survivor Unlocks terminó " +
                "su inicialización."
            );
        }
    }
}