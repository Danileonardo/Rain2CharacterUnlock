# Universal Survivor Unlocks

> **Universal Survivor Unlocks** adds progression-based unlock requirements to compatible modded survivors in **Risk of Rain 2** when those survivors do not provide their own character unlock system.

The goal is to make modded survivors feel more naturally integrated into Risk of Rain 2 progression instead of always becoming immediately available after installation.

---

## ✦ Overview

| Feature | Status |
|---|---:|
| Automatic modded survivor detection | ✅ Implemented |
| Automatic locking for survivors without their own unlock | ✅ Implemented |
| Respect original survivor unlocks | ✅ Implemented |
| Vanilla / official DLC survivor protection | ✅ Implemented |
| Dynamic `UnlockableDef` generation | ✅ Implemented |
| Locked survivor portrait support | ✅ Implemented |
| Persistent `Survivors.json` configuration | ✅ Implemented |
| Configuration backup | ✅ Implemented |
| Survivor uninstall / reinstall configuration recovery | ✅ Implemented |
| `ApplyStatusEffects` gameplay challenge | ✅ Implemented |
| Server-side challenge tracking groundwork | ✅ Implemented |
| Host-authoritative multiplayer configuration | ⚠ Planned / In development |
| Full multiplayer validation | ⚠ Experimental |
| `KillEnemies` gameplay tracking | ⚠ Not yet implemented |

---

## ✦ Features

- Automatically detects installed modded survivors.
- Automatically assigns unlock requirements to compatible modded survivors that do not already provide one.
- Respects unlock requirements created by the original survivor mod authors.
- Does not replace Vanilla or official DLC survivor progression.
- Creates real Risk of Rain 2 `UnlockableDef` entries.
- Uses the game's normal locked survivor behavior.
- Displays modded survivor portraits in generated achievements.
- Automatically generates a configurable `Survivors.json`.
- Preserves survivor configuration when a survivor mod is disabled or uninstalled.
- Restores previous configuration when the survivor becomes available again.
- Automatically creates a backup of the previous configuration.
- Supports configurable survivor challenges.
- Includes gameplay tracking for `ApplyStatusEffects`.
- Includes server-side challenge tracking groundwork for multiplayer-compatible challenges.
- Supports different modded survivor `ContentPack` implementations.
- Compatible with **RealerCheatUnlocks** for testing and manual unlock management.

---

## ✦ How It Works

When Risk of Rain 2 finishes loading, Universal Survivor Unlocks scans the survivor catalog.

| Step | Behavior |
|---:|---|
| 1 | Detect installed modded survivors. |
| 2 | Check whether each survivor already has an unlock requirement. |
| 3 | If the original mod already provides an unlock, leave it untouched. |
| 4 | If no original unlock exists, Universal Survivor Unlocks can assign its own generated unlock. |
| 5 | The survivor remains locked until that generated unlock is granted. |

> **Original survivor unlock systems always take priority.**

Universal Survivor Unlocks does not intentionally replace the progression of Vanilla or official DLC survivors.

---

## ✦ Installation Order

The installation order does **not** matter.

### Survivor installed first

```text
Install Survivor Mod
        ↓
Install Universal Survivor Unlocks
        ↓
Launch Risk of Rain 2
        ↓
Survivor is detected
```

### Universal Survivor Unlocks installed first

```text
Install Universal Survivor Unlocks
        ↓
Install Survivor Mod later
        ↓
Launch Risk of Rain 2
        ↓
Survivor is detected
```

Compatible survivors are scanned again when the game loads.

---

## ✦ Unlock Persistence

Universal Survivor Unlocks separates **challenge configuration** from **unlock progress**.

| Data | Stored by | Purpose |
|---|---|---|
| Challenge configuration | `Survivors.json` | Defines **how** a survivor is unlocked. |
| Completed unlock | Risk of Rain 2 player profile | Stores **whether** the survivor has already been unlocked. |

Conceptually:

```text
Survivors.json
└── HOW the survivor is unlocked

Risk of Rain 2 Player Profile
└── WHETHER the survivor has already been unlocked
```

This allows previously earned survivor unlocks to remain associated with the player profile.

### Reinstall behavior

| Previous state | Uninstall survivor | Reinstall survivor | Expected state |
|---|---:|---:|---:|
| Locked | ✅ | ✅ | 🔒 Remains locked |
| Unlocked | ✅ | ✅ | 🔓 Remains unlocked |

The generated unlock identifier should remain stable between releases so previously earned unlocks can continue to be recognized.

---

## ✦ Configuration

Universal Survivor Unlocks automatically creates its configuration after the game successfully loads with the mod installed.

### Main configuration

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

When using **r2modman** or **Thunderstore Mod Manager**:

```text
<Profile>/BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

### Backup

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.backup.json
```

You do **not** need to manually create `Survivors.json`.

---

## ✦ JSON Structure

The configuration separates survivors into two groups:

```json
{
  "availableSurvivors": {},
  "unavailableSurvivors": {}
}
```

| Group | Description |
|---|---|
| `availableSurvivors` | Modded survivors that are currently installed and available. |
| `unavailableSurvivors` | Previously detected modded survivors that are currently unavailable. |

A survivor may become unavailable because its mod was disabled, removed, temporarily uninstalled, or failed to load.

Its saved configuration is preserved instead of being immediately deleted. If the survivor becomes available again, Universal Survivor Unlocks can restore its previous configuration.

---

## ✦ Example Survivor Configuration

```json
{
  "availableSurvivors": {
    "ExampleBody": {
      "displayName": "Example",
      "internalName": "Example",
      "bodyName": "ExampleBody",
      "source": "com.example.survivor",
      "originalUnlock": "Ninguno",
      "available": true,
      "status": "Available",
      "reason": "",
      "challenge": {
        "enabled": true,
        "name": "Example Challenge",
        "type": "ApplyStatusEffects",
        "parameters": {
          "amount": 100,
          "singleRun": true
        }
      }
    }
  }
}
```

Most metadata fields are maintained automatically by Universal Survivor Unlocks. Normally, users should only modify:

```text
challenge
```

---

## ✦ Challenge Configuration

A challenge uses the following basic structure:

```json
"challenge": {
  "enabled": true,
  "name": "Challenge Name",
  "type": "ChallengeType",
  "parameters": {}
}
```

| Field | Purpose |
|---|---|
| `enabled` | Enables or disables the Universal Survivor Unlocks challenge. |
| `name` | Defines the displayed challenge name. |
| `type` | Defines which challenge tracker should be used. |
| `parameters` | Contains settings specific to the selected challenge type. |

Example:

```json
"challenge": {
  "enabled": true,
  "name": "Status Master",
  "type": "ApplyStatusEffects",
  "parameters": {
    "amount": 100,
    "singleRun": true
  }
}
```

---

# ✦ ApplyStatusEffects

`ApplyStatusEffects` is currently the first fully implemented gameplay challenge type.

The challenge is completed when the configured number of valid status effects are active **simultaneously** during the same run.

For example:

```json
"amount": 100
```

means:

> Maintain at least **100 valid active status effects at the same time**.

---

## Negative Status Effects

Negative status effects count only while they are active on enemies.

```text
Negative Effect
+
Enemy
=
Counts
```

Examples may include valid debuffs, damage-over-time effects, slowing effects, weakening effects, and crowd-control states.

The exact available effects depend on the currently loaded game content and installed mods.

---

## Positive Status Effects

Positive status effects count only while they are active on allies or members of the player team.

```text
Positive Effect
+
Ally
=
Counts
```

Negative status effects affecting allied players do not contribute to the positive total. Positive effects on enemies are not counted as valid enemy debuffs.

---

## Stackable Effects

Stackable effects contribute once for **every currently active stack**.

```text
Bleed x25
=
25 active effects
```

Example across multiple enemies:

```text
Enemy A
Bleed x20

Enemy B
Bleed x15

TOTAL = 35
```

---

## Non-Stackable Effects

A non-stackable effect contributes once per affected entity.

```text
Slow on Enemy A = 1
Slow on Enemy B = 1
Slow on Enemy C = 1

TOTAL = 3
```

Multiple internal stacks of a non-stackable effect do not make that effect contribute multiple times on the same entity.

---

## Multiple Targets

Status effects from multiple valid entities are added together.

```text
Enemy A
Bleed x20

Enemy B
Burn x30

Enemy C
Bleed x25

Ally
Positive Buff x1

TOTAL = 76
```

The challenge uses the total number of valid effects currently active across the battlefield.

---

## Real-Time Tracking

`ApplyStatusEffects` tracks the **current** state of the battlefield.

If an effect expires, is removed, is cleansed, or the affected entity dies, that effect stops contributing to the active total.

```text
100 active effects
↓
10 Bleed stacks expire
↓
90 active effects
```

The active total can therefore increase and decrease during the run.

Once the requirement is reached and the survivor unlock is granted, that unlock remains completed for that player profile.

---

## ✦ Challenge Status

| Challenge Type | Gameplay Tracking | Status |
|---|---:|---:|
| `ApplyStatusEffects` | ✅ | Implemented |
| `KillEnemies` | ❌ | Not yet fully implemented |

Some automatically generated survivor configurations may currently contain:

```json
"type": "KillEnemies",
"parameters": {
  "amount": 100
}
```

The configuration structure supports additional challenge types, but only challenge types with an implemented tracker should currently be considered functional.

---

## ✦ Original Survivor Unlocks

Universal Survivor Unlocks intentionally does **not** replace unlock requirements provided by another survivor mod.

If a modded survivor already provides its own `UnlockableDef`:

```text
Original Unlock Detected
        ↓
Universal Survivor Unlocks
        ↓
Leaves it unchanged
```

This helps prevent conflicts with progression systems designed by other mod authors.

---

## ✦ Compatibility

Universal Survivor Unlocks dynamically detects compatible modded survivors instead of requiring a hardcoded dependency for every character.

| Survivor / Mod | Detection | Unlock handling |
|---|---:|---:|
| Sora | ✅ | ✅ Tested |
| Ralsei | ✅ | ✅ Tested |
| HUNK | ✅ | ✅ Tested |
| Enforcer | ✅ | Original unlock respected |
| Nemesis Enforcer | ✅ | Original unlock respected |
| Auriel | ✅ | Original unlock respected |

These survivor mods are **not required dependencies** of Universal Survivor Unlocks.

---

## ✦ Sora Compatibility

Universal Survivor Unlocks has been extensively tested with **Sora by Dragonyck**.

Sora itself is **not created or maintained by Universal Survivor Unlocks**. Universal Survivor Unlocks only provides external compatibility and unlock integration when appropriate.

| Test | Result |
|---|---:|
| Automatic survivor detection | ✅ |
| Generated USU unlock requirement | ✅ |
| Locked survivor behavior | ✅ |
| Unlock persistence | ✅ |
| Survivor uninstall / reinstall while locked | ✅ |
| Survivor uninstall / reinstall while unlocked | ✅ |
| Status effect tracking | ✅ |
| Positive status effects | ✅ |
| Negative status effects | ✅ |
| Stackable damage-over-time effects | ✅ |
| Non-stackable effects | ✅ |
| Unlock completion | ✅ |
| Repeated unlock prevention after completion | ✅ |

> Sora remains the work of its original mod author. Universal Survivor Unlocks only provides external unlock integration.

---

## ✦ Multiplayer

Universal Survivor Unlocks uses server-side tracking groundwork for gameplay challenges.

This architecture is intended to support multiplayer-compatible unlock conditions.

> ⚠ **Full multiplayer behavior is still experimental.**

Host-to-client challenge configuration synchronization is still being developed and tested.

The planned behavior is:

```text
Host Configuration
        ↓
Authoritative Challenge Settings
        ↓
Players In The Session
```

This would allow the Host's survivor challenge configuration to control the multiplayer session without permanently overwriting each client's personal configuration.

Until multiplayer synchronization has been fully validated, multiplayer challenge behavior should be considered experimental.

---

## ✦ RealerCheatUnlocks

Universal Survivor Unlocks is compatible with **RealerCheatUnlocks**.

It can be useful during testing for:

- manually granting survivor unlocks,
- manually revoking survivor unlocks,
- testing locked survivor portraits,
- validating unlock persistence,
- resetting test states.

RealerCheatUnlocks is optional and is **not required** for normal use.

---

## ✦ Installation

### Thunderstore / r2modman

Install Universal Survivor Unlocks using:

- Thunderstore Mod Manager
- r2modman

Required dependencies should be installed automatically when installing the package with dependencies.

### Manual Installation

Place the plugin inside your Risk of Rain 2 BepInEx installation.

The DLL should ultimately be located under:

```text
BepInEx/plugins/UniversalSurvivorUnlocks/
```

Example:

```text
BepInEx/
└── plugins/
    └── UniversalSurvivorUnlocks/
        └── UniversalSurvivorUnlocks.dll
```

---

## ✦ Dependencies

| Dependency |
|---|
| BepInExPack |
| R2API ContentManagement |
| R2API Language |
| R2API Unlockable |

Exact package dependency versions are defined in:

```text
manifest.json
```

---

## ✦ Troubleshooting

### Survivor Is Not Detected

Make sure:

- the survivor mod itself loads correctly,
- all required dependencies are installed,
- Universal Survivor Unlocks loads successfully,
- the survivor appears in Risk of Rain 2's survivor catalog.

Check:

```text
BepInEx/LogOutput.log
```

and search for:

```text
Universal Survivor Unlocks
```

The log contains detailed survivor detection information.

### Survivor Is Not Locked

The survivor may already provide its own unlock requirement.

Universal Survivor Unlocks intentionally respects progression systems created by the original survivor mod author.

### Survivor Was Uninstalled

Universal Survivor Unlocks preserves previously detected survivor configuration where possible.

The survivor may appear under:

```json
"unavailableSurvivors"
```

When the survivor mod becomes available again, its previous configuration can be restored.

### Configuration Problems

Check:

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

Make sure the JSON syntax is valid.

---

## ⚠ Fields Normally Managed Automatically

Avoid manually changing:

| Field |
|---|
| `displayName` |
| `internalName` |
| `bodyName` |
| `source` |
| `originalUnlock` |
| `available` |
| `status` |
| `reason` |

The recommended editable section is:

```text
challenge
```

---

## ✦ Current Development

Current and planned development areas include:

- additional gameplay challenge types,
- survivor-specific default challenge presets,
- Host-authoritative multiplayer configuration,
- multiplayer synchronization,
- multiplayer compatibility testing,
- kill-based challenges,
- healing-based challenges,
- boss-related challenges,
- elite enemy challenges,
- stage progression challenges,
- difficulty-based challenges,
- run completion challenges,
- additional survivor compatibility testing.

---

## ✦ Planned Challenge Examples

| Example |
|---|
| Kill a configured number of enemies |
| Defeat specific bosses |
| Reach a specific stage |
| Complete a run on a specific difficulty |
| Heal a configured amount of health |
| Complete special survivor-specific objectives |

These examples are development goals and do not necessarily represent currently implemented challenge types.

---

## ✦ Source Code

Source code is available on GitHub:

https://github.com/Danileonardo/Rain2CharacterUnlock

Bug reports, compatibility reports, suggestions and development feedback can also be submitted through the repository.

---

## ✦ Third-Party Survivor Mods

Universal Survivor Unlocks does not claim ownership of third-party survivor mods.

Characters, survivor implementations, assets, animations, sounds, skills and other content belonging to third-party survivor mods remain the work of their respective creators.

Universal Survivor Unlocks only provides an external progression and unlock integration system for compatible survivors.

---

## ✦ Credits

Thanks to the Risk of Rain 2 modding community and the developers of:

- BepInEx
- R2API
- r2modman
- Thunderstore

for providing the tools and infrastructure used by the modding ecosystem.

Third-party survivor mods referenced for compatibility testing remain credited to their respective creators.

---

## ✦ License

Universal Survivor Unlocks is licensed under the **MIT License**.

See the included `LICENSE` file for complete license information.
