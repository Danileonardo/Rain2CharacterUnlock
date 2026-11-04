# Universal Survivor Unlocks

**Universal Survivor Unlocks (USU)** is a configurable progression framework for **modded survivors in Risk of Rain 2**.

USU automatically detects compatible modded survivors, normally respects characters that already provide their own unlock requirement, and can give progression challenges to survivors that would otherwise be available immediately. Curated compatibility cases can also expose a separate USU-owned alternative without modifying the creator's Original system.

**Current source version:** `0.4.0`  
**Previous published release:** `0.3.2`

[Thunderstore](https://thunderstore.io/c/riskofrain2/p/Shleidericks/UniversalSurvivorUnlocks/) · [GitHub / Source Code](https://github.com/Danileonardo/Rain2CharacterUnlock) · [Support development / Donate](https://github.com/sponsors/Danileonardo) · Discord: `@shleiderick`

> **Support / Apoyo**
>
> Donations help me keep the project updated and continue developing new presets, compatibility fixes, multiplayer improvements, and the universal mission system.
>
> Los donativos ayudan a mantener el proyecto actualizado y a continuar desarrollando nuevos presets, correcciones de compatibilidad, mejoras multijugador y el sistema universal de misiones.
>
> Questions, bug reports, survivor requests, preset ideas, balance feedback, and suggestions are welcome on Discord.
>
> Preguntas, reportes de errores, solicitudes de personajes, ideas de presets, comentarios de balance y sugerencias son bienvenidas en Discord.

---

> ## AI Usage Notice
>
> Universal Survivor Unlocks has been developed with assistance from artificial intelligence tools during parts of the programming, design, writing, debugging, documentation, and visual-resource workflow.
>
> This notice is included for transparency so players who prefer to avoid AI-assisted content can make an informed decision before installing the mod.
>
> **Aviso sobre uso de IA:** Universal Survivor Unlocks ha sido desarrollado con asistencia de herramientas de inteligencia artificial durante parte del proceso de programación, diseño, redacción, depuración, documentación y recursos visuales.
>
> Este aviso se incluye por transparencia para que quienes prefieran evitar contenido desarrollado con estas herramientas puedan decidir antes de instalar el mod.

---

# What does USU do?

USU adds progression to compatible modded survivors without replacing the systems created by other survivor authors.

Main goals:

- Automatically detect installed modded survivors.
- Respect survivors that already have their own native unlock requirements.
- Lock compatible survivors when USU assigns them a challenge.
- Unlock survivors through gameplay instead of making every compatible character immediately available.
- Provide built-in creator-made challenge presets.
- Store survivor and challenge information in `Survivors.json`.
- Preserve survivor configuration when character mods are temporarily disabled or uninstalled.
- Support host-authoritative multiplayer.
- Support both shared-progress and per-player challenge designs.
- Keep vanilla and official DLC progression untouched.
- Use a reusable Mission System v2 instead of requiring a unique hardcoded tracker for every future challenge.
- Localize the official challenge names/descriptions according to the game's active language.
- Reward official USU challenges with configurable Lunar Coin rewards.

## 0.4.0 development focus — New official missions and unlock modes

Version `0.4.0` is **under development and awaiting gameplay/multiplayer QA**. This section documents the source currently prepared for the next release, not a published or game-validated build.

- **18 official survivor missions** are currently registered: 6 Individual, 7 Cooperative and 5 Mixed. These three unlock modalities describe reward eligibility; they must not be confused with a historical preset's `ProgressScope` label.
- HUNK has two legitimate unlock routes while continuously carrying the **Volatile Battery** (`QuestVolatileBattery`): escape the Moon or obliterate at the Obelisk. Both yield the same survivor unlock. Only when a new unlock is granted, a localized narrative **medal near the bottom of the screen** distinguishes "Mission accomplished" from "At what cost?"; the normal achievement popup and reward remain unchanged.
- Sora requires **3 grouped Vengeance Umbra waves** with a Mercenary present. No specific `artifactworld` scene is required; the handler groups detected Umbra kills into waves based on party size. Full-wave detection still needs actual gameplay validation.
- Ralsei requires 3 Devotion Lemurians recruited by the party and a completed teleporter, without a Captain/Seeker-only performer requirement or a requirement that the Lemurians remain alive.
- Rocket's 3 successful airborne bombing runs accumulate during a **single game run**, even across stages; incomplete streaks still reset when their encounter ends.
- AH-64's persistent campaign requires 64 minutes of flight, 6,400 airborne shots, 640 airborne missiles/rockets, 64 airborne teleporter-boss kills and an authorized Monsoon **Moon escape** after the campaign goals.
- Source, package manifest and assembly metadata move to `0.4.0`; `Survivors.json` schema remains **v2**. Existing Original / USU / Personalizada / Generada provider data should not be reset for a version bump.

**Pending before release:** C# compilation in the user's build environment, all 18 mission outcomes (including negative tests), the HUNK medal's visibility after ending transitions, Sora wave counting with different party sizes, host/client delivery and persistence, and a final Thunderstore release package. Third-party mod initialization failures remain external to USU.

## 0.3.2 release focus — Multiplayer unlock reliability

Version `0.3.2` focuses on multiplayer unlock notifications and safe handling of already-unlocked characters. The shared Mission Library and its AH-64-inspired layout were introduced in `0.3.1` and are retained here.

- Fixed cases where dynamically generated achievement identifiers appeared in chat instead of readable achievement names when host and client had different local mission settings.
- Added localized unlockable names for English and Spanish to prevent internal `USU_..._UNLOCKABLE_NAME` keys from leaking into native notifications.
- Avoids awarding a new USU achievement or duplicate Lunar Coin rewards to players who already unlocked the survivor using USU or the original survivor mod.
- Preserves host-authoritative mission selection, delivery to eligible connected players, profile saving, and existing Original/USU/Custom/Generated provider behavior.
- A player who dies during a cooperative run can still receive a team-earned unlock. Testing of the popup while spectating is ongoing; no additional death-popup fix is claimed for this release.
- Third-party survivors that fail to load in the current game build are not fixed by USU; their availability depends on the respective mod authors.

## 0.3.1 release focus — Universal Survivor Mission Library

Version `0.3.1` adds a shared, more readable Mission Library layout inspired by AH-64's panel, while preserving each survivor's own content and unlock ownership.

Highlights:

- Shared visual preset for survivor information: portrait, unlock state, provider selection, active mission, overview and Logbook/Notes, without replacing original lore or achievements.
- Provider selection supports **Original / USU / Personalizada / Generada** with the existing persisted-slot and authority rules.
- AH-64 receives **Protocol 64: Air Supremacy**, an experimental cumulative campaign with four tracked goals and a final Monsoon escape. Its campaign cards, progress bars and final operation remain AH-64 specific; they are not shown for other survivors without real tracked objectives.
- Refined text size, scrolling and the compact mission/campaign/notes presentation within Risk Of Options.
- Continues the work on Generated mission controls, provider coexistence, curated official presets, localization and compatibility with additional modded survivors.
- Requires **Risk Of Options** and **ScrollableLobbyUI** in the Thunderstore dependency list; SurvivorSortOrder is listed as a package dependency for roster sorting, not as a hard BepInEx DLL dependency.

**Testing note:** visual improvements and gameplay changes remain subject to compatibility and multiplayer feedback, including AH-64 campaign counters and long-text layouts for individual survivors. No universal multiplayer persistence guarantee is made for the AH-64 campaign.

## 0.3.0 release focus

Version `0.3.0` adds the first public **adaptive Generated mission pipeline** on top of the Mission Authority & Persistence foundation introduced in `0.2.2`.

Highlights:

- Compatible modded survivors with no applicable creator Original, Official USU, or player CUSTOM mission receive a **Generated Mission v2** fallback.
- Generated missions are built from the currently installed survivor/profile context and may differ between players with different local mod rosters. They are persisted in `Survivors.json` with `source = Generated`, but remain managed fallbacks rather than authored presets.
- Editing a Generated mission promotes the edited copy to **CUSTOM**. If USU later gains an Official preset for that survivor, or the mod author adds an Original unlock, authored content keeps normal provider/authority priority.
- The current generated runtime supports `Execution`, `HealingSupport`, `ResourceMastery`, `MeleeCombat`, and `ProjectileCombat` families.
- The historical automatic `KillEnemies 100` fallback is migrated away and is no longer created for newly detected survivors. `KillEnemies` remains available as a legitimate mission objective for authored/custom content.
- Generated missions use the same Mission v2/session architecture as other USU missions, including host-authoritative lobby snapshots.
- Original provider presentation is explicitly preserved: USU does not rewrite EFFECTIVE achievement text while a creator-owned Original provider is active.
- Miner remains the curated exception where the player can explicitly choose the creator's Original unlock or USU's independent `Excavación / Excavation` alternative.

Current runtime validation on the development profile reached **7 Generated candidates → 7 materialized**, with no unsupported or invalid Generated materializations. Client/run multiplayer validation continues as part of the public release test cycle.

## 0.2.2 release focus

Version `0.2.2` establishes the current **Mission Authority & Persistence Foundation**.

Highlights:

- The canonical library now contains **10 official USU missions** while keeping the **12 historical Legacy recipes** hidden and reusable for compatibility and future CUSTOM/editor/generator work.
- Survivors now receive a stable USU identity derived from their content source and Body, allowing mission ownership to survive temporary mod removal.
- `Survivors.json` uses **schema v2** and stores absent survivor missions in `dormantMissions` with `PendingContent` instead of keeping a stale runtime character snapshot.
- Reinstalling a survivor mod rebuilds its runtime/profile data from the real installed content and rehydrates the persisted mission/provider selection from its stable identity.
- Explicit player choices such as `UserSelected`, provider selection, official preset relationship, and stored CUSTOM mission data are preserved across the dormant/reinstall flow.
- Public builds keep development-only preset `AuthoringMode` disabled. Official assigned preset snapshots are refreshed through their stable `BasePresetId` relationship instead.

Miner also receives the official USU preset **Excavación / Excavation**:

- Reach **Abyssal Depths**.
- Defeat the **teleporter boss** there.
- The mission is owned and evaluated by USU and can be selected explicitly from the Mission Library.
- MinerUnearthed's original `Adrenaline Rush` achievement is **not patched, intercepted, rewritten, or deleted**.
- Miner stays on the creator's Original provider by default; the Original provider remains stored and can always be selected/restored.
- If MinerUnearthed changes or fixes its native unlock later, USU's Miner preset remains independent.

## 0.2.1 release focus

Version `0.2.1` closes the current **survivor presentation/localization pass** while keeping the nine creator-made unlock challenges on Mission System v2.

Highlights of this release:

- Added a curated `SurvivorDefinition` layer for **15 survivor integrations**: Aurelion Sol, Auriel, Enforcer, Ralsei, Sora, Wooper, Tinkaton, HUNK, Jhin, Miner, Rocket, Scout, Spy, Heretic, and Nemesis Enforcer.
- Added curated Spanish localization (`es-419` / `es-ES`) and English repairs for visible modded-survivor content when the original mod does not provide the required text.
- Added survivor content profiles for overview/details, skills, keywords, skins, unlock information, and Logbook-facing lore/Notes.
- Kept `Notes` tied to the real native Logbook state instead of creating a separate USU discovery flag.
- Added the in-game Mission Library/provider interface. **Risk Of Options 2.8.6+ is now required** and USU embeds its interface directly inside Mod Options. The legacy Character Select launcher remains only as a defensive fallback if the dependency fails to load.
- Added provider selection/restoration flows for `Original`, `USU`, and stored `Custom` configuration state, while protecting native creator unlock systems from unintended replacement.
- Added a runtime content catalog used by the survivor browser and future mission-editor work.
- Added quieter default logging with optional verbose diagnostics through BepInEx config.

The full visual mission editor and general-purpose custom mission authoring workflow are **not part of 0.2.1 yet**. The current release focuses on stable survivor presentation, preset assignment/provider handling, localization, and runtime foundations.

---

# Important behavior

## Vanilla and DLC survivors

USU does **not** replace or interfere with the normal unlock requirements of vanilla or official DLC survivors.

Characters such as Commando, Huntress, Loader, Railgunner, and other official survivors keep their normal Risk of Rain 2 progression.

## Modded survivors with their own unlock system

If a modded survivor already defines its own unlockable/progression requirement, USU normally respects it and leaves that survivor under the control of the original mod.

A curated compatibility entry may expose a **separate USU-owned alternative** without changing the creator's achievement. Miner is the first such case in `0.2.2`: **Original remains the default**, while the player may explicitly switch to USU's `Excavación` preset.

## Modded survivors without an unlock system

Compatible survivors without their own unlock requirement can receive a USU challenge and start locked until that challenge is completed.

In `0.3.0`, when no applicable Original, Official USU, or player CUSTOM mission owns that survivor, USU builds a **Generated Mission v2** from the locally detected gameplay/profile context and stores it in `Survivors.json` as `source = Generated`. Two players with different installed/unlocked rosters may receive different Generated missions. During multiplayer, the host's effective mission remains authoritative for the session.

Generated missions do not overwrite creator content. They use the same JSON/Mission v2 model as Custom; editing one turns the edited copy into `source = Custom`, while an untouched Generated mission remains automatically refreshable by USU. Generated missions can be rerolled directly from Mission Library with the dice control; deleting their JSON entry (or the whole `Survivors.json`) is kept only as a recovery path that creates a fresh generation nonce on the next startup.

---

# Creator presets and custom missions

USU is moving toward a preset-based mission system with a clear ownership rule.

## Creator preset

A built-in preset created for USU is the **official source preset**.

Version `0.3.0` includes the same **10 official USU presets** and adds persisted, auto-managed Generated missions for otherwise unowned survivor unlock gaps. These source presets are kept separate from the reusable Objective and Condition templates used internally by Mission System v2.

Creator presets are intended to remain unchanged so future updates can improve or rebalance them without destroying the original definition.

## Player customization

When a player customizes a creator preset in a future version, the intended model is to create/use a **separate custom mission based on that preset** instead of editing the creator preset itself.

This allows USU to keep:

- The original creator preset.
- The player's customized version.
- The technical relationship between the custom mission and the preset it came from.

Version `0.3.0` exposes the current preset/provider library in-game and can also show the effective Generated mission for compatible fallback targets. Official built-in presets remain source definitions and are refreshed from their stable `BasePresetId`, so editing a preset-backed snapshot directly in `Survivors.json` is not a reliable way to create a permanent custom variant. Stored custom mission data is kept separate from the official preset source. The full visual editor for creating arbitrary custom missions is still planned for a later release.

---

# Multiplayer

USU uses a **host-authoritative** model.

## Host configuration

During a multiplayer session, the **host's effective challenge configuration is the authoritative mission for that lobby/run**.

A client may have different local values or a different personal configuration, but the host determines the mission rules used during that session.

The host's session configuration is not intended to permanently overwrite the client's personal `Survivors.json`.

## Unlock modalities and progress

The active built-in mission catalog distinguishes **three unlock modes**, which control *who receives the reward*:

- **Individual (6 missions):** Jhin, Scout, Rocket, Banshee, Wanderer, HEL-P. The player must satisfy the objective personally; other participants do not inherit their completion.
- **Cooperative (7 missions):** Spy, HUNK, Ralsei, Sora, Miner, Warlock, Tinkaton. The session can satisfy the required conditions together, and eligible participants receive the unlock through the host's session mechanism.
- **Mixed (5 missions):** Gwen, Kindred, Wooper, Seamstress, AH-64. Mission counters can be shared, but rewards require the participation criteria tracked for each eligible player; a passive spectator must not receive an unearned reward.

The host selects the effective mission in multiplayer. Session progression and unlock eligibility are separate mechanisms: legacy `Shared` / `PerPlayer` labels in earlier release notes do **not** override a mission's current explicit unlock mode. The host's mission is not meant to overwrite the client's saved provider preferences permanently.

## Session completion and unlock delivery

Mission progress is evaluated using the host-authoritative session snapshot. When an authoritative route completes, USU routes the survivor unlock through its multiplayer-aware session unlock system for eligible connected players.

For multiplayer bugs, logs from both host and client are especially useful.

---

# Current built-in official missions — 0.4.0

The current source registers **18 official USU missions**. Missions are grouped by their actual 0.4.0 unlock modality (Individual / Cooperative / Mixed). The game keeps the mod creator's Original unlock separate; choosing an official USU mission must not overwrite it. Names and descriptions have localized English/Spanish support where provided by the existing Definitions.

| Mode | Survivor and official mission | Required achievement in the current code |
|---|---|---|
| Individual | **Jhin — El Cuarto Acto** | Deliver a fatal critical hit of **4,444+** damage to a valid teleporter boss; Railgunner is excluded. |
| Individual | **Scout — Sed Termonuclear** | Either collect **8 Energy Drinks** or finish the first teleporter itemless within **4 minutes**. |
| Individual | **Rocket — La gravedad es opcional** | Complete **3** bombing runs of **5 airborne explosive kills** each, landing between successful runs; finished rounds persist across stages of one run. |
| Individual | **Banshee — Presagio desde las alturas** | Kill **20** enemies while airborne, then land the qualifying final hit on a teleporter boss from the air. |
| Individual | **Wanderer — Último aliento** | Deliver the final hit to **3 teleporter bosses while airborne** in one run. |
| Individual | **HEL-P — Protocolo HEL-P: Reconstrucción** | Scrap **2 healing drones** and **2 gunner drones**, repair **1 emergency drone** and activate its healing. |
| Cooperative | **Spy — Sin que me veas venir** | With Bandit, land the killing **Serrated Dagger backstab** on a boss. |
| Cooperative | **HUNK — La Parca No Falla** | Carry the **Volatile Battery** continuously and either **escape the Moon** or **obliterate at the Obelisk**; the carrier's death or loss of equipment invalidates that carrying condition. |
| Cooperative | **Ralsei — El poder de la bondad** | Recruit **3 Devotion Lemurians** and complete the teleporter; any survivor may contribute and recruited Lemurians do not need to survive. |
| Cooperative | **Sora — Elegido de la Llave Espada** | Defeat **3 grouped Vengeance Umbra waves** with Mercenary in the party. Umbrae can count in all Ambry variants or outside Ambry if Vengeance is active. Wave grouping requires in-game validation. |
| Cooperative | **Miner — Excavación** | Deliver the final hit to the teleporter boss in **Abyssal Depths**. |
| Cooperative | **Warlock — Tributo al trono** | Kill **10 imps**, then defeat an **Imp Overlord**. |
| Cooperative | **Tinkaton — Forjada en Chatarra** | Scrap **6** items, obtain **Shattering Justice**, and defeat a qualifying mechanical Eye boss. |
| Mixed | **Gwen — Entre hilos y agujas** | Obtain **2 Ceremonial Daggers** and defeat a teleporter boss while the condition applies. |
| Mixed | **Kindred — La marca del destino** | Hold **Death Mark** and defeat **4 distinct boss types** in one run. |
| Mixed | **Wooper — De vuelta al agua** | With Acrid in **Wetland Aspect**, kill **20 already-poisoned enemies** using Ravenous Bite. |
| Mixed | **Seamstress — Hilo de sangre** | Make **3 sacrifices at the same Shrine of Blood**, then defeat a teleporter boss. |
| Mixed | **AH-64 — Protocolo 64: Dominio aéreo** | Complete the persistent campaign (**64** flying minutes, **6,400** airborne shots, **640** missiles/rockets, **64** airborne teleporter-boss kills), then escape the **Moon on Monsoon**. |

### HUNK's optional operation medals (introduced in 0.3.3)

HUNK's **normal native unlock notification remains intact**. On a genuine new unlock only, a secondary lower-screen report appears for approximately 5.5 seconds:

- **Moon extraction:** "Misión cumplida" — "Objetivo completado. Extracción exitosa."
- **Obelisk sacrifice:** "¿A qué costo?" — "Objetivo cumplido. El operativo no regresó."

These medals are **presentational only**: no extra achievement, reward or alternative unlock is created. Their appearance during the end-of-run transition has not been verified in-game yet.

### Current validation status

The 18 mission presets are implemented in source, but **not all have been validated in real multiplayer games**. Especially verify Sora's wave grouping, HUNK's two endings and operation medals, AH-64's campaign ending detection, and per-player participation / unlock ownership before publishing `0.4.0`.

---

# Challenge systems currently used by presets

The 18 official presets run through **Mission System v2 compositions**, including dedicated runtime trackers where needed, instead of independent monolithic challenge scripts.

Current runtime building blocks used by the official missions include objectives such as:

- `Kill`
- `HoldItemStack`
- `CompleteTeleporter`
- `BombingRun`
- `CarryEquipment`
- `CompleteEnding`
- `ScrapItems`
- `RecruitMinions`
- `DefeatUmbraWaves`
- `LeaveStage`
- Item-pickup / no-item-pickup tracking

And conditions/modifiers such as:

- Boss / specific-body targets
- `CriticalHit`
- `FatalHit`
- `MinimumDamage`
- `Backstab`
- `RequiredSurvivor`
- `RequiredSkill`
- `RequiredStage`
- `StageSequence`
- `TimeLimit`
- `NoItemPickup`
- `StatusPresent` with before-action timing
- Party-survivor requirements

Legacy challenge trackers remain in the project where they are still useful for compatibility/event sources, but the current creator missions are defined through the reusable Mission v2 model.

---

# Mission System v2 foundation

Version `0.3.0` keeps Mission System v2 as the active foundation and extends it with adaptive Generated missions, while retaining centralized mission authority, stable survivor identity, and dormant mission-only persistence.

The model is built around:

- `MissionDefinition`
- `MissionRoute`
- `MissionObjective`
- `MissionCondition`
- `MissionRules`
- `MissionTarget`
- `MissionPreset`
- Preset/custom mission source tracking through `MissionConfig`
- `Shared` and `PerPlayer` progress scopes
- Host-authoritative session snapshots
- Multiple alternative routes

## Route logic

Objectives/conditions inside one route are treated as **AND** requirements.

Different routes are treated as **OR** alternatives.

Conceptually:

```text
Route A:
Objective
AND Objective
AND Condition

OR

Route B:
Objective
AND Condition
```

This is already used by current presets such as Scout and HUNK.

## Target system

The mission model supports reusable target categories such as:

- Any target.
- Enemy.
- Elite.
- Boss.
- Specific body.
- Specific boss/body combinations.

Specific entities use stable internal IDs instead of localized display names.

Examples:

```text
BrotherBody
SuperRoboBallBossBody
RoboBallBossBody
```

This allows mission logic to remain independent from the player's selected language.

## Current v2 runtime status

In `0.3.0`, the authored library still contains **10 assignable official Mission v2 presets**, while compatible unowned survivor unlock gaps can also receive Generated Mission v2 definitions persisted with `source = Generated` built from reusable objectives, conditions, targets, routes, and rules.

The internal catalog also retains hidden legacy recipes and reusable Objective/Condition templates for compatibility and future editing workflows.

The public Mission Library can now browse survivor profiles and assign compatible USU presets. Provider handling distinguishes `Original`, `USU`, and stored `Custom` state, and runtime refresh support applies valid assignment changes without requiring USU to overwrite the creator's native unlock system.

The full drag-and-drop/general-purpose mission editor and unrestricted custom mission authoring UI are still planned follow-up features.

---

# Configuration

USU stores detected survivor and challenge information in:

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

When using Thunderstore Mod Manager or r2modman, this file is inside the selected profile.

USU can also maintain backup configuration data so survivor information is not lost unnecessarily when configuration is synchronized.

## Important

`Survivors.json` is a persistence/configuration layer.

Metadata identifying a survivor is maintained by USU. Avoid changing internal survivor IDs unless you know exactly what the game/mod uses.

Because the mission format contains nested routes/objectives/conditions, making a backup before manually editing complex challenge data is recommended.

## Built-in preset synchronization in 0.3.0

Official creator presets remain immutable source definitions. `BasePresetId` is treated as the authority for preset-backed assignments, and USU refreshes the persisted mission snapshot when the corresponding built-in preset changes between releases.

This means manual edits to the snapshot of a known official preset inside `Survivors.json` can be replaced during synchronization. Player-owned `CustomMission` data is stored separately and is not the same thing as editing the creator preset itself.

Using **Restore original** returns the character to its base provider policy and removes the player's stored Custom copy for that character, while never deleting or modifying the creator's native unlock implementation or the built-in USU source preset.

---

# Persistence

USU stores its progression/configuration separately from the survivor mod itself.

Starting with `0.2.2`, each known survivor can carry a stable USU identity based on its content source and Body. `Survivors.json` uses schema v2 and separates physically absent content into the `dormantMissions` collection.

When a survivor mod is temporarily disabled or uninstalled, USU keeps only the mission-facing data required to restore that configuration later:

- Stable survivor identity.
- Body and content Source.
- Mission/challenge configuration.
- Provider and SelectionMode.
- `BasePresetId` relationship.
- Stored CUSTOM mission data when present.

USU does **not** treat old runtime data such as skills, skins, prefabs, lore, stats, Unity objects, or a previous ContentProfile as persistent truth while the mod is absent. The dormant record is marked `PendingContent`.

When the survivor mod returns, USU detects the real current content again, rebuilds the runtime/profile information from that installed mod, rehydrates the persisted mission by stable identity, and removes the redundant dormant record.

Mission session progress is run/session state and is not intended to replace persistent unlock/profile progression.

---

# Locked survivor presentation

For survivors locked by USU, the mod integrates the lock into the character selection experience.

The intended presentation includes:

- Darkened/locked survivor portrait.
- Vanilla-like locked framing.
- Challenge information on hover.
- Vanilla-style unlock notification.
- Chat unlock announcement.
- Normal full-color presentation after unlocking.

Official survivors and survivors controlled by another mod's own unlock system are not given USU's generated lock presentation.

---

# Installation

## Thunderstore Mod Manager / r2modman

1. Install **Universal Survivor Unlocks**.
2. Install it **with dependencies**.
3. Install any compatible modded survivors you want to use.
4. Launch Risk of Rain 2.
5. USU will detect/reconcile compatible survivors and configuration automatically.

The survivor mods referenced by USU presets are **not hard dependencies** of Universal Survivor Unlocks.

## Manual installation

Manual installation is intended for users already familiar with BepInEx.

Place the USU plugin in the appropriate BepInEx plugin directory and install every dependency listed in the package `manifest.json`.

Recommended plugin path:

```text
BepInEx/plugins/UniversalSurvivorUnlocks/UniversalSurvivorUnlocks.dll
```

Using a mod manager is recommended.

---

# Dependencies

For the pending `0.4.0` build, use the current `manifest.json` as the authoritative dependency list. New required package entries include Risk Of Options, ScrollableLobbyUI and SurvivorSortOrder (sort order is a package dependency only).

For reference, the earlier Thunderstore package for `0.3.0` declared:

- `bbepis-BepInExPack-5.4.2121`
- `RiskofThunder-R2API_Core-5.3.0`
- `RiskofThunder-R2API_ContentManagement-1.0.11`
- `RiskofThunder-R2API_Language-1.1.0`
- `RiskofThunder-R2API_Unlockable-1.0.2`
- `RiskofThunder-R2API_Networking-1.0.4`

**Risk Of Options is a required dependency in `0.3.1`.** Thunderstore/r2modman will install `Rune580-Risk_Of_Options-2.8.6` together with USU. The Mission Library is embedded inside Mod Options; the old Character Select launcher remains only as a defensive fallback if Risk Of Options fails to initialize.

Always treat the `manifest.json` included with the current release as the authoritative dependency list.

---

# Compatibility

USU is designed to coexist with:

- Vanilla survivor progression.
- Official DLC survivor progression.
- Modded survivors with native unlock systems.
- Modded survivors without native unlock systems.
- Multiplayer sessions using compatible mod profiles.
- RealerCheatUnlocks/manual re-locking workflows used during testing.

Risk of Rain 2 mods can modify skills, networking, damage behavior, content loading, unlockables, inventory state, and UI in many different ways, so compatibility with every third-party mod cannot be guaranteed.

If another mod changes a skill, boss body, item, stage, artifact mechanic, or damage rule used by a USU preset, that challenge may require a compatibility adjustment.

---

# Reporting bugs / requesting presets

The most useful bug report includes:

- Universal Survivor Unlocks version.
- Risk of Rain 2 version.
- Whether the problem happened as **host** or **client**.
- Survivor being unlocked.
- Survivor used to attempt the challenge.
- Challenge/preset being attempted.
- Relevant mod list/profile code when possible.
- What happened.
- What you expected to happen.
- `LogOutput.log` from the affected session.

For multiplayer issues, logs from both host and client are especially useful.

For `0.3.0`, reports about Generated missions, provider switching, Original preservation, dormant/reinstalled survivor persistence, the Mission Library, Logbook/Notes behavior, and multiplayer challenge completion are especially valuable.

You can use:

- **Discord / Direct contact:** `@shleiderick`
  - Original Discord alias: `Shleider#3336`
- **GitHub:** https://github.com/Danileonardo/Rain2CharacterUnlock

You can contact me on Discord for:

- Questions.
- Survivor requests.
- Challenge/preset ideas.
- Balance feedback.
- Suggestions.
- Compatibility reports.
- General project discussion.

---

# Supporting development / Donativos

Universal Survivor Unlocks is free.

If you enjoy the project and want to support continued development, you can use GitHub Sponsors:

**https://github.com/sponsors/Danileonardo**

Donations help support continued work on:

- New survivor presets.
- Multiplayer validation.
- Compatibility fixes.
- Mission System v2.
- The future preset library.
- The future in-game mission editor.
- Configuration tools.
- Documentation.
- Localization.
- Maintenance after Risk of Rain 2 updates.

Donations are optional and do not grant gameplay advantages or affect unlock requirements.

> **Español:** Los donativos son completamente opcionales. Ayudan a mantener el proyecto actualizado y a continuar desarrollando presets, compatibilidad, multijugador y el sistema universal de misiones. No entregan ventajas dentro del juego ni cambian los requisitos de desbloqueo.

---

# Survivor mod credits

Universal Survivor Unlocks does **not** own the third-party survivors used by its built-in presets.

Those characters, assets, survivor implementations, and original mod projects belong to their respective authors and rights holders.

Built-in USU presets currently reference/test against projects including:

- **Sora** — package by **Dragonyck**
  https://thunderstore.io/c/riskofrain2/p/Dragonyck/Sora/

- **RalseiSurvivor** — package by **GodRayProductions** and its credited contributors
  https://thunderstore.io/c/riskofrain2/p/GodRayProductions/RalseiSurvivor/

- **Jhin** — package by **SeroRonin**
  https://thunderstore.io/c/riskofrain2/p/SeroRonin/Jhin/

- **Spy** — package by **tsuyoikenko**
  https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Spy/

- **Scout** — package by **tsuyoikenko**
  https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Scout/

- **Rocket** — package by **EnforcerGang**
  https://thunderstore.io/c/riskofrain2/p/EnforcerGang/Rocket/

- **HUNK** — package by **rob** and its credited contributors
  https://thunderstore.io/c/riskofrain2/p/rob/HUNK/

- **Tinkaton** — package by **DragonycksModdingComms**
  https://thunderstore.io/c/riskofrain2/p/DragonycksModdingComms/Tinkaton/

- **Wooper** — package by **DragonycksModdingComms**
  https://thunderstore.io/c/riskofrain2/p/DragonycksModdingComms/Wooper/

These survivor packages are referenced for compatibility/preset design and are **not automatically dependencies** of USU.

Please support the original survivor mod authors as well.

---

# General credits

**Universal Survivor Unlocks** is an independent Risk of Rain 2 modding project.

- Risk of Rain 2, its original characters, items, skills, and assets belong to their respective owners.
- Third-party characters and franchises belong to their respective rights holders.
- Third-party survivor mods belong to their respective mod authors/contributors.
- USU does not claim ownership over those characters or their original assets.

---

# AI Transparency

USU uses AI-assisted tools as part of its development workflow.

This may include assistance with:

- Programming.
- Debugging.
- Refactoring.
- Challenge design.
- Documentation.
- Writing.
- Visual-resource ideation or generation.

Final implementation decisions, testing, balancing, packaging, project direction, and release decisions remain part of a human-led development process.

This disclosure is intentionally visible so users can decide for themselves whether they are comfortable installing AI-assisted content.

---

# License

Universal Survivor Unlocks is distributed under the **MIT License**.

See the included `LICENSE` file for the complete license terms.

---

# Project direction

USU is evolving from a collection of individual unlock scripts into a general-purpose configurable progression framework for modded survivors.

With the adaptive Generated mission foundation in `0.3.0`, future development can build on:

- Additional Generated template families and richer objective/condition coverage.
- Better revalidation when survivor mods change, without silently overwriting player-owned CUSTOM missions.
- A full visual mission editor and safer custom-mission authoring workflow.
- Preset sharing/importing and additional curated survivor integrations.
- More detailed multiplayer client/run validation and compatibility hardening.
- Continued localization, Mission Library, Logbook/Notes, and presentation improvements.

Generated missions remain a fallback layer. Authored Original, Official USU, and player-owned CUSTOM content remain separate concepts and are not silently replaced by the generator.

---

# Links

**Thunderstore**
https://thunderstore.io/c/riskofrain2/p/Shleidericks/UniversalSurvivorUnlocks/

**Source / Issues**
https://github.com/Danileonardo/Rain2CharacterUnlock

**Support development / Donate**
https://github.com/sponsors/Danileonardo

**Discord / Direct Contact**
Current username: `@shleiderick`
Original alias: `Shleider#3336`

> **Thank you for playing and helping test Universal Survivor Unlocks.**
>
> **Gracias por jugar y ayudar a probar Universal Survivor Unlocks.**


### Generated reroll

Las misiones administradas como **Generated** muestran un dado en Mission Library. El dado vuelve a calcular la misión, actualiza `Survivors.json` y refresca su nombre/descripción inmediatamente. Editar la misión sigue convirtiéndola en **Custom**; el reroll no.
