# Proposal: add-read-history-archive

## Why

正式流程已经包含 15 个可阅读公告、纸条和报告，以及开局、存档地毯、Level_04B 三组操作提示，但这些内容只能在触发位置当场查看。`NoticeBoard.hasBeenRead` 只在当前场景对象内存中存在，换场景后丢失；`TutorialPopup` 的 PlayerPrefs 也只记录三组教程是否弹过，不能组成可浏览目录。玩家因此无法回顾已经发现的剧情与操作说明。

## What Changes

- 在游戏内增加由 B 键打开的“阅读档案”界面，提供“剧情资料”和“操作提示”两个分类；条目导航与内容阅读器同处一个面板，内容不会再以外层 Popup 覆盖档案。
- 档案只列出当前存档实际阅读或触发过的内容；未发现内容不显示标题或缩略信息。
- 为剧情和教程内容建立稳定 ID、标题、分类、角色版本与有序 Sprite 页面目录，使所属关卡卸载后仍可回看。
- 人与狗看到的角色专属页面分别解锁和保存；档案按当前操控角色显示其已经读过的版本。
- 将首次成功打开公告或教程的事件写入持久化发现记录，同时兼容现有三项教程 PlayerPrefs。
- B 打开档案后暂停玩法、显示鼠标并隐藏 HUD；B 或 Esc 关闭并精确恢复此前时间缩放和光标状态。
- 档案与教程 Popup、暂停菜单、设置、死亡界面互斥，避免多层 UI 同时响应同一个按键。
- 提供编辑器构建/校验工具，将现有 15 个 NoticeBoard 和三组教程迁移到统一目录并检查重复/空 ID、缺页与场景绑定遗漏。

## Capabilities

### New Capabilities

- `read-history-archive`: 定义已读内容发现、持久化、分类列表、角色版本回看、B 键开关和 UI 状态恢复契约。

### Modified Capabilities

- `formal-notice-board-pages`: NoticeBoard 在成功打开角色专属页面时必须登记稳定内容 ID，并从中央目录解析可跨场景回看的页面。
- `formal-ui-role-pages`: 三类角色专属教程在成功显示时必须登记对应内容 ID，并能被阅读档案重新打开且不改变首次教程触发状态。

## Impact

- **脚本**：新增档案目录、发现存储和档案 UI 控制器；修改 `NoticeBoard`、`TutorialPopup`、`PauseMenu` 的集成点；新增 Editor 构建与校验工具。
- **场景/UI**：修改 `Persistent.unity` 的 FormalUI，增加档案根面板、分类按钮、条目列表、空状态、关闭按钮和阅读预览入口。
- **数据**：新增一个项目资产形式的内容目录，以及 PlayerPrefs 中按角色保存的已发现 ID 集合；不删除现有教程 PlayerPrefs。
- **内容迁移**：为 15 个正式 NoticeBoard 和三组教程配置稳定 ID 与中文标题，保留现有 Sprite 顺序和人/狗差异。
