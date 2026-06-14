# CuStats

Casualties Unknown 的统计面板。仿 Minecraft 那种，按 U 打开看你这一局走了多远、挖了多少东西、被你打死了多少生物。

## 干什么

按 U 弹面板。顶部切 `总计 / 本世界`：本世界是当前存档（单机按角色 cId 隔离，KrokMP 多人按 client persistent id 隔离），总计是跨所有存档累加。再下来七个分类 tab + 一个设置 tab。

## 安装

要 BepInEx 5.x。把 `CuStats.dll` 丢到 `BepInEx/plugins/CuStats/`。

装了 [CuSaveManager](https://github.com/huanxin996/Cu-SaveManager) 的话，它的主面板里会多一个"统计"分页，跟按 U 看到的是同一个东西。

## 操作

| 键 | 作用 |
|---|---|
| U | 切换面板 |
| ESC | 关闭面板 |

## 都统计了啥

**综合**：移动距离、下降层数、最深层数、方块破坏总数、拾取物品总数、使用物品总数、击杀生物数、相遇求生者数。

**破坏方块**：地图里的真方块（按种类）和 BuildingEntity 那一类（发光草、家具、容器，原版游戏里走另一条销毁路径），合在一起显示。

**获得物品**：从地上、容器、击杀掉落处拿回身上的算。同一局里把同一个物品丢出去再捡回来不会重复加（按 InstanceID 防重）；切层或重启游戏后实例 ID 重置，那时候世界里剩下的物品再被你捡到会算新的。

**使用物品**分四档，运行时关联判定，不靠 tag 列表：
- 食物：`useAction` 调用过 `Body.Eat` 的（植物纤维、星叶果这种 tag 不带 fruit 的也能正确归到这）
- 医疗：`usableOnLimb` 或 category 是 `medical` / `drug` 的
- 战斗：挂了 `GunScript` / `AmmoScript` / `MineScript` 这类组件的
- 杂物：上面都不沾的

## 设置 tab

- **详细日志输出**：默认关。开了之后 Info 级日志才会进 BepInEx 的 LogOutput.log，平时关掉省得刷屏。Warn 和 Error 不受这个开关影响。
- **字号缩放**：0.6 到 2.0，整个面板字体一起缩。建议 0.85 起，再小 tab 文字会被挤。
- **启动时检查更新**：默认开。启动时拉一次 GitHub releases，版本旧了会在 BepInEx 控制台输出一行 Warn。

设置在 `BepInEx/config/com.mod.casualties.stats.cfg` 里也能直接编辑，UI 改完会同步落盘。

## 数据存哪

- `BepInEx/plugins/CuStats/global.json` — 跨存档总计
- `BepInEx/plugins/CuStats/perSave/<saveKey>.json` — 当前世界明细

写盘是 30 秒节流的，关游戏前会再 Flush 一次。

## 已知坑

击杀生物数现在是"任何非玩家 Body 死亡"都算 +1。被陷阱炸死的、踩进辐射圈昏迷死的、从高处摔死的也算在你头上——严格说不全是你打死的。如果觉得别扭就开 issue 我加 `lastDamager` 跟踪。

字号开到 0.6 那种极小情况下，scope 切换按钮里的中文会被挤变形。0.85 以上没问题。

## 自己编译

```powershell
dotnet build mods\CuStats\CuStats.csproj -c Release
powershell -ExecutionPolicy Bypass -File mods\CuStats\deployDll.ps1
```

`deployDll.ps1` 默认 `-GameRoot` 是 `E:\SteamLibrary\steamapps\common\Casualties Unknown Demo`，不在那就传参覆盖。

## 致谢

UI 套了 [CuSaveManager](https://github.com/huanxin996/Cu-SaveManager) 的 BlackWhiteSkin。

## License

MIT
