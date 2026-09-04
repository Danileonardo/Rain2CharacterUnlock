namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// IDs permanentes de la biblioteca integrada.
    ///
    /// base.*
    ///     bloques atómicos reutilizables por el editor.
    ///
    /// creator.*
    ///     recetas/presets creados a partir de desafíos históricos
    ///     o diseñados para personajes concretos.
    /// </summary>
    public static class MissionPresetIds
    {
        // =========================================================
        // OBJETIVOS BASE - COMBATE
        // =========================================================

        public const string KillEnemies =
            "base.objective.kill_enemies";

        public const string KillElite =
            "base.objective.kill_elite";

        public const string KillBoss =
            "base.objective.kill_boss";

        public const string KillSpecificBody =
            "base.objective.kill_specific_body";

        public const string HitTarget =
            "base.objective.hit_target";


        // =========================================================
        // OBJETIVOS BASE - PROGRESIÓN / RUN
        // =========================================================

        public const string ReachLevel =
            "base.objective.reach_level";

        public const string ReachStage =
            "base.objective.reach_stage";

        public const string CompleteStage =
            "base.objective.complete_stage";

        public const string CompleteTeleporter =
            "base.objective.complete_teleporter";

        public const string CompleteRun =
            "base.objective.complete_run";


        // =========================================================
        // OBJETIVOS BASE - CURACIÓN / ESTADOS
        // =========================================================

        public const string HealHealth =
            "base.objective.heal_health";

        public const string ApplyStatusEffects =
            "base.objective.apply_status_effects";

        public const string ApplyNegativeStatusEffects =
            "base.objective.apply_negative_status_effects";

        public const string ApplyPositiveStatusEffects =
            "base.objective.apply_positive_status_effects";

        public const string ApplyDotEffects =
            "base.objective.apply_dot_effects";

        public const string ApplyPoison =
            "base.objective.apply_poison";


        // =========================================================
        // OBJETIVOS BASE - OBJETOS / HABILIDADES / ALIADOS
        // =========================================================

        public const string HoldItemStack =
            "base.objective.hold_item_stack";

        public const string PickupItem =
            "base.objective.pickup_item";

        public const string ScrapItems =
            "base.objective.scrap_items";

        public const string UseSkill =
            "base.objective.use_skill";

        public const string RecruitMinions =
            "base.objective.recruit_minions";

        public const string BombingRun =
            "base.objective.bombing_run";

        public const string CarryEquipment =
            "base.objective.carry_equipment";

        public const string CompleteEnding =
            "base.objective.complete_ending";

        public const string DefeatUmbraWaves =
            "base.objective.defeat_umbra_waves";

        public const string LeaveStage =
            "base.objective.leave_stage";


        // =========================================================
        // CONDICIONES BASE - MOVIMIENTO
        // =========================================================

        public const string Airborne =
            "base.condition.airborne";

        public const string Grounded =
            "base.condition.grounded";


        // =========================================================
        // CONDICIONES BASE - REQUISITOS
        // =========================================================

        public const string RequiredSurvivor =
            "base.condition.required_survivor";

        public const string RequiredStage =
            "base.condition.required_stage";

        public const string RequiredItem =
            "base.condition.required_item";

        public const string RequiredEquipment =
            "base.condition.required_equipment";

        public const string RequiredSkill =
            "base.condition.required_skill";

        public const string Difficulty =
            "base.condition.difficulty";

        public const string HealthBelow =
            "base.condition.health_below";

        public const string TimeLimit =
            "base.condition.time_limit";


        // =========================================================
        // CONDICIONES BASE - DAÑO / GOLPE FINAL
        // =========================================================

        public const string DamageType =
            "base.condition.damage_type";

        public const string ExplosiveDamage =
            "base.condition.damage_explosive";

        public const string CriticalHit =
            "base.condition.critical_hit";

        public const string WeakPoint =
            "base.condition.weak_point";

        public const string Backstab =
            "base.condition.backstab";

        public const string FatalHit =
            "base.condition.fatal_hit";

        public const string MinimumDamage =
            "base.condition.minimum_damage";


        // =========================================================
        // CONDICIONES BASE - RESTRICCIONES
        // =========================================================

        public const string NoDamage =
            "base.condition.no_damage";

        public const string NoHealing =
            "base.condition.no_healing";

        public const string NoDeath =
            "base.condition.no_death";

        public const string NoItemPickup =
            "base.condition.no_item_pickup";

        public const string StageSequence =
            "base.condition.stage_sequence";

        public const string PriorSkillUsed =
            "base.condition.prior_skill_used";

        public const string PartyHasSurvivor =
            "base.condition.party_has_survivor";

        public const string MinionsAlive =
            "base.condition.minions_alive";


        // =========================================================
        // CONDICIONES BASE - ESTADOS
        // =========================================================

        public const string StatusSpecific =
            "base.condition.status_specific";

        public const string StatusAnyValid =
            "base.condition.status_any_valid";

        public const string StatusAnyNegative =
            "base.condition.status_any_negative";

        public const string StatusAnyPositive =
            "base.condition.status_any_positive";

        public const string StatusAnyDot =
            "base.condition.status_any_dot";

        public const string StatusPoison =
            "base.condition.status_poison";

        public const string StatusBurn =
            "base.condition.status_burn";

        public const string StatusBleed =
            "base.condition.status_bleed";

        public const string StatusFreeze =
            "base.condition.status_freeze";

        public const string StatusStun =
            "base.condition.status_stun";


        // =========================================================
        // PRESET GENÉRICO HISTÓRICO
        // =========================================================

        public const string DefaultKillEnemies100 =
            "creator.default.kill_enemies_100";


        // =========================================================
        // RECETAS HISTÓRICAS DE PERSONAJES - LEGACY
        // =========================================================
        // Se conservan como IDs estables para compatibilidad y
        // migración. No deben aparecer como nuevas misiones asignables.

        public const string StatusEffects100 =
            "creator.status_effects_100";

        public const string TeamHealing10000 =
            "creator.team_healing_10000";

        public const string BossCritical44444 =
            "creator.boss_critical_44444";

        public const string EnergyDrinks15 =
            "creator.energy_drinks_15";

        public const string BossBackstab =
            "creator.boss_backstab";

        public const string AirborneExplosionKills15 =
            "creator.airborne_explosion_kills_15";


        // =========================================================
        // HUNK HISTÓRICO - PIEZAS SEPARADAS
        // =========================================================

        public const string RailgunnerWeakPoints24 =
            "creator.railgunner_weak_points_24";

        public const string BanditLightsOutKills24 =
            "creator.bandit_lights_out_kills_24";


        // =========================================================
        // TINKATON - RECETAS HISTÓRICAS SEPARADAS (LEGACY)
        // =========================================================
        // Sus equivalentes reutilizables actuales ya existen como
        // Objective / Condition base del Mission Schema v2.

        public const string ScrapItems6 =
            "creator.scrap_items_6";

        public const string ShatteringJustice1 =
            "creator.shattering_justice_1";

        public const string AlloyBlastCanisterFinisher =
            "creator.alloy_blast_canister_finisher";


        // =========================================================
        // IDS LEGACY COMPUESTOS RESERVADOS PARA MIGRACIÓN
        // =========================================================

        public const string PrecisionExecution24 =
            "creator.precision_execution_24";

        public const string ScrapAlloyFinisher =
            "creator.scrap_alloy_finisher";


        // =========================================================
        // RECETAS OFICIALES ACTUALES
        // =========================================================

        // ID canónico actual de El Cuarto Acto.
        public const string JhinOfficial =
            "creator.official.jhin.fourth_act";

        public const string SpyOfficial =
            "creator.official.spy.unseen_approach";

        public const string ScoutOfficial =
            "creator.official.scout.thermonuclear_thirst";

        public const string RocketOfficial =
            "creator.official.rocket.gravity_optional";

        public const string HunkOfficial =
            "creator.official.hunk.reaper_never_misses";

        public const string TinkatonOfficial =
            "creator.official.tinkaton.forged_in_scrap";

        public const string WooperOfficial =
            "creator.official.wooper.back_to_water";

        public const string SoraOfficial =
            "creator.official.sora.keyblade_chosen";

        public const string RalseiOfficial =
            "creator.official.ralsei.power_of_kindness";
    }
}
