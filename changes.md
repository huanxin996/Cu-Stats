# CuStats 更新日志

> English changelog: see [changes.en.md](changes.en.md)

## v1.0.1

- ""兼容KrokMP"" 兼容KrokMP3与KrokMP4

## v1.0.0

首发版本。

### 新增

- **综合 / 分类双层面板**：综合页含移动距离、下降层数、最深层数、方块破坏 / 物品拾取 / 物品使用总数、击杀生物数、相遇求生者数；下面再分破坏方块、获得物品、食物、医疗、战斗、杂物六个分类页 + 设置 + 关于。
- **跨存档与本世界双维度**：顶栏一键切「总计 / 本世界」。本世界按角色 `cId`（单机）或 client persistent id（KrokMP 多人）隔离，总计跨所有存档累加；落盘到 `global.json` 与 `perSave/<saveKey>.json` 两份独立 JSON。
- **运行时分类判定**：食物分类靠在 `Body.UseItem` 期间监听 `Body.Eat`，命中即把当前物品归入食物分区，避开游戏不规整的 tag 系统。医疗 / 战斗 / 杂物按组件 + category 判定。
- **耐久 / 次数双视角**：使用类四个分页顶栏一键切「耐久件数」（按 condition 减量 ×100，一次完整消耗 ≈ 100）与「使用次数」（每次使用 +1）；两份维度并行落盘，不重写历史数据。
- **击杀归因**：玩家近战 / 开火时间戳 + 1.5 秒时间窗 + 距离判定（近战 ≤ 攻击距离 + 1m，开枪 ≤ 100m）认定为玩家击杀，计数到「击杀生物数」。
- **相遇求生者**：`TraderScript.MeetPlayer` 触发时 +1，借游戏自身的 `startedConvo` 守卫天然每次相遇仅计一次。
- **可拖动 ModalWindow**：仿 saveManager 风格的 GUI.matrix 自适应缩放 + 拖动；嵌入 saveManager 侧栏时自动切到嵌入路径，不画自身窗口框。
- **侧栏嵌入与独立模式**：检测到 CuSaveManager 通过 `ExternalTabRegistry` 注册为侧栏分页；不在则注入主菜单「统计」按钮并以独立 ModalWindow 显示。
- **物品图标与本地化名**：`IconCache` 统一拿物品 / 方块 sprite（含 atlas 子区域），`NameResolver` 走游戏 Locale，跟随设置语言切换。
- **关于分页**：仿 SkinSync 居中风格列出版本号 / 仓库 / 最新发布 / 作者 / 依赖。
- **界面语言**：设置 → 关于内可切「自动 / 中文 / English」，写入配置 `I18n.PreferredLanguage`，重启后保留；自动模式按游戏 Locale 关键词识别。
- **更新检测**：启动时拉一次 GitHub releases，新版在屏幕左上红字提示，点击打开 release 页；可在设置关闭。
