using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    public static partial class SurvivorUnlockManager
    {
        // =========================================================
        // CONSULTAR UNLOCK ORIGINAL RECORDADO
        // =========================================================

        public static bool TryGetRememberedOriginalUnlock(
            SurvivorDef survivorDef,
            out UnlockableDef originalUnlock
        )
        {
            originalUnlock = null;


            if (survivorDef == null)
            {
                return false;
            }


            return OriginalUnlockables.TryGetValue(
                survivorDef,
                out originalUnlock
            );
        }


        public static bool HasOriginalUnlockAvailable(
            SurvivorDef survivorDef,
            SurvivorJsonEntry entry = null
        )
        {
            if (survivorDef != null)
            {
                UnlockableDef current =
                    survivorDef.unlockableDef;


                if (
                    current != null &&
                    !IsCustomUnlock(current)
                )
                {
                    return true;
                }


                if (
                    OriginalUnlockables.TryGetValue(
                        survivorDef,
                        out UnlockableDef remembered
                    ) &&
                    remembered != null
                )
                {
                    return true;
                }
            }


            return HasStoredOriginalUnlock(entry);
        }


        /// <summary>
        /// Comprueba un desbloqueo REAL del proveedor Original en el perfil.
        /// No presupone que un original existe basándose en Survivors.json:
        /// solo se considera ya obtenido si existe su UnlockableDef y
        /// profile.HasUnlockable(original) es verdadero.
        /// </summary>
        public static bool HasRememberedOriginalUnlockInProfile(
            string bodyName,
            UserProfile profile
        )
        {
            if (profile == null || string.IsNullOrWhiteSpace(bodyName))
            {
                return false;
            }

            SurvivorDef[] survivors = SurvivorCatalog.survivorDefs;
            if (survivors == null)
            {
                return false;
            }

            for (int i = 0; i < survivors.Length; i++)
            {
                SurvivorDef survivor = survivors[i];
                if (survivor?.bodyPrefab == null ||
                    !string.Equals(survivor.bodyPrefab.name, bodyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                UnlockableDef original = null;
                if (!TryGetRememberedOriginalUnlock(survivor, out original) ||
                    original == null)
                {
                    original = survivor.unlockableDef;
                }

                return original != null &&
                    !IsCustomUnlock(original) &&
                    profile.HasUnlockable(original);
            }

            return false;
        }


        // =========================================================
        // RECORDAR UNLOCK ORIGINAL
        // =========================================================

        public static void RememberOriginalUnlock(
    SurvivorDef survivorDef,
    UnlockableDef originalUnlock
)
        {
            if (survivorDef == null)
            {
                return;
            }


            /*
             * Nunca almacenamos nuestro propio
             * unlock como "original".
             */
            if (
                IsCustomUnlock(
                    originalUnlock
                )
            )
            {
                return;
            }


            if (
                !OriginalUnlockables.TryGetValue(
                    survivorDef,
                    out UnlockableDef existing
                )
            )
            {
                OriginalUnlockables[
                    survivorDef
                ] =
                    originalUnlock;

                return;
            }


            /*
             * Caso importante:
             *
             * En una primera pasada el mod todavía
             * no había colocado su unlockableDef:
             *
             * original = null
             *
             * Y en una pasada posterior sí aparece.
             *
             * Debemos actualizar null -> unlock real.
             */
            if (
                existing == null &&
                originalUnlock != null
            )
            {
                OriginalUnlockables[
                    survivorDef
                ] =
                    originalUnlock;
            }
        }

        // =========================================================
        // RESTAURAR UNLOCK ORIGINAL
        // =========================================================

        public static void RestoreOriginalUnlock(
            SurvivorDef survivorDef
        )
        {
            if (survivorDef == null)
            {
                return;
            }


            if (
                OriginalUnlockables.TryGetValue(
                    survivorDef,
                    out UnlockableDef originalUnlock
                )
            )
            {
                survivorDef.unlockableDef =
                    originalUnlock;

                return;
            }


            /*
             * Si no conocemos ningún original
             * y actualmente tiene uno nuestro,
             * significa que originalmente era null.
             */
            if (
                IsCustomUnlock(
                    survivorDef.unlockableDef
                )
            )
            {
                survivorDef.unlockableDef =
                    null;
            }
        }


        // =========================================================
        // ASIGNACIÓN TEMPRANA
        // =========================================================

        public static void AssignEarlyCustomUnlock(
            SurvivorDef survivorDef,
            UnlockableDef customUnlock,
            ManualLogSource logger
        )
        {
            if (
                survivorDef == null ||
                customUnlock == null
            )
            {
                return;
            }


            UnlockableDef current =
                survivorDef.unlockableDef;


            if (
                !OriginalUnlockables.ContainsKey(
                    survivorDef
                )
            )
            {
                OriginalUnlockables[
                    survivorDef
                ] =
                    IsCustomUnlock(current)
                        ? null
                        : current;
            }


            // La construcción de ContentPacks puede volver a visitar
            // el mismo SurvivorDef varias veces durante el arranque.
            // Si nuestro unlock ya está aplicado, la operación es
            // idempotente: no reasignamos ni repetimos el mismo log.
            if (
                ReferenceEquals(
                    current,
                    customUnlock
                ) ||
                (
                    current != null &&
                    !string.IsNullOrWhiteSpace(current.cachedName) &&
                    !string.IsNullOrWhiteSpace(customUnlock.cachedName) &&
                    string.Equals(
                        current.cachedName,
                        customUnlock.cachedName,
                        StringComparison.Ordinal
                    )
                )
            )
            {
                return;
            }


            survivorDef.unlockableDef =
                customUnlock;


            UsuLog.Verbose(
                logger,
                $"Unlock temprano asignado | " +
                $"Survivor: {survivorDef.cachedName} | " +
                $"Unlock: {customUnlock.cachedName}"
            );
        }
    }
}
