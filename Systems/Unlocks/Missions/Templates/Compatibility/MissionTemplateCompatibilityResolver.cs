using System;
using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G6 Phase 1 - Mission Template Compatibility.
    ///
    /// Decide qué FAMILIAS de misión tienen sentido para el target. No genera
    /// una misión, no elige cantidades y no modifica unlock/provider.
    ///
    /// Principios:
    /// - el kit/semántica de G3 es la señal principal;
    /// - G5 sólo aporta una pequeña preferencia por complejidad apropiada;
    /// - el target nunca cuenta como performer de su propio unlock;
    /// - se comprueba que existan otros survivors capaces de representar la
    ///   mecánica cuando la familia lo necesita;
    /// - no hay excepciones por nombre de survivor.
    /// </summary>
    public static class MissionTemplateCompatibilityResolver
    {
        public const int ResolverVersion = 1;

        private static readonly MissionTemplateFamily[] Families =
        {
            MissionTemplateFamily.CombatKills,
            MissionTemplateFamily.BossFinisher,
            MissionTemplateFamily.Execution,
            MissionTemplateFamily.CriticalHit,
            MissionTemplateFamily.Backstab,
            MissionTemplateFamily.StatusEffect,
            MissionTemplateFamily.HealingSupport,
            MissionTemplateFamily.DefenseSurvival,
            MissionTemplateFamily.MobilityTraversal,
            MissionTemplateFamily.AirborneCombat,
            MissionTemplateFamily.SummonMinion,
            MissionTemplateFamily.ResourceMastery,
            MissionTemplateFamily.MeleeCombat,
            MissionTemplateFamily.ProjectileCombat,
            MissionTemplateFamily.AreaControl,
            MissionTemplateFamily.SelfBuffMastery
        };


        public static List<MissionTemplateCompatibilityAssessment> Resolve(
            IReadOnlyList<MissionProfile> profiles,
            IReadOnlyList<ChallengeTierAssessment> challengeAssessments
        )
        {
            List<MissionTemplateCompatibilityAssessment> result =
                new List<MissionTemplateCompatibilityAssessment>();

            if (profiles == null)
            {
                return result;
            }

            for (int profileIndex = 0; profileIndex < profiles.Count; profileIndex++)
            {
                MissionProfile profile = profiles[profileIndex];

                if (profile == null)
                {
                    continue;
                }

                ChallengeTierAssessment challenge =
                    FindChallengeAssessment(profile, challengeAssessments);

                for (int familyIndex = 0; familyIndex < Families.Length; familyIndex++)
                {
                    MissionTemplateFamily family = Families[familyIndex];

                    float themeScore =
                        ResolveThemeScore(profile, family);

                    int alternativePerformers;
                    int unlockedAlternativePerformers;

                    ResolveAlternativePerformers(
                        profile,
                        family,
                        profiles,
                        out alternativePerformers,
                        out unlockedAlternativePerformers
                    );

                    float feasibilityScore =
                        ResolvePerformerFeasibilityScore(
                            family,
                            alternativePerformers,
                            unlockedAlternativePerformers
                        );

                    ChallengeTier challengeTier =
                        challenge != null
                            ? challenge.ResolvedTier
                            : ChallengeTier.Tier3;

                    GameplayComplexityLevel gameplayComplexity =
                        challenge != null
                            ? challenge.GameplayComplexity
                            : GameplayComplexityLevel.Level3;

                    float tierFitScore =
                        ResolveTierFitScore(
                            family,
                            challengeTier
                        );

                    /*
                     * La temática manda. La factibilidad sirve para no crear
                     * desafíos que requieran una mecánica que ningún otro
                     * survivor instalado puede ejecutar. ChallengeTier es sólo
                     * un 10% del encaje: nunca decide la familia principal.
                     */
                    float compatibilityScore =
                        Clamp(
                            themeScore * 0.70f +
                            feasibilityScore * 0.20f +
                            tierFitScore * 0.10f,
                            0f,
                            100f
                        );

                    /*
                     * Si el target no presenta realmente la mecánica, una alta
                     * factibilidad externa no debe transformar una familia no
                     * temática en una recomendación válida.
                     */
                    if (themeScore < 35f)
                    {
                        compatibilityScore =
                            Math.Min(compatibilityScore, 49f);
                    }

                    MissionTemplateCompatibilityAssessment assessment =
                        new MissionTemplateCompatibilityAssessment
                        {
                            ResolverVersion = ResolverVersion,
                            SurvivorId = profile.SurvivorId ?? "",
                            BodyName = profile.BodyName ?? "",
                            DisplayName = profile.DisplayName ?? "",
                            IsModded = profile.IsModded,
                            HasOriginalUnlock = profile.HasOriginalUnlock,
                            LacksOriginalUnlock =
                                profile.IsModded && !profile.HasOriginalUnlock,
                            Family = family,
                            ThemeScore = themeScore,
                            PerformerFeasibilityScore = feasibilityScore,
                            TierFitScore = tierFitScore,
                            CompatibilityScore = compatibilityScore,
                            CompatibilityLevel =
                                ResolveCompatibilityLevel(compatibilityScore),
                            ChallengeTier = challengeTier,
                            GameplayComplexity = gameplayComplexity,
                            RequiresTargetUsage = false,
                            AlternativePerformerCount = alternativePerformers,
                            UnlockedAlternativePerformerCount =
                                unlockedAlternativePerformers
                        };

                    AddSupportingSignals(
                        profile,
                        family,
                        assessment
                    );

                    result.Add(assessment);
                }
            }

            return result;
        }


        private static ChallengeTierAssessment FindChallengeAssessment(
            MissionProfile profile,
            IReadOnlyList<ChallengeTierAssessment> assessments
        )
        {
            if (profile == null || assessments == null)
            {
                return null;
            }

            for (int i = 0; i < assessments.Count; i++)
            {
                ChallengeTierAssessment assessment = assessments[i];

                if (assessment == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(profile.SurvivorId) &&
                    string.Equals(
                        profile.SurvivorId,
                        assessment.TargetId,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    return assessment;
                }

                if (!string.IsNullOrWhiteSpace(profile.BodyName) &&
                    string.Equals(
                        profile.BodyName,
                        assessment.BodyName,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    return assessment;
                }
            }

            return null;
        }


        private static float ResolveThemeScore(
            MissionProfile profile,
            MissionTemplateFamily family
        )
        {
            if (profile == null)
            {
                return 0f;
            }

            float damage = CapabilityStrength(profile, MissionProfileCapability.Damage);
            float mobility = CapabilityStrength(profile, MissionProfileCapability.Mobility);
            float airborne = CapabilityStrength(profile, MissionProfileCapability.Airborne);
            float status = CapabilityStrength(profile, MissionProfileCapability.StatusEffect);
            float summon = CapabilityStrength(profile, MissionProfileCapability.SummonMinion);
            float healing = CapabilityStrength(profile, MissionProfileCapability.Healing);
            float defense = CapabilityStrength(profile, MissionProfileCapability.Defense);
            float selfBuff = CapabilityStrength(profile, MissionProfileCapability.SelfBuff);
            float resource = CapabilityStrength(profile, MissionProfileCapability.ResourceMechanic);
            float critical = CapabilityStrength(profile, MissionProfileCapability.CriticalHit);
            float backstab = CapabilityStrength(profile, MissionProfileCapability.Backstab);
            float execution = CapabilityStrength(profile, MissionProfileCapability.Execution);

            float melee = EffectStrength(profile, MissionSkillEffectKind.Melee);
            float projectile = EffectStrength(profile, MissionSkillEffectKind.Projectile);
            float area = EffectStrength(profile, MissionSkillEffectKind.AreaEffect);
            float crowdControl = EffectStrength(profile, MissionSkillEffectKind.CrowdControl);

            switch (family)
            {
                case MissionTemplateFamily.CombatKills:
                    /*
                     * Fallback universal deliberadamente moderado. Si existen
                     * señales más características del kit, deben superar a
                     * "mata N enemigos" en el ranking de G6.
                     */
                    return Clamp(
                        15f + 50f * damage + 5f * Max(melee, projectile, area),
                        0f,
                        100f
                    );

                case MissionTemplateFamily.BossFinisher:
                    return Clamp(
                        10f +
                        45f * damage +
                        20f * execution +
                        10f * critical +
                        10f * backstab,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.Execution:
                    return Clamp(
                        5f + 85f * execution + 10f * damage,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.CriticalHit:
                    return Clamp(
                        5f + 90f * critical + 5f * damage,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.Backstab:
                    return Clamp(
                        5f + 90f * backstab + 5f * damage,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.StatusEffect:
                    return Clamp(
                        10f + 80f * status + 10f * crowdControl,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.HealingSupport:
                    return Clamp(
                        5f + 90f * healing + 5f * summon,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.DefenseSurvival:
                    return Clamp(
                        10f + 80f * defense + 10f * selfBuff,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.MobilityTraversal:
                    return Clamp(
                        10f + 80f * mobility + 10f * airborne,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.AirborneCombat:
                    return Clamp(
                        5f + 65f * airborne + 20f * damage + 10f * mobility,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.SummonMinion:
                    return Clamp(
                        5f + 90f * summon + 5f * healing,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.ResourceMastery:
                    return Clamp(
                        5f + 90f * resource + 5f * selfBuff,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.MeleeCombat:
                    return Clamp(
                        10f + 80f * melee + 10f * damage,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.ProjectileCombat:
                    return Clamp(
                        10f + 80f * projectile + 10f * damage,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.AreaControl:
                    return Clamp(
                        5f +
                        40f * area +
                        30f * crowdControl +
                        25f * status,
                        0f,
                        100f
                    );

                case MissionTemplateFamily.SelfBuffMastery:
                    return Clamp(
                        5f + 85f * selfBuff + 10f * resource,
                        0f,
                        100f
                    );

                default:
                    return 0f;
            }
        }


        private static float ResolvePerformerFeasibilityScore(
            MissionTemplateFamily family,
            int alternativePerformers,
            int unlockedAlternativePerformers
        )
        {
            if (family == MissionTemplateFamily.CombatKills ||
                family == MissionTemplateFamily.BossFinisher)
            {
                if (unlockedAlternativePerformers >= 1)
                {
                    return 100f;
                }

                if (alternativePerformers >= 1)
                {
                    return 90f;
                }

                return 75f;
            }

            if (unlockedAlternativePerformers >= 3)
            {
                return 100f;
            }

            if (unlockedAlternativePerformers >= 1)
            {
                return 95f;
            }

            if (alternativePerformers >= 3)
            {
                return 85f;
            }

            if (alternativePerformers >= 1)
            {
                return 75f;
            }

            /*
             * No lo hacemos cero: algunos trackers futuros pueden permitir
             * cumplir la mecánica mediante items/equipment/eventos globales.
             * Pero la familia queda claramente menos atractiva.
             */
            return 45f;
        }


        private static float ResolveTierFitScore(
            MissionTemplateFamily family,
            ChallengeTier challengeTier
        )
        {
            int tier = (int)challengeTier;
            int preferred = PreferredTier(family);
            int distance = Math.Abs(tier - preferred);

            return Clamp(
                100f - 15f * distance,
                55f,
                100f
            );
        }


        private static int PreferredTier(
            MissionTemplateFamily family
        )
        {
            switch (family)
            {
                case MissionTemplateFamily.CombatKills:
                    return 2;

                case MissionTemplateFamily.ProjectileCombat:
                case MissionTemplateFamily.HealingSupport:
                    return 2;

                case MissionTemplateFamily.BossFinisher:
                case MissionTemplateFamily.StatusEffect:
                case MissionTemplateFamily.DefenseSurvival:
                case MissionTemplateFamily.MobilityTraversal:
                case MissionTemplateFamily.MeleeCombat:
                case MissionTemplateFamily.AreaControl:
                case MissionTemplateFamily.SelfBuffMastery:
                    return 3;

                case MissionTemplateFamily.Execution:
                case MissionTemplateFamily.CriticalHit:
                case MissionTemplateFamily.Backstab:
                case MissionTemplateFamily.AirborneCombat:
                case MissionTemplateFamily.SummonMinion:
                case MissionTemplateFamily.ResourceMastery:
                    return 4;

                default:
                    return 3;
            }
        }


        private static MissionTemplateCompatibilityLevel ResolveCompatibilityLevel(
            float score
        )
        {
            if (score < 45f)
            {
                return MissionTemplateCompatibilityLevel.Incompatible;
            }

            if (score < 60f)
            {
                return MissionTemplateCompatibilityLevel.Weak;
            }

            if (score < 75f)
            {
                return MissionTemplateCompatibilityLevel.Compatible;
            }

            if (score < 88f)
            {
                return MissionTemplateCompatibilityLevel.Strong;
            }

            return MissionTemplateCompatibilityLevel.Preferred;
        }


        private static void ResolveAlternativePerformers(
            MissionProfile target,
            MissionTemplateFamily family,
            IReadOnlyList<MissionProfile> profiles,
            out int alternativePerformers,
            out int unlockedAlternativePerformers
        )
        {
            alternativePerformers = 0;
            unlockedAlternativePerformers = 0;

            if (target == null || profiles == null)
            {
                return;
            }

            for (int i = 0; i < profiles.Count; i++)
            {
                MissionProfile candidate = profiles[i];

                if (candidate == null || SameSurvivor(target, candidate))
                {
                    continue;
                }

                if (!CanPerformFamily(candidate, family))
                {
                    continue;
                }

                alternativePerformers++;

                if (candidate.IsUnlocked)
                {
                    unlockedAlternativePerformers++;
                }
            }
        }


        private static bool CanPerformFamily(
            MissionProfile profile,
            MissionTemplateFamily family
        )
        {
            if (profile == null)
            {
                return false;
            }

            switch (family)
            {
                case MissionTemplateFamily.CombatKills:
                case MissionTemplateFamily.BossFinisher:
                    return HasCapability(profile, MissionProfileCapability.Damage);

                case MissionTemplateFamily.Execution:
                    return HasCapability(profile, MissionProfileCapability.Execution);

                case MissionTemplateFamily.CriticalHit:
                    return HasCapability(profile, MissionProfileCapability.CriticalHit);

                case MissionTemplateFamily.Backstab:
                    return HasCapability(profile, MissionProfileCapability.Backstab);

                case MissionTemplateFamily.StatusEffect:
                    return HasCapability(profile, MissionProfileCapability.StatusEffect);

                case MissionTemplateFamily.HealingSupport:
                    return HasCapability(profile, MissionProfileCapability.Healing);

                case MissionTemplateFamily.DefenseSurvival:
                    return HasCapability(profile, MissionProfileCapability.Defense);

                case MissionTemplateFamily.MobilityTraversal:
                    return HasCapability(profile, MissionProfileCapability.Mobility);

                case MissionTemplateFamily.AirborneCombat:
                    return HasCapability(profile, MissionProfileCapability.Airborne) ||
                           HasCapability(profile, MissionProfileCapability.Mobility);

                case MissionTemplateFamily.SummonMinion:
                    return HasCapability(profile, MissionProfileCapability.SummonMinion);

                case MissionTemplateFamily.ResourceMastery:
                    return HasCapability(profile, MissionProfileCapability.ResourceMechanic);

                case MissionTemplateFamily.MeleeCombat:
                    return HasEffect(profile, MissionSkillEffectKind.Melee);

                case MissionTemplateFamily.ProjectileCombat:
                    return HasEffect(profile, MissionSkillEffectKind.Projectile);

                case MissionTemplateFamily.AreaControl:
                    return HasEffect(profile, MissionSkillEffectKind.AreaEffect) ||
                           HasEffect(profile, MissionSkillEffectKind.CrowdControl) ||
                           HasCapability(profile, MissionProfileCapability.StatusEffect);

                case MissionTemplateFamily.SelfBuffMastery:
                    return HasCapability(profile, MissionProfileCapability.SelfBuff);

                default:
                    return false;
            }
        }


        private static float CapabilityStrength(
            MissionProfile profile,
            MissionProfileCapability capability
        )
        {
            if (!HasCapability(profile, capability))
            {
                return 0f;
            }

            MissionEvidenceConfidence strongest =
                MissionEvidenceConfidence.Low;
            bool foundEvidence = false;

            for (int i = 0; i < profile.Evidence.Count; i++)
            {
                MissionCapabilityEvidence evidence = profile.Evidence[i];

                if (evidence == null || evidence.Capability != capability)
                {
                    continue;
                }

                foundEvidence = true;

                if ((int)evidence.Confidence > (int)strongest)
                {
                    strongest = evidence.Confidence;
                }
            }

            if (!foundEvidence)
            {
                return 0.80f;
            }

            return ConfidenceStrength(strongest);
        }


        private static float EffectStrength(
            MissionProfile profile,
            MissionSkillEffectKind effectKind
        )
        {
            if (profile == null)
            {
                return 0f;
            }

            float strongest = 0f;

            for (int i = 0; i < profile.Skills.Count; i++)
            {
                MissionSkillProfile skill = profile.Skills[i];

                if (skill == null ||
                    (!skill.ContributesToAggregate && !skill.IsDefaultVariant))
                {
                    continue;
                }

                for (int effectIndex = 0; effectIndex < skill.Effects.Count; effectIndex++)
                {
                    MissionSkillEffectProfile effect = skill.Effects[effectIndex];

                    if (effect == null || effect.Kind != effectKind)
                    {
                        continue;
                    }

                    strongest = Math.Max(
                        strongest,
                        ConfidenceStrength(effect.Confidence)
                    );
                }
            }

            return strongest;
        }


        private static bool HasCapability(
            MissionProfile profile,
            MissionProfileCapability capability
        )
        {
            if (profile == null)
            {
                return false;
            }

            for (int i = 0; i < profile.Capabilities.Count; i++)
            {
                if (profile.Capabilities[i] == capability)
                {
                    return true;
                }
            }

            return false;
        }


        private static bool HasEffect(
            MissionProfile profile,
            MissionSkillEffectKind effectKind
        )
        {
            return EffectStrength(profile, effectKind) > 0f;
        }


        private static float ConfidenceStrength(
            MissionEvidenceConfidence confidence
        )
        {
            switch (confidence)
            {
                case MissionEvidenceConfidence.High:
                    return 1f;

                case MissionEvidenceConfidence.Medium:
                    return 0.90f;

                default:
                    return 0.80f;
            }
        }


        private static bool SameSurvivor(
            MissionProfile a,
            MissionProfile b
        )
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(a.SurvivorId) &&
                !string.IsNullOrWhiteSpace(b.SurvivorId) &&
                string.Equals(
                    a.SurvivorId,
                    b.SurvivorId,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                return true;
            }

            return
                !string.IsNullOrWhiteSpace(a.BodyName) &&
                !string.IsNullOrWhiteSpace(b.BodyName) &&
                string.Equals(
                    a.BodyName,
                    b.BodyName,
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static void AddSupportingSignals(
            MissionProfile profile,
            MissionTemplateFamily family,
            MissionTemplateCompatibilityAssessment assessment
        )
        {
            if (profile == null || assessment == null)
            {
                return;
            }

            MissionProfileCapability[] capabilities =
                RelevantCapabilities(family);

            for (int i = 0; i < capabilities.Length; i++)
            {
                MissionProfileCapability capability = capabilities[i];

                if (HasCapability(profile, capability))
                {
                    AddUnique(
                        assessment.SupportingSignals,
                        capability.ToString()
                    );
                }
            }

            MissionSkillEffectKind[] effects =
                RelevantEffects(family);

            for (int i = 0; i < profile.Skills.Count; i++)
            {
                MissionSkillProfile skill = profile.Skills[i];

                if (skill == null ||
                    (!skill.ContributesToAggregate && !skill.IsDefaultVariant))
                {
                    continue;
                }

                bool skillSupports = false;

                for (int effectIndex = 0; effectIndex < skill.Effects.Count; effectIndex++)
                {
                    MissionSkillEffectProfile effect = skill.Effects[effectIndex];

                    if (effect == null)
                    {
                        continue;
                    }

                    for (int relevantIndex = 0; relevantIndex < effects.Length; relevantIndex++)
                    {
                        if (effect.Kind != effects[relevantIndex])
                        {
                            continue;
                        }

                        AddUnique(
                            assessment.SupportingSignals,
                            effect.Kind.ToString()
                        );
                        skillSupports = true;
                    }
                }

                if (skillSupports)
                {
                    AddUnique(
                        assessment.SupportingSkillIds,
                        string.IsNullOrWhiteSpace(skill.StableId)
                            ? skill.DisplayName
                            : skill.StableId
                    );
                }
            }
        }


        private static MissionProfileCapability[] RelevantCapabilities(
            MissionTemplateFamily family
        )
        {
            switch (family)
            {
                case MissionTemplateFamily.CombatKills:
                case MissionTemplateFamily.BossFinisher:
                    return new[] { MissionProfileCapability.Damage };

                case MissionTemplateFamily.Execution:
                    return new[]
                    {
                        MissionProfileCapability.Execution,
                        MissionProfileCapability.Damage
                    };

                case MissionTemplateFamily.CriticalHit:
                    return new[]
                    {
                        MissionProfileCapability.CriticalHit,
                        MissionProfileCapability.Damage
                    };

                case MissionTemplateFamily.Backstab:
                    return new[]
                    {
                        MissionProfileCapability.Backstab,
                        MissionProfileCapability.Damage
                    };

                case MissionTemplateFamily.StatusEffect:
                    return new[] { MissionProfileCapability.StatusEffect };

                case MissionTemplateFamily.HealingSupport:
                    return new[] { MissionProfileCapability.Healing };

                case MissionTemplateFamily.DefenseSurvival:
                    return new[]
                    {
                        MissionProfileCapability.Defense,
                        MissionProfileCapability.SelfBuff
                    };

                case MissionTemplateFamily.MobilityTraversal:
                    return new[]
                    {
                        MissionProfileCapability.Mobility,
                        MissionProfileCapability.Airborne
                    };

                case MissionTemplateFamily.AirborneCombat:
                    return new[]
                    {
                        MissionProfileCapability.Airborne,
                        MissionProfileCapability.Mobility,
                        MissionProfileCapability.Damage
                    };

                case MissionTemplateFamily.SummonMinion:
                    return new[] { MissionProfileCapability.SummonMinion };

                case MissionTemplateFamily.ResourceMastery:
                    return new[]
                    {
                        MissionProfileCapability.ResourceMechanic,
                        MissionProfileCapability.SelfBuff
                    };

                case MissionTemplateFamily.SelfBuffMastery:
                    return new[]
                    {
                        MissionProfileCapability.SelfBuff,
                        MissionProfileCapability.ResourceMechanic
                    };

                default:
                    return new MissionProfileCapability[0];
            }
        }


        private static MissionSkillEffectKind[] RelevantEffects(
            MissionTemplateFamily family
        )
        {
            switch (family)
            {
                case MissionTemplateFamily.MeleeCombat:
                    return new[]
                    {
                        MissionSkillEffectKind.Melee,
                        MissionSkillEffectKind.Damage
                    };

                case MissionTemplateFamily.ProjectileCombat:
                    return new[]
                    {
                        MissionSkillEffectKind.Projectile,
                        MissionSkillEffectKind.Damage
                    };

                case MissionTemplateFamily.AreaControl:
                    return new[]
                    {
                        MissionSkillEffectKind.AreaEffect,
                        MissionSkillEffectKind.CrowdControl,
                        MissionSkillEffectKind.StatusEffect
                    };

                case MissionTemplateFamily.Execution:
                    return new[]
                    {
                        MissionSkillEffectKind.Execution,
                        MissionSkillEffectKind.Damage
                    };

                case MissionTemplateFamily.CriticalHit:
                    return new[]
                    {
                        MissionSkillEffectKind.CriticalHit,
                        MissionSkillEffectKind.Damage
                    };

                case MissionTemplateFamily.Backstab:
                    return new[]
                    {
                        MissionSkillEffectKind.Backstab,
                        MissionSkillEffectKind.Damage
                    };

                case MissionTemplateFamily.StatusEffect:
                    return new[]
                    {
                        MissionSkillEffectKind.StatusEffect,
                        MissionSkillEffectKind.CrowdControl
                    };

                case MissionTemplateFamily.HealingSupport:
                    return new[] { MissionSkillEffectKind.Healing };

                case MissionTemplateFamily.DefenseSurvival:
                    return new[]
                    {
                        MissionSkillEffectKind.Defense,
                        MissionSkillEffectKind.SelfBuff
                    };

                case MissionTemplateFamily.MobilityTraversal:
                    return new[]
                    {
                        MissionSkillEffectKind.Mobility,
                        MissionSkillEffectKind.Airborne
                    };

                case MissionTemplateFamily.AirborneCombat:
                    return new[]
                    {
                        MissionSkillEffectKind.Airborne,
                        MissionSkillEffectKind.Mobility,
                        MissionSkillEffectKind.Damage
                    };

                case MissionTemplateFamily.SummonMinion:
                    return new[] { MissionSkillEffectKind.SummonMinion };

                case MissionTemplateFamily.ResourceMastery:
                    return new[]
                    {
                        MissionSkillEffectKind.Resource,
                        MissionSkillEffectKind.SelfBuff
                    };

                case MissionTemplateFamily.SelfBuffMastery:
                    return new[]
                    {
                        MissionSkillEffectKind.SelfBuff,
                        MissionSkillEffectKind.Resource
                    };

                case MissionTemplateFamily.CombatKills:
                case MissionTemplateFamily.BossFinisher:
                    return new[] { MissionSkillEffectKind.Damage };

                default:
                    return new MissionSkillEffectKind[0];
            }
        }


        private static void AddUnique(
            List<string> values,
            string value
        )
        {
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (string.Equals(
                    values[i],
                    value,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    return;
                }
            }

            values.Add(value);
        }


        private static float Max(
            float a,
            float b,
            float c
        )
        {
            return Math.Max(a, Math.Max(b, c));
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
