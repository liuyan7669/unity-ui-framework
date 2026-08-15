using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UILayoutExtensionsTests
{
    [Test]
    public void RectTransformSetters_OnlyChangeRequestedValues()
    {
        GameObject target = new GameObject("Rect", typeof(RectTransform));
        RectTransform rect = target.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(1f, 2f);
        rect.sizeDelta = new Vector2(100f, 200f);

        rect.ExtSetAnchoredPositionX(5f);
        rect.ExtSetAnchoredPositionY(6f);
        rect.ExtSetSizeDeltaX(300f);
        rect.ExtSetSizeDeltaY(400f);

        Assert.AreEqual(new Vector2(5f, 6f), rect.anchoredPosition);
        Assert.AreEqual(new Vector2(300f, 400f), rect.sizeDelta);
        Object.DestroyImmediate(target);
    }

    [Test]
    public void CanvasGroupAndNavigation_AreSetTogether()
    {
        GameObject target = new GameObject("UI", typeof(RectTransform), typeof(CanvasGroup), typeof(Button));
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        Button button = target.GetComponent<Button>();

        group.ExtSetCanvasGroupState(0.25f, false, false);
        button.ExtSetNavigationNone();

        Assert.AreEqual(0.25f, group.alpha, 0.0001f);
        Assert.IsFalse(group.interactable);
        Assert.IsFalse(group.blocksRaycasts);
        Assert.AreEqual(Navigation.Mode.None, button.navigation.mode);
        Object.DestroyImmediate(target);
    }

    [Test]
    public void SetPivotKeepPosition_PreservesWorldCorners()
    {
        GameObject parent = new GameObject("Parent", typeof(RectTransform));
        GameObject child = new GameObject("Child", typeof(RectTransform));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent.transform, false);
        rect.sizeDelta = new Vector2(200f, 100f);
        rect.anchoredPosition = new Vector2(30f, 40f);
        Vector3[] before = new Vector3[4];
        Vector3[] after = new Vector3[4];
        rect.GetWorldCorners(before);

        rect.ExtSetPivotKeepPosition(new Vector2(0f, 1f));
        rect.GetWorldCorners(after);

        for (int i = 0; i < before.Length; i++) Assert.AreEqual(before[i], after[i]);
        Object.DestroyImmediate(parent);
    }

    [Test]
    public void TextExtensions_SetTextAndVisibility()
    {
        GameObject legacyObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        Text legacyText = legacyObject.GetComponent<Text>();
        legacyText.ExtSetText(string.Empty, true);
        Assert.IsFalse(legacyObject.activeSelf);
        legacyText.ExtSetText("Legacy", true);
        Assert.IsTrue(legacyObject.activeSelf);
        Assert.AreEqual("Legacy", legacyText.text);

        GameObject tmpObject = new GameObject("TMP", typeof(RectTransform), typeof(TextMeshProUGUI));
        TMP_Text tmpText = tmpObject.GetComponent<TMP_Text>();
        tmpText.ExtSetText("TMP");
        Assert.AreEqual("TMP", tmpText.text);

        Object.DestroyImmediate(legacyObject);
        Object.DestroyImmediate(tmpObject);
    }
}
