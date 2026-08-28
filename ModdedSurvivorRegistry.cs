using System.Collections.Generic;
using BepInEx.Logging;
using R2API.ContentManagement;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static class ModdedSurvivorRegistry
    {
        private class ModdedSourceInfo
        {
            public string ContentPackIdentifier = "";
            public string AssemblyName = "";
        }

        private static readonly Dictionary<
            SurvivorDef,
            ModdedSourceInfo
        > ModdedSurvivors =
            new Dictionary<
                SurvivorDef,
                ModdedSourceInfo
            >();

        public static void Rebuild(
            ManualLogSource logger
        )
        {
            ModdedSurvivors.Clear();

            try
            {
                var managedPacks =
                    R2APIContentManager.ManagedContentPacks;

                if (managedPacks.Length == 0)
                {
                    logger.LogWarning(
                        "R2API ManagedContentPacks todavía no está disponible."
                    );

                    return;
                }

                foreach (
                    ManagedReadOnlyContentPack managedPack
                    in managedPacks
                )
                {
                    string contentPackIdentifier =
                        managedPack.Identifier
                        ?? "UnknownContentPack";

                    string assemblyName =
                        managedPack.TiedAssembly != null
                            ? managedPack
                                .TiedAssembly
                                .GetName()
                                .Name
                            : "UnknownAssembly";

                    foreach (
                        SurvivorDef survivor
                        in managedPack
                            .ContentPack
                            .survivorDefs
                    )
                    {
                        if (survivor == null)
                        {
                            continue;
                        }

                        if (
                            ModdedSurvivors.ContainsKey(
                                survivor
                            )
                        )
                        {
                            continue;
                        }

                        ModdedSurvivors.Add(
                            survivor,
                            new ModdedSourceInfo
                            {
                                ContentPackIdentifier =
                                    contentPackIdentifier,

                                AssemblyName =
                                    assemblyName
                            }
                        );

                        logger.LogInfo(
                            $"Survivor MOD detectado por ContentPack | " +
                            $"{survivor.cachedName} | " +
                            $"Pack: {contentPackIdentifier} | " +
                            $"Assembly: {assemblyName}"
                        );
                    }
                }

                logger.LogInfo(
                    $"Survivors modded registrados: " +
                    $"{ModdedSurvivors.Count}"
                );
            }
            catch (System.Exception exception)
            {
                logger.LogError(
                    "Error detectando ContentPacks de mods."
                );

                logger.LogError(
                    exception.Message
                );
            }
        }

        public static bool TryGetSource(
            SurvivorDef survivor,
            out string contentPackIdentifier,
            out string assemblyName
        )
        {
            contentPackIdentifier = "";
            assemblyName = "";

            if (survivor == null)
            {
                return false;
            }

            if (
                !ModdedSurvivors.TryGetValue(
                    survivor,
                    out ModdedSourceInfo sourceInfo
                )
            )
            {
                return false;
            }

            contentPackIdentifier =
                sourceInfo.ContentPackIdentifier;

            assemblyName =
                sourceInfo.AssemblyName;

            return true;
        }
    }
}