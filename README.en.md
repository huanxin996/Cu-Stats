# CuStats V1.0.1 for Casualties: Unknown

> A Minecraft-style statistics panel mod for Casualties: Unknown
> Compatible with: CuSaveManager (settings merge into its panel when present; its absence does not break features)

中文说明：见 [README.md](README.md)

## Overview

Press U to open the panel. MC-style stats: how far you walked this run, how many blocks you cracked, how many mobs you killed. The top toggles `Total / This World` — "this world" is the current save (singleplayer keyed by character `cId`, KrokMP keyed by client persistent id), "total" is summed across every save. Below that: seven category tabs + Settings + About.

## Changelog

Full release history lives in [changes.en.md](changes.en.md) (中文：[changes.md](changes.md)). Builds are published at [Releases](https://github.com/huanxin996/Cu-Stats/releases).

## Install

BepInEx 5.x required. Drop `CuStats.dll` under `BepInEx/plugins/CuStats/`.

If [CuSaveManager](https://github.com/huanxin996/Cu-SaveManager) is installed, its panel gains a "Statistics" sidebar tab — same panel as U opens. Without CuSaveManager the mod injects its own main-menu button and ModalWindow.

## Controls

| Key | Action |
|-----|--------|
| U | Toggle panel (rebindable in settings) |
| ESC | Close panel |

## What it tracks

**General**: distance walked, layers descended, deepest layer, total blocks broken, items picked / used, mobs killed, survivors met.

**Blocks broken**: real world blocks (by type) and BuildingEntity-class destructibles (glowplant, furniture, containers — the game routes these through a different destruction path), shown together.

**Items picked up**: items pulled from the ground / containers / kill drops onto your body. Throwing the same item out and picking it back up in the same session does not double-count (deduped by `InstanceID`); after a layer change or game restart, instance IDs reset and any items still in the world will count as new on pickup.

**Items used**, four buckets, decided at runtime — no tag list:

- Food: `useAction` calls `Body.Eat` during use (handles items whose tags don't include `fruit`, like geofruit / starleaf).
- Medical: `usableOnLimb` or category is `medical` / `drug`.
- Combat: items with a `GunScript` / `AmmoScript` / `MineScript` component.
- Misc: anything that didn't fit above.

Each usage tab has a one-click toggle between "durability equivalent" (`1.02 items`) and "use count" (`3 times`); both metrics are written in parallel.

**Mob kills** only count what you killed: melee (`Body.Attack`) and gunfire (`GunScript.Fire`) timestamp the most recent player attack; when a `BuildingEntity` with `animal=true` dies, +1 if you struck within the last 1.5 seconds at a reasonable range (melee ≤ attack reach + 1 m, gunfire ≤ 100 m).

## Settings

- **Verbose log output**: off by default. When on, Info-level logs go into BepInEx `LogOutput.log`. Warn / Error always log.
- **Font scale**: 0.6 to 2.0.
- **Check updates on startup**: on by default. Pulls the latest GitHub release on launch and shows a red notice top-left if behind; click to open the release page.
- **Toggle hotkey**: default U, rebindable.
- **UI language**: auto / Chinese / English; auto picks by game Locale keyword.

Settings can also be edited directly in `BepInEx/config/com.mod.casualties.stats.cfg`; UI changes are written back automatically.

## Data files

- `BepInEx/plugins/CuStats/global.json` — totals across every save
- `BepInEx/plugins/CuStats/perSave/<saveKey>.json` — current world only

## Soft dependencies

- **CuSaveManager**: when present, this mod registers its panel as a sidebar tab via its extension point for a unified style; when absent or on a version without the extension point, it falls back to a standalone main-menu button + its own ModalWindow.

## Build

```powershell
dotnet build CuStats.csproj -c Release
```

Output goes to `bin/Release/CuStats.dll`; copy it to `BepInEx/plugins/CuStats/`.

Or grab a prebuilt dll from [Releases](https://github.com/huanxin996/Cu-Stats/releases) / GitHub Actions artifacts.

## Related

- [CasualtiesUnknown-SkinEditor](https://github.com/huanxin996/CasualtiesUnknown-SkinEditor): live preview and animation preview.
- [huanxin996/Cu-SaveManager](https://github.com/huanxin996/Cu-SaveManager): multi-save, backup, panel extension point.
- [huanxin996/Cu-Hotbar](https://github.com/huanxin996/Cu-Hotbar): customizable hotbar with item swapping and quick-use.

## License

MIT
