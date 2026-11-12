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
https://github.com/liuyan7669/unity-ui-framework.git#2.0.2
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

## UGUI 快捷扩展（已迁移）

从 `2.0.0` 起，UI Framework 不再包含 UGUI/TMP 快捷扩展，专注于界面分层和面板管理。原 `UIEventExtensions`、`UILayoutExtensions` 中的全部方法已移入 [Cw Extensions](https://github.com/liuyan7669/unity-cw-extensions)（`com.cowart.cw-extensions` 3.0.0）。从 1.x 升级时：

1. 安装 `com.cowart.cw-extensions` 3.0.0 或更高版本。
2. 在调用处添加 `using Cowart.CwExtensions;`。
3. 把方法前缀 `Ext` 改为 `Cw`，例如 `button.ExtSetOnClick(...)` → `button.CwSetOnClick(...)`、`content.ExtRebuildLayoutTree()` → `content.CwRebuildLayoutTree()`；参数不变。

## 示例

Package Manager 中提供 `UI Framework Examples` 导入入口，包含常用界面脚本、Prefab、配套字体资源和新手引导示例。

`UIQuestion` 示例需要 Config Loader 和 Object Utilities。`UINewbieGuide` 示例使用 DOTween 播放聚焦、点按、框选和拖放动画。导入对应示例前，请先安装这些可选依赖；它们不再作为 UI Framework 核心包的强制依赖。

## 测试

包内 `Tests/Editor` 提供 EditMode 测试，覆盖 `UIManager` 的面板缓存与类型匹配、`UIView` 生命周期、`UIRoot` 层级配置。

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
- TextMesh Pro 2.1.6 或更高版本（示例界面使用，通过包依赖自动引入）
- Config Loader、Object Utilities（仅 `UIQuestion` 示例需要）
- DOTween（仅 `UINewbieGuide` 示例需要）

## 许可证

本包使用 MIT License，详情参见 `LICENSE.md`。

## Inspector Groups 可选兼容

安装 Inspector Groups 1.2.2 或更高版本（提供 `InspectorGroupRenderer` 入口）后，UIView 子类的字段和成员支持 FoldoutGroup、EndFoldout、条件显示、Button 等包内特性；UIView 基础设置仍由 UI Framework 单独绘制，不重复显示。未安装或安装的旧版本尚未提供该入口时，子类字段使用普通 Inspector 绘制。UI Framework 不添加对 Inspector Groups 的程序集或 UPM 依赖；使用特性的业务脚本仍需自行引用对应特性包。
