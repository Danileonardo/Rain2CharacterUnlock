# Changelog

All notable changes to Universal Survivor Unlocks will be documented in this file.

The format is based on a simple versioned changelog intended for Thunderstore and GitHub releases.

## 0.1.9

### Added

- Added the first foundation of the new **Mission System v2**.
- Added reusable mission models for:
  - `MissionDefinition`
  - `MissionRoute`
  - `MissionObjective`
  - `MissionCondition`
  - `MissionRules`
  - `MissionTarget`
  - `MissionPreset`
- Added support in the new mission model for alternative mission routes:
  - Conditions inside a route use `AND`.
  - Different routes can work as `OR` alternatives.
- Added mission progress scope definitions for:
  - `Shared`
  - `PerPlayer`
- Added the creator-preset / custom-mission ownership model.
- Creator presets are treated as original source definitions and are not intended to be edited directly.
- Added groundwork for player-created custom missions based on creator presets without replacing the original preset.
- Added initial `Kill` objective evaluation for Mission System v2.
- Added initial reusable mission conditions:
  - `Airborne`
  - `Grounded`
  - `RequiredSurvivor`
- Added reusable mission target categories:
  - `Any`
  - `Enemy`
  - `Boss`
  - `Elite`
  - `SpecificBody`
  - `SpecificBoss`
- Added support for identifying specific enemies and bosses through stable internal body IDs such as `BrotherBody` or `SuperRoboBallBossBody`.
- Added runtime `KillEnemies` challenge tracking.
- Added GitHub Sponsors integration through `.github/FUNDING.yml`.
- Added bilingual project support and donation information to the README.
- Added Discord community sections for questions, bug reports, survivor requests, preset ideas and suggestions.
- Added detailed credits for the third-party survivor mods referenced by USU presets.

### Multiplayer

- Reworked challenge completion routing around a centralized multiplayer-aware completion system.
- Improved session-wide unlock delivery.
- Preserved the host-authoritative challenge model.
- The host's effective challenge configuration remains authoritative during multiplayer sessions.
- Shared challenges can receive valid progress from multiple players.
- Per-player challenges continue to maintain independent player progress.
- A valid individual route can complete its challenge for the session without combining invalid partial player progress.
- The mission architecture now explicitly represents `Shared` and `PerPlayer` progress scopes.
- Player-local configuration is intended to remain separate from the host's effective session configuration.

### Changed

- Bumped Universal Survivor Unlocks to version `0.1.9`.
- Updated package metadata for Thunderstore.
- Updated the Thunderstore website link to the project's GitHub Sponsors page.
- Reworked the README to better explain:
  - Built-in creator presets.
  - Multiplayer behavior.
  - Configuration persistence.
  - Mission System v2.
  - Support and donations.
  - Discord/community support.
  - Survivor mod attribution.
  - AI usage transparency.
- Updated Jhin documentation to match the current preset requirement of **44,444 critical damage**.
- Updated Scout documentation to match the current preset name **Energía Atómica**.
- Documented Rocket's current requirement of **15 airborne explosion kills**.
- Clarified that Ralsei's challenge ignores passive regeneration.
- Improved separation between currently functional challenge trackers and the next-generation universal mission architecture.

### Fixed

- Fixed outdated project version metadata that was still reporting `0.1.6` in the `.csproj`.
- Fixed inconsistent package version metadata between the plugin and Thunderstore manifests.
- Corrected outdated README and CHANGELOG information that no longer matched current preset values.
- Removed outdated documentation implying that all multiplayer challenge tracking was still experimental.
- Improved documentation around creator presets so users are not encouraged to directly overwrite source presets.

### Development

- Existing challenge trackers remain functional while presets are progressively migrated to Mission System v2.
- Mission System v2 currently provides architectural groundwork and initial event evaluation; not every planned objective or condition is exposed to players yet.
- The future in-game mission editor will operate on custom mission copies instead of directly modifying creator presets.

## 0.1.8

### Added
- Added Tinkaton preset: **Forjada en Chatarra**.
- Added the new `ScrapItemBossFinisher` challenge type.
- Added tracking for items converted into Scrap during a run.
- Added final-hit validation for MUL-T's **Bote explosivo** against the Alloy Worship Unit.

### Changed
- Increased Ralsei's **Oración de Esperanza** healing requirement from **5,000** to **10,000**.
- Updated Bandit's multiplayer HUNK route to reduce false streak resets for remote clients.
- Shortened Tinkaton's challenge description to fit correctly in the survivor selection UI.

### Testing
- Continued multiplayer validation for HUNK.
- Added diagnostic logging for Tinkaton's Scrap and final-hit conditions.

## 0.1.6

### Added

- Added built-in unlock presets for additional supported modded survivors:
  - Sora — `Elegido de la Llave Espada`
  - Ralsei — `Oración de Esperanza`
  - Jhin — `El Cuarto Acto`
  - Scout — `Sed Termonuclear`
  - Spy — `Sin que me veas venir`
  - Rocket — `La gravedad es opcional`
  - HUNK — `La Parca No Falla`

- Added the `description` field to survivor challenge configuration.
- Added support for custom challenge descriptions displayed by generated unlocks.

- Added the `HealHealth` challenge type.
  - Tracks actual health restored to Player-team entities during a run.
  - Supports players, allied summons, drones and turrets.
  - Does not count barrier, shields or overhealing.

- Added the `BossCriticalKill` challenge type.
  - Supports boss kills requiring a minimum critical-hit damage value.
  - The current Jhin preset requires a lethal critical hit of at least 4444 damage.

- Added the `HoldItemStack` challenge type.
  - Tracks item stacks independently for each player.
  - Player inventories are never combined.
  - The current Scout preset requires one player to hold 15 Energy Drinks.

- Added the `BackstabBossKill` challenge type.
  - Uses Risk of Rain 2's backstab detection.
  - The current Spy preset requires Bandit's Serrated Dagger to deliver the lethal backstab to a boss.

- Added the `AirborneExplosionKills` challenge type.
  - Tracks lethal explosion / blast kills while the owning player remains airborne.
  - Touching the ground resets that player's streak.
  - Supports blast and shockwave kills.
  - Supports player-owned explosive drones and minions.
  - Drone or minion airborne state does not affect progress; only the owning player's airborne state is checked.
  - Damage-over-time kills occurring after an explosion do not count.

- Added the `PrecisionExecutionStreak` challenge type.
  - Supports consecutive Railgunner M99 weak-point hits.
  - Supports consecutive Bandit Lights Out kills.
  - The current HUNK preset requires either 24 consecutive Railgunner weak-point hits or 24 consecutive Lights Out kills.

- Added player ownership resolution for gameplay events generated by drones and minions.
- Added server-side tracking systems for the new challenge types.
- Added built-in preset authoring synchronization support for development.

### Changed

- Renamed Sora's built-in challenge from `Guerrero de la llave` to `Elegido de la Llave Espada`.
- Updated built-in survivor presets to use explicit challenge descriptions.
- Expanded gameplay challenge architecture to support multiple reusable server-tracked challenge types.
- Improved individual-player tracking so challenge progress from different players is not incorrectly combined.
- Improved handling of current Risk of Rain 2 APIs for item counts, damage sources and bullet hit information.

### Development / Validation

- The new gameplay challenge implementations compile successfully.
- A general gameplay validation pass is still pending for the newly added survivor presets.
- Full multiplayer behavior remains experimental and requires additional validation.

## 0.1.3

- Added the built-in default unlock preset for Dragonyck's Sora.
- Sora now automatically receives the `Guerrero de la llave` challenge when first detected by Universal Survivor Unlocks.
- The default Sora challenge uses `ApplyStatusEffects` and requires 100 valid active status effects simultaneously during a single run.
- Added automatic migration for Sora configurations using the legacy default `KillEnemies 100` challenge.
- Existing customized Sora challenge configurations are preserved and are not overwritten.

## 0.1.1

- Added initial support for configurable survivor challenges.
- Added `ApplyStatusEffects` challenge tracking.
- Status effect progress is calculated from effects currently active during a run.
- Negative status effects are counted only on enemies.
- Positive status effects are counted only on allies.
- Stackable effects contribute once per active stack.
- Non-stackable effects contribute once per affected entity.
- Multiple enemies and allies can contribute to the total simultaneously.
- Status effects stop contributing when they expire, are removed, or the affected entity dies.
- Added server-side tracking groundwork for multiplayer-compatible challenges.
- Improved dynamic survivor unlock handling and challenge configuration.

## [0.1.0] - Initial Public Release

### Added

- Automatic detection of modded survivors.
- Support for modded survivors loaded through different ContentPack implementations.
- Automatic creation of unlock requirements for modded survivors that do not provide their own character unlock.
- Automatic locking of newly detected supported modded survivors on first startup.
- Preservation of unlock requirements created by original survivor mod authors.
- Vanilla and official DLC survivors are excluded from the custom unlock system.
- Automatic generation of `Survivors.json`.
- Automatic preservation of unavailable survivor configurations.
- Automatic creation of `Survivors.backup.json` when updating an existing configuration.
- Default challenge configuration for newly detected supported survivors:
  - `type`: `KillEnemies`
  - `amount`: `100`
- Custom `UnlockableDef` generation for supported modded survivors.
- Custom achievement definitions used by generated survivor unlocks.
- Modded survivor portrait support for generated achievement icons.
- Locked survivor portrait handling in the character selection screen.
- Compatibility with RealerCheatUnlocks for manually granting and revoking generated unlocks.
- Logging for survivor detection, source identification, unlock generation and configuration synchronization.

### Tested Compatibility

The detection and locking system has been tested with several modded survivors using different loading implementations, including:

- Sora
- Ralsei
- HUNK

These survivor mods are not dependencies.

### Configuration

The generated configuration file is located at:

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

When using r2modman or Thunderstore Mod Manager, the file is located inside the selected profile.

### Known Limitations

- Gameplay challenge progress tracking is not yet implemented.
- Challenge definitions such as `KillEnemies` are currently stored in the JSON and used to define generated unlock requirements, but they do not yet automatically complete through gameplay.
- Additional challenge types are planned for future versions.

### Planned

- Functional gameplay challenge tracking.
- Additional challenge types.
- Boss-related unlock conditions.
- Elite enemy challenges.
- Stage progression challenges.
- Difficulty-based challenges.
- Run completion challenges.
- Custom challenge presets for exceptionally powerful modded survivors.
- Additional compatibility testing with survivor frameworks and character packs.
