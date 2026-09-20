namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Familias semánticas que G6 puede considerar para una futura misión.
    ///
    /// No son todavía condiciones persistidas ni trackers concretos. G6 sólo
    /// determina si una familia encaja con el perfil del target. G7 decidirá
    /// composición, cantidades y trackers reales.
    /// </summary>
    public enum MissionTemplateFamily
    {
        Unknown = 0,
        CombatKills = 10,
        BossFinisher = 20,
        Execution = 30,
        CriticalHit = 40,
        Backstab = 50,
        StatusEffect = 60,
        HealingSupport = 70,
        DefenseSurvival = 80,
        MobilityTraversal = 90,
        AirborneCombat = 100,
        SummonMinion = 110,
        ResourceMastery = 120,
        MeleeCombat = 130,
        ProjectileCombat = 140,
        AreaControl = 150,
        SelfBuffMastery = 160
    }
}
