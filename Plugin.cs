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
            "0.3.0";


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
             * G3 Phase 2.3 - Mission Profile Analyzer.
             *
             * Construye señales diagnósticas exclusivamente desde los
             * Content Profiles que existen en el runtime actual, incluida
             * semántica por habilidad con evidencia/confianza.
             * No persiste snapshots ni toca dormantMissions.
             */
            MissionProfileService
                .Initialize(
                    SurvivorContentProfileService.GetAll(),
                    Logger
                );


            /*
             * G4 Phase 1.4 - Community Evidence Calibration.
             *
             * Conserva el análisis local G4 y añade evidencia comunitaria
             * curada offline por Stable SurvivorId. Las fuentes se comparan
             * por grupos independientes, nunca se consultan desde runtime y
             * siguen siendo una señal secundaria: sin dos fuentes realmente
             * independientes el resultado permanece Unknown.
             * No genera ni persiste misiones.
             */
            LocalPowerAssessmentService
                .Initialize(
                    MissionProfileService.GetAll(),
                    Logger
                );


            /*
             * G5 Phase 1.1 - Resolved Power Integration.
             *
             * Consume toda la evaluación G4: la potencia local sigue siendo
             * el ancla principal, GameplayComplexity permanece separada y la
             * evidencia comunitaria sólo aporta el ajuste ya calibrado por G4.
             * No usa CommunityPowerTier como decisión principal ni vuelve a
             * puntuar las fuentes aquí. Sigue siendo runtime-only: no elige
             * templates, no genera/asigna misiones y no cambia unlock/provider.
             */
            ChallengeTierService
                .Initialize(
                    LocalPowerAssessmentService.GetAll(),
                    Logger
                );


            /*
             * G6 Phase 1 - Mission Template Compatibility.
             *
             * Cruza el perfil mecánico G3 con el ChallengeTier G5 para
             * decidir qué familias de misión encajan con cada survivor. El
             * kit sigue siendo la señal principal; el tier sólo matiza el
             * tipo de reto. Para unlocks de survivor el target jamás cuenta
             * como performer, de modo que un personaje todavía bloqueado no
             * reciba una misión que exija usarlo.
             *
             * Sigue siendo diagnóstico: no genera, no asigna y no persiste
             * MissionDefinitions.
             */
            MissionTemplateCompatibilityService
                .Initialize(
                    MissionProfileService.GetAll(),
                    ChallengeTierService.GetAll(),
                    Logger
                );


            /*
             * G7 Phase 1.1 - Deterministic Mission Generator / Authority Bridge.
             *
             * Genera únicamente candidatos TEMPORALES en memoria a partir
             * del ranking de G6. Nunca persiste ni asigna una MissionDefinition
             * en esta fase. La autoridad se resuelve desde Survivors.json y el registro oficial.
             * Un preset oficial o una misión CUSTOM creada con USU siempre
             * tiene prioridad y suprime el fallback generado.
             *
             * La selección es reproducible mediante una seed estable basada
             * en PluginGuid + SurvivorId + GeneratorVersion. El target jamás
             * puede ser requisito para desbloquearse a sí mismo.
             */
            DeterministicMissionGeneratorService
                .Initialize(
                    Survivors,
                    MissionTemplateCompatibilityService.GetAll(),
                    Logger
                );


            /*
             * G8 Phase 1 - Generated Mission Runtime Materialization.
             *
             * Convierte los blueprints temporales válidos de G7 en una
             * MissionDefinition runtime-only. No persiste Generated, no la
             * convierte en CUSTOM y vuelve a comprobar autoridad autorada
             * cada vez que SessionMissionRegistry construye una entrada
             * efectiva. Phase 1.4 añade HealingSupport, ResourceMastery,
             * MeleeCombat y ProjectileCombat al soporte inicial Execution.
             */
            GeneratedMissionRuntimeMaterializer
                .Initialize(
                    Survivors,
                    MissionProfileService.GetAll(),
                    Logger
                );


            /*
             * G8 Phase 1.3 - Effective Mission Presentation.
             *
             * Sincroniza los tokens usados por AchievementDef con la misión
             * efectiva de cada survivor. Esto corrige los popups que seguían
             * mostrando KillEnemies 100 tras materializar Generated y permite
             * que Character Select/Run reflejen temporalmente la misión del host.
             */
            SurvivorUnlockManager
                .InitializeEffectiveMissionPresentation(
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