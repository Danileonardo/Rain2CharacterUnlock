using System;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Condiciones reutilizables relacionadas con un golpe/daño.
    /// No concede progreso: sólo responde si la acción cumple el
    /// filtro configurado.
    /// </summary>
    public static class MissionCombatConditionEvaluator
    {
        public static bool IsCriticalHit(
            MissionEventContext context
        )
        {
            return
                context?.DamageReport != null &&
                context.DamageReport.damageInfo.crit;
        }


        public static bool IsFatalHit(
            MissionEventContext context
        )
        {
            return
                context != null &&
                context.IsFatalHit;
        }


        public static bool HasMinimumDamage(
            JObject parameters,
            MissionEventContext context
        )
        {
            if (
                parameters == null ||
                context?.DamageReport == null
            )
            {
                return false;
            }


            double requiredDamage =
                parameters.Value<double?>(
                    "damage"
                ) ?? 0d;


            if (requiredDamage < 0d)
            {
                requiredDamage = 0d;
            }


            double actualDamage =
                context.DamageReport.damageDealt;


            return actualDamage >= requiredDamage;
        }


        public static bool IsBackstab(
            MissionEventContext context
        )
        {
            if (
                context?.DamageReport == null ||
                context.TargetBody == null
            )
            {
                return false;
            }


            CharacterBody actionBody =
                context.ActionBody ??
                context.DamageReport.attackerBody ??
                context.PlayerBody;


            if (actionBody == null)
            {
                return false;
            }


            Vector3 attackerToHit =
                context.DamageReport.damageInfo.position -
                actionBody.corePosition;


            if (
                attackerToHit.sqrMagnitude <=
                0.0001f
            )
            {
                return false;
            }


            return BackstabManager.IsBackstab(
                attackerToHit,
                context.TargetBody
            );
        }


        public static bool MatchesDamageType(
            JObject parameters,
            MissionEventContext context
        )
        {
            if (
                parameters == null ||
                context == null
            )
            {
                return false;
            }


            string configured =
                parameters.Value<string>(
                    "damageType"
                );


            if (
                string.IsNullOrWhiteSpace(
                    configured
                )
            )
            {
                return false;
            }


            string normalized =
                configured
                    .Trim()
                    .ToLowerInvariant();


            // Explosive se determina observando un BlastAttack real,
            // reutilizando ExplosionKillTracker. No inferimos una
            // explosión sólo a partir del nombre de una habilidad.
            if (
                normalized == "explosive" ||
                normalized == "blast"
            )
            {
                return context.IsExplosiveDamage;
            }


            // También admitimos los cuatro DamageSource básicos.
            // Esto permite crear filtros de slot sin necesidad de
            // seleccionar una SkillDef específica.
            int flag;


            switch (normalized)
            {
                case "primary":
                    flag = 1;
                    break;

                case "secondary":
                    flag = 2;
                    break;

                case "utility":
                    flag = 4;
                    break;

                case "special":
                    flag = 8;
                    break;

                default:
                    return false;
            }


            if (context.DamageReport == null)
            {
                return false;
            }


            int damageSource =
                (int)context
                    .DamageReport
                    .damageInfo
                    .damageType
                    .damageSource;


            if (
                (damageSource & flag) != 0
            )
            {
                return true;
            }


            if (context.BlastAttack != null)
            {
                int blastDamageSource =
                    (int)context
                        .BlastAttack
                        .damageType
                        .damageSource;


                return
                    (blastDamageSource & flag) != 0;
            }


            return false;
        }
    }
}
