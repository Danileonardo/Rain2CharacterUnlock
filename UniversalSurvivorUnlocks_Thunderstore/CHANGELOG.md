# Changelog

All notable changes to Universal Survivor Unlocks will be documented in this file.

The format is based on a simple versioned changelog intended for Thunderstore and GitHub releases.

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
