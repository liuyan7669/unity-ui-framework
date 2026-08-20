using System.Reflection;
using Cowart.UIFramework;
using NUnit.Framework;
using UnityEngine;

namespace Cowart.UIFramework.Tests
{
    public sealed class UIRootTests
    {
        private static readonly FieldInfo InstanceField = typeof(UIRoot).GetField(
            "<Instance>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly FieldInfo DontDestroyOnLoadField = typeof(UIRoot).GetField(
            "dontDestroyOnLoad",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo AwakeMethod = typeof(UIRoot).GetMethod(
            "Awake",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private UIRoot previousInstance;
        private GameObject rootObject;

        [SetUp]
        public void SetUp()
        {
            Assert.IsNotNull(InstanceField, "找不到 UIRoot.Instance 的后备字段。");
            previousInstance = UIRoot.Instance;
            SetInstance(null);
        }

        [TearDown]
        public void TearDown()
        {
            SetInstance(null);
            if (rootObject != null)
            {
                Object.DestroyImmediate(rootObject);
            }

            if (previousInstance != null)
            {
                SetInstance(previousInstance);
            }
        }

        [Test]
        public void Configure_StoresReferences_AndGetLayerReturnsConfiguredTransforms()
        {
            UIRootHierarchy hierarchy = CreateHierarchy();

            Configure(hierarchy);

            Assert.AreSame(hierarchy.Camera, hierarchy.Root.UICamera);
            Assert.AreSame(hierarchy.Canvas, hierarchy.Root.UICanvas);
            Assert.AreSame(hierarchy.Background, hierarchy.Root.GetLayer(UILayer.Background));
            Assert.AreSame(hierarchy.Normal, hierarchy.Root.GetLayer(UILayer.Normal));
            Assert.AreSame(hierarchy.Fixed, hierarchy.Root.GetLayer(UILayer.Fixed));
            Assert.AreSame(hierarchy.Popup, hierarchy.Root.GetLayer(UILayer.Popup));
            Assert.AreSame(hierarchy.Guide, hierarchy.Root.GetLayer(UILayer.Guide));
            Assert.AreSame(hierarchy.Top, hierarchy.Root.GetLayer(UILayer.Top));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => hierarchy.Root.GetLayer((UILayer)999));
        }

        [Test]
        public void Configure_ResetsCanvasScale_AndAppliesUILayerRecursively()
        {
            UIRootHierarchy hierarchy = CreateHierarchy();
            GameObject nestedObject = new GameObject("Nested", typeof(RectTransform));
            nestedObject.transform.SetParent(hierarchy.Normal, false);
            hierarchy.Canvas.transform.localScale = new Vector3(2f, 3f, 4f);
            hierarchy.Canvas.gameObject.layer = 0;
            hierarchy.Normal.gameObject.layer = 0;
            nestedObject.layer = 0;

            Configure(hierarchy);

            int uiLayer = LayerMask.NameToLayer("UI");
            Assert.GreaterOrEqual(uiLayer, 0, "当前项目没有名为 UI 的 Unity Layer。");
            Assert.AreEqual(Vector3.one, hierarchy.Canvas.transform.localScale);
            Assert.AreEqual(uiLayer, hierarchy.Canvas.gameObject.layer);
            Assert.AreEqual(uiLayer, hierarchy.Normal.gameObject.layer);
            Assert.AreEqual(uiLayer, nestedObject.layer);
        }

        [Test]
        public void Awake_SetsInstance_CorrectsViewParent_AndReappliesCanvasDefaults()
        {
            UIRootHierarchy hierarchy = CreateHierarchy();
            GameObject viewObject = new GameObject("PopupView", typeof(RectTransform));
            viewObject.transform.SetParent(hierarchy.Normal, false);
            UIRootViewProbe view = viewObject.AddComponent<UIRootViewProbe>();
            view.Layer = UILayer.Popup;
            Configure(hierarchy);
            viewObject.transform.SetParent(hierarchy.Normal, false);
            hierarchy.Canvas.transform.localScale = new Vector3(2f, 2f, 2f);
            hierarchy.Canvas.gameObject.layer = 0;
            viewObject.layer = 0;

            InvokeAwake(hierarchy.Root);

            int uiLayer = LayerMask.NameToLayer("UI");
            Assert.GreaterOrEqual(uiLayer, 0, "当前项目没有名为 UI 的 Unity Layer。");
            Assert.AreSame(hierarchy.Root, UIRoot.Instance);
            Assert.AreSame(hierarchy.Popup, view.transform.parent);
            Assert.AreEqual(Vector3.one, hierarchy.Canvas.transform.localScale);
            Assert.AreEqual(uiLayer, hierarchy.Canvas.gameObject.layer);
            Assert.AreEqual(uiLayer, viewObject.layer);
        }

        private UIRootHierarchy CreateHierarchy()
        {
            rootObject = new GameObject("UIRoot", typeof(RectTransform));
            rootObject.SetActive(false);
            UIRoot root = rootObject.AddComponent<UIRoot>();
            Assert.IsNotNull(DontDestroyOnLoadField, "找不到 UIRoot.dontDestroyOnLoad 字段。");
            DontDestroyOnLoadField.SetValue(root, false);

            GameObject cameraObject = new GameObject("UICamera", typeof(Camera));
            cameraObject.transform.SetParent(rootObject.transform, false);

            GameObject canvasObject = new GameObject("UICanvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(rootObject.transform, false);

            UIRootHierarchy hierarchy = new UIRootHierarchy
            {
                Root = root,
                Camera = cameraObject.GetComponent<Camera>(),
                Canvas = canvasObject.GetComponent<Canvas>(),
                Background = CreateLayer("Background", canvasObject.transform),
                Normal = CreateLayer("Normal", canvasObject.transform),
                Fixed = CreateLayer("Fixed", canvasObject.transform),
                Popup = CreateLayer("Popup", canvasObject.transform),
                Guide = CreateLayer("Guide", canvasObject.transform),
                Top = CreateLayer("Top", canvasObject.transform)
            };

            return hierarchy;
        }

        private static RectTransform CreateLayer(string name, Transform parent)
        {
            GameObject layerObject = new GameObject(name, typeof(RectTransform));
            layerObject.transform.SetParent(parent, false);
            return layerObject.GetComponent<RectTransform>();
        }

        private static void Configure(UIRootHierarchy hierarchy)
        {
            hierarchy.Root.Configure(
                hierarchy.Camera,
                hierarchy.Canvas,
                hierarchy.Background,
                hierarchy.Normal,
                hierarchy.Fixed,
                hierarchy.Popup,
                hierarchy.Guide,
                hierarchy.Top);
        }

        private static void InvokeAwake(UIRoot root)
        {
            Assert.IsNotNull(AwakeMethod, "找不到 UIRoot.Awake 方法。");
            AwakeMethod.Invoke(root, null);
        }

        private static void SetInstance(UIRoot instance)
        {
            InstanceField.SetValue(null, instance);
        }

        private sealed class UIRootHierarchy
        {
            public UIRoot Root;
            public Camera Camera;
            public Canvas Canvas;
            public RectTransform Background;
            public RectTransform Normal;
            public RectTransform Fixed;
            public RectTransform Popup;
            public RectTransform Guide;
            public RectTransform Top;
        }

    }

    public sealed class UIRootViewProbe : UIView
    {
    }
}
