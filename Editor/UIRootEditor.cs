#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Cowart.UIFramework.Editor
{
    /// <summary>为 UIRoot 提供带中文用途说明的分区 Inspector。</summary>
    [CustomEditor(typeof(UIRoot))]
    [CanEditMultipleObjects]
    public sealed class UIRootEditor : UnityEditor.Editor
    {
        private SerializedProperty scriptProperty;
        private SerializedProperty dontDestroyOnLoadProperty;
        private SerializedProperty uiCameraProperty;
        private SerializedProperty uiCanvasProperty;
        private SerializedProperty backgroundLayerProperty;
        private SerializedProperty normalLayerProperty;
        private SerializedProperty fixedLayerProperty;
        private SerializedProperty popupLayerProperty;
        private SerializedProperty guideLayerProperty;
        private SerializedProperty topLayerProperty;
        private GUIStyle descriptionStyle;

        /// <summary>缓存 UIRoot 的序列化字段。</summary>
        private void OnEnable()
        {
            scriptProperty = serializedObject.FindProperty("m_Script");
            dontDestroyOnLoadProperty = serializedObject.FindProperty("dontDestroyOnLoad");
            uiCameraProperty = serializedObject.FindProperty("uiCamera");
            uiCanvasProperty = serializedObject.FindProperty("uiCanvas");
            backgroundLayerProperty = serializedObject.FindProperty("backgroundLayer");
            normalLayerProperty = serializedObject.FindProperty("normalLayer");
            fixedLayerProperty = serializedObject.FindProperty("fixedLayer");
            popupLayerProperty = serializedObject.FindProperty("popupLayer");
            guideLayerProperty = serializedObject.FindProperty("guideLayer");
            topLayerProperty = serializedObject.FindProperty("topLayer");
        }

        /// <summary>绘制基础设置和从下到上的 UI 层级说明。</summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProperty);
            }

            GUILayout.Space(4f);
            DrawBasicSettings();
            GUILayout.Space(4f);
            DrawLayerSettings();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>绘制 UIRoot 的基础引用。</summary>
        private void DrawBasicSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("基础设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                dontDestroyOnLoadProperty,
                new GUIContent("切换场景时保留", "勾选后，运行时切换场景不会销毁整个 UI 根节点。"));
            EditorGUILayout.PropertyField(
                uiCameraProperty,
                new GUIContent("UI 相机", "专门用于渲染 UGUI 的相机。"));
            EditorGUILayout.PropertyField(
                uiCanvasProperty,
                new GUIContent("主画布", "承载全部 UI 层级的主 Canvas。"));
            EditorGUILayout.EndVertical();
        }

        /// <summary>绘制全部 UI 层级引用及推荐用途。</summary>
        private void DrawLayerSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("界面层级（从下到上）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "越靠后的层通常显示得越靠上。以下是推荐用途，层级本身主要用于区分父节点和显示顺序。",
                MessageType.Info);

            DrawLayer(
                backgroundLayerProperty,
                "背景层（Background）",
                "放全屏背景、场景底图和不会遮挡主界面的装饰内容。");
            DrawLayer(
                normalLayerProperty,
                "普通层（Normal）",
                "放主界面、功能页面等常规面板；大多数 UIView 都放在这一层。");
            DrawLayer(
                fixedLayerProperty,
                "固定层（Fixed）",
                "放需要常驻在普通页面上方的 HUD、状态栏、底部导航等固定界面。");
            DrawLayer(
                popupLayerProperty,
                "弹窗层（Popup）",
                "放确认框、奖励窗口、二级弹窗等需要覆盖常规界面的内容。");
            DrawLayer(
                guideLayerProperty,
                "引导层（Guide）",
                "放新手引导遮罩、指示手势和步骤提示，确保引导内容显示在弹窗之上。");
            DrawLayer(
                topLayerProperty,
                "顶层（Top）",
                "放必须压住其他界面的全局加载遮罩、断线提示或最高优先级内容。");

            EditorGUILayout.EndVertical();
        }

        /// <summary>绘制单个层级引用和用途说明。</summary>
        /// <param name="property">层级引用字段。</param>
        /// <param name="label">层级中文名称。</param>
        /// <param name="description">推荐放置的界面类型。</param>
        private void DrawLayer(SerializedProperty property, string label, string description)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label));
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField(description, DescriptionStyle);
            }
            GUILayout.Space(2f);
        }

        /// <summary>获取支持自动换行的次要说明文字样式。</summary>
        private GUIStyle DescriptionStyle
        {
            get
            {
                if (descriptionStyle == null)
                {
                    descriptionStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        wordWrap = true
                    };
                }

                return descriptionStyle;
            }
        }
    }
}
#endif
