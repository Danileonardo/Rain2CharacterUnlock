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
            Plugin.PluginGuid +
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


            /*
             * Aquí YA NO analizamos survivors.
             *
             * Solamente registramos nuestro
             * IContentPackProvider.
             */
            ContentManager.collectContentPackProviders +=
                CollectContentPackProviders;


            logger.LogInfo(
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


            _logger.LogInfo(
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


                // =================================================
                // EL AUTOR TIENE SU PROPIO UNLOCK
                // =================================================

                if (
                    currentUnlock != null &&
                    !SurvivorUnlockManager
                        .IsCustomUnlock(
                            currentUnlock
                        )
                )
                {
                    /*
                     * IMPORTANTE:
                     *
                     * Puede ocurrir que en una pasada
                     * anterior todavía fuese null y
                     * el autor lo asigne después.
                     *
                     * RememberOriginalUnlock debe poder
                     * actualizar null -> unlock real.
                     */
                    SurvivorUnlockManager
                        .RememberOriginalUnlock(
                            survivor,
                            currentUnlock
                        );


                    if (
                        _loggedOriginalUnlocks.Add(
                            bodyName
                        )
                    )
                    {
                        _logger.LogInfo(
                            $"UNLOCK ORIGINAL RESPETADO | " +
                            $"Body: {bodyName} | " +
                            $"Pack: {packIdentifier} | " +
                            $"Unlock: " +
                            $"{currentUnlock.cachedName}"
                        );
                    }


                    continue;
                }


                // =================================================
                // OBTENER / CREAR CONFIGURACIÓN
                // =================================================

                SurvivorJsonEntry entry =
                    SurvivorJsonManager
                        .GetEntryAnywhere(
                            bodyName
                        );


                /*
                 * Primera vez que este survivor
                 * aparece en el equipo.
                 */
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
                // CONFIGURACIÓN DESACTIVADA
                // =================================================

                if (
                    !SurvivorUnlockManager
                        .RequiresCustomUnlock(
                            entry
                        )
                )
                {
                    /*
                     * Si durante una pasada previa
                     * habíamos asignado nuestro unlock,
                     * restauramos el original.
                     */
                    SurvivorUnlockManager
                        .RestoreOriginalUnlock(
                            survivor
                        );


                    continue;
                }


                // =================================================
                // BUSCAR / CREAR CUSTOM UNLOCK
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
                            $"No fue posible crear " +
                            $"el unlock dinámico de " +
                            $"{bodyName}."
                        );


                        continue;
                    }


                    /*
                     * Este UnlockableDef nació después
                     * de que R2API construyera sus packs.
                     *
                     * Por eso lo añadimos al ContentPack
                     * perteneciente a nuestro propio provider.
                     */
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


                        _logger.LogInfo(
                            $"UnlockableDef añadido al " +
                            $"Dynamic ContentPack | " +
                            $"{customUnlock.cachedName}"
                        );
                    }
                }


                // =================================================
                // RECORDAR ORIGINAL
                // =================================================

                SurvivorUnlockManager
                    .RememberOriginalUnlock(
                        survivor,
                        null
                    );


                // =================================================
                // ASIGNAR CUSTOM UNLOCK
                // =================================================

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
                    _logger.LogInfo(
                        $"AUTOLOCK ACTIVADO | " +
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