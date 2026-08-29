using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RoR2;
using RoR2.Achievements;

namespace UniversalSurvivorUnlocks
{
    public class ApplyStatusEffectsServerAchievement
        : BaseServerAchievement
    {
        private const int DefaultRequiredAmount =
            100;


        private int requiredAmount =
            DefaultRequiredAmount;


        private bool completed;


        // =========================================================
        // INSTALAR
        // =========================================================

        public override void OnInstall()
        {
            base.OnInstall();


            requiredAmount =
                ResolveRequiredAmount();


            StatusEffectTracker.ActiveStatusCountChanged +=
                StatusEffectTracker_ActiveStatusCountChanged;


            /*
             * Por si el tracker se instala cuando
             * la run ya está iniciada.
             */
            TryComplete(
                StatusEffectTracker.CurrentTotalCount
            );
        }


        // =========================================================
        // DESINSTALAR
        // =========================================================

        public override void OnUninstall()
        {
            StatusEffectTracker.ActiveStatusCountChanged -=
                StatusEffectTracker_ActiveStatusCountChanged;


            base.OnUninstall();
        }


        // =========================================================
        // CAMBIÓ EL TOTAL ACTIVO
        // =========================================================

        private void StatusEffectTracker_ActiveStatusCountChanged(
            int negativeCount,
            int positiveCount,
            int totalCount
        )
        {
            TryComplete(
                totalCount
            );
        }


        // =========================================================
        // INTENTAR COMPLETAR
        // =========================================================

        private void TryComplete(
            int totalCount
        )
        {
            if (completed)
            {
                return;
            }


            if (Run.instance == null)
            {
                return;
            }


            if (
                totalCount <
                requiredAmount
            )
            {
                return;
            }


            completed =
                true;


            Grant();


            ServerTryToCompleteActivity();
        }


        // =========================================================
        // LEER AMOUNT DEL JSON
        // =========================================================

        private static int ResolveRequiredAmount()
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (config == null)
            {
                return DefaultRequiredAmount;
            }


            List<int> configuredAmounts =
                new List<int>();


            CollectConfiguredAmounts(
                config.AvailableSurvivors,
                configuredAmounts
            );


            CollectConfiguredAmounts(
                config.UnavailableSurvivors,
                configuredAmounts
            );


            if (configuredAmounts.Count == 0)
            {
                return DefaultRequiredAmount;
            }


            int firstAmount =
                configuredAmounts[0];


            /*
             * Actualmente sólo tenemos un desafío
             * ApplyStatusEffects real: Sora.
             *
             * Si posteriormente varios survivors
             * utilizan este mismo tipo con cantidades
             * diferentes, tendremos que asociar cada
             * instancia de BaseServerAchievement con su
             * AchievementDef concreto.
             */
            for (
                int i = 1;
                i < configuredAmounts.Count;
                i++
            )
            {
                if (
                    configuredAmounts[i] !=
                    firstAmount
                )
                {
                    return DefaultRequiredAmount;
                }
            }


            return firstAmount;
        }


        private static void CollectConfiguredAmounts(
            Dictionary<string, SurvivorJsonEntry> entries,
            List<int> destination
        )
        {
            if (
                entries == null ||
                destination == null
            )
            {
                return;
            }


            foreach (
                KeyValuePair<string, SurvivorJsonEntry> pair
                in entries
            )
            {
                SurvivorJsonEntry entry =
                    pair.Value;


                SurvivorChallengeJson challenge =
                    entry?.Challenge;


                if (
                    challenge == null ||
                    !challenge.Enabled ||
                    !string.Equals(
                        challenge.Type,
                        "ApplyStatusEffects",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                int amount =
                    DefaultRequiredAmount;


                JObject parameters =
                    challenge.Parameters;


                if (parameters != null)
                {
                    JToken token =
                        parameters[
                            "amount"
                        ];


                    if (
                        token != null &&
                        int.TryParse(
                            token.ToString(),
                            out int parsedAmount
                        ) &&
                        parsedAmount > 0
                    )
                    {
                        amount =
                            parsedAmount;
                    }
                }


                destination.Add(
                    amount
                );
            }
        }
    }
}