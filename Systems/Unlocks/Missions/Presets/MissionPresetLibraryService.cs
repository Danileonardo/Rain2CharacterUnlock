using System;
using System.Collections.Generic;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Capa de acceso a la biblioteca de presets de misión.
    ///
    /// BuiltInMissionPresetCatalog sigue siendo la fuente real.
    ///
    /// Reglas de biblioteca:
    /// - Mission + RuntimeSupported + !IsLegacy = asignable.
    /// - Mission + IsLegacy = compatibilidad/migración, no se muestra
    ///   como una misión nueva asignable.
    /// - Objective / Condition permanecen disponibles como piezas
    ///   para el futuro editor.
    ///
    /// El catálogo usa factories, así que todas las entradas devueltas
    /// son copias nuevas y el preset del creador nunca se modifica.
    /// </summary>
    public static class MissionPresetLibraryService
    {
        // =========================================================
        // MISIONES ASIGNABLES
        // =========================================================

        public static List<MissionPreset>
            GetAssignableMissionPresets()
        {
            List<MissionPreset> all =
                BuiltInMissionPresetCatalog
                    .GetByKind(
                        MissionPresetKinds.Mission
                    );


            List<MissionPreset> result =
                new List<MissionPreset>();


            if (all == null)
            {
                return result;
            }


            for (
                int i = 0;
                i < all.Count;
                i++
            )
            {
                MissionPreset preset =
                    all[i];


                if (
                    !IsAssignable(
                        preset
                    )
                )
                {
                    continue;
                }


                result.Add(
                    preset
                );
            }


            SortByName(
                result
            );


            return result;
        }


        // =========================================================
        // PRESETS OFICIALES ACTUALES
        // =========================================================
        //
        // CharacterRecipe + no legacy.
        //
        // Actualmente corresponde a las 9 misiones oficiales de USU.
        // =========================================================

        public static List<MissionPreset>
            GetOfficialMissionPresets()
        {
            List<MissionPreset> all =
                GetAssignableMissionPresets();


            List<MissionPreset> result =
                new List<MissionPreset>();


            for (
                int i = 0;
                i < all.Count;
                i++
            )
            {
                MissionPreset preset =
                    all[i];


                if (
                    preset == null
                )
                {
                    continue;
                }


                if (
                    string.Equals(
                        preset.Category,
                        MissionPresetCategories
                            .CharacterRecipe,
                        StringComparison
                            .OrdinalIgnoreCase
                    )
                )
                {
                    result.Add(
                        preset
                    );
                }
            }


            SortByName(
                result
            );


            return result;
        }


        // =========================================================
        // COMPATIBILIDAD CON EL NOMBRE ANTERIOR DEL MÉTODO
        // =========================================================

        public static List<MissionPreset>
            GetCharacterRecipePresets()
        {
            return GetOfficialMissionPresets();
        }


        // =========================================================
        // RECETAS LEGACY
        // =========================================================
        //
        // Se conservan en BuiltInMissionPresetCatalog para:
        // - compatibilidad;
        // - migraciones;
        // - referencia histórica durante el desarrollo.
        //
        // No deben aparecer como misiones nuevas asignables.
        // =========================================================

        public static List<MissionPreset>
            GetLegacyMissionPresets()
        {
            List<MissionPreset> all =
                BuiltInMissionPresetCatalog
                    .GetByKind(
                        MissionPresetKinds.Mission
                    );


            List<MissionPreset> result =
                new List<MissionPreset>();


            if (all == null)
            {
                return result;
            }


            for (
                int i = 0;
                i < all.Count;
                i++
            )
            {
                MissionPreset preset =
                    all[i];


                if (
                    preset != null &&
                    preset.IsLegacy
                )
                {
                    result.Add(
                        preset
                    );
                }
            }


            SortByName(
                result
            );


            return result;
        }


        // =========================================================
        // PIEZAS PARA EL FUTURO EDITOR
        // =========================================================

        public static List<MissionPreset>
            GetObjectiveTemplates()
        {
            List<MissionPreset> result =
                BuiltInMissionPresetCatalog
                    .GetByKind(
                        MissionPresetKinds.Objective
                    );


            SortByName(
                result
            );


            return result;
        }


        public static List<MissionPreset>
            GetConditionTemplates()
        {
            List<MissionPreset> result =
                BuiltInMissionPresetCatalog
                    .GetByKind(
                        MissionPresetKinds.Condition
                    );


            SortByName(
                result
            );


            return result;
        }


        // =========================================================
        // BUSCAR UNA MISIÓN ASIGNABLE
        // =========================================================

        public static bool
            TryGetAssignableMissionPreset(
                string presetId,
                out MissionPreset preset
            )
        {
            preset =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    presetId
                )
            )
            {
                return false;
            }


            MissionPreset candidate =
                BuiltInMissionPresetCatalog
                    .Get(
                        presetId
                    );


            if (
                !IsAssignable(
                    candidate
                )
            )
            {
                return false;
            }


            preset =
                candidate;


            return true;
        }


        // =========================================================
        // VALIDACIÓN
        // =========================================================

        public static bool IsAssignable(
            MissionPreset preset
        )
        {
            if (preset == null)
            {
                return false;
            }


            if (
                !string.Equals(
                    preset.Kind,
                    MissionPresetKinds.Mission,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }


            /*
             * Las recetas históricas siguen resolviéndose desde
             * BuiltInMissionPresetCatalog, pero no se ofrecen para
             * nuevas asignaciones desde la biblioteca.
             */
            if (
                preset.IsLegacy
            )
            {
                return false;
            }


            if (
                !preset.RuntimeSupported
            )
            {
                return false;
            }


            if (
                preset.Mission == null ||
                preset.Mission.Routes == null ||
                preset.Mission.Routes.Count == 0
            )
            {
                return false;
            }


            return true;
        }


        // =========================================================
        // DIAGNÓSTICO
        // =========================================================

        public static void LogSummary(
            ManualLogSource logger
        )
        {
            if (logger == null)
            {
                return;
            }


            List<MissionPreset> assignable =
                GetAssignableMissionPresets();


            List<MissionPreset> official =
                GetOfficialMissionPresets();


            List<MissionPreset> legacy =
                GetLegacyMissionPresets();


            List<MissionPreset> objectives =
                GetObjectiveTemplates();


            List<MissionPreset> conditions =
                GetConditionTemplates();


            logger.LogInfo(
                "[PRESET LIBRARY] Biblioteca preparada | " +
                $"Misiones asignables: " +
                $"{assignable.Count} | " +
                $"Presets oficiales: " +
                $"{official.Count} | " +
                $"Legacy ocultas: " +
                $"{legacy.Count} | " +
                $"Objetivos: " +
                $"{objectives.Count} | " +
                $"Condiciones: " +
                $"{conditions.Count}"
            );
        }


        // =========================================================
        // ORDEN ESTABLE
        // =========================================================

        private static void SortByName(
            List<MissionPreset> presets
        )
        {
            if (
                presets == null ||
                presets.Count <= 1
            )
            {
                return;
            }


            presets.Sort(
                delegate(
                    MissionPreset first,
                    MissionPreset second
                )
                {
                    string firstName =
                        first?.Name ?? "";


                    string secondName =
                        second?.Name ?? "";


                    return string.Compare(
                        firstName,
                        secondName,
                        StringComparison.OrdinalIgnoreCase
                    );
                }
            );
        }
    }
}
