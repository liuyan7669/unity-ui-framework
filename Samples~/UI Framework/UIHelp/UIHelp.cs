using System;
using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using UnityEngine;
using UnityEngine.UI;

public class UIHelp : UIView
{
    public class UIArg : UIArgs
    {
        public bool backgroundActive;
        public Action action;
    }
    public Button okButton;

    protected override void OnOpen(UIArgs args)
    {
        UIArg arg = args.As<UIArg>();
        SetBackgroundPanelActive(arg.backgroundActive);
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(() =>
        {
            arg.action?.Invoke();
        });
    }
}
