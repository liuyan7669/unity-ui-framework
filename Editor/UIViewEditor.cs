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
        private GUIStyle settingsLayoutStyle;
        private System.IDisposable optionalRenderer;
        private System.Action<UnityEditor.Editor, string[]> drawAttributedProperties;
        private static readonly string[] ExcludedProperties = { "m_Script", "layer", "backgroundPanel" };

        /// <summary>通过可选入口接入 Inspector Groups，不建立程序集或包依赖。</summary>
        private void OnEnable()
        {
            System.Type rendererType = System.Type.GetType(
                "Cowart.InspectorGroups.Editor.InspectorGroupRenderer, Cowart.InspectorGroups.Editor", false);
            if (rendererType == null) return;
            System.Reflection.MethodInfo draw = rendererType.GetMethod("DrawProperties",
                new[] { typeof(UnityEditor.Editor), typeof(string[]) });
            if (draw == null || !typeof(System.IDisposable).IsAssignableFrom(rendererType)) return;
            optionalRenderer = (System.IDisposable)System.Activator.CreateInstance(rendererType);
            drawAttributedProperties = (System.Action<UnityEditor.Editor, string[]>)System.Delegate.CreateDelegate(
                typeof(System.Action<UnityEditor.Editor, string[]>), optionalRenderer, draw);
        }

        private void OnDisable()
        {
            optionalRenderer?.Dispose();
            optionalRenderer = null;
            drawAttributedProperties = null;
        }

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
            if (drawAttributedProperties != null)
                drawAttributedProperties(this, ExcludedProperties);
            else
                DrawPropertiesExcluding(serializedObject, ExcludedProperties);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>在独立的深色区域中绘制 UIView 基础设置。</summary>
        private void DrawUIViewSettings()
        {
            if (settingsLayoutStyle == null)
            {
                settingsLayoutStyle = new GUIStyle
                {
                    padding = new RectOffset(GUI.skin.box.padding.left, GUI.skin.box.padding.right,
                        6, 6),
                    margin = new RectOffset(GUI.skin.box.margin.left, GUI.skin.box.margin.right,
                        GUI.skin.box.margin.top, GUI.skin.box.margin.bottom)
                };
            }
            Rect background = EditorGUILayout.BeginVertical(settingsLayoutStyle);
            // 两侧保留相同边距，使用 Unity 原生圆角框将折叠箭头与内容一起包裹。
            background.xMin = 6f;
            background.xMax = EditorGUIUtility.currentViewWidth - 6f;
            if (Event.current.type == EventType.Repaint)
                GUI.Box(background, GUIContent.none, EditorStyles.helpBox);
            // 按组件实例保存，避免 Editor 重建或切换选中对象时丢失、串用折叠状态。
            string stateKey = $"Cowart.UIFramework.UIView.Settings:{target.GetInstanceID()}";
            bool showUIViewSettings = SessionState.GetBool(stateKey, false);
            bool expanded = EditorGUILayout.Foldout(showUIViewSettings, "UIView 设置", true);
            if (expanded != showUIViewSettings)
            {
                SessionState.SetBool(stateKey, expanded);
                showUIViewSettings = expanded;
            }

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
