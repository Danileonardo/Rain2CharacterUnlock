# Universal Survivor Unlocks

Universal Survivor Unlocks is a Risk of Rain 2 mod that adds an unlock system to modded survivors that do not provide their own character unlock requirement.

The main goal is to make modded survivors feel more integrated with the normal Risk of Rain 2 progression system instead of being immediately available after installation.

## Features

- Automatically detects modded survivors.
- Automatically locks newly installed modded survivors that do not have their own unlock requirement.
- Respects unlock requirements already provided by survivor mod authors.
- Does not modify Vanilla or official DLC survivors.
- Creates a real `UnlockableDef` for supported modded survivors.
- Uses Risk of Rain 2's normal locked survivor behavior.
- Displays modded survivor portraits in custom unlock achievements.
- Keeps configuration for survivors even if their mod is later disabled or uninstalled.
- Automatically generates a configurable `Survivors.json`.
- Automatically creates a backup of the previous configuration.
- Compatible with different modded survivor ContentPack implementations.
- Compatible with RealerCheatUnlocks for testing or manually granting/revoking unlocks.

## How it works

When the game loads, Universal Survivor Unlocks detects installed modded survivors.

For each detected survivor:

1. If the survivor already has an unlock requirement created by its original mod author, Universal Survivor Unlocks leaves it untouched.
2. If the survivor does not have an original unlock requirement, Universal Survivor Unlocks creates one automatically.
3. A newly detected survivor receives a default challenge configuration.
4. The survivor starts locked until its generated unlock is granted.

Vanilla and official DLC survivors are ignored by this system.

## Configuration

The configuration file is automatically created after launching the game with the mod installed.

The file is located at:

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

When using r2modman or Thunderstore Mod Manager, this path is located inside the selected profile:

```text
<Profile>/BepInEx/config/UniversalSurvivorUnlocks/Survivors.json
```

A backup is also automatically created when an existing configuration is updated:

```text
BepInEx/config/UniversalSurvivorUnlocks/Survivors.backup.json
```

You do not need to create `Survivors.json` manually.

## JSON structure

The file separates survivors into two groups:

```json
{
  "availableSurvivors": {},
  "unavailableSurvivors": {}
}
```

### `availableSurvivors`

Contains modded survivors that are currently installed and available.

### `unavailableSurvivors`

Contains previously detected modded survivors that are currently unavailable, for example because their mod was disabled or uninstalled.

Their configuration is preserved so it can be restored if the survivor becomes available again.

## Example survivor configuration

A newly installed modded survivor without an original unlock requirement will receive a configuration similar to this:

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
        "name": "Desafío de desbloqueo",
        "type": "KillEnemies",
        "parameters": {
          "amount": 100
        }
      }
    }
  }
}
```

Most metadata fields are automatically maintained by the mod.

Normally, you should only edit the `challenge` section.

## Editing a survivor challenge

For example, the automatically generated challenge:

```json
"challenge": {
  "enabled": true,
  "name": "Desafío de desbloqueo",
  "type": "KillEnemies",
  "parameters": {
    "amount": 100
  }
}
```

can be modified to:

```json
"challenge": {
  "enabled": true,
  "name": "Desafío de desbloqueo",
  "type": "KillEnemies",
  "parameters": {
    "amount": 250
  }
}
```

This changes the configured amount from:

```text
100
```

to:

```text
250
```

### Disabling the custom unlock

You can disable the custom challenge for a survivor by changing:

```json
"enabled": true
```

to:

```json
"enabled": false
```

When the custom challenge is disabled, Universal Survivor Unlocks will not apply its generated character unlock requirement to that survivor.

Do not manually modify these fields unless you know what you are doing:

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

They are maintained automatically by the mod.

## Default configuration

Newly detected modded survivors without their own unlock requirement currently receive:

```json
"type": "KillEnemies"
```

with:

```json
"amount": 100
```

This default may become more customizable as additional challenge types are implemented.

## Important: current challenge status

Universal Survivor Unlocks v0.1.0 currently implements the survivor detection, automatic locking, unlock creation, JSON configuration and Risk of Rain 2 integration systems.

The challenge execution system is still under development.

This means challenge definitions such as:

```json
"type": "KillEnemies",
"parameters": {
  "amount": 100
}
```

are currently stored and used to define the generated unlock, but automatic gameplay progress tracking for those challenge types is not yet implemented.

Additional challenge types and automatic completion conditions are planned for future versions.

## Original survivor unlocks

Universal Survivor Unlocks intentionally does not replace unlock requirements provided by another survivor mod.

If a modded survivor already has its own `UnlockableDef`, that survivor remains controlled by the original mod.

This prevents Universal Survivor Unlocks from breaking custom progression systems created by other survivor authors.

## Compatibility

The detection and locking system has been tested with modded survivors using different content loading implementations.

Currently tested examples include:

- Sora
- Ralsei
- HUNK

These survivor mods are not dependencies.

Universal Survivor Unlocks is designed to detect compatible modded survivors dynamically instead of requiring a hardcoded list.

## RealerCheatUnlocks

Universal Survivor Unlocks is compatible with RealerCheatUnlocks.

RealerCheatUnlocks can be useful during testing to manually grant or revoke the generated survivor unlock using its normal character selection controls.

RealerCheatUnlocks is optional and is not required to run Universal Survivor Unlocks.

## Installation

Install the mod using r2modman or Thunderstore Mod Manager.

Required dependencies will be installed automatically when installing the mod with dependencies.

For manual installation, place the mod files inside your Risk of Rain 2 BepInEx installation.

## Dependencies

Universal Survivor Unlocks requires:

- BepInExPack
- R2API ContentManagement
- R2API Language
- R2API Unlockable

Exact dependency versions are defined in the Thunderstore `manifest.json`.

## Troubleshooting

If a newly installed modded survivor is not detected:

- Make sure the survivor mod itself loads correctly.
- Make sure all required dependencies are installed.
- Check the BepInEx log for Universal Survivor Unlocks messages.
- Check whether the survivor already provides its own unlock requirement.
- Check `Survivors.json` to confirm whether the survivor was detected.

If `Survivors.json` contains invalid JSON syntax, the mod will avoid overwriting the invalid configuration so it can be corrected manually.

## Planned features

Future development may include:

- Functional gameplay challenge tracking.
- Additional challenge types.
- Boss-related unlock conditions.
- Elite enemy challenges.
- Stage progression challenges.
- Difficulty-based challenges.
- Run completion challenges.
- Custom challenges for exceptionally powerful modded survivors.
- Additional compatibility testing with survivor frameworks and character packs.

## Source code

Source code:

https://github.com/Danileonardo/Rain2CharacterUnlock

Issues and compatibility reports can also be submitted through the GitHub repository.

## License

Universal Survivor Unlocks is licensed under the MIT License.

See the included `LICENSE` file for details.
