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
        // REGISTRAR CONFIGURACIONES YA EXISTENTES
        // =========================================================

        public static void RegisterConfiguredUnlockables(
            ManualLogSource logger
        )
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (config == null)
            {
                return;
            }


            RegisterEntries(
                config.AvailableSurvivors,
                logger
            );


            /*
             * También registramos unavailable.
             *
             * Esto es importante si un mod fue
             * desinstalado y luego vuelve a instalarse.
             */
            RegisterEntries(
                config.UnavailableSurvivors,
                logger
            );
        }


        private static void RegisterEntries(
            Dictionary<
                string,
                SurvivorJsonEntry
            > entries,
            ManualLogSource logger
        )
        {
            if (entries == null)
            {
                return;
            }


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in entries
            )
            {
                string bodyName =
                    pair.Key;


                SurvivorJsonEntry entry =
                    pair.Value;


                if (!CanPrepareCustomUnlock(entry))
                {
                    continue;
                }


                if (
                    CustomUnlockables.ContainsKey(
                        bodyName
                    )
                )
                {
                    continue;
                }


                RegisterOneUnlockable(
                    bodyName,
                    entry,
                    logger,
                    true
                );
            }
        }


        // =========================================================
        // REGISTRAR UNLOCK DINÁMICO
        // =========================================================

        public static UnlockableDef RegisterDynamicUnlockable(
            string bodyName,
            SurvivorJsonEntry entry,
            ManualLogSource logger
        )
        {
            if (
                CustomUnlockables.TryGetValue(
                    bodyName,
                    out UnlockableDef existing
                )
            )
            {
                return existing;
            }


            /*
             * false:
             *
             * NO usar ContentAddition porque en este
             * momento R2API ya construyó sus ContentPacks.
             *
             * UniversalContentPackProvider añadirá este
             * UnlockableDef a nuestro ContentPack dinámico.
             */
            return RegisterOneUnlockable(
                bodyName,
                entry,
                logger,
                false
            );
        }


        // =========================================================
        // BUSCAR UNLOCK
        // =========================================================

        public static bool TryGetCustomUnlockable(
            string bodyName,
            out UnlockableDef unlockable
        )
        {
            return CustomUnlockables.TryGetValue(
                bodyName,
                out unlockable
            );
        }

        // =========================================================
        // BUSCAR ACHIEVEMENT USU
        // =========================================================

        public static bool TryGetCustomAchievement(
            string bodyName,
            out AchievementDef achievementDef
        )
        {
            return CustomAchievements.TryGetValue(
                bodyName,
                out achievementDef
            );
        }
    }
}
