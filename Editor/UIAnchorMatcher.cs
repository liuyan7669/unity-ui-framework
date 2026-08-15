#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cowart.UIFramework.Editor
{
    /// <summary>
    /// 提供将选中 UI 锚点匹配到当前矩形四角的编辑器菜单。
    /// </summary>
    public static class UIAnchorMatcher
    {
        private const string ToolbarMenuPath = "Cowart/UI/锚点匹配四角";
        private const string GameObjectMenuPath = "GameObject/Cowart/UI/锚点匹配四角";
        private const string UndoName = "Match UI Anchors To Corners";
        private const int UiMenuGroupPriority = 100;

        /// <summary>
        /// 从顶部 Cowart 菜单匹配全部选中 UI 的锚点。
        /// </summary>
        [MenuItem(ToolbarMenuPath, false, UiMenuGroupPriority)]
        private static void MatchFromToolbar()
        {
            MatchSelectedAnchorsToCorners();
        }

        /// <summary>验证顶部 Cowart 菜单。</summary>
        [MenuItem(ToolbarMenuPath, true)]
        private static bool ValidateMatchFromToolbar()
        {
            return GetValidTargets().Length > 0;
        }

        /// <summary>
        /// 从物体右键菜单匹配全部选中 UI 的锚点。
        /// </summary>
        [MenuItem(GameObjectMenuPath, false, UiMenuGroupPriority)]
        private static void MatchFromGameObjectMenu()
        {
            MatchSelectedAnchorsToCorners();
        }

        /// <summary>验证物体右键菜单。</summary>
        [MenuItem(GameObjectMenuPath, true)]
        private static bool ValidateMatchFromGameObjectMenu()
        {
            return GetValidTargets().Length > 0;
        }

        /// <summary>
        /// 将所有选中 UI 的锚点移动到当前矩形四角，并保持视觉位置和尺寸不变。
        /// </summary>
        private static void MatchSelectedAnchorsToCorners()
        {
            RectTransform[] targets = GetValidTargets();
            if (targets.Length == 0)
            {
                return;
            }

            Undo.SetCurrentGroupName(UndoName);
            int undoGroup = Undo.GetCurrentGroup();

            try
            {
                Undo.RecordObjects(targets, UndoName);

                for (int i = 0; i < targets.Length; i++)
                {
                    MatchAnchorsToCorners(targets[i]);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targets[i]);
                    EditorUtility.SetDirty(targets[i]);

                    if (targets[i].gameObject.scene.IsValid())
                    {
                        EditorSceneManager.MarkSceneDirty(targets[i].gameObject.scene);
                    }
                }

                Undo.CollapseUndoOperations(undoGroup);
                SceneView.RepaintAll();
            }
            catch
            {
                Undo.RevertAllDownToGroup(undoGroup);
                throw;
            }
        }

        /// <summary>
        /// 获取当前选择中具有有效 RectTransform 父节点的全部 UI。
        /// </summary>
        private static RectTransform[] GetValidTargets()
        {
            Transform[] selectedTransforms = Selection.transforms;
            List<RectTransform> targets = new List<RectTransform>(selectedTransforms.Length);

            for (int i = 0; i < selectedTransforms.Length; i++)
            {
                RectTransform rectTransform = selectedTransforms[i] as RectTransform;
                if (CanMatchAnchors(rectTransform))
                {
                    targets.Add(rectTransform);
                }
            }

            return targets.ToArray();
        }

        /// <summary>
        /// 检查目标及其父矩形是否允许计算锚点。
        /// </summary>
        private static bool CanMatchAnchors(RectTransform rectTransform)
        {
            if (rectTransform == null || EditorUtility.IsPersistent(rectTransform))
            {
                return false;
            }

            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent == null)
            {
                return false;
            }

            Vector2 parentSize = parent.rect.size;
            return !Mathf.Approximately(parentSize.x, 0f) &&
                   !Mathf.Approximately(parentSize.y, 0f);
        }

        /// <summary>
        /// 使用当前偏移计算新锚点，并将四边偏移归零。
        /// </summary>
        private static void MatchAnchorsToCorners(RectTransform rectTransform)
        {
            RectTransform parent = rectTransform.parent as RectTransform;
            Vector2 parentSize = parent.rect.size;
            Vector2 anchorMin = rectTransform.anchorMin;
            Vector2 anchorMax = rectTransform.anchorMax;
            Vector2 offsetMin = rectTransform.offsetMin;
            Vector2 offsetMax = rectTransform.offsetMax;

            anchorMin.x += offsetMin.x / parentSize.x;
            anchorMin.y += offsetMin.y / parentSize.y;
            anchorMax.x += offsetMax.x / parentSize.x;
            anchorMax.y += offsetMax.y / parentSize.y;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
#endif
