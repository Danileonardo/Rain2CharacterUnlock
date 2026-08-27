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
            List<SurvivorInfo> survivorsDetected =
                new List<SurvivorInfo>();

            foreach (SurvivorDef survivor in SurvivorCatalog.survivorDefs)
            {
                if (survivor == null)
                {
                    continue;
                }

                SurvivorInfo info = BuildSurvivorInfo(
                    survivor,
                    logger
                );

                survivorsDetected.Add(info);
            }

            PrintResults(
                survivorsDetected,
                logger
            );

            return survivorsDetected;
        }

        private static SurvivorInfo BuildSurvivorInfo(
            SurvivorDef survivor,
            ManualLogSource logger
        )
        {
            string internalName =
                survivor.cachedName ?? "UnknownSurvivor";

            string displayName =
                Language.GetString(survivor.displayNameToken);

            string bodyName = "Sin Body";

            if (survivor.bodyPrefab != null)
            {
                bodyName = survivor.bodyPrefab.name;
            }

            string unlockableName = "Ninguno";

            if (survivor.unlockableDef != null)
            {
                unlockableName =
                    survivor.unlockableDef.cachedName;
            }

            ExpansionDef requiredExpansion =
                GetRequiredExpansion(survivor);

            string expansionName = "Base Game";

            if (requiredExpansion != null)
            {
                expansionName =
                    Language.GetString(
                        requiredExpansion.nameToken
                    );
            }

            SurvivorStatus status =
                GetSurvivorStatus(
                    survivor,
                    requiredExpansion,
                    logger
                );

            return new SurvivorInfo
            {
                SurvivorDef = survivor,

                InternalName = internalName,

                DisplayName = displayName,

                BodyName = bodyName,

                UnlockableName = unlockableName,

                RequiredExpansion = requiredExpansion,

                ExpansionName = expansionName,

                Status = status
            };
        }

        private static ExpansionDef GetRequiredExpansion(
            SurvivorDef survivor
        )
        {
            if (survivor.bodyPrefab == null)
            {
                return null;
            }

            ExpansionRequirementComponent requirement =
                survivor.bodyPrefab
                    .GetComponent<ExpansionRequirementComponent>();

            if (requirement == null)
            {
                return null;
            }

            return requirement.requiredExpansion;
        }

        private static SurvivorStatus GetSurvivorStatus(
            SurvivorDef survivor,
            ExpansionDef requiredExpansion,
            ManualLogSource logger
        )
        {
            // Survivor oculto.
            if (survivor.hidden)
            {
                return SurvivorStatus.Hidden;
            }

            // Un survivor normal seleccionable necesita
            // Body y representación para la pantalla de selección.
            if (
                survivor.bodyPrefab == null ||
                survivor.displayPrefab == null
            )
            {
                return SurvivorStatus.NotSelectable;
            }

            // Sin expansión requerida:
            // Base Game o survivor de mod sin DLC.
            if (requiredExpansion == null)
            {
                return SurvivorStatus.Available;
            }

            // Algunas expansiones de mods pueden no usar
            // entitlement de Steam.
            if (requiredExpansion.requiredEntitlement == null)
            {
                return SurvivorStatus.Available;
            }

            try
            {
                bool ownsExpansion =
                    EntitlementManager
                        .localUserEntitlementTracker
                        .AnyUserHasEntitlement(
                            requiredExpansion.requiredEntitlement
                        );

                if (!ownsExpansion)
                {
                    return SurvivorStatus.DlcNotOwned;
                }
            }
            catch (System.Exception exception)
            {
                logger.LogWarning(
                    $"No se pudo comprobar el DLC de " +
                    $"{survivor.cachedName}: " +
                    $"{exception.Message}"
                );

                // Si no estamos seguros, no modificamos
                // ese survivor.
                return SurvivorStatus.DlcNotOwned;
            }

            return SurvivorStatus.Available;
        }

        private static void PrintResults(
            List<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            int available = 0;
            int dlcNotOwned = 0;
            int hidden = 0;
            int notSelectable = 0;

            foreach (SurvivorInfo survivor in survivors)
            {
                switch (survivor.Status)
                {
                    case SurvivorStatus.Available:
                        available++;
                        break;

                    case SurvivorStatus.DlcNotOwned:
                        dlcNotOwned++;
                        break;

                    case SurvivorStatus.Hidden:
                        hidden++;
                        break;

                    case SurvivorStatus.NotSelectable:
                        notSelectable++;
                        break;
                }
            }

            logger.LogInfo(
                "========== UNIVERSAL SURVIVOR UNLOCKS =========="
            );

            logger.LogInfo(
                $"Disponibles: {available}"
            );

            logger.LogInfo(
                $"DLC no disponible: {dlcNotOwned}"
            );

            logger.LogInfo(
                $"Ocultos: {hidden}"
            );

            logger.LogInfo(
                $"No seleccionables: {notSelectable}"
            );

            logger.LogInfo(
                "========== DISPONIBLES =========="
            );

            foreach (SurvivorInfo survivor in survivors)
            {
                if (survivor.Status != SurvivorStatus.Available)
                {
                    continue;
                }

                logger.LogInfo(
                    $"{survivor.DisplayName} | " +
                    $"Internal: {survivor.InternalName} | " +
                    $"Body: {survivor.BodyName} | " +
                    $"Origen: {survivor.ExpansionName} | " +
                    $"Unlockable: {survivor.UnlockableName}"
                );
            }

            logger.LogInfo(
                "========== DLC NO DISPONIBLE =========="
            );

            foreach (SurvivorInfo survivor in survivors)
            {
                if (survivor.Status != SurvivorStatus.DlcNotOwned)
                {
                    continue;
                }

                logger.LogInfo(
                    $"{survivor.DisplayName} | " +
                    $"Requiere: {survivor.ExpansionName}"
                );
            }

            logger.LogInfo(
                "========== OCULTOS / NO SELECCIONABLES =========="
            );

            foreach (SurvivorInfo survivor in survivors)
            {
                if (
                    survivor.Status != SurvivorStatus.Hidden &&
                    survivor.Status != SurvivorStatus.NotSelectable
                )
                {
                    continue;
                }

                logger.LogInfo(
                    $"{survivor.DisplayName} | " +
                    $"Internal: {survivor.InternalName} | " +
                    $"Estado: {survivor.Status}"
                );
            }

            logger.LogInfo(
                "==============================================="
            );
        }
    }
}