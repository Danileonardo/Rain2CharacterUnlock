# Universal Survivor Unlocks

> **Universal Survivor Unlocks** adds progression-based unlock requirements to compatible modded survivors in **Risk of Rain 2** when those survivors do not provide their own character unlock system.

The goal is to make modded survivors feel more naturally integrated into Risk of Rain 2 progression instead of becoming immediately available after installation.

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
| Built-in survivor challenge presets | ✅ Implemented |
| Server-side gameplay challenge tracking | ✅ Implemented |
| Host-authoritative multiplayer design | 🧪 Experimental |
| Full multiplayer validation | 🧪 In testing |
| `KillEnemies` fallback tracker | ❌ Not implemented |

---

## ✦ Features

- Automatically detects installed modded survivors.
- Automatically assigns unlock requirements to compatible modded survivors that do not already provide one.
- Respects unlock requirements created by original survivor mod authors.
- Does not replace Vanilla or official DLC survivor progression.
- Creates real Risk of Rain 2 `UnlockableDef` entries.
- Uses the game's normal locked survivor behavior.
- Displays modded survivor portraits in generated achievements.
- Automatically generates a configurable `Survivors.json`.
- Preserves unavailable survivor configuration for reinstall recovery.
- Automatically creates a backup of the previous configuration.
- Supports built-in survivor-specific challenge presets.
- Supports multiple server-side gameplay challenge types.
- Tracks challenge progress per run where required.
- Supports player-owned minion attribution for relevant challenges.
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
| 5 | If a built-in preset exists for that survivor, it is used as the default challenge. |
| 6 | Otherwise, a generic fallback challenge configuration is created. |
| 7 | The survivor remains locked until the generated unlock is granted. |

> **Original survivor unlock systems always take priority.**

Universal Survivor Unlocks does not intentionally replace the progression of Vanilla or official DLC survivors.

---

## ✦ Built-In Survivor Presets

Universal Survivor Unlocks currently includes built-in presets for the following compatible modded survivors.

| Survivor | Default challenge | Requirement |
|---|---|---|
| Sora | **Elegido de la Llave Espada** | Maintain 100 valid status effects simultaneously during one run. |
| Ralsei | **Oración de Esperanza** | Restore a total of 5000 health to the player team during one run. |
| Jhin | **El Cuarto Acto** | Land the final blow on a boss with a critical hit dealing at least 4444 damage. |
| Scout | **Sed Termonuclear** | Have one player hold 15 Energy Drinks at the same time. |
| Spy | **Sin que me veas venir** | Kill a boss with Bandit's Serrated Dagger using a valid backstab. |
| Rocket | **La gravedad es opcional** | Kill 15 enemies with explosions without touching the ground. |
| HUNK | **La Parca No Falla** | Reach either 24 consecutive Railgunner weak-point hits or 24 consecutive Bandit Lights Out kills. |

These survivor mods are **not required dependencies** of Universal Survivor Unlocks.

The current preset text is authored in Spanish. Broader localization support is planned for a future release.

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

Generated unlock identifiers are intended to remain stable between releases so previously earned unlocks can continue to be recognized.

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
        "description": "Complete the configured challenge.",
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

Most metadata fields are maintained automatically by Universal Survivor Unlocks.

---

## ✦ Challenge Configuration

A challenge uses the following basic structure:

```json
"challenge": {
  "enabled": true,
  "name": "Challenge Name",
  "description": "Challenge description.",
  "type": "ChallengeType",
  "parameters": {}
}
```

| Field | Purpose |
|---|---|
| `enabled` | Enables or disables the Universal Survivor Unlocks challenge. |
| `name` | Defines the displayed challenge name. |
| `description` | Defines the displayed challenge description. |
| `type` | Defines which challenge tracker should be used. |
| `parameters` | Contains settings specific to the selected challenge type. |

---

## ✦ Implemented Challenge Types

### `ApplyStatusEffects`

Maintains a configured number of valid active status effects simultaneously during the same run.

- Negative effects count on enemies.
- Positive effects count on Player-team allies.
- Stackable effects count once per active stack.
- Non-stackable effects count once per affected entity.
- Effects stop contributing when they expire, are removed, or the affected entity dies.

### `HealHealth`

Tracks actual health restored to living Player-team entities during a run.

- Includes players, turrets, drones and other valid allies.
- Includes natural regeneration and other real healing sources.
- Counts actual health restored.
- Does not count overheal, barrier or shield gain.
- Progress is cumulative for the run.

### `BossCriticalKill`

Completes when the final blow on a boss is a critical hit that meets the configured minimum damage requirement.

The damage requirement applies to a single lethal critical hit. Damage is not accumulated across multiple attacks or players.

### `HoldItemStack`

Completes when one individual player simultaneously holds the configured number of the required item.

Player inventories are not combined.

The current Scout preset uses:

```json
"item": "SprintBonus",
"amount": 15
```

which corresponds to **Energy Drink**.

### `BackstabBossKill`

Completes when Bandit's **Serrated Dagger** delivers the lethal hit to a boss from a valid backstab position.

The tracker uses Risk of Rain 2's backstab logic instead of treating ordinary side or front attacks as valid.

### `AirborneExplosionKills`

Tracks explosion / blast kills belonging to one player while that player remains airborne.

- The explosion itself must deliver the lethal damage.
- Merely damaging an enemy with an explosion does not count.
- Damage-over-time that kills later does not count as an explosion kill.
- Blast / shockwave attacks are valid.
- Explosive skills, items and equipment can contribute when their blast causes the kill.
- Player-owned explosive minions or drones can contribute to their owner's counter.
- The owner player must be airborne; whether the drone or minion is airborne is irrelevant.
- Touching the ground resets that player's streak.
- Players never combine their streaks.

### `PrecisionExecutionStreak`

Provides two alternate precision routes.

**Railgunner route**

Reach the configured number of consecutive M99 weak-point hits. A failed M99 weak-point attempt resets the Railgunner streak.

**Bandit route**

Reach the configured number of consecutive kills with **Lights Out**. A Lights Out use that does not kill resets the Bandit streak.

Other Bandit abilities do not add to the streak and are not intended to replace Lights Out.

---

## ✦ Challenge Status

| Challenge Type | Tracking | Validation status |
|---|---:|---:|
| `ApplyStatusEffects` | ✅ | Tested |
| `HealHealth` | ✅ | Implementation ready / final unlock validation pending |
| `BossCriticalKill` | ✅ | Implementation ready / gameplay validation pending |
| `HoldItemStack` | ✅ | Implementation ready / gameplay validation pending |
| `BackstabBossKill` | ✅ | Implementation ready / gameplay validation pending |
| `AirborneExplosionKills` | ✅ | Implementation ready / broad explosion-source validation pending |
| `PrecisionExecutionStreak` | ✅ | Implementation ready / gameplay validation pending |
| `KillEnemies` | ❌ | Fallback configuration only |

The newer challenge types are being validated through a general gameplay and multiplayer test pass.

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

| Survivor / Mod | Detection / integration |
|---|---:|
| Sora | ✅ |
| Ralsei | ✅ |
| Jhin | ✅ |
| Scout | ✅ |
| Spy | ✅ |
| Rocket | ✅ |
| HUNK | ✅ |
| Enforcer | Original unlock respected |
| Nemesis Enforcer | Original unlock respected |
| Auriel | Original unlock respected |

Third-party survivor mods are **not dependencies** unless explicitly listed in `manifest.json`.

---

## ✦ Multiplayer

Gameplay challenge trackers are designed to run server-side.

The intended multiplayer model is:

```text
Host / Server
        ↓
Authoritative gameplay tracking
        ↓
Players in the session
```

Some challenges use a shared session event when one player performs the required individual action. Other challenges maintain per-player counters so progress from different players is never incorrectly combined.

> ⚠ **Full multiplayer behavior is still experimental and undergoing validation.**

Host-to-client configuration synchronization is also still under development and should not yet be considered fully validated.

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

Required dependencies should be installed automatically.

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
| R2API Core |
| R2API ContentManagement |
| R2API Language |
| R2API Unlockable |

Exact dependency versions are defined in:

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

### Survivor Is Not Locked

The survivor may already provide its own unlock requirement.

Universal Survivor Unlocks intentionally respects progression systems created by the original survivor mod author.

### Survivor Was Uninstalled

The survivor may appear under:

```json
"unavailableSurvivors"
```

When the survivor mod becomes available again, its saved configuration can be restored.

### Configuration Problems

Check:

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

Make sure the JSON syntax is valid.

---

## ⚠ Fields Normally Managed Automatically

Avoid manually changing metadata fields such as:

```text
displayName
internalName
bodyName
source
originalUnlock
available
status
reason
```

Challenge-related fields live under:

```text
challenge
```

---

## ✦ Current Development

Current development areas include:

- general gameplay validation of the new built-in presets,
- multiplayer validation,
- host-authoritative configuration synchronization,
- broader explosion-source compatibility,
- future in-game challenge configuration,
- custom user-created presets,
- localization support,
- additional reusable challenge types,
- additional survivor compatibility testing.

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
