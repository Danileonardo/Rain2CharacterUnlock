using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using RoR2.ContentManagement;

namespace UniversalSurvivorUnlocks
{
    public sealed class UniversalContentPackProvider :
        IContentPackProvider
    {
        private const string UsuPluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";

        private static ManualLogSource _logger;

        private static bool _initialized;

        private static UniversalContentPackProvider
            _instance;


        private readonly ContentPack _contentPack;


        /*
         * Unlockables que nosotros creamos durante
         * GenerateContentPackAsync.
         */
        private readonly HashSet<string>
            _dynamicUnlockBodies =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


        /*
         * Sólo para evitar inundar el log.
         */
        private readonly HashSet<string>
            _loggedAutoLocks =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


        private readonly HashSet<string>
            _loggedOriginalUnlocks =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


        public string identifier =>
            UsuPluginGuid +
            ".DynamicUnlocks";


        private UniversalContentPackProvider()
        {
            _contentPack =
                new ContentPack();


            _contentPack.identifier =
                identifier;
        }


        // =========================================================
        // INICIALIZAR
        // =========================================================

        public static void Initialize(
            ManualLogSource logger
        )
        {
            if (_initialized)
            {
                return;
            }


            _initialized =
                true;


            _logger =
                logger;


            _instance =
                new UniversalContentPackProvider();


            ModdedSurvivorRegistry.Reset(
                logger
            );


            ContentSourceRegistry.Reset(
                logger
            );


            /*
             * Aquí YA NO analizamos survivors.
             *
             * Solamente registramos nuestro
             * IContentPackProvider.
             */
            ContentManager.collectContentPackProviders +=
                CollectContentPackProviders;


            UsuLog.Verbose(
                logger,
                "UniversalContentPackProvider registrado."
            );
        }


        // =========================================================
        // REGISTRAR PROVIDER
        // =========================================================

        private static void CollectContentPackProviders(
            ContentManager.AddContentPackProviderDelegate
                addContentPackProvider
        )
        {
            ContentManager.collectContentPackProviders -=
                CollectContentPackProviders;


            addContentPackProvider(
                _instance
            );


            UsuLog.Verbose(
                _logger,
                "UniversalContentPackProvider añadido " +
                "al pipeline de ContentManager."
            );
        }


        // =========================================================
        // LOAD STATIC
        // =========================================================

        public IEnumerator LoadStaticContentAsync(
            LoadStaticContentAsyncArgs args
        )
        {
            /*
             * No necesitamos cargar assets aquí.
             *
             * Los survivors de los otros mods todavía
             * están terminando de cargarse.
             */
            args.ReportProgress(
                1f
            );


            yield break;
        }


        // =========================================================
        // GENERATE CONTENT PACK
        // =========================================================

        public IEnumerator GenerateContentPackAsync(
            GetContentPackAsyncArgs args
        )
        {
            /*
             * GenerateContentPackAsync puede ejecutarse
             * varias veces.
             *
             * peerLoadInfos contiene los ContentPacks
             * generados por LOS DEMÁS providers.
             */
            foreach (
                ContentPackLoadInfo peer
                in args.peerLoadInfos
            )
            {
                ContentSourceRegistry
                    .RegisterContentPack(
                        peer.previousContentPack,
                        _logger
                    );


                ModdedSurvivorRegistry
                    .RegisterContentPack(
                        peer.previousContentPack,
                        _logger
                    );
            }


            ProcessDiscoveredSurvivors();


            /*
             * Copiamos nuestro ContentPack DESPUÉS
             * de haber añadido cualquier UnlockableDef
             * dinámico descubierto en esta pasada.
             */
            ContentPack.Copy(
                _contentPack,
                args.output
            );


            args.ReportProgress(
                1f
            );


            yield break;
        }


        // =========================================================
        // PROCESAR SURVIVORS DESCUBIERTOS
        // =========================================================

        private void ProcessDiscoveredSurvivors()
        {
            List<SurvivorDef> survivors =
                ModdedSurvivorRegistry
                    .GetRegisteredSurvivors();


            foreach (
                SurvivorDef survivor
                in survivors
            )
            {
                if (
                    survivor == null ||
                    survivor.hidden ||
                    survivor.bodyPrefab == null
                )
                {
                    continue;
                }


                string bodyName =
                    survivor.bodyPrefab.name;


                ModdedSurvivorRegistry
                    .TryGetSource(
                        survivor,
                        out string packIdentifier,
                        out string assemblyName
                    );


                UnlockableDef currentUnlock =
                    survivor.unlockableDef;


                /*
                 * Recordamos SIEMPRE el estado original antes de decidir qué
                 * provider queda activo. Un provider USU seleccionado por el
                 * usuario puede reemplazar temporalmente un unlock del autor,
                 * pero nunca debe destruirlo.
                 */
                if (
                    currentUnlock == null ||
                    !SurvivorUnlockManager.IsCustomUnlock(
                        currentUnlock
                    )
                )
                {
                    SurvivorUnlockManager
                        .RememberOriginalUnlock(
                            survivor,
                            currentUnlock
                        );
                }


                // =================================================
                // OBTENER / CREAR CONFIGURACIÓN
                // =================================================

                SurvivorJsonEntry entry =
                    SurvivorJsonManager
                        .GetEntryAnywhere(
                            bodyName
                        );


                if (entry == null)
                {
                    entry =
                        SurvivorJsonManager
                            .CreateAutomaticEntry(
                                survivor,
                                packIdentifier,
                                assemblyName,
                                _logger
                            );
                }


                if (entry == null)
                {
                    continue;
                }


                // =================================================
                // PREPARAR UNLOCK USU DORMIDO
                // =================================================
                //
                // Aunque Original esté activo, el UnlockableDef de USU debe
                // existir antes de que RoR2 cierre sus catálogos. Así un cambio
                // Original -> USU puede aplicarse sin reiniciar.
                // =================================================

                UnlockableDef customUnlock;


                if (
                    !SurvivorUnlockManager
                        .TryGetCustomUnlockable(
                            bodyName,
                            out customUnlock
                        )
                )
                {
                    customUnlock =
                        SurvivorUnlockManager
                            .RegisterDynamicUnlockable(
                                bodyName,
                                entry,
                                _logger
                            );


                    if (customUnlock == null)
                    {
                        _logger.LogError(
                            $"No fue posible preparar " +
                            $"el unlock USU de {bodyName}."
                        );


                        continue;
                    }


                    if (
                        _dynamicUnlockBodies.Add(
                            bodyName
                        )
                    )
                    {
                        _contentPack
                            .unlockableDefs
                            .Add(
                                new[]
                                {
                                    customUnlock
                                }
                            );


                        UsuLog.Verbose(
                            _logger,
                            $"UnlockableDef USU preparado en " +
                            $"Dynamic ContentPack | " +
                            $"{customUnlock.cachedName}"
                        );
                    }
                }


                bool originalAvailable =
                    SurvivorUnlockManager
                        .HasOriginalUnlockAvailable(
                            survivor,
                            entry
                        );


                bool useCustomUnlock =
                    SurvivorUnlockManager
                        .RequiresCustomUnlock(
                            entry,
                            originalAvailable
                        );


                if (!useCustomUnlock)
                {
                    SurvivorUnlockManager
                        .RestoreOriginalUnlock(
                            survivor
                        );


                    if (
                        originalAvailable &&
                        _loggedOriginalUnlocks.Add(
                            bodyName
                        )
                    )
                    {
                        UnlockableDef originalUnlock;

                        SurvivorUnlockManager
                            .TryGetRememberedOriginalUnlock(
                                survivor,
                                out originalUnlock
                            );


                        UsuLog.Verbose(
                            _logger,
                            $"UNLOCK ORIGINAL ACTIVO | " +
                            $"Body: {bodyName} | " +
                            $"Pack: {packIdentifier} | " +
                            $"Unlock: " +
                            $"{originalUnlock?.cachedName ?? "<free>"}"
                        );
                    }


                    continue;
                }


                SurvivorUnlockManager
                    .AssignEarlyCustomUnlock(
                        survivor,
                        customUnlock,
                        _logger
                    );


                if (
                    _loggedAutoLocks.Add(
                        bodyName
                    )
                )
                {
                    UsuLog.Verbose(
                        _logger,
                        $"PROVIDER USU ACTIVO | " +
                        $"Body: {bodyName} | " +
                        $"Pack: {packIdentifier}"
                    );
                }
            }
        }


        // =========================================================
        // FINALIZE
        // =========================================================

        public IEnumerator FinalizeAsync(
            FinalizeAsyncArgs args
        )
        {
            _logger.LogInfo(
                $"Universal ContentPack finalizado | " +
                $"Survivors modded detectados: " +
                $"{ModdedSurvivorRegistry.Count} | " +
                $"Unlockables dinámicos: " +
                $"{_dynamicUnlockBodies.Count}"
            );


            args.ReportProgress(
                1f
            );


            yield break;
        }
    }
}