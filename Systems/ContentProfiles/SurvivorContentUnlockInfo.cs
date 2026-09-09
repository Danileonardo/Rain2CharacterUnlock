using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Información de desbloqueo lista para el futuro HUD.
    ///
    /// Regla de producto:
    /// aunque la identidad del contenido esté oculta por no haber sido
    /// descubierto, MissionName/MissionDescription pueden seguir mostrándose.
    /// </summary>
    public sealed class SurvivorContentUnlockInfo
    {
        public UnlockableDef UnlockableDef
        {
            get;
            set;
        }

        public SurvivorContentUnlockSource Source
        {
            get;
            set;
        } = SurvivorContentUnlockSource.Unknown;

        public bool HasRequirement
        {
            get;
            set;
        }

        public bool IsUnlocked
        {
            get;
            set;
        }

        public string UnlockableIdentifier
        {
            get;
            set;
        } = "";

        public string AchievementIdentifier
        {
            get;
            set;
        } = "";

        public string MissionName
        {
            get;
            set;
        } = "";

        public string MissionDescription
        {
            get;
            set;
        } = "";

        public Sprite AchievementIcon
        {
            get;
            set;
        }

        /*
         * Siempre true por diseño. El HUD podrá ocultar nombre/icono de la
         * skill/skin, pero no debe ocultar cómo conseguirla.
         */
        public bool RevealRequirementWhenLocked
        {
            get;
            set;
        } = true;
    }
}
