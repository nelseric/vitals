# Vitals

A client-side Valheim mod that plays sounds for things you might miss on screen:

- **Low health:** two sharp beeps when health drops below 40% of max, repeated every 10 seconds while it stays low. Health has to climb 5 points above the threshold before a new drop beeps again, so regen hovering at the line stays quiet.
- **Food ran out:** a soft falling chime when a food buff expires. Death and cleansing effects don't trigger it.

The sounds are generated in code (no audio files) and are scaled by the game's master volume.

## Install

Needs [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) (see Helmsman's README for setup). Copy `Vitals.dll` to `BepInEx/plugins/Vitals/`.

## Configuration

Settings live in `BepInEx/config/vitals.alerts.cfg`, created on first launch. With [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager), they can be changed in-game (F1).

| Section | Setting | Default | Meaning |
| --- | --- | --- | --- |
| LowHealth | `Enabled` | `true` | Beep when health is low |
| LowHealth | `Threshold` | `0.4` | Fraction of max health below which the alarm sounds |
| LowHealth | `RepeatSeconds` | `10` | Beep again this often while low (0 = once per drop) |
| LowHealth | `Volume` | `0.6` | Alarm volume |
| FoodExpired | `Enabled` | `true` | Chime when a food buff runs out |
| FoodExpired | `Volume` | `0.5` | Chime volume |

## Build

```
dotnet build -c Release
dotnet test -c Release --no-build
```

The build copies `Vitals.dll` into `~/.local/share/Steam/steamapps/common/Valheim/BepInEx/plugins/Vitals` if that folder exists (override with `-p:ValheimDir=...`).
