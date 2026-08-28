using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorLockedPortraitManager
    {
        private static bool initialized;

        private static ManualLogSource Logger;

        private static readonly Dictionary<
            SurvivorIconController,
            Color
        > OriginalColors =
            new Dictionary<
                SurvivorIconController,
                Color
            >();

        public static void Initialize(
            ManualLogSource logger
        )
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            Logger = logger;

            On.RoR2.UI.SurvivorIconController.Rebuild +=
                SurvivorIconController_Rebuild;

            On.RoR2.UI.SurvivorIconController.UpdateAvailability +=
                SurvivorIconController_UpdateAvailability;

            Logger.LogInfo(
                "SurvivorLockedPortraitManager inicializado."
            );
        }

        private static void SurvivorIconController_Rebuild(
            On.RoR2.UI.SurvivorIconController.orig_Rebuild orig,
            SurvivorIconController self
        )
        {
            orig(self);

            RefreshPortrait(
                self
            );
        }

        private static void SurvivorIconController_UpdateAvailability(
            On.RoR2.UI.SurvivorIconController.orig_UpdateAvailability orig,
            SurvivorIconController self
        )
        {
            orig(self);

            RefreshPortrait(
                self
            );
        }

        private static void RefreshPortrait(
            SurvivorIconController self
        )
        {
            if (
                self == null ||
                self.survivorDef == null ||
                self.survivorIcon == null
            )
            {
                return;
            }

            UnlockableDef unlockable =
                self.survivorDef.unlockableDef;

            /*
             * No tocamos unlocks vanilla
             * ni unlocks pertenecientes a otros mods.
             */
            if (
                !SurvivorUnlockManager.IsCustomUnlock(
                    unlockable
                )
            )
            {
                RestoreColor(
                    self
                );

                return;
            }

            /*
             * Usamos el estado que RoR2 ya calculó.
             *
             * Si RealerCheatUnlocks, nuestra quest
             * o cualquier otro sistema concede
             * el UnlockableDef, RoR2 pondrá esto
             * en true.
             */
            if (self.survivorIsUnlocked)
            {
                RestoreColor(
                    self
                );

                return;
            }

            ApplyLockedColor(
                self
            );
        }

        private static void ApplyLockedColor(
            SurvivorIconController self
        )
        {
            if (
                !OriginalColors.ContainsKey(
                    self
                )
            )
            {
                OriginalColors[self] =
                    self.survivorIcon.color;

                Logger.LogInfo(
                    $"Silueta bloqueada aplicada: " +
                    $"{self.survivorDef.cachedName}"
                );
            }

            Color original =
                OriginalColors[self];

            self.survivorIcon.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    original.a
                );
        }

        private static void RestoreColor(
            SurvivorIconController self
        )
        {
            if (
                self == null ||
                self.survivorIcon == null
            )
            {
                return;
            }

            if (
                !OriginalColors.TryGetValue(
                    self,
                    out Color original
                )
            )
            {
                return;
            }

            self.survivorIcon.color =
                original;

            OriginalColors.Remove(
                self
            );

            Logger.LogInfo(
                $"Color restaurado: " +
                $"{self.survivorDef.cachedName}"
            );
        }
    }
}