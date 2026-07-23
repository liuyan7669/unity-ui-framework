using System;
using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRBButton : UIView
{
    public class UIArg : UIArgs
    {
        public string buttonText;
        public Action action;
    }
    public Button okButton;
    public TMP_Text buttonText;

    protected override void OnOpen(UIArgs args)
    {
        UIArg arg = args.As<UIArg>();
        if (string.IsNullOrEmpty(arg.buttonText))
        {
            buttonText.text = "确定";
        }
        else
        {
            buttonText.text = arg.buttonText;
        }
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(() =>
        {
            arg.action?.Invoke();
        });
    }

}
