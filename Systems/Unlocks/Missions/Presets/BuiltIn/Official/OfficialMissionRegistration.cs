using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // REGISTRO - MISIONES OFICIALES
        // =========================================================

        private static void RegisterOfficialCharacterRecipes()
        {
            Register(MissionPresetIds.JhinOfficial, CreateJhinOfficial);
            Register(MissionPresetIds.SpyOfficial, CreateSpyOfficial);
            Register(MissionPresetIds.ScoutOfficial, CreateScoutOfficial);
            Register(MissionPresetIds.RocketOfficial, CreateRocketOfficial);
            Register(MissionPresetIds.HunkOfficial, CreateHunkOfficial);
            Register(MissionPresetIds.TinkatonOfficial, CreateTinkatonOfficial);
            Register(MissionPresetIds.WooperOfficial, CreateWooperOfficial);
            Register(MissionPresetIds.SoraOfficial, CreateSoraOfficial);
            Register(MissionPresetIds.RalseiOfficial, CreateRalseiOfficial);
            Register(MissionPresetIds.MinerOfficial, CreateMinerOfficial);
        }
    }
}
