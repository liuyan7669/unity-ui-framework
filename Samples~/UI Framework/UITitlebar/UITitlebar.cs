using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using UnityEngine;
using UnityEngine.UI;

public class UITitlebar : UIView
{
    public class UIArg : UIArgs
    {
        public bool homeActive;
        public bool helpActive;
    }

    public Button homeButton;
    public Button helpButton;
    public Button fullButton;
    public Button windButton;
    public bool IsFullScreen;

    protected override void OnOpen(UIArgs args)
    {
        UIArg arg = args.As<UIArg>();
        if (arg != null)
        {
            homeButton.gameObject.SetActive(arg.homeActive);
            helpButton.gameObject.SetActive(arg.helpActive);
        }
        else
        {
            homeButton.gameObject.SetActive(true);
            helpButton.gameObject.SetActive(true);
        }

        homeButton.onClick.RemoveAllListeners();
        helpButton.onClick.RemoveAllListeners();
        fullButton.onClick.RemoveAllListeners();
        windButton.onClick.RemoveAllListeners();

        RefreshFullStyle();

        homeButton.onClick.AddListener(() =>
        {

        });
        helpButton.onClick.AddListener(() =>
        {
            UIManager.Open<UIHelp>(new UIHelp.UIArg()
            {
                action = () =>
                {
                    UIManager.Close<UIHelp>();
                }
            });
        });
        fullButton.onClick.AddListener(() =>
        {
            IsFullScreen = true;
            RefreshFullStyle();
        });
        windButton.onClick.AddListener(() =>
        {
            IsFullScreen = false;
            RefreshFullStyle();
        });
    }

    private void RefreshFullStyle()
    {
        fullButton.gameObject.SetActive(!IsFullScreen);
        windButton.gameObject.SetActive(IsFullScreen);
    }

}
