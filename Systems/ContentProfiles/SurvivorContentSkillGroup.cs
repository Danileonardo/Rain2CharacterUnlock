using System.Collections.Generic;
using RoR2;
using RoR2.Skills;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Una fila contextual del loadout de un survivor.
    /// Ejemplo: Primary -> 3 variantes.
    /// </summary>
    public sealed class SurvivorContentSkillGroup
    {
        public string StableId
        {
            get;
            set;
        } = "";

        public SurvivorContentSkillSlot Slot
        {
            get;
            set;
        } = SurvivorContentSkillSlot.Extra;

        public string SlotLabel
        {
            get;
            set;
        } = "";

        public int ExtraSlotIndex
        {
            get;
            set;
        }

        public GenericSkill GenericSkill
        {
            get;
            set;
        }

        public SkillFamily SkillFamily
        {
            get;
            set;
        }

        public List<SurvivorContentSkillVariant> Variants
        {
            get;
        } = new List<SurvivorContentSkillVariant>();
    }
}
