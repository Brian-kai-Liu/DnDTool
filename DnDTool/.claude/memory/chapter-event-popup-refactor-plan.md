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
