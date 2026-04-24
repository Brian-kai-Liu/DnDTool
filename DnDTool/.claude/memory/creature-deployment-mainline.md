# 生物部署主线设计（第一版）

目标：在不打断当前事件系统收口的前提下，先建立“生物模板 -> 战场实例 -> 地图部署保存”的稳定主线，为后续拖拽部署、事件激活生物、遭遇运行做准备。

## 一、当前边界结论

1. 生物模板与战场实例必须分离。
2. 战场实例不写入 `GridCells`，避免把地形、事件、单位三种概念混在同一个格子标记层。
3. 战场实例位置按格子坐标保存，不按 UI 像素坐标保存。
4. 战场实例需要保存：
   - 实例 ID
   - 来源生物模板 ID
   - 锚点格子坐标
5. 战场实例允许 DM 在部署后继续编辑，并保存修改结果。
6. 战场实例需要支持“未激活、不显示、条件满足后出现”。
7. 事件系统后续只应通过 `CreatureInstanceId + 激活/位置策略` 与生物系统对接。

## 二、建议的数据职责划分

### 1. 生物模板层

用途：作为章节内可复用的“原始怪物卡/角色卡”定义。

短期建议：

- 继续复用当前 `ChapterCreatureData / ChapterCreatureDataSaveData`
- 但在语义上正式定义为“生物模板”
- 为模板补上稳定 ID

建议字段：

```csharp
internal sealed class ChapterCreatureTemplateData
{
    public string CreatureId { get; set; } = string.Empty;
    public ChapterCreatureData Sheet { get; set; } = new ChapterCreatureData();
}
```

对应保存 DTO：

```csharp
[Serializable]
internal sealed class ChapterCreatureTemplateSaveData
{
    public string CreatureId = string.Empty;
    public ChapterCreatureDataSaveData Sheet = new ChapterCreatureDataSaveData();
}
```

落地时为了减少改动，第一版不必立刻大范围重命名现有类型。更稳妥的方式是：

1. 先保留当前 `ChapterListItemData.Creatures`
2. 将其语义冻结为“模板列表”
3. 后续再视情况重命名为 `CreatureTemplates`

### 2. 战场实例层

用途：表示某个模板被部署到当前章节地图后形成的独立实例。

第一版建议不要只存“覆盖字段差异”，而是直接在实例中保留一份可编辑快照。原因是：

1. DM 修改实例会非常频繁
2. 全量快照更直观，编辑器实现更简单
3. 不需要在第一版就处理复杂的“模板变更后如何回灌实例”问题

建议结构：

```csharp
internal sealed class ChapterCreatureInstanceData
{
    public string InstanceId { get; set; } = string.Empty;
    public string SourceCreatureId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ChapterCreatureInstancePlacementData Placement { get; set; } = new ChapterCreatureInstancePlacementData();
    public ChapterCreatureData RuntimeSheet { get; set; } = new ChapterCreatureData();
    public string DmNote { get; set; } = string.Empty;
}

internal sealed class ChapterCreatureInstancePlacementData
{
    public ChapterGridCoordinate AnchorCell { get; set; } = ChapterGridCoordinate.Zero;
    public float PreviewScale { get; set; } = 1f;
    public bool SnapToGrid { get; set; } = true;
}
```

对应保存 DTO：

```csharp
[Serializable]
internal sealed class ChapterCreatureInstanceSaveData
{
    public string InstanceId = string.Empty;
    public string SourceCreatureId = string.Empty;
    public bool IsActive = true;
    public ChapterCreatureInstancePlacementSaveData Placement = new ChapterCreatureInstancePlacementSaveData();
    public ChapterCreatureDataSaveData RuntimeSheet = new ChapterCreatureDataSaveData();
    public string DmNote = string.Empty;
}

[Serializable]
internal sealed class ChapterCreatureInstancePlacementSaveData
{
    public int AnchorCellX;
    public int AnchorCellY;
    public float PreviewScale = 1f;
    public bool SnapToGrid = true;
}
```

### 3. 章节聚合层

章节层建议同时持有两套集合：

1. 模板集合：章节可使用的生物卡定义
2. 实例集合：当前地图上已部署或待激活的战场实例

建议目标形态：

```csharp
internal sealed class ChapterListItemData
{
    public List<ChapterCreatureData> Creatures { get; set; } = new List<ChapterCreatureData>(); // 先继续作为模板列表
    public List<ChapterCreatureInstanceData> CreatureInstances { get; set; } = new List<ChapterCreatureInstanceData>();
}
```

保存 DTO 同理：

```csharp
[Serializable]
internal sealed class ChapterItemSaveData
{
    public List<ChapterCreatureDataSaveData> Creatures = new List<ChapterCreatureDataSaveData>();
    public List<ChapterCreatureInstanceSaveData> CreatureInstances = new List<ChapterCreatureInstanceSaveData>();
}
```

## 三、第一版关键设计选择

### 1. 模板 ID 与实例 ID 必须同时存在

- 模板 ID：用于标记“这个实例来自哪个原始模板”
- 实例 ID：用于地图编辑、事件激活、后续战斗追踪

### 2. 实例保存格子锚点，不保存屏幕坐标

- 运行时显示位置根据网格系统换算
- 这样地图缩放、平移、分辨率变化都不会破坏部署结果

### 3. 实例不占用 `GridCells` 标记

- 地形和事件仍属于格子层
- 生物实例属于地图对象层
- 后续如果要做占位、高亮、碰撞，也由实例系统自己计算，不反写格子标记

### 4. `IsActive` 作为第一版唯一可控的“出现状态”

- 常驻 NPC：创建实例时 `IsActive = true`
- 条件登场单位：实例预先存在但 `IsActive = false`
- DM 手动控制与事件控制都先统一到这个字段

### 5. 第一版实例修改采用“运行时快照”方案

- 实例生成时，从模板复制出 `RuntimeSheet`
- DM 后续修改只改 `RuntimeSheet`
- 不回写模板
- 这样能最直接满足“部署后可独立调整并保存”

## 四、与事件系统的对接约束

当前事件系统已经有：

- `Effect.Creature.InstanceId`
- `Effect.Creature.Activate`
- `Effect.Creature.PlacementMode`

因此生物系统第一版应当先保证：

1. 每个实例都有稳定 `InstanceId`
2. 事件能通过 `InstanceId` 找到实例
3. 事件至少能切换 `IsActive`

第一版暂时只建议正式支持一种落地策略：

- `PlacementMode = UseSavedAnchor`

其他策略先保留枚举位，不急着实现：

- 使用当前绑定格子
- 事件触发时手动指定位置

## 五、推荐开发顺序

### 阶段 A：数据结构定型

目标：先把“模板集合 / 实例集合 / 存档 DTO / 运行时映射”定下来。

工作项：

1. 为现有生物模板补 `CreatureId`
2. 新增 `ChapterCreatureInstanceData / SaveData`
3. 在章节数据中新增 `CreatureInstances`
4. 在持久化层补齐实例的存档读写和运行时转换
5. 保证旧存档没有 `CreatureInstances` 时仍能正常读取

这是新的第一优先级主线入口。

### 阶段 B：部署保存 MVP

目标：实现“从模板拖到地图 -> 生成实例 -> 自动吸附格子 -> 保存恢复”。

工作项：

1. 生物卡支持拖拽开始
2. 拖到地图时计算落点格子
3. 松手后创建实例
4. 实例 UI token 读取 `Placement.AnchorCell`
5. 重新打开章节后实例位置能恢复

这一阶段先不做复杂占格，也不做多体型。

### 阶段 C：地图上实例基础操作

目标：让实例成为真正可编辑对象。

工作项：

1. 选中实例
2. 拖动实例并重新吸附
3. 删除实例
4. 切换 `IsActive`
5. 隐藏实例时不渲染 token

### 阶段 D：实例属性编辑

目标：满足 DM 对部署后单位独立调整的需求。

工作项：

1. 打开实例编辑入口
2. 编辑 `RuntimeSheet`
3. 可查看 `SourceCreatureId`
4. 明确“编辑实例不会改模板”
5. 保存后重新进入章节仍能恢复实例改动

### 阶段 E：事件联动最小闭环

目标：先打通“事件激活实例”。

工作项：

1. 事件通过 `InstanceId` 查找实例
2. 事件可切换 `IsActive`
3. 地图层监听刷新实例显示
4. 先不做批量部署和复杂位置策略

## 六、建议暂缓的内容

这些内容建议不要放进第一版主线，否则会拖慢整个闭环：

1. 多格体型占位
2. 严格碰撞检测
3. 先攻、回合制、状态列表
4. 模板变更后批量同步实例
5. 复杂事件部署策略
6. 地图上的实例编队/分组

## 七、第一版验收标准

做到以下几点，就可以认为“生物部署主线第一阶段”已经成立：

1. 章节内模板和实例是两套独立集合
2. 模板有稳定 `CreatureId`
3. 实例有稳定 `InstanceId`
4. 生物卡可拖拽到地图生成实例
5. 实例位置按格子坐标保存并自动吸附
6. 实例不写入 `GridCells`
7. 实例可被 DM 手动激活/隐藏
8. 实例可保存部署后的独立属性修改
9. 事件可通过 `InstanceId` 激活实例

## 八、下一步直接执行建议

下一轮正式动手时，建议按下面顺序推进：

1. 先补模板 ID 与实例数据结构
2. 再补持久化映射
3. 然后做地图 token 渲染层
4. 再接拖拽部署
5. 最后补实例编辑与事件激活

这样可以保证每一步都有可验证结果，而且不会和事件系统正在收尾的部分互相打架。

## 九、当前进度（2026-04-24）

- 已完成：为现有章节内生物模板补入稳定 `CreatureId` 字段，并在运行时编辑、章节内存拷贝、持久化读写中统一补齐缺失 ID。
- 已完成：新增战场实例数据结构：
  - `ChapterCreatureInstanceData`
  - `ChapterCreatureInstancePlacementData`
  - `ChapterCreatureInstanceSaveData`
  - `ChapterCreatureInstancePlacementSaveData`
- 已完成：章节数据与保存 DTO 新增 `CreatureInstances` 集合，形成“模板集合 + 实例集合”并行结构。
- 已完成：持久化层接入模板与实例的运行时/存档映射，旧存档缺少这些字段时仍可正常读取。
- 已完成：补充 `ChapterCreatureDataStructureUtility`，统一处理模板/实例的归一化、ID 生成和深拷贝。
- 已完成：地图 token 渲染层第一版接入，已激活实例会根据 `Placement.AnchorCell` 渲染到网格地图上，并随当前网格缩放/平移刷新。
- 已完成：生物卡片到地图的拖拽部署 MVP：
  - 章节内结构化生物模板卡可直接拖到地图
  - 松手后自动吸附到目标格子
  - 自动生成 `CreatureInstance`
  - 保存后可通过现有持久化链路恢复
- 已完成：地图上实例基础操作第一版：
  - 点击 token 可选中实例
  - 选中实例后可直接拖动到其他格子重新部署
  - 选中实例后可通过地图上的操作面板移除
  - 选中实例后可通过地图上的操作面板手动切换 `IsActive`
- 当前实现说明：
  - 为了便于 DM 编辑，`IsActive = false` 的实例在编辑器地图中会以弱化 token 形式显示，而不是彻底不可见
  - 这样可以继续选中、重新激活、拖动和删除实例
- 已验证：`dotnet build GameLogic.csproj -nologo` 通过，当前为 `0 error`。
- 下一步：进入实例属性编辑和事件联动阶段，补“实例编辑入口 / 运行时快照修改 / 事件按 `InstanceId` 激活实例”。
