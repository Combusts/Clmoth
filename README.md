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
│   │   ├── Core/               # 核心系统 (GameManager, ResourceManager, SceneLoader)
│   │   ├── Player/             # 玩家控制
│   │   ├── Dialogue/           # 对话系统 (DialogueManager)
│   │   ├── SaveSystem/         # 存档系统
│   │   ├── Level/              # 关卡逻辑
│   │   ├── MiniGames/          # 小游戏系统
│   │   ├── Puzzle/             # 解谜元素
│   │   ├── UI/                 # UI脚本 (UIManager)
│   │   └── Data/               # ScriptableObject数据定义
│   │       ├── AudioData.cs    # 音频配置
│   │       ├── DialogueData.cs # 对话数据
│   │       └── LevelConfig.cs  # 关卡配置
│   │
│   ├── Resources/              # 运行时动态加载资源
│   │   ├── Audio/              # 音频资源
│   │   │   ├── Music/          # 背景音乐 (BGM_MainMenu.mp3等)
│   │   │   └── SFX/           # 音效 (UI_Click.wav等)
│   │   ├── Prefabs/           # 动态预制体
│   │   │   ├── Characters/    # 角色预制体 (Player.prefab, NPC_Guide.prefab)
│   │   │   ├── UI/            # UI预制体 (DialogueBox.prefab, PauseMenu.prefab)
│   │   │   ├── Effects/       # 特效预制体 (VFX_Sparkle.prefab)
│   │   │   └── Items/         # 物品预制体 (Collectible_Key.prefab)
│   │   ├── Data/              # ScriptableObject数据实例
│   │   │   ├── Dialogues/     # 对话数据文件
│   │   │   ├── Levels/        # 关卡配置文件
│   │   │   └── Audio/         # 音频配置数据库
│   │   └── Localization/      # 本地化文本 (EN.json, ZH.json)
│   │
│   ├── Scenes/                # 场景文件
│   │   ├── MainMenu.unity     # 主菜单
│   │   ├── Level01.unity      # 关卡1
│   │   └── Level02.unity      # 关卡2
│   │
│   ├── Prefabs/               # 编辑器用预制体 (静态引用)
│   │   ├── Environment/       # 环境对象
│   │   ├── MiniGames/         # 小游戏组件
│   │   ├── Player/           # 玩家相关
│   │   └── UI/               # 静态UI组件
│   │
│   ├── Art/                   # 编辑器用美术资源
│   │   ├── Textures/          # 纹理文件
│   │   ├── Models/            # 3D模型
│   │   └── Animations/        # 动画文件
│   │
│   ├── Audio/                 # 音频源文件 (编辑时引用)
│   └── Materials/             # 材质和着色器
├── Plugins/                   # 第三方插件
└── Settings/                  # URP设置
```

### 项目结构设计理由

#### 🎯 双Prefabs目录设计
- **`Prefabs/`** (编辑器用)：通过Unity编辑器直接引用，编译时打包到场景中
  - 适用于：静态环境对象、固定UI组件、小游戏基础组件
  - 优势：性能最优，无需运行时加载开销
  
- **`Resources/Prefabs/`** (运行时动态加载)：通过`Resources.Load()`动态加载
  - 适用于：动态生成的敌人、特效、可收集物品、弹窗UI
  - 优势：灵活性强，支持按需加载和内存管理

#### 📁 Resources目录设计
- **核心原则**：只存放需要运行时动态加载的资源
- **Audio分类**：Music(背景音乐)和SFX(音效)分离，便于音量控制
- **Data分类**：ScriptableObject数据实例，支持热更新和配置管理
- **Localization**：国际化支持，便于多语言扩展

#### 🎨 Art vs Resources分离
- **Art目录**：编辑器用资源，不参与运行时加载
- **Resources目录**：运行时资源，支持动态加载
- **优势**：避免资源重复，优化构建包大小

#### 📂 Scripts模块化设计
- **按功能模块分类**：Core、Player、Dialogue等
- **Data子目录**：ScriptableObject数据定义，与Resources/Data实例分离
- **优势**：代码结构清晰，便于团队协作和维护

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
