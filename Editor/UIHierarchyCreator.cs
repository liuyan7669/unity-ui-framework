#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cowart.UIFramework;

namespace Cowart.UIFramework.Editor
{
    /// <summary>
    /// 提供在 Unity Hierarchy 中一键创建标准 UGUI 框架结构的编辑器菜单。
    /// </summary>
    public static class UIHierarchyCreator
    {
        private const string MenuPath = "GameObject/Cowart/框架/创建 UI 系统";
        private const string PanelMenuPath = "GameObject/Cowart/UI/创建面板";
        private const string PanelBackgroundSpritePath = "UI/Skin/Background.psd";
        private const int UiMenuGroupPriority = 100;

        /// <summary>
        /// 创建 UIRoot、UICamera、EventSystem、UICanvas 及全部 UI 层级。
        /// </summary>
        /// <param name="menuCommand">Unity 菜单命令上下文。</param>
        [MenuItem(MenuPath, false, -10000)]
        private static void Create(MenuCommand menuCommand)
        {
            GameObject rootObject = new GameObject("UIRoot", typeof(UIRoot), typeof(UIManager));
            Undo.RegisterCreatedObjectUndo(rootObject, "Create UI Framework Root");
            GameObjectUtility.SetParentAndAlign(rootObject, menuCommand.context as GameObject);

            Camera uiCamera = CreateCamera(rootObject.transform);
            CreateEventSystem(rootObject.transform);
            Canvas canvas = CreateCanvas(rootObject.transform, uiCamera);

            RectTransform background = CreateLayer(canvas.transform, "BackgroundLayer");
            RectTransform normal = CreateLayer(canvas.transform, "NormalLayer");
            RectTransform fixedUI = CreateLayer(canvas.transform, "FixedLayer");
            RectTransform popup = CreateLayer(canvas.transform, "PopupLayer");
            RectTransform guide = CreateLayer(canvas.transform, "GuideLayer");
            RectTransform top = CreateLayer(canvas.transform, "TopLayer");

            UIRoot root = rootObject.GetComponent<UIRoot>();
            root.Configure(uiCamera, canvas, background, normal, fixedUI, popup, guide, top);

            Selection.activeGameObject = rootObject;
            EditorSceneManager.MarkSceneDirty(rootObject.scene);
        }

        /// <summary>检查当前场景是否允许创建新的 UIRoot。</summary>
        /// <returns>场景中不存在 UIRoot 时返回 true。</returns>
        [MenuItem(MenuPath, true)]
        private static bool ValidateCreate()
        {
            return !HasUIFrameworkHierarchy(Selection.activeGameObject);
        }

        /// <summary>
        /// 在右键选中的 UGUI 物体下创建一个铺满父节点的标准 Panel。
        /// </summary>
        /// <param name="menuCommand">Unity 菜单命令上下文。</param>
        [MenuItem(PanelMenuPath, false, -10000)]
        private static void CreatePanel(MenuCommand menuCommand)
        {
            GameObject parentObject = menuCommand.context as GameObject;
            if (parentObject == null)
            {
                parentObject = Selection.activeGameObject;
            }

            if (!CanCreatePanel(parentObject))
            {
                return;
            }

            DefaultControls.Resources resources = new DefaultControls.Resources
            {
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>(PanelBackgroundSpritePath)
            };
            GameObject panelObject = DefaultControls.CreatePanel(resources);
            GameObjectUtility.SetParentAndAlign(panelObject, parentObject);
            GameObjectUtility.EnsureUniqueNameForSibling(panelObject);
            panelObject.layer = parentObject.layer;

            Image panelImage = panelObject.GetComponent<Image>();
            Color panelColor = panelImage.color;
            panelColor.a = 0f;
            panelImage.color = panelColor;

            RectTransform panelTransform = panelObject.GetComponent<RectTransform>();
            panelTransform.anchorMin = Vector2.zero;
            panelTransform.anchorMax = Vector2.one;
            panelTransform.offsetMin = Vector2.zero;
            panelTransform.offsetMax = Vector2.zero;
            panelTransform.localScale = Vector3.one;

            Undo.RegisterCreatedObjectUndo(panelObject, "Create UI Panel");
            Selection.activeGameObject = panelObject;
            EditorSceneManager.MarkSceneDirty(panelObject.scene);
        }

        /// <summary>检查当前选择是否允许创建 UGUI Panel。</summary>
        /// <returns>场景已有 UIRoot 且选择的是场景内 UGUI 物体时返回 true。</returns>
        [MenuItem(PanelMenuPath, true)]
        private static bool ValidateCreatePanel()
        {
            return CanCreatePanel(Selection.activeGameObject);
        }

        /// <summary>检查上下文物体所在场景中是否已经存在 UI Framework 根节点。</summary>
        private static bool HasUIFrameworkHierarchy(GameObject contextObject)
        {
            Scene scene = contextObject != null && contextObject.scene.IsValid()
                ? contextObject.scene
                : SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return false;
            }

            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].GetComponentInChildren<UIRoot>(true) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>检查指定物体能否作为新 Panel 的父节点。</summary>
        private static bool CanCreatePanel(GameObject parentObject)
        {
            return parentObject != null
                && !EditorUtility.IsPersistent(parentObject)
                && HasUIFrameworkHierarchy(parentObject)
                && parentObject.transform is RectTransform
                && parentObject.GetComponentsInParent<Canvas>(true).Length > 0;
        }

        /// <summary>
        /// 创建用于渲染 UGUI 的正交相机。
        /// </summary>
        /// <param name="parent">相机的父节点。</param>
        /// <returns>创建完成的 UI 相机。</returns>
        private static Camera CreateCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("UICamera", typeof(Camera));
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -100f);

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Depth;
            camera.cullingMask = LayerMask.GetMask("UI");
            camera.orthographic = true;
            camera.depth = 100f;
            return camera;
        }

        /// <summary>
        /// 创建处理 UGUI 输入事件的 EventSystem。
        /// </summary>
        /// <param name="parent">EventSystem 的父节点。</param>
        private static void CreateEventSystem(Transform parent)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(parent, false);
        }

        /// <summary>
        /// 创建并配置使用屏幕空间相机模式的主 Canvas。
        /// </summary>
        /// <param name="parent">Canvas 的父节点。</param>
        /// <param name="uiCamera">负责渲染 Canvas 的相机。</param>
        /// <returns>创建完成的主 Canvas。</returns>
        private static Canvas CreateCanvas(Transform parent, Camera uiCamera)
        {
            GameObject canvasObject = new GameObject("UICanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            canvasObject.layer = LayerMask.NameToLayer("UI");
            canvasObject.transform.localScale = Vector3.one;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
            canvas.planeDistance = 100f;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return canvas;
        }

        /// <summary>
        /// 创建一个铺满主 Canvas 的 UI 显示层。
        /// </summary>
        /// <param name="parent">显示层的父节点。</param>
        /// <param name="name">显示层名称。</param>
        /// <returns>创建完成的层级 RectTransform。</returns>
        private static RectTransform CreateLayer(Transform parent, string name)
        {
            GameObject layerObject = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            layerObject.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = layerObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            return rectTransform;
        }
    }
}
#endif
