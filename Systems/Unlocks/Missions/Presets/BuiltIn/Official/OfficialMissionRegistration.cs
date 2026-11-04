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
            Register(MissionPresetIds.BladedancerOfficial, CreateBladedancerOfficial);
            Register(MissionPresetIds.SpyOfficial, CreateSpyOfficial);
            Register(MissionPresetIds.ScoutOfficial, CreateScoutOfficial);
            Register(MissionPresetIds.RocketOfficial, CreateRocketOfficial);
            Register(MissionPresetIds.HunkOfficial, CreateHunkOfficial);
            Register(MissionPresetIds.TinkatonOfficial, CreateTinkatonOfficial);
            Register(MissionPresetIds.HelPOfficial, CreateHelPOfficial);
            Register(MissionPresetIds.WooperOfficial, CreateWooperOfficial);
            Register(MissionPresetIds.SoraOfficial, CreateSoraOfficial);
            Register(MissionPresetIds.RalseiOfficial, CreateRalseiOfficial);
            Register(MissionPresetIds.MinerOfficial, CreateMinerOfficial);
            Register(MissionPresetIds.GwenOfficial, CreateGwenOfficial);
            Register(MissionPresetIds.KindredOfficial, CreateKindredOfficial);
            Register(MissionPresetIds.SeamstressOfficial, CreateSeamstressOfficial);
            Register(MissionPresetIds.BansheeOfficial, CreateBansheeOfficial);
            Register(MissionPresetIds.WandererOfficial, CreateWandererOfficial);
            Register(MissionPresetIds.WarlockOfficial, CreateWarlockOfficial);
            Register(MissionPresetIds.AH64Official, CreateAH64Official);
            Register(MissionPresetIds.CelestialWarTankOfficial, CreateCelestialWarTankOfficial);
            Register(MissionPresetIds.BayonettaOfficial, CreateBayonettaOfficial);
            Register(MissionPresetIds.DarthVaderOfficial, CreateDarthVaderOfficial);
            Register(MissionPresetIds.KnightOfficial, CreateKnightOfficial);
            Register(MissionPresetIds.HornetOfficial, CreateHornetOfficial);
            Register(MissionPresetIds.SonicOfficial, CreateSonicOfficial);
            Register(MissionPresetIds.ShadowOfficial, CreateShadowOfficial);
        }
    }
}
