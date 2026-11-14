using UnityEditor;
using Cowart.InspectorGroups.Editor;

namespace Cowart.UIFramework.Editor
{
    /// <summary>使用 Inspector Groups 绘制字段与条件分组。</summary>
    [CustomEditor(typeof(UIManager))]
    [CanEditMultipleObjects]
    public sealed class UIManagerEditor : InspectorGroupsEditor<UIManager>
    {
    }
}
