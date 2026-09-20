using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Perfil diagnóstico G3 de un survivor que existe en el runtime actual.
    ///
    /// IMPORTANTE:
    /// - no se persiste en JSON;
    /// - no existe para mods ausentes;
    /// - no asigna PowerTier ni ChallengeTier;
    /// - no genera ni modifica misiones.
    /// </summary>
    public sealed class MissionProfile
    {
        public int AnalysisVersion
        {
            get;
            set;
        } = MissionProfileAnalyzer.AnalysisVersion;

        public string SurvivorId
        {
            get;
            set;
        } = "";

        public string BodyName
        {
            get;
            set;
        } = "";

        public string DisplayName
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

        public bool IsModded
        {
            get;
            set;
        }

        public SurvivorStatus Status
        {
            get;
            set;
        }

        public bool HasOriginalUnlock
        {
            get;
            set;
        }

        public SurvivorContentUnlockSource UnlockSource
        {
            get;
            set;
        } = SurvivorContentUnlockSource.Unknown;

        public bool IsUnlocked
        {
            get;
            set;
        }

        public MissionBodyProfile Body
        {
            get;
            set;
        } = new MissionBodyProfile();

        public int SkillGroupCount
        {
            get;
            set;
        }

        public int SkillVariantCount
        {
            get;
            set;
        }

        public int LockedSkillVariantCount
        {
            get;
            set;
        }

        public int AlternateSkillVariantCount
        {
            get;
            set;
        }

        public bool HasPassive
        {
            get;
            set;
        }

        public bool HasExtraSkillSlots
        {
            get;
            set;
        }

        public List<MissionSkillProfile> Skills
        {
            get;
        } = new List<MissionSkillProfile>();

        public List<MissionProfileCapability> Capabilities
        {
            get;
        } = new List<MissionProfileCapability>();

        public List<MissionCapabilityEvidence> Evidence
        {
            get;
        } = new List<MissionCapabilityEvidence>();
    }
}
