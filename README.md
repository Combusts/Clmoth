# Spotlight 2025 - 横版解谜游戏

## 项目简介

Spotlight 2025 是一款基于Unity 2022开发的横版解谜剧情推动类型游戏。游戏采用主线横版推进模式，结合小游戏元素，为玩家提供丰富的游戏体验。

## 技术规格

- **引擎**: Unity 2022.3.62f2c1
- **渲染管线**: Universal Render Pipeline (URP)
- **平台**: 2D游戏
- **对话系统**: Yarn Spinner
- **存档系统**: JSON序列化

## 项目结构

```
Assets/
├── _Project/                    # 项目核心代码和资源
│   ├── Scripts/                 # 所有C#脚本
│   │   ├── Core/               # 核心系统
│   │   ├── Player/             # 玩家控制
│   │   ├── Dialogue/           # 对话系统
│   │   ├── SaveSystem/         # 存档系统
│   │   ├── Level/              # 关卡逻辑
│   │   ├── MiniGames/          # 小游戏系统
│   │   ├── Puzzle/             # 解谜元素
│   │   ├── UI/                 # UI脚本
│   │   └── Data/               # ScriptableObject数据
│   ├── Scenes/                 # 场景文件
│   ├── Prefabs/                # 预制体
│   ├── Art/                    # 美术资源
│   ├── Audio/                  # 音频资源
│   └── Materials/              # 材质和着色器
├── Plugins/                    # 第三方插件
└── Settings/                   # URP设置
```

## 核心系统

### 1. 游戏管理器 (GameManager)
- 单例模式管理游戏状态
- 处理场景切换和游戏流程
- 支持游戏状态：主菜单、游戏中、暂停、小游戏、对话、加载

### 2. 存档系统 (SaveSystem)
- 使用JSON序列化保存数据
- 实现ISaveable接口供各模块使用
- 保存内容：关卡进度、对话选项、收集物、小游戏完成状态
- 支持多存档插槽

### 3. 场景架构
- PersistentScene：持久化场景，包含核心管理器
- Additive Loading：使用场景叠加加载，实现无缝过渡

### 4. 小游戏系统
- MiniGameBase抽象基类，统一接口
- MiniGameManager管理进入/退出小游戏
- 支持多种小游戏类型：解谜、动作、记忆、反应等

### 5. 对话系统集成
- 集成Yarn Spinner插件
- DialogueManager桥接Yarn Spinner与游戏逻辑
- 对话数据存储在Data/Dialogue/

### 6. 事件系统
- EventBus事件总线，解耦系统间通信
- 支持游戏状态变化、场景加载、小游戏结束等事件

## 开发规范

### 命名规范
- C#类：PascalCase（如PlayerController）
- 变量/字段：camelCase（如currentHealth）
- 常量：UPPER_SNAKE_CASE（如MAX_HEALTH）
- 场景：PascalCase带下划线（如Level_01）
- Prefab：PascalCase（如PlayerCharacter）

### 代码结构
- 使用命名空间：Spotlight2025.模块名
- 核心系统使用单例模式
- 组件间通信使用事件系统
- 数据配置使用ScriptableObject

## 快速开始

1. 打开Unity 2022.3.62f2c1
2. 打开项目文件夹
3. 等待Unity导入资源
4. 运行MainMenu场景开始游戏

## 依赖插件

- Yarn Spinner：对话系统
- Universal Render Pipeline：渲染管线

## 开发团队

- 项目负责人：[待填写]
- 程序员：[待填写]
- 美术：[待填写]
- 策划：[待填写]

## 版本历史

- v1.0.0 - 初始项目结构搭建
  - 完成核心系统架构
  - 实现存档系统
  - 集成小游戏框架
  - 设置对话系统基础

## 许可证

[待填写许可证信息]
