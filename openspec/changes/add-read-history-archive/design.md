# Design: add-read-history-archive

## Context

- 正式 UI 常驻于 `Persistent.unity` 的 `FormalUI` Canvas，使用 uGUI、`UnityEngine.UI.Text` 和旧 Input API。
- `TutorialPopup` 已提供 Sprite 页翻阅、暂停时间、光标切换和羊皮纸音效；教程内容分开局、检查点和 Level_04B 三组人/狗页面。
- `NoticeBoard` 已在正式关卡中出现 15 次，按 F 将角色专属 Sprite 数组传给 `TutorialPopup.ShowOnce`。当前 `hasBeenRead` 不持久化，且关卡卸载后对应 Sprite 数组不可通过场景对象访问。
- `PauseMenu` 使用 Esc 并自行管理 `Time.timeScale`、HUD 和光标。`TutorialPopup.IsShowing` 与 `DeathScreen.IsShowing` 已用于避免 Esc 重入。

## Goals / Non-Goals

**Goals:**

- 玩家可随时用 B 查看本存档中已读剧情和已触发操作提示。
- 所有可回看内容在关卡卸载后仍然可解析，并保留原有页面顺序和角色差异。
- 发现记录跨场景、跨运行保存，旧教程存档继续有效。
- UI 状态切换不破坏暂停、教程、死亡、光标或 HUD 状态。
- 迁移后的 18 组来源内容可被编辑器工具完整校验。

**Non-Goals:**

- 不显示未发现条目的问号占位、总收集进度或成就。
- 不新增正文文本编辑系统、OCR、搜索、排序选项或云存档。
- 不重绘现有提示图片，不改变公告牌 F 键交互距离与提示文案。
- 不在主菜单、过场动画或死亡画面开放档案。

## Decisions

### D1. 使用 ScriptableObject 中央目录保存跨场景内容

新增 `ReadingArchiveCatalog` 资产，条目包含稳定 `id`、中文 `title`、`category`（Story/Controls）、`role`（Human/Dog）和有序 `Sprite[] pages`。运行时档案只引用该目录，不依赖当前加载关卡中的 NoticeBoard。

同一逻辑来源的人/狗版本使用相同基础 ID 与不同角色维度，而不是复制含角色后缀的自由字符串。发现键按 `(id, role)` 存储，避免狗读过后自动解锁人类版本。

### D2. NoticeBoard 保存稳定 ID，页面以目录为权威来源

`NoticeBoard` 增加 `archiveEntryId`。交互时根据当前角色从目录读取页面；成功打开后登记 `(archiveEntryId, role)`。迁移期间可以保留现有序列化页面作为编辑器采集来源，但完成构建后运行时不再依赖场景数组，避免目录与场景内容漂移。

编辑器工具负责从现有场景采集 15 个公告的页面引用、生成目录条目并回写 ID；ID 使用明确的关卡与序号，例如 `story.level01.01`，不使用 GameObject instance ID 或显示标题。

### D3. 教程组使用固定稳定 ID，并兼容旧 PlayerPrefs

三组教程固定为 `controls.opening`、`controls.checkpoint`、`controls.level04b`。教程成功显示时立即登记当时角色版本；档案回看调用不写旧的“已展示”键，也不会令尚未自然触发的教程以后不再弹出。

加载发现记录时，如果旧教程 PlayerPrefs 已为 1、但新发现集合没有对应条目，则为已有内容页面的角色做一次兼容迁移。由于旧键未记录当时角色，迁移默认解锁 Human 版本；之后自然触发 Dog 版本时再独立解锁。

### D4. 发现集合使用单个版本化 PlayerPrefs JSON

新增 `ReadingArchiveProgress`，以一个版本化 JSON 字符串保存发现键集合，写入后立即 `PlayerPrefs.Save()`。读入时去重、忽略目录中不存在的陈旧 ID；目录条目改名不得改变稳定 ID。

该存储封装提供 `Discover`、`IsDiscovered` 和按分类/角色查询接口，UI 与触发脚本不直接拼 PlayerPrefs 键。

### D5. 档案 UI 是包含导航与阅读区的一体化面板

`ReadingArchiveController` 常驻 FormalUI：

- B 在允许状态下打开档案列表，默认进入上次选择的分类，首次默认为剧情资料。
- 分类按钮重建当前角色的已发现条目；条目区域保持为可滚动导航列表，没有条目时显示“尚未阅读任何内容”。
- 同一 B 面板内设置独立阅读区，包含当前标题、页面图片、页码、上一页与下一页按钮；点击条目只更新阅读区，不隐藏或替换档案根面板。
- 图片按阅读区等比缩放完整显示，多页内容通过面板内按钮以及 A/D 或左右方向键翻页；到达边界时禁用对应翻页按钮。
- 阅读区直接使用目录 Sprite，不调用 `TutorialPopup`，因此不存在内容面板被档案面板遮挡的问题，也不会写教程 PrefKey 或新增发现。
- B 或 Esc 始终关闭整个档案；切换分类时清空旧选择并显示“请选择一条内容”。

条目按钮使用一个序列化模板在运行时实例化，避免为 18 个来源手工维护 18 个按钮。宽屏布局采用左侧滚动列表、右侧阅读区；窄屏时仍保证阅读图片不被列表覆盖。

### D6. UI 模态状态显式互斥

档案只在未显示 TutorialPopup、DeathScreen、PauseMenu/设置且当前存在可操控角色时打开。打开后设置 `ReadingArchiveController.IsShowing`，`PauseMenu` 和玩家交互入口读取该状态并停止响应。

控制器在打开时保存 `Time.timeScale`、光标显示/锁定状态和 HUD active 状态；关闭或销毁时幂等恢复保存值。档案从暂停状态打开被禁止，因此不会接管别的系统拥有的暂停。

### D7. Editor 工具构建并校验，不手写大量场景 YAML

新增 `Tools/DoNotLeaveMe/阅读档案/构建与迁移` 和 `校验`：前者遍历正式关卡、按确定顺序为 NoticeBoard 分配 ID、采集人/狗页面、构建目录并保存场景；后者检查：

- ID 非空且在 NoticeBoard 来源中唯一；
- 每个来源至少有一个有效角色版本；
- 目录中 `(id, role)` 唯一、标题非空、页面无 null；
- 三组教程已进入 Controls 分类；
- Persistent FormalUI 的目录、档案控制器和 UI 引用完整。

所有场景修改通过 Unity Editor API 完成并显式保存，避免无关场景重序列化。

## Risks / Trade-offs

- [目录复制了场景页面引用，可能内容漂移] → 运行时只认目录；Editor 校验对比来源并要求内容改动后重新构建。
- [旧教程键无法确定当时是人还是狗] → 兼容迁移只解锁 Human，并记录为明确的迁移规则。
- [多个系统都操作 timeScale] → 档案禁止从其他模态状态打开，并由一个控制器拥有保存/恢复生命周期。
- [B 与未来输入绑定冲突] → 当前代码库无 KeyCode.B；本期沿用旧 Input API，后续输入系统重构时再集中映射。
- [15 个 NoticeBoard 的稳定排序和标题需要人工语义] → Editor 工具以关卡路径和层级稳定排序生成 ID，标题优先使用显式字段；构建后人工审阅目录资产标题。

## Migration Plan

1. 新增目录和发现存储脚本及其 Editor 测试。
2. 扩展 TutorialPopup 回看模式，并接入 NoticeBoard/教程发现登记。
3. 运行 Editor 构建工具迁移 15 个 NoticeBoard 和三组教程，人工审阅标题。
4. 在 Persistent FormalUI 构建档案面板并连接控制器。
5. 编译、校验、Play Mode 验证 B/ESC、分类、角色隔离、跨场景和旧存档兼容。
6. 回滚时恢复脚本、目录资产和相关场景；新 PlayerPrefs 字符串可安全遗留，不影响旧版本运行。

## Open Questions

- 无。视觉细节沿用现有羊皮纸 UI 风格，条目只显示中文标题，不新增缩略图。
