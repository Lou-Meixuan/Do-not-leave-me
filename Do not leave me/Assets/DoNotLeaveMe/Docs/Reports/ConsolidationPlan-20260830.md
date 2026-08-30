# 整合方案（2026-08-30 复查）

> 原为方案文档。**2026-08-30 已执行步骤 2～5**，执行结果与更正记在下方各节和文末的「执行记录」里。

## 先说结论

工程的大结构**不乱**。`00-目录结构.md` 已经把 `Assets/DoNotLeaveMe/`（逻辑）与 `Assets/Art/`（资源库）分好，
`_ArchivedAssets/` 移出 `Assets/` 也是对的。所以下面**不建议**再动这个三分结构。

真正还散着的是 6 处，按「风险从低到高」排。A、B、C 三条完全不碰 Unity 资产，可以立刻做。

---

## A. openspec 积压：59 个进行中，其中 23 个已完工未归档

风险：**零**（纯文档目录移动，Unity 不导入 `openspec/`）
收益：**最大**（`openspec/changes/` 目录一眼能看清还剩什么没做）

`tasks.md` 里已无未勾选项、可直接 `openspec archive` 的 23 个：

| change | 已完成任务数 |
|---|---|
| add-formal-dog-idle-and-follow-speed | 8 |
| add-formal-notice-board-pages | 4 |
| add-formal-ui-role-pages | 4 |
| add-meshcollider-sync-audit-tool | 8 |
| assemble-formal-level02-art | 10 |
| assemble-formal-level03-art | 9 |
| assemble-formal-level04-art | 10 |
| audit-formal-level03-collider-integrity | 7 |
| complete-formal-level03-nearby-art | 10 |
| defer-cross-level-unload | 7 |
| establish-formal-level-traversal-foundation | 14 |
| establish-formal-level03-traversal-foundation | 10 |
| expand-formal-level02-collider-coverage | 11 |
| expand-formal-level03-collider-coverage | 10 |
| expand-formal-level04-collider-coverage | 7 |
| extract-shared-formal-model-prefabs | 9 |
| fill-formal-level03-center-floor-gap | 9 |
| fix-formal-level03-camera-startup | 6 |
| improve-formal-level02-monster-patrol | 11 |
| optimize-superbreadman-art-assets | 27 |
| relocate-formal-level03-spawn-to-pad | 6 |
| separate-level02-door-preload-from-gm-transition | 8 |
| unify-formal-route-advance-protocol | 24 |

归档后 `openspec/changes/` 从 59 个降到 36 个，`archive/` 从 19 个升到 42 个。

> 注意：这 23 个的命名里还带 `formal` / `superbreadman` 字样，而 Step 4 已经把代码里的 `Formal` 前缀去掉了。
> 归档时**不要改名**——归档是历史记录，改名反而对不上当时的提交。

---

## B. 仓库根 `docs/` 三个文件是旧纪元产物，路径已失效

| 文件 | 大小 | 问题 |
|---|---|---|
| `docs/bakedmeshes-dedup-report.md` | 3.9 KB | 正文写的范围是 `UnityProject/Assets/MoMing/BakedMeshes/` |
| `docs/bakedmeshes-dedup-mapping.csv` | 119 KB | 每行路径都是 `MoMing\FormalLevels\Prefabs\...` |
| `docs/bakedmeshes-dedup-components.log` | 18.8 KB | 同上 |

`UnityProject/` 和 `Assets/MoMing/` 现在都**不存在**（已验证）。这三个是 08-23 那次 BakedMeshes 去重的操作记录。

建议：移进 `Assets/DoNotLeaveMe/Docs/Reports/`（那里已经放着同类的 `SharedModelExtractionReport.md`
等操作报告），并在报告开头补一行「路径为 2026-08-30 改名前的旧路径」。之后删掉空的仓库根 `docs/`。

同时删掉仓库根的空目录 `_to_delete/`（0 个文件）。

---

## C. `AGENTS.md` 整份失效

风险：**零**，但**必须做**——它是给 AI 助手看的入口文档，现在每条路径都是错的。

| AGENTS.md 里写的 | 实际 |
|---|---|
| `UnityProject/` 是工程根 | 工程根是 `Do not leave me/` |
| `UnityProject/Assets/` | `Do not leave me/Assets/` |
| `UnityProject/Assets/Scenes/Test/superbreadman.unity` | **不存在** |
| `UnityProject/Assets/MoMing/Scenes/Test/superbreadman.unity` | **不存在** |
| 「当前规划 change: align-superbreadman-level-scope」 | 这个 change 的目标场景已被归档 |

建议：重写 AGENTS.md，把 `## Project Layout` 一节直接指向 `Assets/DoNotLeaveMe/Docs/00-目录结构.md`
（那份是准的），`## SuperBreadMan Level Scope` 一整节删掉（该纪元已结束，场景在 `_ArchivedAssets/Scenes_SuperBreadManEra/`）。

---

## D. 三处 `Resources/` —— 建议只动一处

`Resources.Load` 的参数是相对**任意** `Resources/` 文件夹的路径，所以现在三处并存**运行时是正常的**，
不是 bug。但读代码的人会找不到东西。现状：

| 实际文件 | 加载路径 | 谁在调 |
|---|---|---|
| `Assets/Resources/AnxietyDirtMask.png` | `"AnxietyDirtMask"` | `Core/AnxietyOverlay.cs:102`、`Core/GameManager.cs:184` |
| `Assets/Resources/UIAdditive.shader` | `"UIAdditive"` | `Core/AnxietyOverlay.cs:115`、`Core/GameManager.cs:210` |
| `Assets/Resources/DoNotLeaveMe/WwiseUIFeedbackSettings.asset` | `"DoNotLeaveMe/WwiseUIFeedbackSettings"`（常量 `WwiseUIFeedbackSettings.cs:10`） | `Audio/FluorescentLightAudioRouter.cs:29`、`UI/WwiseUIFeedbackRouter.cs:47`、`UI/SettingsManager.cs:201` |
| `Assets/DoNotLeaveMe/Resources/Config/music_table.csv` | `"Config/music_table"` | **没有任何代码读它**（只有 `08-音乐系统.md` 提到要填这张表） |

**方案 D1（已于 2026-08-30 执行）**：只把 `Assets/DoNotLeaveMe/Resources/Config/` 挪到
`Assets/Resources/DoNotLeaveMe/Config/`，删掉 `Assets/DoNotLeaveMe/Resources/`。
之后全工程只剩 `Assets/Resources/` 一个 Resources 根。加载路径从 `Config/music_table`
变成 `DoNotLeaveMe/Config/music_table` —— 目前**没有代码依赖它**，所以现在改零风险；
等音乐系统真去读表时就已经是新路径了。要同步改 `08-音乐系统.md` 和 `00-目录结构.md` 里的路径说明。

**方案 D2（更整齐但要改代码）**：把 `AnxietyDirtMask.png` 和 `UIAdditive.shader` 也挪进
`Assets/Resources/DoNotLeaveMe/`，然后改 4 处字符串：
`AnxietyOverlay.cs` 的 `dirtOverlayResourceName` 默认值（:29）、`GameManager.cs` 的同名字段（:51）、
两处 `Resources.Load<Shader>("UIAdditive")`。
⚠️ 已确认 `Persistent.unity:164` 里序列化了 `dirtOverlayResourceName: AnxietyDirtMask`
（`public string` 字段，场景值会覆盖代码默认值）。所以改代码默认值**不够**，必须同时改这个场景。
风险明显高于 D1。

---

## E. Art 与 DoNotLeaveMe 的「重叠」——多数是设计如此，只有 5 个空壳/死链

先澄清：`UI`、`Video`、`Materials`、`Prefabs` 在两边都有，**这不是重复**。按 `00-目录结构.md`，
`Art/` 是美术源、`DoNotLeaveMe/` 是运行时逻辑资产，两边同名是分工不是冲突。**不建议合并。**

真正该清的是这 5 项：

| 项 | 现状 | 建议 |
|---|---|---|
| `DoNotLeaveMe/Video/` | **空目录**，视频实际在 `Art/Video/`（开场动画.mp4 50MB、结尾动画.mp4 75MB） | 删空目录（连 `.meta`） |
| `DoNotLeaveMe/Materials/` 的 12 个 `Mat_*` | ~~全部 0 引用~~ **更正（执行时发现）：只有 7 个无引用**。首轮检查从 `.meta` 取 guid 时没去掉 CRLF 的 `\r`，导致全部误判为 0 引用 | 已归档无引用的 7 个（Box/CheckpointActive/Dog/Floor/Gate/Human/Wall）；`Mat_Checkpoint`、`Mat_Switch`、`Mat_SwitchActive`、`Mat_Monster`、`Mat_SpotlightDarkness` 被 `L04_Content`/`L05_Content`/`Monster*`/`UI_System` 引用，**留在原处** |
| `DoNotLeaveMe/Shaders/SpotlightDarkness.shader` | ~~整条链是死的~~ **更正**：`Mat_SpotlightDarkness` 被 `Prefabs/UI_System.prefab` 引用，所以这条链是活的 | **保留，未归档** |
| `Art/Environment/Shaders/New Shader Graph.shadergraph` | 0 引用，名字还是 Unity 默认的 | 归档；若还要用先改个正经名字 |
| `Art/UI/10_HUD/` 下 `03_EscMenu`、`04_ControlsHint`、`05_ObjectiveLabel` | 3 个**空目录** | 删掉，或确认是占位符后在 `Docs` 里记一笔 |

`DoNotLeaveMe/UI/Tex_White.png` 保留 —— 它被 `Scripts/Editor/BreadManUIBuilder.cs` 用到，是运行时/编辑器资产不是美术源，位置正确。

---

## F. 两个 git 仓库套着 —— 这条最需要你先决定

这是目前最大的隐患，而且**不是文件整理能解决的**：

| | 外层 `Do-not-leave-me/` | 内层 `Do not leave me/` |
|---|---|---|
| remote | **有** `origin/main` | **无** |
| 当前分支 | `delete_used_files` | `master` |
| 最后提交 | `908b89ba` 2026-08-29 | `d1574267` 2026-08-30 |
| 跟踪文件数 | 10329（其中 9694 个在 `Do not leave me/` 下） | 8528 |

**同一批文件被两个仓库各自跟踪。** 而 08-30 那次大整理（Step 2b～Step 6：去 Formal 前缀、
SuperBreadMan→Art 重组、旧角色归档、`_Archive` 移出 Assets、737 处标识符改名）**只存在于内层仓库，
内层没有远端** —— 也就是说这些工作目前没有推到团队那边，只在这台机器上。

三个选项：

1. **把内层的整理成果合进外层**（外层有 origin，是团队仓库）。工作量不小：外层的
   `delete_used_files` 分支和内层 master 已经分叉，等于要把 08-30 的六步重放一遍或做一次大 squash 提交。
2. **让内层接管**：给内层配 remote 推成新分支，外层降级成历史备份。最快，但团队其他人的分支要重新对齐。
3. **先什么都不做，但立刻给内层加 remote 备份**。至少 08-30 的成果不会只躺在一块硬盘上。

我倾向先做 3 保住成果，再和团队定 1 还是 2。

---

## 建议执行顺序

| 步骤 | 内容 | 风险 | Unity 要不要重导入 |
|---|---|---|---|
| 1 | F-3：给内层仓库配 remote 做备份 | 无 | 否 |
| 2 | A：归档 23 个已完工 openspec change | 无 | 否 |
| 3 | B：仓库根 `docs/` 三文件移入 `Docs/Reports/`，删 `_to_delete/` | 无 | 否 |
| 4 | C：重写 AGENTS.md | 无 | 否 |
| 5 | E：删 4 个空目录、归档 12 个死材质 + 2 个死 shader | 低（已验证 0 引用） | 是（资产变动） |
| 6 | D1：`Resources/Config/` 归位 + 改两处文档 | 低（无代码依赖） | 是 |
| 7 | F-1 或 F-2：两个仓库合一 | 高，需团队参与 | 视方案 |

第 5、6 步动 Unity 资产前请先**关闭 Unity 编辑器**，并确保内层仓库工作区是干净的
（目前 `git status` 有 994 项未提交，建议先提交或 stash，否则出问题不好回滚）。

---

## 执行记录（2026-08-30）

| 步骤 | 状态 | 结果 |
|---|---|---|
| 1 外层仓库推送 | **待你决定** | 外层本地有 4 个提交未推到 `origin/main`。另更正：外层仓库**已包含**全部 08-30 整理成果，原方案「成果只在内层」的判断是错的；内层是冗余重复仓库（974MB） |
| 2 openspec 归档 | 已完成 | 归档前补做了 spec 同步：新建 25 份主 spec、`formal-level-collider-normalization` 追加 2 条需求、`formal-level02-physical-door-transition` 已同步过未重复写。进行中 change 59→36，归档 19→42，主 spec 18→43 |
| 3 清仓库根 | 部分完成 | 三个报告已移入本目录并改名（`BakedMeshesDedup*`），`.log` 改 `.txt` 以免被 `.gitignore` 忽略。空目录 `docs/`、`_to_delete/` **需你手动删**（本会话无删除权限） |
| 4 重写 AGENTS.md | 已完成 | 路径全部修正；新增「两个 Git 仓库」警告；记下 `openspec` CLI 未安装及手动归档流程；删掉失效的 SuperBreadMan 一节 |
| 5 归档死资产 | 已完成（范围缩小） | 归档 7 个材质 + `Art/Environment/Shaders/`；5 个材质和 `SpotlightDarkness.shader` 经复查在用，保留。4 个空目录移到 `_to_delete/EmptyFolders-20260830/` |
| 6 Resources 归位 | 已完成 | 按 D1 执行：`Config/` 移到 `Assets/Resources/DoNotLeaveMe/Config/`，全工程只剩一个 Resources 根。移动前已用去 CRLF 的 GUID 扫描确认 `music_table.csv` 无任何引用。同步更新 `00-目录结构.md`（新增 Resources 布局图）、`01-项目规范.md`、`08-音乐系统.md`。D2 未执行 |

### 教训：GUID 检查必须去掉 CRLF

工程里的文本资产是 CRLF 行尾。`grep "^guid:" x.meta | cut -d' ' -f2` 取出来的字符串**末尾带 `\r`**，
拿它去搜会一个都搜不到，于是「在用的资产」被判成「无引用」。正确写法要加 `tr -d '\r'`：

```bash
g=$(grep -m1 "^guid:" "$asset.meta" | cut -d' ' -f2 | tr -d '\r')
grep -rlF "$g" Assets/
```

另外逐个资产跑全库 grep 在这个工程（5.4 万文件）会超时，应把所有 guid 写进一个文件、用 `grep -rFf` 单次扫描。
