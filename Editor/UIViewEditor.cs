#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Cowart.UIFramework;

namespace Cowart.UIFramework.Editor
{
    /// <summary>将 UIView 字段与子类字段分组显示。</summary>
    [CustomEditor(typeof(UIView), true)]
    [CanEditMultipleObjects]
    public sealed class UIViewEditor : UnityEditor.Editor
    {
        /// <summary>是否展开 UIView 的基础设置。</summary>
        private bool showUIViewSettings = true;

        /// <summary>绘制 UIView 基础字段和子类字段。</summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            DrawUIViewSettings();

            EditorGUILayout.Space();
            DrawPropertiesExcluding(serializedObject, "m_Script", "layer", "backgroundPanel");

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>在独立的深色区域中绘制 UIView 基础设置。</summary>
        private void DrawUIViewSettings()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showUIViewSettings = EditorGUILayout.Foldout(showUIViewSettings, "UIView 设置", true);

            if (showUIViewSettings)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("layer"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundPanel"));
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
