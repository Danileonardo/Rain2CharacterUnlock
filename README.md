# Universal Survivor Unlocks

**Universal Survivor Unlocks (USU)** is a configurable progression framework for **modded survivors in Risk of Rain 2**.

USU automatically detects compatible modded survivors, respects characters that already provide their own unlock requirement, and can give progression challenges to survivors that would otherwise be available immediately.

**Current release:** `0.2.0`

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

## 0.2.0 feedback focus

Version `0.2.0` is intentionally focused on the **nine current creator-made survivor challenges**.

The in-game preset library, preset reassignment UI, editable copies, and full mission editor are **not included yet**. The internal architecture for those systems is already being prepared, but this release is meant to gather feedback about the current characters and their missions before the editor becomes public.

Useful feedback includes:

- Does the challenge fit the character thematically?
- Is the objective understandable without reading the source code?
- Is it too easy, too difficult, too long, or too short?
- Did the challenge behave correctly in multiplayer?
- Does the Lunar Coin reward feel appropriate?
- Would you change any objective, route, number, or requirement?

---

# Important behavior

## Vanilla and DLC survivors

USU does **not** replace or interfere with the normal unlock requirements of vanilla or official DLC survivors.

Characters such as Commando, Huntress, Loader, Railgunner, and other official survivors keep their normal Risk of Rain 2 progression.

## Modded survivors with their own unlock system

If a modded survivor already defines its own unlockable/progression requirement, USU respects it and leaves that survivor under the control of the original mod.

## Modded survivors without an unlock system

Compatible survivors without their own unlock requirement can receive a USU challenge and start locked until that challenge is completed.

---

# Creator presets and custom missions

USU is moving toward a preset-based mission system with a clear ownership rule.

## Creator preset

A built-in preset created for USU is the **official source preset**.

Version `0.2.0` includes **9 official creator presets**. These source presets are kept separate from the reusable Objective and Condition templates used internally by Mission System v2.

Creator presets are intended to remain unchanged so future updates can improve or rebalance them without destroying the original definition.

## Player customization

When a player customizes a creator preset in a future version, the intended model is to create/use a **separate custom mission based on that preset** instead of editing the creator preset itself.

This allows USU to keep:

- The original creator preset.
- The player's customized version.
- The technical relationship between the custom mission and the preset it came from.

The public in-game preset library/editor is still under development. In `0.2.0`, official built-in presets are still synchronized from the mod's source definitions, so directly editing one of the nine official presets in `Survivors.json` is not a reliable way to create a permanent custom variant.

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

Players cannot combine partial personal progress unless the mission is explicitly configured as Shared.

## Session completion and unlock delivery

Mission progress is evaluated using the host-authoritative session snapshot. When an authoritative route completes, USU routes the survivor unlock through its multiplayer-aware session unlock system for eligible connected players.

For multiplayer bugs, logs from both host and client are especially useful.

---

# Current built-in presets

The following **9 creator-made presets** are included in USU `0.2.0`.

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
> **asesta un crítico mortal de 44.444 de daño o más.**

Current mission:

- Deliver the **fatal hit** to a boss.
- The hit must be **critical**.
- The lethal hit must deal at least **44,444 damage**.

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

# Challenge systems currently used by presets

The nine official presets now run through **Mission System v2 compositions** instead of being represented as nine independent monolithic challenge scripts.

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

Version `0.2.0` substantially expands the next-generation mission format introduced in earlier releases.

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

At the `0.2.0` release point, the internal preset library is normalized into:

- **9 assignable official mission presets**.
- **12 legacy mission recipes kept hidden for compatibility/history**.
- **26 Objective templates** prepared for reuse.
- **35 Condition templates** prepared for reuse.

The nine official creator presets are already composed from Mission v2 objectives/conditions/routes.

The public **preset reassignment UI**, **editable-copy workflow**, and **full in-game mission editor** are not exposed yet. These are planned follow-up features, not advertised as completed functionality in `0.2.0`.

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

## Built-in preset synchronization in 0.2.0

The nine current creator presets are still treated as source definitions by the current synchronization layer.

This means direct edits to a known built-in preset inside `Survivors.json` may be replaced by the creator preset when USU synchronizes configuration.

This is intentional for this release. The upcoming custom-copy workflow is designed so players can modify a separate personal copy while the creator preset remains intact.

---

# Persistence

USU stores its progression/configuration separately from the survivor mod itself.

This allows the project to remember information about a modded survivor when that character mod is temporarily removed and later installed again.

USU reconciles its detected survivor catalog with the stored configuration during startup.

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

The Thunderstore package for `0.2.0` declares:

- `bbepis-BepInExPack-5.4.2121`
- `RiskofThunder-R2API_Core-5.3.0`
- `RiskofThunder-R2API_ContentManagement-1.0.11`
- `RiskofThunder-R2API_Language-1.1.0`
- `RiskofThunder-R2API_Unlockable-1.0.2`
- `RiskofThunder-R2API_Networking-1.0.4`

**Risk Of Options is not a dependency of version `0.2.0`.** The planned in-game configuration entry/library has not been released yet.

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

For `0.2.0`, balance/theme feedback on the nine creator challenges is also especially valuable.

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

Current next-step goals include:

- Assigning any compatible official preset to another detected survivor.
- A reusable creator-preset library exposed in-game.
- Safe player-created copies/custom missions.
- An in-game mission editor.
- Reusable objectives and conditions.
- Configurable enemy, elite, boss, item, skill, stage, status, and specific-body targets.
- Multi-route challenges.
- Preset sharing/importing.
- Additional localization.
- Strong multiplayer synchronization.
- Persistent configuration without overwriting each player's personal setup.

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
