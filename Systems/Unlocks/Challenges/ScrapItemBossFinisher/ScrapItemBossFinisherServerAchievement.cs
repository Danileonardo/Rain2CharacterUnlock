using System;

using Newtonsoft.Json.Linq;

using RoR2;
using RoR2.Achievements;

using UnityEngine;


namespace UniversalSurvivorUnlocks
{
    public class ScrapItemBossFinisherServerAchievement
        : BaseServerAchievement
    {
        // =========================================================
        // VALORES POR DEFECTO
        // =========================================================

        private const int DefaultScrapAmount =
            6;


        private const string DefaultRequiredBody =
            "ToolbotBody";


        private const string DefaultRequiredItem =
            "ArmorReductionOnHit";


        private const string DefaultBossBody =
            "SuperRoboBallBossBody";


        private const string DefaultDamageSource =
            "Secondary";


        private const string
            DefaultRequiredSecondarySkillToken =
                "TOOLBOT_SECONDARY_NAME";


        // =========================================================
        // CONFIGURACIÓN RESUELTA
        // =========================================================

        private int scrapAmount =
            DefaultScrapAmount;


        private string requiredBodyName =
            DefaultRequiredBody;


        private string requiredItemName =
            DefaultRequiredItem;


        private string bossBodyName =
            DefaultBossBody;


        private string damageSourceName =
            DefaultDamageSource;


        private string requiredSecondarySkillToken =
            DefaultRequiredSecondarySkillToken;


        private BodyIndex requiredBodyIndex;


        private BodyIndex bossBodyIndex;


        private ItemIndex requiredItemIndex;


        private int requiredDamageSourceMask;


        private bool completed;


        // =========================================================
        // INSTALAR
        // =========================================================

        public override void OnInstall()
        {
            base.OnInstall();


            completed =
                false;


            ResolveConfiguration();


            // =====================================================
            // RESOLVER CATÁLOGOS
            // =====================================================

            requiredBodyIndex =
                BodyCatalog.FindBodyIndex(
                    requiredBodyName
                );


            bossBodyIndex =
                BodyCatalog.FindBodyIndex(
                    bossBodyName
                );


            requiredItemIndex =
                ItemCatalog.FindItemIndex(
                    requiredItemName
                );


            // =====================================================
            // DAMAGE SOURCE
            // =====================================================

            DamageSource parsedDamageSource;


            if (
                !Enum.TryParse(
                    damageSourceName,
                    true,
                    out parsedDamageSource
                )
            )
            {
                parsedDamageSource =
                    DamageSource.Secondary;
            }


            requiredDamageSourceMask =
                (int)parsedDamageSource;


            // =====================================================
            // ESCUCHAR MUERTES
            // =====================================================

            ScrapItemBossFinisherTracker
                .PlayerKillDetected +=
                OnPlayerKillDetected;
        }


        // =========================================================
        // DESINSTALAR
        // =========================================================

        public override void OnUninstall()
        {
            ScrapItemBossFinisherTracker
                .PlayerKillDetected -=
                OnPlayerKillDetected;


            base.OnUninstall();
        }


        // =========================================================
        // MUERTE DE UN JUGADOR
        // =========================================================

        private void OnPlayerKillDetected(
            CharacterMaster playerMaster,
            CharacterBody attackerBody,
            CharacterBody victimBody,
            DamageReport damageReport,
            int scrapConverted,
            int damageSourceRaw
        )
        {
            if (completed)
            {
                return;
            }


            if (
                playerMaster == null ||
                attackerBody == null ||
                victimBody == null
            )
            {
                return;
            }


            // =====================================================
            // PRIMERO FILTRAMOS EL JEFE
            // =====================================================
            //
            // Así no llenamos el log con cada Beetle
            // y cada Wisp de la partida.
            //
            // =====================================================

            bool correctBoss =
                victimBody.bodyIndex ==
                bossBodyIndex;


            if (!correctBoss)
            {
                return;
            }


            // =====================================================
            // ¿ES MUL-T?
            // =====================================================

            bool correctBody =
                attackerBody.bodyIndex ==
                requiredBodyIndex;


            // =====================================================
            // ¿CONVIRTIÓ 6 OBJETOS?
            // =====================================================

            bool enoughScrap =
                scrapConverted >=
                scrapAmount;


            // =====================================================
            // ¿TIENE JUSTICIA DEMOLEDORA?
            // =====================================================

            int requiredItemCount =
                0;


            if (
                playerMaster.inventory != null &&
                requiredItemIndex !=
                    ItemIndex.None
            )
            {
                requiredItemCount =
                    playerMaster
                        .inventory
                        .GetItemCountPermanent(
                            requiredItemIndex
                        );
            }


            bool hasRequiredItem =
                requiredItemCount > 0;


            // =====================================================
            // ¿EL GOLPE ES SECONDARY?
            // =====================================================

            bool correctDamageSource =
                requiredDamageSourceMask != 0 &&
                (
                    damageSourceRaw &
                    requiredDamageSourceMask
                )
                != 0;


            // =====================================================
            // ¿LA SECUNDARIA EQUIPADA ES BOTE EXPLOSIVO?
            // =====================================================
            //
            // Esto evita que Hooks of Heresy / Slicing Maelstrom
            // cuente simplemente por ser una habilidad Secondary.
            //
            // =====================================================

            bool correctSecondarySkill =
                IsRequiredSecondarySkill(
                    attackerBody
                );


            // =====================================================
            // DIAGNÓSTICO
            // =====================================================

            string inflictorName =
                damageReport != null &&
                damageReport.damageInfo.inflictor != null
                    ? damageReport
                        .damageInfo
                        .inflictor
                        .name
                    : "<null>";


            Debug.Log(
                "[TINKATON] ALLOY WORSHIP UNIT DERROTADA | " +
                $"MUL-T: {correctBody} | " +
                $"Scrap: {scrapConverted}/{scrapAmount} | " +
                $"Justicia demoledora: {hasRequiredItem} | " +
                $"DamageSource: {damageSourceRaw} | " +
                $"Secondary válida: {correctDamageSource} | " +
                $"Bote explosivo equipado: {correctSecondarySkill} | " +
                $"Inflictor/Body: {inflictorName}"
            );


            // =====================================================
            // TODAS LAS CONDICIONES
            // =====================================================

            if (!correctBody)
            {
                return;
            }


            if (!enoughScrap)
            {
                return;
            }


            if (!hasRequiredItem)
            {
                return;
            }


            if (!correctDamageSource)
            {
                return;
            }


            if (!correctSecondarySkill)
            {
                return;
            }


            // =====================================================
            // COMPLETAR
            // =====================================================

            completed =
                true;


            Debug.Log(
                "[TINKATON] FORJADA EN CHATARRA COMPLETADA."
            );


            Grant();


            ServerTryToCompleteActivity();
        }


        // =========================================================
        // COMPROBAR BOTE EXPLOSIVO
        // =========================================================

        private bool IsRequiredSecondarySkill(
            CharacterBody body
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


            string skillNameToken =
                body
                    .skillLocator
                    .secondary
                    .skillDef
                    .skillNameToken;


            return string.Equals(
                skillNameToken,
                requiredSecondarySkillToken,
                StringComparison.Ordinal
            );
        }

        // =========================================================
        // LEER CONFIGURACIÓN
        // =========================================================

        private void ResolveConfiguration()
        {
            scrapAmount =
                DefaultScrapAmount;


            requiredBodyName =
                DefaultRequiredBody;


            requiredItemName =
                DefaultRequiredItem;


            bossBodyName =
                DefaultBossBody;


            damageSourceName =
                DefaultDamageSource;


            requiredSecondarySkillToken =
                DefaultRequiredSecondarySkillToken;


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
                        "ScrapItemBossFinisher",
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
                // SCRAP
                // =================================================

                JToken scrapToken =
                    parameters[
                        "scrapAmount"
                    ];


                if (scrapToken != null)
                {
                    try
                    {
                        int configuredAmount =
                            scrapToken.Value<int>();


                        if (configuredAmount > 0)
                        {
                            scrapAmount =
                                configuredAmount;
                        }
                    }
                    catch
                    {
                    }
                }


                // =================================================
                // BODY
                // =================================================

                requiredBodyName =
                    ReadString(
                        parameters,
                        "requiredBody",
                        DefaultRequiredBody
                    );


                // =================================================
                // ITEM
                // =================================================

                requiredItemName =
                    ReadString(
                        parameters,
                        "requiredItem",
                        DefaultRequiredItem
                    );


                // =================================================
                // JEFE
                // =================================================

                bossBodyName =
                    ReadString(
                        parameters,
                        "bossBody",
                        DefaultBossBody
                    );


                // =================================================
                // DAMAGE SOURCE
                // =================================================

                damageSourceName =
                    ReadString(
                        parameters,
                        "finalDamageSource",
                        DefaultDamageSource
                    );


                // =================================================
                // SKILL TOKEN
                // =================================================

                requiredSecondarySkillToken =
                    ReadString(
                        parameters,
                        "requiredSecondarySkillToken",
                        DefaultRequiredSecondarySkillToken
                    );


                return;
            }
        }


        // =========================================================
        // LEER STRING
        // =========================================================

        private static string ReadString(
            JObject parameters,
            string key,
            string fallback
        )
        {
            if (parameters == null)
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


            if (
                string.IsNullOrWhiteSpace(
                    value
                )
            )
            {
                return fallback;
            }


            return value;
        }
    }
}