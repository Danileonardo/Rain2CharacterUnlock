using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Registro runtime común para cualquier asset que pueda ser elegido
    /// visualmente por el futuro Mission Editor.
    ///
    /// Asset/Icon/Model son referencias runtime; no se serializan en JSON.
    /// StableId es el valor que más adelante se persistirá en una condición.
    /// </summary>
    public sealed class ContentCatalogEntry
    {
        public ContentCatalogKind Kind
        {
            get;
            set;
        }

        public string StableId
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

        /*
         * 5G.2D-B
         *
         * BodyCategory sólo se usa en entradas Enemy/Boss.
         * No reemplaza Kind; sirve como clasificación adicional para el HUD.
         */
        public ContentCatalogBodyCategory BodyCategory
        {
            get;
            set;
        } = ContentCatalogBodyCategory.NotApplicable;

        /*
         * "Champion" es el dato estático de RoR2 que mejor representa
         * cuerpos pensados como jefes/champions. NO significa que una
         * instancia concreta haya sido generada como jefe del teleporter.
         * Ese contexto es runtime y lo resuelve ContentRuntimeContextTracker.
         */
        public bool IsChampion
        {
            get;
            set;
        }

        /*
         * Unlock usado para resolver discovery/logbook cuando existe.
         * Es una referencia runtime, nunca se persiste directamente.
         */
        public UnlockableDef DiscoveryUnlockable
        {
            get;
            set;
        }

        public bool HasLogBookUnlock
        {
            get;
            set;
        }

        /*
         * Regla visual acordada:
         * - si el contenido no está descubierto, su identidad puede ocultarse;
         * - la misión/requisito para desbloquearlo SIEMPRE puede mostrarse.
         */
        public bool HideIdentityWhenUndiscovered
        {
            get;
            set;
        }

        public bool RevealUnlockRequirementWhenUndiscovered
        {
            get;
            set;
        } = true;

        public bool Hidden
        {
            get;
            set;
        }

        public bool Selectable
        {
            get;
            set;
        } = true;

        public Object Asset
        {
            get;
            set;
        }

        public Sprite IconSprite
        {
            get;
            set;
        }

        public Texture IconTexture
        {
            get;
            set;
        }

        public GameObject ModelPrefab
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

        public override string ToString()
        {
            return
                Kind +
                " | " +
                DisplayName +
                " | " +
                StableId;
        }
    }
}
