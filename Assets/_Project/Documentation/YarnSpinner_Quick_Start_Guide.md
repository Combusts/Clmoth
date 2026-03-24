# Yarn Spinner 快速开始指南

## 概述

本指南将帮助你在 Spotlight 2025 项目中快速设置和使用 Yarn Spinner 对话系统。

## 第一步：安装 Yarn Spinner

### 方法1：通过 Unity Package Manager（推荐）

1. 打开 Unity Editor
2. 选择 `Window` > `Package Manager`
3. 点击左上角的 `+` 按钮
4. 选择 `Add package from git URL`
5. 输入：`https://github.com/YarnSpinnerTool/YarnSpinner.git?path=/YarnSpinner.Unity#release`
6. 点击 `Add`

### 方法2：手动安装

1. 下载 [Yarn Spinner Unity 包](https://github.com/YarnSpinnerTool/YarnSpinner/releases)
2. 解压到 `Assets/Plugins/YarnSpinner/`
3. 等待 Unity 导入完成

## 第二步：设置对话系统

### 1. 创建对话管理器

1. 在场景中创建空物体，命名为 "DialogueManager"
2. 添加以下组件：
   - `DialogueManager`（项目自定义脚本）
   - `DialogueRunner`（Yarn Spinner 组件）
   - `CustomDialogueUI`（项目自定义脚本）

### 2. 配置 DialogueRunner

1. 选择 DialogueManager 物体
2. 在 DialogueRunner 组件中：
   - 设置 `Dialogue UI` 为 CustomDialogueUI 组件
   - 添加 `.yarn` 文件到 `Yarn Scripts` 列表

### 3. 创建对话UI

1. 创建 Canvas 物体
2. 添加以下UI元素：
   - 对话面板（Panel）
   - 角色名称文本（Text）
   - 对话内容文本（Text）
   - 继续按钮（Button）
   - 选择按钮容器（Panel）

## 第三步：编写对话脚本

### 1. 创建 .yarn 文件

1. 在 `Assets/_Project/Data/Dialogue/` 目录下创建 `.yarn` 文件
2. 使用 Yarn Spinner 编辑器或文本编辑器编写对话

### 2. 基本语法示例

```yarn
title: Start

欢迎来到游戏！

-> 开始游戏
-> 查看设置
-> 退出游戏
===

title: Start_Game

让我们开始游戏吧！

<<load_scene Level_01>>
===

title: View_Settings

设置菜单：

-> 返回主菜单
===

title: Quit_Game

你确定要退出吗？

-> 是的
-> 不
===
```

### 3. 使用变量和条件

```yarn
title: Level_Start

<<if $level_completed == false>>
    这是你的第一次游戏！
    <<set $level_completed = true>>
<<else>>
    欢迎回来！
<<endif>>

<<if $collected_items >= 3>>
    你已经收集了所有物品！
<<else>>
    你还需要收集 {3 - $collected_items} 个物品。
<<endif>>
```

## 第四步：在游戏中使用

### 1. 创建对话触发器

```csharp
using UnityEngine;
using Spotlight2025.Dialogue;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private string dialogueNode;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.StartDialogue(dialogueNode);
        }
    }
}
```

### 2. 在UI中触发对话

```csharp
public class MainMenu : MonoBehaviour
{
    public void OnStartGameClicked()
    {
        DialogueManager.Instance.StartDialogue("Start_Game");
    }
}
```

### 3. 与小游戏集成

```csharp
public class MiniGame : MiniGameBase
{
    protected override void OnGameComplete()
    {
        base.OnGameComplete();
        DialogueManager.Instance.StartDialogue("MiniGame_Complete");
    }
}
```

## 第五步：使用自定义命令

### 1. 游戏状态命令

```yarn
<<set_game_state Playing>>
<<set_game_state Paused>>
<<set_game_state MiniGame>>
```

### 2. 小游戏命令

```yarn
<<complete_mini_game Puzzle01>>
<<complete_mini_game MemoryGame>>
```

### 3. 物品收集命令

```yarn
<<collect_item Key01>>
<<collect_item Gem_Red>>
```

### 4. 场景加载命令

```yarn
<<load_scene Level_02>>
<<load_scene MiniGame_01>>
```

## 第六步：使用自定义函数

### 1. 检查存档数据

```yarn
<<if get_save_data("level_01_completed") == true>>
    关卡1已经完成了！
<<endif>>
```

### 2. 检查物品收集

```yarn
<<if is_item_collected("Key01") == true>>
    你已经有钥匙了！
<<endif>>
```

### 3. 检查小游戏状态

```yarn
<<if is_mini_game_completed("Puzzle01") == true>>
    拼图游戏已经完成了！
<<endif>>
```

### 4. 获取关卡进度

```yarn
<<if get_level_progress() >= 0.8>>
    关卡进度已经达到80%！
<<endif>>
```

## 第七步：调试和测试

### 1. 使用调试命令

```yarn
<<if $debug_mode == true>>
    调试信息：当前关卡进度 = {$level_progress}
<<endif>>
```

### 2. 检查日志输出

在 DialogueManager 中启用调试日志：

```csharp
[SerializeField] private bool enableDebugLogs = true;
```

### 3. 测试对话流程

1. 在 Unity 中运行游戏
2. 触发对话
3. 检查控制台输出
4. 验证命令执行

## 常见问题解决

### 1. 对话不显示

**问题**：对话UI不显示
**解决**：
- 检查 DialogueRunner 组件设置
- 确认 .yarn 文件已正确导入
- 验证节点名称是否正确

### 2. 命令不执行

**问题**：自定义命令不工作
**解决**：
- 检查命令名称是否正确
- 确认参数数量是否匹配
- 验证命令处理函数是否已注册

### 3. 变量不保存

**问题**：变量值不持久
**解决**：
- 检查变量名称是否正确
- 确认存档系统是否正常工作
- 验证变量作用域设置

## 最佳实践

### 1. 文件组织

- 按功能分组：`Tutorial.yarn`, `Level01.yarn`
- 使用描述性节点名称
- 保持文件大小适中

### 2. 变量命名

- 使用下划线分隔：`level_progress`
- 使用描述性名称：`tutorial_completed`
- 避免特殊字符

### 3. 条件检查

- 总是检查变量是否存在
- 使用适当的条件逻辑
- 避免过于复杂的嵌套

### 4. 性能优化

- 避免频繁调用游戏命令
- 合理使用变量缓存
- 及时清理不需要的数据

## 扩展功能

### 1. 添加新命令

```csharp
dialogueRunner.AddCommandHandler("custom_command", CustomCommand);
```

### 2. 添加新函数

```csharp
dialogueRunner.AddFunction("custom_function", CustomFunction);
```

### 3. 自定义UI

```csharp
public class MyCustomDialogueUI : CustomDialogueUI
{
    // 自定义UI逻辑
}
```

## 总结

通过这个快速开始指南，你应该能够：

1. 安装和设置 Yarn Spinner
2. 创建基本的对话脚本
3. 在游戏中使用对话系统
4. 使用自定义命令和函数
5. 调试和测试对话功能

记住要遵循最佳实践，保持代码整洁，并充分利用 Yarn Spinner 的强大功能来创建丰富的对话体验。

## 下一步

- 阅读完整的 [Yarn Spinner 集成文档](./YarnSpinner_Integration_Guide.md)
- 查看示例对话文件
- 尝试创建自己的对话脚本
- 探索更多高级功能
