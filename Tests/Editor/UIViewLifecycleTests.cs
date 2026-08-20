using System.Reflection;
using Cowart.UIFramework;
using NUnit.Framework;
using UnityEngine;

namespace Cowart.UIFramework.Tests
{
    public sealed class UIViewLifecycleTests
    {
        private static readonly MethodInfo OpenMethod = typeof(UIView).GetMethod(
            "Open",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo CloseMethod = typeof(UIView).GetMethod(
            "Close",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private GameObject parent;

        [SetUp]
        public void SetUp()
        {
            parent = new GameObject("Parent", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            if (parent != null)
            {
                Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void Open_ActivatesBeforeCallback_MovesToLastSibling_AndPassesArgs()
        {
            UIViewLifecycleProbe view = CreateView();
            GameObject trailingSibling = new GameObject("TrailingSibling", typeof(RectTransform));
            trailingSibling.transform.SetParent(parent.transform, false);
            view.gameObject.SetActive(false);
            UIArgs args = new UIViewLifecycleArgs();

            Open(view, args);

            Assert.IsTrue(view.gameObject.activeSelf);
            Assert.IsTrue(view.gameObject.activeInHierarchy);
            Assert.AreEqual(1, view.OpenCount);
            Assert.IsTrue(view.WasActiveSelfOnOpen);
            Assert.IsTrue(view.WasActiveInHierarchyOnOpen);
            Assert.AreEqual(parent.transform.childCount - 1, view.SiblingIndexOnOpen);
            Assert.AreSame(args, view.LastArgs);
        }

        [Test]
        public void Open_WhenAlreadyActive_InvokesCallbackAgainForRefresh()
        {
            UIViewLifecycleProbe view = CreateView();
            UIArgs firstArgs = new UIViewLifecycleArgs();
            UIArgs secondArgs = new UIViewLifecycleArgs();

            Open(view, firstArgs);
            Open(view, secondArgs);

            Assert.AreEqual(2, view.OpenCount);
            Assert.AreSame(secondArgs, view.LastArgs);
        }

        [Test]
        public void Close_CallsBeforeDeactivation_AndDoesNotRepeatWhenAlreadyClosed()
        {
            UIViewLifecycleProbe view = CreateView();

            Close(view);
            Close(view);

            Assert.AreEqual(1, view.CloseCount);
            Assert.IsTrue(view.WasActiveSelfOnClose);
            Assert.IsTrue(view.WasActiveInHierarchyOnClose);
            Assert.IsFalse(view.gameObject.activeSelf);
            Assert.IsFalse(view.gameObject.activeInHierarchy);
        }

        [Test]
        public void InactiveParent_SuppressesCallbacks_ButStillUpdatesActiveSelf()
        {
            UIViewLifecycleProbe view = CreateView();
            view.gameObject.SetActive(false);
            parent.SetActive(false);

            Open(view, new UIViewLifecycleArgs());

            Assert.IsTrue(view.gameObject.activeSelf);
            Assert.IsFalse(view.gameObject.activeInHierarchy);
            Assert.AreEqual(0, view.OpenCount);

            Close(view);
            Close(view);

            Assert.IsFalse(view.gameObject.activeSelf);
            Assert.AreEqual(0, view.CloseCount);
        }

        private UIViewLifecycleProbe CreateView()
        {
            GameObject viewObject = new GameObject("View", typeof(RectTransform));
            viewObject.transform.SetParent(parent.transform, false);
            return viewObject.AddComponent<UIViewLifecycleProbe>();
        }

        private static void Open(UIView view, UIArgs args)
        {
            Assert.IsNotNull(OpenMethod, "找不到 UIView.Open 内部方法。");
            OpenMethod.Invoke(view, new object[] { args });
        }

        private static void Close(UIView view)
        {
            Assert.IsNotNull(CloseMethod, "找不到 UIView.Close 内部方法。");
            CloseMethod.Invoke(view, null);
        }

    }

    public sealed class UIViewLifecycleProbe : UIView
    {
        public int OpenCount { get; private set; }
        public int CloseCount { get; private set; }
        public bool WasActiveSelfOnOpen { get; private set; }
        public bool WasActiveInHierarchyOnOpen { get; private set; }
        public bool WasActiveSelfOnClose { get; private set; }
        public bool WasActiveInHierarchyOnClose { get; private set; }
        public int SiblingIndexOnOpen { get; private set; }
        public UIArgs LastArgs { get; private set; }

        protected override void OnOpen(UIArgs args)
        {
            OpenCount++;
            WasActiveSelfOnOpen = gameObject.activeSelf;
            WasActiveInHierarchyOnOpen = gameObject.activeInHierarchy;
            SiblingIndexOnOpen = transform.GetSiblingIndex();
            LastArgs = args;
        }

        protected override void OnClose()
        {
            CloseCount++;
            WasActiveSelfOnClose = gameObject.activeSelf;
            WasActiveInHierarchyOnClose = gameObject.activeInHierarchy;
        }
    }

    public sealed class UIViewLifecycleArgs : UIArgs
    {
    }
}
