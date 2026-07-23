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
        UIArg arg = args.As<UIArg>();

        okButton.ExtSetActive(arg.okActive);
        noButton.ExtSetActive(arg.noActive);
        closeButton.ExtSetActive(arg.closeActive);

        okButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        if (arg == null)
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
