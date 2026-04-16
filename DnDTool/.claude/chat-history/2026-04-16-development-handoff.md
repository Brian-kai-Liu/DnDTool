# 开发交接摘要

## 1. 项目与工作流约束

- 当前项目是 Unity + TEngine + HybridCLR + YooAsset + UniTask 的 DnD 工具软件。
- 开发回答与提案默认使用中文。
- 涉及 TEngine 的 L2-L4 任务，默认先按技能和 Wiki 规范执行。
- 核心红线：
  - IO 与资源加载优先 `UniTask`
  - 模块通过 `GameModule.XXX` 访问
  - 资源加载后需要成对释放
  - 模块间用 `GameEvent`，UI 内部用 `AddUIEvent`
  - 注意热更边界：`Main` 不热更，`HotFix` 热更

## 2. 用户明确偏好

- UI 改动默认 prefab 优先，避免代码动态创建或强行改布局。
- 代码侧优先只保留节点绑定、交互逻辑和必要的最小布局兜底。
- UI 文本输入与显示优先使用 `TextMeshProUGUI` / `TMP_InputField`。
- 不主动做 Unity 验证。
- 当 prefab 与代码行为冲突时，以 prefab 中的内容为主，不在代码里额外做 UI 调整。

## 3. 当前主要产品上下文

- 当前讨论与开发重点集中在 DM 侧工具，尤其是模组编辑与章节编辑。
- 关于模组设计，已形成三段式结构：
  - 模组基本信息
  - 冒险前置
  - 剧情章节编辑
- 目前已经深入讨论过：
  - 模组基本信息页的必要功能
  - 模组封面预览字段
  - 章节编辑页中的“添加事件”方向

## 4. 模组封面预览已确定字段

- 模组名称
- 规则版本
- 推荐人数
- 推荐等级
- 预计时长
- 模组标签
- 模组状态（仅 DM 侧显示）
- 用户补充：玩家侧还需要“是否已跑完”的标识

## 5. 当前代码开发主线

- 会话后期主要围绕 `ChapterEditorUI` 和 `ChapterEventPopupUI` 展开。
- `ChapterEditorUI` 已经扩展过章节目标与 DM 备注输入。
- 原“添加检定”方向已经转成“添加事件”方向。
- 当前按钮入口已切到 `ChapterEventPopupUI`。
- `ChapterEventPopupUI` 目前仍是页面骨架，不是完整表单。

## 6. 事件弹窗当前状态

- 已存在 prefab：
  - `Assets/AssetRaw/UI/ChapterEventPopupUI.prefab`
- 已存在代码入口：
  - `Assets/GameScripts/HotFix/GameLogic/UI/ChapterEditorUI/ChapterEditorUI.cs`
- 已存在按钮绑定修改：
  - `Assets/GameScripts/HotFix/GameLogic/UI/Gen/ChapterEditorUI_Gen.g.cs`

当前弹窗逻辑状态：

- `ChapterEditorUI` 中按钮会打开 `ChapterEventPopupUI`
- `ChapterEventPopupUI` 类目前是内联在 `ChapterEditorUI.cs` 中
- 当前仍以骨架页为主，尚未接入完整字段录入与保存

## 7. 事件弹窗需要继续遵守的实现方向

- 不要继续走“代码动态拼页面结构”的方向。
- 后续要改成：
  - prefab 固定结构为主
  - 代码只负责绑定、状态切换、显隐、数据读写
- 之前已经专门纠偏过这一点，不能再回到“代码文本预览代替表单”的做法。

## 8. 当前最自然的下一步开发项

如果继续 `ChapterEventPopupUI`，建议按以下顺序推进：

1. 在 prefab 中补齐事件公共字段输入控件
2. 再补检定事件专属字段区域
3. 再同步 `UIBindComponent` / Gen / 业务代码绑定
4. 最后再设计事件数据模型与保存逻辑

建议优先补的公共字段：

- 事件标题
- 触发说明
- 成功结果
- 失败结果
- DM备注

检定事件后续优先字段：

- 属性/技能选择
- 具体检定项
- 判定方式
- DC 难度

## 9. 关键文件

- `Assets/GameScripts/HotFix/GameLogic/UI/ChapterEditorUI/ChapterEditorUI.cs`
- `Assets/GameScripts/HotFix/GameLogic/UI/Gen/ChapterEditorUI_Gen.g.cs`
- `Assets/AssetRaw/UI/ChapterEditorUI.prefab`
- `Assets/AssetRaw/UI/ChapterEventPopupUI.prefab`
- `Assets/GameScripts/HotFix/GameLogic/UI/ChapterEditorUI/ChapterEditorModels.cs`
- `Assets/GameScripts/HotFix/GameLogic/UI/ChapterEditorUI/ChapterEditorPersistenceService.cs`

## 10. Git 当前处理结果

- 旧 `.git` 已删除并重建。
- 当前本地是一个新的 Git 仓库。
- 分支为 `main`。
- 没有保留旧远程仓库配置。
- 尚未做新的初始提交，用户准备后续自己提交完整项目。

## 11. 聊天记录档案

- 完整恢复版聊天记录文件：
  - `.claude/chat-history/2026-04-16-session-history.md`
- 当前这份文件是面向继续开发的短版交接摘要。

## 12. 建议新窗口开场指令

在新窗口中，可以先让助手读取本文件，再继续开发。建议第一条消息直接写：

`先阅读 .claude/chat-history/2026-04-16-development-handoff.md，然后继续当前项目开发。`

如果要继续事件弹窗，可继续补一句：

`继续 ChapterEventPopupUI，按 prefab 主导方式补齐真实表单。`