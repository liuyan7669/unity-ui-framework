# UI Framework

基于 Unity UGUI 的轻量界面框架，提供标准 UI Hierarchy、面板分层、场景面板扫描、面板复用和统一静态访问接口。

## 功能

- 一键创建 UIRoot、UICamera、EventSystem、UICanvas 和六个 UI Layer
- 启动时扫描并注册场景中已存在的 UIView
- 根据 UIView.Layer 在编辑器和运行时自动校正面板层级
- 通过 UIManager 静态接口打开、查找、关闭和删除面板
- 支持 Predicate 条件查询、关闭和删除
- 支持批量关闭以及排除指定类型
- 使用 UIArgs 子类传递面板专用参数
- 提供 RectTransform、Image、UGUI/TMP 事件和布局树重建扩展
- 支持多选 UI 锚点匹配当前矩形四角，并可通过 Undo 撤销

## 安装

### 通过 OpenUPM 安装

在 Unity 的 `Edit > Project Settings > Package Manager` 中添加：

```text
Name: package.openupm.com
URL: https://package.openupm.com
Scope: com.cowart.ui-framework
```

然后通过 `Add package by name` 安装：

```text
com.cowart.ui-framework
```

### 通过 Git URL 安装

```text
https://github.com/liuyan7669/unity-ui-framework.git#1.1.1
```

## 创建 UI 根节点

在 Unity 菜单中选择：

```text
GameObject > Cowart > UI > 创建 UI 框架层级
```

生成的 Hierarchy：

```text
UIRoot
├── UICamera
├── EventSystem
└── UICanvas
    ├── BackgroundLayer
    ├── NormalLayer
    ├── FixedLayer
    ├── PopupLayer
    ├── GuideLayer
    └── TopLayer
```

选择 `UIRoot` 后，Inspector 会把基础引用和界面层级分区显示，并在每个层级下直接给出中文用途说明，方便判断不同界面应该放在哪一层。

## 锚点匹配四角

在 Hierarchy 中选择一个或多个具有 `RectTransform` 父节点的 UI，然后执行：

```text
Cowart > UI > 锚点匹配四角
```

也可以通过物体右键菜单执行：

```text
GameObject > Cowart > UI > 锚点匹配四角
```

两个入口共用同一套批量逻辑。工具会把每个 UI 的 `anchorMin`、`anchorMax` 移动到其当前矩形四角，并将四边偏移归零。处理前后的视觉位置和尺寸保持不变，全部选中物体统一归入一次 Undo；处理中发生异常时会整体回滚。

## 创建面板

场景中已有 UI Framework 层级后，在任意 UGUI 物体上右键选择：

```text
GameObject > Cowart > UI > 创建面板
```

新面板会铺满父节点，`Image` 颜色为全透明，并且不会自动挂载 `UIView` 脚本。请创建自己的面板脚本并继承 `UIView`，再将它添加到面板物体：

面板根节点脚本继承 `UIView`：

```csharp
using Cowart.UIFramework;

public sealed class UIHome : UIView
{
    protected override void OnOpen(UIArgs args)
    {
        UIHomeArgs homeArgs = args.As<UIHomeArgs>();
    }

    protected override void OnClose()
    {
    }
}

public sealed class UIHomeArgs : UIArgs
{
    public string UserName;
}
```

## 使用 UIManager

```csharp
using Cowart.UIFramework;

UIHome home = UIManager.Open<UIHome>(new UIHomeArgs());
UIHome found = UIManager.Find<UIHome>();
UIManager.Close<UIHome>();
UIManager.Delete<UIHome>();
```

条件与批量操作：

```csharp
UIHome[] activeHomes = UIManager.FindAll<UIHome>(view => view.gameObject.activeInHierarchy);
UIManager.Close<UIView>(view => view.gameObject.activeInHierarchy);
UIManager.CloseAll(typeof(UIHome));
```

`UIManager` 以面板的具体运行时类型作为唯一缓存键，同一个具体面板类型只保留一个受管理实例。类型匹配规则如下：

- `Open<T>(args)`、`Find<T>()`、`Close<T>()` 和 `Delete<T>()` 按 `T` 精确匹配已注册面板。
- `Open<T>(prefab, args)` 按 `prefab.GetType()` 查找和复用实例；即使 Prefab 变量声明为 `UIView`，也不会重复创建相同的具体面板。
- 带 `Predicate<T>` 的 `Find`、`FindAll`、`Close` 和 `Delete` 会遍历已注册面板，并包含可转换为 `T` 的派生类型。

因此，需要处理全部面板时应使用条件重载，例如 `UIManager.Close<UIView>(view => true)`；无条件的 `Close<UIView>()` 只会查找精确的 `UIView` 类型，而 `UIView` 本身是抽象类，正常情况下不会注册该键。

## UGUI 快捷扩展

`UILayoutExtensions` 和 `UIEventExtensions` 位于全局命名空间，无需额外添加 using：

```csharp
Button button = GetComponent<Button>();
Image image = GetComponent<Image>();
RectTransform panel = GetComponent<RectTransform>();

RectTransform buttonRect = button.ExtGetRect();
image.ExtSetAlpha(0.5f);
button.ExtSetOnClick(OnStartButtonClick);
panel.ExtSetAnchoredPositionX(100f);
panel.ExtSetSizeDeltaY(300f);
panel.ExtSetPivotKeepPosition(new Vector2(0.5f, 1f));
panel.ExtRebuildLayoutTree();
panel.ExtRebuildNearestLayoutNextFrame(this);
```

`ExtGetRect` 可由 Button、Image、TMP_Text、Slider、Toggle 等 UI 组件直接调用。目标为空或所在对象不是 RectTransform 时返回 `null`。

`ExtSetAlpha` 会保留 Image 原有 RGB，只修改颜色透明度，并自动将参数限制在 `0–1`。

还提供 `ExtSetCanvasGroupState`、UGUI/TMP 两种 `ExtSetText` 和 `ExtSetNavigationNone`，用于一次设置 CanvasGroup 状态、按文本内容控制显隐以及关闭 Selectable 导航。

事件扩展统一使用 `System.Action`。默认先移除对应事件中通过代码注册的全部运行时监听，再添加新回调；最后一个参数传入 `false` 时保留已有监听并追加。Unity 的 `RemoveAllListeners` 不会移除 Inspector 中配置的持久化监听。

```csharp
toggle.ExtSetOnValueChanged(OnToggleChanged);
inputField.ExtSetOnValueChanged(OnInputChanged);
inputField.ExtSetOnEndEdit(OnInputEndEdit);
dropdown.ExtSetOnValueChanged(OnDropdownChanged);
slider.ExtSetOnValueChanged(OnSliderChanged);
scrollbar.ExtSetOnValueChanged(OnScrollbarChanged);
scrollRect.ExtSetOnValueChanged(OnScrollPositionChanged);

tmpInputField.ExtSetOnValueChanged(OnInputChanged);
tmpInputField.ExtSetOnSubmit(OnInputSubmitted);
tmpDropdown.ExtSetOnValueChanged(OnDropdownChanged);

button.ExtSetOnClick(OnAdditionalClick, false);
```

当前事件扩展覆盖 `Button`、`Toggle`、`InputField`、`Dropdown`、`Slider`、`Scrollbar`、`ScrollRect`、`MaskableGraphic`、`EventTrigger`、`TMP_InputField` 和 `TMP_Dropdown` 的公开 UnityEvent。

Unity 2019 和 2020 的旧版 UGUI 没有独立的 `InputField.onSubmit`，因此该版本中的 `InputField.ExtSetOnSubmit` 会兼容绑定到 `onEndEdit`，输入框失去焦点时也会触发回调。`TMP_InputField.ExtSetOnSubmit` 不受影响。

`ExtRebuildLayoutTree` 会按照先子节点、后父节点的顺序立即重建布局树。延迟重建需要一个有效的 `MonoBehaviour` 启动协程；不传参数时会从目标父级自动查找。

## 示例

Package Manager 中提供 `UI Framework Examples` 导入入口，包含常用界面脚本、Prefab、配套字体资源和新手引导示例。

`UIQuestion` 示例需要 Configuration Tools 和 Object Tools。`UINewbieGuide` 示例使用 DOTween 播放聚焦、点按、框选和拖放动画。导入对应示例前，请先安装这些可选依赖；它们不再作为 UI Framework 核心包的强制依赖。

## 测试

包内 `Tests/Editor` 提供 EditMode 测试，覆盖 `UIManager` 的面板缓存与类型匹配、`UIView` 生命周期、`UIRoot` 层级配置，以及 UGUI/TMP 事件和布局扩展。

通过 Package Manager 安装后，如需在 Test Runner 中显示包测试，请在项目的 `Packages/manifest.json` 根对象中添加 `testables` 字段（以下仅示意）：

```json
{
  "testables": [
    "com.cowart.ui-framework"
  ]
}
```

然后打开 `Window > General > Test Runner`，在 EditMode 页签运行 `Cowart.UIFramework.Editor.Tests`。

## 环境要求

- Unity 2019.4 或更高版本
- Unity UI（`com.unity.ugui`）
- TextMesh Pro 2.1.6 或更高版本
- Configuration Tools、Object Tools（仅 `UIQuestion` 示例需要）
- DOTween（仅 `UINewbieGuide` 示例需要）

## 许可证

本包使用 MIT License，详情参见 `LICENSE.md`。
