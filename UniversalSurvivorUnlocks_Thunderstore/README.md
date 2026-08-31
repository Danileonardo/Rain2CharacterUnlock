# Universal Survivor Unlocks

**Universal Survivor Unlocks (USU)** is a configurable progression framework for **modded survivors in Risk of Rain 2**.

USU automatically detects compatible modded survivors, respects characters that already provide their own unlock requirement, and can give progression challenges to survivors that would otherwise be available immediately.

**Current release:** `0.1.9`

[GitHub / Source Code](https://github.com/Danileonardo/Rain2CharacterUnlock) · [Support development / Donate](https://github.com/sponsors/Danileonardo) · Discord: `@shleiderick`

> **Support / Apoyo**
>
> Donations help me keep the project updated and continue developing new presets, compatibility fixes, multiplayer improvements, and the universal mission system.
>
> Los donativos ayudan a mantener el proyecto actualizado y a continuar desarrollando nuevos presets, correcciones de compatibilidad, mejoras multijugador y el sistema universal de misiones.
>
> Questions, bug reports, survivor requests, preset ideas, and suggestions are welcome on Discord.
>
> Preguntas, reportes de errores, solicitudes de personajes, ideas de presets y sugerencias son bienvenidas en Discord.

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
- Evolve toward a reusable mission framework instead of requiring one hardcoded script for every character.

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

Creator presets are intended to remain unchanged so updates can improve or rebalance them without destroying the original definition.

## Player customization

When a player customizes a creator preset, the intended model is to create/use a **separate custom mission based on that preset** instead of editing the creator preset itself.

This allows USU to keep:

- The original creator preset.
- The player's customized version.
- The technical relationship between the custom mission and the preset it came from.

The public in-game editing workflow is still under development. Existing challenge configuration remains available through `Survivors.json`, while the newer mission/preset architecture is integrated progressively.

---

# Multiplayer

USU uses a **host-authoritative** model.

## Host configuration

During a multiplayer session, the **host's effective challenge configuration is the authoritative mission for that lobby**.

A client may have different local values or a different personal configuration, but the host determines the mission rules used during that session.

The host's session configuration is not intended to permanently overwrite the client's personal `Survivors.json`.

## Shared progress

Shared challenges allow multiple players and valid team entities to contribute toward the same session objective.

Examples:

- Sora
- Ralsei

## Per-player progress

Per-player challenges keep independent counters, streaks, inventories, or route state for each player.

Examples:

- HUNK
- Tinkaton

Players cannot combine partial individual streaks or personal requirements unless the challenge is explicitly designed as shared progress.

## Session-wide completion

When a valid route is completed, USU can grant the corresponding survivor unlock to connected players who still need it.

---

# Current built-in presets

The following creator-made presets are currently included in USU.

## Sora — Elegido de la Llave Espada

Apply **100 valid simultaneous status effects** during a single run.

Valid effects include:

- Negative effects on enemies.
- Positive effects on allies.
- Stackable effects counted by active stack.
- Non-stackable effects counted once per affected entity.
- Effects from multiple valid living entities at the same time.

Expired effects, removed effects, and effects on dead entities stop contributing.

**Progress:** Shared  
**Completion:** Session-wide

---

## Ralsei — Oración de Esperanza

Restore a total of **10,000 health** to valid allied entities during a single run.

The challenge can include valid healing received by:

- Players.
- Allied drones.
- Turrets.
- Other friendly entities.

Passive regeneration is ignored. Barrier, shields, and overhealing are not treated as restored health.

**Progress:** Shared  
**Completion:** Session-wide

---

## Jhin — El Cuarto Acto

Deliver the final blow to a boss with a sufficiently powerful **critical hit**.

The current preset uses a lethal critical-hit requirement of at least **44,444 damage**.

---

## Spy — Sin que me veas venir

Kill a boss with a valid **fatal backstab**.

The preset is designed around a lethal backstab route and uses Risk of Rain 2's backstab/damage information to validate the finishing hit.

---

## Scout — Energía Atómica

Accumulate **15 Energy Drinks** on a single player during the run.

Player inventories are tracked independently and are not combined between players.

---

## Rocket / Soldier — La gravedad es opcional

Kill **15 enemies** with valid explosions while the owning player remains **airborne**.

The challenge is designed around explosive Rocket/Soldier-style combat and validates the player ownership of qualifying explosive damage.

Touching the ground can reset the relevant airborne streak/progress according to the configured challenge rules.

---

## HUNK — La Parca No Falla

Complete **one** of the following individual routes:

- Land **24 consecutive Railgunner M99 weak-point hits**, **OR**
- Score **24 consecutive kills with Bandit's Luces fuera / Lights Out**.

Important multiplayer rules:

- Each player's streak is independent.
- Two players cannot combine partial streaks.
- Other Bandit abilities do **not** reset the Lights Out streak.
- A real Lights Out use that fails to kill resets that player's Lights Out streak.
- If one player completes a valid route, the challenge can complete for the session.

Example:

```text
Railgunner A = 12 / 24
Railgunner B = 12 / 24

Result:
A remains at 12 / 24
B remains at 12 / 24

They do NOT become 24 / 24 combined.
```

**Progress:** Per-player  
**Completion:** Session-wide

---

## Tinkaton — Forjada en Chatarra

> **Recicla 6 objetos; obtén Justicia demoledora y derriba**  
> **a la Unidad de Aleación con Bote explosivo de MUL-T.**

Current requirements:

1. Play as **MUL-T**.
2. Convert at least **6 items into Scrap** during the same run.
3. Have **Justicia demoledora / Shattering Justice** in your inventory.
4. Defeat the **Alloy Worship Unit**.
5. The final hit must come from **Bote explosivo / Blast Canister** or a valid child bomblet generated by the same skill.

Scrap progress belongs to the player who performed the conversions and remains remembered for the run even if the Scrap is later consumed.

Different players cannot combine Tinkaton's personal requirements.

**Progress:** Per-player  
**Completion:** Session-wide

---

# Challenge systems currently used by presets

USU already includes multiple reusable gameplay challenge systems:

- `ApplyStatusEffects`
- `HealHealth`
- `BossCriticalKill`
- `BackstabBossKill`
- `HoldItemStack`
- `AirborneExplosionKills`
- `PrecisionExecutionStreak`
- `ScrapItemBossFinisher`
- `KillEnemies`

These systems are progressively being moved toward a more general mission architecture.

---

# Mission System v2 foundation

Version `0.1.9` includes development groundwork for the next-generation mission format.

The new model is designed around:

- `MissionDefinition`
- `MissionRoute`
- `MissionObjective`
- `MissionCondition`
- `MissionRules`
- `MissionTarget`
- `MissionPreset`
- Preset/custom mission source tracking
- `Shared` and `PerPlayer` progress scopes
- Session reward scope
- Multiple alternative routes

## Route logic

Conditions inside one route are treated as **AND** requirements.

Different routes are treated as **OR** alternatives.

Conceptually:

```text
Route A:
Objective
AND Condition
AND Condition

OR

Route B:
Objective
AND Condition
```

This model is useful for challenges such as HUNK, where two completely different gameplay routes can unlock the same survivor.

## Target system

The mission model is being built to support reusable target categories such as:

- Any enemy.
- Enemy.
- Elite.
- Boss.
- Specific body.
- Specific boss.

Specific entities use stable internal body IDs instead of localized display names.

Examples:

```text
BrotherBody
SuperRoboBallBossBody
ScavBody
```

This allows future missions to target bosses by their internal body identity without depending on the language selected by the player.

## Current v2 runtime status

The v2 architecture is being integrated progressively.

The current foundation includes evaluation for the `Kill` objective and initial conditions/targets, including:

- `Airborne`
- `Grounded`
- `RequiredSurvivor`
- `Enemy`
- `Boss`
- `Elite`
- `SpecificBody`
- `SpecificBoss`

Existing creator presets continue to use the established gameplay trackers while they are migrated to the universal mission format.

This section documents the architecture already present in the project; it does **not** mean every planned objective or editor option is already available to players.

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

Because the mission system is evolving, making a backup before manually editing complex challenge data is recommended.

## Built-in preset synchronization in 0.1.9

Built-in creator presets are still treated as source definitions by the current authoring/synchronization layer.

This means direct edits to a known built-in preset inside `Survivors.json` may be replaced by the creator preset when USU synchronizes configuration.

This is intentional while the separate custom-copy workflow is being integrated: the creator preset remains preserved, and future player customization is intended to live in a separate custom mission instead of modifying the source preset itself.

---

# Persistence

USU stores its progression/configuration separately from the survivor mod itself.

This allows the project to remember information about a modded survivor when that character mod is temporarily removed and later installed again.

USU reconciles its detected survivor catalog with the stored configuration during startup.

---

# Locked survivor presentation

For survivors locked by USU, the mod integrates the lock into the character selection experience.

The intended presentation includes:

- Darkened/locked survivor portrait.
- Vanilla-like locked framing.
- Challenge information on hover.
- Unlock notification when the requirement is completed.
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

Using a mod manager is recommended.

---

# Dependencies

The Thunderstore package currently declares:

- `bbepis-BepInExPack-5.4.2121`
- `RiskofThunder-R2API_Core-5.3.0`
- `RiskofThunder-R2API_ContentManagement-1.0.11`
- `RiskofThunder-R2API_Language-1.1.0`
- `RiskofThunder-R2API_Unlockable-1.0.2`
- `RiskofThunder-R2API_Networking-1.0.4`

Always treat the `manifest.json` included with the current release as the authoritative dependency list.

---

# Compatibility

USU is designed to coexist with:

- Vanilla survivor progression.
- Official DLC survivor progression.
- Modded survivors with native unlock systems.
- Modded survivors without native unlock systems.
- Multiplayer sessions using compatible mod profiles.

Risk of Rain 2 mods can modify skills, networking, damage behavior, content loading, unlockables, and UI in many different ways, so compatibility with every third-party mod cannot be guaranteed.

If another mod changes a skill or mechanic used by a USU preset, that challenge may require a compatibility adjustment.

---

# Reporting bugs / requesting presets

The most useful bug report includes:

- Universal Survivor Unlocks version.
- Risk of Rain 2 version.
- Whether the problem happened as **host** or **client**.
- Survivor used.
- Challenge/preset being attempted.
- Relevant mod list.
- What happened.
- What you expected to happen.
- `LogOutput.log` from the affected session.

For multiplayer issues, logs from both host and client are especially useful.

You can use:

- **Discord / Direct contact:** `@shleiderick`
  - Original Discord alias: `Shleider#3336`
- **GitHub:** https://github.com/Danileonardo/Rain2CharacterUnlock

You can contact me on Discord for:

- Questions.
- Survivor requests.
- Challenge/preset ideas.
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

- **Sora** — mod by **Dragonyck**  
  https://thunderstore.io/c/riskofrain2/p/Dragonyck/Sora/

- **RalseiSurvivor** — package by **GodRayProductions** and its credited contributors  
  https://thunderstore.io/c/riskofrain2/p/GodRayProductions/RalseiSurvivor/

- **Jhin** — mod by **SeroRonin**  
  https://thunderstore.io/c/riskofrain2/p/SeroRonin/Jhin/

- **Spy** — mod by **tsuyoikenko**  
  https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Spy/

- **Scout** — mod by **tsuyoikenko**  
  https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Scout/

- **Rocket** — mod by **EnforcerGang**  
  https://thunderstore.io/c/riskofrain2/p/EnforcerGang/Rocket/

- **HUNK** — mod by **public_ParticleSystem**  
  https://thunderstore.io/c/riskofrain2/p/public_ParticleSystem/HUNK/

- **Tinkaton** — package by **DragonycksModdingComms**  
  https://thunderstore.io/c/riskofrain2/p/DragonycksModdingComms/Tinkaton/

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

Long-term goals include:

- A reusable creator-preset library.
- Safe player-created copies/custom missions.
- An in-game mission editor.
- Reusable objectives and conditions.
- Configurable enemy, elite, boss, and specific-body targets.
- Multi-route challenges.
- Preset sharing/importing.
- Better localization.
- Strong multiplayer synchronization.
- Persistent configuration without overwriting each player's personal setup.

---

# Links

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
