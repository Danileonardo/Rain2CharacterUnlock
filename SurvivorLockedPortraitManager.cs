using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using RoR2.UI;
using UnityEngine;

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
             * No tocamos personajes cuyo unlock
             * pertenece a vanilla u otro mod.
             */
            if (
                !SurvivorUnlockManager
                    .IsCustomUnlock(
                        unlockable
                    )
            )
            {
                RestoreColor(self);
                return;
            }

            LocalUser localUser =
                LocalUserManager
                    .GetFirstLocalUser();

            if (
                localUser == null ||
                localUser.userProfile == null
            )
            {
                return;
            }

            bool unlocked =
                localUser
                    .userProfile
                    .HasUnlockable(
                        unlockable
                    );

            if (unlocked)
            {
                RestoreColor(self);
                return;
            }

            /*
             * Guardamos el color original.
             */
            if (
                !OriginalColors
                    .ContainsKey(self)
            )
            {
                OriginalColors[self] =
                    self.survivorIcon.color;
            }

            /*
             * Negro real.
             *
             * Conservamos el alpha de la textura,
             * así que sólo queda la silueta.
             */
            Color original =
                OriginalColors[self];

            self.survivorIcon.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    original.a
                );

            Logger.LogInfo(
                $"Silueta bloqueada aplicada: " +
                $"{self.survivorDef.cachedName}"
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
                OriginalColors.TryGetValue(
                    self,
                    out Color original
                )
            )
            {
                self.survivorIcon.color =
                    original;

                OriginalColors.Remove(
                    self
                );
            }
        }
    }
}