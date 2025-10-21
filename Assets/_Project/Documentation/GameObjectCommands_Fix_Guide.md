# GameObjectCommands 修复指南

## 问题描述

`activate_object` 命令无法激活对象的问题已经修复。问题的根源是 `GameObjectCommands` 脚本没有在场景中实例化，导致 YarnSpinner 无法发现和执行这些命令。

## 修复内容

### 1. 修改方法签名
- 将所有 `[YarnCommand]` 方法从 `static` 改为实例方法
- 将 `FindGameObjectByName` 方法从 `static` 改为实例方法
- 增加了详细的调试日志输出

### 2. 增强调试功能
- 添加了详细的查找过程日志
- 当找不到对象时，会列出场景中所有GameObject的名称和状态
- 提供了更好的错误诊断信息

## 设置步骤

### 步骤1：在场景中添加CommandManager

1. 打开 `Level_01_01` 场景
2. 创建一个空的GameObject，命名为 "CommandManager"
3. 将 `GameObjectCommands` 组件添加到这个GameObject上
4. 确保CommandManager在场景中保持激活状态

### 步骤2：验证设置

1. 运行游戏
2. 触发包含 `<<activate_object "2 with doc">>` 的对话
3. 查看Console窗口的调试日志

## 调试信息

修复后的命令会输出以下调试信息：

```
[GameObjectCommands] 正在查找GameObject: '2 with doc'
[GameObjectCommands] 通过GameObject.Find找到: '2 with doc'
[GameObjectCommands] 已激活GameObject: 2 with doc
```

如果找不到对象，会显示：
```
[GameObjectCommands] 未找到名为 '2 with doc' 的GameObject
[GameObjectCommands] 场景中所有GameObject名称:
  - 'Main Camera' (Active: True)
  - '2 without doc' (Active: False)
  - '2 with doc' (Active: True)
  - ...
```

## 可用的命令

修复后，以下命令都可以正常使用：

- `<<activate_object "ObjectName">>` - 激活指定对象
- `<<deactivate_object "ObjectName">>` - 停用指定对象
- `<<toggle_object "ObjectName">>` - 切换对象状态
- `<<activate_objects "Obj1, Obj2, Obj3">>` - 激活多个对象
- `<<deactivate_objects "Obj1, Obj2, Obj3">>` - 停用多个对象
- `<<check_object_active "ObjectName" "variableName">>` - 检查对象状态

## 注意事项

1. **CommandManager必须存在**：确保场景中有CommandManager GameObject且包含GameObjectCommands组件
2. **对象名称必须精确匹配**：GameObject的名称必须与命令中使用的名称完全一致
3. **调试日志**：如果遇到问题，查看Console窗口的调试日志来诊断问题
4. **场景切换**：如果使用DontDestroyOnLoad，CommandManager会在场景切换时保持存在

## 测试建议

1. 在Level_01场景中测试 `<<activate_object "2 with doc">>` 命令
2. 验证对象确实被激活
3. 测试 `<<toggle_object "2 without doc">>` 命令
4. 检查调试日志确认命令正常执行

## 故障排除

如果命令仍然不工作：

1. 确认CommandManager GameObject存在且激活
2. 确认GameObjectCommands组件已添加
3. 检查Console窗口的调试日志
4. 验证目标GameObject的名称是否正确
5. 确认YarnSpinner的DialogueRunner正常工作
