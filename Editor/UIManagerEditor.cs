#if UNITY_EDITOR
using UnityEditor;

namespace Cowart.UIFramework.Editor
{
    /// <summary>绘制 UIManager 的 Inspector，隐藏脚本引用行。</summary>
    [CustomEditor(typeof(UIManager))]
    [CanEditMultipleObjects]
    public sealed class UIManagerEditor : UnityEditor.Editor
    {
        /// <summary>绘制除脚本引用外的序列化字段。</summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
