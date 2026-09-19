using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - RALSEI
        // =========================================================

        private static MissionPreset CreateRalseiOfficial()
        {
            MissionObjective recruit = new MissionObjective
            {
                Id = "devotion_friends_3",
                Type = "RecruitMinions",
                Amount = 3d,
                ResetScope = "Run",
                Target = new MissionTarget { Category = "Ally" },
                Parameters = new JObject { ["markerItem"] = "LemurianHarness" }
            };

            MissionObjective teleporter = new MissionObjective
            {
                Id = "portal_with_friends",
                Type = "CompleteTeleporter",
                Amount = 1d,
                ResetScope = "Run"
            };

            // "Completa el portal con ellos" es una condición real, no sólo
            // texto temático: al finalizar el teletransportador el mismo
            // jugador debe conservar vivos al menos 3 Lemurianos de Devoción.
            AddObjectiveConditions(
                teleporter,
                CreateCondition(
                    "MinionsAlive",
                    new JObject
                    {
                        ["amount"] = 3,
                        ["markerItem"] = "LemurianHarness"
                    }
                )
            );

            return CreateMissionPreset(
                MissionPresetIds.RalseiOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RalseiBody",
                "El poder de la bondad",
                "Usa Devoción y reúne 3 nuevos amigos Lemurianos;\ncompleta el portal con ellos - Captain o Seeker",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "ralsei_devotion",
                    new MissionObjective[] { recruit, teleporter },
                    new MissionCondition[] { CreateRequiredSurvivorCondition("CaptainBody", "SeekerBody") },
                    new MissionRules { SingleRun = true }
                )
            );
        }
    }
}
