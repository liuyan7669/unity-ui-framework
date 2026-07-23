using System;
using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using UnityEngine;
using UnityEngine.UI;

public class UIHome : UIView
{
    public class Arg : UIArgs
    {
        public Action startAction;
    }

    public Button startButton;

    protected override void OnOpen(UIArgs args)
    {
        Arg arg = args.As<Arg>();
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() =>
        {
            arg.startAction?.Invoke();
        });
    }

}
