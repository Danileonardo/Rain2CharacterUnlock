using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Registro central de integraciones curadas conocidas por USU.
    ///
    /// IMPORTANTE:
    /// - No carga ni referencia DLLs de mods externos.
    /// - Una definición registrada no implica que su mod esté instalado.
    /// - Sólo pasa a la lista ActiveDefinitions cuando encuentra un SurvivorInfo
    ///   detectado cuya procedencia y Body coinciden.
    /// - En esta primera fase el registro es de sólo lectura/diagnóstico y no altera
    ///   providers, unlocks, misiones, traducciones, JSON ni multiplayer.
    /// </summary>
    public static class SurvivorDefinitionRegistry
    {
        private static readonly List<SurvivorDefinition>
            RegisteredDefinitions =
                new List<SurvivorDefinition>
                {
                    new AurelionSolDefinition(),
                    new AurielDefinition(),
                    new RalseiDefinition(),
                    new SoraDefinition(),
                    new WooperDefinition(),
                    new TinkatonDefinition(),
                    new EnforcerDefinition(),
                    new HunkDefinition(),
                    new JhinDefinition(),
                    new MinerDefinition(),
                    new RocketDefinition(),
                    new ScoutDefinition(),
                    new SpyDefinition(),
                    new HereticDefinition(),
                    new NemesisEnforcerDefinition()
                };

        private static readonly List<SurvivorDefinition>
            ActiveDefinitions =
                new List<SurvivorDefinition>();

        private static readonly Dictionary<string, SurvivorDefinition>
            ActiveByBody =
                new Dictionary<string, SurvivorDefinition>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static bool initialized;

        public static IReadOnlyList<SurvivorDefinition> Registered
        {
            get
            {
                return RegisteredDefinitions;
            }
        }

        public static IReadOnlyList<SurvivorDefinition> Active
        {
            get
            {
                return ActiveDefinitions;
            }
        }

        public static void Initialize(
            IReadOnlyList<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            ActiveDefinitions.Clear();
            ActiveByBody.Clear();

            if (survivors != null)
            {
                for (
                    int definitionIndex = 0;
                    definitionIndex < RegisteredDefinitions.Count;
                    definitionIndex++
                )
                {
                    SurvivorDefinition definition =
                        RegisteredDefinitions[definitionIndex];

                    if (definition == null)
                    {
                        continue;
                    }

                    SurvivorInfo matchedSurvivor =
                        FindDetectedSurvivor(
                            definition,
                            survivors
                        );

                    if (matchedSurvivor == null)
                    {
                        continue;
                    }

                    ActiveDefinitions.Add(
                        definition
                    );

                    if (
                        !string.IsNullOrWhiteSpace(
                            definition.BodyName
                        )
                    )
                    {
                        ActiveByBody[
                            definition.BodyName
                        ] = definition;
                    }
                }
            }

            initialized = true;

            if (logger == null)
            {
                return;
            }

            logger.LogInfo(
                "[SURVIVOR DEFINITIONS] Registry inicializado | " +
                $"Registradas: {RegisteredDefinitions.Count} | " +
                $"Activas: {ActiveDefinitions.Count}"
            );

            for (
                int i = 0;
                i < ActiveDefinitions.Count;
                i++
            )
            {
                SurvivorDefinition definition =
                    ActiveDefinitions[i];

                logger.LogInfo(
                    "[SURVIVOR DEFINITIONS] Activa | " +
                    $"{definition.BodyName} | " +
                    definition.SourceIdentifier
                );
            }
        }

        public static SurvivorDefinition FindFor(
            SurvivorInfo survivor
        )
        {
            if (survivor == null)
            {
                return null;
            }

            if (
                !string.IsNullOrWhiteSpace(
                    survivor.BodyName
                ) &&
                ActiveByBody.TryGetValue(
                    survivor.BodyName,
                    out SurvivorDefinition definition
                ) &&
                definition != null &&
                definition.Matches(
                    survivor
                )
            )
            {
                return definition;
            }

            return null;
        }

        public static SurvivorDefinition FindByBodyName(
            string bodyName
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return null;
            }

            return
                ActiveByBody.TryGetValue(
                    bodyName,
                    out SurvivorDefinition definition
                )
                    ? definition
                    : null;
        }

        public static bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }

        private static SurvivorInfo FindDetectedSurvivor(
            SurvivorDefinition definition,
            IReadOnlyList<SurvivorInfo> survivors
        )
        {
            for (
                int i = 0;
                i < survivors.Count;
                i++
            )
            {
                SurvivorInfo survivor =
                    survivors[i];

                if (
                    definition.Matches(
                        survivor
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
