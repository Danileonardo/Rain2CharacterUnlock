using System;
using EntityStates;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Evalúa StatusPresent utilizando las mismas familias que el
    /// catálogo/editor de USU:
    ///
    /// - Buff positivo
    /// - Debuff
    /// - DoT
    /// - Crowd Control
    ///
    /// El caso de Wooper usa:
    /// mode = Specific
    /// statusId = dot:Poison
    /// subject = Target
    /// timing = BeforeAction
    /// </summary>
    public static class MissionStatusPresentEvaluator
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


            string subject =
                parameters.Value<string>(
                    "subject"
                ) ?? "Target";


            CharacterBody body =
                ResolveSubject(
                    subject,
                    context
                );


            if (body == null)
            {
                return false;
            }


            string timing =
                parameters.Value<string>(
                    "timing"
                ) ?? "AtAction";


            // BeforeAction usa una captura tomada justo antes de
            // HealthComponent.TakeDamageProcess. Esto evita que un
            // estado aplicado por el mismo golpe satisfaga la condición.
            if (
                string.Equals(
                    timing,
                    "BeforeAction",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                string snapshotMode =
                    parameters.Value<string>("mode") ?? "Specific";

                if (
                    string.Equals(
                        snapshotMode,
                        "Specific",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return MissionPreDamageStatusTracker
                        .HasStatusBeforeCurrentDamage(
                            body,
                            parameters.Value<string>("statusId")
                        );
                }

                // Los modos agregados seguirán usando AtAction hasta
                // que necesitemos snapshots agregados para ellos.
                return false;
            }


            if (
                !string.Equals(
                    timing,
                    "AtAction",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }


            string mode =
                parameters.Value<string>(
                    "mode"
                ) ?? "Specific";


            switch (
                mode.Trim().ToLowerInvariant()
            )
            {
                case "specific":
                    return HasSpecificStatus(
                        body,
                        parameters.Value<string>(
                            "statusId"
                        )
                    );

                case "anyvalid":
                    return
                        HasAnyNegativeBuff(body) ||
                        HasAnyPositiveBuff(body) ||
                        HasAnyDot(body) ||
                        HasAnyCrowdControl(body);

                case "anynegative":
                    return
                        HasAnyNegativeBuff(body) ||
                        HasAnyDot(body) ||
                        HasAnyCrowdControl(body);

                case "anypositive":
                    return HasAnyPositiveBuff(
                        body
                    );

                case "anydot":
                    return HasAnyDot(
                        body
                    );

                default:
                    return false;
            }
        }


        private static CharacterBody ResolveSubject(
            string subject,
            MissionEventContext context
        )
        {
            switch (
                (subject ?? "Target")
                    .Trim()
                    .ToLowerInvariant()
            )
            {
                case "target":
                case "victim":
                    return context.TargetBody;

                case "player":
                case "owner":
                    return context.PlayerBody;

                case "action":
                case "attacker":
                    return
                        context.ActionBody ??
                        context.PlayerBody;

                default:
                    return null;
            }
        }


        private static bool HasSpecificStatus(
            CharacterBody body,
            string statusId
        )
        {
            if (
                body == null ||
                string.IsNullOrWhiteSpace(
                    statusId
                )
            )
            {
                return false;
            }


            string normalized =
                statusId.Trim();


            if (
                normalized.StartsWith(
                    "dot:",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return HasDot(
                    body,
                    normalized.Substring(4)
                );
            }


            if (
                normalized.StartsWith(
                    "buff:",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return HasBuff(
                    body,
                    normalized.Substring(5)
                );
            }


            if (
                normalized.StartsWith(
                    "cc:",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return HasCrowdControl(
                    body,
                    normalized.Substring(3)
                );
            }


            // Compatibilidad de configuración manual:
            // si no trae prefijo, intentamos Buff y DoT.
            return
                HasBuff(body, normalized) ||
                HasDot(body, normalized) ||
                HasCrowdControl(body, normalized);
        }


        private static bool HasBuff(
            CharacterBody body,
            string internalName
        )
        {
            if (
                body == null ||
                string.IsNullOrWhiteSpace(
                    internalName
                )
            )
            {
                return false;
            }


            BuffIndex[] activeBuffs =
                body.activeBuffsList;


            if (activeBuffs == null)
            {
                return false;
            }


            for (
                int i = 0;
                i < activeBuffs.Length;
                i++
            )
            {
                BuffDef buffDef =
                    BuffCatalog.GetBuffDef(
                        activeBuffs[i]
                    );


                if (
                    buffDef != null &&
                    string.Equals(
                        buffDef.name,
                        internalName,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    body.GetBuffCount(
                        buffDef
                    ) > 0
                )
                {
                    return true;
                }
            }


            return false;
        }


        private static bool HasDot(
            CharacterBody body,
            string dotName
        )
        {
            if (
                body == null ||
                string.IsNullOrWhiteSpace(
                    dotName
                )
            )
            {
                return false;
            }


            DotController dotController =
                DotController.FindDotController(
                    body.gameObject
                );


            if (
                dotController == null ||
                dotController.dotStackList == null
            )
            {
                return false;
            }


            for (
                int i = 0;
                i < dotController.dotStackList.Count;
                i++
            )
            {
                var dotStack =
                    dotController.dotStackList[i];


                string currentName =
                    dotStack.dotIndex.ToString();


                if (
                    string.Equals(
                        currentName,
                        dotName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }


                var dotDef =
                    DotController.GetDotDef(
                        dotStack.dotIndex
                    );


                if (
                    dotDef != null &&
                    dotDef.associatedBuff != null &&
                    string.Equals(
                        dotDef.associatedBuff.name,
                        dotName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }


            return false;
        }


        private static bool HasAnyNegativeBuff(
            CharacterBody body
        )
        {
            if (
                body == null ||
                body.activeBuffsList == null
            )
            {
                return false;
            }


            BuffIndex[] buffs =
                body.activeBuffsList;


            for (
                int i = 0;
                i < buffs.Length;
                i++
            )
            {
                if (
                    StatusEffectScanner.IsNegative(
                        buffs[i]
                    )
                )
                {
                    return true;
                }
            }


            return false;
        }


        private static bool HasAnyPositiveBuff(
            CharacterBody body
        )
        {
            if (
                body == null ||
                body.activeBuffsList == null
            )
            {
                return false;
            }


            BuffIndex[] buffs =
                body.activeBuffsList;


            for (
                int i = 0;
                i < buffs.Length;
                i++
            )
            {
                if (
                    StatusEffectScanner.IsPositive(
                        buffs[i]
                    )
                )
                {
                    return true;
                }
            }


            return false;
        }


        private static bool HasAnyDot(
            CharacterBody body
        )
        {
            if (body == null)
            {
                return false;
            }


            DotController dotController =
                DotController.FindDotController(
                    body.gameObject
                );


            return
                dotController != null &&
                dotController.dotStackList != null &&
                dotController.dotStackList.Count > 0;
        }


        private static bool HasAnyCrowdControl(
            CharacterBody body
        )
        {
            return
                HasCrowdControl(
                    body,
                    "Freeze"
                ) ||
                HasCrowdControl(
                    body,
                    "Stun"
                );
        }


        private static bool HasCrowdControl(
            CharacterBody body,
            string crowdControl
        )
        {
            if (
                body == null ||
                string.IsNullOrWhiteSpace(
                    crowdControl
                )
            )
            {
                return false;
            }


            EntityStateMachine stateMachine =
                null;


            SetStateOnHurt setStateOnHurt =
                body.GetComponent<SetStateOnHurt>();


            if (
                setStateOnHurt != null &&
                setStateOnHurt.targetStateMachine != null
            )
            {
                stateMachine =
                    setStateOnHurt.targetStateMachine;
            }


            if (stateMachine == null)
            {
                stateMachine =
                    EntityStateMachine.FindByCustomName(
                        body.gameObject,
                        "Body"
                    );
            }


            if (stateMachine == null)
            {
                return false;
            }


            if (
                string.Equals(
                    crowdControl,
                    "Freeze",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return stateMachine.state is FrozenState;
            }


            if (
                string.Equals(
                    crowdControl,
                    "Stun",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return stateMachine.state is StunState;
            }


            return false;
        }
    }
}
