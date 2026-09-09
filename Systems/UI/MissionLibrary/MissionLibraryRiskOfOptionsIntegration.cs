using System;
using System.IO;
using System.Reflection;

using BepInEx.Bootstrap;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Events;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// 5G.2E-A FIX3 - Integración opcional con Risk of Options.
    ///
    /// Registra cuatro categorías nativas de Risk of Options y una
    /// GenericButtonOption ANCLA por categoría. En FIX3 esas anclas ya no son
    /// contenido visible: el EmbeddedHost las conserva únicamente como punto
    /// técnico para localizar el VerticalLayout y las oculta fuera del layout
    /// antes del render. Todas las filas visibles son administradas por USU.
    ///
    /// Se mantiene por reflexión para no convertir RiskOfOptions en
    /// dependencia dura de compilación.
    /// </summary>
    internal static class MissionLibraryRiskOfOptionsIntegration
    {
        private const string RiskOfOptionsGuid =
            "com.rune580.riskofoptions";

        private const string UsuPluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";

        private const string UsuPluginName =
            "Universal Survivor Unlocks";

        // 5G.2E-A FIX3:
        // Los nombres siguen siendo etiquetas válidas para que podamos localizar
        // las opciones creadas por RiskOfOptions, pero el host las trata como
        // anclas técnicas ocultas y genera una copia visible de la primera fila.
        internal static string GeneralAnchorName { get; private set; } =
            "Universal Survivor Unlocks";

        internal static string SurvivorsAnchorName { get; private set; } =
            "Perfiles de personaje";

        internal static string MissionsAnchorName { get; private set; } =
            "Presets asignables";

        internal static string AdvancedAnchorName { get; private set; } =
            "Catálogo runtime";

        private static bool attempted;
        private static bool registered;

        private static GameObject embeddedHostObject;

        // Mantener referencias Unity vivas durante toda la sesión. El icono
        // se carga desde el recurso embebido icon.png del propio assembly.
        private static Texture2D modIconTexture;
        private static Sprite modIconSprite;


        public static bool TryRegister(
            ManualLogSource logger
        )
        {
            if (attempted)
            {
                return registered;
            }

            attempted = true;

            try
            {
                if (
                    !Chainloader.PluginInfos.TryGetValue(
                        RiskOfOptionsGuid,
                        out var pluginInfo
                    ) ||
                    pluginInfo?.Instance == null
                )
                {
                    return false;
                }

                Assembly riskAssembly =
                    pluginInfo.Instance
                        .GetType()
                        .Assembly;

                Type managerType =
                    riskAssembly.GetType(
                        "RiskOfOptions.ModSettingsManager"
                    );

                Type buttonType =
                    riskAssembly.GetType(
                        "RiskOfOptions.Options.GenericButtonOption"
                    );

                if (
                    managerType == null ||
                    buttonType == null
                )
                {
                    logger?.LogWarning(
                        "[MISSION LIBRARY UI] RiskOfOptions detectado, " +
                        "pero no se encontraron sus tipos públicos esperados."
                    );

                    return false;
                }

                ConstructorInfo buttonConstructor =
                    buttonType.GetConstructor(
                        new Type[]
                        {
                            typeof(string),
                            typeof(string),
                            typeof(string),
                            typeof(string),
                            typeof(UnityAction)
                        }
                    );

                if (buttonConstructor == null)
                {
                    logger?.LogWarning(
                        "[MISSION LIBRARY UI] RiskOfOptions detectado, " +
                        "pero GenericButtonOption no expone la firma esperada."
                    );

                    return false;
                }

                MethodInfo addOptionMethod =
                    FindAddOptionMethod(
                        managerType
                    );

                if (addOptionMethod == null)
                {
                    logger?.LogWarning(
                        "[MISSION LIBRARY UI] RiskOfOptions detectado, " +
                        "pero no se encontró ModSettingsManager.AddOption(option, guid, name)."
                    );

                    return false;
                }

                // Las anclas usan etiquetas reales y localizadas para que el
                // nombre del GameObject sea estable. El host las vuelve
                // transparentes y las saca del Layout en LateUpdate.
                GeneralAnchorName =
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryEmbeddedTitleToken
                    );

                SurvivorsAnchorName =
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryEmbeddedProfilesToken
                    );

                MissionsAnchorName =
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryEmbeddedAssignablePresetsToken
                    );

                AdvancedAnchorName =
                    Language.GetString(
                        SurvivorLocalization.MissionLibraryEmbeddedRuntimeCatalogToken
                    );

                RegisterEmbeddedAnchor(
                    buttonConstructor,
                    addOptionMethod,
                    GeneralAnchorName,
                    "",
                    Language.GetString(
                        SurvivorLocalization
                            .MissionLibraryEmbeddedGeneralCategoryToken
                    )
                );

                RegisterEmbeddedAnchor(
                    buttonConstructor,
                    addOptionMethod,
                    SurvivorsAnchorName,
                    "",
                    Language.GetString(
                        SurvivorLocalization
                            .MissionLibraryEmbeddedSurvivorsCategoryToken
                    )
                );

                RegisterEmbeddedAnchor(
                    buttonConstructor,
                    addOptionMethod,
                    MissionsAnchorName,
                    "",
                    Language.GetString(
                        SurvivorLocalization
                            .MissionLibraryEmbeddedMissionsCategoryToken
                    )
                );

                RegisterEmbeddedAnchor(
                    buttonConstructor,
                    addOptionMethod,
                    AdvancedAnchorName,
                    "",
                    Language.GetString(
                        SurvivorLocalization
                            .MissionLibraryEmbeddedAdvancedCategoryToken
                    )
                );

                TrySetDescription(
                    managerType,
                    logger
                );

                TrySetIcon(
                    managerType,
                    logger
                );

                EnsureEmbeddedHost(
                    logger
                );

                registered = true;

                logger?.LogInfo(
                    "[MISSION LIBRARY UI] 5G.2E-A registrada en RiskOfOptions | " +
                    "Categorías: General / Personajes / Misiones / Avanzado | " +
                    "Modo: Embedded"
                );

                return true;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(
                    "[MISSION LIBRARY UI] No se pudo registrar 5G.2E-A en RiskOfOptions; " +
                    "se usará el launcher fallback. " +
                    ex.GetType().Name +
                    ": " +
                    ex.Message
                );

                return false;
            }
        }


        private static void RegisterEmbeddedAnchor(
            ConstructorInfo buttonConstructor,
            MethodInfo addOptionMethod,
            string anchorName,
            string initialValue,
            string category
        )
        {
            UnityAction noOp =
                delegate
                {
                    // Ancla técnica: nunca debe ser visible ni interactuable.
                };

            string description =
                Language.GetString(
                    SurvivorLocalization
                        .MissionLibraryEmbeddedMarkerDescriptionToken
                );

            object button =
                buttonConstructor.Invoke(
                    new object[]
                    {
                        anchorName,
                        category,
                        description,
                        initialValue ?? "",
                        noOp
                    }
                );

            addOptionMethod.Invoke(
                null,
                new object[]
                {
                    button,
                    UsuPluginGuid,
                    UsuPluginName
                }
            );
        }


        private static void EnsureEmbeddedHost(
            ManualLogSource logger
        )
        {
            if (embeddedHostObject != null)
            {
                return;
            }

            embeddedHostObject =
                new GameObject(
                    "USU_RiskOfOptions_EmbeddedHost"
                );

            UnityEngine.Object.DontDestroyOnLoad(
                embeddedHostObject
            );

            MissionLibraryRiskOfOptionsEmbeddedHost host =
                embeddedHostObject.AddComponent<
                    MissionLibraryRiskOfOptionsEmbeddedHost
                >();

            host.Initialize(
                logger
            );
        }


        private static MethodInfo FindAddOptionMethod(
            Type managerType
        )
        {
            MethodInfo[] methods =
                managerType.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.Static
                );

            for (
                int i = 0;
                i < methods.Length;
                i++
            )
            {
                MethodInfo method =
                    methods[i];

                if (
                    method == null ||
                    !string.Equals(
                        method.Name,
                        "AddOption",
                        StringComparison.Ordinal
                    )
                )
                {
                    continue;
                }

                ParameterInfo[] parameters =
                    method.GetParameters();

                if (
                    parameters.Length == 3 &&
                    parameters[1].ParameterType == typeof(string) &&
                    parameters[2].ParameterType == typeof(string)
                )
                {
                    return method;
                }
            }

            return null;
        }




        private static void TrySetIcon(
            Type managerType,
            ManualLogSource logger
        )
        {
            try
            {
                if (managerType == null)
                {
                    return;
                }

                MethodInfo iconMethod =
                    managerType.GetMethod(
                        "SetModIcon",
                        BindingFlags.Public |
                        BindingFlags.Static,
                        null,
                        new Type[]
                        {
                            typeof(Sprite),
                            typeof(string),
                            typeof(string)
                        },
                        null
                    );

                if (iconMethod == null)
                {
                    logger?.LogDebug(
                        "[MISSION LIBRARY UI] RiskOfOptions: " +
                        "SetModIcon(Sprite, guid, name) no disponible."
                    );

                    return;
                }

                Sprite icon = LoadEmbeddedModIcon(logger);

                if (icon == null)
                {
                    return;
                }

                iconMethod.Invoke(
                    null,
                    new object[]
                    {
                        icon,
                        UsuPluginGuid,
                        UsuPluginName
                    }
                );
            }
            catch (Exception ex)
            {
                logger?.LogDebug(
                    "[MISSION LIBRARY UI] RiskOfOptions: icono no aplicado | " +
                    ex.Message
                );
            }
        }


        private static Sprite LoadEmbeddedModIcon(
            ManualLogSource logger
        )
        {
            if (modIconSprite != null)
            {
                return modIconSprite;
            }

            try
            {
                Assembly assembly =
                    typeof(MissionLibraryRiskOfOptionsIntegration)
                        .Assembly;

                const string resourceName =
                    "UniversalSurvivorUnlocks.icon.png";

                using Stream stream =
                    assembly.GetManifestResourceStream(
                        resourceName
                    );

                if (stream == null)
                {
                    logger?.LogDebug(
                        "[MISSION LIBRARY UI] No se encontró recurso embebido " +
                        resourceName + "."
                    );

                    return null;
                }

                byte[] bytes = new byte[stream.Length];
                int offset = 0;

                while (offset < bytes.Length)
                {
                    int read = stream.Read(
                        bytes,
                        offset,
                        bytes.Length - offset
                    );

                    if (read <= 0)
                    {
                        break;
                    }

                    offset += read;
                }

                if (offset != bytes.Length)
                {
                    return null;
                }

                modIconTexture =
                    new Texture2D(2, 2);

                modIconTexture.name =
                    "USU RiskOfOptions Icon Texture";
                modIconTexture.wrapMode = TextureWrapMode.Clamp;
                modIconTexture.filterMode = FilterMode.Bilinear;

                if (
                    !ImageConversion.LoadImage(
                        modIconTexture,
                        bytes,
                        false
                    )
                )
                {
                    UnityEngine.Object.Destroy(
                        modIconTexture
                    );

                    modIconTexture = null;
                    return null;
                }

                modIconSprite = Sprite.Create(
                    modIconTexture,
                    new Rect(
                        0f,
                        0f,
                        modIconTexture.width,
                        modIconTexture.height
                    ),
                    new Vector2(0.5f, 0.5f),
                    100f
                );

                modIconSprite.name =
                    "USU RiskOfOptions Icon";

                return modIconSprite;
            }
            catch (Exception ex)
            {
                logger?.LogDebug(
                    "[MISSION LIBRARY UI] No se pudo cargar icon.png embebido | " +
                    ex.Message
                );

                return null;
            }
        }

        private static void TrySetDescription(
            Type managerType,
            ManualLogSource logger
        )
        {
            try
            {
                MethodInfo descriptionMethod =
                    managerType.GetMethod(
                        "SetModDescription",
                        BindingFlags.Public |
                        BindingFlags.Static,
                        null,
                        new Type[]
                        {
                            typeof(string),
                            typeof(string),
                            typeof(string)
                        },
                        null
                    );

                if (descriptionMethod == null)
                {
                    return;
                }

                descriptionMethod.Invoke(
                    null,
                    new object[]
                    {
                        Language.GetString(
                            SurvivorLocalization
                                .MissionLibrarySettingsModDescriptionToken
                        ),
                        UsuPluginGuid,
                        UsuPluginName
                    }
                );
            }
            catch (Exception ex)
            {
                logger?.LogDebug(
                    "[MISSION LIBRARY UI] RiskOfOptions: descripción no aplicada | " +
                    ex.Message
                );
            }
        }
    }
}
