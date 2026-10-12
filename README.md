# MdsBuildTools

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for **[My Dream Setup](https://store.steampowered.com/app/2200780/My_Dream_Setup/)** that adds the building tools the community keeps asking for: grid snapping, alignment, and objects that stay on top of the floor.

**Every feature can be toggled.** Use a hotkey in-game, the Mod Settings menu, or the config file.

> ⚠️ **Beta — v0.2.0 hooks into the game but has not been play-tested yet.** If something behaves oddly, toggle the feature off with its hotkey and [open an issue](../../issues) with your `BepInEx/LogOutput.log`.

## Features

| Feature | Default | Hotkey | What it does |
|---|---|---|---|
| **Grid snapping** | ON | `G` | Snaps position to a grid (default 5 cm), in room space so it still works when the room is rotated. Floor objects snap on X/Z, wall objects on the wall plane. Scroll rotation uses your step (default 15°) instead of the game's fixed 45°. `Shift`+scroll stays fine-grained. |
| **Alignment** | ON | `H` | Lines up the edges/centre of the object you're moving with nearby placed furniture (within 3 cm by default). Takes priority over the grid when both apply. |
| **Keep above floor** | ON | `J` | Some models have their pivot above their base, so the game sinks them into the floor. This raises them until they rest on it. |
| **Bypass** | — | hold `Left Alt` | Turns off snapping and alignment while held, for free placement. |

A short on-screen message confirms each toggle. You can turn it off with `ShowToggleToasts`.

### Status

| Feature | Core logic | Game hook |
|---|---|---|
| Grid snapping (position + rotation) | ✅ tested | ✅ wired, not play-tested |
| Alignment | ✅ tested | ✅ wired, not play-tested |
| Keep above floor | ✅ tested | ✅ wired, not play-tested |
| Hotkeys / toggles / settings menu | ✅ | — |
| Exact wall lengths | parser ✅ | 🔜 planned (see below) |

### Why no exact wall lengths yet?
The game's room editor is built on a fixed 1 m grid. The blueprint has 14×8 points spaced exactly 1 m apart, every wall is a 1 m segment, and floor/ceiling are generated as 1 m tiles. Supporting arbitrary lengths means rewriting room generation (walls, floor, ceiling, colliders, shadows, save format), not just patching a value. It's planned as a separate, opt-in feature.

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
| General | `BypassKey` | `LeftAlt` | Hold to place freely. |
| General | `ShowToggleToasts` | `true` | |

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet test                                     # core unit tests
dotnet build src/MdsBuildTools.Plugin -c Release  # -> src/MdsBuildTools.Plugin/bin/Release/MdsBuildTools.dll
```

- With no extra setup it compiles against stock `UnityEngine.Modules`. ModSettingsAPI is downloaded from Thunderstore at build time and is not redistributed in this repo.
- The plugin binds to game code by reflection at runtime, so building doesn't need the game. To check that a new game version still has every member the mod uses, run:
  ```bash
  dotnet run --project tests/GameRefsCheck -- "<game>/MDS_Data/Managed/Assembly-CSharp.dll"
  ```
- Optionally, to compile against the real game DLLs, create `src/MdsBuildTools.Plugin/GameFolder.props`. It's gitignored.
  ```xml
  <Project><PropertyGroup>
    <GameFolder>C:\Program Files (x86)\Steam\steamapps\common\My dream setup\MDS</GameFolder>
  </PropertyGroup></Project>
  ```
  This defines `HAS_GAME` and references `MDS_Data/Managed/*`. If a publicized `Assembly-CSharp_publicized.dll` is present, it gets used instead; see [MdsAutoPublicize](https://github.com/My-Dream-Setup-Modding/MdsAutoPublicize).

### Layout

```
src/MdsBuildTools.Core     pure C# math (snap, floor clamp, alignment, length parsing), no Unity
src/MdsBuildTools.Plugin   BepInEx plugin; compiles Core in → ships as a single DLL
tests/                     xUnit tests for Core + GameRefsCheck (verifies game members exist)
```

## Credits

- [BepInEx](https://github.com/BepInEx/BepInEx) and [Harmony](https://github.com/pardeike/Harmony)
- [My-Dream-Setup-Modding](https://github.com/My-Dream-Setup-Modding) for ModSettingsAPI and MdsAutoPublicize

Not affiliated with the developers of My Dream Setup. No game files are included in this repository.

## License

[MIT](LICENSE)
