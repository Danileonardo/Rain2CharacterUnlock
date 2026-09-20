using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G4 Phase 1.4 - Community Evidence Calibration.
    ///
    /// Mantiene toda la calibración local de 1.3.2 y activa la señal
    /// comunitaria secundaria/offline mediante un catálogo curado. Cada hilo
    /// o tierlist cuenta una sola vez como grupo independiente, la comunidad
    /// sólo corrige moderadamente LocalPower y Unknown sigue siendo obligatorio
    /// cuando no existen dos grupos comparables.
    ///
    /// IMPORTANTE:
    /// - CommunityPowerTier sólo se resuelve desde fuentes curadas offline;
    /// - no asigna ChallengeTier;
    /// - no genera misiones;
    /// - no persiste resultados.
    /// </summary>
    public static class LocalPowerAssessmentAnalyzer
    {
        public const int AssessmentVersion = 7;

        private const float MinimumSkillReference = 0.75f;
        private const float MaximumRelativeIndex = 200f;
        private const float SoftCurveScale = 1.35f;
        private const float MinimumIndirectReference = 0.30f;
        private const float MinimumQuantitativeReference = 0.40f;

        public static List<LocalPowerAssessment> AnalyzeAll(
            IReadOnlyList<MissionProfile> profiles
        )
        {
            List<LocalPowerAssessment> result =
                new List<LocalPowerAssessment>();

            if (profiles == null || profiles.Count == 0)
            {
                return result;
            }

            List<RawPowerVector> rawVectors =
                new List<RawPowerVector>();

            for (int i = 0; i < profiles.Count; i++)
            {
                MissionProfile profile = profiles[i];

                if (profile == null)
                {
                    continue;
                }

                rawVectors.Add(
                    BuildRawVector(profile)
                );
            }

            VanillaReference reference =
                BuildVanillaReference(rawVectors);

            List<float> vanillaCompositeValues =
                new List<float>();

            for (int i = 0; i < rawVectors.Count; i++)
            {
                RawPowerVector raw = rawVectors[i];

                LocalPowerAssessment assessment =
                    BuildAssessment(raw, reference);

                result.Add(assessment);

                if (!raw.Profile.IsModded)
                {
                    vanillaCompositeValues.Add(
                        assessment.CompositeIndex
                    );
                }
            }

            float vanillaCompositeMedian =
                MedianPositive(
                    vanillaCompositeValues,
                    100f
                );

            for (int i = 0; i < result.Count; i++)
            {
                LocalPowerAssessment assessment = result[i];

                assessment.LocalPowerScore =
                    SoftRelativeIndex(
                        SafeRatio(
                            assessment.CompositeIndex,
                            vanillaCompositeMedian,
                            1f
                        )
                    );

                ApplyCommunityCalibration(assessment);
            }

            return result;
        }


        private static RawPowerVector BuildRawVector(
            MissionProfile profile
        )
        {
            RawPowerVector result =
                new RawPowerVector
                {
                    Profile = profile
                };

            MissionBodyProfile body =
                profile.Body ?? new MissionBodyProfile();

            result.BaseHealth = body.BaseMaxHealth;
            result.BaseRegen = body.BaseRegen;
            result.BaseDamage = body.BaseDamage;
            result.BaseMoveSpeed = body.BaseMoveSpeed;
            result.BaseAcceleration = body.BaseAcceleration;
            result.BaseJumpPower = body.BaseJumpPower;
            result.BaseArmor = body.BaseArmor;
            result.BaseAttackSpeed = body.BaseAttackSpeed;
            result.BaseCrit = body.BaseCrit;
            result.BaseJumpCount = body.BaseJumpCount;

            SlotPowerVector primary = new SlotPowerVector();
            SlotPowerVector secondary = new SlotPowerVector();
            SlotPowerVector utility = new SlotPowerVector();
            SlotPowerVector special = new SlotPowerVector();
            SlotPowerVector passive = new SlotPowerVector();
            SlotPowerVector extras = new SlotPowerVector();

            for (int i = 0; i < profile.Skills.Count; i++)
            {
                MissionSkillProfile skill = profile.Skills[i];

                if (skill == null)
                {
                    continue;
                }

                SkillMagnitudeProfile magnitude =
                    SkillMagnitudeAnalyzer.Analyze(skill);

                result.AllSkillMagnitudes.Add(magnitude);

                bool defaultLoadoutOption =
                    skill.UsageKind == MissionSkillUsageKind.LoadoutOption &&
                    skill.IsDefaultVariant;

                /*
                 * G3 excluye todas las LoadoutOption para evitar híbridos
                 * imposibles. G4 puede incluir de forma segura únicamente la
                 * variante DEFAULT de cada grupo Extra: representa el loadout
                 * coherente que el survivor trae instalado, sin sumar todas
                 * las alternativas como si coexistieran.
                 */
                if (!skill.ContributesToAggregate && !defaultLoadoutOption)
                {
                    continue;
                }

                if (defaultLoadoutOption)
                {
                    result.DefaultLoadoutOptionCount++;
                }

                result.AggregateSkillCount++;
                result.QuantitativeSkillCount++;

                if (magnitude != null && magnitude.HasQuantitativeSignal)
                {
                    result.QuantitativeScoredSkillCount++;
                }

                SlotPowerVector skillVector =
                    ScoreSkill(skill, magnitude);

                if (skillVector.HasSemanticSignal)
                {
                    result.ScoredSkillCount++;
                }

                switch (skill.Slot)
                {
                    case SurvivorContentSkillSlot.Primary:
                        primary.SelectRepresentative(skillVector);
                        break;

                    case SurvivorContentSkillSlot.Secondary:
                        secondary.SelectRepresentative(skillVector);
                        break;

                    case SurvivorContentSkillSlot.Utility:
                        utility.SelectRepresentative(skillVector);
                        break;

                    case SurvivorContentSkillSlot.Special:
                        special.SelectRepresentative(skillVector);
                        break;

                    case SurvivorContentSkillSlot.Passive:
                        passive.SelectRepresentative(skillVector);
                        break;

                    default:
                        /*
                         * Extras que G3 marcó AuxiliaryActive/Passive sí
                         * pueden coexistir con el kit normal. Se suman con un
                         * techo por dimensión para impedir que un mod con
                         * muchos slots auxiliares infle el resultado sin
                         * límite. Las opciones LoadoutOption ya fueron
                         * excluidas arriba.
                         */
                        extras.AddCapped(skillVector, 2.5f);
                        break;
                }
            }

            result.Offense =
                primary.Offense +
                secondary.Offense +
                utility.Offense +
                special.Offense +
                passive.Offense +
                extras.Offense;

            result.Sustain =
                primary.Sustain +
                secondary.Sustain +
                utility.Sustain +
                special.Sustain +
                passive.Sustain +
                extras.Sustain;

            result.Defense =
                primary.Defense +
                secondary.Defense +
                utility.Defense +
                special.Defense +
                passive.Defense +
                extras.Defense;

            result.Mobility =
                primary.Mobility +
                secondary.Mobility +
                utility.Mobility +
                special.Mobility +
                passive.Mobility +
                extras.Mobility;

            result.Control =
                primary.Control +
                secondary.Control +
                utility.Control +
                special.Control +
                passive.Control +
                extras.Control;

            result.Utility =
                primary.Utility +
                secondary.Utility +
                utility.Utility +
                special.Utility +
                passive.Utility +
                extras.Utility;

            result.Quantitative =
                primary.Quantitative +
                secondary.Quantitative +
                utility.Quantitative +
                special.Quantitative +
                passive.Quantitative +
                extras.Quantitative;

            result.Indirect =
                primary.Indirect +
                secondary.Indirect +
                utility.Indirect +
                special.Indirect +
                passive.Indirect +
                extras.Indirect;

            CollectIndirectSignals(result.IndirectSignals, primary);
            CollectIndirectSignals(result.IndirectSignals, secondary);
            CollectIndirectSignals(result.IndirectSignals, utility);
            CollectIndirectSignals(result.IndirectSignals, special);
            CollectIndirectSignals(result.IndirectSignals, passive);
            CollectIndirectSignals(result.IndirectSignals, extras);

            CountEvidenceConfidence(
                profile,
                result
            );

            return result;
        }


        private static SlotPowerVector ScoreSkill(
            MissionSkillProfile skill,
            SkillMagnitudeProfile magnitude
        )
        {
            SlotPowerVector result =
                new SlotPowerVector();

            if (skill == null)
            {
                return result;
            }

            float availability =
                GetAvailabilityMultiplier(skill);

            for (
                int effectIndex = 0;
                effectIndex < skill.Effects.Count;
                effectIndex++
            )
            {
                MissionSkillEffectProfile effect =
                    skill.Effects[effectIndex];

                if (effect == null)
                {
                    continue;
                }

                if (
                    HasEarlierEffectKind(
                        skill.Effects,
                        effectIndex,
                        effect.Kind
                    )
                )
                {
                    continue;
                }

                MissionEvidenceConfidence strongest =
                    GetStrongestConfidence(
                        skill.Effects,
                        effect.Kind
                    );

                float confidenceWeight =
                    ConfidenceWeight(strongest);

                float points =
                    availability * confidenceWeight;

                switch (effect.Kind)
                {
                    case MissionSkillEffectKind.Damage:
                        result.Offense += 1.00f * points;
                        break;

                    case MissionSkillEffectKind.AreaEffect:
                        result.Offense += 0.35f * points;
                        break;

                    case MissionSkillEffectKind.CriticalHit:
                        result.Offense += 0.45f * points;
                        break;

                    case MissionSkillEffectKind.Backstab:
                        result.Offense += 0.45f * points;
                        break;

                    case MissionSkillEffectKind.Execution:
                        result.Offense += 0.60f * points;
                        break;

                    case MissionSkillEffectKind.Healing:
                        result.Sustain += 1.00f * points;
                        break;

                    case MissionSkillEffectKind.Defense:
                        result.Defense += 1.00f * points;
                        break;

                    case MissionSkillEffectKind.Mobility:
                        result.Mobility += 1.00f * points;
                        break;

                    case MissionSkillEffectKind.Airborne:
                        result.Mobility += 0.35f * points;
                        break;

                    case MissionSkillEffectKind.CrowdControl:
                        result.Control += 1.00f * points;
                        break;

                    case MissionSkillEffectKind.StatusEffect:
                        result.Control += 0.55f * points;
                        break;

                    case MissionSkillEffectKind.SummonMinion:
                        result.Utility += 1.00f * points;
                        break;

                    case MissionSkillEffectKind.Resource:
                        result.Utility += 0.55f * points;
                        break;

                    case MissionSkillEffectKind.SelfBuff:
                        result.Utility += 0.55f * points;
                        break;

                    case MissionSkillEffectKind.Utility:
                        result.Utility += 0.35f * points;
                        break;
                }
            }

            ApplyIndirectMechanics(
                skill,
                result,
                availability
            );

            if (magnitude != null)
            {
                result.Quantitative =
                    Math.Max(0f, magnitude.RawPowerContribution);
            }

            result.HasSemanticSignal =
                result.Offense > 0f ||
                result.Sustain > 0f ||
                result.Defense > 0f ||
                result.Mobility > 0f ||
                result.Control > 0f ||
                result.Utility > 0f ||
                result.Indirect > 0f;

            return result;
        }


        private static void ApplyIndirectMechanics(
            MissionSkillProfile skill,
            SlotPowerVector result,
            float availability
        )
        {
            if (skill == null || result == null)
            {
                return;
            }

            string text = NormalizeSemanticText(
                (skill.DisplayName ?? "") + " " +
                (skill.Description ?? "") + " " +
                (skill.InternalName ?? "")
            );

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            bool maxHealthDamage =
                ContainsAny(
                    text,
                    "maximum health",
                    "max health",
                    "salud maxima",
                    "vida maxima"
                ) &&
                ContainsAny(text, "damage", "dano");

            if (maxHealthDamage)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.MaxHealthDamage,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.60f,
                    0.45f, 0f, 0f, 0f, 0f, 0f,
                    "damage scales with target maximum health"
                );
            }

            bool executeThreshold =
                ContainsAny(
                    text,
                    "execute",
                    "executes",
                    "execution",
                    "ejecuta",
                    "ejecutar",
                    "remata",
                    "rematar"
                ) &&
                ContainsAny(
                    text,
                    "below",
                    "under",
                    "por debajo",
                    "menos de"
                );

            if (executeThreshold)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.ExecuteThreshold,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.30f,
                    0.20f, 0f, 0f, 0f, 0f, 0f,
                    "execution threshold"
                );
            }

            bool damageAmplification =
                !maxHealthDamage &&
                ContainsAny(
                    text,
                    "bonus damage",
                    "additional damage",
                    "extra damage",
                    "increased damage",
                    "increase damage",
                    "dano adicional",
                    "dano extra",
                    "aumenta el dano",
                    "aumenta dano",
                    "amplifica el dano",
                    "double damage",
                    "doubles damage",
                    "twice the damage",
                    "doble dano",
                    "el doble de dano",
                    "duplica el dano"
                );

            bool teamDamageAmplification =
                damageAmplification &&
                ContainsAny(
                    text,
                    "allies", "ally", "teammates", "team",
                    "aliados", "aliado", "equipo"
                );

            /*
             * Un buff de equipo ya incluye la amplificación personal. No se
             * registran ambas señales para evitar contar dos veces la misma
             * frase (p.ej. "duplica el daño propio y de los aliados").
             */
            if (teamDamageAmplification)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.TeamDamageAmplification,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.52f,
                    0.22f, 0f, 0f, 0f, 0f, 0.34f,
                    "team damage amplification"
                );
            }
            else if (damageAmplification)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.DamageAmplification,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.35f,
                    0.28f, 0f, 0f, 0f, 0f, 0f,
                    "damage amplification"
                );
            }

            bool attackSpeedBoost =
                ContainsAny(
                    text,
                    "attack speed",
                    "velocidad de ataque"
                ) &&
                ContainsAny(
                    text,
                    "gain",
                    "gains",
                    "increase",
                    "increases",
                    "increased",
                    "bonus",
                    "obtiene",
                    "gana",
                    "aumenta",
                    "otorga",
                    "+"
                ) &&
                !ContainsAny(
                    text,
                    "reduces enemy attack speed",
                    "reduce la velocidad de ataque del enemigo",
                    "reduce su velocidad de ataque"
                );

            if (attackSpeedBoost)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.AttackSpeedBoost,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.38f,
                    0.24f, 0f, 0f, 0f, 0f, 0.10f,
                    "positive attack-speed modifier"
                );
            }

            bool cooldownAcceleration =
                (ContainsAny(text, "cooldown", "enfriamiento", "tiempo de recarga", "tiempos de recarga") &&
                 ContainsAny(text, "refund", "reduce", "reduction", "reset", "reembol", "reduccion", "reinicia", "reiniciar"));

            if (cooldownAcceleration)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.CooldownAcceleration,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.48f,
                    0.10f, 0f, 0f, 0f, 0f, 0.38f,
                    "cooldown acceleration/refund"
                );
            }

            bool recast =
                ContainsAny(
                    text,
                    "recast",
                    "cast again",
                    "use again",
                    "twice more",
                    "volver a activar",
                    "vuelve a activar",
                    "puede volver a usar",
                    "puede reutilizar"
                );

            if (recast)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.RecastOrRepeat,
                    MissionEvidenceConfidence.Low,
                    availability,
                    0.28f,
                    0.12f, 0f, 0f, 0f, 0f, 0.20f,
                    "recast/repeat access"
                );
            }

            bool invulnerability =
                ContainsAny(
                    text,
                    "invulnerab",
                    "immune to damage",
                    "inmune al dano",
                    "block all damage",
                    "bloquea todo el dano",
                    "bloquear todo el dano",
                    "negating the damage",
                    "anulando el dano",
                    "anula el dano"
                );

            if (invulnerability)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.InvulnerabilityOrNegation,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.55f,
                    0f, 0f, 0.48f, 0f, 0f, 0.08f,
                    "strong damage negation/invulnerability"
                );
            }

            bool mitigation =
                !invulnerability &&
                (
                    ContainsAny(
                        text,
                        "damage reduction",
                        "reduces damage taken",
                        "reduce el dano recibido",
                        "probabilidad de bloqueo",
                        "block chance"
                    ) ||
                    (ContainsAny(text, "armor", "armadura") &&
                     ContainsAny(text, "gain", "gains", "increase", "obtiene", "gana", "aumenta", "otorga", "+"))
                );

            if (mitigation)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.StrongMitigation,
                    MissionEvidenceConfidence.Low,
                    availability,
                    0.32f,
                    0f, 0f, 0.28f, 0f, 0f, 0.05f,
                    "strong mitigation/armor modifier"
                );
            }

            bool teamDefenseBuff =
                ContainsAny(text, "armor", "armadura", "damage reduction", "reduccion de dano") &&
                ContainsAny(text, "allies", "ally", "team", "aliados", "aliado", "equipo") &&
                ContainsAny(text, "gain", "gains", "grant", "grants", "obtiene", "gana", "otorga", "concede", "+");

            if (teamDefenseBuff)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.TeamDefenseBuff,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.42f,
                    0f, 0.05f, 0.30f, 0f, 0f, 0.30f,
                    "team defensive stat buff"
                );
            }

            bool armorShredOrBypass =
                ContainsAny(text, "armor", "armadura") &&
                ContainsAny(
                    text,
                    "reduce", "reduces", "reduction", "lower",
                    "ignore", "ignores", "ignoring",
                    "reduce", "reduccion", "ignora", "ignorar"
                ) &&
                !teamDefenseBuff;

            if (armorShredOrBypass)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.ArmorShredOrBypass,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.38f,
                    0.22f, 0f, 0f, 0f, 0.18f, 0.05f,
                    "armor reduction or bypass"
                );
            }

            bool arsenal =
                ContainsAny(
                    text,
                    "arsenal",
                    "weapon box",
                    "weapon boxes",
                    "caja de armas",
                    "cajas de armas",
                    "swap weapon",
                    "switch weapon",
                    "cambia a otra arma",
                    "ultima arma equipada",
                    "ammo",
                    "municion"
                );

            if (arsenal)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.ArsenalOrResourceAccess,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.42f,
                    0.05f, 0f, 0f, 0f, 0f, 0.38f,
                    "arsenal/resource access"
                );
            }

            bool weakPoint =
                ContainsAny(
                    text,
                    "weak point",
                    "weakpoint",
                    "weak points",
                    "punto debil",
                    "puntos debiles"
                ) &&
                ContainsAny(
                    text,
                    "expose",
                    "exposes",
                    "expone",
                    "expon",
                    "exponer",
                    "reveal",
                    "marca",
                    "mark"
                );

            if (weakPoint)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.WeakPointExposure,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.34f,
                    0.26f, 0f, 0f, 0f, 0.10f, 0.08f,
                    "weak-point exposure"
                );
            }

            bool reactiveCounter =
                ContainsAny(
                    text,
                    "counterattack",
                    "counter attack",
                    "contraataque",
                    "perfect dodge",
                    "esquiva perfecta",
                    "perfect parry",
                    "parada perfecta"
                );

            if (reactiveCounter)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.ReactiveCounter,
                    MissionEvidenceConfidence.Low,
                    availability,
                    0.28f,
                    0.12f, 0f, 0.16f, 0f, 0f, 0.05f,
                    "reactive counter/perfect timing payoff"
                );
            }

            bool stacking =
                (ContainsAny(text, "stack", "stacks", "acumulacion", "acumulaciones", "acumula", "acumular") &&
                 ContainsAny(text, "up to", "hasta", "cada", "every"));

            if (stacking)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.StackingOrScaling,
                    MissionEvidenceConfidence.Low,
                    availability,
                    0.22f,
                    0.08f, 0f, 0f, 0f, 0f, 0.16f,
                    "stacking/scaling mechanic"
                );
            }

            bool teamReposition =
                ContainsAny(text, "teleport", "teletransport") &&
                ContainsAny(text, "allies", "ally", "aliados", "aliado");

            if (teamReposition)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.TeamReposition,
                    MissionEvidenceConfidence.Low,
                    availability,
                    0.26f,
                    0f, 0f, 0f, 0.08f, 0f, 0.22f,
                    "team repositioning"
                );
            }

            bool persistentFlight =
                ContainsAny(
                    text,
                    "fly at will", "can fly", "permanent flight",
                    "volar a voluntad", "puede volar", "vuelo permanente"
                );

            if (persistentFlight)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.PersistentFlight,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.42f,
                    0f, 0f, 0f, 0.52f, 0f, 0.08f,
                    "persistent/free flight access"
                );
            }

            bool forcedMovementControl =
                ContainsAny(
                    text,
                    "pull enemies", "pulls enemies", "pulling",
                    "drag enemies", "drags enemies", "dragging",
                    "attract enemies", "attracts enemies",
                    "atrae", "atrayendo", "arrastrando", "arrastra"
                );

            if (forcedMovementControl)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.ForcedMovementControl,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.34f,
                    0f, 0f, 0f, 0f, 0.46f, 0.08f,
                    "forced enemy movement/control"
                );
            }

            bool allyCooldownReset =
                ContainsAny(text, "cooldown", "cooldowns", "tiempo de recarga", "tiempos de recarga") &&
                ContainsAny(text, "reset", "resets", "restablece", "reinicia", "reiniciar") &&
                ContainsAny(text, "ally", "allies", "aliado", "aliados", "team", "equipo");

            if (allyCooldownReset)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.AllyCooldownReset,
                    MissionEvidenceConfidence.Medium,
                    availability,
                    0.46f,
                    0.05f, 0f, 0f, 0f, 0f, 0.48f,
                    "ally/team cooldown reset"
                );
            }

            bool explicitResurrection =
                ContainsAny(
                    text,
                    "resurrect", "revive", "revives",
                    "resucita", "resucitar", "revivir", "vuelve a la vida"
                );

            bool resurrectionLabel =
                !explicitResurrection &&
                ContainsAny(text, "resurrection", "resurreccion");

            if (explicitResurrection || resurrectionLabel)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.ResurrectionAccess,
                    explicitResurrection
                        ? MissionEvidenceConfidence.Medium
                        : MissionEvidenceConfidence.Low,
                    availability,
                    explicitResurrection ? 0.58f : 0.30f,
                    0f, explicitResurrection ? 0.40f : 0.18f, 0f, 0f, 0f,
                    explicitResurrection ? 0.42f : 0.22f,
                    explicitResurrection
                        ? "explicit resurrection/revival access"
                        : "resurrection-labelled subskill access"
                );
            }

            bool multiHitOrChannelScaling =
                ContainsAny(
                    text,
                    "per hit", "each hit", "multiple hits", "multi-hit",
                    "por impacto", "cada impacto", "multiples impactos",
                    "varios impactos", "mas impactos", "más impactos"
                );

            if (multiHitOrChannelScaling)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.MultiHitOrChannelScaling,
                    MissionEvidenceConfidence.Low,
                    availability,
                    0.22f,
                    0.16f, 0f, 0f, 0f, 0f, 0.04f,
                    "multi-hit/channel scaling"
                );
            }

            /*
             * Algunos mods exponen una SkillDef contenedora que abre varios
             * hechizos/modos/subskills internos. Si G3 sólo ve el contenedor,
             * no inventamos el contenido oculto, pero tampoco lo tratamos como
             * una skill vacía. Esta señal es deliberadamente Low confidence.
             */
            bool customSkillDef =
                !string.IsNullOrWhiteSpace(skill.SkillDefType) &&
                !string.Equals(skill.SkillDefType, "SkillDef", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(skill.SkillDefType, "None", StringComparison.OrdinalIgnoreCase);

            bool explicitSubskillAccess =
                HasEffect(skill, MissionSkillEffectKind.Resource) &&
                ContainsAny(
                    text,
                    "energy skills", "resource skills", "additional skills",
                    "habilidades de energia", "habilidades de recurso",
                    "habilidades adicionales",
                    "enable the skills", "enables the skills",
                    "habilitar las habilidades", "habilita las habilidades"
                );

            bool modeOrSubskillAccess =
                explicitSubskillAccess ||
                (
                    skill.UsageKind != MissionSkillUsageKind.Passive &&
                    customSkillDef &&
                    HasEffect(skill, MissionSkillEffectKind.Resource) &&
                    ContainsAny(
                        text,
                        "different skill", "different ability",
                        "depending on input", "based on input",
                        "switch between", "cycle through", "select a", "choose a",
                        "spell", "spells", "magic", "elements",
                        "habilidad segun", "habilidad según", "segun la entrada",
                        "según la entrada", "cambia entre", "alternar entre",
                        "hechizo", "hechizos", "magia", "elementos",
                        "modo", "forma", "postura"
                    )
                );

            if (modeOrSubskillAccess)
            {
                AddIndirectSignal(
                    result,
                    skill,
                    IndirectPowerSignalKind.ModeOrSubskillAccess,
                    MissionEvidenceConfidence.Low,
                    availability,
                    0.44f,
                    0.10f, 0f, 0f, 0f, 0.05f, 0.34f,
                    "custom resource-driven mode/subskill container"
                );
            }
        }


        private static void AddIndirectSignal(
            SlotPowerVector result,
            MissionSkillProfile skill,
            IndirectPowerSignalKind kind,
            MissionEvidenceConfidence confidence,
            float availability,
            float indirectWeight,
            float offense,
            float sustain,
            float defense,
            float mobility,
            float control,
            float utility,
            string detail
        )
        {
            if (result == null || skill == null)
            {
                return;
            }

            for (int i = 0; i < result.IndirectSignals.Count; i++)
            {
                IndirectPowerSignal existing = result.IndirectSignals[i];

                if (existing != null && existing.Kind == kind)
                {
                    return;
                }
            }

            float confidenceWeight = ConfidenceWeight(confidence);
            float scale = availability * confidenceWeight;
            float rawContribution = indirectWeight * scale;

            result.Indirect += rawContribution;
            result.Offense += offense * scale;
            result.Sustain += sustain * scale;
            result.Defense += defense * scale;
            result.Mobility += mobility * scale;
            result.Control += control * scale;
            result.Utility += utility * scale;

            result.IndirectSignals.Add(
                new IndirectPowerSignal
                {
                    Kind = kind,
                    SkillStableId = skill.StableId ?? "",
                    Detail = detail ?? "",
                    Confidence = confidence,
                    RawContribution = rawContribution
                }
            );
        }


        private static void CollectIndirectSignals(
            List<IndirectPowerSignal> destination,
            SlotPowerVector source
        )
        {
            if (destination == null || source == null)
            {
                return;
            }

            for (int i = 0; i < source.IndirectSignals.Count; i++)
            {
                IndirectPowerSignal signal = source.IndirectSignals[i];

                if (signal != null)
                {
                    destination.Add(signal);
                }
            }
        }


        private static string NormalizeSemanticText(
            string value
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string decomposed =
                value.ToLowerInvariant().Normalize(
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


        private static bool HasEarlierEffectKind(
            IReadOnlyList<MissionSkillEffectProfile> effects,
            int currentIndex,
            MissionSkillEffectKind kind
        )
        {
            for (int i = 0; i < currentIndex; i++)
            {
                MissionSkillEffectProfile previous = effects[i];

                if (
                    previous != null &&
                    previous.Kind == kind
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static MissionEvidenceConfidence GetStrongestConfidence(
            IReadOnlyList<MissionSkillEffectProfile> effects,
            MissionSkillEffectKind kind
        )
        {
            MissionEvidenceConfidence strongest =
                MissionEvidenceConfidence.Low;

            for (int i = 0; i < effects.Count; i++)
            {
                MissionSkillEffectProfile candidate = effects[i];

                if (
                    candidate != null &&
                    candidate.Kind == kind &&
                    (int)candidate.Confidence > (int)strongest
                )
                {
                    strongest = candidate.Confidence;
                }
            }

            return strongest;
        }


        private static float GetAvailabilityMultiplier(
            MissionSkillProfile skill
        )
        {
            if (skill == null)
            {
                return 1f;
            }

            float usage = 1f;

            switch (skill.UsageKind)
            {
                case MissionSkillUsageKind.Passive:
                    usage = 0.90f;
                    break;

                case MissionSkillUsageKind.AuxiliaryActive:
                    usage = 0.85f;
                    break;
            }

            float cooldown = 1f;

            if (skill.UsageKind != MissionSkillUsageKind.Passive)
            {
                if (skill.RechargeInterval <= 0f)
                {
                    cooldown = 1.10f;
                }
                else if (skill.RechargeInterval <= 2f)
                {
                    cooldown = 1.05f;
                }
                else if (skill.RechargeInterval <= 6f)
                {
                    cooldown = 1.00f;
                }
                else if (skill.RechargeInterval <= 12f)
                {
                    cooldown = 0.90f;
                }
                else
                {
                    cooldown = 0.80f;
                }
            }

            int extraStocks =
                Math.Max(
                    0,
                    skill.MaxStock - 1
                );

            float stock =
                1f + Math.Min(
                    0.20f,
                    extraStocks * 0.05f
                );

            return usage * cooldown * stock;
        }


        private static VanillaReference BuildVanillaReference(
            IReadOnlyList<RawPowerVector> vectors
        )
        {
            VanillaReference result =
                new VanillaReference();

            List<float> health = new List<float>();
            List<float> regen = new List<float>();
            List<float> damage = new List<float>();
            List<float> move = new List<float>();
            List<float> acceleration = new List<float>();
            List<float> jumpPower = new List<float>();
            List<float> armor = new List<float>();
            List<float> attackSpeed = new List<float>();
            List<float> crit = new List<float>();
            List<float> jumpCount = new List<float>();

            List<float> offense = new List<float>();
            List<float> sustain = new List<float>();
            List<float> defense = new List<float>();
            List<float> mobility = new List<float>();
            List<float> control = new List<float>();
            List<float> utility = new List<float>();
            List<float> indirect = new List<float>();
            List<float> quantitative = new List<float>();

            for (int i = 0; i < vectors.Count; i++)
            {
                RawPowerVector vector = vectors[i];

                if (
                    vector == null ||
                    vector.Profile == null ||
                    vector.Profile.IsModded
                )
                {
                    continue;
                }

                result.ProfileCount++;

                AddPositive(health, vector.BaseHealth);
                AddPositive(regen, vector.BaseRegen);
                AddPositive(damage, vector.BaseDamage);
                AddPositive(move, vector.BaseMoveSpeed);
                AddPositive(acceleration, vector.BaseAcceleration);
                AddPositive(jumpPower, vector.BaseJumpPower);
                AddFinite(armor, vector.BaseArmor);
                AddPositive(attackSpeed, vector.BaseAttackSpeed);
                AddFinite(crit, vector.BaseCrit);
                AddPositive(jumpCount, vector.BaseJumpCount);

                AddPositive(offense, vector.Offense);
                AddPositive(sustain, vector.Sustain);
                AddPositive(defense, vector.Defense);
                AddPositive(mobility, vector.Mobility);
                AddPositive(control, vector.Control);
                AddPositive(utility, vector.Utility);
                AddPositive(indirect, vector.Indirect);
                AddPositive(quantitative, vector.Quantitative);
            }

            result.BaseHealth = MedianPositive(health, 110f);
            result.BaseRegen = MedianPositive(regen, 1f);
            result.BaseDamage = MedianPositive(damage, 12f);
            result.BaseMoveSpeed = MedianPositive(move, 7f);
            result.BaseAcceleration = MedianPositive(acceleration, 80f);
            result.BaseJumpPower = MedianPositive(jumpPower, 15f);
            result.BaseArmor = MedianFinite(armor, 0f);
            result.BaseAttackSpeed = MedianPositive(attackSpeed, 1f);
            result.BaseCrit = MedianFinite(crit, 1f);
            result.BaseJumpCount = MedianPositive(jumpCount, 1f);

            result.Offense =
                Math.Max(
                    MinimumSkillReference,
                    MedianPositive(offense, MinimumSkillReference)
                );

            result.Sustain =
                Math.Max(
                    MinimumSkillReference,
                    MedianPositive(sustain, MinimumSkillReference)
                );

            result.Defense =
                Math.Max(
                    MinimumSkillReference,
                    MedianPositive(defense, MinimumSkillReference)
                );

            result.Mobility =
                Math.Max(
                    MinimumSkillReference,
                    MedianPositive(mobility, MinimumSkillReference)
                );

            result.Control =
                Math.Max(
                    MinimumSkillReference,
                    MedianPositive(control, MinimumSkillReference)
                );

            result.Utility =
                Math.Max(
                    MinimumSkillReference,
                    MedianPositive(utility, MinimumSkillReference)
                );

            result.Indirect =
                Math.Max(
                    MinimumIndirectReference,
                    MedianPositive(indirect, MinimumIndirectReference)
                );

            result.Quantitative =
                Math.Max(
                    MinimumQuantitativeReference,
                    MedianPositive(
                        quantitative,
                        MinimumQuantitativeReference
                    )
                );

            result.IndirectReferenceCount = indirect.Count;
            result.QuantitativeReferenceCount = quantitative.Count;

            return result;
        }


        private static LocalPowerAssessment BuildAssessment(
            RawPowerVector raw,
            VanillaReference reference
        )
        {
            LocalPowerAssessment result =
                new LocalPowerAssessment
                {
                    SurvivorId = raw.Profile.SurvivorId ?? "",
                    BodyName = raw.Profile.BodyName ?? "",
                    DisplayName = raw.Profile.DisplayName ?? "",
                    IsModded = raw.Profile.IsModded,
                    AggregateSkillCount = raw.AggregateSkillCount,
                    DefaultLoadoutOptionCount = raw.DefaultLoadoutOptionCount,
                    ScoredSkillCount = raw.ScoredSkillCount,
                    HighConfidenceEvidence = raw.HighEvidence,
                    MediumConfidenceEvidence = raw.MediumEvidence,
                    LowConfidenceEvidence = raw.LowEvidence
                };

            float bodyOffense =
                WeightedAverage(
                    SafeStatRatio(raw.BaseDamage, reference.BaseDamage), 0.60f,
                    SafeStatRatio(raw.BaseAttackSpeed, reference.BaseAttackSpeed), 0.30f,
                    CritRelative(raw.BaseCrit, reference.BaseCrit), 0.10f
                ) * 100f;

            float bodySurvival =
                WeightedAverage(
                    SafeStatRatio(raw.BaseHealth, reference.BaseHealth), 0.55f,
                    SafeStatRatio(raw.BaseRegen, reference.BaseRegen), 0.20f,
                    ArmorRelative(raw.BaseArmor, reference.BaseArmor), 0.25f
                ) * 100f;

            float bodyMobility =
                WeightedAverage4(
                    SafeStatRatio(raw.BaseMoveSpeed, reference.BaseMoveSpeed), 0.50f,
                    SafeStatRatio(raw.BaseAcceleration, reference.BaseAcceleration), 0.15f,
                    SafeStatRatio(raw.BaseJumpPower, reference.BaseJumpPower), 0.20f,
                    SafeStatRatio(raw.BaseJumpCount, reference.BaseJumpCount), 0.15f
                ) * 100f;

            float bodyComposite =
                bodyOffense * 0.40f +
                bodySurvival * 0.40f +
                bodyMobility * 0.20f;

            result.BodyIndex =
                Math.Max(
                    25f,
                    SoftRelativeIndex(bodyComposite / 100f)
                );

            result.OffenseIndex =
                RelativeSkillIndex(raw.Offense, reference.Offense);

            result.SustainIndex =
                RelativeSkillIndex(raw.Sustain, reference.Sustain);

            result.DefenseIndex =
                RelativeSkillIndex(raw.Defense, reference.Defense);

            result.MobilityIndex =
                RelativeSkillIndex(raw.Mobility, reference.Mobility);

            result.ControlIndex =
                RelativeSkillIndex(raw.Control, reference.Control);

            result.UtilityIndex =
                RelativeSkillIndex(raw.Utility, reference.Utility);

            result.IndirectPowerIndex =
                RelativeIndirectIndex(raw.Indirect, reference.Indirect);

            result.QuantitativeSkillCount = raw.QuantitativeSkillCount;
            result.QuantitativeScoredSkillCount =
                raw.QuantitativeScoredSkillCount;

            float quantitativeCoverageRatio =
                raw.QuantitativeSkillCount > 0
                    ? Clamp(
                        (float)raw.QuantitativeScoredSkillCount /
                        raw.QuantitativeSkillCount,
                        0f,
                        1f
                    )
                    : 0f;

            result.QuantitativeCoverage =
                quantitativeCoverageRatio * 100f;

            result.QuantitativeRawIndex =
                RelativeQuantitativeIndex(
                    raw.Quantitative,
                    reference.Quantitative
                );

            /*
             * G4 1.3.1: dato desconocido != personaje débil.
             * Por debajo de 35% de cobertura, Quantitative es neutral (100).
             * Desde ahí su influencia crece de forma continua hasta utilizar el
             * índice completo al llegar a 100% de cobertura.
             */
            float quantitativeReliability =
                Clamp(
                    (quantitativeCoverageRatio - 0.35f) / 0.65f,
                    0f,
                    1f
                );

            result.QuantitativeReliability =
                quantitativeReliability * 100f;

            result.QuantitativePowerIndex =
                100f +
                (result.QuantitativeRawIndex - 100f) *
                quantitativeReliability;

            for (int magnitudeIndex = 0;
                 magnitudeIndex < raw.AllSkillMagnitudes.Count;
                 magnitudeIndex++)
            {
                SkillMagnitudeProfile magnitude =
                    raw.AllSkillMagnitudes[magnitudeIndex];

                if (magnitude != null)
                {
                    result.SkillMagnitudes.Add(magnitude);
                }
            }

            for (int i = 0; i < raw.IndirectSignals.Count; i++)
            {
                IndirectPowerSignal signal = raw.IndirectSignals[i];

                if (signal == null)
                {
                    continue;
                }

                result.IndirectSignals.Add(signal);
                result.IndirectSignalCount++;

                switch (signal.Confidence)
                {
                    case MissionEvidenceConfidence.High:
                        result.HighConfidenceIndirectSignals++;
                        break;

                    case MissionEvidenceConfidence.Medium:
                        result.MediumConfidenceIndirectSignals++;
                        break;

                    default:
                        result.LowConfidenceIndirectSignals++;
                        break;
                }
            }

            result.VersatilityCount = 0;

            if (raw.Offense > 0f) result.VersatilityCount++;
            if (raw.Sustain > 0f) result.VersatilityCount++;
            if (raw.Defense > 0f) result.VersatilityCount++;
            if (raw.Mobility > 0f) result.VersatilityCount++;
            if (raw.Control > 0f) result.VersatilityCount++;
            if (raw.Utility > 0f) result.VersatilityCount++;

            result.SemanticCoverage =
                raw.AggregateSkillCount > 0
                    ? Clamp(
                        100f * raw.ScoredSkillCount /
                        raw.AggregateSkillCount,
                        0f,
                        100f
                    )
                    : 0f;

            int totalEvidence =
                raw.HighEvidence +
                raw.MediumEvidence +
                raw.LowEvidence;

            float evidenceConfidence =
                totalEvidence > 0
                    ? 100f * (
                        raw.HighEvidence * 1.00f +
                        raw.MediumEvidence * 0.70f +
                        raw.LowEvidence * 0.40f
                    ) / totalEvidence
                    : 0f;

            result.Confidence =
                Clamp(
                    20f +
                    evidenceConfidence * 0.55f +
                    result.SemanticCoverage * 0.25f,
                    0f,
                    100f
                );

            /*
             * Phase 1.3.2 mantiene semántica y magnitud separadas. El 13%
             * cuantitativo distingue magnitudes/sinergias, pero el índice se
             * aproxima a 100 (neutral) cuando la cobertura es insuficiente.
             * Confidence continúa fuera del cálculo: mide certeza.
             */
            result.CompositeIndex =
                Clamp(
                    result.BodyIndex * 0.28f +
                    result.OffenseIndex * 0.31f +
                    result.SustainIndex * 0.04f +
                    result.DefenseIndex * 0.05f +
                    result.MobilityIndex * 0.08f +
                    result.ControlIndex * 0.04f +
                    result.UtilityIndex * 0.02f +
                    result.IndirectPowerIndex * 0.05f +
                    result.QuantitativePowerIndex * 0.13f,
                    0f,
                    MaximumRelativeIndex
                );

            return result;
        }


        private static void ApplyCommunityCalibration(
            LocalPowerAssessment assessment
        )
        {
            if (assessment == null)
            {
                return;
            }

            CommunityPowerAssessment community;
            bool known =
                CommunityPowerCatalog.Instance.TryGet(
                    assessment.SurvivorId,
                    out community
                );

            assessment.ResolvedPowerScore = assessment.LocalPowerScore;

            if (community == null)
            {
                return;
            }

            assessment.CommunityPowerTier = community.Tier;
            assessment.CommunitySourceCount = community.SourceCount;
            assessment.CommunityIndependentSourceCount =
                community.IndependentSourceCount;
            assessment.CommunityConfidence = community.Confidence * 100f;
            assessment.CommunityConsensus = community.Consensus * 100f;

            if (!known || !community.IsKnown)
            {
                assessment.CommunityPowerTier = CommunityPowerTier.Unknown;
                return;
            }

            float targetScore =
                CommunityTierTargetScore(community.Tier);

            float reliability =
                Clamp(
                    community.Confidence * community.Consensus,
                    0f,
                    1f
                );

            float adjustment =
                (targetScore - assessment.LocalPowerScore) *
                reliability * 0.45f;

            /*
             * La comunidad sólo puede corregir aproximadamente una banda.
             * El análisis local sigue siendo la autoridad primaria.
             */
            adjustment = Clamp(adjustment, -10f, 10f);

            assessment.CommunityAdjustment = adjustment;
            assessment.ResolvedPowerScore =
                Clamp(
                    assessment.LocalPowerScore + adjustment,
                    25f,
                    200f
                );
        }


        private static float CommunityTierTargetScore(
            CommunityPowerTier tier
        )
        {
            switch (tier)
            {
                case CommunityPowerTier.C:
                    return 82f;

                case CommunityPowerTier.B:
                    return 92f;

                case CommunityPowerTier.A:
                    return 102f;

                case CommunityPowerTier.S:
                    return 114f;

                case CommunityPowerTier.SPlus:
                    return 126f;

                default:
                    return 100f;
            }
        }


        private static void CountEvidenceConfidence(
            MissionProfile profile,
            RawPowerVector result
        )
        {
            if (profile == null || result == null)
            {
                return;
            }

            for (int i = 0; i < profile.Evidence.Count; i++)
            {
                MissionCapabilityEvidence evidence =
                    profile.Evidence[i];

                if (evidence == null)
                {
                    continue;
                }

                switch (evidence.Confidence)
                {
                    case MissionEvidenceConfidence.High:
                        result.HighEvidence++;
                        break;

                    case MissionEvidenceConfidence.Medium:
                        result.MediumEvidence++;
                        break;

                    default:
                        result.LowEvidence++;
                        break;
                }
            }
        }


        private static float ConfidenceWeight(
            MissionEvidenceConfidence confidence
        )
        {
            switch (confidence)
            {
                case MissionEvidenceConfidence.High:
                    return 1.00f;

                case MissionEvidenceConfidence.Medium:
                    return 0.92f;

                default:
                    return 0.78f;
            }
        }


        private static float RelativeQuantitativeIndex(
            float value,
            float reference
        )
        {
            if (value <= 0f)
            {
                return 0f;
            }

            float ratio =
                SafeRatio(
                    value,
                    Math.Max(reference, MinimumQuantitativeReference),
                    0f
                );

            return SoftRelativeIndex(ratio);
        }


        private static float RelativeIndirectIndex(
            float value,
            float reference
        )
        {
            if (value <= 0f)
            {
                return 0f;
            }

            float ratio =
                SafeRatio(
                    value,
                    Math.Max(reference, MinimumIndirectReference),
                    0f
                );

            return SoftRelativeIndex(ratio);
        }


        private static float RelativeSkillIndex(
            float value,
            float reference
        )
        {
            if (value <= 0f)
            {
                return 0f;
            }

            float ratio =
                SafeRatio(
                    value,
                    Math.Max(reference, MinimumSkillReference),
                    0f
                );

            return SoftRelativeIndex(ratio);
        }


        private static float SoftRelativeIndex(
            float ratio
        )
        {
            if (ratio <= 0f)
            {
                return 0f;
            }

            if (ratio <= 1f)
            {
                return ratio * 100f;
            }

            /*
             * Sobre la referencia vanilla se usa una curva saturante en vez
             * de una multiplicación lineal con techo duro. Un valor 2x sigue
             * siendo claramente superior a 1x, pero ya no convierte pequeñas
             * referencias semánticas en picos artificiales de 300.
             */
            double excess = ratio - 1f;
            double saturated =
                1.0 - Math.Exp(-excess / SoftCurveScale);

            return Clamp(
                100f + (float)(saturated * 100.0),
                0f,
                MaximumRelativeIndex
            );
        }


        private static float SafeStatRatio(
            float value,
            float reference
        )
        {
            if (value <= 0f || reference <= 0f)
            {
                return 1f;
            }

            return Clamp(
                value / reference,
                0.40f,
                2.50f
            );
        }


        private static float ArmorRelative(
            float armor,
            float referenceArmor
        )
        {
            return Clamp(
                1f + (armor - referenceArmor) / 100f,
                0.50f,
                2.00f
            );
        }


        private static float CritRelative(
            float crit,
            float referenceCrit
        )
        {
            return Clamp(
                1f + (crit - referenceCrit) / 100f,
                0.75f,
                1.75f
            );
        }


        private static float SafeRatio(
            float value,
            float reference,
            float fallback
        )
        {
            if (reference <= 0f)
            {
                return fallback;
            }

            return value / reference;
        }


        private static float WeightedAverage(
            float a,
            float weightA,
            float b,
            float weightB,
            float c,
            float weightC
        )
        {
            float totalWeight =
                weightA + weightB + weightC;

            if (totalWeight <= 0f)
            {
                return 1f;
            }

            return (
                a * weightA +
                b * weightB +
                c * weightC
            ) / totalWeight;
        }


        private static float WeightedAverage4(
            float a,
            float weightA,
            float b,
            float weightB,
            float c,
            float weightC,
            float d,
            float weightD
        )
        {
            float totalWeight =
                weightA + weightB + weightC + weightD;

            if (totalWeight <= 0f)
            {
                return 1f;
            }

            return (
                a * weightA +
                b * weightB +
                c * weightC +
                d * weightD
            ) / totalWeight;
        }


        private static void AddPositive(
            List<float> values,
            float value
        )
        {
            if (!float.IsNaN(value) && !float.IsInfinity(value) && value > 0f)
            {
                values.Add(value);
            }
        }


        private static void AddFinite(
            List<float> values,
            float value
        )
        {
            if (!float.IsNaN(value) && !float.IsInfinity(value))
            {
                values.Add(value);
            }
        }


        private static float MedianPositive(
            List<float> values,
            float fallback
        )
        {
            if (values == null || values.Count == 0)
            {
                return fallback;
            }

            values.Sort();

            int middle = values.Count / 2;

            if ((values.Count % 2) == 1)
            {
                return values[middle];
            }

            return (
                values[middle - 1] +
                values[middle]
            ) * 0.5f;
        }


        private static float MedianFinite(
            List<float> values,
            float fallback
        )
        {
            return MedianPositive(values, fallback);
        }


        private static float Clamp(
            float value,
            float min,
            float max
        )
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }


        private sealed class VanillaReference
        {
            public int ProfileCount;
            public float BaseHealth;
            public float BaseRegen;
            public float BaseDamage;
            public float BaseMoveSpeed;
            public float BaseAcceleration;
            public float BaseJumpPower;
            public float BaseArmor;
            public float BaseAttackSpeed;
            public float BaseCrit;
            public float BaseJumpCount;
            public float Offense;
            public float Sustain;
            public float Defense;
            public float Mobility;
            public float Control;
            public float Utility;
            public float Indirect;
            public float Quantitative;
            public int IndirectReferenceCount;
            public int QuantitativeReferenceCount;
        }


        private sealed class RawPowerVector
        {
            public MissionProfile Profile;
            public float BaseHealth;
            public float BaseRegen;
            public float BaseDamage;
            public float BaseMoveSpeed;
            public float BaseAcceleration;
            public float BaseJumpPower;
            public float BaseArmor;
            public float BaseAttackSpeed;
            public float BaseCrit;
            public float BaseJumpCount;
            public float Offense;
            public float Sustain;
            public float Defense;
            public float Mobility;
            public float Control;
            public float Utility;
            public float Indirect;
            public float Quantitative;
            public List<SkillMagnitudeProfile> AllSkillMagnitudes =
                new List<SkillMagnitudeProfile>();
            public List<IndirectPowerSignal> IndirectSignals =
                new List<IndirectPowerSignal>();
            public int AggregateSkillCount;
            public int DefaultLoadoutOptionCount;
            public int ScoredSkillCount;
            public int QuantitativeSkillCount;
            public int QuantitativeScoredSkillCount;
            public int HighEvidence;
            public int MediumEvidence;
            public int LowEvidence;
        }


        private sealed class SlotPowerVector
        {
            public float Offense;
            public float Sustain;
            public float Defense;
            public float Mobility;
            public float Control;
            public float Utility;
            public float Indirect;
            public float Quantitative;
            public List<IndirectPowerSignal> IndirectSignals =
                new List<IndirectPowerSignal>();
            public bool HasSemanticSignal;

            public void SelectRepresentative(
                SlotPowerVector other
            )
            {
                if (other == null)
                {
                    return;
                }

                /*
                 * Una ranura estándar sólo puede aportar una variante a la
                 * vez. Phase 1 tomaba el máximo de cada dimensión por separado
                 * y podía fabricar un loadout imposible combinando ventajas de
                 * varias alternativas. Ahora se conserva el vector completo de
                 * una sola variante representativa.
                 */
                if (
                    !HasRepresentativeSignal() ||
                    other.RepresentativeScore() > RepresentativeScore()
                )
                {
                    CopyFrom(other);
                }
            }


            private float RepresentativeScore()
            {
                return
                    Offense * 0.25f +
                    Sustain * 0.13f +
                    Defense * 0.13f +
                    Mobility * 0.10f +
                    Control * 0.12f +
                    Utility * 0.08f +
                    Indirect * 0.07f +
                    Quantitative * 0.12f;
            }


            private bool HasRepresentativeSignal()
            {
                return HasSemanticSignal || Quantitative > 0f;
            }


            private void CopyFrom(
                SlotPowerVector other
            )
            {
                Offense = other.Offense;
                Sustain = other.Sustain;
                Defense = other.Defense;
                Mobility = other.Mobility;
                Control = other.Control;
                Utility = other.Utility;
                Indirect = other.Indirect;
                Quantitative = other.Quantitative;

                IndirectSignals.Clear();

                for (int i = 0; i < other.IndirectSignals.Count; i++)
                {
                    IndirectPowerSignal signal = other.IndirectSignals[i];

                    if (signal != null)
                    {
                        IndirectSignals.Add(signal);
                    }
                }

                HasSemanticSignal = other.HasSemanticSignal;
            }

            public void AddCapped(
                SlotPowerVector other,
                float cap
            )
            {
                if (other == null)
                {
                    return;
                }

                Offense = Math.Min(cap, Offense + other.Offense);
                Sustain = Math.Min(cap, Sustain + other.Sustain);
                Defense = Math.Min(cap, Defense + other.Defense);
                Mobility = Math.Min(cap, Mobility + other.Mobility);
                Control = Math.Min(cap, Control + other.Control);
                Utility = Math.Min(cap, Utility + other.Utility);
                Indirect = Math.Min(cap, Indirect + other.Indirect);
                Quantitative = Math.Min(4.0f, Quantitative + other.Quantitative);

                for (int i = 0; i < other.IndirectSignals.Count; i++)
                {
                    IndirectPowerSignal signal = other.IndirectSignals[i];

                    if (signal != null)
                    {
                        IndirectSignals.Add(signal);
                    }
                }

                HasSemanticSignal =
                    HasSemanticSignal ||
                    other.HasSemanticSignal;
            }
        }
    }
}
