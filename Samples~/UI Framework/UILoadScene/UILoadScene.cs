using System;
using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILoadScene : UIView
{
    private const float SceneReadyProgress = 0.9f;

    /// <summary>
    /// 打开加载界面时传入的参数。
    /// </summary>
    public class UIArg : UIArgs
    {
        /// <summary>
        /// 要加载的场景名称。填写后优先使用此字段加载，场景需要加入 Build Settings。
        /// </summary>
        public string sceneName;

        /// <summary>
        /// 要加载的场景索引。当 sceneName 为空时使用此字段，-1 表示不通过索引加载。
        /// </summary>
        public int sceneIndex = -1;

        /// <summary>
        /// 加载过程中显示在界面文本上的内容。
        /// </summary>
        public string loadText = "Loading...";

        /// <summary>
        /// 加载进度刷新回调。参数范围为 0 到 1，加载完成时会再回调一次 1。
        /// </summary>
        public Action<float> progressAction;

        /// <summary>
        /// 场景加载完成后的回调，会在进度刷新为 1 之后触发。
        /// </summary>
        public Action completeAction;
    }

    public Slider slider;
    public TMP_Text text;

    private Coroutine loadCoroutine;
    private AsyncOperation loadOperation;
    private ThreadPriority previousLoadingPriority;
    private bool loadingPriorityChanged;

    protected override void OnOpen(UIArgs args)
    {
        UIArg arg = args.As<UIArg>();

        StopLoading();

        RefreshProgress(0, arg);

        if (arg == null)
        {
            return;
        }

        loadCoroutine = StartCoroutine(LoadScene(arg));
    }

    protected override void OnClose()
    {
        StopLoading();
    }

    private IEnumerator LoadScene(UIArg arg)
    {
        AsyncOperation operation = null;
        SetLoadingPriority();

        try
        {
            if (!string.IsNullOrEmpty(arg.sceneName))
            {
                operation = SceneManager.LoadSceneAsync(arg.sceneName);
            }
            else if (arg.sceneIndex >= 0)
            {
                operation = SceneManager.LoadSceneAsync(arg.sceneIndex);
            }

            if (operation == null)
            {
                yield break;
            }

            loadOperation = operation;
            operation.allowSceneActivation = false;

            while (operation.progress < SceneReadyProgress)
            {
                float progress = operation.progress;
                RefreshProgressDisplay(progress / SceneReadyProgress);
                arg.progressAction?.Invoke(progress);
                yield return null;
            }

            // 先让 100% 至少绘制一帧，再进入只能在主线程执行的场景激活阶段。
            RefreshProgressDisplay(1);
            arg.progressAction?.Invoke(SceneReadyProgress);
            yield return null;

            operation.allowSceneActivation = true;
            while (!operation.isDone)
            {
                yield return null;
            }
        }
        finally
        {
            if (operation != null && !operation.isDone)
            {
                operation.allowSceneActivation = true;
            }

            if (operation == null || loadOperation == operation)
            {
                loadOperation = null;
                RestoreLoadingPriority();
                loadCoroutine = null;
            }
        }

        RefreshProgress(1, arg);
        arg.completeAction?.Invoke();
    }

    private void RefreshProgress(float progress, UIArg arg)
    {
        progress = Mathf.Clamp01(progress);
        RefreshProgressDisplay(progress);
        arg?.progressAction?.Invoke(progress);
    }

    private void RefreshProgressDisplay(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (slider != null)
        {
            slider.value = progress;
        }

        if (text != null)
        {
            // text.text = arg == null ? string.Empty : arg.loadText;
            text.text = Mathf.RoundToInt(progress * 100) + "%";
        }
    }

    private void SetLoadingPriority()
    {
        if (loadingPriorityChanged)
        {
            return;
        }

        previousLoadingPriority = Application.backgroundLoadingPriority;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        loadingPriorityChanged = true;
    }

    private void RestoreLoadingPriority()
    {
        if (!loadingPriorityChanged)
        {
            return;
        }

        Application.backgroundLoadingPriority = previousLoadingPriority;
        loadingPriorityChanged = false;
    }

    private void StopLoading()
    {
        if (loadOperation != null && !loadOperation.isDone)
        {
            loadOperation.allowSceneActivation = true;
        }

        if (loadCoroutine != null)
        {
            Coroutine coroutine = loadCoroutine;
            loadCoroutine = null;
            StopCoroutine(coroutine);
        }

        loadOperation = null;
        RestoreLoadingPriority();
    }
}
