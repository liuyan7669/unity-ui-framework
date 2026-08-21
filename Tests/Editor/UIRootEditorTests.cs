using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Cowart.UIFramework.Tests
{
    /// <summary>验证 UIRoot 使用包含全部序列化字段的自定义 Inspector。</summary>
    public sealed class UIRootEditorTests
    {
        /// <summary>确保自定义 Inspector 已注册并能找到全部展示字段。</summary>
        [Test]
        public void CreateEditor_UsesUIRootEditorAndCachesAllProperties()
        {
            GameObject rootObject = new GameObject("UIRootEditorTest");
            UIRoot root = rootObject.AddComponent<UIRoot>();
            UnityEditor.Editor editor = null;

            try
            {
                editor = UnityEditor.Editor.CreateEditor(root);

                Assert.AreEqual("Cowart.UIFramework.Editor.UIRootEditor", editor.GetType().FullName);

                string[] propertyFieldNames =
                {
                    "scriptProperty",
                    "dontDestroyOnLoadProperty",
                    "uiCameraProperty",
                    "uiCanvasProperty",
                    "backgroundLayerProperty",
                    "normalLayerProperty",
                    "fixedLayerProperty",
                    "popupLayerProperty",
                    "guideLayerProperty",
                    "topLayerProperty"
                };

                for (int i = 0; i < propertyFieldNames.Length; i++)
                {
                    FieldInfo field = editor.GetType().GetField(
                        propertyFieldNames[i],
                        BindingFlags.Instance | BindingFlags.NonPublic);

                    Assert.IsNotNull(field, "找不到 Inspector 字段：" + propertyFieldNames[i]);
                    Assert.IsNotNull(field.GetValue(editor), "Inspector 未缓存字段：" + propertyFieldNames[i]);
                }
            }
            finally
            {
                if (editor != null)
                {
                    Object.DestroyImmediate(editor);
                }

                Object.DestroyImmediate(rootObject);
            }
        }
    }
}
