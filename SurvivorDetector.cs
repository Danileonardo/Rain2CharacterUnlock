using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorDetector
    {
        public static List<SurvivorInfo> DetectSurvivors(
            ManualLogSource logger
        )
        {
            List<SurvivorInfo> survivors =
                new List<SurvivorInfo>();

            if (SurvivorCatalog.survivorDefs == null)
            {
                logger.LogWarning(
                    "SurvivorCatalog.survivorDefs todavía no está disponible."
                );

                return survivors;
            }

            logger.LogInfo(
                $"Survivors encontrados en SurvivorCatalog: " +
                $"{SurvivorCatalog.survivorDefs.Length}"
            );

            foreach (
                SurvivorDef survivor
                in SurvivorCatalog.survivorDefs
            )
            {
                if (survivor == null)
                {
                    continue;
                }

                SurvivorInfo info =
                    BuildSurvivorInfo(
                        survivor,
                        logger
                    );

                if (info != null)
                {
                    survivors.Add(
                        info
                    );
                }
            }

            PrintResults(
                survivors,
                logger
            );

            return survivors;
        }


        // =========================================================
        // CONSTRUIR INFORMACIÓN DEL SURVIVOR
        // =========================================================

        private static SurvivorInfo BuildSurvivorInfo(
            SurvivorDef survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null)
            {
                return null;
            }


            // -----------------------------------------------------
            // NOMBRE INTERNO
            // -----------------------------------------------------

            string internalName =
                !string.IsNullOrWhiteSpace(
                    survivor.cachedName
                )
                    ? survivor.cachedName
                    : "UnknownSurvivor";


            // -----------------------------------------------------
            // NOMBRE MOSTRADO
            // -----------------------------------------------------

            string displayName =
                internalName;

            if (
                !string.IsNullOrWhiteSpace(
                    survivor.displayNameToken
                )
            )
            {
                string translatedName =
                    Language.GetString(
                        survivor.displayNameToken
                    );

                if (
                    !string.IsNullOrWhiteSpace(
                        translatedName
                    )
                )
                {
                    displayName =
                        translatedName;
                }
            }


            // -----------------------------------------------------
            // BODY
            // -----------------------------------------------------

            string bodyName =
                "UnknownBody";

            if (survivor.bodyPrefab != null)
            {
                bodyName =
                    survivor.bodyPrefab.name;
            }


            // -----------------------------------------------------
            // UNLOCK ORIGINAL
            // -----------------------------------------------------

            string unlockableName =
    "Ninguno";


            UnlockableDef detectedUnlock =
                survivor.unlockableDef;


            /*
             * Nuestro propio UnlockableDef no debe
             * contar como un desbloqueo original
             * perteneciente al autor del survivor.
             */
            bool customUnlock =
                SurvivorUnlockManager.IsCustomUnlock(
                    detectedUnlock
                );


            bool hasOriginalUnlock =
                detectedUnlock != null &&
                !customUnlock;


            if (
                hasOriginalUnlock &&
                !string.IsNullOrWhiteSpace(
                    detectedUnlock.cachedName
                )
            )
            {
                unlockableName =
                    detectedUnlock.cachedName;
            }


            // -----------------------------------------------------
            // EXPANSIÓN REQUERIDA
            // -----------------------------------------------------

            ExpansionDef requiredExpansion =
                GetRequiredExpansion(
                    survivor
                );

            string expansionName =
                "Base Game";

            if (requiredExpansion != null)
            {
                expansionName =
                    GetExpansionName(
                        requiredExpansion
                    );
            }


            // -----------------------------------------------------
            // ¿PERTENECE A UN MOD?
            // -----------------------------------------------------

            bool isModded =
                ModdedSurvivorRegistry.TryGetSource(
                    survivor,
                    out string contentPackIdentifier,
                    out string sourceAssembly
                );


            // -----------------------------------------------------
            // ESTADO
            // -----------------------------------------------------

            SurvivorStatus status =
                DetermineStatus(
                    survivor,
                    requiredExpansion
                );


            SurvivorInfo info =
                new SurvivorInfo
                {
                    SurvivorDef =
                        survivor,

                    InternalName =
                        internalName,

                    DisplayName =
                        displayName,

                    BodyName =
                        bodyName,

                    UnlockableName =
                        unlockableName,

                    RequiredExpansion =
                        requiredExpansion,

                    ExpansionName =
                        expansionName,

                    Status =
                        status,

                    IsModded =
                        isModded,

                    HasOriginalUnlock =
                        hasOriginalUnlock,

                    ContentPackIdentifier =
                        contentPackIdentifier,

                    SourceAssembly =
                        sourceAssembly
                };


            return info;
        }


        // =========================================================
        // OBTENER EXPANSIÓN DEL SURVIVOR
        // =========================================================

        private static ExpansionDef GetRequiredExpansion(
            SurvivorDef survivor
        )
        {
            if (
                survivor == null ||
                survivor.bodyPrefab == null
            )
            {
                return null;
            }

            ExpansionRequirementComponent requirement =
                survivor
                    .bodyPrefab
                    .GetComponent<
                        ExpansionRequirementComponent
                    >();

            if (requirement == null)
            {
                return null;
            }

            return requirement.requiredExpansion;
        }


        // =========================================================
        // OBTENER NOMBRE DE EXPANSIÓN
        // =========================================================

        private static string GetExpansionName(
            ExpansionDef expansion
        )
        {
            if (expansion == null)
            {
                return "Base Game";
            }

            if (
                !string.IsNullOrWhiteSpace(
                    expansion.nameToken
                )
            )
            {
                string translatedName =
                    Language.GetString(
                        expansion.nameToken
                    );

                if (
                    !string.IsNullOrWhiteSpace(
                        translatedName
                    )
                )
                {
                    return translatedName;
                }
            }

            if (
                !string.IsNullOrWhiteSpace(
                    expansion.name
                )
            )
            {
                return expansion.name;
            }

            return "Unknown Expansion";
        }


        // =========================================================
        // COMPROBAR SI POSEEMOS LA EXPANSIÓN
        // =========================================================

        private static bool OwnsExpansion(
            ExpansionDef expansion
        )
        {
            /*
             * Sin expansión requerida =
             * contenido base / libre.
             */
            if (expansion == null)
            {
                return true;
            }

            /*
             * Algunas expansiones creadas por mods
             * pueden no tener entitlement.
             */
            if (
                expansion.requiredEntitlement == null
            )
            {
                return true;
            }

            if (
                EntitlementManager
                    .localUserEntitlementTracker
                == null
            )
            {
                return false;
            }

            return
                ((BaseUserEntitlementTracker<LocalUser>)
                    EntitlementManager
                        .localUserEntitlementTracker)
                .AnyUserHasEntitlement(
                    expansion.requiredEntitlement
                );
        }


        // =========================================================
        // DETERMINAR ESTADO
        // =========================================================

        private static SurvivorStatus DetermineStatus(
            SurvivorDef survivor,
            ExpansionDef requiredExpansion
        )
        {
            /*
             * Ejemplo vanilla:
             * Heretic.
             *
             * Los hidden se excluyen posteriormente
             * del JSON configurable.
             */
            if (survivor.hidden)
            {
                return SurvivorStatus.Hidden;
            }


            /*
             * Un SurvivorDef sin Body no puede funcionar
             * como survivor seleccionable normal.
             */
            if (survivor.bodyPrefab == null)
            {
                return SurvivorStatus.NotSelectable;
            }


            /*
             * Survivor perteneciente a DLC que
             * este jugador no posee.
             */
            if (
                requiredExpansion != null &&
                !OwnsExpansion(
                    requiredExpansion
                )
            )
            {
                return SurvivorStatus.DlcNotOwned;
            }


            return SurvivorStatus.Available;
        }


        // =========================================================
        // IMPRIMIR RESULTADOS
        // =========================================================

        private static void PrintResults(
            List<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            int availableCount = 0;
            int dlcCount = 0;
            int hiddenCount = 0;
            int notSelectableCount = 0;
            int moddedCount = 0;


            logger.LogInfo(
                "==============================================="
            );

            logger.LogInfo(
                "========== UNIVERSAL SURVIVOR DETECTOR =========="
            );


            foreach (
                SurvivorInfo survivor
                in survivors
            )
            {
                switch (survivor.Status)
                {
                    case SurvivorStatus.Available:
                        availableCount++;
                        break;

                    case SurvivorStatus.DlcNotOwned:
                        dlcCount++;
                        break;

                    case SurvivorStatus.Hidden:
                        hiddenCount++;
                        break;

                    case SurvivorStatus.NotSelectable:
                        notSelectableCount++;
                        break;
                }


                if (survivor.IsModded)
                {
                    moddedCount++;
                }


                logger.LogInfo(
                    $"{survivor.DisplayName} | " +
                    $"Internal: {survivor.InternalName} | " +
                    $"Body: {survivor.BodyName} | " +
                    $"Estado: {survivor.Status} | " +
                    $"Expansion: {survivor.ExpansionName} | " +
                    $"Unlock: {survivor.UnlockableName} | " +
                    $"Modded: {(survivor.IsModded ? "SI" : "NO")}"
                );
            }


            logger.LogInfo(
                "-----------------------------------------------"
            );

            logger.LogInfo(
                $"Disponibles: {availableCount}"
            );

            logger.LogInfo(
                $"DLC no disponible: {dlcCount}"
            );

            logger.LogInfo(
                $"Ocultos: {hiddenCount}"
            );

            logger.LogInfo(
                $"No seleccionables: {notSelectableCount}"
            );

            logger.LogInfo(
                $"Modded detectados: {moddedCount}"
            );


            // =====================================================
            // LISTADO ESPECÍFICO DE SURVIVORS MODDED
            // =====================================================

            logger.LogInfo(
                "==============================================="
            );

            logger.LogInfo(
                "========== SURVIVORS MODDED =========="
            );


            foreach (
                SurvivorInfo survivor
                in survivors
            )
            {
                if (!survivor.IsModded)
                {
                    continue;
                }

                logger.LogInfo(
                    $"{survivor.DisplayName} | " +
                    $"Internal: {survivor.InternalName} | " +
                    $"Body: {survivor.BodyName} | " +
                    $"Pack: {survivor.ContentPackIdentifier} | " +
                    $"Assembly: {survivor.SourceAssembly} | " +
                    $"Unlock propio: " +
                    $"{(survivor.HasOriginalUnlock ? "SI" : "NO")} | " +
                    $"Unlock: {survivor.UnlockableName}"
                );
            }


            if (moddedCount == 0)
            {
                logger.LogInfo(
                    "No se detectaron survivors pertenecientes a ContentPacks de mods."
                );
            }


            logger.LogInfo(
                "==============================================="
            );
        }
    }
}