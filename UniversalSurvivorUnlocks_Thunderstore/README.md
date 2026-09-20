# Universal Survivor Unlocks

**Universal Survivor Unlocks (USU)** is a configurable progression framework for **modded survivors in Risk of Rain 2**.

USU automatically detects compatible modded survivors, normally respects characters that already provide their own unlock requirement, and can give progression challenges to survivors that would otherwise be available immediately. Curated compatibility cases can also expose a separate USU-owned alternative without modifying the creator's Original system.

**Current release:** `0.3.0`

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

## 0.3.0 release focus

Version `0.3.0` adds the first public **adaptive Generated mission pipeline** on top of the Mission Authority & Persistence foundation introduced in `0.2.2`.

Highlights:

- Compatible modded survivors with no applicable creator Original, Official USU, or player CUSTOM mission can receive a **temporary Generated mission** at runtime.
- Generated missions are built from the currently installed survivor/profile context and may differ between players with different local mod rosters. That is intentional: they are temporary fallbacks, not permanent authored presets.
- Generated content is **not persisted as CUSTOM**. If USU later gains an Official preset for that survivor, or the mod author adds an Original unlock, the authored content takes priority according to the normal provider/authority rules.
- The current generated runtime supports `Execution`, `HealingSupport`, `ResourceMastery`, `MeleeCombat`, and `ProjectileCombat` families.
- The historical `KillEnemies 100` challenge remains only as an emergency compatibility fallback; USU does not use it as a reason to replace vanilla/DLC progression or a mod author's Original unlock.
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
- Added the in-game Mission Library/provider interface. When **Risk Of Options** is installed, USU embeds its interface there; otherwise a Character Select launcher is used as fallback.
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

In `0.3.0`, when no applicable Original, Official USU, or player CUSTOM mission owns that survivor, USU can build a **temporary Generated mission** from the locally detected gameplay/profile context. Generated missions are intentionally local and temporary: two players with different installed/unlocked rosters may receive different temporary missions for the same newly installed survivor. During multiplayer, the host's effective mission remains authoritative for the session.

Generated missions do not overwrite creator content and are not saved as player CUSTOM missions. If authored content later becomes available, the normal authority/provider rules take precedence.

---

# Creator presets and custom missions

USU is moving toward a preset-based mission system with a clear ownership rule.

## Creator preset

A built-in preset created for USU is the **official source preset**.

Version `0.3.0` includes the same **10 official USU presets** and adds temporary Generated missions for otherwise unowned survivor unlock gaps. These source presets are kept separate from the reusable Objective and Condition templates used internally by Mission System v2.

Creator presets are intended to remain unchanged so future updates can improve or rebalance them without destroying the original definition.

## Player customization

When a player customizes a creator preset in a future version, the intended model is to create/use a **separate custom mission based on that preset** instead of editing the creator preset itself.

This allows USU to keep:

- The original creator preset.
- The player's customized version.
- The technical relationship between the custom mission and the preset it came from.

Version `0.3.0` exposes the current preset/provider library in-game and can also show the effective temporary Generated mission for compatible fallback targets. Official built-in presets remain source definitions and are refreshed from their stable `BasePresetId`, so editing a preset-backed snapshot directly in `Survivors.json` is not a reliable way to create a permanent custom variant. Stored custom mission data is kept separate from the official preset source. The full visual editor for creating arbitrary custom missions is still planned for a later release.

---

# Multiplayer

USU uses a **host-authoritative** model.

## Host configuration

During a multiplayer session, the **host's effective challenge configuration is the authoritative mission for that lobby/run**.

A client may have different local values or a different personal configuration, but the host determines the mission rules used during that session.

The host's session configuration is not intended to permanently overwrite the client's personal `Survivors.json`.

## Shared progress

A Shared mission can receive valid progress from multiple players/entities according to that mission's rules.

Current example:

- Sora — `Elegido de la Llave Espada`

## Per-player progress

Per-player challenges keep independent counters, inventories, route state, and other personal requirements for each player.

Current official examples include:

- Ralsei
- Jhin
- Spy
- Scout
- Rocket
- HUNK
- Tinkaton
- Wooper
- Miner

Players cannot combine partial personal progress unless the mission is explicitly configured as Shared.

## Session completion and unlock delivery

Mission progress is evaluated using the host-authoritative session snapshot. When an authoritative route completes, USU routes the survivor unlock through its multiplayer-aware session unlock system for eligible connected players.

For multiplayer bugs, logs from both host and client are especially useful.

---

# Current built-in presets

The following **10 official USU presets** are included in USU `0.3.0`.

Official challenge names/descriptions are currently localized for English and Spanish (`es-419` / `es-ES`), with English used as fallback.

## Sora — Elegido de la Llave Espada

> **Abre paso entre mundos en Baluarte de Ambry;**
> **vence a sombras y completa Venganza - Mercenary**

Current mission:

1. The party must include **Mercenary**.
2. Enter **Bulwark's Ambry** (`artifactworld`).
3. Complete the **Artifact of Vengeance** encounter by defeating the required Umbra waves.
4. Successfully leave the Ambry after the encounter.

USU currently tracks **3 Vengeance Umbra waves** and then the stage exit.

**Progress:** Shared
**Lunar Coin reward:** `+4`

---

## Ralsei — El poder de la bondad

> **Usa Devoción y reúne 3 nuevos amigos Lemurianos;**
> **completa el portal con ellos - Captain o Seeker**

Current mission:

1. Play as **Captain** or **Seeker**.
2. Use **Devotion** to recruit **3 Lemurian allies**.
3. Complete the teleporter while satisfying the mission route.

**Progress:** Per-player
**Lunar Coin reward:** `+3`

Public feedback for this preset is especially welcome because Devotion/minion behavior can be affected by game-content and mod interactions.

---

## Jhin — El Cuarto Acto

> **Convierte a un jefe en tu gran final;**
> **asesta un crítico mortal de 4.444 de daño o más.**

Current mission:

- Deliver the **fatal hit** to a boss.
- The hit must be **critical**.
- The lethal hit must deal at least **4,444 damage**.

**Progress:** Per-player
**Lunar Coin reward:** `+4`

---

## Spy — Sin que me veas venir

> **Que el jefe nunca vea venir tu golpe final;**
> **remátalo por detrás con Daga serrada - Bandit**

Current mission:

1. Play as **Bandit**.
2. Use **Serrated Dagger / Daga serrada**.
3. Deliver a valid **backstab** to a boss.
4. The backstab must be the **fatal hit**.

**Progress:** Per-player
**Lunar Coin reward:** `+3`

---

## Scout — Sed Termonuclear

> **Sacia tu sed con 8 Bebidas energéticas;**
> **o completa el primer sector sin objetos en 4 min.**

Complete **either** route:

### Route A — Drink collection

- Hold **8 Energy Drinks** on the same player during the run.

### Route B — Fast and itemless

- Complete the **first stage teleporter** within **4 minutes**.
- Do not pick up items before completing that route.

The two routes are alternatives (`OR`), not requirements that must both be completed.

**Progress:** Per-player
**Lunar Coin reward:** `+2`

---

## Rocket — La gravedad es opcional

> **Haz llover explosiones desde el cielo;**
> **derriba 5 antes de caer; haz la hazaña 3 veces.**

Current mission:

- Kill **5 enemies with qualifying explosions before landing**.
- Land after a successful bombing run.
- Repeat the successful bombing run **3 times**.
- Progress is personal to the player; owned minions are not used for this preset's bombing-run count.
- The bombing-run objective uses **stage-scoped progress**.

**Progress:** Per-player
**Lunar Coin reward:** `+4`

---

## HUNK — La Parca No Falla

> **Protege la batería y sobrevive a toda costa;**
> **escapa de la Luna o sacrifícate en el Obelisco.**

Complete **either** route while protecting the **Fuel Array**:

### Route A — Escape

- Carry the Fuel Array continuously.
- Reach and complete the **Moon escape** ending.

### Route B — Obliterate

- Carry the Fuel Array continuously.
- **Obliterate** at the Obelisk.

Important rules:

- Losing/swapping the required Fuel Array invalidates the continuous-carry requirement.
- Death invalidates the route.
- The two endings are alternatives (`OR`).

**Progress:** Per-player
**Lunar Coin reward:** `+5`

---

## Tinkaton — Forjada en Chatarra

> **Haz de 6 chatarras el inicio de tu gran golpe;**
> **ten Justicia demoledora y vence un Ojo mecánico.**

Current mission uses three objectives in the same route (`AND`):

1. Convert **6 items into Scrap** during the run.
2. Hold **Shattering Justice / Justicia demoledora**.
3. Defeat a valid mechanical Eye boss target. The current mission accepts the internal body IDs `SuperRoboBallBossBody` or `RoboBallBossBody`.

The old MUL-T + Blast Canister requirement is no longer part of the current official preset.

**Progress:** Per-player
**Lunar Coin reward:** `+5`

---

## Wooper — De vuelta al agua

> **Haz de los Humedales tu hogar; marca territorio;**
> **caza y muerde a 20 presas envenenadas - Acrid**

Current mission:

1. Play as **Acrid**.
2. Be in **Wetland Aspect / Aspecto de los Humedales** (`foggyswamp`).
3. Poison an enemy **before** the qualifying finishing action.
4. Kill that poisoned target with **Ravenous Bite / Mordida voraz**.
5. Repeat until **20** valid poisoned-bite kills are completed in the stage-scoped objective.

A target that was not already poisoned before the qualifying bite does not satisfy the status-before-action requirement.

**Progress:** Per-player
**Lunar Coin reward:** `+2`

---

## Miner — Excavación

> **Desciende a las Profundidades Abisales;**
> **vence al jefe del portal y despeja el camino.**

Current mission:

1. Reach **Abyssal Depths** (`dampcavesimple`).
2. Defeat the **real teleporter boss** in that stage with the qualifying final hit.
3. The route is evaluated by USU Mission System v2 and does not hook MinerUnearthed's `Adrenaline Rush` achievement.

This preset is deliberately **independent** from MinerUnearthed's Original unlock. The creator's Original provider remains available and is still the default until the player explicitly chooses the USU provider/preset.

**Progress:** Per-player
**Lunar Coin reward:** `+3`

---

# Challenge systems currently used by presets

The ten official presets now run through **Mission System v2 compositions** instead of being represented as nine independent monolithic challenge scripts.

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

In `0.3.0`, the authored library still contains **10 assignable official Mission v2 presets**, while compatible unowned survivor unlock gaps can also receive temporary Generated Mission v2 definitions built from reusable objectives, conditions, targets, routes, and rules.

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

The Thunderstore package for `0.3.0` declares:

- `bbepis-BepInExPack-5.4.2121`
- `RiskofThunder-R2API_Core-5.3.0`
- `RiskofThunder-R2API_ContentManagement-1.0.11`
- `RiskofThunder-R2API_Language-1.1.0`
- `RiskofThunder-R2API_Unlockable-1.0.2`
- `RiskofThunder-R2API_Networking-1.0.4`

**Risk Of Options is an optional soft integration in version `0.3.0`, not a hard Thunderstore dependency.** When it is installed, USU embeds the Mission Library inside Mod Options. Without it, USU uses its Character Select launcher fallback.

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
