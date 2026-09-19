using System;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static partial class SurvivorUnlockManager
    {
        // =========================================================
        // ¿ES NUESTRO UNLOCK?
        // =========================================================

        public static bool IsCustomUnlock(
            UnlockableDef unlockableDef
        )
        {
            if (unlockableDef == null)
            {
                return false;
            }

            return
                unlockableDef.cachedName != null &&
                unlockableDef.cachedName.StartsWith(
                    "UniversalSurvivorUnlocks.",
                    StringComparison.Ordinal
                );
        }


        // =========================================================
        // ¿LA CONFIGURACIÓN REQUIERE UNLOCK?
        // =========================================================

        /// <summary>
        /// Indica si USU debe controlar actualmente el unlock según la
        /// configuración persistida.
        /// </summary>
        public static bool RequiresCustomUnlock(
            SurvivorJsonEntry entry
        )
        {
            return RequiresCustomUnlock(
                entry,
                UnlockProviderPolicy.HasStoredOriginalUnlock(entry)
            );
        }


        /// <summary>
        /// Variante usada durante detección temprana. La decisión de provider
        /// efectivo vive en UnlockProviderPolicy para que runtime y ambas UIs
        /// compartan exactamente la misma regla.
        /// </summary>
        public static bool RequiresCustomUnlock(
            SurvivorJsonEntry entry,
            bool originalUnlockAvailable
        )
        {
            if (
                entry == null ||
                entry.Challenge == null ||
                !entry.Challenge.Enabled
            )
            {
                return false;
            }

            bool originalAvailable =
                originalUnlockAvailable ||
                UnlockProviderPolicy.HasStoredOriginalUnlock(entry);

            UnlockProviderKind effectiveProvider =
                UnlockProviderPolicy.ResolveEffectiveProvider(
                    entry,
                    originalAvailable
                );

            if (effectiveProvider == UnlockProviderKind.Original)
            {
                return false;
            }

            bool hasLegacyType =
                !string.IsNullOrWhiteSpace(
                    entry.Challenge.Type
                ) &&
                !string.Equals(
                    entry.Challenge.Type,
                    "Original",
                    StringComparison.OrdinalIgnoreCase
                );

            bool hasMissionV2 =
                entry.Challenge.Mission != null &&
                entry.Challenge.Mission.Routes != null &&
                entry.Challenge.Mission.Routes.Count > 0;

            return
                hasLegacyType ||
                hasMissionV2;
        }


        /// <summary>
        /// Compatibilidad pública con callers existentes. La implementación
        /// real vive en UnlockProviderPolicy.
        /// </summary>
        public static bool HasStoredOriginalUnlock(
            SurvivorJsonEntry entry
        )
        {
            return
                UnlockProviderPolicy.HasStoredOriginalUnlock(
                    entry
                );
        }


        /// <summary>
        /// Un UnlockableDef de USU se prepara en startup aunque esté dormido.
        /// Esto hace posible Original -> USU sin intentar registrar contenido
        /// después de que RoR2 haya cerrado sus catálogos.
        /// </summary>
        private static bool CanPrepareCustomUnlock(
            SurvivorJsonEntry entry
        )
        {
            return
                entry != null &&
                entry.Challenge != null;
        }
    }
}
