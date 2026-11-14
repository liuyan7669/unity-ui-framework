using UnityEditor;
using Cowart.InspectorGroups.Editor;

namespace Cowart.UIFramework.Editor
{
    [CustomEditor(typeof(UIView), true)]
    [CanEditMultipleObjects]
    public sealed class UIViewEditor : InspectorGroupsEditor<UIView>
    {
    }
}
