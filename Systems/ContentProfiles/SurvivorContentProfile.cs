using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Ficha contextual completa que consumirá 5G.2E.
    ///
    /// El jugador entra SIEMPRE por el survivor; Skills/Skins no son
    /// catálogos de configuración separados.
    /// </summary>
    public sealed class SurvivorContentProfile
    {
        public SurvivorInfo SurvivorInfo
        {
            get;
            set;
        }

        public SurvivorDef SurvivorDef
        {
            get;
            set;
        }

        public string BodyName
        {
            get;
            set;
        } = "";

        public string InternalName
        {
            get;
            set;
        } = "";

        public string DisplayName
        {
            get;
            set;
        } = "";

        public string Subtitle
        {
            get;
            set;
        } = "";

        public string Description
        {
            get;
            set;
        } = "";

        public string Lore
        {
            get;
            set;
        } = "";

        public string SourceIdentifier
        {
            get;
            set;
        } = "";

        public string SourceAssembly
        {
            get;
            set;
        } = "";

        public Texture Portrait
        {
            get;
            set;
        }

        public GameObject BodyPrefab
        {
            get;
            set;
        }

        public ContentCatalogDiscoveryState Discovery
        {
            get;
            set;
        } = ContentCatalogDiscoveryState.Unknown;

        public bool HideIdentityWhenUndiscovered
        {
            get;
            set;
        }

        public SurvivorContentUnlockInfo SurvivorUnlock
        {
            get;
            set;
        }

        public List<SurvivorContentSkillGroup> SkillGroups
        {
            get;
        } = new List<SurvivorContentSkillGroup>();

        public List<SurvivorContentSkinEntry> Skins
        {
            get;
        } = new List<SurvivorContentSkinEntry>();

        public bool ShouldHideIdentity
        {
            get
            {
                return
                    HideIdentityWhenUndiscovered &&
                    Discovery ==
                    ContentCatalogDiscoveryState.Undiscovered;
            }
        }
    }
}
