# Changelog

## [2.0.2] - 2026-11-12

- 同步 UIRoot Inspector 测试，移除对已隐藏 Script 字段的过时要求。

- 统一名称和关联包引用，修正 UGUI 程序集引用（适用时），保持核心包独立。

## [2.0.1] - 2026-11-12

- UIRoot 和 UIView 设置使用带左右边距的圆角折叠分组，首次默认收起，按组件和分组独立保存会话状态。
- 增加 UIRoot 层级说明框上下间距，隐藏 UIRoot 和 UIManager 的 Script 引用行。
- 可选兼容 Inspector Groups 1.2.2 或更高版本，支持 UIView 子类的分组、条件显示和按钮等特性，基础字段不重复绘制；未安装兼容入口时退回普通绘制，不新增硬依赖。

## [2.0.0] - 2026-11-08

### 破坏性变更

- 移除 `UIEventExtensions` 和 `UILayoutExtensions` 中全部 UGUI/TMP 快捷扩展（23 个方法名、33 个方法，如 `ExtSetOnClick`、`ExtSetOnValueChanged`、`ExtGetRect`、`ExtSetAlpha`、`ExtSetPivotKeepPosition`、`ExtRebuildLayoutTree`）。框架核心与示例均未使用这些扩展。它们已迁移到 Extensions（`com.cowart.cw-extensions`）2.0.0，前缀改为 `Cw`，例如 `button.ExtSetOnClick(...)` → `button.CwSetOnClick(...)`。
- 运行时程序集不再引用 `Unity.TextMeshPro`。包仍依赖 TextMesh Pro，供示例界面使用。
- 移除对应的 EditMode 测试。

## [1.1.4] - 2026-11-08

- 统一 Cowart 包图标：重新设计为按功能类别配色的圆角图标（层叠的面板窗口），同步更新 `Icon.png` 与 `Icon@2x.png`。

## [1.1.3] - 2026-08-23

- 新增包专用图标，并将包内组件脚本的默认图标统一替换为该图标。

## [1.1.2] - 2026-08-23

- 将 `UIQuestion` 示例同步到 Object Utilities 的 `PrefabSpawner.SpawnedObjects` 新属性。
- 更新示例 Prefab 的生成对象列表序列化字段名，保持导入后的引用一致。

## [1.1.1] - 2026-08-21

- 新增 `UIRoot` 自定义 Inspector，将基础引用和界面层级分区显示。
- 为 Background、Normal、Fixed、Popup、Guide 和 Top 层补充中文用途说明。
- 新增自定义 Inspector 注册及序列化字段缓存测试。

## [1.1.0] - 2026-08-19

- 修复使用基类静态类型传入 Prefab 时，`UIManager.Open` 重复创建并遗留失管面板的问题。
- 明确无条件 API 的精确类型匹配与 Predicate API 的多态匹配规则，并简化 `CloseAll` 的关闭流程。
- 修正 `UIManager`、`UIRoot` 类型文档和 RectTransform 坐标、尺寸扩展的 XML 注释。
- 新增 `UIManager`、`UIView`、`UIRoot` 和 `UIEventExtensions` 的 EditMode 测试，并增强测试程序集隔离配置。
- 在 README 中补充类型匹配规则、测试启用方式和覆盖范围。

## [1.0.12] - 2026-08-15

- 删除 `UIEmptyPanel` 占位组件及其自动替换编辑器逻辑；创建面板时不再自动挂载 `UIView` 脚本。
- 将最低支持版本调整为 Unity 2019.4，并兼容旧版父级组件查询及 UGUI `InputField` 提交事件接口。
- 移除核心代码未使用的 Cowart 强制依赖，将 TextMesh Pro 最低依赖调整为 2.1.6。

## [1.0.11] - 2026-08-07

- 新增 RectTransform 锚点位置、尺寸和保持视觉位置修改 Pivot 的扩展。
- 新增 CanvasGroup 状态、UGUI/TMP 文本显隐和 Selectable 关闭导航扩展。
- 增加 UI 布局扩展的 EditMode 自动化测试。

## [1.0.10] - 2026-08-07

- 将顶部和物体右键的 UI 菜单统一归入独立优先级分组。
- UI 菜单与物体、组件、碰撞体工具组及其他 Cowart 工具组分别显示分隔线。

## [1.0.9] - 2026-08-07

- 新增 `Cowart > UI > 锚点匹配四角` 顶部菜单，并保留对应的物体右键菜单。
- 两个入口共用同一套多选处理逻辑。
- 调整 Undo 分组边界，全部选中 UI 统一归入一次撤销，异常时整批回滚。

## [1.0.8] - 2026-08-07

- 新增 `GameObject > Cowart > UI > 锚点匹配四角` 菜单。
- 支持同时处理多个选中 RectTransform，并保持当前视觉位置和尺寸不变。
- 支持整批 Undo 撤销和 Prefab 实例属性覆盖记录。

## [1.0.7] - 2026-08-07

- 将 UI 事件扩展的公开回调类型统一改为 `System.Action`。
- 补充 Toggle、InputField、Dropdown、Slider、Scrollbar、ScrollRect、MaskableGraphic 和 EventTrigger 事件快捷绑定。
- 补充 TMP_InputField、TMP_Dropdown 的公开 UnityEvent 快捷绑定。

## [1.0.6] - 2026-08-07

- 新增 `ExtSetOnClick(Button, Action, bool)`，默认清空已有运行时点击监听后添加新回调。

## [1.0.5] - 2026-08-07

- 新增 `ExtSetAlpha(Image, float)`，支持保留 RGB 并直接设置 Image 颜色透明度。

## [1.0.4] - 2026-08-07

- 新增 `ExtGetRect(Component)`，支持从 UI 组件直接获取所在对象的 `RectTransform`。

## [1.0.3] - 2026-08-07

- 从 Extensions 包迁入 `UILayoutExtensions`，保留原类名、全局命名空间和扩展方法签名。
- 新增 RectTransform 布局树即时重建和下一帧最近布局节点重建能力。

## [1.0.2] - 2026-08-07

- 新增 `UINewbieGuide` 新手引导示例脚本、Prefab 和手指 Sprite。
- 支持点按、精确文字框选和拖放引导动画。
- 修正示例 Prefab 的引导层序列化字段并移除失效的占位 Sprite 引用。
- 补充 `UINewbieGuide` 示例的 DOTween 前置依赖说明。

## [1.0.1] - 2026-07-31

- 将 Hierarchy 创建入口统一到 `GameObject > Cowart > UI > 创建 UI 框架层级`。
- 更新 Config Loader 和 Object Utilities 依赖版本。

## [1.0.0] - 2026-07-23

- 新增 UIRoot、UIManager、UIView、UIArgs 和 UILayer 核心类型。
- 新增场景面板自动扫描、层级校正和 UGUI Hierarchy 创建菜单。
- 新增面板打开、查找、关闭、批量关闭和删除接口。
- 集成 UI Framework 示例脚本、Prefab 和字体资源。
- 补充示例所需的 Config Loader、Object Utilities 和 TextMesh Pro 依赖。
