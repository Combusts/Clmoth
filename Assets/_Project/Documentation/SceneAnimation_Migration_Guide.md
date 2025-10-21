# 场景动画系统迁移指南

## 概述

本指南将帮助你将MainScene的场景动画控制从Animator状态机迁移到Animation组件系统。新的系统支持多动画同时保留状态，解决了Animator状态机无法保留前一个动画效果的问题。

## 迁移原因

### 当前问题
- Animator状态机一次只能处于一个状态
- 状态切换时前一个状态的效果会丢失
- 无法实现多个动画效果的叠加

### 新系统优势
- 使用Animation组件，支持多动画同时播放
- 可以保留每个动画的最终状态
- 更灵活的动画控制
- 保持Yarn命令兼容性

## Unity Editor配置步骤

### 1. 准备动画文件

确保以下动画文件存在于 `Assets/_Project/Scenes/Levels/Level_01/` 目录：
- `Manager_Exit.anim`
- `DocumentPop.anim`
- `DocumentExit.anim`

### 2. 在MainScene中配置

#### 步骤1：移除Animator组件
1. 选择MainScene GameObject
2. 在Inspector中找到Animator组件
3. 点击组件右上角的齿轮图标
4. 选择"Remove Component"

#### 步骤2：添加Animation组件
1. 在Inspector中点击"Add Component"
2. 搜索并添加"Animation"组件
3. 确保Animation组件已启用

#### 步骤3：添加SceneAnimationManager脚本
1. 在Inspector中点击"Add Component"
2. 搜索并添加"SceneAnimationManager"脚本

#### 步骤4：配置动画列表
1. 在SceneAnimationManager组件的"Animations"列表中
2. 设置Size为3
3. 配置每个动画项：

**Manager_Exit动画：**
- Name: `Manager_Exit`
- Clip: 拖入`Manager_Exit.anim`
- Keep Last Frame: ✓ (勾选)
- Weight: 1

**DocumentPop动画：**
- Name: `DocumentPop`
- Clip: 拖入`DocumentPop.anim`
- Keep Last Frame: ✓ (勾选)
- Weight: 1

**DocumentExit动画：**
- Name: `DocumentExit`
- Clip: 拖入`DocumentExit.anim`
- Keep Last Frame: ✓ (勾选)
- Weight: 1

### 3. 测试配置

#### 在编辑器中测试
1. 选择MainScene GameObject
2. 在SceneAnimationManager组件中
3. 右键点击组件标题
4. 选择"Preview All Animations"（仅在运行时可用）

#### 在游戏中测试
运行游戏并触发Yarn对话，观察动画是否正确播放并保留最终状态。

## Yarn脚本使用

### 现有命令保持不变

你的Yarn脚本无需修改，现有命令继续有效：

```yarn
// 播放动画并等待完成，保持在最后一帧
<<animator_play_and_wait MainScene "Manager_Exit">>

// 播放动画并保持在最后一帧
<<animator_play_and_stay MainScene "DocumentPop">>

// 触发动画（等同于play_and_stay）
<<animator_trigger MainScene "DocumentExit">>
```

### 新增命令（可选）

如果需要更精细的控制，可以使用新命令：

```yarn
// 停止特定动画
<<stop_scene_animation MainScene "Manager_Exit">>

// 停止所有动画
<<stop_all_scene_animations MainScene>>

// 设置动画播放速度
<<set_animation_speed MainScene "DocumentPop" 0.5>>
```

## 系统架构

### 组件优先级
AnimationCommands脚本会按以下优先级选择动画组件：

1. **SceneAnimationManager** - 最高优先级，用于场景动画
2. **Animation** - 中等优先级，备用场景动画方案
3. **Animator** - 最低优先级，用于角色动画等

### 兼容性
- 保持所有现有Yarn命令名称不变
- 支持BaseCharacter等使用Animator的GameObject
- 自动检测并选择合适的动画组件

## 故障排除

### 常见问题

**Q: 动画不播放**
- 检查Animation组件是否已添加并启用
- 确认动画文件路径正确
- 检查SceneAnimationManager配置是否正确

**Q: 动画播放但不停留在最后一帧**
- 确认"Keep Last Frame"选项已勾选
- 检查动画文件是否设置为"Once"模式

**Q: Yarn命令报错**
- 确认GameObject名称正确（如"MainScene"）
- 检查SceneAnimationManager脚本是否已添加

### 调试信息

在Console中查看以下调试信息：
- `AnimationCommands: 检测到SceneAnimationManager组件在 MainScene`
- `SceneAnimationManager: 播放动画: Manager_Exit`

## 备份和回滚

### 备份文件
- `MainSceneAnimator.controller` - 保留作为备份
- `MainSceneAnimator.controller.meta` - 保留作为备份

### 如需回滚
1. 移除Animation组件和SceneAnimationManager脚本
2. 重新添加Animator组件
3. 将Animator Controller设置为`MainSceneAnimator.controller`

## 性能考虑

### Animation vs Animator
- Animation组件更轻量，适合简单动画
- Animator适合复杂的动画状态机
- 场景动画使用Animation，角色动画继续使用Animator

### 内存使用
- Animation组件内存占用更少
- 支持更多同时播放的动画
- 不会产生状态机开销

## 扩展功能

### 添加新动画
1. 创建新的AnimationClip文件
2. 在SceneAnimationManager中添加新的SceneAnimation项
3. 在Yarn脚本中使用相同的命令调用

### 自定义动画行为
可以通过修改SceneAnimationManager脚本来实现：
- 动画混合
- 动画序列
- 条件动画播放
- 动画事件回调

## 总结

通过这次迁移，你获得了：
- 更灵活的动画控制系统
- 多动画状态保留能力
- 保持现有Yarn脚本兼容性
- 更好的性能和扩展性

如果遇到任何问题，请参考故障排除部分或检查Console中的调试信息。
