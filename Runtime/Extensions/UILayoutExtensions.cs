using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 提供 UI 组件的 RectTransform 获取、Image 透明度设置和布局重建扩展。
/// </summary>
public static class UILayoutExtensions
{
    /// <summary>
    /// 获取 UI 组件所在对象的 RectTransform。
    /// </summary>
    /// <param name="component">目标 UI 组件。</param>
    /// <returns>目标使用 RectTransform 时返回该组件；目标为空或不是 UI 对象时返回 null。</returns>
    public static RectTransform ExtGetRect(this Component component)
    {
        return component != null ? component.transform as RectTransform : null;
    }

    /// <summary>
    /// 设置 Image 颜色的透明度，并保留原有 RGB。
    /// </summary>
    /// <param name="image">目标 Image。</param>
    /// <param name="alpha">透明度，自动限制在 0 到 1。</param>
    public static void ExtSetAlpha(this Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    /// <summary>设置 RectTransform 的 anchoredPosition.x，并保留 y。</summary>
    /// <param name="target">目标 RectTransform。</param>
    /// <param name="value">要设置的 x 坐标。</param>
    public static void ExtSetAnchoredPositionX(this RectTransform target, float value)
    {
        if (target == null)
        {
            return;
        }

        Vector2 position = target.anchoredPosition;
        position.x = value;
        target.anchoredPosition = position;
    }

    /// <summary>设置 RectTransform 的 anchoredPosition.y，并保留 x。</summary>
    /// <param name="target">目标 RectTransform。</param>
    /// <param name="value">要设置的 y 坐标。</param>
    public static void ExtSetAnchoredPositionY(this RectTransform target, float value)
    {
        if (target == null)
        {
            return;
        }

        Vector2 position = target.anchoredPosition;
        position.y = value;
        target.anchoredPosition = position;
    }

    /// <summary>设置 RectTransform 的 sizeDelta.x，并保留 y。</summary>
    /// <param name="target">目标 RectTransform。</param>
    /// <param name="value">要设置的宽度差值。</param>
    public static void ExtSetSizeDeltaX(this RectTransform target, float value)
    {
        if (target == null)
        {
            return;
        }

        Vector2 size = target.sizeDelta;
        size.x = value;
        target.sizeDelta = size;
    }

    /// <summary>设置 RectTransform 的 sizeDelta.y，并保留 x。</summary>
    /// <param name="target">目标 RectTransform。</param>
    /// <param name="value">要设置的高度差值。</param>
    public static void ExtSetSizeDeltaY(this RectTransform target, float value)
    {
        if (target == null)
        {
            return;
        }

        Vector2 size = target.sizeDelta;
        size.y = value;
        target.sizeDelta = size;
    }

    /// <summary>一次设置 CanvasGroup 的透明度、交互和射线拦截状态。</summary>
    public static void ExtSetCanvasGroupState(
        this CanvasGroup target,
        float alpha,
        bool interactable,
        bool blocksRaycasts)
    {
        if (target == null)
        {
            return;
        }

        target.alpha = Mathf.Clamp01(alpha);
        target.interactable = interactable;
        target.blocksRaycasts = blocksRaycasts;
    }

    /// <summary>修改 Pivot，并补偿世界坐标以保持 UI 矩形的视觉位置不变。</summary>
    public static void ExtSetPivotKeepPosition(this RectTransform target, Vector2 pivot)
    {
        if (target == null)
        {
            return;
        }

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 originalBottomLeft = corners[0];

        target.pivot = pivot;
        target.GetWorldCorners(corners);
        target.position += originalBottomLeft - corners[0];
    }

    /// <summary>设置 UGUI Text 文本，并可根据内容是否为空控制物体显隐。</summary>
    public static void ExtSetText(
        this Text target,
        string value,
        bool hideWhenEmpty = false)
    {
        if (target == null)
        {
            return;
        }

        target.text = value ?? string.Empty;
        if (hideWhenEmpty)
        {
            target.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }
    }

    /// <summary>设置 TMP_Text 文本，并可根据内容是否为空控制物体显隐。</summary>
    public static void ExtSetText(
        this TMP_Text target,
        string value,
        bool hideWhenEmpty = false)
    {
        if (target == null)
        {
            return;
        }

        target.text = value ?? string.Empty;
        if (hideWhenEmpty)
        {
            target.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }
    }

    /// <summary>关闭 Selectable 的键盘和手柄导航。</summary>
    public static void ExtSetNavigationNone(this Selectable target)
    {
        if (target == null)
        {
            return;
        }

        Navigation navigation = target.navigation;
        navigation.mode = Navigation.Mode.None;
        target.navigation = navigation;
    }

    /// <summary>
    /// 按照先子节点、后父节点的顺序立即重建整个布局树。
    /// </summary>
    /// <param name="root">要重建的布局树根节点。</param>
    public static void ExtRebuildLayoutTree(this RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        RebuildLayoutTree(root);
    }

    /// <summary>
    /// 在下一帧重建目标自身或其最近的布局控制节点。
    /// </summary>
    /// <param name="target">需要刷新布局的目标。</param>
    /// <param name="coroutineRunner">用于启动协程的组件；为空时从目标父级自动查找。</param>
    /// <remarks>目标需要保持有效至下一帧布局重建完成。</remarks>
    public static void ExtRebuildNearestLayoutNextFrame(
        this RectTransform target,
        MonoBehaviour coroutineRunner = null)
    {
        if (target == null)
        {
            return;
        }

        if (coroutineRunner == null)
        {
            coroutineRunner = target.GetComponentInParent<MonoBehaviour>();
        }

        if (coroutineRunner == null)
        {
            Debug.LogWarning("下一帧重建布局需要一个 MonoBehaviour 来运行协程。", target);
            return;
        }

        coroutineRunner.StartCoroutine(RebuildNearestLayoutNextFrame(target));
    }

    /// <summary>
    /// 递归重建布局树，确保子节点先于父节点更新。
    /// </summary>
    private static void RebuildLayoutTree(RectTransform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform child = root.GetChild(i) as RectTransform;
            if (child != null)
            {
                RebuildLayoutTree(child);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }

    /// <summary>
    /// 等待一帧后重建最近的布局控制节点。
    /// </summary>
    private static IEnumerator RebuildNearestLayoutNextFrame(RectTransform target)
    {
        yield return null;
        RebuildNearestLayout(target);
    }

    /// <summary>
    /// 查找目标自身或最近的 LayoutGroup、ContentSizeFitter，并立即重建布局。
    /// </summary>
    private static void RebuildNearestLayout(RectTransform target)
    {
        RectTransform layoutRoot = target;
        while (layoutRoot.parent != null && layoutRoot.parent is RectTransform)
        {
            if (layoutRoot.GetComponent<LayoutGroup>() != null ||
                layoutRoot.GetComponent<ContentSizeFitter>() != null)
            {
                break;
            }

            layoutRoot = layoutRoot.parent as RectTransform;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
    }
}
