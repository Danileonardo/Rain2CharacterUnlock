# Universal Survivor Unlocks

**Universal Survivor Unlocks (USU)** adds configurable unlock challenges to modded survivors in **Risk of Rain 2**.

The mod automatically detects installed modded survivors, respects characters that already have their own unlock system, and can assign USU challenges to survivors that do not.

> ## AI Usage Notice
> Universal Survivor Unlocks has been developed with assistance from artificial intelligence tools during parts of the programming, design, writing, debugging, and documentation process. Some visual resources may also be created or assisted with AI tools.
>
> This notice is included for transparency so that players who prefer to avoid AI-assisted content can make an informed decision before installing the mod.
>
> **Aviso sobre uso de IA:** Universal Survivor Unlocks ha sido desarrollado con asistencia de herramientas de inteligencia artificial durante parte del proceso de programación, diseño, redacción, depuración y documentación. Algunos recursos visuales también pueden haber sido creados o asistidos mediante IA. Este aviso se incluye por transparencia para quienes prefieran evitar contenido desarrollado con estas herramientas.

---

## What does USU do?

USU creates unlock challenges for modded survivors that would otherwise be available immediately.

Its main goals are:

- Automatically detect installed modded survivors.
- Respect survivors that already have their own native unlock requirements.
- Lock compatible survivors when USU assigns them a challenge.
- Unlock them by completing configurable missions.
- Store challenge configuration in an editable JSON file.
- Preserve survivor configuration when character mods are temporarily uninstalled.
- Support multiplayer with host-authoritative challenge configuration.
- Allow both shared-progress and per-player challenge designs.
- Keep vanilla and DLC survivor unlocks untouched.

---

## Important behavior

### Vanilla and DLC survivors

USU does **not** replace or interfere with the normal unlock requirements of vanilla or DLC survivors.

Characters such as Commando, Huntress, Loader, Railgunner, etc. keep their original Risk of Rain 2 progression.

### Survivors with their own unlock system

If a modded survivor already defines its own unlockable, USU respects it and does not replace it.

### Survivors without an unlock system

USU can automatically give these survivors a configurable challenge and lock them until the requirement is completed.

---

# Current built-in challenges

The following survivor presets are currently included in the project.

## Sora — Elegido de la Llave Espada

Apply **100 valid simultaneous status effects** during a single run.

Valid effects are counted across the team and living entities:

- Negative effects on enemies.
- Positive effects on allies.
- Stackable effects count by stack.
- Non-stackable effects count once per affected entity.
- Expired effects and effects on dead entities stop contributing.

**Progress:** Shared  
**Reward:** Session-wide

---

## Ralsei — Oración de Esperanza

Restore a total of **10,000 health** to your team during a single run.

The challenge uses valid non-regeneration healing and can include healing received by allied entities such as players, drones, turrets, and other friendly units.

**Progress:** Shared  
**Reward:** Session-wide

> The healing target was increased from 5,000 to 10,000 after multiplayer testing showed that healing drones and other allied healing sources could accumulate progress quickly.

---

## Jhin — El Cuarto Acto

Deliver the final blow to a boss with a sufficiently powerful critical hit.

The exact damage threshold can be configured in `Survivors.json`.

---

## Spy — Sin que me veas venir

Kill a boss with a valid fatal backstab.

---

## Scout — Sed Termonuclear

Accumulate the required number of **Bebidas energéticas** during a run.

The current preset uses **15 Energy Drinks**.

---

## Rocket / Soldier — La gravedad es opcional

Kill enemies with explosions while airborne.

This challenge tracks lethal explosion damage and is designed around Rocket/Soldier-style explosive combat.

---

## HUNK — La Parca No Falla

Complete **one** of the following individual streaks:

- Land **24 consecutive Railgunner M99 weak-point hits**, or
- Score **24 consecutive kills with Bandit's Luces fuera**.

Important multiplayer rule:

- Streaks belong to individual players.
- Two players cannot combine partial streaks.
- Any one player who completes a valid route completes the challenge for the session.

Example:

```text
Railgunner A = 12 weak points
Railgunner B = 12 weak points

Result: 12 / 24 for each player
NOT 24 / 24 combined
```

Other Bandit abilities do **not** reset the Lights Out streak.

Only an actual use of **Luces fuera** that fails to kill resets that player's Bandit streak.

**Progress:** Per-player  
**Reward:** Session-wide

---

## Tinkaton — Forjada en Chatarra

> **Recicla 6 objetos; obtén Justicia demoledora y derriba**  
> **a la Unidad de Aleación con Bote explosivo de MUL-T.**

The challenge is designed around Tinkaton's themes of scrap, metal, a massive hammer, and bringing down a flying metallic target.

Current requirements:

1. Play as **MUL-T**.
2. Convert at least **6 items into Scrap** during the same run.
3. Have **Justicia demoledora** (`Shattering Justice`) in your inventory.
4. Defeat the **Alloy Worship Unit**.
5. The final hit must come from **Bote explosivo** (`Blast Canister`) or a valid child bomblet from the same skill.

Scrap progress is remembered for the run even if the Scrap is later consumed in a 3D Printer or another interaction.

The requirements belong to the same player. Different players cannot combine them.

**Progress:** Per-player  
**Reward:** Session-wide

---

# Multiplayer

USU is designed around host-authoritative multiplayer.

## Challenge configuration

The **host's configuration** determines the effective mission parameters for the session.

A client may have different values in their local JSON, but the host's challenge configuration is the authoritative one for that lobby.

## Progress types

USU supports two conceptual progress styles.

### Shared progress

The whole team contributes to a single session total.

Examples:

- Sora
- Ralsei

### Per-player progress

Each player has an independent counter or streak.

Examples:

- HUNK
- Tinkaton

If any single player completes a valid per-player route, the challenge is completed for the session.

---

# Configuration

USU stores survivor challenge information in:

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

The file contains detected modded survivors and their challenge definitions.

Example:

```json
{
  "Challenge": {
    "enabled": true,
    "name": "Forjada en Chatarra",
    "description": "Recicla 6 objetos; obtén Justicia demoledora y derriba\na la Unidad de Aleación con Bote explosivo de MUL-T.",
    "type": "ScrapItemBossFinisher",
    "parameters": {
      "scrapAmount": 6,
      "requiredBody": "ToolbotBody",
      "requiredItem": "ArmorReductionOnHit",
      "bossBody": "SuperRoboBallBossBody",
      "finalDamageSource": "Secondary",
      "requiredSecondarySkillToken": "TOOLBOT_SECONDARY_NAME",
      "singleRun": true
    }
  }
}
```

---

# Authoring Mode

During development, USU currently uses an internal **Authoring Mode**.

When enabled:

- Built-in source presets are treated as the development source of truth.
- Known survivor challenge entries can be automatically synchronized into `Survivors.json`.
- Changes made directly to a known preset inside JSON may be overwritten on startup.

This behavior is useful while developing and testing the built-in challenge library.

A future public customization workflow is planned so users can freely edit challenges without source presets overwriting them.

---

# Planned configuration system

The long-term configuration system is intended to have three main parts.

## Preset library

A library of ready-made challenge presets that can be assigned or imported to compatible modded survivors.

Presets should not be permanently tied to only the survivor they were originally designed for.

## Per-survivor challenge editor

An in-game interface for editing:

- Challenge name
- Description
- Challenge type
- Objective
- Parameters

`Survivors.json` will remain the editable persistence layer.

## Reusable challenge types

USU challenges are being built as reusable systems rather than one-off character-specific scripts whenever practical.

Examples include:

- `ApplyStatusEffects`
- `HealHealth`
- `BossCriticalKill`
- `BackstabBossKill`
- `HoldItemStack`
- `AirborneExplosionKills`
- `PrecisionExecutionStreak`
- `ScrapItemBossFinisher`

---

# Locked survivor presentation

USU modifies the survivor selection presentation for survivors currently locked by the mod.

The intended presentation includes:

- Darkened survivor portrait.
- Vanilla-like locked framing.
- Challenge information on hover.
- Unlock notification when the challenge is completed.
- Normal full-color portrait after unlocking.

Vanilla and DLC survivor presentation is left untouched.

---

# Persistence

Challenge configuration is stored separately from the survivor mod itself.

This allows USU to remember configuration for a survivor even if that character mod is temporarily removed and installed again later.

USU automatically reconciles the detected survivor catalog with the JSON configuration when the game starts.

---

# Dependencies

USU is built for the BepInEx / R2API Risk of Rain 2 modding ecosystem.

Current development dependencies include:

- BepInExPack
- R2API
- R2API Networking
- HookGenPatcher

Exact dependency versions should be taken from the Thunderstore package manifest for each release.

---

# Installation

## Thunderstore Mod Manager / r2modman

1. Install **Universal Survivor Unlocks**.
2. Install the required dependencies automatically through Thunderstore.
3. Install any supported modded survivors you want to use.
4. Launch the game once so USU can generate or reconcile `Survivors.json`.
5. Restart the game if required after modifying configuration.

## Manual installation

Manual installation is intended for users already familiar with BepInEx.

Place the USU DLL and required package files in the appropriate BepInEx plugin directory and install all dependencies listed in the package manifest.

Using a mod manager is recommended.

---

# Compatibility

USU is designed to coexist with:

- Vanilla survivor unlocks.
- DLC survivor unlocks.
- Modded survivors with their own unlock systems.
- Modded survivors without unlock systems.
- Multiplayer sessions where all players use compatible mod profiles.

Because Risk of Rain 2 mods can heavily modify skills, damage behavior, UI, networking, and unlockables, compatibility with every third-party mod cannot be guaranteed.

If another mod changes the exact behavior of a skill or survivor used by a USU challenge, that mission may need a compatibility adjustment.

---

# Debugging

USU writes detailed information to the BepInEx log while the project is under active development.

Useful log prefixes include:

```text
[HUNK]
[HealHealth]
[ApplyStatusEffects]
[ExplosionKill]
[TINKATON]
```

These logs are used to validate:

- Multiplayer ownership.
- Per-player streaks.
- Shared totals.
- Skill origin.
- Damage source.
- Healing behavior.
- Scrap conversion.
- Boss final-hit detection.

---

# Known development notes

Some systems are still undergoing multiplayer and edge-case testing.

In particular:

- Bandit's remote Lights Out route has recently been redesigned to avoid fragile host-side shot/death timing correlation.
- Tinkaton's Scrap and Blast Canister requirements are being validated against real runs.
- Healing from allied drones and other friendly entities is intentionally visible during Ralsei balance testing.
- Explosion-origin classification may be tightened further for challenges that require specific explosive abilities.
- Dynamic localization for challenge text is planned for a future version.

---

# Localization

Current built-in challenge text is primarily authored in Spanish.

When vanilla skill or item names are referenced, the project aims to use the names shown by the current Spanish Latin American (`es-419`) localization whenever confirmed.

A future localization system is planned so challenge text can automatically support multiple game languages.

---

# Contributing / bug reports

When reporting a challenge problem, please include:

- USU version.
- Risk of Rain 2 version.
- Whether you were host or client.
- Survivor used.
- Challenge being attempted.
- Relevant mod list.
- `LogOutput.log` from the affected session.
- Clear description of what happened and what you expected to happen.

For multiplayer issues, logs from both host and client are especially useful.

---

# Credits

**Universal Survivor Unlocks** is an independent Risk of Rain 2 modding project.

Risk of Rain 2 and its original characters, items, skills, and assets belong to their respective owners.

Third-party modded survivors belong to their respective mod authors.

USU does not claim ownership over those characters or their original assets.

---

# AI Transparency

USU uses AI-assisted tools as part of its development workflow.

This can include assistance with:

- Programming
- Debugging
- Refactoring
- Challenge design
- Documentation
- Writing
- Visual resource ideation or generation

Final implementation decisions, testing, balancing, packaging, and project direction remain part of the project's human-led development process.

This disclosure is intentionally included so users can decide for themselves whether they are comfortable installing AI-assisted content.

---

# License

See the included `LICENSE` file for the project's license terms.

---

# Current project direction

USU is evolving from a collection of unlock challenges into a general-purpose configurable framework for modded survivor progression.

The long-term goal is to let players:

- Detect any compatible modded survivor.
- Assign a challenge preset.
- Edit it in-game.
- Share presets.
- Keep configuration between installs.
- Use the same system in multiplayer.
- Build custom progression without modifying the survivor mod itself.

If you find a modded survivor that USU does not handle correctly, logs and reproduction steps are highly valuable for improving compatibility.
