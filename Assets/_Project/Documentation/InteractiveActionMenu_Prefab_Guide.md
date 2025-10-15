# InteractiveActionMenu Prefab Creation Guide

This guide explains how to create the `InteractiveActionMenu.prefab` manually in Unity Editor.

## Step 1: Create the Root GameObject

1. Right-click in the Project window → Create → Prefab
2. Name it `InteractiveActionMenu`
3. Double-click to open the prefab for editing

## Step 2: Setup Canvas

1. Select the root GameObject
2. Add Component → Canvas
3. Configure Canvas settings:
   - **Render Mode**: World Space
   - **Pixel Perfect**: ✓ (checked)
   - **Sort Order**: 10 (to appear above other UI)

4. Add Component → Canvas Scaler
5. Configure Canvas Scaler:
   - **UI Scale Mode**: Constant Pixel Size
   - **Scale Factor**: 1

6. Add Component → GraphicRaycaster

## Step 3: Create Background Panel

1. Right-click on Canvas → UI → Image
2. Name it `Background`
3. Configure Image component:
   - **Source Image**: None (use default)
   - **Color**: RGBA(0, 0, 0, 0.7) - Semi-transparent black

4. Configure RectTransform:
   - **Width**: 250
   - **Height**: 200
   - **Anchor**: Center
   - **Position**: (0, 0, 0)

## Step 4: Create Menu Items Container

1. Right-click on Background → UI → Panel
2. Name it `MenuItems`
3. Add Component → Vertical Layout Group
4. Configure Vertical Layout Group:
   - **Padding**: Left=10, Right=10, Top=10, Bottom=10
   - **Spacing**: 5
   - **Child Alignment**: Middle Center
   - **Child Controls Size**: ✓ Width, ✓ Height
   - **Child Force Expand**: ✓ Width, ✗ Height

5. Configure RectTransform:
   - **Anchor**: Stretch
   - **Left**: 0, **Right**: 0, **Top**: 0, **Bottom**: 0

## Step 5: Create Menu Item Template

1. Right-click on MenuItems → UI → Panel
2. Name it `MenuItemTemplate`
3. Add Component → Image
4. Configure Image:
   - **Color**: RGBA(0, 0, 0, 0.5) - Semi-transparent background

5. Configure RectTransform:
   - **Width**: 200
   - **Height**: 40

6. Add TextMeshPro Text as child:
   - Right-click on MenuItemTemplate → UI → Text - TextMeshPro
   - Configure TextMeshPro:
     - **Text**: "Action Name"
     - **Font Size**: 24
     - **Alignment**: Center
     - **Color**: White

7. Configure TextMeshPro RectTransform:
   - **Anchor**: Stretch
   - **Left**: 0, **Right**: 0, **Top**: 0, **Bottom**: 0

## Step 6: Setup InteractiveActionMenu Component

1. Select the root GameObject (Canvas)
2. Add Component → InteractiveActionMenu
3. Configure InteractiveActionMenu:
   - **Menu Prefab**: Drag this prefab itself into this field
   - **Menu Scale**: 0.01
   - **Menu Offset Y**: 2

## Step 7: Create Menu Item Prefab

1. Create another prefab from MenuItemTemplate
2. Name it `InteractiveActionMenuItem`
3. Add Component → InteractiveActionMenuItem
4. Configure colors in InteractiveActionMenuItem:
   - **Normal Color**: White
   - **Selected Color**: Yellow
   - **Normal Background Color**: RGBA(0, 0, 0, 0.5)
   - **Selected Background Color**: RGBA(1, 1, 0, 0.3)

## Step 8: Final Setup

1. Remove MenuItemTemplate from the main prefab (it's just for reference)
2. Save the InteractiveActionMenu prefab
3. Place the prefab in: `Assets/_Project/Prefabs/UI/InteractiveActionMenu.prefab`

## Usage Instructions

### For Table Setup:
1. Select your Table GameObject
2. In the Table component, configure:
   - **Actions**: Add InteractiveAction entries
   - **Action Menu Prefab**: Assign the InteractiveActionMenu prefab
   - **Menu Offset**: Adjust Y value to position menu above table
   - **Interacting Color**: Set to gray or desired color

### For Each Action:
1. Click the + button in Actions list
2. Set **Action Name** (e.g., "Sit Down", "Examine", "Use")
3. In **On Execute**, add UnityEvent calls to your desired methods

## Notes

- The menu will automatically scale to 0.01 (1% size) for world space
- Menu items are created dynamically based on the actions list
- W/S keys navigate, F key executes selected action
- Menu closes automatically after action execution
- Player movement is disabled while menu is open
