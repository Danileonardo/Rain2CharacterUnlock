using System;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Fuente única para resolver la política de providers de survivors.
    ///
    /// Mantiene separadas tres preguntas que antes estaban repetidas entre
    /// UI, MissionAssignmentService y SurvivorUnlockManager:
    /// 1) si existe evidencia de un Original;
    /// 2) qué provider es efectivo en este momento;
    /// 3) si USU/Community deben tratarse como la familia USU en la UI.
    ///
    /// Esta clase NO asigna misiones ni toca runtime. Sólo resuelve política.
    /// </summary>
    public static class UnlockProviderPolicy
    {
        /// <summary>
        /// Determina si Survivors.json conserva evidencia de un unlock original
        /// real perteneciente al creador del survivor.
        /// </summary>
        public static bool HasStoredOriginalUnlock(
            SurvivorJsonEntry entry
        )
        {
            if (entry == null)
            {
                return false;
            }

            string value =
                entry.OriginalUnlock?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return
                !string.Equals(
                    value,
                    "Ninguno",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                !string.Equals(
                    value,
                    "None",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                !string.Equals(
                    value,
                    "Null",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        /// <summary>
        /// Indica si una fuente observada por ContentProfiles representa un
        /// provider Original utilizable. UnlockedByDefault también pertenece al
        /// comportamiento Original: simplemente no exige achievement.
        /// </summary>
        public static bool IsOriginalContentSource(
            SurvivorContentUnlockSource source
        )
        {
            return
                source == SurvivorContentUnlockSource.Original ||
                source == SurvivorContentUnlockSource.UnlockedByDefault;
        }


        /// <summary>
        /// Resuelve disponibilidad Original combinando persistencia y la fuente
        /// observada en runtime por ContentProfiles.
        /// </summary>
        public static bool IsOriginalAvailable(
            SurvivorJsonEntry entry,
            SurvivorContentUnlockSource contentSource
        )
        {
            return
                HasStoredOriginalUnlock(entry) ||
                IsOriginalContentSource(contentSource);
        }


        /// <summary>
        /// Resuelve el provider efectivo usando únicamente la información de
        /// persistencia y una disponibilidad Original ya calculada por el caller.
        ///
        /// Reglas preservadas:
        /// - Provider Original siempre devuelve Original.
        /// - AutomaticFallback cede al Original cuando existe.
        /// - UserSelected USU/Community/Custom conserva la elección explícita.
        /// - Configuraciones antiguas sin MissionConfig respetan Original si existe.
        /// </summary>
        public static UnlockProviderKind ResolveEffectiveProvider(
            SurvivorJsonEntry entry,
            bool originalAvailable
        )
        {
            MissionConfiguration missionConfig =
                entry?.Challenge?.MissionConfig;

            if (missionConfig != null)
            {
                if (
                    missionConfig.EffectiveSelectionMode ==
                        UnlockSelectionMode.AutomaticFallback &&
                    originalAvailable
                )
                {
                    return UnlockProviderKind.Original;
                }

                return missionConfig.EffectiveProvider;
            }

            return originalAvailable
                ? UnlockProviderKind.Original
                : UnlockProviderKind.USU;
        }


        /// <summary>
        /// Variante usada por la UI de ContentProfiles. Reproduce exactamente
        /// la precedencia histórica del navegador embebido:
        /// - UnlockedByDefault siempre es Original.
        /// - Si existe MissionConfig, AutomaticFallback sólo cede ante la
        ///   evidencia persistida de OriginalUnlock.
        /// - Sin MissionConfig, la fuente Original observada por el perfil sí
        ///   basta para representar Original.
        ///
        /// La distinción es deliberada para no cambiar comportamiento durante
        /// esta fase de limpieza.
        /// </summary>
        public static UnlockProviderKind ResolveEffectiveProvider(
            SurvivorJsonEntry entry,
            SurvivorContentUnlockSource contentSource
        )
        {
            if (
                contentSource ==
                    SurvivorContentUnlockSource.UnlockedByDefault
            )
            {
                return UnlockProviderKind.Original;
            }

            MissionConfiguration missionConfig =
                entry?.Challenge?.MissionConfig;

            if (missionConfig != null)
            {
                if (
                    missionConfig.EffectiveSelectionMode ==
                        UnlockSelectionMode.AutomaticFallback &&
                    HasStoredOriginalUnlock(entry)
                )
                {
                    return UnlockProviderKind.Original;
                }

                return missionConfig.EffectiveProvider;
            }

            if (HasStoredOriginalUnlock(entry))
            {
                return UnlockProviderKind.Original;
            }

            if (
                contentSource ==
                    SurvivorContentUnlockSource.Original
            )
            {
                return UnlockProviderKind.Original;
            }

            return UnlockProviderKind.USU;
        }


        /// <summary>
        /// Convierte la evidencia persistida de OriginalUnlock al modelo de
        /// restauración. UnlockedByDefault requiere información runtime y por
        /// eso no puede inferirse únicamente desde Survivors.json.
        /// </summary>
        public static OriginalUnlockState ResolveStoredOriginalState(
            SurvivorJsonEntry entry
        )
        {
            return HasStoredOriginalUnlock(entry)
                ? OriginalUnlockState.HasUnlockSystem
                : OriginalUnlockState.Unknown;
        }


        /// <summary>
        /// Provider base que debe quedar guardado tras "Restaurar original".
        /// AutomaticFallback seguirá decidiendo en runtime cuál queda activo.
        /// </summary>
        public static UnlockProviderKind ResolveProviderAfterRestore(
            OriginalUnlockState originalState
        )
        {
            return
                originalState == OriginalUnlockState.HasUnlockSystem ||
                originalState == OriginalUnlockState.UnlockedByDefault
                    ? UnlockProviderKind.Original
                    : UnlockProviderKind.USU;
        }


        /// <summary>
        /// USU y Community comparten presentación/estado activo en la UI actual.
        /// CUSTOM se mantiene separado.
        /// </summary>
        public static bool IsUsuFamily(
            UnlockProviderKind provider
        )
        {
            return
                provider == UnlockProviderKind.USU ||
                provider == UnlockProviderKind.Community;
        }
    }
}
