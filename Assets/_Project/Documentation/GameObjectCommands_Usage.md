# GameObject & Interactive Commands 使用说明

## 概述
GameObjectCommands 和 InteractiveCommands 提供了一系列Yarn Commands来在对话过程中控制场景中GameObject的激活状态和交互功能。

## GameObject 控制命令

### 1. activate_object
激活指定名称的GameObject
```yarn
<<activate_object "ObjectName">>
```

### 2. deactivate_object  
停用指定名称的GameObject
```yarn
<<deactivate_object "ObjectName">>
```

### 3. toggle_object
切换指定名称的GameObject的激活状态
```yarn
<<toggle_object "ObjectName">>
```

### 4. activate_objects
激活多个GameObject（用逗号分隔名称）
```yarn
<<activate_objects "Object1, Object2, Object3">>
```

### 5. deactivate_objects
停用多个GameObject（用逗号分隔名称）
```yarn
<<deactivate_objects "Object1, Object2, Object3">>
```

### 6. check_object_active
检查指定名称的GameObject是否激活，并将结果存储到Yarn变量中
```yarn
<<check_object_active "ObjectName" "variableName">>
<<if $variableName>>
// GameObject是激活状态
<<else>>
// GameObject是停用状态
<<endif>>
```

## Interactive 控制命令

### 7. enable_interaction
启用指定名称的交互对象的交互功能
```yarn
<<enable_interaction "InteractiveObjectName">>
```

### 8. disable_interaction
禁用指定名称的交互对象的交互功能
```yarn
<<disable_interaction "InteractiveObjectName">>
```

### 9. toggle_interaction
切换指定名称的交互对象的交互状态
```yarn
<<toggle_interaction "InteractiveObjectName">>
```

### 10. enable_interactions
启用多个交互对象的交互功能（用逗号分隔名称）
```yarn
<<enable_interactions "Interactive1, Interactive2, Interactive3">>
```

### 11. disable_interactions
禁用多个交互对象的交互功能（用逗号分隔名称）
```yarn
<<disable_interactions "Interactive1, Interactive2, Interactive3">>
```

### 12. check_interaction_enabled
检查指定名称的交互对象是否可以交互，并将结果存储到Yarn变量中
```yarn
<<check_interaction_enabled "InteractiveObjectName" "variableName">>
<<if $variableName>>
// 交互对象可以交互
<<else>>
// 交互对象不能交互
<<endif>>
```

### 13. set_hint_text
设置交互对象的提示文本
```yarn
<<set_hint_text "InteractiveObjectName" "新的提示文本">>
```

### 14. show_hint
显示指定交互对象的提示
```yarn
<<show_hint "InteractiveObjectName">>
```

### 15. hide_hint
隐藏指定交互对象的提示
```yarn
<<hide_hint "InteractiveObjectName">>
```

## 使用示例

### 基本用法
```yarn
title: Example
---
角色: 让我们控制一些对象

// 激活一个门
<<activate_object "Door">>
角色: 门已经打开了

// 停用一个敌人
<<deactivate_object "Enemy">>
角色: 敌人消失了

// 切换一个开关的状态
<<toggle_object "Switch">>
角色: 开关状态已改变

// 启用交互功能
<<enable_interaction "TreasureChest">>
角色: 宝箱现在可以打开了

// 禁用交互功能
<<disable_interaction "LockedDoor">>
角色: 门被锁住了
```

### 条件控制
```yarn
// 检查对象状态并做出反应
<<check_object_active "SecretDoor" "doorActive">>
<<if $doorActive>>
角色: 秘密门是打开的！
<<else>>
角色: 秘密门是关闭的
<<endif>>

// 检查交互状态
<<check_interaction_enabled "MagicCrystal" "canInteract">>
<<if $canInteract>>
角色: 水晶可以交互了
<<else>>
角色: 水晶暂时无法交互
<<endif>>
```

### 批量操作
```yarn
// 激活多个UI元素
<<activate_objects "MenuPanel, StartButton, OptionsButton">>
角色: 所有菜单元素都已显示

// 停用多个敌人
<<deactivate_objects "Enemy1, Enemy2, Enemy3">>
角色: 所有敌人都被击败了

// 启用多个交互对象
<<enable_interactions "Chest1, Chest2, Chest3">>
角色: 所有宝箱都可以打开了

// 禁用多个陷阱
<<disable_interactions "Trap1, Trap2, Trap3">>
角色: 所有陷阱都已失效
```

### 提示系统控制
```yarn
// 设置新的提示文本
<<set_hint_text "MysteriousBox" "这是一个神秘的盒子">>
角色: 提示文本已更新

// 显示提示
<<show_hint "ImportantItem">>
角色: 重要物品的提示已显示

// 隐藏提示
<<hide_hint "SecretPassage">>
角色: 秘密通道的提示已隐藏
```

## 注意事项

1. **对象名称**: 确保GameObject的名称与命令中使用的名称完全匹配（区分大小写）
2. **查找范围**: 命令会在当前场景中查找GameObject，如果未找到会尝试在所有场景中查找
3. **错误处理**: 如果找不到指定的GameObject，会在控制台输出警告信息
4. **性能**: 批量操作命令会逐个处理每个对象，如果某个对象不存在会跳过并继续处理其他对象

## 调试信息
所有命令都会在Unity控制台输出相应的日志信息，包括：
- 成功操作的对象名称
- 未找到的对象名称警告
- 批量操作的成功计数

## 集成说明
这些命令会自动注册到YarnSpinner系统中，无需额外配置。只需要确保GameObjectCommands脚本存在于场景中即可使用。
