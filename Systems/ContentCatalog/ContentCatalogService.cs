using System;
using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// 5G.2D-A - Runtime Content Catalog.
    ///
    /// Construye una vista de sólo lectura del contenido instalado que luego
    /// será consumida por el HUD visual y el Mission Editor.
    ///
    /// Esta fase NO cambia unlocks, providers ni progreso.
    /// </summary>
    public static class ContentCatalogService
    {
        private static readonly List<ContentCatalogEntry>
            Entries =
                new List<ContentCatalogEntry>();

        private static readonly Dictionary<
            ContentCatalogKind,
            List<ContentCatalogEntry>
        > EntriesByKind =
            new Dictionary<
                ContentCatalogKind,
                List<ContentCatalogEntry>
            >();

        private static readonly HashSet<string>
            StableIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static ManualLogSource logger;
        private static bool initialized;

        public static int Count =>
            Entries.Count;

        public static void Initialize(
            ManualLogSource activeLogger
        )
        {
            logger =
                activeLogger;

            if (initialized)
            {
                Rebuild();
                return;
            }

            initialized =
                true;

            Rebuild();

            Language.onCurrentLanguageChanged +=
                OnLanguageChanged;
        }

        private static void OnLanguageChanged()
        {
            // Nombres/descripciones del catálogo respetan el idioma actual.
            Rebuild();
        }

        public static void Rebuild()
        {
            Entries.Clear();
            EntriesByKind.Clear();
            StableIds.Clear();

            ScanSurvivors();
            ScanItems();
            ScanEquipment();
            ScanSkills();
            ScanBodies();
            ScanStages();

            /*
             * 5G.2D-B
             *
             * El catálogo técnico conoce todo el contenido instalado.
             * Discovery decide qué identidad puede revelar el HUD al perfil
             * local, sin cambiar jamás el runtime de una misión.
             */
            ContentCatalogDiscoveryService.Refresh(
                Entries,
                logger
            );

            SortAll();
            LogSummary();
        }

        public static IReadOnlyList<ContentCatalogEntry> GetAll()
        {
            return Entries;
        }

        public static void RefreshDiscovery()
        {
            ContentCatalogDiscoveryService.Refresh(
                Entries,
                logger
            );
        }

        public static IReadOnlyList<ContentCatalogEntry> GetEntries(
            ContentCatalogKind kind
        )
        {
            if (
                EntriesByKind.TryGetValue(
                    kind,
                    out List<ContentCatalogEntry> list
                )
            )
            {
                return list;
            }

            return Array.Empty<ContentCatalogEntry>();
        }

        public static bool TryGet(
            string stableId,
            out ContentCatalogEntry entry
        )
        {
            entry =
                null;

            if (string.IsNullOrWhiteSpace(stableId))
            {
                return false;
            }

            for (
                int i = 0;
                i < Entries.Count;
                i++
            )
            {
                ContentCatalogEntry candidate =
                    Entries[i];

                if (
                    candidate != null &&
                    string.Equals(
                        candidate.StableId,
                        stableId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    entry =
                        candidate;

                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // SURVIVORS
        // =========================================================

        private static void ScanSurvivors()
        {
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

                if (survivor == null)
                {
                    continue;
                }

                string bodyName =
                    survivor.bodyPrefab != null
                        ? survivor.bodyPrefab.name
                        : survivor.cachedName;

                ContentCatalogEntry entry =
                    CreateBaseEntry(
                        ContentCatalogKind.Survivor,
                        survivor,
                        "survivor:" + bodyName,
                        survivor.cachedName,
                        new string[]
                        {
                            "displayNameToken"
                        },
                        new string[]
                        {
                            "descriptionToken"
                        }
                    );

                entry.Hidden =
                    survivor.hidden;

                entry.Selectable =
                    !survivor.hidden &&
                    survivor.bodyPrefab != null;

                entry.ModelPrefab =
                    survivor.bodyPrefab;

                entry.IconTexture =
                    ContentCatalogReflection
                        .ReadUnityObject<Texture>(
                            survivor,
                            "portraitIcon"
                        );

                entry.DiscoveryUnlockable =
                    survivor.unlockableDef;

                entry.HideIdentityWhenUndiscovered =
                    survivor.unlockableDef != null;

                Add(entry);
            }
        }

        // =========================================================
        // ITEMS
        // =========================================================

        private static void ScanItems()
        {
            ItemDef[] items =
                Resources.FindObjectsOfTypeAll<ItemDef>();

            if (items == null)
            {
                return;
            }

            for (
                int i = 0;
                i < items.Length;
                i++
            )
            {
                ItemDef item =
                    items[i];

                if (item == null)
                {
                    continue;
                }

                string internalName =
                    string.IsNullOrWhiteSpace(item.name)
                        ? "UnknownItem"
                        : item.name;

                ContentCatalogEntry entry =
                    CreateBaseEntry(
                        ContentCatalogKind.Item,
                        item,
                        "item:" + internalName,
                        internalName,
                        new string[]
                        {
                            "nameToken"
                        },
                        new string[]
                        {
                            "descriptionToken",
                            "pickupToken"
                        }
                    );

                entry.IconSprite =
                    ContentCatalogReflection
                        .ReadUnityObject<Sprite>(
                            item,
                            "pickupIconSprite"
                        );

                entry.ModelPrefab =
                    ContentCatalogReflection
                        .ReadUnityObject<GameObject>(
                            item,
                            "pickupModelPrefab"
                        );

                entry.DiscoveryUnlockable =
                    item.unlockableDef;

                entry.HideIdentityWhenUndiscovered =
                    true;

                Add(entry);
            }
        }

        // =========================================================
        // EQUIPMENT
        // =========================================================

        private static void ScanEquipment()
        {
            EquipmentDef[] equipmentDefs =
                Resources.FindObjectsOfTypeAll<EquipmentDef>();

            if (equipmentDefs == null)
            {
                return;
            }

            for (
                int i = 0;
                i < equipmentDefs.Length;
                i++
            )
            {
                EquipmentDef equipment =
                    equipmentDefs[i];

                if (equipment == null)
                {
                    continue;
                }

                string internalName =
                    string.IsNullOrWhiteSpace(equipment.name)
                        ? "UnknownEquipment"
                        : equipment.name;

                ContentCatalogEntry entry =
                    CreateBaseEntry(
                        ContentCatalogKind.Equipment,
                        equipment,
                        "equipment:" + internalName,
                        internalName,
                        new string[]
                        {
                            "nameToken"
                        },
                        new string[]
                        {
                            "descriptionToken",
                            "pickupToken"
                        }
                    );

                entry.IconSprite =
                    ContentCatalogReflection
                        .ReadUnityObject<Sprite>(
                            equipment,
                            "pickupIconSprite"
                        );

                entry.ModelPrefab =
                    ContentCatalogReflection
                        .ReadUnityObject<GameObject>(
                            equipment,
                            "pickupModelPrefab"
                        );

                entry.DiscoveryUnlockable =
                    equipment.unlockableDef;

                entry.HideIdentityWhenUndiscovered =
                    true;

                Add(entry);
            }
        }

        // =========================================================
        // SKILLS
        // =========================================================

        private static void ScanSkills()
        {
            SkillDef[] skills =
                Resources.FindObjectsOfTypeAll<SkillDef>();

            if (skills == null)
            {
                return;
            }

            for (
                int i = 0;
                i < skills.Length;
                i++
            )
            {
                SkillDef skill =
                    skills[i];

                if (skill == null)
                {
                    continue;
                }

                string internalName =
                    SkillCatalog.GetSkillName(
                        skill.skillIndex
                    );

                if (string.IsNullOrWhiteSpace(internalName))
                {
                    internalName =
                        skill.skillName;
                }

                if (string.IsNullOrWhiteSpace(internalName))
                {
                    internalName =
                        skill.skillNameToken;
                }

                if (string.IsNullOrWhiteSpace(internalName))
                {
                    // Sin un identificador persistible no lo exponemos aún.
                    // 5G.2D-B podrá asociarlo a su SkillFamily/variant.
                    continue;
                }

                ContentCatalogEntry entry =
                    CreateBaseEntry(
                        ContentCatalogKind.Skill,
                        skill,
                        "skill:" + internalName,
                        internalName,
                        new string[]
                        {
                            "skillNameToken"
                        },
                        new string[]
                        {
                            "skillDescriptionToken"
                        }
                    );

                entry.IconSprite =
                    ContentCatalogReflection
                        .ReadUnityObject<Sprite>(
                            skill,
                            "icon"
                        );

                Add(entry);
            }
        }

        // =========================================================
        // BODIES / ENEMIES / BOSSES
        // =========================================================

        private static void ScanBodies()
        {
            GameObject[] bodyPrefabs =
                BodyCatalog.bodyPrefabs;

            if (bodyPrefabs == null)
            {
                return;
            }

            HashSet<GameObject> survivorBodies =
                new HashSet<GameObject>();

            SurvivorDef[] survivorDefs =
                SurvivorCatalog.survivorDefs;

            if (survivorDefs != null)
            {
                for (
                    int i = 0;
                    i < survivorDefs.Length;
                    i++
                )
                {
                    SurvivorDef survivor =
                        survivorDefs[i];

                    if (
                        survivor != null &&
                        survivor.bodyPrefab != null
                    )
                    {
                        survivorBodies.Add(
                            survivor.bodyPrefab
                        );
                    }
                }
            }

            for (
                int i = 0;
                i < bodyPrefabs.Length;
                i++
            )
            {
                GameObject bodyPrefab =
                    bodyPrefabs[i];

                if (
                    bodyPrefab == null ||
                    survivorBodies.Contains(bodyPrefab)
                )
                {
                    continue;
                }

                CharacterBody body =
                    bodyPrefab.GetComponent<CharacterBody>();

                if (body == null)
                {
                    continue;
                }

                /*
                 * RoR2 trata a los jefes clásicos como Champions.
                 *
                 * "isBoss" no estaba resolviendo los cuerpos del catálogo
                 * actual (Bosses: 0), por eso 5G.2D-B usa directamente
                 * CharacterBody.isChampion como señal estática.
                 */
                bool isChampion =
                    body.isChampion;

                ContentCatalogKind kind =
                    isChampion
                        ? ContentCatalogKind.Boss
                        : ContentCatalogKind.Enemy;

                string internalName =
                    bodyPrefab.name;

                DeathRewards deathRewards =
                    bodyPrefab.GetComponent<DeathRewards>();

                UnlockableDef logUnlockable =
                    ContentCatalogReflection
                        .ReadUnityObject<UnlockableDef>(
                            deathRewards,
                            "logUnlockableDef"
                        );

                ContentCatalogBodyCategory bodyCategory =
                    ClassifyBody(
                        bodyPrefab,
                        deathRewards,
                        isChampion
                    );

                ContentCatalogEntry entry =
                    CreateBaseEntry(
                        kind,
                        bodyPrefab,
                        (isChampion ? "boss:" : "enemy:") +
                        internalName,
                        internalName,
                        new string[]
                        {
                            "baseNameToken"
                        },
                        Array.Empty<string>(),
                        tokenSource: body
                    );

                entry.ModelPrefab =
                    bodyPrefab;

                entry.IconTexture =
                    ContentCatalogReflection
                        .ReadUnityObject<Texture>(
                            body,
                            "portraitIcon"
                        );

                entry.BodyCategory =
                    bodyCategory;

                entry.IsChampion =
                    isChampion;

                entry.DiscoveryUnlockable =
                    logUnlockable;

                entry.HasLogBookUnlock =
                    logUnlockable != null;

                /*
                 * Si existe entrada de LogBook, el navegador podrá ocultar
                 * identidad/arte hasta que el perfil la haya descubierto.
                 * La misión de desbloqueo nunca se oculta.
                 */
                entry.HideIdentityWhenUndiscovered =
                    logUnlockable != null;

                if (
                    bodyCategory ==
                    ContentCatalogBodyCategory.Internal
                )
                {
                    entry.Hidden =
                        true;

                    entry.Selectable =
                        false;
                }

                Add(entry);
            }
        }

        private static ContentCatalogBodyCategory ClassifyBody(
            GameObject bodyPrefab,
            DeathRewards deathRewards,
            bool isChampion
        )
        {
            if (bodyPrefab == null)
            {
                return
                    ContentCatalogBodyCategory.Internal;
            }

            if (isChampion)
            {
                return
                    ContentCatalogBodyCategory.Champion;
            }

            string name =
                bodyPrefab.name ?? "";

            string lowered =
                name.ToLowerInvariant();

            /*
             * Sólo ocultamos casos inequívocamente técnicos.
             * Sombras/Umbra, summons, decoys y otros cuerpos jugables
             * permanecen disponibles como posibles targets.
             */
            if (
                lowered.Contains("dummy") ||
                lowered.Contains("testbody") ||
                lowered.Contains("placeholder")
            )
            {
                return
                    ContentCatalogBodyCategory.Internal;
            }

            if (lowered.Contains("decoy"))
            {
                return
                    ContentCatalogBodyCategory.Decoy;
            }

            /*
             * DroneCatalog es la señal primaria. El nombre queda como
             * fallback para drones modded que todavía no se hayan
             * registrado en el catálogo nativo.
             */
            bool registeredDrone =
                false;

            try
            {
                CharacterBody body =
                    bodyPrefab.GetComponent<CharacterBody>();

                if (body != null)
                {
                    registeredDrone =
                        (int)DroneCatalog
                            .GetDroneIndexFromBodyIndex(
                                body.bodyIndex
                            ) >= 0;
                }
            }
            catch
            {
                registeredDrone =
                    false;
            }

            if (
                registeredDrone ||
                lowered.Contains("drone")
            )
            {
                return
                    ContentCatalogBodyCategory.Drone;
            }

            if (
                lowered.Contains("turret") ||
                lowered.Contains("minion")
            )
            {
                return
                    ContentCatalogBodyCategory.Summon;
            }

            /*
             * DeathRewards es una señal fuerte de que el cuerpo participa
             * como objetivo de combate normal del juego.
             */
            if (deathRewards != null)
            {
                return
                    ContentCatalogBodyCategory.Enemy;
            }

            return
                ContentCatalogBodyCategory.Special;
        }

        // =========================================================
        // STAGES
        // =========================================================

        private static void ScanStages()
        {
            SceneDef[] scenes =
                Resources.FindObjectsOfTypeAll<SceneDef>();

            if (scenes == null)
            {
                return;
            }

            for (
                int i = 0;
                i < scenes.Length;
                i++
            )
            {
                SceneDef scene =
                    scenes[i];

                if (scene == null)
                {
                    continue;
                }

                string internalName =
                    string.IsNullOrWhiteSpace(scene.cachedName)
                        ? "UnknownScene"
                        : scene.cachedName;

                ContentCatalogEntry entry =
                    CreateBaseEntry(
                        ContentCatalogKind.Stage,
                        scene,
                        "stage:" + internalName,
                        internalName,
                        new string[]
                        {
                            "nameToken"
                        },
                        new string[]
                        {
                            "subtitleToken"
                        }
                    );

                entry.IconTexture =
                    ContentCatalogReflection
                        .ReadUnityObject<Texture>(
                            scene,
                            "previewTexture"
                        );

                entry.DiscoveryUnlockable =
                    ContentCatalogReflection
                        .ReadUnityObject<UnlockableDef>(
                            scene,
                            "unlockableDef",
                            "logUnlockableDef"
                        );

                entry.HasLogBookUnlock =
                    entry.DiscoveryUnlockable != null;

                entry.HideIdentityWhenUndiscovered =
                    entry.DiscoveryUnlockable != null;

                Add(entry);
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static ContentCatalogEntry CreateBaseEntry(
            ContentCatalogKind kind,
            UnityEngine.Object asset,
            string stableId,
            string internalName,
            string[] nameTokenMembers,
            string[] descriptionTokenMembers,
            object tokenSource = null
        )
        {
            object localizationSource =
                tokenSource ?? asset;

            string nameToken =
                ContentCatalogReflection
                    .ReadString(
                        localizationSource,
                        nameTokenMembers
                    );

            string displayName =
                ResolveLocalizedText(
                    localizationSource,
                    nameTokenMembers
                );

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName =
                    internalName;
            }

            string description =
                ResolveLocalizedText(
                    localizationSource,
                    descriptionTokenMembers
                );

            ContentSourceRegistry.TryGetSource(
                asset,
                out ContentCatalogSourceInfo source
            );

            return new ContentCatalogEntry
            {
                Kind =
                    kind,

                StableId =
                    stableId,

                InternalName =
                    internalName ?? "",

                DisplayName =
                    displayName ?? "",

                Description =
                    description ?? "",

                NameToken =
                    nameToken ?? "",

                Source =
                    source,

                Discovery =
                    ContentCatalogDiscoveryState.Unknown,

                Asset =
                    asset
            };
        }

        private static string ResolveLocalizedText(
            object source,
            params string[] tokenMembers
        )
        {
            if (
                source == null ||
                tokenMembers == null ||
                tokenMembers.Length == 0
            )
            {
                return "";
            }

            string token =
                ContentCatalogReflection
                    .ReadString(
                        source,
                        tokenMembers
                    );

            if (string.IsNullOrWhiteSpace(token))
            {
                return "";
            }

            string localized =
                Language.GetString(token);

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

        private static void Add(
            ContentCatalogEntry entry
        )
        {
            if (
                entry == null ||
                string.IsNullOrWhiteSpace(entry.StableId) ||
                !StableIds.Add(entry.StableId)
            )
            {
                return;
            }

            Entries.Add(entry);

            if (
                !EntriesByKind.TryGetValue(
                    entry.Kind,
                    out List<ContentCatalogEntry> list
                )
            )
            {
                list =
                    new List<ContentCatalogEntry>();

                EntriesByKind.Add(
                    entry.Kind,
                    list
                );
            }

            list.Add(entry);
        }

        private static void SortAll()
        {
            Comparison<ContentCatalogEntry> comparison =
                delegate(
                    ContentCatalogEntry a,
                    ContentCatalogEntry b
                )
                {
                    return string.Compare(
                        a?.DisplayName,
                        b?.DisplayName,
                        StringComparison.CurrentCultureIgnoreCase
                    );
                };

            Entries.Sort(comparison);

            foreach (
                KeyValuePair<
                    ContentCatalogKind,
                    List<ContentCatalogEntry>
                > pair
                in EntriesByKind
            )
            {
                pair.Value.Sort(comparison);
            }
        }

        private static void LogSummary()
        {
            int survivors =
                GetCount(ContentCatalogKind.Survivor);

            int skills =
                GetCount(ContentCatalogKind.Skill);

            int items =
                GetCount(ContentCatalogKind.Item);

            int equipment =
                GetCount(ContentCatalogKind.Equipment);

            int enemies =
                GetCount(ContentCatalogKind.Enemy);

            int bosses =
                GetCount(ContentCatalogKind.Boss);

            int stages =
                GetCount(ContentCatalogKind.Stage);

            int internalBodies =
                GetBodyCategoryCount(
                    ContentCatalogBodyCategory.Internal
                );

            int summons =
                GetBodyCategoryCount(
                    ContentCatalogBodyCategory.Summon
                );

            int drones =
                GetBodyCategoryCount(
                    ContentCatalogBodyCategory.Drone
                );

            int decoys =
                GetBodyCategoryCount(
                    ContentCatalogBodyCategory.Decoy
                );

            int discovered =
                GetDiscoveryCount(
                    ContentCatalogDiscoveryState.Discovered
                );

            int undiscovered =
                GetDiscoveryCount(
                    ContentCatalogDiscoveryState.Undiscovered
                );

            int discoveryUnknown =
                GetDiscoveryCount(
                    ContentCatalogDiscoveryState.Unknown
                );

            logger?.LogInfo(
                "[CONTENT CATALOG] Catálogo 5G.2D-B construido | " +
                "Total: " +
                Entries.Count +
                " | Survivors: " +
                survivors +
                " | Skills(raw): " +
                skills +
                " | Items: " +
                items +
                " | Equipment: " +
                equipment +
                " | Enemies: " +
                enemies +
                " | Bosses/Champions: " +
                bosses +
                " | Stages: " +
                stages +
                " | Summons: " +
                summons +
                " | Drones: " +
                drones +
                " | Decoys: " +
                decoys +
                " | Internal ocultos: " +
                internalBodies +
                " | Descubiertos: " +
                discovered +
                " | No descubiertos: " +
                undiscovered +
                " | Discovery desconocido: " +
                discoveryUnknown +
                " | Assets con procedencia: " +
                ContentSourceRegistry.Count
            );
        }

        private static int GetBodyCategoryCount(
            ContentCatalogBodyCategory category
        )
        {
            int count =
                0;

            for (
                int i = 0;
                i < Entries.Count;
                i++
            )
            {
                ContentCatalogEntry entry =
                    Entries[i];

                if (
                    entry != null &&
                    entry.BodyCategory == category
                )
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetDiscoveryCount(
            ContentCatalogDiscoveryState state
        )
        {
            int count =
                0;

            for (
                int i = 0;
                i < Entries.Count;
                i++
            )
            {
                ContentCatalogEntry entry =
                    Entries[i];

                if (
                    entry != null &&
                    entry.Discovery == state
                )
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetCount(
            ContentCatalogKind kind
        )
        {
            if (
                EntriesByKind.TryGetValue(
                    kind,
                    out List<ContentCatalogEntry> list
                )
            )
            {
                return list.Count;
            }

            return 0;
        }
    }
}
