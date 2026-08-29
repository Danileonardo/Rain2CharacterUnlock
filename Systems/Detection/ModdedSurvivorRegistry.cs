using System;
using System.Collections.Generic;
using BepInEx.Logging;
using R2API.ContentManagement;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    public static class ModdedSurvivorRegistry
    {
        private class ModdedSourceInfo
        {
            public string ContentPackIdentifier =
                "";

            public string AssemblyName =
                "";
        }


        private static readonly Dictionary<
            SurvivorDef,
            ModdedSourceInfo
        > ModdedSurvivors =
            new Dictionary<
                SurvivorDef,
                ModdedSourceInfo
            >();


        /*
         * Relación BodyPrefab -> mod.
         *
         * Sirve como respaldo para providers
         * que registran el Body antes que el
         * SurvivorDef.
         */
        private static readonly Dictionary<
            GameObject,
            ModdedSourceInfo
        > ModdedBodies =
            new Dictionary<
                GameObject,
                ModdedSourceInfo
            >();


        public static int Count =>
            ModdedSurvivors.Count;


        // =========================================================
        // REINICIAR
        // =========================================================

        public static void Reset(
            ManualLogSource logger
        )
        {
            ModdedSurvivors.Clear();

            ModdedBodies.Clear();


            logger.LogInfo(
                "Registro universal de survivors modded reiniciado."
            );
        }


        // =========================================================
        // REGISTRAR UN CONTENT PACK REAL DE ROR2
        // =========================================================

        public static void RegisterContentPack(
            ReadOnlyContentPack contentPack,
            ManualLogSource logger
        )
        {
            string identifier =
                contentPack.identifier;


            if (
                string.IsNullOrWhiteSpace(
                    identifier
                )
            )
            {
                identifier =
                    "UnknownContentPack";
            }


            /*
             * Los ContentPacks oficiales usan
             * identificadores RoR2.*
             *
             * Ejemplos:
             * RoR2.BaseContent
             * RoR2.DLC1
             * RoR2.DLC2
             */
            if (
                identifier.StartsWith(
                    "RoR2.",
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }


            /*
             * Ignoramos nuestro propio pack.
             */
            if (
                identifier.StartsWith(
                    Plugin.PluginGuid,
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }


            string assemblyName =
                ResolveAssemblyName(
                    identifier
                );


            ModdedSourceInfo sourceInfo =
                new ModdedSourceInfo
                {
                    ContentPackIdentifier =
                        identifier,

                    AssemblyName =
                        assemblyName
                };


            // =====================================================
            // BODY PREFABS
            // =====================================================

            foreach (
                GameObject bodyPrefab
                in contentPack.bodyPrefabs
            )
            {
                if (bodyPrefab == null)
                {
                    continue;
                }


                if (
                    !ModdedBodies.ContainsKey(
                        bodyPrefab
                    )
                )
                {
                    ModdedBodies.Add(
                        bodyPrefab,
                        sourceInfo
                    );
                }
            }


            // =====================================================
            // SURVIVOR DEFS
            // =====================================================

            foreach (
                SurvivorDef survivor
                in contentPack.survivorDefs
            )
            {
                if (survivor == null)
                {
                    continue;
                }


                /*
                 * También asociamos su Body al mod,
                 * incluso si bodyPrefabs no lo contenía.
                 */
                if (
                    survivor.bodyPrefab != null &&
                    !ModdedBodies.ContainsKey(
                        survivor.bodyPrefab
                    )
                )
                {
                    ModdedBodies.Add(
                        survivor.bodyPrefab,
                        sourceInfo
                    );
                }


                RegisterSurvivor(
                    survivor,
                    sourceInfo,
                    "PeerContentPack",
                    logger
                );
            }
        }


        // =========================================================
        // CRUZAR CONTRA SURVIVORCATALOG
        // =========================================================

        public static void ReconcileCatalog(
            ManualLogSource logger
        )
        {
            if (
                SurvivorCatalog.survivorDefs ==
                null
            )
            {
                return;
            }


            foreach (
                SurvivorDef survivor
                in SurvivorCatalog.survivorDefs
            )
            {
                if (
                    survivor == null ||
                    survivor.bodyPrefab == null
                )
                {
                    continue;
                }


                if (
                    ModdedSurvivors.ContainsKey(
                        survivor
                    )
                )
                {
                    continue;
                }


                if (
                    !ModdedBodies.TryGetValue(
                        survivor.bodyPrefab,
                        out ModdedSourceInfo sourceInfo
                    )
                )
                {
                    continue;
                }


                RegisterSurvivor(
                    survivor,
                    sourceInfo,
                    "BodyPrefabFallback",
                    logger
                );
            }


            logger.LogInfo(
                $"Survivors modded registrados tras reconciliar catálogo: " +
                $"{ModdedSurvivors.Count}"
            );
        }


        // =========================================================
        // REGISTRAR SURVIVOR
        // =========================================================

        private static void RegisterSurvivor(
            SurvivorDef survivor,
            ModdedSourceInfo sourceInfo,
            string detectionMethod,
            ManualLogSource logger
        )
        {
            if (
                survivor == null ||
                sourceInfo == null
            )
            {
                return;
            }


            if (
                ModdedSurvivors.ContainsKey(
                    survivor
                )
            )
            {
                return;
            }


            ModdedSurvivors.Add(
                survivor,
                sourceInfo
            );


            string bodyName =
                survivor.bodyPrefab != null
                    ? survivor.bodyPrefab.name
                    : "Sin Body";


            logger.LogInfo(
                $"Survivor MOD detectado por " +
                $"{detectionMethod} | " +
                $"{survivor.cachedName} | " +
                $"Body: {bodyName} | " +
                $"Pack: " +
                $"{sourceInfo.ContentPackIdentifier} | " +
                $"Assembly: " +
                $"{sourceInfo.AssemblyName}"
            );
        }


        // =========================================================
        // RESOLVER ASSEMBLY
        // =========================================================

        private static string ResolveAssemblyName(
            string identifier
        )
        {
            try
            {
                var managedPacks =
                    R2APIContentManager
                        .ManagedContentPacks;


                foreach (
                    ManagedReadOnlyContentPack managedPack
                    in managedPacks
                )
                {
                    if (
                        !string.Equals(
                            managedPack.Identifier,
                            identifier,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        continue;
                    }


                    if (
                        managedPack.TiedAssembly !=
                        null
                    )
                    {
                        return managedPack
                            .TiedAssembly
                            .GetName()
                            .Name;
                    }
                }
            }
            catch
            {
                /*
                 * Esto es sólo metadata.
                 *
                 * Nunca debe impedir detectar
                 * un survivor.
                 */
            }


            return identifier;
        }


        // =========================================================
        // OBTENER TODOS
        // =========================================================

        public static List<SurvivorDef>
            GetRegisteredSurvivors()
        {
            return new List<SurvivorDef>(
                ModdedSurvivors.Keys
            );
        }


        // =========================================================
        // OBTENER FUENTE
        // =========================================================

        public static bool TryGetSource(
            SurvivorDef survivor,
            out string contentPackIdentifier,
            out string assemblyName
        )
        {
            contentPackIdentifier =
                "";

            assemblyName =
                "";


            if (survivor == null)
            {
                return false;
            }


            if (
                !ModdedSurvivors.TryGetValue(
                    survivor,
                    out ModdedSourceInfo sourceInfo
                )
            )
            {
                return false;
            }


            contentPackIdentifier =
                sourceInfo.ContentPackIdentifier;


            assemblyName =
                sourceInfo.AssemblyName;


            return true;
        }
    }
}