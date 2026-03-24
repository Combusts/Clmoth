# BaseCharacter 表情控制 Yarn Spinner 命令

## 概述

为 `BaseCharacter` 类添加了完整的 Yarn Spinner 表情控制命令，允许在对话脚本中直接控制角色的表情状态。

## 可用命令

### 1. 设置表情命令

#### `set_emotion`
设置角色的表情状态。

**语法：**
```yarn
<<set_emotion emotionIndex>>
```

**参数：**
- `emotionIndex` (int): 表情索引
  - `0`: 隐藏表情
  - `1-N`: 显示对应的表情（N为表情总数）

**示例：**
```yarn
<<set_emotion 1>>  // 显示第1个表情
<<set_emotion 0>>  // 隐藏表情
<<set_emotion 3>>  // 显示第3个表情
```

### 2. 随机表情命令

#### `set_random_emotion`
随机设置一个表情。

**语法：**
```yarn
<<set_random_emotion>>
```

**示例：**
```yarn
<<set_random_emotion>>  // 随机显示一个表情
```

### 3. 清除表情命令

#### `clear_emotion`
清除当前表情（等同于 `set_emotion 0`）。

**语法：**
```yarn
<<clear_emotion>>
```

**示例：**
```yarn
<<clear_emotion>>  // 隐藏表情
```

## 可用函数

### 1. 获取当前表情

#### `get_emotion()`
获取当前的表情索引。

**返回值：**
- `int`: 当前表情索引（0表示无表情）

**示例：**
```yarn
<<if get_emotion() == 1>>
    当前显示的是第1个表情！
<<elseif get_emotion() == 0>>
    当前没有表情显示。
<<else>>
    当前表情索引是：{get_emotion()}
<<endif>>
```

### 2. 检查表情是否存在

#### `has_emotion(emotionIndex)`
检查指定索引的表情是否存在。

**参数：**
- `emotionIndex` (int): 要检查的表情索引

**返回值：**
- `bool`: 表情是否存在

**示例：**
```yarn
<<if has_emotion(1) == true>>
    角色有第1个表情！
<<else>>
    角色没有第1个表情。
<<endif>>
```

### 3. 获取表情总数

#### `get_emotion_count()`
获取角色的表情总数。

**返回值：**
- `int`: 表情总数

**示例：**
```yarn
角色总共有 {get_emotion_count()} 个表情。

<<if get_emotion_count() > 0>>
    有表情可以使用！
<<else>>
    没有配置任何表情。
<<endif>>
```

## 完整示例

```yarn
title: EmotionDemo

# 表情控制演示

欢迎！让我们测试表情系统：

<<set_emotion 1>>
这是第一个表情！

<<set_emotion 2>>
这是第二个表情！

<<set_random_emotion>>
随机表情！

<<clear_emotion>>
表情已清除。

# 检查表情状态
<<if get_emotion() == 0>>
    当前没有表情显示。
<<else>>
    当前表情索引：{get_emotion()}
<<endif>>

# 检查表情可用性
<<if has_emotion(1) == true>>
    第1个表情可用！
<<endif>>

<<if has_emotion(2) == true>>
    第2个表情可用！
<<endif>>

总共有 {get_emotion_count()} 个表情。

===
```

## 错误处理

命令包含完整的错误处理：

- **负数索引**：会输出警告并忽略操作
- **超出范围索引**：会输出警告并忽略操作
- **空表情列表**：随机表情命令会输出警告
- **组件缺失**：会在控制台输出详细的调试信息

## 使用方法

1. 确保场景中的角色对象有 `BaseCharacter` 组件
2. 在 Inspector 中配置 `emotionSprites` 列表
3. 在 Yarn 脚本中使用上述命令
4. 运行游戏测试表情控制

## 注意事项

- **命令与函数区别**：`YarnCommand` 是实例方法，`YarnFunction` 是静态方法
- **多角色支持**：如果有多个 BaseCharacter，函数会使用第一个找到的实例
- **表情索引**：表情索引从 1 开始（1, 2, 3...），0 表示隐藏表情
- **表情配置**：确保 `emotionSprites` 列表中已正确配置表情图片
- **动画效果**：表情动画会自动播放（`EmotionPop` 和 `EmotionQuit`）
- **调试信息**：所有命令都会在控制台输出详细的调试信息
- **错误处理**：包含完整的错误处理，找不到角色时会输出警告
