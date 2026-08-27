using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    [BepInPlugin(
        PluginGuid,
        PluginName,
        PluginVersion
    )]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";

        public const string PluginName =
            "Universal Survivor Unlocks";

        public const string PluginVersion =
            "0.0.2";

        private void Awake()
        {
            Logger.LogInfo(
                "Universal Survivor Unlocks cargado correctamente."
            );

            RoR2Application.onLoad += OnGameLoaded;
        }

        private void OnGameLoaded()
        {
            Logger.LogInfo(
                "Risk of Rain 2 terminó de cargar. Detectando survivors..."
            );

            SurvivorDetector.DetectSurvivors(Logger);

            RoR2Application.onLoad -= OnGameLoaded;
        }
    }
}