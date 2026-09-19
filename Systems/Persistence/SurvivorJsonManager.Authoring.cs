using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static partial class SurvivorJsonManager
    {
        // =========================================================
        // SINCRONIZAR PRESETS DURANTE AUTORÍA
        // =========================================================

        private static bool ApplyAuthoringPresets(
            SurvivorJsonFile file,
            ManualLogSource logger
        )
        {
            if (
                file == null ||
                !SurvivorChallengePresets.AuthoringMode
            )
            {
                return false;
            }


            bool changed =
                false;


            changed |=
                ApplyAuthoringPresetsToEntries(
                    file.AvailableSurvivors,
                    logger
                );


            changed |=
                ApplyAuthoringPresetsToEntries(
                    file.UnavailableSurvivors,
                    logger
                );


            return changed;
        }


        // =========================================================
        // SINCRONIZAR UNA COLECCIÓN DE SURVIVORS
        // =========================================================

        private static bool ApplyAuthoringPresetsToEntries(
            Dictionary<
                string,
                SurvivorJsonEntry
            > entries,
            ManualLogSource logger
        )
        {
            if (entries == null)
            {
                return false;
            }


            bool changed =
                false;


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in entries
            )
            {
                SurvivorJsonEntry entry =
                    pair.Value;


                if (entry == null)
                {
                    continue;
                }


                // =================================================
                // PROTEGER ELECCIONES EXPLÍCITAS DEL JUGADOR
                // =================================================
                //
                // AuthoringMode sólo debe mantener actualizados los
                // fallbacks automáticos del autor. Una vez que el jugador
                // selecciona Original, USU o Custom manualmente, reiniciar
                // el juego no puede sobrescribir esa elección.
                // =================================================

                if (
                    entry.Challenge?.MissionConfig != null &&
                    entry.Challenge
                        .MissionConfig
                        .EffectiveSelectionMode ==
                            UnlockSelectionMode.UserSelected
                )
                {
                    continue;
                }


                string bodyName =
                    !string.IsNullOrWhiteSpace(
                        entry.BodyName
                    )
                        ? entry.BodyName
                        : pair.Key;


                string contentPackIdentifier =
                    entry.Source;


                // =================================================
                // ¿USU TIENE UN PRESET PARA ESTE SURVIVOR?
                // =================================================

                if (
                    !SurvivorChallengePresets
                        .TryCreatePreset(
                            bodyName,
                            contentPackIdentifier,
                            out SurvivorChallengeJson preset
                        )
                )
                {
                    continue;
                }


                // =================================================
                // PRESERVAR AUTOMATICFALLBACK -> ORIGINAL
                // =================================================
                //
                // Desde 5G.1D-D, "Restaurar personaje" puede dejar un
                // survivor en Original + AutomaticFallback mientras mantiene
                // su preset USU dormido. AuthoringMode puede actualizar el
                // contenido de ese preset, pero NO debe devolver el provider
                // a USU por accidente al reiniciar el juego.
                // =================================================

                MissionConfiguration currentMissionConfig =
                    entry.Challenge?.MissionConfig;


                if (
                    currentMissionConfig != null &&
                    currentMissionConfig.EffectiveSelectionMode ==
                        UnlockSelectionMode.AutomaticFallback &&
                    currentMissionConfig.EffectiveProvider ==
                        UnlockProviderKind.Original
                )
                {
                    if (preset.MissionConfig == null)
                    {
                        preset.MissionConfig =
                            new MissionConfiguration();
                    }


                    preset.MissionConfig.Provider =
                        UnlockProviderKind.Original;

                    preset.MissionConfig.SelectionMode =
                        UnlockSelectionMode.AutomaticFallback;

                    // RestoreCharacter elimina Custom. AuthoringMode nunca
                    // debe reintroducir una copia personalizada.
                    preset.MissionConfig.CustomMission =
                        null;
                }


                // =================================================
                // YA ESTÁ ACTUALIZADO
                // =================================================

                if (
                    ChallengesAreEqual(
                        entry.Challenge,
                        preset
                    )
                )
                {
                    continue;
                }


                // =================================================
                // ACTUALIZAR DESDE EL PRESET DE AUTORÍA
                // =================================================

                entry.Challenge =
                    preset;


                changed =
                    true;


                UsuLog.Verbose(
                    logger,
                    $"Preset de autoría actualizado | " +
                    $"Body: {bodyName} | " +
                    $"Pack: {contentPackIdentifier} | " +
                    $"Nombre: {preset.Name} | " +
                    $"Tipo: {preset.Type}"
                );
            }


            return changed;
        }


        // =========================================================
        // COMPARAR DOS CHALLENGES
        // =========================================================

        private static bool ChallengesAreEqual(
            SurvivorChallengeJson first,
            SurvivorChallengeJson second
        )
        {
            if (
                first == null &&
                second == null
            )
            {
                return true;
            }


            if (
                first == null ||
                second == null
            )
            {
                return false;
            }


            try
            {
                JToken firstToken =
                    JToken.FromObject(
                        first
                    );


                JToken secondToken =
                    JToken.FromObject(
                        second
                    );


                return JToken.DeepEquals(
                    firstToken,
                    secondToken
                );
            }
            catch
            {
                /*
                 * Si por alguna razón no se pueden
                 * comparar, preferimos asumir que
                 * son diferentes durante autoría.
                 */
                return false;
            }
        }


        // =========================================================
        // BUSCAR UN SURVIVOR EN TODO EL JSON
        // =========================================================

    }
}
