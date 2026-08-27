using RoR2;
using RoR2.ExpansionManagement;

namespace UniversalSurvivorUnlocks
{
    public enum SurvivorStatus
    {
        Available,
        DlcNotOwned,
        Hidden,
        NotSelectable
    }

    public class SurvivorInfo
    {
        public SurvivorDef SurvivorDef { get; set; }

        public string InternalName { get; set; }

        public string DisplayName { get; set; }

        public string BodyName { get; set; }

        public string UnlockableName { get; set; }

        public ExpansionDef RequiredExpansion { get; set; }

        public string ExpansionName { get; set; }

        public SurvivorStatus Status { get; set; }
    }
}