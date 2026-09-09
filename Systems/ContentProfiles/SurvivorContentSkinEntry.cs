using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    public sealed class SurvivorContentSkinEntry
    {
        public string StableId
        {
            get;
            set;
        } = "";

        public int SkinIndex
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

        public string NameToken
        {
            get;
            set;
        } = "";

        public SkinDef SkinDef
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
