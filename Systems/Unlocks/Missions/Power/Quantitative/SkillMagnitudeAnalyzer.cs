using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G4 Phase 1.3.2 - Global Mechanic Calibration.
    ///
    /// Extrae magnitudes observables de la descripción/runtime de cada skill
    /// sin hardcodear survivors. El objetivo es distinguir, por ejemplo, una
    /// habilidad de 120% de otra de 500%, o una skill normal de una con
    /// execute, reset, invulnerabilidad y escalado por objetivo.
    ///
    /// No intenta reconstruir DPS exacto ni simular una run. El score crudo se
    /// normaliza después contra vanilla, por lo que sólo necesita conservar
    /// relaciones razonables y estables entre kits.
    /// </summary>
    public static class SkillMagnitudeAnalyzer
    {
        private static readonly Regex RichTextRegex =
            new Regex(
                "<[^>]*>",
                RegexOptions.Compiled | RegexOptions.CultureInvariant
            );

        private static readonly Regex PercentRegex =
            new Regex(
                @"(?<!\d)(\d+(?:[\.,]\d+)?)\s*%",
                RegexOptions.Compiled | RegexOptions.CultureInvariant
            );

        private static readonly Regex SecondsRegex =
            new Regex(
                @"(?<!\d)(\d+(?:[\.,]\d+)?)\s*(?:s|sec|secs|second|seconds|segundo|segundos)\b",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant
            );

        private static readonly Regex ResourceRegex =
            new Regex(
                @"(?<!\d)(\d+(?:[\.,]\d+)?)\s*(?:mp|ammo|municion|munición|energy|energia|energía)\b",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant
            );

        private static readonly Regex HitCountRegex =
            new Regex(
                @"(?:\bx\s*(\d+)\b|\b(\d+)\s*(?:hits|hit|golpes|golpe|impacts|impact|impactos|impacto|veces)\b)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant
            );

        private static readonly Regex MaximumCountRegex =
            new Regex(
                @"(?:up\s+to|max(?:imum)?\s+of|hasta\s+(?:un\s+)?maximo\s+de)\s*(\d+)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant
            );

        private static readonly Regex ArmorValueRegex =
            new Regex(
                @"(?<!\d)(\d+(?:[\.,]\d+)?)\s*(?:de\s+)?(?:armor|armadura)\b|(?:armor|armadura)\s*(?:(?:of\s+the\s+target|del\s+objetivo)\s*)?(?:by|de|en)?\s*[+-]?\s*(\d+(?:[\.,]\d+)?)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant
            );

        public static SkillMagnitudeProfile Analyze(
            MissionSkillProfile skill
        )
        {
            SkillMagnitudeProfile result =
                new SkillMagnitudeProfile();

            if (skill == null)
            {
                return result;
            }

            result.SkillStableId = skill.StableId ?? "";
            result.Slot = skill.Slot;
            result.DisplayName = skill.DisplayName ?? "";
            result.UsageKind = skill.UsageKind;
            result.ContributesToAggregate = skill.ContributesToAggregate;
            result.RechargeInterval = skill.RechargeInterval;
            result.MaxStock = skill.MaxStock;

            string text =
                NormalizeText(
                    (skill.DisplayName ?? "") + " " +
                    (skill.Description ?? "") + " " +
                    (skill.InternalName ?? "")
                );

            ParsePercentSignals(text, result);
            ParseDurationSignals(text, result);
            ParseResourceSignals(text, result);
            ParseHitCount(text, result);
            ParseDamageMultiplier(text, result);
            ParseStatModifiers(text, result);
            ApplySemanticLeverage(skill, text, result);
            ApplyConditionalSynergy(skill, result);
            CalculateRawContribution(skill, result);
            CalculateConfidence(result);

            result.HasQuantitativeSignal =
                result.Signals.Count > 0 ||
                result.RawPowerContribution > 0f;

            return result;
        }


        private static void ParsePercentSignals(
            string text,
            SkillMagnitudeProfile result
        )
        {
            MatchCollection matches = PercentRegex.Matches(text ?? "");

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                float value;

                if (!TryParseFloat(match.Groups[1].Value, out value))
                {
                    continue;
                }

                string context =
                    GetContext(text, match.Index, match.Length, 78);

                // Las frases de escalado deben estar realmente junto al
                // porcentaje. Con una ventana amplia, un 460% base podía
                // heredar erróneamente el "40% por enemigo" de la oración
                // siguiente (caso genérico observado en kits con escalado).
                string scalingContext =
                    GetContext(text, match.Index, match.Length, 34);

                bool perTarget =
                    ContainsAny(
                        scalingContext,
                        "per enemy",
                        "for each enemy",
                        "per target",
                        "for each target",
                        "por enemigo",
                        "por cada enemigo",
                        "por objetivo",
                        "por cada objetivo"
                    );

                bool perStack =
                    ContainsAny(
                        scalingContext,
                        "per stack",
                        "for each stack",
                        "por acumulacion",
                        "por cada acumulacion",
                        "por carga",
                        "por cada carga"
                    );

                bool maxHealth =
                    ContainsAny(
                        context,
                        "maximum health",
                        "max health",
                        "salud maxima",
                        "vida maxima"
                    ) &&
                    ContainsDamageWord(context);

                bool healing =
                    ContainsAny(
                        context,
                        "heal",
                        "healing",
                        "heals",
                        "restore health",
                        "restores health",
                        "curar",
                        "cura ",
                        "curarse",
                        "curacion",
                        "recupera salud",
                        "restaura salud"
                    );

                bool barrier =
                    ContainsAny(
                        context,
                        "barrier",
                        "barrera"
                    );

                bool chancePercent =
                    ContainsAny(
                        context,
                        " chance",
                        "probability",
                        "probabilidad"
                    );

                bool damage =
                    ContainsDamageWord(context) &&
                    !chancePercent &&
                    !ContainsAny(
                        context,
                        "damage reduction",
                        "reduces damage taken",
                        "reduce el dano recibido",
                        "reduccion de dano"
                    );

                if (maxHealth)
                {
                    result.MaxHealthDamagePercent =
                        Math.Max(result.MaxHealthDamagePercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.MaxHealthDamagePercent,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "target max-health damage percent"
                    );

                    continue;
                }

                if (perTarget && damage)
                {
                    result.ScalingPerTargetPercent =
                        Math.Max(result.ScalingPerTargetPercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.ScalingPerTarget,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "damage scaling per target"
                    );

                    continue;
                }

                if (perStack && damage)
                {
                    result.ScalingPerStackPercent =
                        Math.Max(result.ScalingPerStackPercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.ScalingPerStack,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "damage scaling per stack"
                    );

                    continue;
                }

                if (healing)
                {
                    result.HealingPercent =
                        Math.Max(result.HealingPercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.HealingPercent,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "healing percent"
                    );

                    continue;
                }

                if (barrier)
                {
                    result.BarrierPercent =
                        Math.Max(result.BarrierPercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.BarrierPercent,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "barrier percent"
                    );

                    continue;
                }

                if (damage)
                {
                    if (result.BaseDamagePercent <= 0f)
                    {
                        result.BaseDamagePercent = value;
                    }

                    result.PeakDamagePercent =
                        Math.Max(result.PeakDamagePercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.DamagePercent,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "damage percent"
                    );
                }
            }
        }


        private static void ParseDurationSignals(
            string text,
            SkillMagnitudeProfile result
        )
        {
            MatchCollection matches = SecondsRegex.Matches(text ?? "");

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                float value;

                if (!TryParseFloat(match.Groups[1].Value, out value))
                {
                    continue;
                }

                string context =
                    GetContext(text, match.Index, match.Length, 70);

                if (
                    ContainsAny(
                        context,
                        "invulnerab",
                        "immune to damage",
                        "inmune al dano",
                        "block all damage",
                        "blocks all damage",
                        "bloquear todo el dano",
                        "bloquea todo el dano"
                    )
                )
                {
                    result.InvulnerabilitySeconds =
                        Math.Max(result.InvulnerabilitySeconds, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.InvulnerabilityWindow,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "invulnerability seconds"
                    );
                }

                if (ContainsAny(context, "invisible", "invisibility", "invisibilidad"))
                {
                    result.InvisibilitySeconds =
                        Math.Max(result.InvisibilitySeconds, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.InvisibilityWindow,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "invisibility seconds"
                    );
                }

                if (
                    ContainsAny(
                        context,
                        "stun",
                        "stunned",
                        "freeze",
                        "frozen",
                        "root",
                        "snare",
                        "aturd",
                        "congel",
                        "inmovil"
                    )
                )
                {
                    result.CrowdControlSeconds =
                        Math.Max(result.CrowdControlSeconds, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.CrowdControlDuration,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "crowd-control seconds"
                    );
                }
            }
        }


        private static void ParseResourceSignals(
            string text,
            SkillMagnitudeProfile result
        )
        {
            MatchCollection matches = ResourceRegex.Matches(text ?? "");

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                float value;

                if (!TryParseFloat(match.Groups[1].Value, out value))
                {
                    continue;
                }

                string context =
                    GetContext(text, match.Index, match.Length, 72);

                bool gain =
                    ContainsAny(
                        context,
                        "restore",
                        "restores",
                        "gain",
                        "gains",
                        "replenish",
                        "recupera",
                        "restaura",
                        "obtiene",
                        "gana"
                    );

                bool cost =
                    ContainsAny(
                        context,
                        "cost",
                        "costs",
                        "consume",
                        "consumes",
                        "spend",
                        "gasta",
                        "cuesta",
                        "a cambio de"
                    );

                if (gain)
                {
                    result.ResourceGain =
                        Math.Max(result.ResourceGain, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.ResourceGain,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "resource gain"
                    );
                }

                if (cost)
                {
                    result.ResourceCost =
                        Math.Max(result.ResourceCost, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.ResourceCost,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "resource cost"
                    );
                }
            }
        }


        private static void ParseHitCount(
            string text,
            SkillMagnitudeProfile result
        )
        {
            MatchCollection matches = HitCountRegex.Matches(text ?? "");

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int value = 0;

                if (match.Groups[1].Success)
                {
                    int.TryParse(match.Groups[1].Value, out value);
                }
                else if (match.Groups[2].Success)
                {
                    int.TryParse(match.Groups[2].Value, out value);
                }

                if (value <= 1 || value > 100)
                {
                    continue;
                }

                string context =
                    GetContext(text, match.Index, match.Length, 65);

                if (!ContainsAny(context, "damage", "dano", "hit", "golpe", "attack", "ataque", "impact"))
                {
                    continue;
                }

                result.HitCount = Math.Max(result.HitCount, value);

                AddSignal(
                    result,
                    SkillMagnitudeSignalKind.MultiHit,
                    value,
                    MissionEvidenceConfidence.Low,
                    "multi-hit count"
                );
            }

            MatchCollection maximumMatches =
                MaximumCountRegex.Matches(text ?? "");

            for (int i = 0; i < maximumMatches.Count; i++)
            {
                Match match = maximumMatches[i];
                int value;

                if (!int.TryParse(match.Groups[1].Value, out value))
                {
                    continue;
                }

                if (value <= 1 || value > 100)
                {
                    continue;
                }

                string context =
                    GetContext(text, match.Index, match.Length, 80);

                if (
                    !ContainsAny(
                        context,
                        "hits", "hit", "golpes", "golpe",
                        "impacts", "impact", "impactos", "impacto",
                        "attacks", "ataques"
                    )
                )
                {
                    continue;
                }

                result.HitCount = Math.Max(result.HitCount, value);

                AddSignal(
                    result,
                    SkillMagnitudeSignalKind.MultiHit,
                    value,
                    MissionEvidenceConfidence.Low,
                    "maximum multi-hit count"
                );
            }
        }


        private static void ParseDamageMultiplier(
            string text,
            SkillMagnitudeProfile result
        )
        {
            float multiplier = 1f;

            if (ContainsAny(text, "quadruple damage", "cuadruple dano", "cuatro veces el dano"))
            {
                multiplier = 4f;
            }
            else if (ContainsAny(text, "triple damage", "triple dano", "tres veces el dano"))
            {
                multiplier = 3f;
            }
            else if (ContainsAny(text, "double damage", "doubles damage", "doble dano", "duplica el dano", "el doble de dano"))
            {
                multiplier = 2f;
            }

            if (multiplier <= 1f)
            {
                return;
            }

            result.DamageMultiplier = multiplier;

            AddSignal(
                result,
                SkillMagnitudeSignalKind.DamageMultiplier,
                multiplier,
                MissionEvidenceConfidence.Medium,
                "explicit damage multiplier"
            );
        }


        private static void ParseStatModifiers(
            string text,
            SkillMagnitudeProfile result
        )
        {
            MatchCollection percentMatches =
                PercentRegex.Matches(text ?? "");

            for (int i = 0; i < percentMatches.Count; i++)
            {
                Match match = percentMatches[i];
                float value;

                if (!TryParseFloat(match.Groups[1].Value, out value))
                {
                    continue;
                }

                string context =
                    GetContext(text, match.Index, match.Length, 52);

                if (
                    ContainsAny(context, "attack speed", "velocidad de ataque") &&
                    ContainsAny(context, "gain", "gains", "increase", "bonus", "obtiene", "gana", "aumenta", "otorga", "+")
                )
                {
                    result.AttackSpeedBonusPercent =
                        Math.Max(result.AttackSpeedBonusPercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.AttackSpeedBonusPercent,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "attack-speed bonus percent"
                    );
                }

                if (
                    ContainsAny(context, "movement speed", "move speed", "velocidad de movimiento") &&
                    ContainsAny(context, "gain", "gains", "increase", "bonus", "obtiene", "gana", "aumenta", "otorga", "+")
                )
                {
                    result.MoveSpeedBonusPercent =
                        Math.Max(result.MoveSpeedBonusPercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.MoveSpeedBonusPercent,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "movement-speed bonus percent"
                    );
                }

                if (
                    ContainsAny(
                        context,
                        "damage reduction",
                        "reduces damage taken",
                        "reduced damage taken",
                        "reduccion de dano",
                        "reduce el dano recibido"
                    )
                )
                {
                    result.DamageReductionPercent =
                        Math.Max(result.DamageReductionPercent, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.DamageReductionPercent,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "damage-reduction percent"
                    );
                }
            }

            MatchCollection armorMatches =
                ArmorValueRegex.Matches(text ?? "");

            for (int i = 0; i < armorMatches.Count; i++)
            {
                Match match = armorMatches[i];
                string rawValue =
                    match.Groups[1].Success
                        ? match.Groups[1].Value
                        : match.Groups[2].Value;

                float value;

                if (!TryParseFloat(rawValue, out value))
                {
                    continue;
                }

                string context =
                    GetContext(text, match.Index, match.Length, 62);

                bool reduction =
                    ContainsAny(
                        context,
                        "reduce armor", "reduces armor", "lower armor",
                        "reduces the armor", "armor reduction",
                        "reduce la armadura", "reduce armadura",
                        "reduccion de armadura"
                    );

                if (reduction)
                {
                    result.ArmorReductionFlat =
                        Math.Max(result.ArmorReductionFlat, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.ArmorReductionFlat,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "armor reduction"
                    );
                }
                else if (
                    ContainsAny(
                        context,
                        "gain", "gains", "grant", "grants",
                        "obtiene", "gana", "otorga", "concede", "+"
                    )
                )
                {
                    result.ArmorBonusFlat =
                        Math.Max(result.ArmorBonusFlat, value);

                    AddSignal(
                        result,
                        SkillMagnitudeSignalKind.ArmorBonusFlat,
                        value,
                        MissionEvidenceConfidence.Medium,
                        "armor bonus"
                    );
                }
            }
        }


        private static void ApplySemanticLeverage(
            MissionSkillProfile skill,
            string text,
            SkillMagnitudeProfile result
        )
        {
            if (HasEffect(skill, MissionSkillEffectKind.CriticalHit))
            {
                result.CriticalLeverage = 0.35f;

                if (
                    ContainsAny(
                        text,
                        "guaranteed critical",
                        "guarantees a critical",
                        "always critical",
                        "asesta un golpe critico",
                        "golpe critico garantizado",
                        "critico garantizado"
                    )
                )
                {
                    result.CriticalLeverage = 0.65f;
                }

                AddSignal(
                    result,
                    SkillMagnitudeSignalKind.CriticalLeverage,
                    result.CriticalLeverage,
                    MissionEvidenceConfidence.Medium,
                    "critical-hit leverage"
                );
            }

            if (HasEffect(skill, MissionSkillEffectKind.Backstab))
            {
                result.CriticalLeverage =
                    Math.Max(result.CriticalLeverage, 0.50f);
            }

            bool executionText =
                ContainsAny(
                    text,
                    "instantly kill",
                    "instantly kills",
                    "kill instantly",
                    "mata al instante",
                    "matar al instante",
                    "execute",
                    "execution"
                );

            if (
                HasEffect(skill, MissionSkillEffectKind.Execution) ||
                executionText
            )
            {
                result.ExecutionLeverage =
                    executionText ? 0.90f : 0.55f;

                AddSignal(
                    result,
                    SkillMagnitudeSignalKind.ExecutionLeverage,
                    result.ExecutionLeverage,
                    MissionEvidenceConfidence.Medium,
                    "execution leverage"
                );
            }

            bool reset =
                ContainsAny(
                    text,
                    "reset the cooldown",
                    "resets the cooldown",
                    "reset cooldown",
                    "reinicia el enfriamiento",
                    "reiniciar el enfriamiento",
                    "reinicia el tiempo de recarga",
                    "reiniciar el tiempo de recarga"
                );

            bool reduction =
                ContainsAny(
                    text,
                    "cooldown reduction",
                    "reduce cooldown",
                    "reduces cooldown",
                    "reduce el enfriamiento",
                    "reduce el tiempo de recarga",
                    "reduccion de enfriamiento",
                    "reduccion del tiempo de recarga"
                );

            if (reset)
            {
                result.CooldownLeverage = 0.60f;

                AddSignal(
                    result,
                    SkillMagnitudeSignalKind.CooldownReset,
                    1f,
                    MissionEvidenceConfidence.Medium,
                    "cooldown reset/refund"
                );
            }
            else if (reduction)
            {
                result.CooldownLeverage = 0.30f;

                AddSignal(
                    result,
                    SkillMagnitudeSignalKind.CooldownEfficiency,
                    1f,
                    MissionEvidenceConfidence.Medium,
                    "cooldown reduction"
                );
            }

            if (
                HasEffect(skill, MissionSkillEffectKind.AreaEffect) ||
                ContainsAny(
                    text,
                    "nearby enemies",
                    "all nearby",
                    "large area",
                    "wide area",
                    "enemies nearby",
                    "enemigos cercanos",
                    "zona amplia",
                    "gran area",
                    "todo lo cercano"
                )
            )
            {
                result.AreaLeverage = 0.25f;

                AddSignal(
                    result,
                    SkillMagnitudeSignalKind.AreaCoverage,
                    1f,
                    MissionEvidenceConfidence.Low,
                    "area/multi-target coverage"
                );
            }
        }


        private static void ApplyConditionalSynergy(
            MissionSkillProfile skill,
            SkillMagnitudeProfile result
        )
        {
            if (skill == null || result == null)
            {
                return;
            }

            bool hasDamage =
                result.PeakDamagePercent > 0f ||
                HasEffect(skill, MissionSkillEffectKind.Damage);

            bool hasCritical =
                result.CriticalLeverage > 0f ||
                HasEffect(skill, MissionSkillEffectKind.CriticalHit);

            bool hasBackstab =
                HasEffect(skill, MissionSkillEffectKind.Backstab);

            bool hasExecution =
                result.ExecutionLeverage > 0f ||
                HasEffect(skill, MissionSkillEffectKind.Execution);

            bool hasCooldownPayoff = result.CooldownLeverage > 0f;
            bool hasScaling =
                result.ScalingPerTargetPercent > 0f ||
                result.ScalingPerStackPercent > 0f ||
                result.DamageMultiplier > 1f ||
                result.MaxHealthDamagePercent > 0f;

            float synergy = 0f;

            // Burst condicionado: daño que además puede criticar/ejecutar.
            if (hasDamage && hasCritical) synergy += 0.18f;
            if (hasDamage && hasExecution) synergy += 0.28f;
            if (hasDamage && hasBackstab) synergy += 0.16f;

            // La combinación backstab + crítico/execute representa una
            // recompensa de posicionamiento que no se ve sumando flags.
            if (hasBackstab && hasCritical) synergy += 0.18f;
            if (hasBackstab && hasExecution) synergy += 0.22f;

            // Reset/reducción sobre una habilidad ofensiva aumenta el uptime.
            if (hasDamage && hasCooldownPayoff) synergy += 0.18f;

            if (hasDamage && hasScaling) synergy += 0.14f;

            if (
                result.InvulnerabilitySeconds > 0f &&
                hasCooldownPayoff
            )
            {
                synergy += 0.10f;
            }

            if (
                result.HealingPercent > 0f &&
                result.ResourceGain > 0f
            )
            {
                synergy += 0.08f;
            }

            synergy = Clamp(synergy, 0f, 1.15f);

            if (synergy <= 0f)
            {
                return;
            }

            result.SynergyLeverage = synergy;

            AddSignal(
                result,
                SkillMagnitudeSignalKind.ConditionalSynergy,
                synergy,
                MissionEvidenceConfidence.Medium,
                "conditional mechanic synergy"
            );
        }


        private static void CalculateRawContribution(
            MissionSkillProfile skill,
            SkillMagnitudeProfile result
        )
        {
            float damage = 0f;

            if (result.PeakDamagePercent > 0f)
            {
                float effectiveDamage =
                    result.PeakDamagePercent *
                    Math.Max(1f, result.DamageMultiplier);

                damage =
                    (float)Math.Sqrt(
                        Math.Max(0f, effectiveDamage) / 100f
                    );

                if (result.HitCount > 1)
                {
                    damage *=
                        Math.Min(
                            1.65f,
                            (float)Math.Sqrt(result.HitCount)
                        );
                }
            }

            float maxHealth =
                result.MaxHealthDamagePercent > 0f
                    ? Math.Min(
                        1.75f,
                        0.55f *
                        (float)Math.Sqrt(
                            result.MaxHealthDamagePercent / 5f
                        )
                    )
                    : 0f;

            float healing =
                result.HealingPercent > 0f
                    ? Math.Min(
                        1.35f,
                        0.55f *
                        (float)Math.Sqrt(
                            result.HealingPercent / 10f
                        )
                    )
                    : 0f;

            float barrier =
                result.BarrierPercent > 0f
                    ? Math.Min(
                        0.90f,
                        0.35f *
                        (float)Math.Sqrt(
                            result.BarrierPercent / 10f
                        )
                    )
                    : 0f;

            float invulnerability =
                Math.Min(
                    1.00f,
                    result.InvulnerabilitySeconds * 0.85f
                );

            float invisibility =
                Math.Min(
                    0.75f,
                    result.InvisibilitySeconds * 0.09f
                );

            float crowdControl =
                Math.Min(
                    0.80f,
                    result.CrowdControlSeconds * 0.18f
                );

            float perTarget =
                result.ScalingPerTargetPercent > 0f
                    ? Math.Min(
                        0.90f,
                        0.35f *
                        (float)Math.Sqrt(
                            result.ScalingPerTargetPercent / 25f
                        )
                    )
                    : 0f;

            float perStack =
                result.ScalingPerStackPercent > 0f
                    ? Math.Min(
                        0.70f,
                        0.28f *
                        (float)Math.Sqrt(
                            result.ScalingPerStackPercent / 20f
                        )
                    )
                    : 0f;

            float attackSpeedBonus =
                result.AttackSpeedBonusPercent > 0f
                    ? Math.Min(0.70f, result.AttackSpeedBonusPercent / 120f)
                    : 0f;

            float moveSpeedBonus =
                result.MoveSpeedBonusPercent > 0f
                    ? Math.Min(0.45f, result.MoveSpeedBonusPercent / 150f)
                    : 0f;

            float armorBonus =
                result.ArmorBonusFlat > 0f
                    ? Math.Min(0.70f, result.ArmorBonusFlat / 120f)
                    : 0f;

            float armorReduction =
                result.ArmorReductionFlat > 0f
                    ? Math.Min(0.65f, result.ArmorReductionFlat / 80f)
                    : 0f;

            float damageReduction =
                result.DamageReductionPercent > 0f
                    ? Math.Min(0.85f, result.DamageReductionPercent / 70f)
                    : 0f;

            float damageAmplification =
                result.DamageMultiplier > 1f
                    ? Math.Min(0.85f, (result.DamageMultiplier - 1f) * 0.45f)
                    : 0f;

            float resourceEconomy =
                result.ResourceGain > 0f
                    ? Math.Min(0.35f, result.ResourceGain / 50f)
                    : 0f;

            if (result.ResourceCost > 0f && result.ResourceGain <= 0f)
            {
                resourceEconomy -=
                    Math.Min(0.12f, result.ResourceCost / 200f);
            }

            float baseContribution =
                damage +
                maxHealth +
                healing +
                barrier +
                invulnerability +
                invisibility +
                crowdControl +
                perTarget +
                perStack +
                attackSpeedBonus +
                moveSpeedBonus +
                armorBonus +
                armorReduction +
                damageReduction +
                damageAmplification +
                resourceEconomy +
                result.CriticalLeverage +
                result.ExecutionLeverage +
                result.CooldownLeverage +
                result.AreaLeverage +
                result.SynergyLeverage;

            if (baseContribution <= 0f)
            {
                result.RawPowerContribution = 0f;
                return;
            }

            result.RawPowerContribution =
                Math.Max(
                    0f,
                    baseContribution *
                    AvailabilityMultiplier(skill)
                );
        }


        private static float AvailabilityMultiplier(
            MissionSkillProfile skill
        )
        {
            if (skill == null)
            {
                return 1f;
            }

            float usage = 1f;

            if (skill.UsageKind == MissionSkillUsageKind.Passive)
            {
                usage = 0.95f;
            }
            else if (skill.UsageKind == MissionSkillUsageKind.AuxiliaryActive)
            {
                usage = 0.85f;
            }

            float cooldown = 1f;

            if (skill.UsageKind != MissionSkillUsageKind.Passive)
            {
                if (skill.RechargeInterval <= 0f)
                {
                    cooldown = 1.15f;
                }
                else if (skill.RechargeInterval <= 2f)
                {
                    cooldown = 1.10f;
                }
                else if (skill.RechargeInterval <= 6f)
                {
                    cooldown = 1.00f;
                }
                else if (skill.RechargeInterval <= 12f)
                {
                    cooldown = 0.85f;
                }
                else
                {
                    cooldown = 0.70f;
                }
            }

            int extraStocks = Math.Max(0, skill.MaxStock - 1);
            float stock = 1f + Math.Min(0.24f, extraStocks * 0.06f);

            return usage * cooldown * stock;
        }


        private static void CalculateConfidence(
            SkillMagnitudeProfile result
        )
        {
            if (result == null)
            {
                return;
            }

            if (result.Signals.Count == 0)
            {
                result.Confidence = 0f;
                return;
            }

            float weighted = 0f;

            for (int i = 0; i < result.Signals.Count; i++)
            {
                SkillMagnitudeSignal signal = result.Signals[i];

                if (signal == null)
                {
                    continue;
                }

                switch (signal.Confidence)
                {
                    case MissionEvidenceConfidence.High:
                        weighted += 1.00f;
                        break;

                    case MissionEvidenceConfidence.Medium:
                        weighted += 0.78f;
                        break;

                    default:
                        weighted += 0.50f;
                        break;
                }
            }

            float average = weighted / Math.Max(1, result.Signals.Count);
            float breadth = Math.Min(1f, result.Signals.Count / 4f);

            result.Confidence =
                Clamp(
                    35f +
                    average * 45f +
                    breadth * 20f,
                    0f,
                    100f
                );
        }


        private static void AddSignal(
            SkillMagnitudeProfile result,
            SkillMagnitudeSignalKind kind,
            float value,
            MissionEvidenceConfidence confidence,
            string detail
        )
        {
            if (result == null || kind == SkillMagnitudeSignalKind.None)
            {
                return;
            }

            for (int i = 0; i < result.Signals.Count; i++)
            {
                SkillMagnitudeSignal existing = result.Signals[i];

                if (
                    existing != null &&
                    existing.Kind == kind &&
                    Math.Abs(existing.Value - value) < 0.001f
                )
                {
                    return;
                }
            }

            result.Signals.Add(
                new SkillMagnitudeSignal
                {
                    Kind = kind,
                    Value = value,
                    Confidence = confidence,
                    Detail = detail ?? ""
                }
            );
        }


        private static bool HasEffect(
            MissionSkillProfile skill,
            MissionSkillEffectKind kind
        )
        {
            if (skill == null)
            {
                return false;
            }

            for (int i = 0; i < skill.Effects.Count; i++)
            {
                MissionSkillEffectProfile effect = skill.Effects[i];

                if (effect != null && effect.Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }


        private static bool ContainsDamageWord(
            string text
        )
        {
            return ContainsAny(
                text,
                "damage",
                "dano"
            );
        }


        private static string NormalizeText(
            string value
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string withoutTags = RichTextRegex.Replace(value, " ");
            string decomposed =
                withoutTags.ToLowerInvariant().Normalize(
                    NormalizationForm.FormD
                );

            StringBuilder builder =
                new StringBuilder(decomposed.Length);

            for (int i = 0; i < decomposed.Length; i++)
            {
                char character = decomposed[i];

                if (
                    CharUnicodeInfo.GetUnicodeCategory(character) !=
                    UnicodeCategory.NonSpacingMark
                )
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }


        private static string GetContext(
            string text,
            int index,
            int length,
            int radius
        )
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            int start = Math.Max(0, index - radius);
            int end = Math.Min(text.Length, index + length + radius);

            if (end <= start)
            {
                return "";
            }

            return text.Substring(start, end - start);
        }


        private static bool TryParseFloat(
            string value,
            out float result
        )
        {
            result = 0f;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return float.TryParse(
                value.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result
            );
        }


        private static bool ContainsAny(
            string text,
            params string[] patterns
        )
        {
            if (string.IsNullOrEmpty(text) || patterns == null)
            {
                return false;
            }

            for (int i = 0; i < patterns.Length; i++)
            {
                string pattern = patterns[i];

                if (
                    !string.IsNullOrEmpty(pattern) &&
                    text.IndexOf(
                        pattern,
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    return true;
                }
            }

            return false;
        }


        private static float Clamp(
            float value,
            float minimum,
            float maximum
        )
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }
    }
}
