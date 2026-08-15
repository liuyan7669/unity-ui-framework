using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 提供 UGUI 和 TextMesh Pro UI 事件的快捷操作扩展。
/// </summary>
public static class UIEventExtensions
{
    /// <summary>
    /// 设置 Button 点击事件。
    /// </summary>
    public static void ExtSetOnClick(
        this Button button,
        Action callback,
        bool removeAllListeners = true)
    {
        if (button == null)
        {
            return;
        }

        SetListener(button.onClick, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 Toggle 值变化事件。
    /// </summary>
    public static void ExtSetOnValueChanged(
        this Toggle toggle,
        Action<bool> callback,
        bool removeAllListeners = true)
    {
        if (toggle == null)
        {
            return;
        }

        SetListener(toggle.onValueChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 Slider 值变化事件。
    /// </summary>
    public static void ExtSetOnValueChanged(
        this Slider slider,
        Action<float> callback,
        bool removeAllListeners = true)
    {
        if (slider == null)
        {
            return;
        }

        SetListener(slider.onValueChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 Scrollbar 值变化事件。
    /// </summary>
    public static void ExtSetOnValueChanged(
        this Scrollbar scrollbar,
        Action<float> callback,
        bool removeAllListeners = true)
    {
        if (scrollbar == null)
        {
            return;
        }

        SetListener(scrollbar.onValueChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 Dropdown 选项变化事件。
    /// </summary>
    public static void ExtSetOnValueChanged(
        this Dropdown dropdown,
        Action<int> callback,
        bool removeAllListeners = true)
    {
        if (dropdown == null)
        {
            return;
        }

        SetListener(dropdown.onValueChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 ScrollRect 滚动位置变化事件。
    /// </summary>
    public static void ExtSetOnValueChanged(
        this ScrollRect scrollRect,
        Action<Vector2> callback,
        bool removeAllListeners = true)
    {
        if (scrollRect == null)
        {
            return;
        }

        SetListener(scrollRect.onValueChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 InputField 文本变化事件。
    /// </summary>
    public static void ExtSetOnValueChanged(
        this InputField inputField,
        Action<string> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onValueChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 InputField 编辑结束事件。
    /// </summary>
    public static void ExtSetOnEndEdit(
        this InputField inputField,
        Action<string> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onEndEdit, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 InputField 提交事件。
    /// </summary>
    /// <remarks>
    /// Unity 2019 和 2020 的 UGUI 没有独立的 onSubmit 事件，因此会使用 onEndEdit 兼容，
    /// 该版本下输入框失去焦点时也会触发回调。
    /// </remarks>
    public static void ExtSetOnSubmit(
        this InputField inputField,
        Action<string> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

#if UNITY_2021_1_OR_NEWER
        SetListener(inputField.onSubmit, callback, removeAllListeners);
#else
        SetListener(inputField.onEndEdit, callback, removeAllListeners);
#endif
    }

    /// <summary>
    /// 设置 MaskableGraphic 裁剪状态变化事件。
    /// </summary>
    public static void ExtSetOnCullStateChanged(
        this MaskableGraphic graphic,
        Action<bool> callback,
        bool removeAllListeners = true)
    {
        if (graphic == null)
        {
            return;
        }

        SetListener(graphic.onCullStateChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_Dropdown 选项变化事件。
    /// </summary>
    public static void ExtSetOnValueChanged(
        this TMP_Dropdown dropdown,
        Action<int> callback,
        bool removeAllListeners = true)
    {
        if (dropdown == null)
        {
            return;
        }

        SetListener(dropdown.onValueChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_InputField 文本变化事件。
    /// </summary>
    public static void ExtSetOnValueChanged(
        this TMP_InputField inputField,
        Action<string> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onValueChanged, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_InputField 编辑结束事件。
    /// </summary>
    public static void ExtSetOnEndEdit(
        this TMP_InputField inputField,
        Action<string> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onEndEdit, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_InputField 提交事件。
    /// </summary>
    public static void ExtSetOnSubmit(
        this TMP_InputField inputField,
        Action<string> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onSubmit, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_InputField 获得焦点事件。
    /// </summary>
    public static void ExtSetOnSelect(
        this TMP_InputField inputField,
        Action<string> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onSelect, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_InputField 失去焦点事件。
    /// </summary>
    public static void ExtSetOnDeselect(
        this TMP_InputField inputField,
        Action<string> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onDeselect, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_InputField 文本选区变化事件。
    /// </summary>
    public static void ExtSetOnTextSelection(
        this TMP_InputField inputField,
        Action<string, int, int> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onTextSelection, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_InputField 文本选区结束事件。
    /// </summary>
    public static void ExtSetOnEndTextSelection(
        this TMP_InputField inputField,
        Action<string, int, int> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(inputField.onEndTextSelection, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置 TMP_InputField 触屏键盘状态变化事件。
    /// </summary>
    public static void ExtSetOnTouchScreenKeyboardStatusChanged(
        this TMP_InputField inputField,
        Action<TouchScreenKeyboard.Status> callback,
        bool removeAllListeners = true)
    {
        if (inputField == null)
        {
            return;
        }

        SetListener(
            inputField.onTouchScreenKeyboardStatusChanged,
            callback,
            removeAllListeners);
    }

    /// <summary>
    /// 设置 EventTrigger 的指定事件。
    /// </summary>
    public static void ExtSetEventTrigger(
        this EventTrigger eventTrigger,
        EventTriggerType eventType,
        Action<BaseEventData> callback,
        bool removeAllListeners = true)
    {
        if (eventTrigger == null)
        {
            return;
        }

        EventTrigger.Entry entry = eventTrigger.triggers.Find(
            item => item.eventID == eventType);

        if (entry == null)
        {
            if (callback == null)
            {
                return;
            }

            entry = new EventTrigger.Entry
            {
                eventID = eventType
            };
            eventTrigger.triggers.Add(entry);
        }

        if (entry.callback == null)
        {
            entry.callback = new EventTrigger.TriggerEvent();
        }

        SetListener(entry.callback, callback, removeAllListeners);
    }

    /// <summary>
    /// 设置无参数 UnityEvent 监听。
    /// </summary>
    private static void SetListener(
        UnityEvent targetEvent,
        Action callback,
        bool removeAllListeners)
    {
        if (targetEvent == null)
        {
            return;
        }

        if (removeAllListeners)
        {
            targetEvent.RemoveAllListeners();
        }

        if (callback != null)
        {
            targetEvent.AddListener(() => callback());
        }
    }

    /// <summary>
    /// 设置单参数 UnityEvent 监听。
    /// </summary>
    private static void SetListener<T>(
        UnityEvent<T> targetEvent,
        Action<T> callback,
        bool removeAllListeners)
    {
        if (targetEvent == null)
        {
            return;
        }

        if (removeAllListeners)
        {
            targetEvent.RemoveAllListeners();
        }

        if (callback != null)
        {
            targetEvent.AddListener(value => callback(value));
        }
    }

    /// <summary>
    /// 设置三参数 UnityEvent 监听。
    /// </summary>
    private static void SetListener<T0, T1, T2>(
        UnityEvent<T0, T1, T2> targetEvent,
        Action<T0, T1, T2> callback,
        bool removeAllListeners)
    {
        if (targetEvent == null)
        {
            return;
        }

        if (removeAllListeners)
        {
            targetEvent.RemoveAllListeners();
        }

        if (callback != null)
        {
            targetEvent.AddListener(
                (value0, value1, value2) => callback(value0, value1, value2));
        }
    }
}
