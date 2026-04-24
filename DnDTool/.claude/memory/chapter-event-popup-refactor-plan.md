# ChapterEventPopupUI 改造方案

目标：在尽量复用当前 `ChapterEventPopupUI` 弹窗、现有事件数据字段与当前编辑流程的前提下，把“事件分类 / 触发条件 / 执行效果”梳理清楚，并为后续“激活生物实例”“开始战斗”“前置事件触发”铺路。

## 一、当前现状

当前弹窗的主要问题：

1. 顶层分类混杂了“处理方式”和“内容类型”。
2. `Check` 与 `DmDirect` 这两个顶层分类语义过粗。
3. `剧情 / 对话 / 选择 / 战斗 / 探索 / 区域进入 / 时间推进 / 随机 / 特殊` 被放在 `DmDirect` 子类里，和“触发方式”维度混在一起。
4. `Automatic / DmManual` 触发方式不足以表达后续需要的事件链、前置条件、实例激活。
5. 当前 UI 显示逻辑以“检定类事件”为中心，不利于扩展到“激活生物实例”等效果。

当前已存在且可复用的节点：

- 标题：`m_tmpTitle`
- 关闭按钮：`m_btnClose`
- 事件分类下拉：`m_tmpDropdownEventCategory`
- 触发方式下拉：`m_tmpDropdownTriggerMode`
- 能力检定区：`m_rectAbilityCheckSection`
- 技能检定区：`m_rectSkillCheckSection`
- DM 扩展区：`m_rectDmDirectSection`
- 标题输入：`m_tmpInputEventTitle`
- 触发描述：`m_tmpInputTriggerDescription`
- 成功结果：`m_tmpInputSuccessResult`
- 失败结果：`m_tmpInputFailureResult`
- DM备注：`m_tmpInputDmNote`
- DM提示：`m_tmpInputDmPrompt`
- 确认按钮：`m_btnConfirm`

## 二、改造目标

弹窗改造后，UI 逻辑应当明确分为三层：

1. 基础信息
2. 触发条件
3. 执行效果

其中：

- “什么时候触发”归触发条件区
- “触发后做什么”归执行效果区
- 不再把“区域进入”“战斗”“对话”混着当同一层级的分类

## 三、目标交互结构

### 1. 基础信息区

始终显示：

- 事件标题
- 事件摘要/说明
- 绑定范围提示
- 是否启用
- 是否一次性
- DM备注

说明：

- `事件标题` 继续复用 `m_tmpInputEventTitle`
- `事件摘要/说明` 可以优先复用 `m_tmpInputTriggerDescription`，后续再视情况拆分
- `DM备注` 继续复用 `m_tmpInputDmNote`
- “绑定范围提示”第一版可只做只读文本，不一定先落库

### 2. 触发条件区

新增一层明确概念：`触发类型`

第一版建议支持：

1. `DM手动触发`
2. `进入绑定区域触发`
3. `与场景对象交互触发`
4. `前置事件完成后触发`

建议显示逻辑：

- 统一通过一个新的“触发类型下拉”控制
- 下方显示对应子面板

各触发类型对应字段：

#### DM手动触发

- 无额外必填项
- 只显示一段说明文本

#### 进入绑定区域触发

- 是否首次进入才触发
- 是否所有绑定格子共享同一事件
- 触发说明

#### 与场景对象交互触发

- 交互对象说明
- 是否需要玩家主动确认

#### 前置事件完成后触发

- 前置事件引用
- 延迟触发说明

### 3. 执行效果区

新增一层明确概念：`效果类型`

第一版建议支持：

1. `检定`
2. `剧情提示`
3. `对话/交互提示`
4. `激活生物实例`
5. `开始战斗`

建议显示逻辑：

- 统一通过一个新的“效果类型下拉”控制
- 下方显示对应效果配置面板

各效果类型对应字段：

#### 检定

复用当前已有能力/技能检定 UI：

- 检定目标模式
  - 属性
  - 技能
  - 工具
  - 对抗
  - 被动
  - 豁免
- 判定方式
  - 掷骰
  - DM直接判定
- 成功结果
- 失败结果
- 属性阈值输入区
- 技能阈值输入区

对应复用节点：

- `m_rectAbilityCheckSection`
- `m_rectSkillCheckSection`
- `m_tmpInputSuccessResult`
- `m_tmpInputFailureResult`

#### 剧情提示

- 展示文本
- 是否仅 DM 可见

可先复用：

- `m_rectDmDirectSection`
- `m_tmpInputDmPrompt`

#### 对话/交互提示

- 对话对象说明
- 对话摘要
- 后续提示

也可先复用：

- `m_rectDmDirectSection`
- `m_tmpInputDmPrompt`

#### 激活生物实例

这是后续最关键的效果类型。

需要显示：

- 目标实例选择
- 目标激活状态
- 激活后位置策略
  - 使用实例已保存位置
  - 使用当前绑定格子
  - 手动指定

第一版 UI 可先留空节点方案，不急着接完整业务。

#### 开始战斗

需要显示：

- 关联遭遇说明
- 是否自动包含当前已激活实例
- 战斗开始说明

第一版也可先仅保留占位结构。

## 四、建议的顶层 UI 重组

### 当前旧逻辑

- 事件分类：`Check / DmDirect`
- DM子类：剧情、对话、选择、互动、战斗、探索、区域进入、时间推进、随机、特殊

### 新逻辑

改为：

#### 顶部固定区

- 标题
- 基础信息

#### 中部第一层

- 触发类型

#### 中部第二层

- 效果类型

#### 底部区

- 确认
- 关闭

## 五、数据层对应调整建议

当前 `ChapterGridEventData` 仍是旧字段结构。为了渐进式改造，建议分两步：

### 第一步：UI先清晰化，数据层先兼容

保留现有字段继续落库：

- `EventCategory`
- `EventSubType`
- `TriggerMode`
- `CheckTargetMode`
- `CheckResolutionMode`
- `EventTitle`
- `TriggerDescription`
- `SuccessResult`
- `FailureResult`
- `DmNote`
- `DmPrompt`

但 UI 解释方式改成新语义：

- `EventCategory` 逐步退化为“效果主类”
- `EventSubType` 逐步退化为“效果子类”
- `TriggerMode` 先继续存在，但后续替换为更细的触发类型

### 第二步：正式拆分为 Trigger / Effect

未来新增字段建议：

- `TriggerType`
- `TriggerParam`
- `EffectType`
- `EffectParam`
- `IsEnabled`
- `IsOneShot`

这一阶段不在本轮立即落实，只作为下一步结构目标。

## 六、Prefab 修改方案

建议按“最小破坏”方式改：

1. 保留现有根结构和关闭/确认按钮。
2. 保留现有能力检定区和技能检定区。
3. 将 `m_rectDmDirectSection` 从“DM直判专区”改成“通用效果扩展区”。
4. 新增两个更明确的下拉框：
   - `m_tmpDropdownTriggerType`
   - `m_tmpDropdownEffectType`
5. 保留旧的 `m_tmpDropdownEventCategory` 一段时间作为过渡，最终再移除。
6. 为后续扩展预留两个面板：
   - `m_rectEffectNarrativeSection`
   - `m_rectEffectCombatantActivationSection`

## 七、代码修改分阶段计划

### 阶段 A：先做 UI 语义梳理

目标：

- 不大改数据结构
- 先把弹窗显示逻辑理顺

工作项：

1. 调整枚举命名与注释
2. 调整下拉文本
3. 新增“触发类型 / 效果类型”下拉
4. 重写 `RefreshView()`
5. 保留旧字段存储映射

### 阶段 B：接入效果扩展面板

目标：

- 在 UI 上先支持“剧情提示 / 激活生物实例 / 开始战斗”的显示框架

工作项：

1. prefab 新增扩展区
2. 绑定生成代码同步
3. `ScriptGenerator()` 接新节点
4. `OnRefresh()` / `BuildEventData()` / `ApplyExistingEventData()` 接新字段映射

### 阶段 C：数据结构正式拆 Trigger / Effect

目标：

- 让事件模型真正脱离旧的 `Check / DmDirect` 思维

工作项：

1. `ChapterGridEventData` 新增 Trigger / Effect 结构
2. Persistence 映射兼容旧数据
3. 旧字段逐步废弃

## 八、推荐实施顺序

1. 先改弹窗文案与显示逻辑
2. 再改 prefab 面板结构
3. 再接新的“效果区”
4. 最后才动事件数据模型字段

这样风险最低，且每一步都能单独验证。

## 九、验收标准

改造完成后应满足：

1. 用户能明确区分“触发条件”和“执行效果”。
2. 检定类事件仍可正常编辑和保存。
3. 批量绑定格子的事件流程不受破坏。
4. 事件弹窗为“激活生物实例”预留明确入口。
5. 旧存档仍可读取。

## 十、后续直接执行建议

下一步实际动手时，建议按以下顺序：

1. 先改 `ChapterEventPopupUI` 的枚举、下拉文案和 `RefreshView()`。
2. 再改 `ChapterEventPopupUI.prefab`，补齐触发类型与效果类型下拉。
3. 然后补“剧情提示区”和“激活生物实例区”的 prefab 节点。
4. 最后再接新的数据字段。
## 当前开发状态（2026-04-24）

### 已开发完成

- 基础信息区已完成重组：
  - 事件标题
  - 事件摘要/说明
  - 绑定范围提示
  - 是否启用
  - 是否一次性
  - DM 备注
- 触发条件区已完成重组：
  - 新的“触发类型”下拉
  - `DM 手动触发`
  - `进入绑定区域触发`
  - `与场景对象交互触发`
  - `前置事件完成后触发`
- 触发条件子面板已接入当前数据字段：
  - 区域触发：首次进入、共享事件
  - 交互触发：交互对象说明、是否需要确认
  - 前置事件触发：前置事件 ID、延迟说明
- 执行效果区已完成重组：
  - 新的“效果类型”下拉
  - `检定`
  - `剧情提示`
  - `对话/交互提示`
  - `激活生物实例`
  - `开始战斗`
- 检定区已完成接入：
  - 检定目标模式
  - 判定方式
  - 能力检定阈值
  - 技能检定阈值
  - 成功结果
  - 失败结果
- 效果扩展面板已完成基础字段接入：
  - 剧情提示：文本、仅 DM 可见
  - 对话/交互提示：目标、摘要、提示词
  - 激活生物实例：实例 ID、激活开关、位置策略
  - 开始战斗：战斗引用、是否包含已激活生物、战斗说明
- Prefab 结构已完成首轮重组：
  - 新增触发区与效果区相关面板
  - 新增触发类型与效果类型下拉
  - 保留并复用能力检定区、技能检定区、通用输入区
- `RefreshView()` 已按新语义重写，旧数据读取时已做兼容映射：
  - 旧 `EventCategory / EventSubType / TriggerMode / DmPrompt`
  - 新 `TriggerType / EffectType / 触发参数 / 效果参数`
- 事件弹窗排版自适应已接入：
  - 触发区切换时自适应重排
  - 效果区切换时自适应重排
  - 前置事件说明多行输入高度自适应
  - 面板整体高度自适应

### **尚未开发完成**

- `ChapterGridEventData / ChapterGridEventSaveData` 已新增独立的 `Trigger / Effect` 结构，并已接入弹窗构建、弹窗回填、事件集合克隆、事件集合写回与持久化链路；运行时编辑与集合操作已统一优先面向 `Trigger / Effect`
- `Trigger / Effect` 内部的 `TriggerParam / EffectParam` 形式嵌套参数模型已落地，并已接入弹窗编辑、归一化、克隆与持久化
- **旧字段的彻底退役尚未开始，目前仍保留兼容映射**
- **`ChapterGridEventData` 的最终目标形态尚未完成，目前仍处于“运行时以结构化字段为主、旧字段仅做兼容镜像”的过渡阶段**
- **持久化层虽然已经可以跟随新字段工作，但“新旧结构并行后的完整清理”尚未完成**
- **文档中提到的“阶段性过渡后再移除旧事件分类下拉”等收尾工作尚未完成**
- **事件引用目前仍以输入字段为主，尚未升级为更明确的事件选择器/引用器 UI**
- **整套改造的最终验收项虽然大部分能力已具备，但还没有单独做一次完整的回归验收记录**

## 未完成项优先级建议

### P1：可先独立推进

- 逐步调整 `ChapterGridEventData` 到最终目标形态
- 持久化层完成新旧结构并行后的清理与收口
- 旧字段彻底退役，移除当前兼容映射
- 移除旧事件分类下拉等过渡性 UI 收尾
- 补一轮完整的事件弹窗回归验收记录

### P2：更适合等待其他系统支持

- 事件引用从纯输入框升级为事件选择器 / 引用器 UI

说明：

- 这一项会依赖事件列表、事件索引、章节内事件查询、ID 管理，后续如果要支持“跳转到目标事件编辑”还会依赖编辑器联动能力。
## 当前进度（2026-04-24）

- 已完成：`Trigger / Effect` 主结构、`TriggerParam / EffectParam` 嵌套参数模型、弹窗/持久化/克隆链路接入。
- 已完成：运行时 `ChapterGridEventData` 的旧平铺字段大幅收口，只保留最小兼容映射思路。
- 已完成：`ChapterGridEventData / ChapterGridEventSaveData` 顶层已精简为 `EventId / IsEnabled / IsOneShot / Trigger / Effect / EventTitle / TriggerDescription / DmNote`。
- 已完成：旧存档读取链路改为 `LegacySaveData -> CurrentSaveData` 迁移，旧平铺字段只保留在最小兼容映射层，不再参与新的存档写入。
- 进行中：`Trigger / Effect` 内部保存结构的进一步收口，以及旧兼容字段的最终退役。
- 待推进：事件引用选择器、事件索引与后续联动能力。

## 今日修改记录（2026-04-24）

- 已将 `ChapterGridEventSaveData` 从“新旧字段并存”进一步收口为最小顶层模型，仅保留当前运行时和新存档真正需要的字段。
- 已新增独立的旧存档 DTO：
  - `ChapterEditorLegacySaveData`
  - `ChapterItemLegacySaveData`
  - `ChapterGridCellLegacySaveData`
  - `ChapterGridEventLegacySaveData`
- 已将旧存档兼容逻辑改为“读取旧 DTO -> 迁移到当前 DTO”模式：
  - 当前存档写入不再回写旧事件平铺字段
  - 旧 `EventType / EventCategory / EventSubType` 仅保留在读取兼容层用于迁移
- 已补充 `ChapterEditorPersistenceService` 中旧数据迁移时的显式拷贝逻辑，避免旧 DTO 的列表对象在迁移后被直接复用：
  - `SelectedGridCellKeys`
  - `Creatures`
  - `EventBindings`
- 已完成一次编译验证：
  - `dotnet build GameLogic.csproj -nologo`
  - 结果为 `0 error`

## 下一步建议

- 继续收口 `Trigger / Effect` 内部的保存结构，明确哪些字段仍然只是兼容镜像。
- 在此基础上再推进旧兼容字段的最终退役，避免过早删除影响旧存档读取。
