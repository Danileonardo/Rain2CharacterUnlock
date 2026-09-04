using System;
using System.Collections.Generic;

using BepInEx.Logging;

using Newtonsoft.Json.Linq;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    public static class ChallengeCompletionRouter
    {
        private static ManualLogSource logger;

        private static bool initialized;


        // =========================================================
        // MISIONES YA COMPLETADAS EN ESTA RUN
        // =========================================================

        private static readonly HashSet<string>
            CompletedBodiesThisRun =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


        // =========================================================
        // INICIALIZAR
        // =========================================================

        public static void Initialize(
            ManualLogSource pluginLogger
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;


            logger =
                pluginLogger;


            // =====================================================
            // TRACKERS
            // =====================================================

            StatusEffectTracker
                .ActiveStatusCountChanged +=
                OnStatusEffectsChanged;


            HealHealthTracker
                .HealingChanged +=
                OnHealingChanged;


            BossCriticalKillTracker
                .BossCriticalKillDetected +=
                OnBossCriticalKillDetected;


            BackstabBossKillTracker
                .BackstabBossKillDetected +=
                OnBackstabBossKillDetected;


            HoldItemStackTracker
                .ItemCountChanged +=
                OnItemCountChanged;


            AirborneExplosionKillTracker
                .ExplosionKillCountChanged +=
                OnExplosionKillCountChanged;


            PrecisionExecutionStreakTracker
                .RailgunnerWeakPointStreakChanged +=
                OnRailgunnerWeakPointStreakChanged;


            PrecisionExecutionStreakTracker
                .BanditLightsOutStreakChanged +=
                OnBanditLightsOutStreakChanged;


            ScrapItemBossFinisherTracker
                .PlayerKillDetected +=
                OnScrapItemBossFinisherKill;


            KillEnemiesTracker
                .KillCountChanged +=
                OnKillEnemiesChanged;


            // =====================================================
            // RUN
            // =====================================================

            Run.onRunStartGlobal +=
                OnRunStart;


            Run.onRunDestroyGlobal +=
                OnRunEnd;


            logger?.LogInfo(
                "ChallengeCompletionRouter GLOBAL inicializado."
            );
        }


        // =========================================================
        // RUN
        // =========================================================

        private static void OnRunStart(
            Run run
        )
        {
            CompletedBodiesThisRun.Clear();
        }


        private static void OnRunEnd(
            Run run
        )
        {
            CompletedBodiesThisRun.Clear();
        }


        // =========================================================
        // APPLY STATUS EFFECTS
        // =========================================================

        private static void OnStatusEffectsChanged(
            int negativeCount,
            int positiveCount,
            int totalCount
        )
        {
            EvaluateIntThreshold(
                "ApplyStatusEffects",
                "amount",
                totalCount,
                100,
                "Estados activos"
            );
        }


        // =========================================================
        // HEAL HEALTH
        // =========================================================

        private static void OnHealingChanged(
            float addedHealing,
            float totalHealing
        )
        {
            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in GetMatchingChallenges(
                    "HealHealth"
                )
            )
            {
                SurvivorChallengeJson challenge =
                    pair.Value.Challenge;


                float target =
                    ReadPositiveFloat(
                        challenge.Parameters,
                        "amount",
                        10000f
                    );


                if (totalHealing < target)
                {
                    continue;
                }


                CompleteChallenge(
                    pair,
                    "HealHealth",
                    $"Curación: {totalHealing:0.##}/{target:0.##}"
                );
            }
        }


        // =========================================================
        // BOSS CRITICAL KILL
        // =========================================================

        private static void OnBossCriticalKillDetected(
            float damage
        )
        {
            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in GetMatchingChallenges(
                    "BossCriticalKill"
                )
            )
            {
                float minimumDamage =
                    ReadPositiveFloat(
                        pair
                            .Value
                            .Challenge
                            .Parameters,
                        "minimumDamage",
                        4444f
                    );


                if (damage < minimumDamage)
                {
                    continue;
                }


                CompleteChallenge(
                    pair,
                    "BossCriticalKill",
                    $"Daño crítico mortal: {damage:0.##}/{minimumDamage:0.##}"
                );
            }
        }


        // =========================================================
        // BACKSTAB BOSS KILL
        // =========================================================

        private static void OnBackstabBossKillDetected()
        {
            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in GetMatchingChallenges(
                    "BackstabBossKill"
                )
            )
            {
                CompleteChallenge(
                    pair,
                    "BackstabBossKill",
                    "Backstab mortal válido sobre jefe"
                );
            }
        }


        // =========================================================
        // HOLD ITEM STACK
        // =========================================================

        private static void OnItemCountChanged(
            CharacterMaster playerMaster,
            int currentCount
        )
        {
            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in GetMatchingChallenges(
                    "HoldItemStack"
                )
            )
            {
                JObject parameters =
                    pair
                        .Value
                        .Challenge
                        .Parameters;


                /*
                 * El tracker actual está construido
                 * específicamente alrededor de SprintBonus
                 * (Energy Drink).
                 *
                 * Cuando hagamos el editor dinámico,
                 * generalizaremos este tracker para ItemCatalog.
                 */
                string itemName =
                    ReadString(
                        parameters,
                        "item",
                        "SprintBonus"
                    );


                if (
                    !string.Equals(
                        itemName,
                        "SprintBonus",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                int target =
                    ReadPositiveInt(
                        parameters,
                        "amount",
                        15
                    );


                if (currentCount < target)
                {
                    continue;
                }


                CompleteChallenge(
                    pair,
                    "HoldItemStack",
                    $"Objetos: {currentCount}/{target}"
                );
            }
        }


        // =========================================================
        // AIRBORNE EXPLOSION KILLS
        // =========================================================

        private static void OnExplosionKillCountChanged(
            CharacterMaster playerMaster,
            int currentCount
        )
        {
            EvaluateIntThreshold(
                "AirborneExplosionKills",
                "amount",
                currentCount,
                15,
                "Bajas explosivas aéreas"
            );
        }


        // =========================================================
        // HUNK - RAILGUNNER
        // =========================================================

        private static void OnRailgunnerWeakPointStreakChanged(
            CharacterMaster playerMaster,
            int currentStreak
        )
        {
            EvaluateIntThreshold(
                "PrecisionExecutionStreak",
                "railgunnerWeakPoints",
                currentStreak,
                24,
                "Railgunner M99"
            );
        }


        // =========================================================
        // HUNK - BANDIT
        // =========================================================

        private static void OnBanditLightsOutStreakChanged(
            CharacterMaster playerMaster,
            int currentStreak
        )
        {
            EvaluateIntThreshold(
                "PrecisionExecutionStreak",
                "banditLightsOutKills",
                currentStreak,
                24,
                "Bandit Lights Out"
            );
        }


        // =========================================================
        // KILL ENEMIES
        // =========================================================

        private static void OnKillEnemiesChanged(
            CharacterMaster playerMaster,
            int currentCount
        )
        {
            EvaluateIntThreshold(
                "KillEnemies",
                "amount",
                currentCount,
                100,
                "Enemigos eliminados"
            );
        }


        // =========================================================
        // SCRAP ITEM BOSS FINISHER
        // =========================================================

        private static void OnScrapItemBossFinisherKill(
            CharacterMaster playerMaster,
            CharacterBody attackerBody,
            CharacterBody victimBody,
            DamageReport damageReport,
            int scrapConverted,
            int damageSourceRaw
        )
        {
            if (
                playerMaster == null ||
                attackerBody == null ||
                victimBody == null
            )
            {
                return;
            }


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in GetMatchingChallenges(
                    "ScrapItemBossFinisher"
                )
            )
            {
                JObject parameters =
                    pair
                        .Value
                        .Challenge
                        .Parameters;


                int scrapTarget =
                    ReadPositiveInt(
                        parameters,
                        "scrapAmount",
                        6
                    );


                string requiredBodyName =
                    ReadString(
                        parameters,
                        "requiredBody",
                        "ToolbotBody"
                    );


                string requiredItemName =
                    ReadString(
                        parameters,
                        "requiredItem",
                        "ArmorReductionOnHit"
                    );


                string bossBodyName =
                    ReadString(
                        parameters,
                        "bossBody",
                        "SuperRoboBallBossBody"
                    );


                string damageSourceName =
                    ReadString(
                        parameters,
                        "finalDamageSource",
                        "Secondary"
                    );


                string requiredSkillToken =
                    ReadString(
                        parameters,
                        "requiredSecondarySkillToken",
                        "TOOLBOT_SECONDARY_NAME"
                    );


                // =================================================
                // BODY
                // =================================================

                BodyIndex requiredBodyIndex =
                    BodyCatalog.FindBodyIndex(
                        requiredBodyName
                    );


                if (
                    attackerBody.bodyIndex !=
                    requiredBodyIndex
                )
                {
                    continue;
                }


                // =================================================
                // BOSS
                // =================================================

                BodyIndex bossBodyIndex =
                    BodyCatalog.FindBodyIndex(
                        bossBodyName
                    );


                if (
                    victimBody.bodyIndex !=
                    bossBodyIndex
                )
                {
                    continue;
                }


                // =================================================
                // SCRAP
                // =================================================

                if (scrapConverted < scrapTarget)
                {
                    continue;
                }


                // =================================================
                // ITEM
                // =================================================

                if (playerMaster.inventory == null)
                {
                    continue;
                }


                ItemIndex requiredItemIndex =
                    ItemCatalog.FindItemIndex(
                        requiredItemName
                    );


                if (
                    requiredItemIndex ==
                    ItemIndex.None
                )
                {
                    continue;
                }


                int itemCount =
                    playerMaster
                        .inventory
                        .GetItemCountPermanent(
                            requiredItemIndex
                        );


                if (itemCount <= 0)
                {
                    continue;
                }


                // =================================================
                // DAMAGE SOURCE
                // =================================================

                if (
                    !Enum.TryParse(
                        damageSourceName,
                        true,
                        out DamageSource requiredDamageSource
                    )
                )
                {
                    requiredDamageSource =
                        DamageSource.Secondary;
                }


                int requiredDamageMask =
                    (int)requiredDamageSource;


                if (
                    requiredDamageMask == 0 ||
                    (
                        damageSourceRaw &
                        requiredDamageMask
                    ) == 0
                )
                {
                    continue;
                }


                // =================================================
                // SKILL
                // =================================================

                if (
                    !HasRequiredSecondarySkill(
                        attackerBody,
                        requiredSkillToken
                    )
                )
                {
                    continue;
                }


                CompleteChallenge(
                    pair,
                    "ScrapItemBossFinisher",
                    $"Scrap: {scrapConverted}/{scrapTarget}"
                );
            }
        }


        // =========================================================
        // EVALUADOR NUMÉRICO GLOBAL
        // =========================================================

        private static void EvaluateIntThreshold(
            string challengeType,
            string parameterName,
            int currentValue,
            int fallback,
            string detailName
        )
        {
            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in GetMatchingChallenges(
                    challengeType
                )
            )
            {
                int target =
                    ReadPositiveInt(
                        pair
                            .Value
                            .Challenge
                            .Parameters,
                        parameterName,
                        fallback
                    );


                if (currentValue < target)
                {
                    continue;
                }


                CompleteChallenge(
                    pair,
                    challengeType,
                    $"{detailName}: {currentValue}/{target}"
                );
            }
        }


        // =========================================================
        // BUSCAR CHALLENGES
        // =========================================================

        private static IEnumerable<
            KeyValuePair<
                string,
                SurvivorJsonEntry
            >
        > GetMatchingChallenges(
            string challengeType
        )
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (
                config == null ||
                config.AvailableSurvivors == null
            )
            {
                yield break;
            }


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in config.AvailableSurvivors
            )
            {
                SurvivorJsonEntry entry =
                    pair.Value;


                /*
                 * Mission Schema v2 tiene su propio catálogo,
                 * progreso compuesto y dispatcher.
                 *
                 * Aunque el campo legacy "type" permanezca en el
                 * JSON por compatibilidad, una misión v2 nunca debe
                 * ser evaluada también por este router antiguo.
                 */
                if (
                    entry?.Challenge?.Mission != null &&
                    entry.Challenge.Mission.Routes != null &&
                    entry.Challenge.Mission.Routes.Count > 0
                )
                {
                    continue;
                }


                if (
                    !SurvivorUnlockManager
                        .RequiresCustomUnlock(
                            entry
                        )
                )
                {
                    continue;
                }


                if (
                    !string.Equals(
                        entry.Challenge.Type,
                        challengeType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                yield return pair;
            }
        }


        // =========================================================
        // COMPLETAR MISIÓN
        // =========================================================

        private static void CompleteChallenge(
            KeyValuePair<
                string,
                SurvivorJsonEntry
            > pair,
            string challengeType,
            string details
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance == null)
            {
                return;
            }


            SurvivorJsonEntry entry =
                pair.Value;


            string bodyName =
                !string.IsNullOrWhiteSpace(
                    entry?.BodyName
                )
                    ? entry.BodyName
                    : pair.Key;


            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return;
            }


            // Ya se resolvió esta misión durante esta run.
            if (
                CompletedBodiesThisRun.Contains(
                    bodyName
                )
            )
            {
                return;
            }


            // Debe ser realmente un unlock administrado por USU.
            if (
                !SurvivorUnlockManager
                    .TryGetCustomUnlockable(
                        bodyName,
                        out UnlockableDef unlockable
                    ) ||
                unlockable == null
            )
            {
                logger?.LogWarning(
                    "[MISSION ROUTER] " +
                    "No existe Unlockable USU | " +
                    $"Body: {bodyName}"
                );


                return;
            }


            CompletedBodiesThisRun.Add(
                bodyName
            );


            logger?.LogInfo(
                "[MISSION ROUTER] MISIÓN COMPLETADA | " +
                $"Body: {bodyName} | " +
                $"Tipo: {challengeType} | " +
                details
            );


            SessionUnlockManager.CompleteMission(
                bodyName
            );
        }


        // =========================================================
        // SECONDARY SKILL
        // =========================================================

        private static bool HasRequiredSecondarySkill(
            CharacterBody body,
            string requiredSkillToken
        )
        {
            if (
                body == null ||
                body.skillLocator == null ||
                body.skillLocator.secondary == null ||
                body.skillLocator.secondary.skillDef == null
            )
            {
                return false;
            }


            string skillToken =
                body
                    .skillLocator
                    .secondary
                    .skillDef
                    .skillNameToken;


            return string.Equals(
                skillToken,
                requiredSkillToken,
                StringComparison.Ordinal
            );
        }


        // =========================================================
        // JSON HELPERS
        // =========================================================

        private static int ReadPositiveInt(
            JObject parameters,
            string key,
            int fallback
        )
        {
            if (
                parameters == null ||
                string.IsNullOrWhiteSpace(
                    key
                )
            )
            {
                return fallback;
            }


            JToken token =
                parameters[key];


            if (token == null)
            {
                return fallback;
            }


            try
            {
                int value =
                    token.Value<int>();


                return
                    value > 0
                        ? value
                        : fallback;
            }
            catch
            {
                return fallback;
            }
        }


        private static float ReadPositiveFloat(
            JObject parameters,
            string key,
            float fallback
        )
        {
            if (
                parameters == null ||
                string.IsNullOrWhiteSpace(
                    key
                )
            )
            {
                return fallback;
            }


            JToken token =
                parameters[key];


            if (token == null)
            {
                return fallback;
            }


            try
            {
                float value =
                    token.Value<float>();


                return
                    value > 0f
                        ? value
                        : fallback;
            }
            catch
            {
                return fallback;
            }
        }


        private static string ReadString(
            JObject parameters,
            string key,
            string fallback
        )
        {
            if (
                parameters == null ||
                string.IsNullOrWhiteSpace(
                    key
                )
            )
            {
                return fallback;
            }


            JToken token =
                parameters[key];


            if (token == null)
            {
                return fallback;
            }


            string value =
                token.ToString();


            return
                !string.IsNullOrWhiteSpace(
                    value
                )
                    ? value
                    : fallback;
        }
    }
}
