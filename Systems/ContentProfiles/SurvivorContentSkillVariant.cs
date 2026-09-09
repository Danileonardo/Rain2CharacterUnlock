using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    public sealed class SurvivorContentSkillVariant
    {
        public string StableId
        {
            get;
            set;
        } = "";

        public int VariantIndex
        {
            get;
            set;
        }

        public bool IsDefaultVariant
        {
            get;
            set;
        }

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

        public string Description
        {
            get;
            set;
        } = "";

        public string NameToken
        {
            get;
            set;
        } = "";

        public SkillDef SkillDef
        {
            get;
            set;
        }

        public Sprite Icon
        {
            get;
            set;
        }

        public ContentCatalogSourceInfo Source
        {
            get;
            set;
        }

        public ContentCatalogDiscoveryState Discovery
        {
            get;
            set;
        } = ContentCatalogDiscoveryState.Unknown;

        public SurvivorContentUnlockInfo Unlock
        {
            get;
            set;
        }

        public bool HideIdentityWhenUndiscovered
        {
            get;
            set;
        }

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
