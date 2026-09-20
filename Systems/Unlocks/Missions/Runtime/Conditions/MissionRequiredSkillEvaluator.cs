using System;
using Newtonsoft.Json.Linq;
using RoR2;
using RoR2.Skills;

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
                context == null
            )
            {
                return false;
            }


            CharacterBody actionBody =
                context.ActionBody ??
                context.DamageReport?.attackerBody ??
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


            string skillName =
                parameters.Value<string>(
                    "skillName"
                );


            if (
                string.IsNullOrWhiteSpace(slot) &&
                string.IsNullOrWhiteSpace(skillToken) &&
                string.IsNullOrWhiteSpace(skillName)
            )
            {
                return false;
            }


            GenericSkill genericSkill;

            if (context.ActivatedSkill != null)
            {
                genericSkill =
                    context.ActivatedSkill;

                // Para eventos UseSkill/Heal no existe DamageReport.
                // La identidad del slot se valida por referencia contra el
                // GenericSkill que acaba de activarse.
                if (!string.IsNullOrWhiteSpace(slot))
                {
                    GenericSkill expectedSlotSkill =
                        ResolveSkill(
                            actionBody,
                            slot
                        );

                    if (!ReferenceEquals(expectedSlotSkill, genericSkill))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (context.DamageReport == null)
                {
                    return false;
                }

                genericSkill =
                    ResolveSkill(
                        actionBody,
                        slot
                    );

                // En acciones de daño, el DamageReport debe indicar que el
                // daño realmente provino del slot configurado.
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
            }


            if (
                genericSkill == null ||
                genericSkill.skillDef == null
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


            /*
             * Algunos mods no publican skillNameToken de forma utilizable,
             * pero sí conservan SkillDef.skillName. G8 puede transportar
             * ambas identidades; si ambas están presentes exigimos que las
             * dos coincidan para no ampliar accidentalmente el requisito.
             */
            if (
                !string.IsNullOrWhiteSpace(skillName)
            )
            {
                string expectedSkillName =
                    skillName.Trim();


                string catalogSkillName =
                    SkillCatalog.GetSkillName(
                        genericSkill.skillDef.skillIndex
                    );


                if (
                    !string.Equals(
                        genericSkill.skillDef.skillName,
                        expectedSkillName,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    !string.Equals(
                        catalogSkillName,
                        expectedSkillName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return false;
                }
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
