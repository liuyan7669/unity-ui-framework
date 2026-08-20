using System;
using UnityEngine;

namespace Cowart.UIFramework
{
    /// <summary>
    /// 保存 UGUI 根节点、相机、Canvas 及各显示层级，并负责运行时层级校正。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-32000)]
    public sealed class UIRoot : MonoBehaviour
    {
        /// <summary>是否在切换场景时保留整个 UI 根节点。</summary>
        [SerializeField] private bool dontDestroyOnLoad = true;

        /// <summary>用于渲染 UGUI 的专用相机。</summary>
        [SerializeField] private Camera uiCamera;

        /// <summary>承载所有 UI 层级的主 Canvas。</summary>
        [SerializeField] private Canvas uiCanvas;

        /// <summary>背景显示层。</summary>
        [SerializeField] private RectTransform backgroundLayer;

        /// <summary>普通面板显示层。</summary>
        [SerializeField] private RectTransform normalLayer;

        /// <summary>固定界面显示层。</summary>
        [SerializeField] private RectTransform fixedLayer;

        /// <summary>弹窗显示层。</summary>
        [SerializeField] private RectTransform popupLayer;

        /// <summary>引导界面显示层。</summary>
        [SerializeField] private RectTransform guideLayer;

        /// <summary>最高优先级显示层。</summary>
        [SerializeField] private RectTransform topLayer;

        /// <summary>获取当前生效的 UI 根节点实例。</summary>
        public static UIRoot Instance { get; private set; }

        /// <summary>获取 UGUI 专用相机。</summary>
        public Camera UICamera => uiCamera;

        /// <summary>获取主 UI Canvas。</summary>
        public Canvas UICanvas => uiCanvas;

        /// <summary>初始化单例、校正面板层级并应用 Canvas 默认设置。</summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CorrectViewLayers();
            ApplyUILayer();
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>在根节点销毁时清理单例引用。</summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 获取指定 UI 层级对应的父节点。
        /// </summary>
        /// <param name="layer">目标 UI 层级。</param>
        /// <returns>层级对应的 RectTransform。</returns>
        public RectTransform GetLayer(UILayer layer)
        {
            switch (layer)
            {
                case UILayer.Background:
                    return backgroundLayer;
                case UILayer.Normal:
                    return normalLayer;
                case UILayer.Fixed:
                    return fixedLayer;
                case UILayer.Popup:
                    return popupLayer;
                case UILayer.Guide:
                    return guideLayer;
                case UILayer.Top:
                    return topLayer;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
            }
        }

        /// <summary>
        /// 配置 UI 根节点所需的相机、Canvas 和显示层引用。
        /// </summary>
        /// <param name="camera">UGUI 专用相机。</param>
        /// <param name="canvas">主 UI Canvas。</param>
        /// <param name="background">背景层节点。</param>
        /// <param name="normal">普通层节点。</param>
        /// <param name="fixedUI">固定层节点。</param>
        /// <param name="popup">弹窗层节点。</param>
        /// <param name="guide">引导层节点。</param>
        /// <param name="top">顶层节点。</param>
        public void Configure(
            Camera camera,
            Canvas canvas,
            RectTransform background,
            RectTransform normal,
            RectTransform fixedUI,
            RectTransform popup,
            RectTransform guide,
            RectTransform top)
        {
            uiCamera = camera;
            uiCanvas = canvas;
            backgroundLayer = background;
            normalLayer = normal;
            fixedLayer = fixedUI;
            popupLayer = popup;
            guideLayer = guide;
            topLayer = top;
            ApplyUILayer();
        }

#if UNITY_EDITOR
        /// <summary>在 Inspector 数据变化时重新应用 Canvas 默认设置。</summary>
        private void OnValidate()
        {
            ApplyUILayer();
        }
#endif

        /// <summary>恢复 Canvas 缩放，并将 Canvas 子树统一设置为 UI Layer。</summary>
        private void ApplyUILayer()
        {
            if (uiCanvas == null)
            {
                return;
            }

            uiCanvas.transform.localScale = Vector3.one;

            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer < 0)
            {
                return;
            }

            SetLayerRecursively(uiCanvas.transform, uiLayer);
        }

        /// <summary>按照每个面板声明的 UILayer 校正场景中预置面板的父节点。</summary>
        private void CorrectViewLayers()
        {
            if (uiCanvas == null)
            {
                return;
            }

            UIView[] views = uiCanvas.GetComponentsInChildren<UIView>(true);
            foreach (UIView view in views)
            {
                RectTransform targetLayer = GetLayer(view.Layer);
                if (targetLayer != null && view.transform.parent != targetLayer)
                {
                    view.transform.SetParent(targetLayer, false);
                }
            }
        }

        /// <summary>
        /// 递归设置指定节点及其全部子节点的 Unity Layer。
        /// </summary>
        /// <param name="target">需要设置的根节点。</param>
        /// <param name="layer">目标 Unity Layer 编号。</param>
        private static void SetLayerRecursively(Transform target, int layer)
        {
            target.gameObject.layer = layer;
            for (int i = 0; i < target.childCount; i++)
            {
                SetLayerRecursively(target.GetChild(i), layer);
            }
        }
    }
}
