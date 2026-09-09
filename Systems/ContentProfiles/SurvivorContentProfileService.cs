using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using BepInEx.Logging;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// 5G.2D-C - Survivor Content Profile.
    ///
    /// Convierte el catálogo bruto en una jerarquía centrada en el survivor:
    ///
    /// Survivor
    ///  |- información / lore
    ///  |- unlock del personaje
    ///  |- sus SkillFamily + variantes
    ///  |- sus SkinDef
    ///
    /// No modifica ningún unlock. Es una capa de sólo lectura para 5G.2E.
    /// </summary>
    public static class SurvivorContentProfileService
    {
        private static readonly List<SurvivorContentProfile>
            Profiles =
                new List<SurvivorContentProfile>();

        private static readonly Dictionary<string, SurvivorContentProfile>
            ProfilesByBody =
                new Dictionary<string, SurvivorContentProfile>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static readonly List<SurvivorInfo>
            CachedSurvivors =
                new List<SurvivorInfo>();

        private static ManualLogSource logger;
        private static bool initialized;

        public static int Count =>
            Profiles.Count;

        public static void Initialize(
            IReadOnlyList<SurvivorInfo> survivors,
            ManualLogSource activeLogger
        )
        {
            logger =
                activeLogger;

            CacheSurvivors(
                survivors
            );

            if (!initialized)
            {
                initialized =
                    true;

                Language.onCurrentLanguageChanged +=
                    OnLanguageChanged;
            }

            Rebuild();
        }

        public static void Rebuild()
        {
            Profiles.Clear();
            ProfilesByBody.Clear();

            UserProfile profile =
                TryGetLocalProfile();

            for (
                int i = 0;
                i < CachedSurvivors.Count;
                i++
            )
            {
                SurvivorInfo survivorInfo =
                    CachedSurvivors[i];

                SurvivorContentProfile contentProfile =
                    BuildProfile(
                        survivorInfo,
                        profile
                    );

                if (contentProfile == null)
                {
                    continue;
                }

                Profiles.Add(
                    contentProfile
                );

                if (
                    !string.IsNullOrWhiteSpace(
                        contentProfile.BodyName
                    ) &&
                    !ProfilesByBody.ContainsKey(
                        contentProfile.BodyName
                    )
                )
                {
                    ProfilesByBody.Add(
                        contentProfile.BodyName,
                        contentProfile
                    );
                }
            }

            Profiles.Sort(
                delegate(
                    SurvivorContentProfile a,
                    SurvivorContentProfile b
                )
                {
                    return string.Compare(
                        a?.DisplayName,
                        b?.DisplayName,
                        StringComparison.CurrentCultureIgnoreCase
                    );
                }
            );

            LogSummary();
        }

        public static void RefreshDiscovery()
        {
            /*
             * Por ahora el refresco reconstruye solamente estas fichas.
             * No toca el ContentCatalog global ni providers.
             */
            Rebuild();
        }

        /// <summary>
        /// Comprueba el estado REAL del UserProfile contra el snapshot que
        /// consume el Browser. Sólo reconstruye si detecta un cambio.
        ///
        /// Esto cubre desbloqueos/rebloqueos hechos por otros mods (por
        /// ejemplo right-click unlock) sin convertir el Browser en un polling
        /// costoso: el host decide cada cuánto consultar y aquí no se hace
        /// ningún Rebuild si el estado sigue idéntico.
        /// </summary>
        public static bool RefreshDiscoveryIfChanged()
        {
            UserProfile userProfile =
                TryGetLocalProfile();

            if (userProfile == null)
            {
                return false;
            }

            for (int i = 0; i < Profiles.Count; i++)
            {
                SurvivorContentProfile profile =
                    Profiles[i];

                if (
                    profile == null ||
                    profile.SurvivorDef == null
                )
                {
                    continue;
                }

                UnlockableDef currentUnlockable =
                    profile.SurvivorDef.unlockableDef;

                ContentCatalogDiscoveryState currentDiscovery =
                    ResolveUnlockableDiscovery(
                        currentUnlockable,
                        userProfile,
                        unlockedWhenNull: true
                    );

                bool currentIsUnlocked =
                    currentUnlockable == null ||
                    currentDiscovery ==
                        ContentCatalogDiscoveryState.Discovered;

                bool unlockableChanged =
                    profile.SurvivorUnlock == null
                        ? currentUnlockable != null
                        : !ReferenceEquals(
                            profile.SurvivorUnlock.UnlockableDef,
                            currentUnlockable
                        );

                bool unlockedFlagChanged =
                    profile.SurvivorUnlock != null &&
                    profile.SurvivorUnlock.IsUnlocked !=
                        currentIsUnlocked;

                if (
                    profile.Discovery != currentDiscovery ||
                    unlockableChanged ||
                    unlockedFlagChanged
                )
                {
                    Rebuild();
                    return true;
                }
            }

            return false;
        }

        public static IReadOnlyList<SurvivorContentProfile> GetAll()
        {
            return Profiles;
        }

        public static bool TryGetByBodyName(
            string bodyName,
            out SurvivorContentProfile profile
        )
        {
            profile =
                null;

            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return false;
            }

            return ProfilesByBody.TryGetValue(
                bodyName,
                out profile
            );
        }

        private static void OnLanguageChanged()
        {
            Rebuild();
        }

        private static void CacheSurvivors(
            IReadOnlyList<SurvivorInfo> survivors
        )
        {
            CachedSurvivors.Clear();

            if (survivors == null)
            {
                return;
            }

            for (
                int i = 0;
                i < survivors.Count;
                i++
            )
            {
                SurvivorInfo survivor =
                    survivors[i];

                if (survivor != null)
                {
                    CachedSurvivors.Add(
                        survivor
                    );
                }
            }
        }

        // =========================================================
        // PROFILE
        // =========================================================

        private static SurvivorContentProfile BuildProfile(
            SurvivorInfo survivorInfo,
            UserProfile userProfile
        )
        {
            if (
                survivorInfo == null ||
                survivorInfo.SurvivorDef == null
            )
            {
                return null;
            }

            SurvivorDef survivorDef =
                survivorInfo.SurvivorDef;

            GameObject bodyPrefab =
                survivorDef.bodyPrefab;

            if (bodyPrefab == null)
            {
                return null;
            }

            CharacterBody body =
                bodyPrefab.GetComponent<CharacterBody>();

            string bodyName =
                !string.IsNullOrWhiteSpace(
                    survivorInfo.BodyName
                )
                    ? survivorInfo.BodyName
                    : bodyPrefab.name;

            string displayName =
                ResolveLocalizedToken(
                    survivorDef,
                    "displayNameToken"
                );

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName =
                    survivorInfo.DisplayName;
            }

            string subtitle =
                ResolveLocalizedToken(
                    body,
                    "subtitleNameToken",
                    "subtitleToken"
                );

            string description =
                ResolveLocalizedToken(
                    survivorDef,
                    "descriptionToken"
                );

            string lore =
                ResolveLore(
                    survivorDef,
                    body
                );

            Texture portrait =
                body != null
                    ? body.portraitIcon
                    : null;

            if (portrait == null)
            {
                ContentCatalogEntry catalogEntry;

                if (
                    ContentCatalogService.TryGet(
                        "survivor:" + bodyName,
                        out catalogEntry
                    )
                )
                {
                    portrait =
                        catalogEntry.IconTexture;
                }
            }

            UnlockableDef survivorUnlockable =
                survivorDef.unlockableDef;

            ContentCatalogDiscoveryState discovery =
                ResolveUnlockableDiscovery(
                    survivorUnlockable,
                    userProfile,
                    unlockedWhenNull: true
                );

            SurvivorContentProfile result =
                new SurvivorContentProfile
                {
                    SurvivorInfo =
                        survivorInfo,

                    SurvivorDef =
                        survivorDef,

                    BodyName =
                        bodyName ?? "",

                    InternalName =
                        survivorInfo.InternalName ?? "",

                    DisplayName =
                        displayName ?? "",

                    Subtitle =
                        subtitle ?? "",

                    Description =
                        description ?? "",

                    Lore =
                        lore ?? "",

                    SourceIdentifier =
                        ResolveSourceIdentifier(
                            survivorInfo,
                            survivorDef
                        ),

                    SourceAssembly =
                        survivorInfo.SourceAssembly ?? "",

                    Portrait =
                        portrait,

                    BodyPrefab =
                        bodyPrefab,

                    Discovery =
                        discovery,

                    HideIdentityWhenUndiscovered =
                        survivorUnlockable != null,

                    SurvivorUnlock =
                        BuildUnlockInfo(
                            survivorUnlockable,
                            userProfile
                        )
                };

            BuildSkillGroups(
                result,
                bodyPrefab,
                userProfile
            );

            BuildSkins(
                result,
                bodyPrefab,
                body,
                userProfile
            );

            UsuLog.Verbose(
                logger,
                "[SURVIVOR PROFILE] " +
                result.DisplayName +
                " | Body: " +
                result.BodyName +
                " | Skills: " +
                CountSkillVariants(result) +
                " | Grupos: " +
                result.SkillGroups.Count +
                " | Skins: " +
                result.Skins.Count +
                " | Lore: " +
                (!string.IsNullOrWhiteSpace(result.Lore) ? "Sí" : "No") +
                " | Unlock: " +
                result.SurvivorUnlock.Source
            );

            return result;
        }

        // =========================================================
        // SKILLS CONTEXTUALES
        // =========================================================

        private static void BuildSkillGroups(
            SurvivorContentProfile profile,
            GameObject bodyPrefab,
            UserProfile userProfile
        )
        {
            if (
                profile == null ||
                bodyPrefab == null
            )
            {
                return;
            }

            SkillLocator locator =
                bodyPrefab.GetComponent<SkillLocator>();

            BuildPassive(
                profile,
                locator
            );

            GenericSkill[] genericSkills =
                bodyPrefab.GetComponents<GenericSkill>();

            if (genericSkills == null)
            {
                return;
            }

            int extraIndex =
                0;

            HashSet<string> stableIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            for (
                int i = 0;
                i < genericSkills.Length;
                i++
            )
            {
                GenericSkill genericSkill =
                    genericSkills[i];

                if (genericSkill == null)
                {
                    continue;
                }

                SkillFamily family =
                    ContentCatalogReflection
                        .ReadUnityObject<SkillFamily>(
                            genericSkill,
                            "skillFamily"
                        );

                SurvivorContentSkillSlot slot =
                    ResolveSkillSlot(
                        genericSkill,
                        locator
                    );

                int currentExtraIndex =
                    0;

                if (slot == SurvivorContentSkillSlot.Extra)
                {
                    extraIndex++;
                    currentExtraIndex =
                        extraIndex;
                }

                SurvivorContentSkillGroup group =
                    new SurvivorContentSkillGroup
                    {
                        StableId =
                            BuildGroupStableId(
                                profile.BodyName,
                                slot,
                                currentExtraIndex
                            ),

                        Slot =
                            slot,

                        SlotLabel =
                            ResolveSlotLabel(
                                slot,
                                currentExtraIndex
                            ),

                        ExtraSlotIndex =
                            currentExtraIndex,

                        GenericSkill =
                            genericSkill,

                        SkillFamily =
                            family
                    };

                if (family != null)
                {
                    BuildFamilyVariants(
                        profile,
                        group,
                        family,
                        userProfile,
                        stableIds
                    );
                }
                else
                {
                    /*
                     * Algunos mods asignan la SkillDef directamente y no
                     * exponen una SkillFamily normal. La recogemos igualmente.
                     */
                    SkillDef currentSkillDef =
                        ContentCatalogReflection
                            .ReadUnityObject<SkillDef>(
                                genericSkill,
                                "skillDef"
                            );

                    if (currentSkillDef != null)
                    {
                        SurvivorContentSkillVariant variant =
                            BuildSkillVariant(
                                profile,
                                group,
                                currentSkillDef,
                                null,
                                0,
                                true,
                                userProfile
                            );

                        if (
                            variant != null &&
                            stableIds.Add(variant.StableId)
                        )
                        {
                            group.Variants.Add(
                                variant
                            );
                        }
                    }
                }

                if (group.Variants.Count > 0)
                {
                    profile.SkillGroups.Add(
                        group
                    );
                }
            }

            profile.SkillGroups.Sort(
                delegate(
                    SurvivorContentSkillGroup a,
                    SurvivorContentSkillGroup b
                )
                {
                    int compare =
                        ((int)a.Slot).CompareTo(
                            (int)b.Slot
                        );

                    if (compare != 0)
                    {
                        return compare;
                    }

                    return a.ExtraSlotIndex.CompareTo(
                        b.ExtraSlotIndex
                    );
                }
            );
        }

        private static void BuildFamilyVariants(
            SurvivorContentProfile profile,
            SurvivorContentSkillGroup group,
            SkillFamily family,
            UserProfile userProfile,
            HashSet<string> stableIds
        )
        {
            Array variants =
                ReadArrayMember(
                    family,
                    "variants"
                );

            if (variants == null)
            {
                return;
            }

            int defaultVariantIndex =
                ReadIntegerMember(
                    family,
                    0,
                    "defaultVariantIndex"
                );

            for (
                int i = 0;
                i < variants.Length;
                i++
            )
            {
                object rawVariant =
                    variants.GetValue(i);

                if (rawVariant == null)
                {
                    continue;
                }

                SkillDef skillDef =
                    ContentCatalogReflection
                        .ReadUnityObject<SkillDef>(
                            rawVariant,
                            "skillDef"
                        );

                if (skillDef == null)
                {
                    continue;
                }

                UnlockableDef unlockable =
                    ContentCatalogReflection
                        .ReadUnityObject<UnlockableDef>(
                            rawVariant,
                            "unlockableDef"
                        );

                SurvivorContentSkillVariant variant =
                    BuildSkillVariant(
                        profile,
                        group,
                        skillDef,
                        unlockable,
                        i,
                        i == defaultVariantIndex,
                        userProfile
                    );

                if (
                    variant == null ||
                    !stableIds.Add(variant.StableId)
                )
                {
                    continue;
                }

                group.Variants.Add(
                    variant
                );
            }
        }

        private static SurvivorContentSkillVariant BuildSkillVariant(
            SurvivorContentProfile profile,
            SurvivorContentSkillGroup group,
            SkillDef skillDef,
            UnlockableDef unlockable,
            int variantIndex,
            bool isDefault,
            UserProfile userProfile
        )
        {
            if (
                profile == null ||
                group == null ||
                skillDef == null
            )
            {
                return null;
            }

            string internalName =
                SkillCatalog.GetSkillName(
                    skillDef.skillIndex
                );

            if (string.IsNullOrWhiteSpace(internalName))
            {
                internalName =
                    skillDef.skillName;
            }

            if (string.IsNullOrWhiteSpace(internalName))
            {
                internalName =
                    skillDef.skillNameToken;
            }

            if (string.IsNullOrWhiteSpace(internalName))
            {
                internalName =
                    group.Slot +
                    "_Variant_" +
                    variantIndex;
            }

            string displayName =
                ResolveToken(
                    skillDef.skillNameToken
                );

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName =
                    internalName;
            }

            string description =
                ResolveToken(
                    skillDef.skillDescriptionToken
                );

            ContentSourceRegistry.TryGetSource(
                skillDef,
                out ContentCatalogSourceInfo source
            );

            if (source == null)
            {
                ContentSourceRegistry.TryGetSource(
                    profile.SurvivorDef,
                    out source
                );
            }

            return new SurvivorContentSkillVariant
            {
                StableId =
                    "survivorSkill:" +
                    profile.BodyName +
                    ":" +
                    group.Slot +
                    ":" +
                    internalName,

                VariantIndex =
                    variantIndex,

                IsDefaultVariant =
                    isDefault,

                InternalName =
                    internalName,

                DisplayName =
                    displayName,

                Description =
                    description,

                NameToken =
                    skillDef.skillNameToken ?? "",

                SkillDef =
                    skillDef,

                Icon =
                    skillDef.icon,

                Source =
                    source,

                Discovery =
                    ResolveUnlockableDiscovery(
                        unlockable,
                        userProfile,
                        unlockedWhenNull: true
                    ),

                Unlock =
                    BuildUnlockInfo(
                        unlockable,
                        userProfile
                    ),

                HideIdentityWhenUndiscovered =
                    unlockable != null
            };
        }

        private static void BuildPassive(
            SurvivorContentProfile profile,
            SkillLocator locator
        )
        {
            if (
                profile == null ||
                locator == null
            )
            {
                return;
            }

            object passive =
                ReadMemberObject(
                    locator,
                    "passiveSkill"
                );

            if (passive == null)
            {
                return;
            }

            bool enabled =
                ContentCatalogReflection.ReadBool(
                    passive,
                    true,
                    "enabled"
                );

            if (!enabled)
            {
                return;
            }

            string nameToken =
                ContentCatalogReflection.ReadString(
                    passive,
                    "skillNameToken",
                    "nameToken"
                );

            string descriptionToken =
                ContentCatalogReflection.ReadString(
                    passive,
                    "skillDescriptionToken",
                    "descriptionToken"
                );

            string displayName =
                ResolveToken(
                    nameToken
                );

            string description =
                ResolveToken(
                    descriptionToken
                );

            Sprite icon =
                ContentCatalogReflection
                    .ReadUnityObject<Sprite>(
                        passive,
                        "icon"
                    );

            if (
                string.IsNullOrWhiteSpace(displayName) &&
                string.IsNullOrWhiteSpace(description) &&
                icon == null
            )
            {
                return;
            }

            SurvivorContentSkillGroup group =
                new SurvivorContentSkillGroup
                {
                    StableId =
                        "survivorSkillGroup:" +
                        profile.BodyName +
                        ":Passive",

                    Slot =
                        SurvivorContentSkillSlot.Passive,

                    SlotLabel =
                        "Passive"
                };

            group.Variants.Add(
                new SurvivorContentSkillVariant
                {
                    StableId =
                        "survivorSkill:" +
                        profile.BodyName +
                        ":Passive",

                    VariantIndex =
                        0,

                    IsDefaultVariant =
                        true,

                    InternalName =
                        string.IsNullOrWhiteSpace(nameToken)
                            ? "Passive"
                            : nameToken,

                    DisplayName =
                        string.IsNullOrWhiteSpace(displayName)
                            ? "Passive"
                            : displayName,

                    Description =
                        description,

                    NameToken =
                        nameToken ?? "",

                    Icon =
                        icon,

                    Discovery =
                        ContentCatalogDiscoveryState.Discovered,

                    Unlock =
                        BuildUnlockInfo(
                            null,
                            TryGetLocalProfile()
                        ),

                    HideIdentityWhenUndiscovered =
                        false
                }
            );

            profile.SkillGroups.Add(
                group
            );
        }

        private static SurvivorContentSkillSlot ResolveSkillSlot(
            GenericSkill skill,
            SkillLocator locator
        )
        {
            if (
                skill == null ||
                locator == null
            )
            {
                return SurvivorContentSkillSlot.Extra;
            }

            if (ReferenceEquals(skill, locator.primary))
            {
                return SurvivorContentSkillSlot.Primary;
            }

            if (ReferenceEquals(skill, locator.secondary))
            {
                return SurvivorContentSkillSlot.Secondary;
            }

            if (ReferenceEquals(skill, locator.utility))
            {
                return SurvivorContentSkillSlot.Utility;
            }

            if (ReferenceEquals(skill, locator.special))
            {
                return SurvivorContentSkillSlot.Special;
            }

            return SurvivorContentSkillSlot.Extra;
        }

        // =========================================================
        // SKINS CONTEXTUALES
        // =========================================================

        private static void BuildSkins(
            SurvivorContentProfile profile,
            GameObject bodyPrefab,
            CharacterBody body,
            UserProfile userProfile
        )
        {
            if (
                profile == null ||
                bodyPrefab == null ||
                body == null
            )
            {
                return;
            }

            List<SkinDef> skins =
                GetBodySkins(
                    bodyPrefab,
                    body.bodyIndex
                );

            HashSet<SkinDef> seen =
                new HashSet<SkinDef>();

            for (
                int i = 0;
                i < skins.Count;
                i++
            )
            {
                SkinDef skin =
                    skins[i];

                if (
                    skin == null ||
                    !seen.Add(skin)
                )
                {
                    continue;
                }

                string nameToken =
                    ContentCatalogReflection.ReadString(
                        skin,
                        "nameToken"
                    );

                string displayName =
                    ResolveToken(
                        nameToken
                    );

                string internalName =
                    !string.IsNullOrWhiteSpace(skin.name)
                        ? skin.name
                        : !string.IsNullOrWhiteSpace(nameToken)
                            ? nameToken
                            : "Skin_" + i;

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName =
                        internalName;
                }

                UnlockableDef unlockable =
                    ContentCatalogReflection
                        .ReadUnityObject<UnlockableDef>(
                            skin,
                            "unlockableDef"
                        );

                Sprite icon =
                    ContentCatalogReflection
                        .ReadUnityObject<Sprite>(
                            skin,
                            "icon"
                        );

                ContentSourceRegistry.TryGetSource(
                    skin,
                    out ContentCatalogSourceInfo source
                );

                if (source == null)
                {
                    ContentSourceRegistry.TryGetSource(
                        profile.SurvivorDef,
                        out source
                    );
                }

                profile.Skins.Add(
                    new SurvivorContentSkinEntry
                    {
                        StableId =
                            "survivorSkin:" +
                            profile.BodyName +
                            ":" +
                            internalName,

                        SkinIndex =
                            i,

                        InternalName =
                            internalName,

                        DisplayName =
                            displayName,

                        NameToken =
                            nameToken ?? "",

                        SkinDef =
                            skin,

                        Icon =
                            icon,

                        Source =
                            source,

                        Discovery =
                            ResolveUnlockableDiscovery(
                                unlockable,
                                userProfile,
                                unlockedWhenNull: true
                            ),

                        Unlock =
                            BuildUnlockInfo(
                                unlockable,
                                userProfile
                            ),

                        HideIdentityWhenUndiscovered =
                            unlockable != null
                    }
                );
            }
        }

        private static List<SkinDef> GetBodySkins(
            GameObject bodyPrefab,
            BodyIndex bodyIndex
        )
        {
            List<SkinDef> result =
                new List<SkinDef>();

            /*
             * Ruta 1: SkinCatalog. Se invoca por reflexión para tolerar
             * pequeños cambios de firma entre versiones de RoR2.
             */
            try
            {
                MethodInfo[] methods =
                    typeof(SkinCatalog).GetMethods(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static
                    );

                for (
                    int i = 0;
                    i < methods.Length;
                    i++
                )
                {
                    MethodInfo method =
                        methods[i];

                    if (
                        method == null ||
                        !string.Equals(
                            method.Name,
                            "GetBodySkins",
                            StringComparison.Ordinal
                        )
                    )
                    {
                        continue;
                    }

                    ParameterInfo[] parameters =
                        method.GetParameters();

                    if (
                        parameters.Length != 1 ||
                        parameters[0].ParameterType != typeof(BodyIndex)
                    )
                    {
                        continue;
                    }

                    object value =
                        method.Invoke(
                            null,
                            new object[]
                            {
                                bodyIndex
                            }
                        );

                    AddSkinEnumerable(
                        value,
                        result
                    );

                    if (result.Count > 0)
                    {
                        break;
                    }
                }
            }
            catch
            {
                // Fallback al ModelSkinController.
            }

            /*
             * Ruta 2: ModelSkinController del modelo. Ayuda con mods que
             * tienen su SkinDef preparada pero no se comportan exactamente
             * como el catálogo vanilla.
             */
            try
            {
                ModelLocator modelLocator =
                    bodyPrefab != null
                        ? bodyPrefab.GetComponent<ModelLocator>()
                        : null;

                if (
                    modelLocator != null &&
                    modelLocator.modelTransform != null
                )
                {
                    ModelSkinController controller =
                        modelLocator
                            .modelTransform
                            .GetComponent<ModelSkinController>();

                    if (controller != null)
                    {
                        object skinsValue =
                            ReadMemberObject(
                                controller,
                                "skins"
                            );

                        AddSkinEnumerable(
                            skinsValue,
                            result
                        );
                    }
                }
            }
            catch
            {
                // Un skin modded defectuoso no debe bloquear el perfil.
            }

            return result;
        }

        private static void AddSkinEnumerable(
            object value,
            List<SkinDef> destination
        )
        {
            if (
                value == null ||
                destination == null ||
                !(value is IEnumerable enumerable)
            )
            {
                return;
            }

            foreach (
                object element
                in enumerable
            )
            {
                if (element is SkinDef skin && skin != null)
                {
                    if (!destination.Contains(skin))
                    {
                        destination.Add(
                            skin
                        );
                    }
                }
            }
        }

        // =========================================================
        // UNLOCK / ACHIEVEMENT
        // =========================================================

        private static SurvivorContentUnlockInfo BuildUnlockInfo(
            UnlockableDef unlockable,
            UserProfile userProfile
        )
        {
            SurvivorContentUnlockInfo result =
                new SurvivorContentUnlockInfo
                {
                    UnlockableDef =
                        unlockable,

                    HasRequirement =
                        unlockable != null,

                    IsUnlocked =
                        unlockable == null ||
                        (
                            userProfile != null &&
                            userProfile.HasUnlockable(
                                unlockable
                            )
                        ),

                    UnlockableIdentifier =
                        unlockable != null
                            ? unlockable.cachedName ?? ""
                            : "",

                    Source =
                        ResolveUnlockSource(
                            unlockable
                        )
                };

            if (unlockable == null)
            {
                return result;
            }

            AchievementDef achievement =
                FindAchievementForUnlockable(
                    unlockable
                );

            if (achievement == null)
            {
                return result;
            }

            result.AchievementIdentifier =
                achievement.identifier ?? "";

            result.MissionName =
                ResolveToken(
                    achievement.nameToken
                );

            result.MissionDescription =
                ResolveToken(
                    achievement.descriptionToken
                );

            result.AchievementIcon =
                achievement.achievedIcon;

            return result;
        }

        private static AchievementDef FindAchievementForUnlockable(
            UnlockableDef unlockable
        )
        {
            if (
                unlockable == null ||
                string.IsNullOrWhiteSpace(
                    unlockable.cachedName
                )
            )
            {
                return null;
            }

            try
            {
                object definitions =
                    ReadStaticMemberObject(
                        typeof(AchievementManager),
                        "achievementDefs"
                    );

                if (!(definitions is IEnumerable enumerable))
                {
                    return null;
                }

                foreach (
                    object element
                    in enumerable
                )
                {
                    if (!(element is AchievementDef achievement))
                    {
                        continue;
                    }

                    if (
                        string.Equals(
                            achievement.unlockableRewardIdentifier,
                            unlockable.cachedName,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        return achievement;
                    }
                }
            }
            catch
            {
                // Algunos mods crean achievements fuera del flujo estándar.
            }

            return null;
        }

        private static SurvivorContentUnlockSource ResolveUnlockSource(
            UnlockableDef unlockable
        )
        {
            if (unlockable == null)
            {
                return
                    SurvivorContentUnlockSource.UnlockedByDefault;
            }

            if (
                SurvivorUnlockManager.IsCustomUnlock(
                    unlockable
                )
            )
            {
                return
                    SurvivorContentUnlockSource.UsuManaged;
            }

            return
                SurvivorContentUnlockSource.Original;
        }

        private static ContentCatalogDiscoveryState ResolveUnlockableDiscovery(
            UnlockableDef unlockable,
            UserProfile userProfile,
            bool unlockedWhenNull
        )
        {
            if (unlockable == null)
            {
                return unlockedWhenNull
                    ? ContentCatalogDiscoveryState.Discovered
                    : ContentCatalogDiscoveryState.NotApplicable;
            }

            if (userProfile == null)
            {
                return
                    ContentCatalogDiscoveryState.Unknown;
            }

            try
            {
                return userProfile.HasUnlockable(
                    unlockable
                )
                    ? ContentCatalogDiscoveryState.Discovered
                    : ContentCatalogDiscoveryState.Undiscovered;
            }
            catch
            {
                return
                    ContentCatalogDiscoveryState.Unknown;
            }
        }

        // =========================================================
        // TEXTOS / LORE
        // =========================================================

        private static string ResolveLore(
            SurvivorDef survivor,
            CharacterBody body
        )
        {
            List<string> candidates =
                new List<string>();

            string survivorNameToken =
                ContentCatalogReflection.ReadString(
                    survivor,
                    "displayNameToken"
                );

            string bodyNameToken =
                ContentCatalogReflection.ReadString(
                    body,
                    "baseNameToken"
                );

            AddLoreCandidate(
                candidates,
                survivorNameToken
            );

            AddLoreCandidate(
                candidates,
                bodyNameToken
            );

            for (
                int i = 0;
                i < candidates.Count;
                i++
            )
            {
                string resolved =
                    ResolveToken(
                        candidates[i]
                    );

                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    return resolved;
                }
            }

            return "";
        }

        private static void AddLoreCandidate(
            List<string> candidates,
            string nameToken
        )
        {
            if (
                candidates == null ||
                string.IsNullOrWhiteSpace(nameToken)
            )
            {
                return;
            }

            string token =
                nameToken.Trim();

            if (
                token.EndsWith(
                    "_NAME",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                candidates.Add(
                    token.Substring(
                        0,
                        token.Length - "_NAME".Length
                    ) +
                    "_LORE"
                );
            }

            if (
                token.EndsWith(
                    "_DISPLAYNAME",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                candidates.Add(
                    token.Substring(
                        0,
                        token.Length - "_DISPLAYNAME".Length
                    ) +
                    "_LORE"
                );
            }
        }

        private static string ResolveLocalizedToken(
            object source,
            params string[] memberNames
        )
        {
            string token =
                ContentCatalogReflection.ReadString(
                    source,
                    memberNames
                );

            return ResolveToken(
                token
            );
        }

        private static string ResolveToken(
            string token
        )
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return "";
            }

            try
            {
                string localized =
                    Language.GetString(
                        token
                    );

                if (
                    string.IsNullOrWhiteSpace(localized) ||
                    string.Equals(
                        localized,
                        token,
                        StringComparison.Ordinal
                    )
                )
                {
                    return "";
                }

                return localized;
            }
            catch
            {
                return "";
            }
        }

        private static string ResolveSourceIdentifier(
            SurvivorInfo survivorInfo,
            SurvivorDef survivorDef
        )
        {
            if (
                survivorInfo != null &&
                !string.IsNullOrWhiteSpace(
                    survivorInfo.ContentPackIdentifier
                )
            )
            {
                return
                    survivorInfo.ContentPackIdentifier;
            }

            if (
                survivorDef != null &&
                ContentSourceRegistry.TryGetSource(
                    survivorDef,
                    out ContentCatalogSourceInfo source
                ) &&
                source != null
            )
            {
                return
                    source.ContentPackIdentifier ?? "";
            }

            return
                survivorInfo?.ExpansionName ?? "";
        }

        // =========================================================
        // REFLECTION HELPERS
        // =========================================================

        private static object ReadMemberObject(
            object target,
            params string[] memberNames
        )
        {
            if (
                target == null ||
                memberNames == null
            )
            {
                return null;
            }

            Type type =
                target.GetType();

            for (
                int i = 0;
                i < memberNames.Length;
                i++
            )
            {
                string memberName =
                    memberNames[i];

                if (string.IsNullOrWhiteSpace(memberName))
                {
                    continue;
                }

                try
                {
                    PropertyInfo property =
                        type.GetProperty(
                            memberName,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance
                        );

                    if (
                        property != null &&
                        property.CanRead
                    )
                    {
                        return property.GetValue(
                            target,
                            null
                        );
                    }

                    FieldInfo field =
                        type.GetField(
                            memberName,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance
                        );

                    if (field != null)
                    {
                        return field.GetValue(
                            target
                        );
                    }
                }
                catch
                {
                    // Probar siguiente alias.
                }
            }

            return null;
        }

        private static object ReadStaticMemberObject(
            Type type,
            params string[] memberNames
        )
        {
            if (
                type == null ||
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
                string memberName =
                    memberNames[i];

                try
                {
                    PropertyInfo property =
                        type.GetProperty(
                            memberName,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Static
                        );

                    if (
                        property != null &&
                        property.CanRead
                    )
                    {
                        return property.GetValue(
                            null,
                            null
                        );
                    }

                    FieldInfo field =
                        type.GetField(
                            memberName,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Static
                        );

                    if (field != null)
                    {
                        return field.GetValue(
                            null
                        );
                    }
                }
                catch
                {
                    // Probar siguiente alias.
                }
            }

            return null;
        }

        private static Array ReadArrayMember(
            object target,
            params string[] memberNames
        )
        {
            object value =
                ReadMemberObject(
                    target,
                    memberNames
                );

            return value as Array;
        }

        private static int ReadIntegerMember(
            object target,
            int fallback,
            params string[] memberNames
        )
        {
            object value =
                ReadMemberObject(
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
                    value
                );
            }
            catch
            {
                return fallback;
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static UserProfile TryGetLocalProfile()
        {
            try
            {
                if (
                    LocalUserManager.readOnlyLocalUsersList != null &&
                    LocalUserManager.readOnlyLocalUsersList.Count > 0
                )
                {
                    LocalUser localUser =
                        LocalUserManager.readOnlyLocalUsersList[0];

                    if (localUser != null)
                    {
                        return
                            localUser.userProfile;
                    }
                }
            }
            catch
            {
                // Se podrá refrescar al abrir el HUD.
            }

            return null;
        }

        private static string BuildGroupStableId(
            string bodyName,
            SurvivorContentSkillSlot slot,
            int extraIndex
        )
        {
            return
                "survivorSkillGroup:" +
                bodyName +
                ":" +
                slot +
                (
                    slot == SurvivorContentSkillSlot.Extra
                        ? ":" + extraIndex
                        : ""
                );
        }

        private static string ResolveSlotLabel(
            SurvivorContentSkillSlot slot,
            int extraIndex
        )
        {
            switch (slot)
            {
                case SurvivorContentSkillSlot.Passive:
                    return "Passive";

                case SurvivorContentSkillSlot.Primary:
                    return "Primary";

                case SurvivorContentSkillSlot.Secondary:
                    return "Secondary";

                case SurvivorContentSkillSlot.Utility:
                    return "Utility";

                case SurvivorContentSkillSlot.Special:
                    return "Special";

                default:
                    return
                        "Extra " +
                        extraIndex;
            }
        }

        private static int CountSkillVariants(
            SurvivorContentProfile profile
        )
        {
            if (profile == null)
            {
                return 0;
            }

            int result =
                0;

            for (
                int i = 0;
                i < profile.SkillGroups.Count;
                i++
            )
            {
                SurvivorContentSkillGroup group =
                    profile.SkillGroups[i];

                if (group != null)
                {
                    result +=
                        group.Variants.Count;
                }
            }

            return result;
        }

        private static void LogSummary()
        {
            int groups =
                0;

            int skillVariants =
                0;

            int lockedSkills =
                0;

            int skins =
                0;

            int lockedSkins =
                0;

            int loreResolved =
                0;

            int originalUnlocks =
                0;

            int usuUnlocks =
                0;

            int unlockedByDefault =
                0;

            for (
                int i = 0;
                i < Profiles.Count;
                i++
            )
            {
                SurvivorContentProfile profile =
                    Profiles[i];

                if (profile == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(profile.Lore))
                {
                    loreResolved++;
                }

                if (profile.SurvivorUnlock != null)
                {
                    switch (profile.SurvivorUnlock.Source)
                    {
                        case SurvivorContentUnlockSource.Original:
                            originalUnlocks++;
                            break;

                        case SurvivorContentUnlockSource.UsuManaged:
                            usuUnlocks++;
                            break;

                        case SurvivorContentUnlockSource.UnlockedByDefault:
                            unlockedByDefault++;
                            break;
                    }
                }

                groups +=
                    profile.SkillGroups.Count;

                for (
                    int g = 0;
                    g < profile.SkillGroups.Count;
                    g++
                )
                {
                    SurvivorContentSkillGroup group =
                        profile.SkillGroups[g];

                    if (group == null)
                    {
                        continue;
                    }

                    skillVariants +=
                        group.Variants.Count;

                    for (
                        int v = 0;
                        v < group.Variants.Count;
                        v++
                    )
                    {
                        SurvivorContentSkillVariant variant =
                            group.Variants[v];

                        if (
                            variant != null &&
                            variant.Discovery ==
                            ContentCatalogDiscoveryState.Undiscovered
                        )
                        {
                            lockedSkills++;
                        }
                    }
                }

                skins +=
                    profile.Skins.Count;

                for (
                    int s = 0;
                    s < profile.Skins.Count;
                    s++
                )
                {
                    SurvivorContentSkinEntry skin =
                        profile.Skins[s];

                    if (
                        skin != null &&
                        skin.Discovery ==
                        ContentCatalogDiscoveryState.Undiscovered
                    )
                    {
                        lockedSkins++;
                    }
                }
            }

            logger?.LogInfo(
                "[SURVIVOR PROFILE] 5G.2D-C construido | " +
                "Profiles: " +
                Profiles.Count +
                " | SkillGroups: " +
                groups +
                " | SkillVariants: " +
                skillVariants +
                " | Skills bloqueadas: " +
                lockedSkills +
                " | Skins: " +
                skins +
                " | Skins bloqueadas: " +
                lockedSkins +
                " | Lore resuelto: " +
                loreResolved +
                " | Survivor unlock Original: " +
                originalUnlocks +
                " | USU-managed: " +
                usuUnlocks +
                " | Libres: " +
                unlockedByDefault
            );
        }
    }
}
