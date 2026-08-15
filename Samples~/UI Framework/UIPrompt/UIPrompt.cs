using System;
using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPrompt : UIView
{
    public class UIArg : UIArgs
    {
        public string text;

        /// <summary>
        /// 要播放的 Resources 音频路径，不包含文件扩展名。
        /// 为空时不播放音频。
        /// </summary>
        public string audioPath;
        public Action closeAction;
        public Action okAction;
        public Action noAction;
        public bool okActive;
        public bool noActive;
        public bool closeActive;
    }

    public TMP_Text text;

    public Button okButton;
    public Button noButton;
    public Button closeButton;

    protected override void OnOpen(UIArgs args)
    {
        UIArg arg = args == null ? null : args.As<UIArg>();

        bool hasArgs = arg != null;
        okButton.gameObject.SetActive(hasArgs && arg.okActive);
        noButton.gameObject.SetActive(hasArgs && arg.noActive);
        closeButton.gameObject.SetActive(hasArgs && arg.closeActive);

        okButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        if (!hasArgs)
        {
            text.text = string.Empty;
            return;
        }

        text.text = arg.text;
        // PlayResourceAudio(arg.audioPath);

        okButton.onClick.AddListener(() =>
        {
            arg.okAction?.Invoke();
            UIManager.Close<UIPrompt>();
        });

        noButton.onClick.AddListener(() =>
        {
            arg.noAction?.Invoke();
            UIManager.Close<UIPrompt>();
        });

        closeButton.onClick.AddListener(() =>
        {
            arg.closeAction?.Invoke();
            UIManager.Close<UIPrompt>();
        });
    }

}
