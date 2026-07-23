using System;
using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using UnityEngine;
using UnityEngine.UI;

public class UIMenu : UIView
{
    public enum ButtonStyle
    {
        /// <summary>
        /// 无样式。
        /// </summary>
        None,
        /// <summary>
        /// 未开始样式。
        /// </summary>
        NotStarted,
        /// <summary>
        /// 进行中样式。
        /// </summary>
        InProgress,
        /// <summary>
        /// 已完成样式。
        /// </summary>
        Completed
    }

    public class Arg : UIArgs
    {
        public Action<int> selectedAction;
    }

    public List<Button> buttons = new List<Button>();

    protected override void OnOpen(UIArgs args)
    {
        Arg arg = args.As<Arg>();
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            SetStyle(index, ButtonStyle.None);
            Button button = buttons[index];
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                arg.selectedAction?.Invoke(index);
            });
        }
    }

    public void SetStyle(int index, ButtonStyle style)
    {
        Transform button = buttons[index].transform;
        button.Find(nameof(ButtonStyle.NotStarted)).gameObject.SetActive(style == ButtonStyle.NotStarted);
        button.Find(nameof(ButtonStyle.InProgress)).gameObject.SetActive(style == ButtonStyle.InProgress);
        button.Find(nameof(ButtonStyle.Completed)).gameObject.SetActive(style == ButtonStyle.Completed);
    }

}
