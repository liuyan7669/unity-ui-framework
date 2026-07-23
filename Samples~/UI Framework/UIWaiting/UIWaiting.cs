using System;
using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using TMPro;
using UnityEngine;

/// <summary>
/// 等待提示界面。
/// 支持显示提示文本、按指定秒数自动关闭，以及由外部手动关闭。
/// </summary>
public class UIWaiting : UIView
{
    /// <summary>
    /// 等待界面刷新参数。
    /// </summary>
    public class UIArg : UIArgs
    {
        /// <summary>
        /// 等待界面显示的提示文本。
        /// </summary>
        public string text;

        /// <summary>
        /// 自动关闭等待时间，单位秒。
        /// 大于 0 时延迟关闭；等于 0 或小于 0 时不自动关闭，需要手动调用 Close。
        /// </summary>
        public float duration;

        /// <summary>
        /// 关闭等待界面时触发的回调。
        /// 自动关闭和手动关闭都会触发。
        /// </summary>
        public Action closeAction;
    }

    /// <summary>
    /// 提示文本组件。
    /// </summary>
    public TMP_Text text;

    /// <summary>
    /// 当前显示参数缓存，用于关闭时触发回调。
    /// </summary>
    private UIArg currentArg;

    /// <summary>
    /// 自动关闭协程引用。
    /// </summary>
    private Coroutine closeCoroutine;

    /// <summary>
    /// 显示或刷新等待界面。
    /// duration 大于 0 时启动自动关闭；duration 小于等于 0 时保持显示等待手动关闭。
    /// </summary>
    /// <param name="args">等待界面刷新参数。</param>
    protected override void OnOpen(UIArgs args)
    {
        UIArg arg = args.As<UIArg>();
        currentArg = arg;

        StopCloseCoroutine();

        if (text != null)
        {
            text.text = arg == null ? string.Empty : arg.text;
        }

        if (arg == null || arg.duration <= 0)
        {
            return;
        }

        closeCoroutine = StartCoroutine(DelayClose(arg.duration));
    }

    /// <summary>
    /// 隐藏等待界面时清理自动关闭协程和参数缓存。
    /// </summary>
    protected override void OnClose()
    {
        StopCloseCoroutine();
        currentArg = null;
    }

    /// <summary>
    /// 手动关闭等待界面。
    /// 如果配置了 closeAction，会先触发回调再关闭界面。
    /// </summary>
    public void Close()
    {
        currentArg?.closeAction?.Invoke();
        UIManager.Close<UIWaiting>();
    }

    /// <summary>
    /// 延迟指定秒数后自动关闭等待界面。
    /// </summary>
    /// <param name="duration">延迟关闭时间，单位秒。</param>
    /// <returns>协程迭代器。</returns>
    private IEnumerator DelayClose(float duration)
    {
        yield return new WaitForSeconds(duration);
        closeCoroutine = null;
        Close();
    }

    /// <summary>
    /// 停止当前自动关闭协程。
    /// </summary>
    private void StopCloseCoroutine()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }
    }
}
