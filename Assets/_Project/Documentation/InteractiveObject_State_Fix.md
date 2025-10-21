# 交互物体状态保存和恢复修复

## 问题描述

1. **存档系统问题**：存档并没有保存对应交互物体的deactivated和disabled状态
2. **对话完成问题**：对话完成后并没有执行对应的deactivated和disabled状态更新

## 修复内容

### 1. 扩展存档数据结构 (`GameSaveData.cs`)

添加了新的字段来保存交互物体状态：
```csharp
[Header("交互物体状态")]
public Dictionary<string, bool> interactiveObjectStates = new Dictionary<string, bool>(); // 交互物体的disabled状态
public Dictionary<string, bool> interactiveObjectActiveStates = new Dictionary<string, bool>(); // 交互物体的active状态
```

添加了相关方法：
- `SetInteractiveObjectState(string interactiveID, bool isDisabled, bool isDeactivated)` - 设置交互物体状态
- `IsInteractiveObjectDisabled(string interactiveID)` - 检查是否禁用
- `IsInteractiveObjectDeactivated(string interactiveID)` - 检查是否隐藏

### 2. 扩展存档管理器 (`SaveManager.cs`)

添加了交互物体状态管理方法：
- `SetInteractiveObjectState()` - 保存交互物体状态
- `IsInteractiveObjectDisabled()` - 检查禁用状态
- `IsInteractiveObjectDeactivated()` - 检查隐藏状态

更新了加载逻辑，确保新的字典字段正确初始化。

### 3. 改进交互基类 (`IInteractive.cs`)

**改进状态检查逻辑**：
- 优先检查直接保存的交互物体状态
- 如果没有直接保存的状态，则检查对话节点完成状态
- 自动保存状态到存档系统

**添加对话完成处理**：
- 新增 `OnDialogueCompleted(string completedNodeName)` 方法
- 当对话完成时自动更新状态并保存到存档

### 4. 修复对话管理器 (`YarnSpinnerManager.cs`)

**添加交互物体通知机制**：
- 在 `OnDialogueComplete()` 中调用 `NotifyInteractiveObjects()`
- 新增 `NotifyInteractiveObjects()` 方法，通知所有交互物体对话已完成
- 确保对话完成后立即更新相关交互物体状态

## 工作流程

### 对话完成后的状态更新流程：

1. 玩家与交互物体交互，触发对话
2. 对话完成后，`YarnSpinnerManager.OnDialogueComplete()` 被调用
3. 保存对话节点完成状态到存档
4. 调用 `NotifyInteractiveObjects()` 通知所有交互物体
5. 每个交互物体检查是否关联该对话节点
6. 如果关联，则根据设置更新 `disabled` 和 `deactivated` 状态
7. 将更新后的状态保存到存档

### 场景加载时的状态恢复流程：

1. 场景加载时，每个交互物体调用 `CheckSaveState()`
2. 优先检查是否有直接保存的交互物体状态
3. 如果有，直接恢复状态
4. 如果没有，检查关联的对话节点是否已完成
5. 如果对话已完成，根据设置更新状态并保存到存档

## 使用说明

### 对于交互物体：

1. 设置 `interactiveID`（唯一标识符）
2. 设置 `linkedDialogueNode`（关联的对话节点）
3. 设置 `disableAfterDialogue`（对话后是否禁用交互）
4. 设置 `deactivateAfterDialogue`（对话后是否隐藏物体）

### 存档兼容性：

- 新版本存档包含交互物体状态信息
- 旧版本存档会自动初始化新的字典字段
- 向后兼容，不会影响现有存档

## 测试建议

1. 创建交互物体并设置相关参数
2. 与物体交互触发对话
3. 完成对话后检查物体状态是否正确更新
4. 重新加载场景检查状态是否正确恢复
5. 检查存档文件是否包含交互物体状态信息
