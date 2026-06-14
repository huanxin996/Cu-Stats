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

**击杀生物**只计你亲手干掉的：近战（`Body.Attack`）和开枪（`GunScript.Fire`）会记最近一次时间戳，动物 `BuildingEntity` 销毁时若你最近 1.5 秒内出过手且距离够近（近战 ≤ 攻击距离 + 1m，开枪 ≤ 100m）就 +1

## 设置 tab

- **详细日志输出**：默认关。开了之后 Info 级日志才会进 BepInEx 的 LogOutput.log。Warn 和 Error 不受这个开关影响。
- **字号缩放**：0.6 到 2.0。
- **启动时检查更新**：默认开。启动时拉一次 GitHub releases，版本旧了在屏幕左上红字提示，可点直跳 release 页。

设置在 `BepInEx/config/com.mod.casualties.stats.cfg` 里也能直接编辑，UI 改完会同步落盘。

## 数据存哪

- `BepInEx/plugins/CuStats/global.json` — 跨存档总计
- `BepInEx/plugins/CuStats/perSave/<saveKey>.json` — 当前世界明细

## 编译

```powershell
dotnet build CuStats.csproj -c Release
```

产出在 `bin/Release/CuStats.dll`，复制到 `BepInEx/plugins/CuStats/` 即可。

也可以直接到 [Releases](https://github.com/huanxin996/Cu-Stats/releases) 拿构建好的 dll，或下载 GitHub Actions 的 artifact。

## License

MIT
