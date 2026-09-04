using System;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Evalúa si una acción de daño provino del slot de habilidad
    /// configurado y, opcionalmente, si el survivor lleva equipada
    /// una SkillDef concreta en ese slot.
    ///
    /// Se reutiliza para:
    /// - Spy / Daga serrada;
    /// - Tinkaton / Bote explosivo;
    /// - HUNK / Luces fuera;
    /// - futuros presets de habilidad.
    /// </summary>
    public static class MissionRequiredSkillEvaluator
    {
        public static bool IsSatisfied(
            JObject parameters,
            MissionEventContext context
        )
        {
            if (
                parameters == null ||
                context == null ||
                context.DamageReport == null
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


            string slot =
                parameters.Value<string>(
                    "slot"
                );


            string skillToken =
                parameters.Value<string>(
                    "skillToken"
                );


            if (
                string.IsNullOrWhiteSpace(slot) &&
                string.IsNullOrWhiteSpace(skillToken)
            )
            {
                return false;
            }


            GenericSkill genericSkill =
                ResolveSkill(
                    actionBody,
                    slot
                );


            if (
                genericSkill == null ||
                genericSkill.skillDef == null
            )
            {
                return false;
            }


            // Si se configuró un slot, el DamageReport debe indicar
            // que el daño realmente provino de ese slot.
            if (
                !string.IsNullOrWhiteSpace(slot) &&
                !DamageSourceContainsSlot(
                    context.DamageReport,
                    slot
                )
            )
            {
                return false;
            }


            if (
                !string.IsNullOrWhiteSpace(skillToken) &&
                !string.Equals(
                    genericSkill.skillDef.skillNameToken,
                    skillToken.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }


            return true;
        }


        private static GenericSkill ResolveSkill(
            CharacterBody body,
            string slot
        )
        {
            if (
                body == null ||
                body.skillLocator == null ||
                string.IsNullOrWhiteSpace(slot)
            )
            {
                return null;
            }


            switch (
                slot.Trim().ToLowerInvariant()
            )
            {
                case "primary":
                    return body.skillLocator.primary;

                case "secondary":
                    return body.skillLocator.secondary;

                case "utility":
                    return body.skillLocator.utility;

                case "special":
                    return body.skillLocator.special;

                default:
                    return null;
            }
        }


        private static bool DamageSourceContainsSlot(
            DamageReport damageReport,
            string slot
        )
        {
            if (
                damageReport == null ||
                string.IsNullOrWhiteSpace(slot)
            )
            {
                return false;
            }


            int requiredFlag;


            switch (
                slot.Trim().ToLowerInvariant()
            )
            {
                case "primary":
                    requiredFlag = 1;
                    break;

                case "secondary":
                    requiredFlag = 2;
                    break;

                case "utility":
                    requiredFlag = 4;
                    break;

                case "special":
                    requiredFlag = 8;
                    break;

                default:
                    return false;
            }


            int damageSource =
                (int)damageReport
                    .damageInfo
                    .damageType
                    .damageSource;


            if (
                (damageSource & requiredFlag) != 0
            )
            {
                return true;
            }


            /*
             * En algunos BlastAttack el DamageReport puede perder el
             * DamageSource del slot, mientras el BlastAttack activo sí
             * lo conserva. El contexto genérico lo expone para estos
             * casos.
             */
            BlastAttack activeBlast;


            if (
                ExplosionKillTracker
                    .TryGetActiveBlast(
                        out activeBlast
                    ) &&
                activeBlast != null
            )
            {
                int blastDamageSource =
                    (int)activeBlast
                        .damageType
                        .damageSource;


                return
                    (blastDamageSource & requiredFlag) != 0;
            }


            return false;
        }
    }
}
