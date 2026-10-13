using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Runtime-only compatibility audit for official content introduced by
    /// newer Risk of Rain 2 builds. It never changes unlock authority,
    /// assignments, persistence, or generated missions.
    /// </summary>
    public static class CurrentGameCompatibilityService
    {
        private sealed class OfficialSurvivorExpectation
        {
            public string BodyName = "";
            public string Label = "";
        }

        private static readonly OfficialSurvivorExpectation[] Dlc4Survivors =
        {
            new OfficialSurvivorExpectation
            {
                BodyName = "Enforcer2Body",
                Label = "Enforcer"
            },
            new OfficialSurvivorExpectation
            {
                BodyName = "AcolyteBody",
                Label = "Acolyte"
            }
        };

        public static void Audit(
            IReadOnlyList<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            if (survivors == null)
            {
                return;
            }

            int detected = 0;
            int originalUnlocks = 0;
            int classificationErrors = 0;

            foreach (OfficialSurvivorExpectation expected in Dlc4Survivors)
            {
                SurvivorInfo survivor = FindByBodyName(
                    survivors,
                    expected.BodyName
                );

                if (survivor == null)
                {
                    UsuLog.Verbose(
                        logger,
                        $"[CURRENT GAME COMPAT] DLC4 {expected.Label} no detectado | " +
                        $"Body esperado: {expected.BodyName}"
                    );
                    continue;
                }

                detected++;

                if (survivor.HasOriginalUnlock)
                {
                    originalUnlocks++;
                }

                if (survivor.IsModded)
                {
                    classificationErrors++;
                    logger?.LogWarning(
                        $"[CURRENT GAME COMPAT] {expected.BodyName} fue clasificado como modded. " +
                        "USU conservará la autoridad existente y no debe generar un fallback hasta corregir la procedencia."
                    );
                }

                UsuLog.Verbose(
                    logger,
                    $"[CURRENT GAME COMPAT] DLC4 {expected.Label} | " +
                    $"Body: {survivor.BodyName} | " +
                    $"Original: {(survivor.HasOriginalUnlock ? "SI" : "NO")} | " +
                    $"Modded: {(survivor.IsModded ? "SI" : "NO")} | " +
                    $"Estado: {survivor.Status} | " +
                    $"Expansion: {survivor.ExpansionName}"
                );
            }

            UsuLog.Diagnostic(
                logger,
                $"[CURRENT GAME COMPAT] RoR2 1.5/DLC4 auditado | " +
                $"Survivors oficiales esperados: {Dlc4Survivors.Length} | " +
                $"Detectados: {detected} | Unlock Original: {originalUnlocks} | " +
                $"Clasificación incorrecta como modded: {classificationErrors} | " +
                "Autoridad modificada: no"
            );
        }

        private static SurvivorInfo FindByBodyName(
            IReadOnlyList<SurvivorInfo> survivors,
            string bodyName
        )
        {
            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorInfo survivor = survivors[i];
                if (survivor == null)
                {
                    continue;
                }

                if (
                    string.Equals(
                        survivor.BodyName,
                        bodyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return survivor;
                }
            }

            return null;
        }
    }
}
