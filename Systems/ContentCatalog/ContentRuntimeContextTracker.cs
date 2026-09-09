using System;
using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// 5G.2D-B
    ///
    /// Distingue la identidad estática de un body de su contexto durante
    /// una run.
    ///
    /// Ejemplos:
    /// - StoneTitanBody: Champion estático.
    /// - una instancia de StoneTitanBody: puede ser TeleporterBoss + Elite.
    /// - CommandoBody enemigo de Vengeance: Umbra.
    ///
    /// Esta clase todavía NO cambia ninguna misión. Expone el dato para que
    /// Mission Schema/Editor puedan consumirlo sin volver a implementar la
    /// detección en cada tracker.
    /// </summary>
    public static class ContentRuntimeContextTracker
    {
        private static readonly HashSet<CharacterMaster>
            TeleporterBossMasters =
                new HashSet<CharacterMaster>();

        private static readonly HashSet<string>
            SurvivorBodyNames =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static ManualLogSource logger;
        private static bool initialized;
        private static BossGroup activeTeleporterBossGroup;

        public static void Initialize(
            ManualLogSource activeLogger
        )
        {
            logger =
                activeLogger;

            if (initialized)
            {
                return;
            }

            initialized =
                true;

            BuildSurvivorBodyNameCache();

            On.RoR2.BossGroup.OnMemberAddedServer +=
                BossGroup_OnMemberAddedServer;

            logger?.LogInfo(
                "[CONTENT CONTEXT] Runtime context tracker inicializado | " +
                "TeleporterBoss / Elite / Umbra / Summon."
            );
        }

        public static ContentRuntimeTargetFlags GetFlags(
            CharacterBody body
        )
        {
            if (body == null)
            {
                return
                    ContentRuntimeTargetFlags.None;
            }

            ContentRuntimeTargetFlags flags =
                ContentRuntimeTargetFlags.None;

            if (IsTeleporterBoss(body))
            {
                flags |=
                    ContentRuntimeTargetFlags.TeleporterBoss;
            }

            if (IsElite(body))
            {
                flags |=
                    ContentRuntimeTargetFlags.Elite;
            }

            if (IsUmbra(body))
            {
                flags |=
                    ContentRuntimeTargetFlags.Umbra;
            }

            if (IsSummon(body))
            {
                flags |=
                    ContentRuntimeTargetFlags.Summon;
            }

            return flags;
        }

        public static bool IsTeleporterBoss(
            CharacterBody body
        )
        {
            if (
                body == null ||
                body.master == null
            )
            {
                return false;
            }

            CleanupDestroyedMasters();

            return
                TeleporterBossMasters.Contains(
                    body.master
                );
        }

        public static bool IsElite(
            CharacterBody body
        )
        {
            return
                ContentCatalogReflection
                    .ReadBool(
                        body,
                        false,
                        "isElite"
                    );
        }

        public static bool IsSummon(
            CharacterBody body
        )
        {
            if (
                body == null ||
                body.master == null
            )
            {
                return false;
            }

            try
            {
                MinionOwnership ownership =
                    body.master.minionOwnership;

                return
                    ownership != null &&
                    ownership.ownerMaster != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsUmbra(
            CharacterBody body
        )
        {
            if (body == null)
            {
                return false;
            }

            if (SurvivorBodyNames.Count == 0)
            {
                BuildSurvivorBodyNameCache();
            }

            string bodyName =
                NormalizeRuntimeBodyName(
                    body.gameObject != null
                        ? body.gameObject.name
                        : ""
                );

            if (
                string.IsNullOrWhiteSpace(bodyName) ||
                !SurvivorBodyNames.Contains(bodyName)
            )
            {
                return false;
            }

            /*
             * Un survivor real controlado por jugador no es Umbra.
             */
            bool isPlayerControlled =
                ContentCatalogReflection
                    .ReadBool(
                        body,
                        false,
                        "isPlayerControlled"
                    );

            if (isPlayerControlled)
            {
                return false;
            }

            TeamComponent teamComponent =
                body.GetComponent<TeamComponent>();

            return
                teamComponent != null &&
                teamComponent.teamIndex ==
                TeamIndex.Monster;
        }

        private static void BossGroup_OnMemberAddedServer(
            On.RoR2.BossGroup.orig_OnMemberAddedServer orig,
            BossGroup self,
            CharacterMaster memberMaster
        )
        {
            orig(
                self,
                memberMaster
            );

            if (
                self == null ||
                memberMaster == null ||
                !IsTeleporterBossGroup(self)
            )
            {
                return;
            }

            /*
             * Al encontrar un BossGroup de otro teleporter/sector,
             * descartamos referencias de la etapa anterior.
             */
            if (activeTeleporterBossGroup != self)
            {
                activeTeleporterBossGroup =
                    self;

                TeleporterBossMasters.Clear();
            }

            TeleporterBossMasters.Add(
                memberMaster
            );

            UsuLog.Verbose(
                logger,
                "[CONTENT CONTEXT] TeleporterBoss registrado | " +
                "Master: " +
                memberMaster.name +
                " | Total grupo: " +
                TeleporterBossMasters.Count
            );
        }

        private static bool IsTeleporterBossGroup(
            BossGroup bossGroup
        )
        {
            if (bossGroup == null)
            {
                return false;
            }

            TeleporterInteraction teleporter =
                TeleporterInteraction.instance;

            if (teleporter == null)
            {
                return false;
            }

            /*
             * En el teleporter vanilla/modded convencional, BossGroup y
             * TeleporterInteraction viven en el mismo GameObject.
             */
            if (
                bossGroup.gameObject ==
                teleporter.gameObject
            )
            {
                return true;
            }

            TeleporterInteraction localTeleporter =
                bossGroup.GetComponent<TeleporterInteraction>();

            return
                localTeleporter != null &&
                localTeleporter == teleporter;
        }

        private static void CleanupDestroyedMasters()
        {
            if (TeleporterBossMasters.Count == 0)
            {
                return;
            }

            TeleporterBossMasters.RemoveWhere(
                master =>
                    master == null
            );
        }

        private static void BuildSurvivorBodyNameCache()
        {
            SurvivorBodyNames.Clear();

            SurvivorDef[] survivors =
                SurvivorCatalog.survivorDefs;

            if (survivors == null)
            {
                return;
            }

            for (
                int i = 0;
                i < survivors.Length;
                i++
            )
            {
                SurvivorDef survivor =
                    survivors[i];

                if (
                    survivor == null ||
                    survivor.bodyPrefab == null
                )
                {
                    continue;
                }

                string bodyName =
                    NormalizeRuntimeBodyName(
                        survivor.bodyPrefab.name
                    );

                if (!string.IsNullOrWhiteSpace(bodyName))
                {
                    SurvivorBodyNames.Add(
                        bodyName
                    );
                }
            }
        }

        private static string NormalizeRuntimeBodyName(
            string value
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            const string cloneSuffix =
                "(Clone)";

            string result =
                value.Trim();

            if (
                result.EndsWith(
                    cloneSuffix,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                result =
                    result.Substring(
                        0,
                        result.Length -
                        cloneSuffix.Length
                    ).Trim();
            }

            return result;
        }
    }
}
