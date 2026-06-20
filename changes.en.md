# CuStats Changelog

> 中文更新日志：见 [changes.md](changes.md)

## v1.0.1

- "KrokMP compatibility" Compatible with KrokMP3 and KrokMP4.

## v1.0.0

Initial release.

### Added

- **General + category panel**: General tab covers distance walked, layers descended, deepest layer, total blocks broken / items picked / items used, mobs killed, survivors met. Six category tabs follow (blocks broken, items picked up, food, medical, combat, misc), plus Settings and About.
- **Total / per-save dual scope**: top toggle switches between "Total" (across every save) and "This World" (current run). Per-world keyed by character `cId` (singleplayer) or client persistent id (KrokMP); written to `global.json` and `perSave/<saveKey>.json` as two independent JSON files.
- **Runtime classification**: food detection listens for `Body.Eat` inside a `Body.UseItem` window and reclassifies the active item into the food partition, sidestepping the game's irregular tag system. Medical / combat / misc decided by component + category.
- **Durability / count dual metric**: usage tabs have a one-click toggle between "durability equivalent" (`condition` delta × 100, full consumption ≈ 100) and "use count" (+1 per use); both metrics are written in parallel without rewriting history.
- **Mob kill attribution**: player melee / gunfire timestamps with a 1.5-second window and range check (melee ≤ reach + 1 m, gunfire ≤ 100 m) credit the kill to the player.
- **Survivor encounters**: incremented when `TraderScript.MeetPlayer` fires; the game's own `startedConvo` guard ensures one count per encounter.
- **Draggable ModalWindow**: saveManager-style GUI.matrix scaling + drag; switches to embedded path automatically when running inside the saveManager sidebar (no inner window frame in that case).
- **Sidebar embed + standalone**: registers as a sidebar tab via `ExternalTabRegistry` when CuSaveManager is present; otherwise injects a main-menu "Statistics" button and shows its own ModalWindow.
- **Item icons + localized names**: `IconCache` unifies item / block sprite lookup (atlas sub-rect aware), `NameResolver` reads through the game `Locale` and follows the configured UI language.
- **About tab**: SkinSync-style centered layout listing version / repo / latest release / author / dependencies.
- **UI language**: settings / About lets you pick auto / Chinese / English; written to `I18n.PreferredLanguage` and persisted across restarts. Auto mode picks by game Locale keyword.
- **Update check**: pulls the latest GitHub release on startup and shows a red notice top-left when behind; click to open the release page. Toggleable in settings.
