
## 0.3.1 DEV — C2.3 Vanilla Unlock Method Catalog

- Added a read-only catalog of official survivor unlock achievement structures.
- Logs achievement type, server tracker type, prerequisite and reward for vanilla/DLC survivor unlocks.
- Intended to identify reusable Mission V2 mechanics without modifying official unlock authority.

# Changelog

## 0.3.1 — Universal visual preset and expanded provider support

### Main changes

- Reused the AH-64-inspired Mission Library design for other survivors, while using each survivor's actual portrait, state, active provider mission, overview and unlocked Logbook notes.
- Added clearer mission and notes cards with scrollable content and larger text; retained the existing character selection and provider actions.
- Refined AH-64's `Protocol 64: Air Supremacy` cumulative campaign presentation: tracked goals for flight time, airborne bullets, missiles/rockets and teleporter bosses, followed by a Monsoon escape objective. The campaign widgets are exclusive to AH-64.
- Expanded curated mission/definition/localization work for compatible modded survivors; preserved Original ownership and native achievements where applicable.
- Continued improvements to Generated mission reroll, provider coexistence/persistence, multiplayer authority pathways and logging.
- Updated declared dependencies for Risk Of Options, ScrollableLobbyUI, R2API components and SurvivorSortOrder (the latter is not a required BepInEx assembly).

### Known limitations / ongoing QA

- Long mission text and custom provider combinations can still require UI adjustments.
- AH-64's campaign tracking and multiplayer persistence are not fully verified for all configurations.
- Some survivor-specific unlock paths and multiplayer scenarios need broader testing.



- Adds generic Mission v2 `ConsumeItemOnInteraction` objective, extracted from the official Acolyte unlock pattern without replacing its native unlock.

## 0.3.1 (development) — Current game dependency alignment

- Risk Of Options is now a **required dependency** instead of a soft integration.
- Minimum/packaged dependency: `Rune580-Risk_Of_Options-2.8.6`.
- Updated Thunderstore BepInEx dependency to `bbepis-BepInExPack-5.4.2122`.
- Confirmed the R2API modules used directly by USU are already at their current published versions: Core 5.3.0, ContentManagement 1.0.11, Language 1.1.0, Unlockable 1.0.2 and Networking 1.0.4.
- The legacy Character Select launcher remains as a defensive runtime fallback only; it is no longer the supported normal configuration path.

## 0.3.1 — Generated Controls, Provider Completion & Compatibility

> **0.3.1 release history:** additional development details below. Full compatibility QA continues.

- Fixes the release-version integrity path so the packaged DLL, BepInEx plugin metadata, assembly version and Thunderstore manifest cannot silently disagree.
- Includes the Generated mission reroll control and final full-face dice icon.
- Includes the four-source provider row: Original / USU / Personalizada / Generada.
- Includes provider-slot persistence work from G8 Phase 1.7.
- Compatibility validation for the Risk of Rain 2 1.5.0 / Hallowed Concepts update is in progress.
- Adds the first DLC4-inspired reusable Mission v2 building block: `InteractWithTarget` (`base.objective.interact_with_target`), without hardcoding or replacing Acolyte's native unlock.

All notable changes to Universal Survivor Unlocks are documented here for Thunderstore and GitHub releases.

## 0.3.0 — Adaptive Generated Missions & Runtime Authority

- G8 Phase 1.6 — Generated Reroll UI: las misiones `Source=Generated` muestran proveedor **Generada** y un dado de reroll en el navegador. El reroll cambia `generationNonce`, recalcula y persiste Mission V2, y actualiza nombre/descripción sin convertir la misión en Custom.

### Highlights

- Added the first public **adaptive Generated mission pipeline** for modded survivors that do not currently have an applicable Original, Official USU, or player CUSTOM mission.
- Generated missions now use the same persisted Mission v2 configuration path as Custom: they are stored in `Survivors.json` with `source = Generated`, while remaining auto-managed fallbacks rather than player CUSTOM content.
- Added survivor gameplay/profile analysis, local power assessment, challenge-tier resolution, template compatibility scoring, deterministic candidate generation, and runtime mission materialization.
- Preserved creator-owned unlock content: vanilla/DLC progression and mod-author Original unlocks remain authoritative when present. USU Generated content does not replace them.
- Kept the 10 Official Mission v2 presets authoritative over Generated candidates and retained the 12 Legacy recipes as hidden reusable compatibility/editor assets.

### Generated mission system

- Added `MissionProfileAnalyzer` and related profile models to classify survivor bodies, skills, effects, usage patterns, and mission-relevant capabilities.
- Added local power/complexity assessment plus ChallengeTier resolution used to scale generated mission difficulty without hardcoding individual survivor names.
- Added compatibility-scored reusable mission template families and a deterministic generator that selects temporary mission candidates from the local installed/unlocked context.
- Added runtime materialization for the currently supported Generated families:
  - `Execution`
  - `HealingSupport`
  - `ResourceMastery`
  - `MeleeCombat`
  - `ProjectileCombat`
- Generated missions use compatible alternative performers rather than requiring the target survivor itself, and the generated target is never used to unlock itself.
- Added generated presentation formatting with compact vanilla-style text, including the existing two-line / 50-character-per-line presentation rule. Generic mechanic missions no longer expose internal performer counts such as `+7`; character suffixes are reserved for missions that actually require specific characters.
- Retained the historical 100-kill challenge only as an emergency compatibility fallback. Its presence is not used as a reason to replace Original creator content.

### Runtime, authority, and presentation

- Added Generated Mission v2 persistence into `Survivors.json` with a per-entry generation nonce and automatic refresh when the survivor/profile analysis changes.
- Deleting a Generated survivor entry or the whole `Survivors.json` now creates a fresh generation nonce on the next startup, providing a temporary manual reroll path until the in-game reroll UI exists.
- Editing a stamped Generated block directly in `Survivors.json` is detected on the next sync and promoted to `source = Custom` so player edits are not overwritten by the generator.
- Removed the historical automatic `KillEnemies 100` creation path. Existing exact legacy fallback entries are migrated to Official, Original-neutral, or Generated as appropriate; genuine Custom/author missions of 100 kills are preserved.
- Added effective mission routing so Mission v2 Generated content is used for challenge evaluation.
- Added `AllowedPerformerSkill` runtime condition support and generalized skill-token/internal-skill matching for generated performer constraints.
- Added Mission v2 objective support used by the new generated families for healing and compatible skill/resource actions.
- Added an effective presentation bridge for Character Select / achievement-facing text so Generated and Official USU missions present the mission that is actually active.
- Original providers now explicitly bypass USU EFFECTIVE token refresh, preserving the creator's own presentation for Artillerist, Shock Trooper, Aurelion Sol, Auriel, Enforcer, Nemesis Enforcer, Miner while Original is selected, and future compatible Original providers.
- Miner remains the intentional curated exception where the player can explicitly choose between the creator's Original unlock and USU's separate `Excavación / Excavation` preset.

### Multiplayer/session groundwork

- Generated missions participate in the same host-authoritative session snapshot model as other effective missions.
- Host Character Select snapshots include Generated/Official/selected mission data without persisting host choices into a client's local configuration.
- Generated progress scopes are materialized as shared session progress where required by the current generated templates.
- Host-side lobby snapshot construction and fragmented dispatch were validated with the current 23-survivor test profile. Broader client/run validation continues during the public release test cycle.

### Validation

- Previous G8 test profile reached **7 Generated candidates → 7 materialized**, with `Unsupported families: 0` and `Invalid materializations: 0`; Phase 1.5 now requires a fresh compile/runtime validation because persistence semantics changed.
- Confirmed that Original provider presentation is preserved while Generated/Official USU presentation continues to refresh normally.
- Confirmed that generated mission data remains temporary (`Persistencia: no`) and does not become a stored CUSTOM mission.

## 0.2.2 — Mission Authority & Persistence Foundation

### Highlights

- Separated the **10 current official Mission System v2 presets** from the **12 hidden Legacy recipes**. Legacy recipes remain preserved for compatibility, future CUSTOM missions, and later editor/generator work.
- Added Miner's independent USU preset **Excavación / Excavation** while preserving MinerUnearthed's native Original unlock. Miner remains Original by default and can explicitly switch **Original ↔ USU**.
- Added stable survivor identity using `ContentPack + Body` (`survivor:<source>:<body>`) so persisted mission ownership no longer depends only on a transient runtime entry.
- Upgraded `Survivors.json` to **schema v2** with `dormantMissions` and the explicit `PendingContent` state for survivor mods that are temporarily absent.
- Reinstalled survivors now rehydrate the same persisted mission/configuration by stable identity while rebuilding runtime metadata from the currently installed mod.

### Persistence and compatibility

- Absent survivor mods persist only the minimum required data: stable identity, Body/Source, challenge/Mission v2 configuration, provider/selection state, preset relationship, CUSTOM data, and forward-compatible mission metadata.
- Runtime character snapshots such as skills, skins, prefabs, lore, stats, Unity objects, and ContentProfile data are not stored as dormant truth.
- Added migration from the previous `MissingContent` persistence model to mission-only dormant records.
- Official USU missions may remain prepared while their referenced survivor mod is not installed.
- `Provider`, `SelectionMode`, `BasePresetId`, `CustomMission`, Mission v2 data, and unknown forward-compatible fields are preserved during normalization.
- Future unsupported `schemaVersion` values put persistence into read-only mode instead of allowing an older build to overwrite newer data.
- Saves continue to use a temporary file before replacement and keep a previous-version backup.

### Mission authority and provider handling

- Added centralized authority validation for the fixed current set of **10 Official / 12 Legacy** presets, stable IDs, Body/Source bindings, Mission v2 metadata, localization bindings, reward ranges, and Legacy isolation.
- Centralized provider policy and provider selection so the embedded Risk Of Options UI, fallback UI, persistence, and runtime assignment paths share the same Original/USU/CUSTOM precedence rules.
- Preserved the global Original-first policy. Explicit `UserSelected` choices are not silently replaced by automatic fallback logic.
- Miner is the current curated direct Original ↔ USU exception and targets `creator.official.miner.excavation`.

### Runtime and maintenance

- Consolidated gameplay bootstrap, Mission v2 run lifecycle, Legacy run lifecycle, shared gameplay event hooks, polling tiers, and duplicated scrapper handling without intentionally changing mission semantics.
- Reduced duplicated high-frequency engine subscriptions and retained full-rate polling only where mission behavior requires it.
- Removed closed development-only migration/dead-code paths and reorganized survivor, persistence, provider, UI, Mission preset, and runtime files by responsibility.
- Public release builds now keep preset `AuthoringMode` disabled; official assigned snapshots continue to refresh through their stable `BasePresetId` path.

### Release validation

- Mission Authority runtime validation: **10 Official / 12 Legacy / Miner direct Original ↔ USU**.
- Miner persistence workflow validated through **installed → absent/PendingContent → reinstalled**, preserving the same stable identity, `Excavación`, `Provider=USU`, and `SelectionMode=UserSelected` without leaving a duplicate dormant record.
- This release does **not** introduce adaptive mission generation, power scoring, ChallengeTier resolution, or random template generation; those remain future work.

## 0.2.1 — Survivor Definitions & Mission Library Update

### Release focus

- Closed the current curated survivor presentation/localization pass for the installed/tested survivor set.
- Added the first public Mission Library/provider-management interface while keeping the full mission editor for a later release.
- Kept the nine creator-made unlock missions on the existing Mission System v2 backend; this release does not migrate native/legacy unlock logic into Survivor Definitions.

### Added

- Added the central `SurvivorDefinition` / `SurvivorDefinitionRegistry` system.
- Added curated Definitions for **15 survivor integrations**:
  - Aurelion Sol
  - Auriel
  - Enforcer
  - Ralsei
  - Sora
  - Wooper
  - Tinkaton
  - HUNK
  - Jhin
  - Miner
  - Rocket
  - Scout
  - Spy
  - Heretic
  - Nemesis Enforcer
- Added manual curated Spanish localization (`es-419` / `es-ES`) plus English repairs for visible modded-survivor content where needed.
- Added survivor content profiles covering overview/details, skills, keywords, skins, unlock information, and Logbook-facing lore/Notes.
- Added runtime content catalog services for survivors, items, equipment, skills, bodies, and stages, including discovery-aware presentation support.
- Added Mission Library UI with an optional embedded **Risk Of Options** integration.
- Added a Character Select launcher fallback when Risk Of Options is not installed.
- Added provider/assignment services for Original/USU/Custom state, preset assignment, restore-original behavior, preset snapshot refresh, and runtime mission refresh.
- Added centralized `UsuLog` policy with `Logging.VerboseLogging=false` by default.
- Added runtime context tracking used by mission/content systems.

### Changed

- Updated Jhin's active **El Cuarto Acto** requirement to **4,444+ lethal critical damage** in the current preset/configuration source.
- Expanded survivor detection/content-profile handling so curated Definitions can activate without hard references to external survivor DLLs.
- Heretic now uses a special Definition path compatible with Moffein-Heretic exposing the vanilla `HereticBody`/SurvivorDef instead of registering a separate modded SurvivorDef.
- Updated locked-survivor presentation, browser/profile refresh behavior, and configuration reconciliation for the new profile/Definition layers.
- Improved runtime refresh and assignment handling so provider changes can be reflected without replacing the player's unrelated personal configuration.
- Risk Of Options is now a **soft dependency/integration**, not a required package dependency.

### Localization / survivor presentation

- Curated visible-content coverage now includes character overview/details, skills, keyword panels, skins, achievements/requirements, and lore where available for the registered Definitions.
- `Notes` continues to represent Logbook lore only and follows the real native Logbook discovery state instead of a separate USU-known flag.
- Removed the runtime automatic-translation direction from the current release path; 0.2.1 uses curated manual localization.

### Compatibility and release metadata

- Bumped plugin, assembly project metadata, source manifest, and Thunderstore staging manifest to `0.2.1`.
- Updated README documentation for the current Mission Library, optional Risk Of Options integration, Definitions, and provider behavior.
- Preserved native survivor unlock providers when they exist; Definitions do not replace creator-owned unlock logic.
- Full visual mission authoring/editing remains planned for a later release.


## 0.2.0 — Creator Challenges Update

### Release focus

- This release is focused on public testing and balance feedback for the current **9 creator-made survivor unlock challenges**.
- The in-game preset library, preset reassignment UI, editable copies, and full mission editor are **not included yet**.
- Internal preset/library architecture is included so future releases can build on the same stable mission definitions without reverting the current challenge work.

### Added

- Added/finished the current official Mission System v2 presets for:
  - Sora — **Elegido de la Llave Espada**
  - Ralsei — **El poder de la bondad**
  - Jhin — **El Cuarto Acto**
  - Spy — **Sin que me veas venir**
  - Scout — **Sed Termonuclear**
  - Rocket — **La gravedad es opcional**
  - HUNK — **La Parca No Falla**
  - Tinkaton — **Forjada en Chatarra**
  - Wooper — **De vuelta al agua**
- Added localized official challenge names/descriptions for English and Spanish (`es-419` / `es-ES`), with English fallback.
- Added Lunar Coin rewards to the official generated achievements:
  - Sora: `+4`
  - Ralsei: `+3`
  - Jhin: `+4`
  - Spy: `+3`
  - Scout: `+2`
  - Rocket: `+4`
  - HUNK: `+5`
  - Tinkaton: `+5`
  - Wooper: `+2`
- Added `MissionConfig` / preset-origin metadata so official missions can retain a stable source preset identity for the upcoming library/editor workflow.
- Added the read-only `MissionPresetLibraryService` layer over the built-in preset catalog.
- Added internal library access for assignable missions, official missions, legacy missions, Objective templates, and Condition templates.
- Added/expanded reusable Mission v2 runtime handlers for current official mission compositions, including:
  - `Kill`
  - `HoldItemStack`
  - `PickupItem` / no-item-pickup tracking
  - `CompleteTeleporter`
  - `ScrapItems`
  - `BombingRun`
  - `CarryEquipment`
  - `CompleteEnding`
  - `RecruitMinions`
  - `DefeatUmbraWaves`
  - `LeaveStage`
- Added reusable tracking/evaluation support for skill history and pre-damage target status so conditions can validate actions such as Wooper's poisoned Ravenous Bite requirement.
- Added global Mission v2 progress logging with milestone-based output to reduce log spam while retaining useful progress diagnostics.

### Current official challenge changes

- **Sora — Elegido de la Llave Espada**
  - Replaced the old 100-status-effect challenge with a Bulwark's Ambry / Artifact of Vengeance mission.
  - Requires Mercenary to be represented in the party, the Vengeance Umbra encounter to be completed, and the Ambry to be exited successfully.
  - Uses Shared mission progress.
- **Ralsei — El poder de la bondad**
  - Replaced the old 10,000-healing challenge with a Devotion-focused mission.
  - Requires Captain or Seeker, recruiting 3 Lemurian allies, and completing the teleporter route.
- **Jhin — El Cuarto Acto**
  - Preserves the lethal boss critical-hit concept at **44,444+ damage**.
  - Added a dedicated `JhinOfficial` preset identity so Jhin is consistent with the other official creator presets.
- **Spy — Sin que me veas venir**
  - Requires Bandit, Serrated Dagger, a valid boss backstab, and a fatal finishing hit.
  - Removed the older Smoke Bomb prerequisite.
- **Scout — Sed Termonuclear**
  - Reworked into two alternative routes:
    - Hold 8 Energy Drinks, **OR**
    - Complete the first-stage teleporter within 4 minutes without picking up items.
- **Rocket — La gravedad es opcional**
  - Reworked into 3 successful bombing runs.
  - Each bombing run requires 5 qualifying explosion kills before landing, with landing required between successful runs.
- **HUNK — La Parca No Falla**
  - Replaced the old 24-hit / 24-kill execution streak challenge.
  - Requires continuously protecting the Fuel Array and either escaping the Moon or obliterating at the Obelisk.
  - Losing the required equipment or dying invalidates the route.
- **Tinkaton — Forjada en Chatarra**
  - Reworked into a three-objective Mission v2 composition:
    - Scrap 6 items.
    - Hold Shattering Justice.
    - Defeat a valid mechanical Eye boss target.
  - Removed the old mandatory MUL-T + Blast Canister finishing-hit requirement.
- **Wooper — De vuelta al agua**
  - Reworked to require Acrid in Wetland Aspect.
  - Requires 20 kills with Ravenous Bite against enemies that were already poisoned before the qualifying bite.
  - Removed the old Bison Steak and stage-completion requirements.

### Multiplayer

- Continued the host-authoritative session mission model.
- Mission snapshots are frozen/synchronized for the active multiplayer session instead of replacing each client's personal configuration.
- Shared and PerPlayer mission scopes are represented explicitly in Mission v2.
- Improved multiplayer completion routing and session unlock delivery.
- Preserved independent personal progress for PerPlayer mission routes.

### Performance and diagnostics

- Added runtime activity planning so inactive legacy/runtime trackers can avoid unnecessary work.
- Added runtime budgets/throttling for periodic mission scans.
- Reduced repetitive progress/status/healing/kill logging.
- Added milestone-based progress output based on objective size.
- Prevented completed mission routes from continuing to emit unnecessary progress logs.
- Suppressed normal local progress/reward logging when all local profiles already own the survivor unlock, while preserving required network/session processing.
- Prevented repeated `Unlock temprano asignado` messages when the same generated unlock is revisited during ContentPack construction.
- Hardened Ralsei's Devotion recruitment detection: `LemurianHarness` is resolved through the item catalog when available and falls back to `DevotedLemurianController` when the static marker asset is unavailable.

### Preset catalog cleanup

- Normalized the internal built-in preset catalog before exposing it to players.
- The current library reports:
  - **9 assignable official missions**
  - **12 hidden legacy mission recipes**
  - **26 Objective templates**
  - **35 Condition templates**
- Legacy mission recipes remain available internally for compatibility/history but are no longer treated as new assignable missions.
- Official missions are returned as fresh copies from catalog factories so source presets are not mutated.

### Changed

- Bumped the public release version to `0.2.0`.
- Updated README documentation to match the actual current nine official mission designs.
- Updated README multiplayer examples/scopes to match the Mission v2 definitions.
- Updated HUNK attribution to the current `rob-HUNK` package and added Wooper to survivor credits.
- Clarified that Risk Of Options and the in-game mission editor are **not dependencies/features of 0.2.0**.
- Clarified that current official presets are still source-synchronized and that permanent editable copies are a future feature.

### Known validation focus

- Public testing is especially useful for balance/theme feedback across all nine official challenges.
- HUNK, Tinkaton, Sora, and Ralsei benefit from additional end-to-end testing across different mod profiles/multiplayer sessions.
- Devotion/Lemurian content can produce asset-assignment warnings in some profiles; reports should include `LogOutput.log` so compatibility issues can be separated from unrelated third-party warnings.

### Planned after 0.2.0

- Preset assignment service for assigning an official mission to another compatible detected survivor.
- In-game preset library entry point/configuration.
- Safe editable copies of official presets.
- Full Mission System v2 editor.
- User-created/shareable presets.

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
- Added an AI usage transparency notice to the README.

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

### 0.3.1 DEV — C2.1 Current Game / DLC4 compatibility audit
- Added a runtime-only compatibility audit for official `Enforcer2Body` and `AcolyteBody`.
- Confirms official survivors are not misclassified as modded content.
- Reports whether each new official survivor keeps an Original unlock.
- Does not assign Generated missions to official survivors and does not alter provider authority.

### 0.3.1 DEV — C2.4 Vanilla Unlock Coverage
- Added read-only `VanillaUnlockCoverageService`.
- Compares official survivor achievement/tracker type names against existing Mission V2 primitives.
- Reports conservative `Covered`, `LikelyCovered`, and `Unmapped` candidates.
- Does not change original unlocks, provider authority, persistence, or Generated missions.

## 0.3.1 DEV — C2.5-C2.7 Current Game Compatibility completion

- Added `VanillaUnlockSemanticInspectionService` to inspect current official Achievement/ServerTracker implementations without changing them.
- Added semantic member fingerprints for official survivor unlocks so future Mission V2 primitives are based on runtime evidence rather than class-name guesses.
- Added `OfficialContentAuthorityInvariantService` after G7/G8 persistence.
- The new authority guard verifies official/vanilla-DLC survivors receive neither Generated candidates nor persisted `Source=Generated` ownership.
- Official-content violations are reported as errors and are never silently rewritten.
- C2 implementation scope is now code-complete; one accumulated Windows/runtime pass remains to validate C2.2-C2.6 together on RoR2 1.5.0/DLC4.
