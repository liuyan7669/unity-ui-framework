using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cowart.UIFramework
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIRoot))]
    [DefaultExecutionOrder(-32000)]
    /// <summary>
    /// 负责 UI 面板实例的创建、缓存、查询、打开和关闭。
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        #region 字段

        /// <summary>按面板类型保存已经创建的 UI 实例。</summary>
        private readonly Dictionary<Type, UIView> views = new Dictionary<Type, UIView>();

        /// <summary>当前管理器使用的 UI 根节点。</summary>
        private UIRoot root;

        /// <summary>当前生效的 UI 管理器实例，仅供类内部的静态入口使用。</summary>
        private static UIManager instance;

        #endregion

        #region Unity 生命周期

        /// <summary>初始化单例并获取同物体上的 UI 根节点。</summary>
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                return;
            }

            instance = this;
            root = GetComponent<UIRoot>();
            RegisterSceneViews();
        }

        /// <summary>在管理器销毁时清理单例引用。</summary>
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region 公开静态方法

        /// <summary>
        /// 打开场景启动时已扫描注册，或此前已经创建并保留的指定类型面板。
        /// </summary>
        /// <remarks>
        /// 此重载不会创建新实例。每次调用都会把打开参数传给面板；当面板及其父级均处于激活状态时，
        /// 面板会移动到当前层级的最后一个同级节点，并触发 <see cref="UIView.OnOpen"/>。
        /// 找不到面板时应改用带 Prefab 参数的重载。
        /// </remarks>
        /// <typeparam name="T">面板类型。</typeparam>
        /// <param name="args">传递给面板打开生命周期的数据。</param>
        /// <returns>已经打开的面板实例。</returns>
        /// <exception cref="InvalidOperationException">指定类型的面板尚未注册或创建。</exception>
        public static T Open<T>(UIArgs args = null) where T : UIView
        {
            UIManager manager = GetInstance();
            T view;
            if (!manager.TryGetInternal(out view))
            {
                throw new InvalidOperationException($"UI view {typeof(T).Name} is not registered. Use the prefab overload to create it.");
            }

            view.Open(args);
            return view;
        }

        /// <summary>
        /// 打开指定类型的 UI 面板；不存在已注册实例时使用 Prefab 创建并注册面板。
        /// </summary>
        /// <remarks>
        /// 字典中已有同类型实例时会直接复用该实例，传入的 Prefab 不会再次实例化。
        /// 没有实例时，Prefab 会被实例化到其 <see cref="UIView.Layer"/> 对应的层级下。
        /// 每次调用都会传递 args；当面板及其父级均激活时触发 <see cref="UIView.OnOpen"/>。
        /// </remarks>
        /// <typeparam name="T">面板类型。</typeparam>
        /// <param name="prefab">用于创建面板的 Prefab。</param>
        /// <param name="args">传递给面板打开生命周期的数据。</param>
        /// <returns>已经打开的面板实例。</returns>
        /// <exception cref="ArgumentNullException">prefab 为 null。</exception>
        /// <exception cref="InvalidOperationException">Prefab 指定的 UI 层级未配置。</exception>
        public static T Open<T>(T prefab, UIArgs args = null) where T : UIView
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            UIManager manager = GetInstance();
            UIView cachedView;
            if (!manager.views.TryGetValue(typeof(T), out cachedView) || cachedView == null)
            {
                RectTransform layer = manager.root.GetLayer(prefab.Layer);
                if (layer == null)
                {
                    throw new InvalidOperationException($"UI layer {prefab.Layer} is not configured.");
                }

                cachedView = UnityEngine.Object.Instantiate(prefab, layer, false);
                manager.RegisterView(cachedView);
            }

            cachedView.Open(args);
            return (T)cachedView;
        }

        /// <summary>
        /// 查找指定类型的已注册面板。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        /// <returns>找到时返回面板实例；面板未注册、已删除或实例失效时返回 null。</returns>
        public static T Find<T>() where T : UIView
        {
            T view;
            return GetInstance().TryGetInternal(out view) ? view : null;
        }

        /// <summary>
        /// 根据条件查找第一个匹配的已注册面板。
        /// </summary>
        /// <param name="match">用于判断面板是否匹配的条件。</param>
        /// <returns>第一个匹配的面板；没有匹配项时返回 null。</returns>
        /// <exception cref="ArgumentNullException">match 为 null。</exception>
        public static T Find<T>(Predicate<T> match) where T : UIView
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            foreach (UIView view in GetInstance().views.Values)
            {
                T typedView = view as T;
                if (typedView != null && match(typedView))
                {
                    return typedView;
                }
            }

            return null;
        }

        /// <summary>
        /// 根据条件获取所有匹配的已注册面板实例。
        /// </summary>
        /// <typeparam name="T">需要查找的面板类型。</typeparam>
        /// <param name="match">用于判断面板是否匹配的条件。</param>
        /// <returns>符合条件的已注册面板实例数组。</returns>
        /// <exception cref="ArgumentNullException">match 为 null。</exception>
        public static T[] FindAll<T>(Predicate<T> match) where T : UIView
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            UIManager manager = GetInstance();
            List<T> matchedViews = new List<T>();

            foreach (UIView view in manager.views.Values)
            {
                T typedView = view as T;
                if (typedView == null || !match(typedView))
                {
                    continue;
                }

                matchedViews.Add(typedView);
            }

            return matchedViews.ToArray();
        }

        /// <summary>
        /// 关闭并隐藏指定类型的面板，保留实例以供后续复用。
        /// </summary>
        /// <remarks>
        /// 面板未注册时不执行任何操作。面板自身已关闭时不会重复处理。
        /// 面板在 Hierarchy 中实际激活时，先触发 <see cref="UIView.OnClose"/>，再将面板 GameObject 设为未激活。
        /// 如果父级已经关闭，则不重复触发 OnClose，但仍会关闭面板自身，防止父级恢复后面板意外显示。
        /// 此方法不会从字典移除或销毁面板。
        /// </remarks>
        /// <typeparam name="T">面板类型。</typeparam>
        public static void Close<T>() where T : UIView
        {
            Close(typeof(T));
        }

        /// <summary>
        /// 根据条件关闭并隐藏所有匹配的已注册面板，保留实例以供后续复用。
        /// </summary>
        /// <typeparam name="T">需要筛选的面板类型。</typeparam>
        /// <param name="match">用于判断面板是否需要关闭的条件。</param>
        /// <exception cref="ArgumentNullException">match 为 null。</exception>
        public static void Close<T>(Predicate<T> match) where T : UIView
        {
            T[] matchedViews = FindAll(match);
            foreach (T view in matchedViews)
            {
                view.Close();
            }
        }

        /// <summary>
        /// 一次关闭并隐藏多个指定类型的面板，保留实例以供后续复用。
        /// </summary>
        /// <remarks>
        /// 重复类型只处理一次；null、未注册类型和已经关闭的面板会被忽略。
        /// 调用示例：Close(typeof(PanelA), typeof(PanelB))。
        /// </remarks>
        /// <param name="viewTypes">需要关闭的面板类型集合。</param>
        /// <exception cref="ArgumentException">传入的类型没有继承 UIView。</exception>
        public static void Close(params Type[] viewTypes)
        {
            if (viewTypes == null || viewTypes.Length == 0)
            {
                return;
            }

            UIManager manager = GetInstance();
            HashSet<Type> uniqueTypes = new HashSet<Type>(viewTypes);
            foreach (Type viewType in uniqueTypes)
            {
                if (viewType == null)
                {
                    continue;
                }

                if (!typeof(UIView).IsAssignableFrom(viewType))
                {
                    throw new ArgumentException($"Type {viewType.Name} does not inherit UIView.", nameof(viewTypes));
                }

                UIView view;
                if (manager.views.TryGetValue(viewType, out view) && view != null)
                {
                    view.Close();
                }
            }
        }

        /// <summary>
        /// 关闭并隐藏所有已注册面板，可按面板类型忽略不需要关闭的面板。
        /// </summary>
        /// <remarks>
        /// 此方法只执行关闭流程，不会从字典移除或销毁面板。未激活面板会由 UIView 自行忽略。
        /// ignoredTypes 使用精确类型匹配，例如 CloseAll(typeof(HUDPanel), typeof(LoadingPanel))。
        /// </remarks>
        /// <param name="ignoredTypes">需要保持当前状态、不执行关闭的面板类型。</param>
        public static void CloseAll(params Type[] ignoredTypes)
        {
            HashSet<Type> ignoredTypeSet = ignoredTypes == null ? new HashSet<Type>() : new HashSet<Type>(ignoredTypes);
            UIView[] viewsToClose = FindAll<UIView>(view => !ignoredTypeSet.Contains(view.GetType()));
            Type[] viewTypes = new Type[viewsToClose.Length];
            for (int i = 0; i < viewsToClose.Length; i++)
            {
                viewTypes[i] = viewsToClose[i].GetType();
            }

            Close(viewTypes);
        }

        /// <summary>
        /// 从管理器中移除并销毁指定类型的面板实例。
        /// </summary>
        /// <remarks>
        /// 面板未注册时不执行任何操作。销毁前会先执行与 <see cref="Close{T}"/> 相同的关闭流程，
        /// 随后从字典移除，并在当前帧结束时销毁面板 GameObject。销毁后再次打开该类型时，
        /// 必须使用带 Prefab 参数的 Open 重载重新创建实例。
        /// </remarks>
        /// <typeparam name="T">面板类型。</typeparam>
        public static void Delete<T>() where T : UIView
        {
            Delete(typeof(T));
        }

        /// <summary>
        /// 根据条件删除所有匹配的已注册面板实例。
        /// </summary>
        /// <typeparam name="T">需要筛选的面板类型。</typeparam>
        /// <param name="match">用于判断面板是否需要删除的条件。</param>
        /// <exception cref="ArgumentNullException">match 为 null。</exception>
        public static void Delete<T>(Predicate<T> match) where T : UIView
        {
            UIManager manager = GetInstance();
            T[] matchedViews = FindAll(match);
            foreach (T view in matchedViews)
            {
                view.Close();
                manager.views.Remove(view.GetType());
                UnityEngine.Object.Destroy(view.gameObject);
            }
        }

        /// <summary>
        /// 一次删除多个指定类型的面板实例。
        /// </summary>
        /// <remarks>
        /// 每个已注册面板都会先执行关闭流程，再从字典移除并在当前帧结束时销毁。
        /// 重复类型只处理一次；未注册类型和 null 会被忽略。
        /// 调用示例：Delete(typeof(PanelA), typeof(PanelB))。
        /// </remarks>
        /// <param name="viewTypes">需要删除的面板类型集合。</param>
        /// <exception cref="ArgumentException">传入的类型没有继承 UIView。</exception>
        public static void Delete(params Type[] viewTypes)
        {
            if (viewTypes == null || viewTypes.Length == 0)
            {
                return;
            }

            UIManager manager = GetInstance();
            HashSet<Type> uniqueTypes = new HashSet<Type>(viewTypes);
            foreach (Type viewType in uniqueTypes)
            {
                if (viewType == null)
                {
                    continue;
                }

                if (!typeof(UIView).IsAssignableFrom(viewType))
                {
                    throw new ArgumentException($"Type {viewType.Name} does not inherit UIView.", nameof(viewTypes));
                }

                UIView view;
                if (!manager.views.TryGetValue(viewType, out view))
                {
                    continue;
                }

                if (view != null)
                {
                    view.Close();
                }

                manager.views.Remove(viewType);
                if (view != null)
                {
                    UnityEngine.Object.Destroy(view.gameObject);
                }
            }
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 尝试从当前管理器字典中获取指定类型的面板实例。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        /// <param name="view">找到的面板实例。</param>
        /// <returns>找到有效实例时返回 true，否则返回 false。</returns>
        private bool TryGetInternal<T>(out T view) where T : UIView
        {
            UIView cachedView;
            if (views.TryGetValue(typeof(T), out cachedView) && cachedView != null)
            {
                view = (T)cachedView;
                return true;
            }

            view = null;
            return false;
        }

        /// <summary>获取内部单例；框架尚未初始化时抛出明确异常。</summary>
        /// <returns>当前生效的 UI 管理器实例。</returns>
        /// <exception cref="InvalidOperationException">场景中不存在已完成 Awake 初始化的 UIManager。</exception>
        private static UIManager GetInstance()
        {
            if (instance == null)
            {
                throw new InvalidOperationException("UIManager is not initialized. Ensure an active UIRoot exists before using the UI API.");
            }

            return instance;
        }

        /// <summary>扫描并注册主 Canvas 下包含未激活对象在内的所有场景面板。</summary>
        private void RegisterSceneViews()
        {
            if (root.UICanvas == null)
            {
                return;
            }

            UIView[] sceneViews = root.UICanvas.GetComponentsInChildren<UIView>(true);
            foreach (UIView view in sceneViews)
            {
                RegisterView(view);
            }
        }

        /// <summary>
        /// 注册一个面板实例。
        /// </summary>
        /// <param name="view">需要注册的面板实例。</param>
        private void RegisterView(UIView view)
        {
            Type viewType = view.GetType();
            UIView registeredView;
            if (views.TryGetValue(viewType, out registeredView) && registeredView != null && registeredView != view)
            {
                Debug.LogWarning($"UI view {viewType.Name} is already registered. The duplicate view {view.name} was ignored.", view);
                return;
            }

            views[viewType] = view;
        }

        #endregion
    }
}
