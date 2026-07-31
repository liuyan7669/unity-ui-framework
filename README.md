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
https://github.com/liuyan7669/unity-ui-framework.git#1.0.1
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

## 创建面板

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

## 示例

Package Manager 中提供 `UI Framework Examples` 导入入口，包含常用界面脚本、Prefab 和配套字体资源。

## 环境要求

- Unity 2021.3 或更高版本
- Unity UI（`com.unity.ugui`）

## 许可证

本包使用 MIT License，详情参见 `LICENSE.md`。
