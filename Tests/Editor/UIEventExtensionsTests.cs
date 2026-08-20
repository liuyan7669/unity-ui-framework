using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cowart.UIFramework.Tests
{
    public sealed class UIEventExtensionsTests
    {
        private readonly List<GameObject> testObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < testObjects.Count; i++)
            {
                if (testObjects[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(testObjects[i]);
                }
            }

            testObjects.Clear();
        }

        [Test]
        public void ButtonClick_ReplacesAndAppendsListeners()
        {
            Button button = CreateComponent<Button>("Button");

            AssertNoParameterListener(
                button.onClick,
                (callback, removeAllListeners) =>
                    button.ExtSetOnClick(callback, removeAllListeners));
        }

        [Test]
        public void UGUIValueEvents_ReplaceAppendAndPassValues()
        {
            Toggle toggle = CreateComponent<Toggle>("Toggle");
            AssertSingleParameterListener<bool>(
                toggle.onValueChanged,
                (callback, removeAllListeners) =>
                    toggle.ExtSetOnValueChanged(callback, removeAllListeners),
                true,
                false);

            Slider slider = CreateComponent<Slider>("Slider");
            AssertSingleParameterListener<float>(
                slider.onValueChanged,
                (callback, removeAllListeners) =>
                    slider.ExtSetOnValueChanged(callback, removeAllListeners),
                0.25f,
                0.75f);

            Scrollbar scrollbar = CreateComponent<Scrollbar>("Scrollbar");
            AssertSingleParameterListener<float>(
                scrollbar.onValueChanged,
                (callback, removeAllListeners) =>
                    scrollbar.ExtSetOnValueChanged(callback, removeAllListeners),
                0.2f,
                0.8f);

            Dropdown dropdown = CreateComponent<Dropdown>("Dropdown");
            AssertSingleParameterListener<int>(
                dropdown.onValueChanged,
                (callback, removeAllListeners) =>
                    dropdown.ExtSetOnValueChanged(callback, removeAllListeners),
                1,
                2);

            ScrollRect scrollRect = CreateComponent<ScrollRect>("ScrollRect");
            AssertSingleParameterListener<Vector2>(
                scrollRect.onValueChanged,
                (callback, removeAllListeners) =>
                    scrollRect.ExtSetOnValueChanged(callback, removeAllListeners),
                new Vector2(0.1f, 0.2f),
                new Vector2(0.7f, 0.8f));

            InputField inputField = CreateComponent<InputField>("InputFieldValue");
            AssertSingleParameterListener<string>(
                inputField.onValueChanged,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnValueChanged(callback, removeAllListeners),
                "first",
                "second");
        }

        [Test]
        public void LegacyInputFieldEditingEvents_ReplaceAppendAndPassValues()
        {
            InputField endEditInput = CreateComponent<InputField>("InputFieldEndEdit");
            AssertSingleParameterListener<string>(
                endEditInput.onEndEdit,
                (callback, removeAllListeners) =>
                    endEditInput.ExtSetOnEndEdit(callback, removeAllListeners),
                "end-first",
                "end-second");

            InputField submitInput = CreateComponent<InputField>("InputFieldSubmit");
    #if UNITY_2021_1_OR_NEWER
            AssertSingleParameterListener<string>(
                submitInput.onSubmit,
                (callback, removeAllListeners) =>
                    submitInput.ExtSetOnSubmit(callback, removeAllListeners),
                "submit-first",
                "submit-second");
    #else
            AssertSingleParameterListener<string>(
                submitInput.onEndEdit,
                (callback, removeAllListeners) =>
                    submitInput.ExtSetOnSubmit(callback, removeAllListeners),
                "submit-first",
                "submit-second");
    #endif
        }

        [Test]
        public void CullStateChanged_ReplacesAppendsAndPassesValue()
        {
            Image image = CreateComponent<Image>("Image");

            AssertSingleParameterListener<bool>(
                image.onCullStateChanged,
                (callback, removeAllListeners) =>
                    image.ExtSetOnCullStateChanged(callback, removeAllListeners),
                true,
                false);
        }

        [Test]
        public void TMPDropdownValueChanged_ReplacesAppendsAndPassesValue()
        {
            TMP_Dropdown dropdown = CreateComponent<TMP_Dropdown>("TMPDropdown");

            AssertSingleParameterListener<int>(
                dropdown.onValueChanged,
                (callback, removeAllListeners) =>
                    dropdown.ExtSetOnValueChanged(callback, removeAllListeners),
                3,
                4);
        }

        [Test]
        public void TMPInputFieldStringEvents_ReplaceAppendAndPassValues()
        {
            TMP_InputField inputField = CreateComponent<TMP_InputField>("TMPInputField");

            AssertSingleParameterListener<string>(
                inputField.onValueChanged,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnValueChanged(callback, removeAllListeners),
                "value-first",
                "value-second");
            AssertSingleParameterListener<string>(
                inputField.onEndEdit,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnEndEdit(callback, removeAllListeners),
                "end-first",
                "end-second");
            AssertSingleParameterListener<string>(
                inputField.onSubmit,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnSubmit(callback, removeAllListeners),
                "submit-first",
                "submit-second");
            AssertSingleParameterListener<string>(
                inputField.onSelect,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnSelect(callback, removeAllListeners),
                "select-first",
                "select-second");
            AssertSingleParameterListener<string>(
                inputField.onDeselect,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnDeselect(callback, removeAllListeners),
                "deselect-first",
                "deselect-second");
        }

        [Test]
        public void TMPInputFieldSelectionEvents_ReplaceAppendAndPassValues()
        {
            TMP_InputField inputField = CreateComponent<TMP_InputField>("TMPSelection");

            AssertThreeParameterListener<string, int, int>(
                inputField.onTextSelection,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnTextSelection(callback, removeAllListeners),
                "selection-first",
                1,
                2,
                "selection-second",
                3,
                4);
            AssertThreeParameterListener<string, int, int>(
                inputField.onEndTextSelection,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnEndTextSelection(callback, removeAllListeners),
                "end-selection-first",
                5,
                6,
                "end-selection-second",
                7,
                8);
        }

        [Test]
        public void TMPKeyboardStatusChanged_ReplacesAppendsAndPassesValue()
        {
            TMP_InputField inputField = CreateComponent<TMP_InputField>("TMPKeyboard");

            AssertSingleParameterListener<TouchScreenKeyboard.Status>(
                inputField.onTouchScreenKeyboardStatusChanged,
                (callback, removeAllListeners) =>
                    inputField.ExtSetOnTouchScreenKeyboardStatusChanged(
                        callback,
                        removeAllListeners),
                TouchScreenKeyboard.Status.Visible,
                TouchScreenKeyboard.Status.Done);
        }

        [Test]
        public void EventTrigger_ReplacesAppendsAndPassesEventData()
        {
            EventTrigger eventTrigger = CreateComponent<EventTrigger>("EventTrigger");
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            eventTrigger.triggers.Add(entry);

            int removedListenerCalls = 0;
            int replacementCalls = 0;
            int appendedCalls = 0;
            BaseEventData replacementValue = null;
            BaseEventData appendedValue = null;
            entry.callback.AddListener(value => removedListenerCalls++);

            eventTrigger.ExtSetEventTrigger(
                EventTriggerType.PointerClick,
                value =>
                {
                    replacementCalls++;
                    replacementValue = value;
                });

            BaseEventData firstValue = new BaseEventData(null);
            entry.callback.Invoke(firstValue);

            Assert.AreEqual(0, removedListenerCalls);
            Assert.AreEqual(1, replacementCalls);
            Assert.AreSame(firstValue, replacementValue);

            eventTrigger.ExtSetEventTrigger(
                EventTriggerType.PointerClick,
                value =>
                {
                    appendedCalls++;
                    appendedValue = value;
                },
                false);

            BaseEventData secondValue = new BaseEventData(null);
            entry.callback.Invoke(secondValue);

            Assert.AreEqual(0, removedListenerCalls);
            Assert.AreEqual(2, replacementCalls);
            Assert.AreEqual(1, appendedCalls);
            Assert.AreSame(secondValue, replacementValue);
            Assert.AreSame(secondValue, appendedValue);
            Assert.AreEqual(1, eventTrigger.triggers.Count);
        }

        private T CreateComponent<T>(string name) where T : Component
        {
            GameObject target = new GameObject(name, typeof(RectTransform), typeof(T));
            testObjects.Add(target);
            return target.GetComponent<T>();
        }

        private static void AssertNoParameterListener(
            UnityEvent targetEvent,
            Action<Action, bool> setListener)
        {
            int removedListenerCalls = 0;
            int replacementCalls = 0;
            int appendedCalls = 0;
            targetEvent.AddListener(() => removedListenerCalls++);

            setListener(() => replacementCalls++, true);
            targetEvent.Invoke();

            Assert.AreEqual(0, removedListenerCalls);
            Assert.AreEqual(1, replacementCalls);

            setListener(() => appendedCalls++, false);
            targetEvent.Invoke();

            Assert.AreEqual(0, removedListenerCalls);
            Assert.AreEqual(2, replacementCalls);
            Assert.AreEqual(1, appendedCalls);
        }

        private static void AssertSingleParameterListener<T>(
            UnityEvent<T> targetEvent,
            Action<Action<T>, bool> setListener,
            T firstValue,
            T secondValue)
        {
            int removedListenerCalls = 0;
            int replacementCalls = 0;
            int appendedCalls = 0;
            T replacementValue = default(T);
            T appendedValue = default(T);
            targetEvent.AddListener(value => removedListenerCalls++);

            setListener(
                value =>
                {
                    replacementCalls++;
                    replacementValue = value;
                },
                true);
            targetEvent.Invoke(firstValue);

            Assert.AreEqual(0, removedListenerCalls);
            Assert.AreEqual(1, replacementCalls);
            Assert.AreEqual(firstValue, replacementValue);

            setListener(
                value =>
                {
                    appendedCalls++;
                    appendedValue = value;
                },
                false);
            targetEvent.Invoke(secondValue);

            Assert.AreEqual(0, removedListenerCalls);
            Assert.AreEqual(2, replacementCalls);
            Assert.AreEqual(1, appendedCalls);
            Assert.AreEqual(secondValue, replacementValue);
            Assert.AreEqual(secondValue, appendedValue);
        }

        private static void AssertThreeParameterListener<T0, T1, T2>(
            UnityEvent<T0, T1, T2> targetEvent,
            Action<Action<T0, T1, T2>, bool> setListener,
            T0 firstValue0,
            T1 firstValue1,
            T2 firstValue2,
            T0 secondValue0,
            T1 secondValue1,
            T2 secondValue2)
        {
            int removedListenerCalls = 0;
            int replacementCalls = 0;
            int appendedCalls = 0;
            T0 replacementValue0 = default(T0);
            T1 replacementValue1 = default(T1);
            T2 replacementValue2 = default(T2);
            T0 appendedValue0 = default(T0);
            T1 appendedValue1 = default(T1);
            T2 appendedValue2 = default(T2);
            targetEvent.AddListener((value0, value1, value2) => removedListenerCalls++);

            setListener(
                (value0, value1, value2) =>
                {
                    replacementCalls++;
                    replacementValue0 = value0;
                    replacementValue1 = value1;
                    replacementValue2 = value2;
                },
                true);
            targetEvent.Invoke(firstValue0, firstValue1, firstValue2);

            Assert.AreEqual(0, removedListenerCalls);
            Assert.AreEqual(1, replacementCalls);
            Assert.AreEqual(firstValue0, replacementValue0);
            Assert.AreEqual(firstValue1, replacementValue1);
            Assert.AreEqual(firstValue2, replacementValue2);

            setListener(
                (value0, value1, value2) =>
                {
                    appendedCalls++;
                    appendedValue0 = value0;
                    appendedValue1 = value1;
                    appendedValue2 = value2;
                },
                false);
            targetEvent.Invoke(secondValue0, secondValue1, secondValue2);

            Assert.AreEqual(0, removedListenerCalls);
            Assert.AreEqual(2, replacementCalls);
            Assert.AreEqual(1, appendedCalls);
            Assert.AreEqual(secondValue0, replacementValue0);
            Assert.AreEqual(secondValue1, replacementValue1);
            Assert.AreEqual(secondValue2, replacementValue2);
            Assert.AreEqual(secondValue0, appendedValue0);
            Assert.AreEqual(secondValue1, appendedValue1);
            Assert.AreEqual(secondValue2, appendedValue2);
        }
    }
}
