# MdsBuildTools

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for **[My Dream Setup](https://store.steampowered.com/app/2200780/My_Dream_Setup/)** that adds the building tools the community keeps asking for: grid snapping, alignment, objects that stay on top of the floor, and exact wall lengths.

**Every feature can be toggled.** Use a hotkey in-game, the Mod Settings menu, or the config file.

> ⚠️ **Work in progress — v0.1.0 is a skeleton.** The settings, hotkeys and core math work and are unit-tested, but the hooks into the game's placement/wall code are not wired yet. Don't install it expecting the features to work until a release says so.

## Features

| Feature | Default | Hotkey | What it does |
|---|---|---|---|
| **Grid snapping** | ON | `G` | Snaps position (X/Z) to a grid (default 5 cm) and rotation to steps (default 15°). |
| **Alignment** | ON | `H` | Lines up the edges/centre of the object you're moving with nearby objects and walls (within 3 cm by default). |
| **Keep above floor** | ON | `J` | When an object sinks into the floor, it gets pushed back up onto the surface. |
| **Exact wall lengths** | ON | — | Type a wall length (`3.47`, `3,47 m`, `347cm`, `3470 mm`) instead of being stuck with 1 m steps. Press `L` while editing a wall. |
| **Bypass** | — | hold `Left Alt` | Turns off snapping and alignment while held, for free placement. |

A short on-screen message confirms each toggle. You can turn it off with `ShowToggleToasts`.

### Status

| Feature | Core logic | Game hook |
|---|---|---|
| Grid snapping | ✅ tested | ⏳ |
| Alignment | ✅ tested | ⏳ |
| Keep above floor | ✅ tested | ⏳ |
| Exact wall lengths | ✅ tested (parser) | ⏳ |
| Hotkeys / toggles / settings menu | ✅ | — |

## Install

1. Install **BepInExPack** for My Dream Setup. The easiest way is a mod manager: [r2modman](https://thunderstore.io/c/my-dream-setup/p/ebkr/r2modman/) or [Gale](https://thunderstore.io/c/my-dream-setup/p/Kesomannen/GaleModManager/).
2. *(Optional, recommended)* Install [ModSettingsAPI](https://thunderstore.io/c/my-dream-setup/p/Endskill/ModSettingsAPI/) to get an in-game settings tab.
3. Download `MdsBuildTools.dll` from [Releases](../../releases) and drop it in `My Dream Setup/BepInEx/plugins/`.
4. Launch the game.

## Configuration

After the first launch, settings live in `BepInEx/config/com.miguelneves.mdsbuildtools.cfg`. With ModSettingsAPI installed you can also change them in the in-game Mod Settings tab.

| Section | Key | Default | Range / notes |
|---|---|---|---|
| Grid | `Enabled` | `true` | |
| Grid | `ToggleKey` | `G` | Any Unity `KeyCode`. Set it to `None` to unbind. |
| Grid | `StepCm` | `5` | 0.5 – 100 |
| Grid | `AngleStepDeg` | `15` | 1 – 90 |
| Alignment | `Enabled` | `true` | |
| Alignment | `ToggleKey` | `H` | |
| Alignment | `ThresholdCm` | `3` | 0.5 – 20 |
| Floor | `Enabled` | `true` | |
| Floor | `ToggleKey` | `J` | |
| Walls | `Enabled` | `true` | |
| Walls | `ToggleKey` | `None` | |
| Walls | `LengthKey` | `L` | Opens the length input while editing a wall. |
| General | `BypassKey` | `LeftAlt` | Hold to place freely. |
| General | `ShowToggleToasts` | `true` | |

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet test                                     # core unit tests
dotnet build src/MdsBuildTools.Plugin -c Release  # -> src/MdsBuildTools.Plugin/bin/Release/MdsBuildTools.dll
```

- With no extra setup it compiles against stock `UnityEngine.Modules`. ModSettingsAPI is downloaded from Thunderstore at build time and is not redistributed in this repo.
- To build against the real game, which is needed for the game hooks, create `src/MdsBuildTools.Plugin/GameFolder.props`. It's gitignored.
  ```xml
  <Project><PropertyGroup>
    <GameFolder>C:\Program Files (x86)\Steam\steamapps\common\My Dream Setup</GameFolder>
  </PropertyGroup></Project>
  ```
  This defines `HAS_GAME` and references `MDS_Data/Managed/*`. If a publicized `Assembly-CSharp_publicized.dll` is present, it gets used instead; see [MdsAutoPublicize](https://github.com/My-Dream-Setup-Modding/MdsAutoPublicize).

### Layout

```
src/MdsBuildTools.Core     pure C# math (snap, floor clamp, alignment, length parsing), no Unity
src/MdsBuildTools.Plugin   BepInEx plugin; compiles Core in → ships as a single DLL
tests/                     xUnit tests for Core
```

## Credits

- [BepInEx](https://github.com/BepInEx/BepInEx) and [Harmony](https://github.com/pardeike/Harmony)
- [My-Dream-Setup-Modding](https://github.com/My-Dream-Setup-Modding) for ModSettingsAPI and MdsAutoPublicize

Not affiliated with the developers of My Dream Setup. No game files are included in this repository.

## License

[MIT](LICENSE)
