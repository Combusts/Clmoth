# Yarn Spinner 集成文档

## 概述

Yarn Spinner 是一个强大的对话系统工具，专为游戏开发设计。本文档详细说明如何在 Spotlight 2025 项目中集成和使用 Yarn Spinner。

## 安装 Yarn Spinner

### 1. 通过 Unity Package Manager 安装

1. 打开 Unity Editor
2. 选择 `Window` > `Package Manager`
3. 点击左上角的 `+` 按钮，选择 `Add package from git URL`
4. 输入以下 URL：
   ```
   https://github.com/YarnSpinnerTool/YarnSpinner.git?path=/YarnSpinner.Unity#release
   ```
5. 点击 `Add` 安装

### 2. 手动安装

1. 从 [Yarn Spinner GitHub](https://github.com/YarnSpinnerTool/YarnSpinner) 下载最新版本
2. 解压到 `Assets/Plugins/YarnSpinner/` 目录
3. 等待 Unity 导入资源

## 项目结构

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   └── Dialogue/
│   │       ├── DialogueManager.cs              # 对话管理器
│   │       └── YarnSpinnerIntegration/
│   │           └── CustomDialogueUI.cs         # 自定义UI组件
│   ├── Data/
│   │   └── Dialogue/
│   │       ├── Tutorial.yarn                   # 教程对话
│   │       ├── Level01.yarn                    # 关卡1对话
│   │       └── MiniGame01.yarn                 # 小游戏1对话
│   └── Prefabs/
│       └── UI/
│           └── DialogueUI.prefab               # 对话UI预制体
└── Plugins/
    └── YarnSpinner/                            # Yarn Spinner 插件文件
```

## 核心组件

### 1. DialogueManager

`DialogueManager` 是对话系统的核心管理器，负责：

- 桥接 Yarn Spinner 与游戏逻辑
- 注册自定义命令和函数
- 管理对话状态
- 处理游戏状态切换

#### 主要功能：

```csharp
// 开始对话
DialogueManager.Instance.StartDialogue("Start");

// 停止对话
DialogueManager.Instance.StopDialogue();

// 检查对话是否进行中
bool isActive = DialogueManager.Instance.IsDialogueActive;
```

### 2. CustomDialogueUI

`CustomDialogueUI` 是自定义的对话UI组件，提供：

- 打字机效果
- 自动推进选项
- 自定义UI布局
- 选择按钮管理

## Yarn 脚本编写

### 1. 基本语法

Yarn 脚本使用简单的标记语言，以下是基本语法：

```yarn
title: Start

<<if $visited_tutorial == false>>
    欢迎来到 Spotlight 2025！
    这是你的第一次游戏。
    <<set $visited_tutorial = true>>
<<else>>
    欢迎回来！
<<endif>>

-> 开始游戏
-> 查看设置
-> 退出游戏
```

### 2. 变量和条件

```yarn
title: Level01_Start

<<if $level_progress >= 0.5>>
    你已经完成了关卡的一半！
<<else>>
    继续努力吧！
<<endif>>

<<if $collected_items > 3>>
    你收集了很多物品！
<<endif>>
```

### 3. 自定义命令

项目集成了以下自定义命令：

#### 游戏状态命令
```yarn
<<set_game_state Playing>>
<<set_game_state Paused>>
<<set_game_state MiniGame>>
```

#### 小游戏命令
```yarn
<<complete_mini_game Puzzle01>>
<<complete_mini_game MemoryGame>>
```

#### 物品收集命令
```yarn
<<collect_item Key01>>
<<collect_item Gem_Red>>
```

#### 选择保存命令
```yarn
<<save_choice player_choice true>>
<<save_choice tutorial_completed false>>
```

#### 场景加载命令
```yarn
<<load_scene Level_02>>
<<load_scene MiniGame_01>>
```

#### 音效播放命令
```yarn
<<play_sound collect_item>>
<<play_sound level_complete>>
```

#### 图片显示命令
```yarn
<<show_image character_portrait>>
<<show_image tutorial_image>>
```

### 4. 自定义函数

项目集成了以下自定义函数：

#### 存档数据函数
```yarn
<<if get_save_data("level_01_completed") == true>>
    关卡1已经完成了！
<<endif>>
```

#### 物品检查函数
```yarn
<<if is_item_collected("Key01") == true>>
    你已经有钥匙了！
<<else>>
    你需要找到钥匙。
<<endif>>
```

#### 小游戏状态函数
```yarn
<<if is_mini_game_completed("Puzzle01") == true>>
    拼图游戏已经完成了！
<<endif>>
```

#### 关卡进度函数
```yarn
<<if get_level_progress() >= 0.8>>
    关卡进度已经达到80%！
<<endif>>
```

## 游戏中的实现

### 1. 设置对话触发器

创建对话触发器脚本：

```csharp
using UnityEngine;
using Spotlight2025.Dialogue;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private string dialogueNode;
    [SerializeField] private bool triggerOnStart = false;
    [SerializeField] private bool triggerOnCollision = true;
    
    private void Start()
    {
        if (triggerOnStart)
        {
            TriggerDialogue();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnCollision && other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }
    
    public void TriggerDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueNode);
        }
    }
}
```

### 2. 在关卡中使用

1. 在场景中创建空物体
2. 添加 `DialogueTrigger` 组件
3. 设置对话节点名称
4. 配置触发条件

### 3. 与小游戏集成

在小游戏中触发对话：

```csharp
// 小游戏完成时
public class MiniGameExample : MiniGameBase
{
    protected override void OnGameComplete()
    {
        base.OnGameComplete();
        
        // 触发完成对话
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("MiniGame_Complete");
        }
    }
}
```

### 4. 与UI集成

在UI按钮中触发对话：

```csharp
public class MainMenu : MonoBehaviour
{
    public void OnTutorialClicked()
    {
        DialogueManager.Instance.StartDialogue("Tutorial_Start");
    }
}
```

## 最佳实践

### 1. 对话文件组织

- 按功能分组：`Tutorial.yarn`, `Level01.yarn`, `MiniGame01.yarn`
- 使用描述性节点名称：`Level01_Start`, `Level01_Complete`, `Tutorial_Basic_Movement`
- 保持文件大小适中，避免单个文件过大

### 2. 变量命名

- 使用下划线分隔：`level_progress`, `collected_items`
- 使用描述性名称：`tutorial_completed`, `player_choice`
- 避免使用特殊字符和空格

### 3. 条件检查

- 总是检查变量是否存在
- 使用适当的条件逻辑
- 避免过于复杂的嵌套条件

### 4. 性能优化

- 避免在对话中频繁调用游戏命令
- 合理使用变量缓存
- 及时清理不需要的对话数据

## 调试技巧

### 1. 使用 Yarn Spinner 编辑器

1. 安装 [Yarn Spinner 编辑器](https://github.com/YarnSpinnerTool/YarnSpinnerEditor)
2. 打开 `.yarn` 文件进行编辑
3. 使用语法高亮和错误检查

### 2. 调试命令

在对话中添加调试信息：

```yarn
<<if $debug_mode == true>>
    调试信息：当前关卡进度 = {$level_progress}
<<endif>>
```

### 3. 日志输出

在 `DialogueManager` 中启用调试日志：

```csharp
[SerializeField] private bool enableDebugLogs = true;
```

## 常见问题

### 1. 对话不显示

- 检查 `DialogueRunner` 组件是否正确设置
- 确认 `.yarn` 文件已正确导入
- 验证节点名称是否正确

### 2. 命令不执行

- 检查命令名称是否正确
- 确认参数数量是否匹配
- 验证命令处理函数是否已注册

### 3. 变量不保存

- 检查变量名称是否正确
- 确认存档系统是否正常工作
- 验证变量作用域设置

## 扩展功能

### 1. 添加新命令

在 `DialogueManager` 中添加新命令：

```csharp
dialogueRunner.AddCommandHandler("custom_command", CustomCommand);
```

### 2. 添加新函数

在 `DialogueManager` 中添加新函数：

```csharp
dialogueRunner.AddFunction("custom_function", CustomFunction);
```

### 3. 自定义UI

继承 `CustomDialogueUI` 创建自定义UI：

```csharp
public class MyCustomDialogueUI : CustomDialogueUI
{
    // 自定义UI逻辑
}
```

## 总结

Yarn Spinner 为 Spotlight 2025 提供了强大的对话系统支持。通过合理的架构设计和最佳实践，可以创建丰富、交互性强的对话体验。记住要：

1. 保持对话文件组织清晰
2. 使用描述性的变量和节点名称
3. 合理使用自定义命令和函数
4. 注意性能优化和调试
5. 遵循最佳实践和命名规范

通过这个集成系统，你可以轻松创建复杂的对话树，实现丰富的剧情体验。
