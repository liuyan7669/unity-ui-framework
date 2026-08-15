using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cowart.UIFramework
{
    /// <summary>
    /// 所有可被 UIManager 管理的 UI 面板基类，提供层级和开关生命周期。
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class UIView : MonoBehaviour
    {
        #region 字段

        /// <summary>面板应该归属的 UI 显示层级。</summary>
        [SerializeField] private UILayer layer = UILayer.Normal;

        /// <summary>面板使用的背景面板。</summary>
        [SerializeField] private GameObject backgroundPanel;

        #endregion

        #region 公开属性

        /// <summary>
        /// 获取或设置面板显示层级；设置新值时会立即校正父节点。
        /// </summary>
        public UILayer Layer
        {
            get => layer;
            set
            {
                if (layer == value)
                {
                    return;
                }

                layer = value;
                CorrectLayer();
            }
        }

        /// <summary>设置背景面板是否显示。</summary>
        /// <param name="isActive">是否显示背景面板。</param>
        public void SetBackgroundPanelActive(bool isActive)
        {
            if (backgroundPanel != null)
            {
                backgroundPanel.SetActive(isActive);
            }
        }

        #endregion

        #region Unity 生命周期

#if UNITY_EDITOR
        /// <summary>Inspector 数据变化后延迟校正面板层级，避免在 OnValidate 中直接修改 Transform。</summary>
        private void OnValidate()
        {
            EditorApplication.delayCall -= CorrectLayer;
            EditorApplication.delayCall += CorrectLayer;
        }
#endif

        #endregion

        #region 内部方法

        /// <summary>
        /// 接收 UIManager 的打开请求，并统一交给面板状态处理入口。
        /// </summary>
        /// <remarks>
        /// 该方法仅供同程序集内的 UIManager 调用。面板会先激活；只有面板及其全部父级实际激活时，
        /// 才会置于当前层级最前端并触发 <see cref="OnOpen"/>。重复打开已激活面板仍会触发 OnOpen，
        /// 以便使用新的 args 刷新面板内容。
        /// </remarks>
        /// <param name="args">传递给面板的打开参数。</param>
        internal void Open(UIArgs args)
        {
            SetOpenState(true, args);
        }

        /// <summary>接收 UIManager 的关闭请求，并统一交给面板状态处理入口。</summary>
        /// <remarks>
        /// 该方法仅供同程序集内的 UIManager 调用。面板自身已经关闭时不会重复执行；面板实际显示时，
        /// 会先触发 <see cref="OnClose"/>，再关闭 GameObject。父级已经关闭时不会重复触发 OnClose，
        /// 但仍会关闭面板自身。
        /// </remarks>
        internal void Close()
        {
            SetOpenState(false, null);
        }

        #endregion

        #region 子类生命周期方法

        /// <summary>
        /// 当 <see cref="UIManager.Open{T}(UIArgs)"/> 或 Prefab 重载请求打开面板时调用。
        /// 框架会先执行 gameObject.SetActive(true)；只有面板及其全部父级均已激活，
        /// 即 gameObject.activeInHierarchy 为 true 时，才会调用此方法。
        /// 已经打开的面板再次调用 Open 时仍会重复触发，可用于刷新面板内容；
        /// 任意父级未激活时不会触发。
        /// </summary>
        /// <param name="args">打开面板时传入的数据。</param>
        protected virtual void OnOpen(UIArgs args)
        {
        }

        /// <summary>
        /// 当 <see cref="UIManager.Close{T}"/>、CloseAll 或 Delete 请求关闭面板时调用。
        /// 只有面板自身处于激活状态，并且全部父级也已激活，
        /// 即 gameObject.activeInHierarchy 为 true 时，才会在 gameObject.SetActive(false) 之前调用此方法。
        /// 面板自身已经关闭时不会重复触发；任意父级已经关闭时也不会触发，
        /// 但框架仍会将面板自身设置为未激活，防止父级恢复后面板重新显示。
        /// </summary>
        protected virtual void OnClose()
        {
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 统一处理面板唯一的 SetActive 调用，以及打开、关闭生命周期的触发顺序。
        /// </summary>
        /// <remarks>
        /// 打开顺序：激活 GameObject，确认 activeInHierarchy，调整同级顺序，触发 OnOpen。
        /// 关闭顺序：确认面板实际显示，触发 OnClose，关闭 GameObject。
        /// </remarks>
        /// <param name="isOpen">是否打开面板。</param>
        /// <param name="args">打开面板时传入的数据。</param>
        private void SetOpenState(bool isOpen, UIArgs args)
        {
            if (!isOpen && !gameObject.activeSelf)
            {
                return;
            }

            if (!isOpen && gameObject.activeInHierarchy)
            {
                OnClose();
            }

            gameObject.SetActive(isOpen);

            if (isOpen && gameObject.activeInHierarchy)
            {
                transform.SetAsLastSibling();
                OnOpen(args);
            }
        }

        /// <summary>根据 Layer 属性自动将面板移动到对应的 UI 层级。</summary>
        private void CorrectLayer()
        {
            if (this == null)
            {
                return;
            }

            UIRoot[] roots = GetComponentsInParent<UIRoot>(true);
            UIRoot root = roots.Length > 0 ? roots[0] : null;
            if (root == null)
            {
                return;
            }

            RectTransform targetLayer = root.GetLayer(layer);
            if (targetLayer == null || transform.parent == targetLayer)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Vector3 localPosition = transform.localPosition;
                Quaternion localRotation = transform.localRotation;
                Vector3 localScale = transform.localScale;
                RectTransform rectTransform = transform as RectTransform;
                Vector2 anchorMin = rectTransform != null ? rectTransform.anchorMin : default;
                Vector2 anchorMax = rectTransform != null ? rectTransform.anchorMax : default;
                Vector3 anchoredPosition = rectTransform != null ? rectTransform.anchoredPosition3D : default;
                Vector2 sizeDelta = rectTransform != null ? rectTransform.sizeDelta : default;
                Vector2 pivot = rectTransform != null ? rectTransform.pivot : default;

                Undo.SetTransformParent(transform, targetLayer, "Change UI Layer");
                Undo.RecordObject(transform, "Change UI Layer");

                transform.localPosition = localPosition;
                transform.localRotation = localRotation;
                transform.localScale = localScale;
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = anchorMin;
                    rectTransform.anchorMax = anchorMax;
                    rectTransform.anchoredPosition3D = anchoredPosition;
                    rectTransform.sizeDelta = sizeDelta;
                    rectTransform.pivot = pivot;
                }

                return;
            }
#endif

            transform.SetParent(targetLayer, false);
        }

        #endregion
    }

}
