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
        "2.8.6"
    )]

    [BepInDependency(
        "com.KingEnderBrine.ScrollableLobbyUI",
        "1.10.5"
    )]

    public class Plugin :
        BaseUnityPlugin
    {
        public const string PluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";


        public const string PluginName =
            "Universal Survivor Unlocks";


        public const string PluginVersion =
            "0.3.1";


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

            TemporaryGeneratedTestOptions.Initialize(
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
             * C2.1 - Current Game / DLC4 compatibility audit.
             *
             * Reconoce explícitamente los nuevos survivors oficiales y
             * verifica que sigan clasificados como contenido del juego, no
             * como mods. Es sólo diagnóstico: no cambia unlocks, providers,
             * JSON ni Generated.
             */
            CurrentGameCompatibilityService
                .Audit(
                    Survivors,
                    Logger
                );


            /*
             * C2.3 - Catálogo estructural de métodos de desbloqueo vanilla.
             *
             * Lee los AchievementDef que entregan el unlock de cada survivor
             * oficial para identificar patrones reutilizables en Mission V2.
             * Es diagnóstico solamente: no altera autoridad ni progreso.
             */
            VanillaUnlockMethodCatalogService
                .Audit(
                    Survivors,
                    Logger
                );


            /*
             * C2.4 - Cobertura de métodos vanilla contra Mission V2.
             *
             * Clasifica de forma conservadora qué Achievement/Tracker oficiales
             * parecen estar representados por objetivos reutilizables existentes.
             * Sólo diagnóstico: los casos dudosos quedan como LikelyCovered o
             * Unmapped para revisión con el log real.
             */
            VanillaUnlockCoverageService
                .Audit(
                    Survivors,
                    Logger
                );


            /*
             * C2.5 - Inspección semántica de métodos oficiales.
             *
             * Extrae por reflexión nombres de miembros relevantes desde los
             * Achievement/ServerTracker reales. Esto nos permite decidir qué
             * mecánicas Mission V2 faltan usando evidencia del runtime actual,
             * sin modificar achievements ni autoridad.
             */
            VanillaUnlockSemanticInspectionService
                .Audit(
                    Survivors,
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
             * C5 - 0.3.0 -> 0.3.1 persistence migration gate.
             *
             * Audita el resultado real después de NormalizeAfterLoad + Sync.
             * No modifica el JSON; sólo comprueba que la forma final siga
             * siendo compatible con provider/source y slots dormidos.
             */
            PersistenceMigrationAuditService
                .Audit(
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
             * Genera el blueprint a partir del ranking de G6 y de un nonce
             * persistido por survivor. G8 materializa y guarda Mission V2.
             * La autoridad se resuelve desde Survivors.json y el registro oficial.
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
             * Convierte los blueprints válidos de G7 en MissionDefinition.
             * Phase 1.5 persiste la misión con Source=Generated en el mismo
             * Survivors.json que usa Custom, sin convertirla en Custom.
             * Vuelve a comprobar autoridad autorada
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
             * G8 Phase 1.5 - Generated JSON Authority.
             *
             * Cada Generated materializada se guarda con Source=Generated.
             * Si el jugador borra su entrada o todo Survivors.json, también
             * desaparece generationNonce y el siguiente arranque usa una seed
             * nueva: reroll manual temporal hasta añadirlo a la UI.
             */
            GeneratedMissionRuntimeMaterializer
                .PersistGeneratedMissions();


            /*
             * C2.6 - Invariante final de autoridad de contenido oficial.
             *
             * Una actualización del juego nunca debe hacer que un survivor
             * oficial/DLC pase a ser objetivo Generated. El guard sólo audita
             * y alerta; no reescribe silenciosamente Survivors.json.
             */
            OfficialContentAuthorityInvariantService
                .Audit(
                    Survivors,
                    Logger
                );

            UsuLog.Diagnostic(
                Logger,
                "[CURRENT GAME COMPAT] C2 runtime validado | " +
                "Dependencias actuales + DLC4 + métodos vanilla + " +
                "Mission V2 extraction + authority guard."
            );


            /*
             * C4 - Provider Coexistence / G8 Phase 1.7 validation bridge.
             *
             * Audita la fila Original - USU - Personalizada - Generada sin
             * modificar Survivors.json. Cuando existen slots dormidos, prueba
             * su restauración únicamente sobre clones en memoria. También
             * comprueba coherencia Provider/Source, snapshot Generated y si
             * el nonce necesario para reroll sigue presente.
             */
            ProviderCoexistenceAuditService
                .Audit(
                    Logger
                );

            UsuLog.Verbose(
                Logger,
                "[PROVIDER COEXISTENCE] C4 código completo | " +
                "inventario de slots + restore dry-run + Generated snapshot + " +
                "reroll readiness + provider/source invariant | " +
                "cambios persistidos por auditoría: no."
            );


            /*
             * G8 Phase 1.6 - Generated Reroll UI.
             *
             * La Mission Library muestra Source=Generated como "Generada" y
             * habilita un dado junto al bloque de misión. Al pulsarlo cambia
             * generationNonce, recalcula G7/G8, persiste la nueva Mission V2 y
             * refresca presentación/runtime sin convertirla a Custom.
             */


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
             * con VerboseLogging=true emite sólo un resumen;
             * ExtendedAuditLogging=true habilita el volcado completo.
             * En una ejecución normal no añade trabajo ni spam al arranque.
             */
            if (UsuLog.VerboseEnabled || UsuLog.ExtendedAuditEnabled)
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