using System.Collections;
using System.Collections.Generic;
using Cowart.UIFramework;
using TMPro;
using UnityEngine;

public class UITopTip : UIView
{
    public class Arg : UIArgs
    {
        public string text;

        /// <summary>
        /// 要播放的 Resources 音频路径，不包含文件扩展名。
        /// 为空时不播放音频。
        /// </summary>
        public string audioPath;
    }

    public TMP_Text text;

    protected override void OnOpen(UIArgs args)
    {
        Arg arg = args.As<Arg>();

        text.text = arg.text;
        Debug.Log("<color=white>应该同时播放的配音：" + arg.audioPath + "</color>");
    }

}
