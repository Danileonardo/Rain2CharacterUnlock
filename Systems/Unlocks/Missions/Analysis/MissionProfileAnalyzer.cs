using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

using RoR2;
using RoR2.Skills;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G3 Phase 2.3 - Mission Profile Analyzer + Semantic Edge Cases.
    ///
    /// Convierte SurvivorContentProfile (runtime real) en señales descriptivas
    /// consumibles por las futuras fases del generador.
    ///
    /// Esta clase NO decide dificultad/poder, NO genera misiones y NO persiste
    /// el perfil. Las detecciones semánticas se guardan además con evidencia
    /// para que las fases posteriores puedan distinguir dato observado de una
    /// inferencia textual conservadora.
    /// </summary>
    public static class MissionProfileAnalyzer
    {
        public const int AnalysisVersion = 5;

        private static readonly Dictionary<
            MissionProfileCapability,
            string[]
        > CapabilityKeywords =
            new Dictionary<
                MissionProfileCapability,
                string[]
            >
            {
                {
                    MissionProfileCapability.Mobility,
                    new string[]
                    {
                        "dash", "blink", "teleport", "roll", "leap",
                        "lunge", "grapple", "sprint", "movement",
                        "mobility", "chargeforward", "slide", "dodge",
                        "evade", "glide",
                        "salto", "embestida", "carrera", "movilidad",
                        "teletrans", "desplaz", "esquiv", "desliz",
                        "impuls", "abalanz", "ascend", "asciend",
                        "float", "flot"
                    }
                },
                {
                    MissionProfileCapability.Airborne,
                    new string[]
                    {
                        "airborne", "aerial", "flight", "flying", "fly",
                        "hover", "midair", "airstrike", "sky",
                        "aereo", "aéreo", "volar", "vuelo", "cielo",
                        "suspendid", "en el aire", "ascend", "asciend",
                        "float", "flot", "gran altura"
                    }
                },
                {
                    MissionProfileCapability.StatusEffect,
                    new string[]
                    {
                        "poison", "venom", "bleed", "burn", "ignite",
                        "stun", "freeze", "frozen", "chill", "shock",
                        "debuff", "dot", "slow", "root",
                        "enven", "sangr", "quem", "aturd", "congel",
                        "ralent", "estado"
                    }
                },
                {
                    MissionProfileCapability.SummonMinion,
                    new string[]
                    {
                        "minion", "turret", "drone", "companion",
                        "familiar", "spawnally", "allymaster",
                        "dummy", "decoy",
                        "esbirro", "torreta", "dron", "compañero",
                        "muñeco de práctica", "muneco de practica",
                        "señuelo", "senuelo"
                    }
                },
                {
                    MissionProfileCapability.Healing,
                    new string[]
                    {
                        "heal", "healing", "regenerate", "regeneration",
                        "restorehealth", "lifesteal", "leech",
                        "curar", "curan", "curarse",
                        "curacion", "curación", "sanar",
                        "regener", "robo de vida"
                    }
                },
                {
                    MissionProfileCapability.Defense,
                    new string[]
                    {
                        "barrier", "shield", "armor", "armour", "block",
                        "parry", "invulner", "immune", "guard",
                        "dodge", "evade",
                        "barrera", "escudo", "armadura", "bloque",
                        "parada", "invulner", "inmune", "esquiv",
                        "proteccion", "protección", "protection",
                        "evitar un ataque"
                    }
                },
                {
                    MissionProfileCapability.ResourceMechanic,
                    new string[]
                    {
                        "ammo", "ammunition", "energy", "mana", "rage",
                        "fuel", "meter", "resource", "combo", "stardust",
                        "battery", "heat", "overheat", "chargecounter",
                        "municion", "munición", "energia", "energía",
                        "furia", "combustible", "recurso", "polvo estelar",
                        "bateria", "batería", "sobrecal"
                    }
                },
                {
                    MissionProfileCapability.CriticalHit,
                    new string[]
                    {
                        "critical", "crit", "headshot", "weakpoint",
                        "critico", "crítico", "punto debil", "punto débil"
                    }
                },
                {
                    MissionProfileCapability.Backstab,
                    new string[]
                    {
                        "backstab", "back stab", "from behind", "behind",
                        "por la espalda", "desde atras", "desde atrás"
                    }
                },
                {
                    MissionProfileCapability.Execution,
                    new string[]
                    {
                        "execute", "execution", "finisher", "fatal hit",
                        "killing blow", "rematar", "ejecut", "golpe final"
                    }
                },
                {
                    MissionProfileCapability.Damage,
                    new string[]
                    {
                        "strike", "shot", "slash",
                        "blast", "explosion", "explode", "projectile",
                        "bullet", "missile", "bomb", "fireball",
                        "golpe", "disparo", "corte",
                        "explos", "proyectil", "misil", "bomba"
                    }
                }
            };

        private static readonly Dictionary<string, string[]>
            StatusEffectKeywords =
                new Dictionary<string, string[]>(
                    StringComparer.OrdinalIgnoreCase
                )
                {
                    {
                        "Poison",
                        new string[] { "poison", "venom", "enven" }
                    },
                    {
                        "Bleed",
                        new string[] { "bleed", "sangr" }
                    },
                    {
                        "Burn",
                        new string[] { "burn", "ignite", "quem" }
                    },
                    {
                        "Stun",
                        new string[] { "stun", "aturd" }
                    },
                    {
                        "Freeze",
                        new string[] { "freeze", "frozen", "congel" }
                    },
                    {
                        "Slow",
                        new string[] { "slow", "ralent" }
                    },
                    {
                        "Root",
                        new string[] { "root", "immobil", "inmov" }
                    },
                    {
                        "Soaked",
                        new string[] { "soaked", "empap" }
                    },
                    {
                        "Tangled",
                        new string[] { "tangle", "tangled", "enred" }
                    },
                    {
                        "ArmorBreak",
                        new string[]
                        {
                            "armorbreak", "armor break",
                            "armourbreak", "armour break",
                            "rompearmadura", "reduces armor",
                            "reduce armor"
                        }
                    },
                    {
                        "Protect",
                        new string[] { "protect", "proteg" }
                    },
                    {
                        "Reflect",
                        new string[] { "reflect", "reflej" }
                    },
                    {
                        "Sleep",
                        new string[] { "sleep", "sleeping", "sueñ" }
                    },
                    {
                        "Fatigued",
                        new string[] { "fatigue", "fatig" }
                    },
                    {
                        "Empowered",
                        new string[] { "empower", "potenciad" }
                    }
                };

        public static MissionProfile Analyze(
            SurvivorContentProfile contentProfile
        )
        {
            if (
                contentProfile == null ||
                contentProfile.SurvivorDef == null ||
                contentProfile.BodyPrefab == null
            )
            {
                return null;
            }

            SurvivorInfo survivorInfo =
                contentProfile.SurvivorInfo;

            string survivorId =
                SurvivorIdentity.Build(
                    survivorInfo
                );

            if (string.IsNullOrWhiteSpace(survivorId))
            {
                survivorId =
                    SurvivorIdentity.Build(
                        contentProfile.SourceIdentifier,
                        contentProfile.BodyName
                    );
            }

            MissionProfile result =
                new MissionProfile
                {
                    SurvivorId =
                        survivorId ?? "",

                    BodyName =
                        contentProfile.BodyName ?? "",

                    DisplayName =
                        contentProfile.DisplayName ?? "",

                    SourceIdentifier =
                        contentProfile.SourceIdentifier ?? "",

                    SourceAssembly =
                        contentProfile.SourceAssembly ?? "",

                    IsModded =
                        survivorInfo != null &&
                        survivorInfo.IsModded,

                    Status =
                        survivorInfo != null
                            ? survivorInfo.Status
                            : SurvivorStatus.Available,

                    HasOriginalUnlock =
                        survivorInfo != null &&
                        survivorInfo.HasOriginalUnlock,

                    UnlockSource =
                        contentProfile.SurvivorUnlock != null
                            ? contentProfile.SurvivorUnlock.Source
                            : SurvivorContentUnlockSource.Unknown,

                    IsUnlocked =
                        contentProfile.SurvivorUnlock == null ||
                        contentProfile.SurvivorUnlock.IsUnlocked,

                    Body =
                        BuildBodyProfile(
                            contentProfile
                        )
                };

            AnalyzeSkills(
                contentProfile,
                result
            );

            return result;
        }


        private static MissionBodyProfile BuildBodyProfile(
            SurvivorContentProfile contentProfile
        )
        {
            MissionBodyProfile result =
                new MissionBodyProfile();

            CharacterBody body =
                contentProfile.BodyPrefab != null
                    ? contentProfile.BodyPrefab
                        .GetComponent<CharacterBody>()
                    : null;

            if (body == null)
            {
                return result;
            }

            result.BaseMaxHealth =
                ReadFloat(
                    body,
                    0f,
                    "baseMaxHealth"
                );

            result.BaseRegen =
                ReadFloat(
                    body,
                    0f,
                    "baseRegen"
                );

            result.BaseDamage =
                ReadFloat(
                    body,
                    0f,
                    "baseDamage"
                );

            result.BaseMoveSpeed =
                ReadFloat(
                    body,
                    0f,
                    "baseMoveSpeed"
                );

            result.BaseAcceleration =
                ReadFloat(
                    body,
                    0f,
                    "baseAcceleration"
                );

            result.BaseJumpPower =
                ReadFloat(
                    body,
                    0f,
                    "baseJumpPower"
                );

            result.BaseArmor =
                ReadFloat(
                    body,
                    0f,
                    "baseArmor"
                );

            result.BaseAttackSpeed =
                ReadFloat(
                    body,
                    0f,
                    "baseAttackSpeed"
                );

            result.BaseCrit =
                ReadFloat(
                    body,
                    0f,
                    "baseCrit"
                );

            result.BaseJumpCount =
                ReadInt(
                    body,
                    0,
                    "baseJumpCount"
                );

            return result;
        }


        private static void AnalyzeSkills(
            SurvivorContentProfile contentProfile,
            MissionProfile missionProfile
        )
        {
            if (
                contentProfile == null ||
                missionProfile == null
            )
            {
                return;
            }

            missionProfile.SkillGroupCount =
                contentProfile.SkillGroups.Count;

            for (
                int groupIndex = 0;
                groupIndex < contentProfile.SkillGroups.Count;
                groupIndex++
            )
            {
                SurvivorContentSkillGroup group =
                    contentProfile.SkillGroups[groupIndex];

                if (group == null)
                {
                    continue;
                }

                if (group.Slot == SurvivorContentSkillSlot.Passive)
                {
                    missionProfile.HasPassive = true;
                }

                if (group.Slot == SurvivorContentSkillSlot.Extra)
                {
                    missionProfile.HasExtraSkillSlots = true;
                }

                for (
                    int variantIndex = 0;
                    variantIndex < group.Variants.Count;
                    variantIndex++
                )
                {
                    SurvivorContentSkillVariant variant =
                        group.Variants[variantIndex];

                    MissionSkillProfile skillProfile =
                        BuildSkillProfile(
                            group,
                            variant,
                            group.Variants.Count
                        );

                    if (skillProfile == null)
                    {
                        continue;
                    }

                    missionProfile.Skills.Add(
                        skillProfile
                    );

                    missionProfile.SkillVariantCount++;

                    if (!skillProfile.IsDefaultVariant)
                    {
                        missionProfile.AlternateSkillVariantCount++;
                    }

                    if (
                        skillProfile.HasUnlockRequirement &&
                        !skillProfile.IsUnlocked
                    )
                    {
                        missionProfile.LockedSkillVariantCount++;
                    }

                    MergeSkillSignals(
                        missionProfile,
                        skillProfile
                    );
                }
            }
        }


        private static MissionSkillProfile BuildSkillProfile(
            SurvivorContentSkillGroup group,
            SurvivorContentSkillVariant variant,
            int groupVariantCount
        )
        {
            if (
                group == null ||
                variant == null
            )
            {
                return null;
            }

            SkillDef skillDef =
                variant.SkillDef;

            MissionSkillProfile result =
                new MissionSkillProfile
                {
                    StableId =
                        variant.StableId ?? "",

                    Slot =
                        group.Slot,

                    VariantIndex =
                        variant.VariantIndex,

                    IsDefaultVariant =
                        variant.IsDefaultVariant,

                    InternalName =
                        variant.InternalName ?? "",

                    DisplayName =
                        variant.DisplayName ?? "",

                    Description =
                        variant.Description ?? "",

                    NameToken =
                        variant.NameToken ?? "",

                    SkillDefType =
                        skillDef != null
                            ? (
                                skillDef.GetType().FullName ??
                                skillDef.GetType().Name ??
                                ""
                            )
                            : "",

                    HasUnlockRequirement =
                        variant.Unlock != null &&
                        variant.Unlock.HasRequirement,

                    IsUnlocked =
                        variant.Unlock == null ||
                        variant.Unlock.IsUnlocked
                };

            if (skillDef == null)
            {
                DetectCapabilities(
                    variant,
                    result
                );

                AnalyzeSkillTextSemantics(
                    variant,
                    result
                );

                ApplyDescriptionSemanticOverrides(
                    variant.Description,
                    result
                );

                ClassifySkillUsage(
                    group,
                    groupVariantCount,
                    result
                );

                return result;
            }

            result.RechargeInterval =
                ReadFloat(
                    skillDef,
                    0f,
                    "baseRechargeInterval"
                );

            result.MaxStock =
                ReadInt(
                    skillDef,
                    0,
                    "baseMaxStock"
                );

            result.RequiredStock =
                ReadInt(
                    skillDef,
                    0,
                    "requiredStock"
                );

            result.StockToConsume =
                ReadInt(
                    skillDef,
                    0,
                    "stockToConsume"
                );

            result.BeginCooldownOnSkillEnd =
                ContentCatalogReflection.ReadBool(
                    skillDef,
                    false,
                    "beginSkillCooldownOnSkillEnd"
                );

            result.CancelSprintingOnActivation =
                ContentCatalogReflection.ReadBool(
                    skillDef,
                    false,
                    "cancelSprintingOnActivation"
                );

            result.ForceSprintDuringState =
                ContentCatalogReflection.ReadBool(
                    skillDef,
                    false,
                    "forceSprintDuringState"
                );

            result.ActivationStateMachineName =
                ContentCatalogReflection.ReadString(
                    skillDef,
                    "activationStateMachineName"
                );

            Type activationStateType =
                ResolveActivationStateTypeObject(
                    skillDef
                );

            result.ActivationStateType =
                activationStateType != null
                    ? (
                        activationStateType.FullName ??
                        activationStateType.Name ??
                        ""
                    )
                    : "";

            AppendStringValues(
                ReadMemberValue(
                    skillDef,
                    "keywordTokens"
                ),
                result.KeywordTokens
            );

            if (result.ForceSprintDuringState)
            {
                AddEvidence(
                    result,
                    MissionProfileCapability.Mobility,
                    "SkillMetadata",
                    "forceSprintDuringState",
                    MissionEvidenceConfidence.High
                );

                AddEffect(
                    result,
                    MissionSkillEffectKind.Mobility,
                    "SkillMetadata",
                    "forceSprintDuringState",
                    MissionEvidenceConfidence.High
                );
            }

            DetectCapabilities(
                variant,
                result
            );

            AnalyzeSkillTextSemantics(
                variant,
                result
            );

            AnalyzeRuntimeSemantics(
                skillDef,
                activationStateType,
                result
            );

            ApplyDescriptionSemanticOverrides(
                variant.Description,
                result
            );

            ClassifySkillUsage(
                group,
                groupVariantCount,
                result
            );

            return result;
        }


        private static void DetectCapabilities(
            SurvivorContentSkillVariant variant,
            MissionSkillProfile skillProfile
        )
        {
            if (
                variant == null ||
                skillProfile == null
            )
            {
                return;
            }

            StringBuilder builder =
                new StringBuilder();

            AppendSearchText(
                builder,
                variant.InternalName
            );

            AppendSearchText(
                builder,
                variant.NameToken
            );

            AppendSearchText(
                builder,
                variant.DisplayName
            );

            AppendSearchText(
                builder,
                variant.Description
            );

            AppendSearchText(
                builder,
                skillProfile.ActivationStateType
            );

            for (
                int i = 0;
                i < skillProfile.KeywordTokens.Count;
                i++
            )
            {
                AppendSearchText(
                    builder,
                    skillProfile.KeywordTokens[i]
                );
            }

            string searchable =
                NormalizeSearchText(
                    builder.ToString()
                );

            string descriptionSearchable =
                NormalizeSearchText(
                    variant.Description ?? ""
                );

            foreach (
                KeyValuePair<
                    MissionProfileCapability,
                    string[]
                > pair in CapabilityKeywords
            )
            {
                string matchedKeyword =
                    FindCapabilityKeyword(
                        searchable,
                        pair.Key,
                        pair.Value
                    );

                if (string.IsNullOrWhiteSpace(matchedKeyword))
                {
                    continue;
                }

                if (
                    ShouldSuppressTextCapability(
                        descriptionSearchable,
                        pair.Key
                    )
                )
                {
                    continue;
                }

                AddEvidence(
                    skillProfile,
                    pair.Key,
                    "SkillText",
                    matchedKeyword
                );
            }
        }


        private static void AnalyzeSkillTextSemantics(
            SurvivorContentSkillVariant variant,
            MissionSkillProfile skillProfile
        )
        {
            if (
                variant == null ||
                skillProfile == null
            )
            {
                return;
            }

            StringBuilder builder =
                new StringBuilder();

            AppendSearchText(builder, variant.InternalName);
            AppendSearchText(builder, variant.NameToken);
            AppendSearchText(builder, variant.DisplayName);
            AppendSearchText(builder, variant.Description);
            AppendSearchText(builder, skillProfile.SkillDefType);
            AppendSearchText(builder, skillProfile.ActivationStateType);

            for (
                int i = 0;
                i < skillProfile.KeywordTokens.Count;
                i++
            )
            {
                AppendSearchText(
                    builder,
                    skillProfile.KeywordTokens[i]
                );
            }

            string searchable =
                NormalizeSearchText(
                    builder.ToString()
                );

            string descriptionSearchable =
                NormalizeSearchText(
                    variant.Description ?? ""
                );

            foreach (
                KeyValuePair<
                    MissionProfileCapability,
                    string[]
                > pair in CapabilityKeywords
            )
            {
                string matchedKeyword =
                    FindCapabilityKeyword(
                        searchable,
                        pair.Key,
                        pair.Value
                    );

                if (string.IsNullOrWhiteSpace(matchedKeyword))
                {
                    continue;
                }

                if (
                    ShouldSuppressTextCapability(
                        descriptionSearchable,
                        pair.Key
                    )
                )
                {
                    continue;
                }

                MissionSkillEffectKind effectKind =
                    MapCapabilityToEffect(
                        pair.Key
                    );

                AddEffect(
                    skillProfile,
                    effectKind,
                    "SkillText",
                    matchedKeyword,
                    MissionEvidenceConfidence.Low
                );
            }

            if (
                ContainsWholeWord(
                    searchable,
                    "mp"
                ) &&
                !ShouldSuppressTextCapability(
                    descriptionSearchable,
                    MissionProfileCapability.ResourceMechanic
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Resource,
                    MissionProfileCapability.ResourceMechanic,
                    "SkillText",
                    "MP",
                    MissionEvidenceConfidence.Low
                );
            }

            if (
                HasOutgoingDamageText(
                    searchable
                ) &&
                !ShouldSuppressTextCapability(
                    descriptionSearchable,
                    MissionProfileCapability.Damage
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Damage,
                    MissionProfileCapability.Damage,
                    "SkillText",
                    "explicit outgoing damage",
                    MissionEvidenceConfidence.Low
                );
            }

            DetectStatusNames(
                searchable,
                skillProfile,
                "SkillText",
                MissionEvidenceConfidence.Low,
                descriptionSearchable
            );

            if (
                ContainsAny(
                    searchable,
                    "projectile",
                    "proyectil",
                    "missile",
                    "misil",
                    "bullet",
                    "bala"
                )
            )
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.Projectile,
                    "SkillText",
                    "projectile/ranged",
                    MissionEvidenceConfidence.Low
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "blast",
                    "explosion",
                    "explode",
                    "radius",
                    "area of effect",
                    "aoe",
                    "explos",
                    "radio",
                    "area"
                )
            )
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.AreaEffect,
                    "SkillText",
                    "area/blast",
                    MissionEvidenceConfidence.Low
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "melee",
                    "slash",
                    "swing",
                    "punch",
                    "kick",
                    "hammer",
                    "knife",
                    "cuchillo",
                    "martillo",
                    "corte",
                    "golpe cuerpo"
                )
            )
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.Melee,
                    "SkillText",
                    "melee",
                    MissionEvidenceConfidence.Low
                );
            }
        }


        private static void AnalyzeRuntimeSemantics(
            SkillDef skillDef,
            Type activationStateType,
            MissionSkillProfile skillProfile
        )
        {
            if (
                skillDef == null ||
                skillProfile == null
            )
            {
                return;
            }

            AnalyzeTypeNameSemantics(
                skillDef.GetType(),
                skillProfile,
                "SkillDefType"
            );

            if (activationStateType != null)
            {
                AnalyzeTypeNameSemantics(
                    activationStateType,
                    skillProfile,
                    "ActivationStateType"
                );
            }

            InspectRuntimeMembers(
                skillDef.GetType(),
                skillDef,
                skillProfile,
                "SkillDefField"
            );

            if (activationStateType != null)
            {
                InspectRuntimeMembers(
                    activationStateType,
                    null,
                    skillProfile,
                    "StateField"
                );
            }
        }


        private static void AnalyzeTypeNameSemantics(
            Type type,
            MissionSkillProfile skillProfile,
            string source
        )
        {
            if (
                type == null ||
                skillProfile == null
            )
            {
                return;
            }

            string typeText =
                NormalizeSearchText(
                    (
                        type.FullName ??
                        type.Name ??
                        ""
                    )
                );

            if (string.IsNullOrWhiteSpace(typeText))
            {
                return;
            }

            AnalyzeSemanticText(
                typeText,
                skillProfile,
                source,
                MissionEvidenceConfidence.Medium
            );

            if (
                ContainsAny(
                    typeText,
                    "hammer",
                    "melee",
                    "slash",
                    "swing",
                    "knife"
                )
            )
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.Melee,
                    source,
                    type.Name,
                    MissionEvidenceConfidence.Medium
                );
            }

            if (
                ContainsAny(
                    typeText,
                    "projectile",
                    "missile",
                    "bullet"
                )
            )
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.Projectile,
                    source,
                    type.Name,
                    MissionEvidenceConfidence.Medium
                );
            }
        }


        private static void InspectRuntimeMembers(
            Type initialType,
            object target,
            MissionSkillProfile skillProfile,
            string source
        )
        {
            if (
                initialType == null ||
                skillProfile == null
            )
            {
                return;
            }

            Type currentType =
                initialType;

            int inspectedMembers = 0;

            while (
                currentType != null &&
                currentType != typeof(object) &&
                inspectedMembers < 96
            )
            {
                if (
                    currentType != initialType &&
                    IsFrameworkBaseType(currentType)
                )
                {
                    break;
                }

                FieldInfo[] fields;

                try
                {
                    fields =
                        currentType.GetFields(
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Static |
                            BindingFlags.Instance |
                            BindingFlags.DeclaredOnly
                        );
                }
                catch
                {
                    fields = null;
                }

                if (fields != null)
                {
                    for (
                        int i = 0;
                        i < fields.Length &&
                        inspectedMembers < 96;
                        i++
                    )
                    {
                        FieldInfo field =
                            fields[i];

                        if (field == null)
                        {
                            continue;
                        }

                        inspectedMembers++;

                        object value = null;
                        bool valueRead = false;

                        if (
                            field.IsStatic ||
                            (
                                target != null &&
                                field.DeclaringType != null &&
                                field.DeclaringType.IsInstanceOfType(target)
                            )
                        )
                        {
                            try
                            {
                                value =
                                    field.GetValue(
                                        field.IsStatic
                                            ? null
                                            : target
                                    );

                                valueRead = true;
                            }
                            catch
                            {
                                value = null;
                            }
                        }

                        /*
                         * No tratamos la mera existencia de un campo como una
                         * prueba de comportamiento. En EntityStates abundan
                         * campos de soporte (velocity, armor, slow, etc.) que
                         * no necesariamente representan un efecto aplicado por
                         * la skill. Sólo usamos miembros cuyo valor runtime se
                         * pudo leer y es semánticamente significativo.
                         */
                        if (
                            !valueRead ||
                            !IsMeaningfulRuntimeValue(value)
                        )
                        {
                            continue;
                        }

                        string detail =
                            BuildMemberDetail(
                                field,
                                value,
                                true
                            );

                        string searchable =
                            NormalizeSearchText(
                                (
                                    field.Name ?? ""
                                ) +
                                " " +
                                BuildValueSearchText(value)
                            );

                        if (string.IsNullOrWhiteSpace(searchable))
                        {
                            continue;
                        }

                        AnalyzeSemanticText(
                            searchable,
                            skillProfile,
                            source,
                            MissionEvidenceConfidence.High,
                            detail
                        );
                    }
                }

                currentType =
                    currentType.BaseType;
            }
        }


        private static bool IsFrameworkBaseType(
            Type type
        )
        {
            if (type == null)
            {
                return true;
            }

            if (type == typeof(SkillDef))
            {
                return true;
            }

            string namespaceName =
                type.Namespace ?? "";

            string typeName =
                type.Name ?? "";

            if (
                string.Equals(
                    namespaceName,
                    "EntityStates",
                    StringComparison.Ordinal
                ) &&
                (
                    string.Equals(
                        typeName,
                        "EntityState",
                        StringComparison.Ordinal
                    ) ||
                    string.Equals(
                        typeName,
                        "BaseState",
                        StringComparison.Ordinal
                    ) ||
                    string.Equals(
                        typeName,
                        "BaseSkillState",
                        StringComparison.Ordinal
                    )
                )
            )
            {
                return true;
            }

            return false;
        }


        private static void AnalyzeSemanticText(
            string searchable,
            MissionSkillProfile skillProfile,
            string source,
            MissionEvidenceConfidence confidence,
            string detailOverride = null
        )
        {
            if (
                string.IsNullOrWhiteSpace(searchable) ||
                skillProfile == null
            )
            {
                return;
            }

            string detail =
                string.IsNullOrWhiteSpace(detailOverride)
                    ? searchable
                    : detailOverride;

            bool runtimeMember =
                string.Equals(
                    source,
                    "SkillDefField",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    source,
                    "StateField",
                    StringComparison.Ordinal
                );

            bool damageSignal =
                runtimeMember
                    ? ContainsAny(
                        searchable,
                        "damagecoefficient",
                        "damage coefficient",
                        "basedamage",
                        "bonusdamage",
                        "attackdamage"
                    )
                    : ContainsAny(
                        searchable,
                        "damagecoefficient",
                        "damage coefficient",
                        "basedamage",
                        "bonusdamage",
                        "damage",
                        "attackdamage",
                        "damagetype"
                    );

            if (damageSignal)
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Damage,
                    MissionProfileCapability.Damage,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                HasHealingSemanticSignal(
                    searchable
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Healing,
                    MissionProfileCapability.Healing,
                    source,
                    detail,
                    confidence
                );
            }

            bool crowdControl =
                runtimeMember
                    ? ContainsAny(
                        searchable,
                        "stun",
                        "freeze",
                        "frozen",
                        "slowbuff",
                        "slowduration",
                        "slowed",
                        "root",
                        "knockback",
                        "knockup",
                        "tangle",
                        "immobil"
                    )
                    : ContainsAny(
                        searchable,
                        "stun",
                        "freeze",
                        "frozen",
                        "slow",
                        "root",
                        "knockback",
                        "knockup",
                        "tangle",
                        "immobil"
                    );

            bool statusEffect =
                crowdControl ||
                ContainsAny(
                    searchable,
                    "buffdef",
                    "buffindex",
                    "debuff",
                    "dotindex",
                    "dotcontroller",
                    "poison",
                    "venom",
                    "bleed",
                    "burn",
                    "ignite",
                    "soaked",
                    "armorbreak",
                    "armor break",
                    "status"
                );

            if (statusEffect)
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.StatusEffect,
                    MissionProfileCapability.StatusEffect,
                    source,
                    detail,
                    confidence
                );

                DetectStatusNames(
                    searchable,
                    skillProfile,
                    source,
                    confidence
                );
            }

            if (crowdControl)
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.CrowdControl,
                    source,
                    detail,
                    confidence
                );
            }

            bool mobilitySignal =
                runtimeMember
                    ? ContainsAny(
                        searchable,
                        "dash",
                        "blink",
                        "teleport",
                        "leap",
                        "lunge",
                        "grapple",
                        "jump",
                        "flight",
                        "hover",
                        "airdash"
                    )
                    : ContainsAny(
                        searchable,
                        "dash",
                        "blink",
                        "teleport",
                        "leap",
                        "lunge",
                        "grapple",
                        "movespeed",
                        "move speed",
                        "velocity",
                        "jump",
                        "flight",
                        "hover",
                        "airdash"
                    );

            if (mobilitySignal)
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Mobility,
                    MissionProfileCapability.Mobility,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "airborne",
                    "aerial",
                    "flight",
                    "flying",
                    "hover",
                    "midair",
                    "airdash",
                    "air dash"
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Airborne,
                    MissionProfileCapability.Airborne,
                    source,
                    detail,
                    confidence
                );
            }

            bool armorReduction =
                ContainsAny(
                    searchable,
                    "armorbreak",
                    "armor break",
                    "armourbreak",
                    "armour break",
                    "armorreduction",
                    "armor reduction",
                    "reducearmor",
                    "reduce armor"
                );

            if (
                !armorReduction &&
                ContainsAny(
                    searchable,
                    "barrier",
                    "shield",
                    "armor",
                    "armour",
                    "block",
                    "parry",
                    "reflect",
                    "invulner",
                    "immune",
                    "guard"
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Defense,
                    MissionProfileCapability.Defense,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "summon",
                    "minion",
                    "deployable",
                    "turret",
                    "drone",
                    "companion",
                    "allymaster",
                    "spawnally",
                    "masterprefab",
                    "mastersummon"
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.SummonMinion,
                    MissionProfileCapability.SummonMinion,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "ammo",
                    "ammunition",
                    "mana",
                    "energy",
                    "resource",
                    "meter",
                    "gauge",
                    "stockpile",
                    "rage",
                    "fuel",
                    "stardust",
                    "heat",
                    "overheat"
                ) ||
                ContainsWholeWord(
                    searchable,
                    "mp"
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Resource,
                    MissionProfileCapability.ResourceMechanic,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "critical",
                    "crit",
                    "weakpoint",
                    "headshot"
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.CriticalHit,
                    MissionProfileCapability.CriticalHit,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "backstab",
                    "frombehind",
                    "from behind",
                    "behind"
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Backstab,
                    MissionProfileCapability.Backstab,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "execute",
                    "execution",
                    "finisher",
                    "killingblow",
                    "killing blow",
                    "fatalhit",
                    "fatal hit"
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Execution,
                    MissionProfileCapability.Execution,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "projectile",
                    "missile",
                    "bullet",
                    "orb"
                )
            )
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.Projectile,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "blast",
                    "radius",
                    "aoe",
                    "areaeffect",
                    "area effect",
                    "explosion"
                )
            )
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.AreaEffect,
                    source,
                    detail,
                    confidence
                );
            }

            if (
                ContainsAny(
                    searchable,
                    "melee",
                    "slash",
                    "swing",
                    "hammer",
                    "knife",
                    "punch",
                    "kick"
                )
            )
            {
                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.Melee,
                    source,
                    detail,
                    confidence
                );
            }
        }


        private static void AddSemanticEvidence(
            MissionSkillProfile skillProfile,
            MissionSkillEffectKind effectKind,
            MissionProfileCapability capability,
            string source,
            string detail,
            MissionEvidenceConfidence confidence
        )
        {
            AddEffect(
                skillProfile,
                effectKind,
                source,
                detail,
                confidence
            );

            AddEvidence(
                skillProfile,
                capability,
                source,
                detail,
                confidence
            );
        }


        private static void AddEffect(
            MissionSkillProfile skillProfile,
            MissionSkillEffectKind kind,
            string source,
            string detail,
            MissionEvidenceConfidence confidence
        )
        {
            if (
                skillProfile == null ||
                kind == MissionSkillEffectKind.Unknown
            )
            {
                return;
            }

            string normalizedDetail =
                detail ?? "";

            for (
                int i = 0;
                i < skillProfile.Effects.Count;
                i++
            )
            {
                MissionSkillEffectProfile existing =
                    skillProfile.Effects[i];

                if (
                    existing != null &&
                    existing.Kind == kind &&
                    string.Equals(
                        existing.Detail ?? "",
                        normalizedDetail,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    if ((int)confidence > (int)existing.Confidence)
                    {
                        existing.Confidence = confidence;
                        existing.Source = source ?? "";
                    }

                    return;
                }
            }

            if (skillProfile.Effects.Count >= 32)
            {
                return;
            }

            skillProfile.Effects.Add(
                new MissionSkillEffectProfile
                {
                    Kind = kind,
                    Source = source ?? "",
                    Detail = normalizedDetail,
                    Confidence = confidence
                }
            );
        }


        private static MissionSkillEffectKind MapCapabilityToEffect(
            MissionProfileCapability capability
        )
        {
            switch (capability)
            {
                case MissionProfileCapability.Damage:
                    return MissionSkillEffectKind.Damage;

                case MissionProfileCapability.Mobility:
                    return MissionSkillEffectKind.Mobility;

                case MissionProfileCapability.Airborne:
                    return MissionSkillEffectKind.Airborne;

                case MissionProfileCapability.StatusEffect:
                    return MissionSkillEffectKind.StatusEffect;

                case MissionProfileCapability.SummonMinion:
                    return MissionSkillEffectKind.SummonMinion;

                case MissionProfileCapability.Healing:
                    return MissionSkillEffectKind.Healing;

                case MissionProfileCapability.Defense:
                    return MissionSkillEffectKind.Defense;

                case MissionProfileCapability.SelfBuff:
                    return MissionSkillEffectKind.SelfBuff;

                case MissionProfileCapability.ResourceMechanic:
                    return MissionSkillEffectKind.Resource;

                case MissionProfileCapability.CriticalHit:
                    return MissionSkillEffectKind.CriticalHit;

                case MissionProfileCapability.Backstab:
                    return MissionSkillEffectKind.Backstab;

                case MissionProfileCapability.Execution:
                    return MissionSkillEffectKind.Execution;

                default:
                    return MissionSkillEffectKind.Unknown;
            }
        }


        private static void DetectStatusNames(
            string searchable,
            MissionSkillProfile skillProfile,
            string source,
            MissionEvidenceConfidence confidence,
            string negationText = null
        )
        {
            if (
                string.IsNullOrWhiteSpace(searchable) ||
                skillProfile == null
            )
            {
                return;
            }

            foreach (
                KeyValuePair<string, string[]> pair
                    in StatusEffectKeywords
            )
            {
                string match =
                    FindKeyword(
                        searchable,
                        pair.Value
                    );

                if (string.IsNullOrWhiteSpace(match))
                {
                    continue;
                }

                if (
                    !string.IsNullOrWhiteSpace(negationText) &&
                    IsAnyTermNegated(
                        negationText,
                        pair.Value
                    )
                )
                {
                    continue;
                }

                AddUniqueString(
                    skillProfile.StatusEffects,
                    pair.Key
                );

                AddEffect(
                    skillProfile,
                    MissionSkillEffectKind.StatusEffect,
                    source,
                    pair.Key + ":" + match,
                    confidence
                );
            }
        }


        private static string BuildMemberDetail(
            FieldInfo field,
            object value,
            bool valueRead
        )
        {
            if (field == null)
            {
                return "";
            }

            string detail =
                field.Name ?? "";

            if (!valueRead || value == null)
            {
                return detail;
            }

            string valueText =
                BuildValueDisplayText(
                    value
                );

            if (string.IsNullOrWhiteSpace(valueText))
            {
                return detail;
            }

            return
                detail +
                "=" +
                valueText;
        }


        private static string BuildValueDisplayText(
            object value
        )
        {
            if (value == null)
            {
                return "";
            }

            if (value is UnityEngine.Object unityObject)
            {
                string objectName =
                    unityObject.name ?? "";

                string typeName =
                    unityObject.GetType().Name ?? "";

                if (string.IsNullOrWhiteSpace(objectName))
                {
                    return typeName;
                }

                return
                    typeName +
                    ":" +
                    objectName;
            }

            if (
                value is string ||
                value is Enum ||
                value is bool ||
                value is byte ||
                value is sbyte ||
                value is short ||
                value is ushort ||
                value is int ||
                value is uint ||
                value is long ||
                value is ulong ||
                value is float ||
                value is double ||
                value is decimal
            )
            {
                try
                {
                    return
                        Convert.ToString(
                            value,
                            CultureInfo.InvariantCulture
                        ) ?? "";
                }
                catch
                {
                    return value.ToString();
                }
            }

            return "";
        }


        private static bool IsMeaningfulRuntimeValue(
            object value
        )
        {
            if (value == null)
            {
                return false;
            }

            if (value is bool boolValue)
            {
                return boolValue;
            }

            if (value is string stringValue)
            {
                return
                    !string.IsNullOrWhiteSpace(
                        stringValue
                    );
            }

            if (value is Enum enumValue)
            {
                try
                {
                    return
                        Convert.ToInt64(
                            enumValue,
                            CultureInfo.InvariantCulture
                        ) != 0L;
                }
                catch
                {
                    return true;
                }
            }

            if (
                value is byte ||
                value is sbyte ||
                value is short ||
                value is ushort ||
                value is int ||
                value is uint ||
                value is long ||
                value is ulong ||
                value is float ||
                value is double ||
                value is decimal
            )
            {
                try
                {
                    double numericValue =
                        Convert.ToDouble(
                            value,
                            CultureInfo.InvariantCulture
                        );

                    return
                        Math.Abs(numericValue) >
                        0.000001d;
                }
                catch
                {
                    return false;
                }
            }

            if (value is Type)
            {
                return true;
            }

            if (value is UnityEngine.Object)
            {
                return true;
            }

            /*
             * Objetos arbitrarios se ignoran: su sola existencia no prueba
             * una semántica de gameplay y fue la principal fuente de señales
             * High espurias en Phase 2.
             */
            return false;
        }


        private static string BuildValueSearchText(
            object value
        )
        {
            if (value == null)
            {
                return "";
            }

            if (value is UnityEngine.Object unityObject)
            {
                return
                    (
                        unityObject.GetType().FullName ??
                        unityObject.GetType().Name ??
                        ""
                    ) +
                    " " +
                    (
                        unityObject.name ?? ""
                    );
            }

            if (value is Type typeValue)
            {
                return
                    typeValue.FullName ??
                    typeValue.Name ??
                    "";
            }

            if (
                value is string ||
                value is Enum
            )
            {
                return value.ToString();
            }

            return "";
        }


        private static void ApplyDescriptionSemanticOverrides(
            string description,
            MissionSkillProfile skillProfile
        )
        {
            if (skillProfile == null)
            {
                return;
            }

            string searchable =
                NormalizeSearchText(
                    description ?? ""
                );

            if (string.IsNullOrWhiteSpace(searchable))
            {
                return;
            }

            /*
             * Algunas traducciones describen la curación mediante formas
             * verbales que no contienen literalmente "heal"/"curar".
             * Exigimos además una referencia explícita a salud/health para
             * no confundir recuperación de MP, munición o cooldowns.
             */
            if (
                HasExplicitHealingDescription(searchable) &&
                !ShouldSuppressTextCapability(
                    searchable,
                    MissionProfileCapability.Healing
                )
            )
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.Healing,
                    MissionProfileCapability.Healing,
                    "SkillDescription",
                    "explicit health recovery",
                    MissionEvidenceConfidence.Low
                );
            }

            /*
             * "Aliado" por sí solo NO implica invocación. Sí tratamos como
             * capacidad de minions la conversión explícita de enemigos en
             * aliados, porque crea unidades aliadas controladas por la skill.
             */
            if (HasEnemyToAllyConversion(searchable))
            {
                AddSemanticEvidence(
                    skillProfile,
                    MissionSkillEffectKind.SummonMinion,
                    MissionProfileCapability.SummonMinion,
                    "SkillDescription",
                    "enemy-to-ally conversion",
                    MissionEvidenceConfidence.Low
                );
            }

            /*
             * Una mejora explícita de la propia habilidad/usuario no es un
             * debuff ni un status genérico sobre un objetivo. La registramos
             * separadamente para que G4 pueda valorar auto-buffs sin
             * confundirlos con control/estados ofensivos.
             */
            ApplySelfEmpowermentSemantics(
                searchable,
                skillProfile
            );

            /*
             * El texto localizado es la autoridad semántica para negaciones
             * explícitas. Corrige casos como "Vara aturdidora: en realidad
             * no aturde", incluso si el nombre/tipo runtime contiene "stun".
             */
            ApplyExplicitStatusNegations(
                searchable,
                skillProfile
            );
        }


        private static string FindCapabilityKeyword(
            string searchable,
            MissionProfileCapability capability,
            string[] keywords
        )
        {
            if (
                capability != MissionProfileCapability.Healing
            )
            {
                return FindKeyword(
                    searchable,
                    keywords
                );
            }

            /*
             * "heal" es prefijo de "health". Un Contains normal hacía
             * que frases como "maximum health" o "below 5% health"
             * pareciesen curación. Conservamos restorehealth/healthrestore
             * como señales explícitas y retiramos health antes de buscar
             * stems de heal/healing.
             */
            if (
                ContainsAny(
                    searchable,
                    "restorehealth",
                    "healthrestore"
                )
            )
            {
                return "restorehealth";
            }

            string withoutHealth =
                RemoveHealthReferenceForHealing(
                    searchable
                );

            return FindKeyword(
                withoutHealth,
                keywords
            );
        }


        private static bool HasHealingSemanticSignal(
            string searchable
        )
        {
            if (string.IsNullOrWhiteSpace(searchable))
            {
                return false;
            }

            if (
                ContainsAny(
                    searchable,
                    "restorehealth",
                    "healthrestore"
                )
            )
            {
                return true;
            }

            string withoutHealth =
                RemoveHealthReferenceForHealing(
                    searchable
                );

            return
                ContainsAny(
                    withoutHealth,
                    "heal",
                    "healing",
                    "regeneration",
                    "regen",
                    "lifesteal",
                    "leech"
                );
        }


        private static string RemoveHealthReferenceForHealing(
            string searchable
        )
        {
            if (string.IsNullOrWhiteSpace(searchable))
            {
                return "";
            }

            return searchable.Replace(
                "health",
                " "
            );
        }


        private static void ApplySelfEmpowermentSemantics(
            string descriptionSearchable,
            MissionSkillProfile skillProfile
        )
        {
            if (
                skillProfile == null ||
                !HasSelfSkillEmpowerment(
                    descriptionSearchable
                )
            )
            {
                return;
            }

            AddSemanticEvidence(
                skillProfile,
                MissionSkillEffectKind.SelfBuff,
                MissionProfileCapability.SelfBuff,
                "SkillDescription",
                "self/skill empowerment",
                MissionEvidenceConfidence.Low
            );

            /*
             * DetectStatusNames puede haber interpretado la palabra
             * Empowered/empowers como un estado. Si proviene sólo del texto
             * de auto-potenciación, retiramos esa etiqueta concreta. No
             * tocamos otros estados reales (Slow, Stun, Poison, etc.) ni
             * evidencia runtime de confianza Medium/High.
             */
            for (
                int i = skillProfile.StatusEffects.Count - 1;
                i >= 0;
                i--
            )
            {
                if (
                    string.Equals(
                        skillProfile.StatusEffects[i],
                        "Empowered",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    skillProfile.StatusEffects.RemoveAt(i);
                }
            }

            for (
                int i = skillProfile.Effects.Count - 1;
                i >= 0;
                i--
            )
            {
                MissionSkillEffectProfile effect =
                    skillProfile.Effects[i];

                if (
                    effect != null &&
                    effect.Kind ==
                        MissionSkillEffectKind.StatusEffect &&
                    effect.Confidence ==
                        MissionEvidenceConfidence.Low &&
                    ContainsAny(
                        NormalizeSearchText(
                            effect.Detail ?? ""
                        ),
                        "empower",
                        "potenciad"
                    )
                )
                {
                    skillProfile.Effects.RemoveAt(i);
                }
            }
        }


        private static bool HasSelfSkillEmpowerment(
            string searchable
        )
        {
            if (string.IsNullOrWhiteSpace(searchable))
            {
                return false;
            }

            return
                ContainsAny(
                    searchable,
                    "empower your",
                    "empowers your",
                    "empower her",
                    "empowers her",
                    "empower his",
                    "empowers his",
                    "empower their",
                    "empowers their",
                    "empower next",
                    "empowers next",
                    "potencia tu",
                    "potencia tus",
                    "potencia su",
                    "potencia sus",
                    "potenciar tu",
                    "potenciar su",
                    "potencia la siguiente",
                    "potencia el siguiente"
                );
        }


        private static bool HasExplicitHealingDescription(
            string searchable
        )
        {
            if (string.IsNullOrWhiteSpace(searchable))
            {
                return false;
            }

            if (
                HasHealingSemanticSignal(searchable) ||
                ContainsAny(
                    searchable,
                    "curar", "curan", "curarse",
                    "sanar", "robo de vida"
                ) ||
                ContainsWholeWord(
                    searchable,
                    "cura"
                )
            )
            {
                return true;
            }

            bool hasHealthReference =
                ContainsAny(
                    searchable,
                    " health", "health ", "salud", " hp ",
                    "hit points", "puntos de vida"
                );

            bool hasRecoveryVerb =
                ContainsAny(
                    searchable,
                    "recover", "recovers", "recovered",
                    "restore", "restores", "restored",
                    "recupera", "recuperar", "restaura", "restaurar"
                );

            return
                hasHealthReference &&
                hasRecoveryVerb;
        }


        private static bool HasEnemyToAllyConversion(
            string searchable
        )
        {
            if (string.IsNullOrWhiteSpace(searchable))
            {
                return false;
            }

            return
                ContainsAny(
                    searchable,
                    "convert enemies into allies",
                    "converts enemies into allies",
                    "converted into allies",
                    "become allies",
                    "becomes an ally",
                    "se convierten en aliados",
                    "se convierte en aliado",
                    "convierte enemigos en aliados",
                    "convertir enemigos en aliados"
                );
        }


        private static void ApplyExplicitStatusNegations(
            string descriptionSearchable,
            MissionSkillProfile skillProfile
        )
        {
            if (
                string.IsNullOrWhiteSpace(descriptionSearchable) ||
                skillProfile == null
            )
            {
                return;
            }

            bool removedAny = false;

            foreach (
                KeyValuePair<string, string[]> pair
                    in StatusEffectKeywords
            )
            {
                if (
                    !IsAnyTermNegated(
                        descriptionSearchable,
                        pair.Value
                    )
                )
                {
                    continue;
                }

                for (
                    int i = skillProfile.StatusEffects.Count - 1;
                    i >= 0;
                    i--
                )
                {
                    if (
                        string.Equals(
                            skillProfile.StatusEffects[i],
                            pair.Key,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        skillProfile.StatusEffects.RemoveAt(i);
                        removedAny = true;
                    }
                }
            }

            if (!removedAny)
            {
                return;
            }

            /*
             * Si tras aplicar las negaciones no queda ningún estado concreto,
             * eliminamos las señales genéricas de estado/control que podían
             * provenir únicamente del nombre o de metadata con ese término.
             */
            if (skillProfile.StatusEffects.Count == 0)
            {
                for (
                    int i = skillProfile.Effects.Count - 1;
                    i >= 0;
                    i--
                )
                {
                    MissionSkillEffectProfile effect =
                        skillProfile.Effects[i];

                    if (
                        effect != null &&
                        (
                            effect.Kind ==
                                MissionSkillEffectKind.StatusEffect ||
                            effect.Kind ==
                                MissionSkillEffectKind.CrowdControl
                        )
                    )
                    {
                        skillProfile.Effects.RemoveAt(i);
                    }
                }

                for (
                    int i = skillProfile.Evidence.Count - 1;
                    i >= 0;
                    i--
                )
                {
                    MissionCapabilityEvidence evidence =
                        skillProfile.Evidence[i];

                    if (
                        evidence != null &&
                        evidence.Capability ==
                            MissionProfileCapability.StatusEffect
                    )
                    {
                        skillProfile.Evidence.RemoveAt(i);
                    }
                }

                skillProfile.Capabilities.Remove(
                    MissionProfileCapability.StatusEffect
                );
            }
        }


        private static bool HasOutgoingDamageText(
            string searchable
        )
        {
            if (string.IsNullOrWhiteSpace(searchable))
            {
                return false;
            }

            return
                ContainsAny(
                    searchable,
                    "deal damage",
                    "deals damage",
                    "dealing damage",
                    "inflict",
                    "inflig",
                    "% damage",
                    "% de daño",
                    "damage per",
                    "daño por",
                    "damage to",
                    "daño a",
                    "strike",
                    "slash",
                    "shot",
                    "blast",
                    "explosion",
                    "explode",
                    "projectile",
                    "bullet",
                    "missile",
                    "bomb",
                    "fireball",
                    "golpe",
                    "disparo",
                    "corte",
                    "explos",
                    "proyectil",
                    "misil",
                    "bomba"
                );
        }


        private static bool ContainsWholeWord(
            string searchable,
            string term
        )
        {
            if (
                string.IsNullOrWhiteSpace(searchable) ||
                string.IsNullOrWhiteSpace(term)
            )
            {
                return false;
            }

            int index = 0;

            while (index < searchable.Length)
            {
                index =
                    searchable.IndexOf(
                        term,
                        index,
                        StringComparison.OrdinalIgnoreCase
                    );

                if (index < 0)
                {
                    return false;
                }

                int beforeIndex =
                    index - 1;

                int afterIndex =
                    index + term.Length;

                bool validBefore =
                    beforeIndex < 0 ||
                    !char.IsLetterOrDigit(
                        searchable[beforeIndex]
                    );

                bool validAfter =
                    afterIndex >= searchable.Length ||
                    !char.IsLetterOrDigit(
                        searchable[afterIndex]
                    );

                if (
                    validBefore &&
                    validAfter
                )
                {
                    return true;
                }

                index +=
                    Math.Max(
                        1,
                        term.Length
                    );
            }

            return false;
        }


        private static bool ShouldSuppressTextCapability(
            string descriptionSearchable,
            MissionProfileCapability capability
        )
        {
            if (
                IsCapabilityExplicitlyNegated(
                    descriptionSearchable,
                    capability
                )
            )
            {
                return true;
            }

            if (
                capability ==
                    MissionProfileCapability.Mobility &&
                IsExternalTargetMobilityOnly(
                    descriptionSearchable
                )
            )
            {
                return true;
            }

            return false;
        }


        private static bool IsExternalTargetMobilityOnly(
            string descriptionSearchable
        )
        {
            if (string.IsNullOrWhiteSpace(descriptionSearchable))
            {
                return false;
            }

            bool hasTargetReference =
                ContainsAny(
                    descriptionSearchable,
                    "ally",
                    "allies",
                    "enemy",
                    "enemies",
                    "target",
                    "aliado",
                    "aliados",
                    "enemigo",
                    "enemigos",
                    "objetivo"
                );

            bool hasMovementReference =
                ContainsAny(
                    descriptionSearchable,
                    "teleport",
                    "teletrans",
                    "pull",
                    "push",
                    "knockback",
                    "atrae",
                    "empuja"
                );

            bool hasSelfMovementReference =
                ContainsAny(
                    descriptionSearchable,
                    "dash",
                    "blink",
                    "roll",
                    "leap",
                    "lunge",
                    "grapple",
                    "slide",
                    "dodge",
                    "evade",
                    "glide",
                    "salto",
                    "embestida",
                    "desplaz",
                    "esquiv",
                    "desliz",
                    "impuls",
                    "abalanz",
                    "te teletransport",
                    "teleport yourself"
                );

            return
                hasTargetReference &&
                hasMovementReference &&
                !hasSelfMovementReference;
        }


        private static bool IsCapabilityExplicitlyNegated(
            string descriptionSearchable,
            MissionProfileCapability capability
        )
        {
            if (string.IsNullOrWhiteSpace(descriptionSearchable))
            {
                return false;
            }

            string[] terms;

            switch (capability)
            {
                case MissionProfileCapability.Damage:
                    terms =
                        new string[]
                        {
                            "damage", "daño", "attack", "ataque",
                            "hit", "golpe"
                        };
                    break;

                case MissionProfileCapability.Healing:
                    terms =
                        new string[]
                        {
                            "heal", "healing", "regen", "regener",
                            "curar", "cura", "sanar"
                        };
                    break;

                case MissionProfileCapability.StatusEffect:
                    terms =
                        new string[]
                        {
                            "stun", "aturd", "freeze", "congel",
                            "slow", "ralent", "root", "inmov",
                            "poison", "enven", "bleed", "sangr",
                            "burn", "quem"
                        };
                    break;

                case MissionProfileCapability.SummonMinion:
                    terms =
                        new string[]
                        {
                            "summon", "minion", "companion",
                            "invoc", "esbirro", "compañero"
                        };
                    break;

                case MissionProfileCapability.Defense:
                    terms =
                        new string[]
                        {
                            "shield", "barrier", "armor", "armour",
                            "block", "parry", "invulner",
                            "escudo", "barrera", "armadura",
                            "bloque", "parada", "invulner"
                        };
                    break;

                case MissionProfileCapability.ResourceMechanic:
                    terms =
                        new string[]
                        {
                            "mana", "energy", "resource", "ammo",
                            "munición", "municion", "energía", "energia",
                            "recurso", "mp"
                        };
                    break;

                default:
                    return false;
            }

            return
                IsAnyTermNegated(
                    descriptionSearchable,
                    terms
                );
        }


        private static bool IsAnyTermNegated(
            string searchable,
            string[] terms
        )
        {
            if (
                string.IsNullOrWhiteSpace(searchable) ||
                terms == null
            )
            {
                return false;
            }

            for (
                int termIndex = 0;
                termIndex < terms.Length;
                termIndex++
            )
            {
                string term =
                    terms[termIndex];

                if (string.IsNullOrWhiteSpace(term))
                {
                    continue;
                }

                int searchIndex = 0;

                while (searchIndex < searchable.Length)
                {
                    int index =
                        searchable.IndexOf(
                            term,
                            searchIndex,
                            StringComparison.OrdinalIgnoreCase
                        );

                    if (index < 0)
                    {
                        break;
                    }

                    int windowStart =
                        Math.Max(
                            0,
                            index - 42
                        );

                    string prefix =
                        searchable.Substring(
                            windowStart,
                            index - windowStart
                        );

                    if (
                        ContainsAny(
                            " " + prefix + " ",
                            " no ",
                            " not ",
                            " never ",
                            " nunca ",
                            " sin ",
                            " cannot ",
                            " can t ",
                            " does not ",
                            " doesn t ",
                            " en realidad no "
                        )
                    )
                    {
                        return true;
                    }

                    searchIndex =
                        index +
                        Math.Max(
                            1,
                            term.Length
                        );
                }
            }

            return false;
        }


        private static bool ContainsAny(
            string searchable,
            params string[] terms
        )
        {
            if (
                string.IsNullOrWhiteSpace(searchable) ||
                terms == null
            )
            {
                return false;
            }

            for (
                int i = 0;
                i < terms.Length;
                i++
            )
            {
                string term =
                    terms[i];

                if (
                    !string.IsNullOrWhiteSpace(term) &&
                    searchable.IndexOf(
                        term,
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    return true;
                }
            }

            return false;
        }


        private static void ClassifySkillUsage(
            SurvivorContentSkillGroup group,
            int groupVariantCount,
            MissionSkillProfile skillProfile
        )
        {
            if (
                group == null ||
                skillProfile == null
            )
            {
                return;
            }

            if (group.Slot == SurvivorContentSkillSlot.Passive)
            {
                skillProfile.UsageKind =
                    MissionSkillUsageKind.Passive;

                skillProfile.ContributesToAggregate =
                    true;

                return;
            }

            if (group.Slot != SurvivorContentSkillSlot.Extra)
            {
                skillProfile.UsageKind =
                    MissionSkillUsageKind.Active;

                skillProfile.ContributesToAggregate =
                    true;

                return;
            }

            string stateName =
                ShortTypeName(
                    skillProfile.ActivationStateType
                );

            bool passiveLikeState =
                string.IsNullOrWhiteSpace(stateName) ||
                string.Equals(
                    stateName,
                    "Idle",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    stateName,
                    "BaseState",
                    StringComparison.OrdinalIgnoreCase
                );

            if (
                passiveLikeState &&
                groupVariantCount > 1
            )
            {
                /*
                 * Muchos mods representan armas, perks o configuraciones
                 * alternativas como grupos Extra cuyos SkillDef apuntan a
                 * Idle/BaseState. Son opciones mutuamente excluyentes, no
                 * habilidades simultáneas. Las conservamos para diagnóstico
                 * pero no las agregamos al perfil que consumirá G4.
                 */
                skillProfile.UsageKind =
                    MissionSkillUsageKind.LoadoutOption;

                skillProfile.ContributesToAggregate =
                    false;

                return;
            }

            if (passiveLikeState)
            {
                skillProfile.UsageKind =
                    MissionSkillUsageKind.Passive;

                skillProfile.ContributesToAggregate =
                    true;

                return;
            }

            skillProfile.UsageKind =
                MissionSkillUsageKind.AuxiliaryActive;

            skillProfile.ContributesToAggregate =
                true;
        }


        private static string ShortTypeName(
            string fullName
        )
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "";
            }

            int index =
                fullName.LastIndexOf('.');

            if (
                index < 0 ||
                index >= fullName.Length - 1
            )
            {
                return fullName;
            }

            return
                fullName.Substring(
                    index + 1
                );
        }


        private static void MergeSkillSignals(
            MissionProfile missionProfile,
            MissionSkillProfile skillProfile
        )
        {
            if (
                missionProfile == null ||
                skillProfile == null ||
                !skillProfile.ContributesToAggregate
            )
            {
                return;
            }

            for (
                int i = 0;
                i < skillProfile.Capabilities.Count;
                i++
            )
            {
                MissionProfileCapability capability =
                    skillProfile.Capabilities[i];

                if (!missionProfile.Capabilities.Contains(capability))
                {
                    missionProfile.Capabilities.Add(
                        capability
                    );
                }
            }

            for (
                int i = 0;
                i < skillProfile.Evidence.Count;
                i++
            )
            {
                MissionCapabilityEvidence evidence =
                    skillProfile.Evidence[i];

                if (evidence != null)
                {
                    missionProfile.Evidence.Add(
                        evidence
                    );
                }
            }
        }


        private static void AddEvidence(
            MissionSkillProfile skillProfile,
            MissionProfileCapability capability,
            string source,
            string detail,
            MissionEvidenceConfidence confidence =
                MissionEvidenceConfidence.Low
        )
        {
            if (
                skillProfile == null ||
                capability == MissionProfileCapability.Unknown
            )
            {
                return;
            }

            if (!skillProfile.Capabilities.Contains(capability))
            {
                skillProfile.Capabilities.Add(
                    capability
                );
            }

            for (
                int i = 0;
                i < skillProfile.Evidence.Count;
                i++
            )
            {
                MissionCapabilityEvidence existing =
                    skillProfile.Evidence[i];

                if (
                    existing != null &&
                    existing.Capability == capability
                )
                {
                    if ((int)confidence > (int)existing.Confidence)
                    {
                        existing.Source = source ?? "";
                        existing.Detail = detail ?? "";
                        existing.Confidence = confidence;
                    }

                    return;
                }
            }

            skillProfile.Evidence.Add(
                new MissionCapabilityEvidence
                {
                    Capability = capability,
                    SkillStableId = skillProfile.StableId ?? "",
                    Source = source ?? "",
                    Detail = detail ?? "",
                    Confidence = confidence
                }
            );
        }


        private static Type ResolveActivationStateTypeObject(
            SkillDef skillDef
        )
        {
            object activationState =
                ReadMemberValue(
                    skillDef,
                    "activationState"
                );

            if (activationState == null)
            {
                return null;
            }

            object stateType =
                ReadMemberValue(
                    activationState,
                    "stateType"
                );

            if (stateType is Type type)
            {
                return type;
            }

            return null;
        }


        private static void AppendSearchText(
            StringBuilder builder,
            string value
        )
        {
            if (
                builder == null ||
                string.IsNullOrWhiteSpace(value)
            )
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(value);
        }


        private static string NormalizeSearchText(
            string value
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string withoutMarkup =
                StripRichTextTags(
                    value
                );

            StringBuilder builder =
                new StringBuilder(
                    withoutMarkup.Length
                );

            bool previousWasSpace =
                false;

            for (
                int i = 0;
                i < withoutMarkup.Length;
                i++
            )
            {
                char character =
                    char.ToLowerInvariant(
                        withoutMarkup[i]
                    );

                bool isSeparator =
                    char.IsWhiteSpace(character) ||
                    character == '_' ||
                    character == '-' ||
                    character == '\\' ||
                    character == '/' ||
                    character == ':' ||
                    character == ';' ||
                    character == ',' ||
                    character == '.' ||
                    character == '(' ||
                    character == ')' ||
                    character == '[' ||
                    character == ']' ||
                    character == '{' ||
                    character == '}' ||
                    character == '!' ||
                    character == '?' ||
                    character == '"' ||
                    character == '\'';

                if (isSeparator)
                {
                    if (
                        builder.Length > 0 &&
                        !previousWasSpace
                    )
                    {
                        builder.Append(' ');
                        previousWasSpace = true;
                    }

                    continue;
                }

                builder.Append(character);
                previousWasSpace = false;
            }

            return
                builder
                    .ToString()
                    .Trim();
        }


        private static string StripRichTextTags(
            string value
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            StringBuilder builder =
                new StringBuilder(
                    value.Length
                );

            bool insideTag =
                false;

            for (
                int i = 0;
                i < value.Length;
                i++
            )
            {
                char character =
                    value[i];

                if (character == '<')
                {
                    insideTag = true;
                    continue;
                }

                if (insideTag)
                {
                    if (character == '>')
                    {
                        insideTag = false;
                    }

                    continue;
                }

                builder.Append(character);
            }

            return builder.ToString();
        }


        private static string FindKeyword(
            string searchable,
            string[] keywords
        )
        {
            if (
                string.IsNullOrWhiteSpace(searchable) ||
                keywords == null
            )
            {
                return "";
            }

            for (
                int i = 0;
                i < keywords.Length;
                i++
            )
            {
                string keyword =
                    keywords[i];

                if (
                    !string.IsNullOrWhiteSpace(keyword) &&
                    searchable.IndexOf(
                        keyword,
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    return keyword;
                }
            }

            return "";
        }


        private static float ReadFloat(
            object target,
            float fallback,
            params string[] memberNames
        )
        {
            object value =
                ReadFirstMemberValue(
                    target,
                    memberNames
                );

            if (value == null)
            {
                return fallback;
            }

            try
            {
                return Convert.ToSingle(
                    value,
                    CultureInfo.InvariantCulture
                );
            }
            catch
            {
                return fallback;
            }
        }


        private static int ReadInt(
            object target,
            int fallback,
            params string[] memberNames
        )
        {
            object value =
                ReadFirstMemberValue(
                    target,
                    memberNames
                );

            if (value == null)
            {
                return fallback;
            }

            try
            {
                return Convert.ToInt32(
                    value,
                    CultureInfo.InvariantCulture
                );
            }
            catch
            {
                return fallback;
            }
        }


        private static object ReadFirstMemberValue(
            object target,
            string[] memberNames
        )
        {
            if (
                target == null ||
                memberNames == null
            )
            {
                return null;
            }

            for (
                int i = 0;
                i < memberNames.Length;
                i++
            )
            {
                object value =
                    ReadMemberValue(
                        target,
                        memberNames[i]
                    );

                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }


        private static object ReadMemberValue(
            object target,
            string memberName
        )
        {
            if (
                target == null ||
                string.IsNullOrWhiteSpace(memberName)
            )
            {
                return null;
            }

            Type type =
                target.GetType();

            const BindingFlags Flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;

            try
            {
                PropertyInfo property =
                    type.GetProperty(
                        memberName,
                        Flags
                    );

                if (
                    property != null &&
                    property.CanRead
                )
                {
                    return
                        property.GetValue(
                            target,
                            null
                        );
                }

                FieldInfo field =
                    type.GetField(
                        memberName,
                        Flags
                    );

                if (field != null)
                {
                    return field.GetValue(target);
                }
            }
            catch
            {
                // Diagnóstico best-effort: una propiedad problemática no
                // debe impedir construir el resto del perfil.
            }

            return null;
        }


        private static void AppendStringValues(
            object rawValue,
            List<string> destination
        )
        {
            if (
                rawValue == null ||
                destination == null
            )
            {
                return;
            }

            if (rawValue is string singleValue)
            {
                AddUniqueString(
                    destination,
                    singleValue
                );

                return;
            }

            if (!(rawValue is IEnumerable enumerable))
            {
                return;
            }

            foreach (object value in enumerable)
            {
                if (value == null)
                {
                    continue;
                }

                AddUniqueString(
                    destination,
                    value.ToString()
                );
            }
        }


        private static void AddUniqueString(
            List<string> destination,
            string value
        )
        {
            if (
                destination == null ||
                string.IsNullOrWhiteSpace(value)
            )
            {
                return;
            }

            for (
                int i = 0;
                i < destination.Count;
                i++
            )
            {
                if (
                    string.Equals(
                        destination[i],
                        value,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return;
                }
            }

            destination.Add(value);
        }
    }
}
