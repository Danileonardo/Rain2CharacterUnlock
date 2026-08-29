using System;
using Newtonsoft.Json.Linq;
using RoR2;
using RoR2.Achievements;

namespace UniversalSurvivorUnlocks
{
    public class PrecisionExecutionStreakServerAchievement
        : BaseServerAchievement
    {
        private const int DefaultRailgunnerTarget =
            24;

        private const int DefaultBanditTarget =
            24;


        private int railgunnerTarget =
            DefaultRailgunnerTarget;

        private int banditTarget =
            DefaultBanditTarget;


        private bool completed;


        // =========================================================
        // INSTALAR
        // =========================================================

        public override void OnInstall()
        {
            base.OnInstall();


            completed =
                false;


            ResolveTargets(
                out railgunnerTarget,
                out banditTarget
            );


            PrecisionExecutionStreakTracker
                .RailgunnerWeakPointStreakChanged +=
                OnRailgunnerStreakChanged;


            PrecisionExecutionStreakTracker
                .BanditLightsOutStreakChanged +=
                OnBanditStreakChanged;


            // =====================================================
            // COMPROBACIÓN DE SEGURIDAD
            // =====================================================
            //
            // Si este achievement se instala cuando ya
            // existe una racha válida, la detectamos.
            // =====================================================

            CheckRailgunnerProgress(
                PrecisionExecutionStreakTracker
                    .GetHighestRailgunnerStreak()
            );


            CheckBanditProgress(
                PrecisionExecutionStreakTracker
                    .GetHighestBanditStreak()
            );
        }


        // =========================================================
        // DESINSTALAR
        // =========================================================

        public override void OnUninstall()
        {
            PrecisionExecutionStreakTracker
                .RailgunnerWeakPointStreakChanged -=
                OnRailgunnerStreakChanged;


            PrecisionExecutionStreakTracker
                .BanditLightsOutStreakChanged -=
                OnBanditStreakChanged;


            base.OnUninstall();
        }


        // =========================================================
        // RAILGUNNER
        // =========================================================

        private void OnRailgunnerStreakChanged(
            CharacterMaster playerMaster,
            int currentStreak
        )
        {
            CheckRailgunnerProgress(
                currentStreak
            );
        }


        private void CheckRailgunnerProgress(
            int currentStreak
        )
        {
            if (completed)
            {
                return;
            }


            if (
                currentStreak <
                railgunnerTarget
            )
            {
                return;
            }


            CompleteChallenge();
        }


        // =========================================================
        // BANDIT
        // =========================================================

        private void OnBanditStreakChanged(
            CharacterMaster playerMaster,
            int currentStreak
        )
        {
            CheckBanditProgress(
                currentStreak
            );
        }


        private void CheckBanditProgress(
            int currentStreak
        )
        {
            if (completed)
            {
                return;
            }


            if (
                currentStreak <
                banditTarget
            )
            {
                return;
            }


            CompleteChallenge();
        }


        // =========================================================
        // COMPLETAR
        // =========================================================

        private void CompleteChallenge()
        {
            if (completed)
            {
                return;
            }


            completed =
                true;


            Grant();


            ServerTryToCompleteActivity();
        }


        // =========================================================
        // LEER OBJETIVOS DESDE SURVIVORS.JSON
        // =========================================================

        private static void ResolveTargets(
            out int railgunnerAmount,
            out int banditAmount
        )
        {
            railgunnerAmount =
                DefaultRailgunnerTarget;


            banditAmount =
                DefaultBanditTarget;


            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (
                config == null ||
                config.AvailableSurvivors == null
            )
            {
                return;
            }


            foreach (
                SurvivorJsonEntry entry
                in config.AvailableSurvivors.Values
            )
            {
                SurvivorChallengeJson challenge =
                    entry?.Challenge;


                if (
                    challenge == null ||
                    !challenge.Enabled
                )
                {
                    continue;
                }


                if (
                    !string.Equals(
                        challenge.Type,
                        "PrecisionExecutionStreak",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                JObject parameters =
                    challenge.Parameters;


                if (parameters == null)
                {
                    return;
                }


                // =================================================
                // RAILGUNNER
                // =================================================

                JToken railgunnerToken =
                    parameters[
                        "railgunnerWeakPoints"
                    ];


                if (railgunnerToken != null)
                {
                    try
                    {
                        int value =
                            railgunnerToken
                                .Value<int>();


                        if (value > 0)
                        {
                            railgunnerAmount =
                                value;
                        }
                    }
                    catch
                    {
                        // Dejamos 24 por defecto.
                    }
                }


                // =================================================
                // BANDIT
                // =================================================

                JToken banditToken =
                    parameters[
                        "banditLightsOutKills"
                    ];


                if (banditToken != null)
                {
                    try
                    {
                        int value =
                            banditToken
                                .Value<int>();


                        if (value > 0)
                        {
                            banditAmount =
                                value;
                        }
                    }
                    catch
                    {
                        // Dejamos 24 por defecto.
                    }
                }


                return;
            }
        }
    }
}