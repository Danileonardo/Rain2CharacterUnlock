using RoR2;
using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorDetector
    {
        public static void DetectSurvivors(ManualLogSource logger)
        {
            SurvivorDef[] survivors = SurvivorCatalog.survivorDefs;

            logger.LogInfo(
                $"Se detectaron {survivors.Length} survivors."
            );

            foreach (SurvivorDef survivor in survivors)
            {
                if (survivor == null)
                {
                    continue;
                }

                string internalName = survivor.cachedName;

                string bodyName = "Sin Body";

                if (survivor.bodyPrefab != null)
                {
                    bodyName = survivor.bodyPrefab.name;
                }

                logger.LogInfo(
                    $"Survivor detectado | Nombre: {internalName} | Body: {bodyName}"
                );
            }
        }
    }
}