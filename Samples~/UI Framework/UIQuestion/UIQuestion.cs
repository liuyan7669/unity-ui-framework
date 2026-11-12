using System;
using System.Collections;
using System.Collections.Generic;
using Cowart.ConfigLoader;
using Cowart.ObjectUtilities;
using Cowart.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 选择题界面。
/// 按顺序显示题目，支持单选和多选，并在确认后显示答案与解析。
/// </summary>
public class UIQuestion : UIView
{
    /// <summary>
    /// 题目、答案和解析文本使用的首行缩进。
    /// </summary>
    private const string FirstLineIndent = "\u3000\u3000";

    /// <summary>
    /// 打开选择题界面时传入的参数。
    /// </summary>
    public class UIArg : UIArgs
    {
        /// <summary>
        /// 需要按顺序显示的题目列表。
        /// 界面打开时会清空每道题的当前得分和已选答案。
        /// </summary>
        public List<QuestionData> questions = new List<QuestionData>();

        /// <summary>
        /// 开始显示一道题时触发的回调，参数为题目在 questions 中的索引。
        /// </summary>
        public Action<int> StartAction;

        /// <summary>
        /// 一道题首次确认完成时触发的回调，参数为题目在 questions 中的索引。
        /// </summary>
        public Action<int> EndAction;

        /// <summary>
        /// 第一次点击按钮并成功确认当前题目后触发的回调，参数为题目在 questions 中的索引。
        /// 此时已经完成得分计算、锁定选项并显示答案与解析。
        /// 未选择答案且不允许空选时不会触发。
        /// </summary>
        public Action<int> ConfirmAction;

        /// <summary>
        /// 当前题目确认后、进入下一题前触发的回调，参数为当前题目在 questions 中的索引。
        /// 普通模式下由第二次点击触发；跳过确认停留时紧接 ConfirmAction 在同一次点击中触发。
        /// 最后一题也会触发，随后再触发 AllFinishAction。
        /// </summary>
        public Action<int> NextAction;

        /// <summary>
        /// 用户操作选项并使选择结果发生变化时触发的回调。
        /// 参数依次为题目索引、选项索引、该选项是否选中，以及更新后的答案字符串。
        /// 单选题切换选项时回调新选中的选项；多选题选中或取消选项时都会回调。
        /// </summary>
        public Action<int, int, bool, string> SelectionChangedAction;

        /// <summary>
        /// 最后一题确认完成并再次点击“下一题”时触发的回调，整个答题流程只触发一次。
        /// </summary>
        public Action AllFinishAction;

        /// <summary>
        /// 是否允许未选择任何答案时确认题目。
        /// null 表示使用 Inspector 中的默认设置，true 或 false 表示覆盖本次打开期间的设置。
        /// </summary>
        public bool? allowEmptySelection;

        /// <summary>
        /// 是否跳过确认后的停留步骤，成功确认后直接进入下一题。
        /// null 表示使用 Inspector 中的默认设置，true 或 false 表示覆盖本次打开期间的设置。
        /// 开启后仍会保存已选答案、计算得分并触发确认相关回调。
        /// </summary>
        public bool? skipConfirmation;
    }

    /// <summary>
    /// 用于生成题目项的预制体生成器。
    /// </summary>
    [SerializeField] private PrefabSpawner generation;

    /// <summary>
    /// 确认答案及进入下一题的按钮。
    /// </summary>
    [SerializeField] private Button okButton;

    /// <summary>
    /// 确认按钮上的文本组件。
    /// </summary>
    [SerializeField] private TMP_Text buttonText;

    /// <summary>
    /// 确认答案后，正确选项文本使用的颜色。
    /// </summary>
    [SerializeField] private Color correctOptionColor = new Color(0f, 0.65f, 0.2f);

    /// <summary>
    /// 确认答案后，非正确选项文本使用的颜色。
    /// </summary>
    [SerializeField] private Color wrongOptionColor = Color.red;

    /// <summary>
    /// 是否允许未选择任何答案时确认当前题目。
    /// </summary>
    [SerializeField, Tooltip("是否允许不选择答案也继续下一题。")] private bool allowEmptySelection;

    /// <summary>
    /// 是否在成功确认后直接进入下一题，不停留显示答案与解析。
    /// </summary>
    [SerializeField, Tooltip("是否跳过确认后的停留步骤，记录答案和得分后直接进入下一题。")] private bool skipConfirmation;

    /// <summary>
    /// 当前答题流程使用的题目列表。
    /// </summary>
    private readonly List<QuestionData> currentQuestions = new List<QuestionData>();

    /// <summary>
    /// 已触发结束回调的题目编号，用于避免重复触发 EndAction。
    /// </summary>
    private readonly HashSet<string> finishedQuestionIds = new HashSet<string>();

    /// <summary>
    /// 当前题目生成的选项 Toggle 列表。
    /// </summary>
    private readonly List<Toggle> currentToggles = new List<Toggle>();

    /// <summary>
    /// 当前题目各选项的文本图形组件。
    /// </summary>
    private readonly List<Graphic> currentOptionTexts = new List<Graphic>();

    /// <summary>
    /// 当前题目各选项文本的初始颜色。
    /// </summary>
    private readonly List<Color> currentOptionDefaultColors = new List<Color>();

    /// <summary>
    /// 当前界面参数缓存，用于在答题过程中触发回调。
    /// </summary>
    private UIArg currentArg;

    /// <summary>
    /// 当前题目的答案结果文本组件。
    /// </summary>
    private TMP_Text currentAnswerText;

    /// <summary>
    /// 当前题目的解析文本组件。
    /// </summary>
    private TMP_Text currentAnalysisText;

    /// <summary>
    /// 当前显示题目在 currentQuestions 中的索引。
    /// </summary>
    private int currentQuestionIndex;

    /// <summary>
    /// 当前题目是否已经确认答案。
    /// </summary>
    private bool currentQuestionConfirmed;

    /// <summary>
    /// 本次打开期间实际使用的空选确认设置。
    /// </summary>
    private bool currentAllowEmptySelection;

    /// <summary>
    /// 本次打开期间实际使用的跳过确认停留设置。
    /// </summary>
    private bool currentSkipConfirmation;

    /// <summary>
    /// 全部题目完成回调是否已经触发。
    /// </summary>
    private bool allFinishInvoked;

    /// <summary>
    /// 延迟到下一帧重建布局的协程引用。
    /// </summary>
    private Coroutine rebuildLayoutCoroutine;

    /// <summary>
    /// 打开界面并初始化答题流程。
    /// </summary>
    /// <param name="args">选择题界面参数。</param>
    protected override void OnOpen(UIArgs args)
    {
        UIArg arg = args.As<UIArg>();
        currentArg = arg;
        currentQuestionIndex = 0;
        currentQuestionConfirmed = false;
        currentAllowEmptySelection = arg.allowEmptySelection ?? allowEmptySelection;
        currentSkipConfirmation = arg.skipConfirmation ?? skipConfirmation;
        allFinishInvoked = false;
        currentQuestions.Clear();
        finishedQuestionIds.Clear();
        currentToggles.Clear();
        currentOptionTexts.Clear();
        currentOptionDefaultColors.Clear();
        currentAnswerText = null;
        currentAnalysisText = null;

        generation.ClearGeneratedObjects();
        okButton.gameObject.SetActive(true);
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(OnOkButtonClick);

        List<QuestionData> questions = arg.questions;
        for (int i = 0; i < questions.Count; i++)
        {
            QuestionData questionData = questions[i];
            ResetQuestionAnswer(questionData);
            currentQuestions.Add(questionData);
        }

        if (currentQuestions.Count == 0)
        {
            okButton.gameObject.SetActive(false);
            RequestRebuildLayout();
            return;
        }

        ShowQuestion(0);
    }

    /// <summary>
    /// 关闭界面时停止布局协程并清理运行时状态和按钮监听。
    /// </summary>
    protected override void OnClose()
    {
        if (rebuildLayoutCoroutine != null)
        {
            StopCoroutine(rebuildLayoutCoroutine);
            rebuildLayoutCoroutine = null;
        }

        currentArg = null;
        currentQuestions.Clear();
        finishedQuestionIds.Clear();
        currentToggles.Clear();
        currentOptionTexts.Clear();
        currentOptionDefaultColors.Clear();
        currentAnswerText = null;
        currentAnalysisText = null;
        currentQuestionIndex = 0;
        currentQuestionConfirmed = false;
        currentAllowEmptySelection = false;
        currentSkipConfirmation = false;
        allFinishInvoked = false;
        generation.ClearGeneratedObjects();
        okButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 生成并显示指定索引的题目。
    /// </summary>
    /// <param name="questionIndex">题目在当前题目列表中的索引。</param>
    private void ShowQuestion(int questionIndex)
    {
        if (questionIndex < 0 || questionIndex >= currentQuestions.Count)
        {
            return;
        }

        currentQuestionIndex = questionIndex;
        currentQuestionConfirmed = false;
        currentToggles.Clear();
        currentOptionTexts.Clear();
        currentOptionDefaultColors.Clear();
        currentAnswerText = null;
        currentAnalysisText = null;

        generation.ClearGeneratedObjects();

        QuestionData questionData = currentQuestions[currentQuestionIndex];
        CreateQuestionItem(questionData, currentQuestionIndex);
        currentArg?.StartAction?.Invoke(currentQuestionIndex);
        RefreshOkButtonText(currentSkipConfirmation ? "下一题" : "确认");
        RequestRebuildLayout();
    }

    /// <summary>
    /// 清空题目的本次作答结果。
    /// </summary>
    /// <param name="questionData">需要重置的题目数据。</param>
    private void ResetQuestionAnswer(QuestionData questionData)
    {
        questionData.currentScore = 0;
        questionData.selectAnswer = string.Empty;
    }

    /// <summary>
    /// 创建题目项并填充题干、答案、解析和选项。
    /// </summary>
    /// <param name="questionData">题目数据。</param>
    /// <param name="questionIndex">题目在当前题目列表中的索引。</param>
    private void CreateQuestionItem(QuestionData questionData, int questionIndex)
    {
        GameObject item = generation.ClonePrefab();
        if (item == null)
        {
            return;
        }

        Transform itemTransform = item.transform;
        SetChildText(itemTransform, "TitleText", "Question " + (questionIndex + 1));
        SetChildText(itemTransform, "QuestionText", GetFirstLineIndentedText(questionData.question));

        currentAnswerText = GetChildTMPText(itemTransform, "AnswerText");
        currentAnalysisText = GetChildTMPText(itemTransform, "AnalysisText");
        Transform optionsTransform = itemTransform.Find("Options");

        RefreshAnswerAndAnalysis(questionData, false);
        CreateOptions(questionData, optionsTransform);
    }

    /// <summary>
    /// 根据题目数据生成选项，并绑定选择变化监听。
    /// </summary>
    /// <param name="questionData">题目数据。</param>
    /// <param name="optionsTransform">选项列表根节点。</param>
    private void CreateOptions(QuestionData questionData, Transform optionsTransform)
    {
        if (optionsTransform == null)
        {
            return;
        }

        PrefabSpawner optionGeneration = optionsTransform.GetComponent<PrefabSpawner>();
        if (optionGeneration == null)
        {
            return;
        }

        optionGeneration.ClearGeneratedObjects();
        ToggleGroup toggleGroup = optionsTransform.GetComponent<ToggleGroup>();
        bool isMultiple = IsMultipleQuestion(questionData);
        if (toggleGroup != null)
        {
            toggleGroup.allowSwitchOff = true;
        }

        List<Toggle> toggles = new List<Toggle>();
        for (int i = 0; i < questionData.options.Count; i++)
        {
            int optionIndex = i;
            QuestionOptionData optionData = questionData.options[optionIndex];
            GameObject optionItem = optionGeneration.ClonePrefab();
            if (optionItem == null)
            {
                continue;
            }

            Toggle toggle = optionItem.GetComponent<Toggle>();
            if (toggle == null)
            {
                continue;
            }

            toggles.Add(toggle);
            currentToggles.Add(toggle);
            toggle.onValueChanged.RemoveAllListeners();
            toggle.group = isMultiple ? null : toggleGroup;
            Graphic optionTextGraphic = SetOptionText(optionItem.transform, optionData.text);
            currentOptionTexts.Add(optionTextGraphic);
            currentOptionDefaultColors.Add(optionTextGraphic != null ? optionTextGraphic.color : Color.white);
            toggle.SetIsOnWithoutNotify(IsAnswerSelected(questionData, optionIndex));
            toggle.onValueChanged.AddListener((bool isOn) =>
            {
                if (isMultiple || isOn)
                {
                    RefreshSelection(questionData, toggles);
                    currentArg?.SelectionChangedAction?.Invoke(
                        currentQuestionIndex,
                        optionIndex,
                        isOn,
                        questionData.selectAnswer);
                }
            });
        }

        RefreshSelection(questionData, toggles);
    }

    /// <summary>
    /// 将当前选中的选项写回题目数据。
    /// </summary>
    /// <param name="questionData">当前题目数据。</param>
    /// <param name="toggles">当前题目的选项 Toggle 列表。</param>
    private void RefreshSelection(QuestionData questionData, List<Toggle> toggles)
    {
        if (currentQuestionConfirmed)
        {
            return;
        }

        questionData.selectAnswer = GetSelectedAnswer(toggles);
        RequestRebuildLayout();
    }

    /// <summary>
    /// 刷新当前题目的作答结果和答案解析文本。
    /// </summary>
    /// <param name="questionData">当前题目数据。</param>
    /// <param name="showAnalysis">是否显示作答结果和解析。</param>
    private void RefreshAnswerAndAnalysis(QuestionData questionData, bool showAnalysis)
    {
        if (currentAnswerText != null)
        {
            if (showAnalysis)
            {
                string answerText = IsCurrentAnswerCorrect(questionData) ? "回答正确" : "回答错误";
                answerText += "，正确答案：" + questionData.standardAnswer;
                currentAnswerText.text = GetFirstLineIndentedText(answerText);
            }
            else
            {
                currentAnswerText.text = string.Empty;
            }
        }

        if (currentAnalysisText != null)
        {
            currentAnalysisText.text = showAnalysis ? GetFirstLineIndentedText(questionData.analysis) : string.Empty;
        }
    }

    /// <summary>
    /// 在当前题目满足完成条件时触发一次 EndAction。
    /// </summary>
    /// <param name="questionData">当前题目数据。</param>
    private void TryFinishQuestion(QuestionData questionData)
    {
        if (!currentAllowEmptySelection && string.IsNullOrEmpty(questionData.selectAnswer))
        {
            return;
        }

        if (!finishedQuestionIds.Contains(questionData.id))
        {
            finishedQuestionIds.Add(questionData.id);
            currentArg?.EndAction?.Invoke(currentQuestionIndex);
        }

    }

    /// <summary>
    /// 刷新确认按钮文本。
    /// </summary>
    /// <param name="text">要显示的文本。</param>
    private void RefreshOkButtonText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    /// <summary>
    /// 处理按钮点击：普通模式首次点击确认答案、再次点击进入下一题；
    /// 跳过确认停留时在同一次点击中完成确认并进入下一题。
    /// </summary>
    private void OnOkButtonClick()
    {
        if (currentQuestionConfirmed)
        {
            NextQuestion();
            return;
        }

        ConfirmCurrentQuestion();
        if (currentSkipConfirmation && currentQuestionConfirmed)
        {
            NextQuestion();
        }
    }

    /// <summary>
    /// 确认当前题目，计算得分、锁定选项并显示答案解析。
    /// </summary>
    private void ConfirmCurrentQuestion()
    {
        if (currentQuestionIndex < 0 || currentQuestionIndex >= currentQuestions.Count)
        {
            return;
        }

        QuestionData questionData = currentQuestions[currentQuestionIndex];
        if (!currentAllowEmptySelection && string.IsNullOrEmpty(questionData.selectAnswer))
        {
            return;
        }

        questionData.currentScore = IsCurrentAnswerCorrect(questionData) ? questionData.standardScore : 0;
        currentQuestionConfirmed = true;
        SetCurrentTogglesInteractable(false);
        RefreshAnswerAndAnalysis(questionData, true);
        RefreshOptionTextColors(questionData);
        TryFinishQuestion(questionData);
        RefreshOkButtonText("下一题");
        RequestRebuildLayout();
        currentArg?.ConfirmAction?.Invoke(currentQuestionIndex);
    }

    /// <summary>
    /// 进入下一题；当前题目为最后一题时触发全部完成回调。
    /// </summary>
    private void NextQuestion()
    {
        if (!currentQuestionConfirmed)
        {
            ConfirmCurrentQuestion();
            return;
        }

        currentArg?.NextAction?.Invoke(currentQuestionIndex);

        if (currentQuestionIndex < currentQuestions.Count - 1)
        {
            ShowQuestion(currentQuestionIndex + 1);
            return;
        }

        if (!allFinishInvoked)
        {
            allFinishInvoked = true;
            currentArg?.AllFinishAction?.Invoke();
        }
    }

    /// <summary>
    /// 判断题目的已选答案是否与标准答案完全一致。
    /// </summary>
    /// <param name="questionData">题目数据。</param>
    /// <returns>答案一致时返回 true，否则返回 false。</returns>
    private bool IsCurrentAnswerCorrect(QuestionData questionData)
    {
        return questionData.selectAnswer == questionData.standardAnswer;
    }

    /// <summary>
    /// 设置当前题目所有选项是否允许交互。
    /// </summary>
    /// <param name="interactable">是否允许交互。</param>
    private void SetCurrentTogglesInteractable(bool interactable)
    {
        for (int i = 0; i < currentToggles.Count; i++)
        {
            Toggle toggle = currentToggles[i];
            if (toggle != null)
            {
                toggle.interactable = interactable;
            }
        }
    }

    /// <summary>
    /// 确认答案后，根据标准答案刷新各选项文本颜色。
    /// </summary>
    /// <param name="questionData">当前题目数据。</param>
    private void RefreshOptionTextColors(QuestionData questionData)
    {
        if (string.IsNullOrEmpty(questionData.standardAnswer))
        {
            RestoreOptionTextColors();
            return;
        }

        for (int i = 0; i < currentOptionTexts.Count; i++)
        {
            Graphic optionText = currentOptionTexts[i];
            if (optionText != null)
            {
                optionText.color = IsStandardAnswerOption(questionData, i) ? correctOptionColor : wrongOptionColor;
            }
        }
    }

    /// <summary>
    /// 恢复当前题目各选项文本的初始颜色。
    /// </summary>
    private void RestoreOptionTextColors()
    {
        for (int i = 0; i < currentOptionTexts.Count; i++)
        {
            Graphic optionText = currentOptionTexts[i];
            if (optionText != null && i < currentOptionDefaultColors.Count)
            {
                optionText.color = currentOptionDefaultColors[i];
            }
        }
    }

    /// <summary>
    /// 根据标准答案包含的选项数量判断是否为多选题。
    /// </summary>
    /// <param name="questionData">题目数据。</param>
    /// <returns>标准答案包含多个选项时返回 true，否则返回 false。</returns>
    private bool IsMultipleQuestion(QuestionData questionData)
    {
        return !string.IsNullOrEmpty(questionData.standardAnswer) && questionData.standardAnswer.Length > 1;
    }

    /// <summary>
    /// 判断指定选项是否包含在标准答案中。
    /// </summary>
    /// <param name="questionData">题目数据。</param>
    /// <param name="optionIndex">选项索引。</param>
    /// <returns>选项属于标准答案时返回 true，否则返回 false。</returns>
    private bool IsStandardAnswerOption(QuestionData questionData, int optionIndex)
    {
        if (string.IsNullOrEmpty(questionData.standardAnswer))
        {
            return false;
        }

        string answerCode = GetAnswerCode(optionIndex);
        return questionData.standardAnswer.Contains(answerCode);
    }

    /// <summary>
    /// 判断指定选项是否包含在当前已选答案中。
    /// </summary>
    /// <param name="questionData">题目数据。</param>
    /// <param name="optionIndex">选项索引。</param>
    /// <returns>选项已被选择时返回 true，否则返回 false。</returns>
    private bool IsAnswerSelected(QuestionData questionData, int optionIndex)
    {
        if (string.IsNullOrEmpty(questionData.selectAnswer))
        {
            return false;
        }

        string answerCode = GetAnswerCode(optionIndex);
        return questionData.selectAnswer.Contains(answerCode);
    }

    /// <summary>
    /// 按选项顺序组合当前选中的答案编码。
    /// </summary>
    /// <param name="toggles">选项 Toggle 列表。</param>
    /// <returns>由 A、B、C 等选项编码组成的答案字符串。</returns>
    private string GetSelectedAnswer(List<Toggle> toggles)
    {
        string selectedAnswer = string.Empty;
        for (int i = 0; i < toggles.Count; i++)
        {
            Toggle toggle = toggles[i];
            if (toggle != null && toggle.isOn)
            {
                selectedAnswer += GetAnswerCode(i);
            }
        }

        return selectedAnswer;
    }

    /// <summary>
    /// 将从零开始的选项索引转换为 A、B、C 等答案编码。
    /// </summary>
    /// <param name="optionIndex">选项索引。</param>
    /// <returns>选项对应的答案编码。</returns>
    private string GetAnswerCode(int optionIndex)
    {
        return ((char)('A' + optionIndex)).ToString();
    }

    /// <summary>
    /// 设置选项文本，并返回用于后续修改颜色的图形组件。
    /// 优先查找名为 Label 的子节点，同时兼容 TMP_Text 和旧版 Text。
    /// </summary>
    /// <param name="optionTransform">选项根节点。</param>
    /// <param name="text">选项显示文本。</param>
    /// <returns>找到的文本图形组件；未找到时返回 null。</returns>
    private Graphic SetOptionText(Transform optionTransform, string text)
    {
        Transform labelTransform = optionTransform.Find("Label");
        if (labelTransform != null)
        {
            SetText(labelTransform, text);
            return GetTextGraphic(labelTransform);
        }

        TMP_Text tmpText = optionTransform.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null)
        {
            tmpText.text = text;
            return tmpText;
        }

        Text legacyText = optionTransform.GetComponentInChildren<Text>(true);
        if (legacyText != null)
        {
            legacyText.text = text;
            return legacyText;
        }

        return null;
    }

    /// <summary>
    /// 设置指定名称子节点上的文本。
    /// </summary>
    /// <param name="parent">父节点。</param>
    /// <param name="childName">子节点名称。</param>
    /// <param name="text">要显示的文本。</param>
    private void SetChildText(Transform parent, string childName, string text)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            SetText(child, text);
        }
    }

    /// <summary>
    /// 获取指定名称子节点上的 TMP_Text 组件。
    /// </summary>
    /// <param name="parent">父节点。</param>
    /// <param name="childName">子节点名称。</param>
    /// <returns>找到的 TMP_Text 组件；未找到时返回 null。</returns>
    private TMP_Text GetChildTMPText(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    /// <summary>
    /// 为非空文本添加首行缩进。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <returns>添加首行缩进后的文本；原始文本为空时返回空字符串。</returns>
    private string GetFirstLineIndentedText(string text)
    {
        return string.IsNullOrEmpty(text) ? string.Empty : FirstLineIndent + text;
    }

    /// <summary>
    /// 获取节点上的文本图形组件，兼容 TMP_Text 和旧版 Text。
    /// </summary>
    /// <param name="target">目标节点。</param>
    /// <returns>找到的文本图形组件；未找到时返回 null。</returns>
    private Graphic GetTextGraphic(Transform target)
    {
        TMP_Text tmpText = target.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            return tmpText;
        }

        Text legacyText = target.GetComponent<Text>();
        if (legacyText != null)
        {
            return legacyText;
        }

        return null;
    }

    /// <summary>
    /// 设置节点上的文本，优先使用 TMP_Text，同时兼容旧版 Text。
    /// </summary>
    /// <param name="target">目标节点。</param>
    /// <param name="text">要显示的文本。</param>
    private void SetText(Transform target, string text)
    {
        TMP_Text tmpText = target.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = text;
            return;
        }

        Text legacyText = target.GetComponent<Text>();
        if (legacyText != null)
        {
            legacyText.text = text;
        }
    }

    /// <summary>
    /// 立即重建一次题目布局，并安排下一帧再次重建以等待 UI 尺寸更新。
    /// </summary>
    private void RequestRebuildLayout()
    {
        RebuildQuestionLayout();

        if (rebuildLayoutCoroutine != null)
        {
            StopCoroutine(rebuildLayoutCoroutine);
        }

        if (isActiveAndEnabled)
        {
            rebuildLayoutCoroutine = StartCoroutine(RebuildLayoutNextFrame());
        }
    }

    /// <summary>
    /// 等待一帧后再次重建题目布局。
    /// </summary>
    /// <returns>协程迭代器。</returns>
    private IEnumerator RebuildLayoutNextFrame()
    {
        yield return null;
        RebuildQuestionLayout();
        rebuildLayoutCoroutine = null;
    }

    /// <summary>
    /// 从选项子列表到题目列表逐层强制重建布局。
    /// </summary>
    private void RebuildQuestionLayout()
    {
        if (generation == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < generation.SpawnedObjects.Count; i++)
        {
            GameObject item = generation.SpawnedObjects[i];
            if (item == null)
            {
                continue;
            }

            PrefabSpawner[] childGenerations = item.GetComponentsInChildren<PrefabSpawner>(true);
            for (int j = 0; j < childGenerations.Length; j++)
            {
                childGenerations[j].RebuildNestedLayouts();
            }

            RectTransform itemRectTransform = item.transform as RectTransform;
            if (itemRectTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemRectTransform);
            }
        }

        generation.RebuildNestedLayouts();

        RectTransform generationRectTransform = generation.transform as RectTransform;
        if (generationRectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(generationRectTransform);
        }

        Canvas.ForceUpdateCanvases();
    }
}
