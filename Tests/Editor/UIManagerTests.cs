using System;
using System.Collections.Generic;
using System.Reflection;
using Cowart.UIFramework;
using NUnit.Framework;
using UnityEngine;

namespace Cowart.UIFramework.Tests
{
    public sealed class UIManagerTests
    {
        private static readonly FieldInfo ManagerInstanceField = typeof(UIManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo ManagerRootField = typeof(UIManager).GetField("root", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo RootDontDestroyField = typeof(UIRoot).GetField("dontDestroyOnLoad", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo RootInstanceField = typeof(UIRoot).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly List<GameObject> prefabObjects = new List<GameObject>();

        private object previousManagerInstance;
        private object previousRootInstance;
        private GameObject rootObject;

        [SetUp]
        public void SetUp()
        {
            Assert.IsNotNull(ManagerInstanceField);
            Assert.IsNotNull(ManagerRootField);
            Assert.IsNotNull(RootDontDestroyField);
            Assert.IsNotNull(RootInstanceField);

            previousManagerInstance = ManagerInstanceField.GetValue(null);
            previousRootInstance = RootInstanceField.GetValue(null);
            ManagerInstanceField.SetValue(null, null);
            RootInstanceField.SetValue(null, null);

            rootObject = new GameObject("UIManager Test Root", typeof(RectTransform));
            rootObject.SetActive(false);
            UIRoot root = rootObject.AddComponent<UIRoot>();
            UIManager manager = rootObject.AddComponent<UIManager>();

            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(rootObject.transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();

            GameObject normalLayerObject = new GameObject("Normal", typeof(RectTransform));
            normalLayerObject.transform.SetParent(canvasObject.transform, false);
            RectTransform normalLayer = normalLayerObject.GetComponent<RectTransform>();

            root.Configure(null, canvas, null, normalLayer, null, null, null, null);
            RootDontDestroyField.SetValue(root, false);
            ManagerRootField.SetValue(manager, root);
            ManagerInstanceField.SetValue(null, manager);
            RootInstanceField.SetValue(null, root);
            rootObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject prefabObject in prefabObjects)
            {
                if (prefabObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(prefabObject);
                }
            }

            prefabObjects.Clear();
            if (rootObject != null)
            {
                UnityEngine.Object.DestroyImmediate(rootObject);
            }

            ManagerInstanceField.SetValue(null, previousManagerInstance);
            RootInstanceField.SetValue(null, previousRootInstance);
        }

        [Test]
        public void Open_BaseTypedPrefab_ReusesConcreteInstance()
        {
            UIManagerPrimaryTestPanel prefab = CreatePrefab<UIManagerPrimaryTestPanel>("Primary Prefab");
            UIView basePrefab = prefab;

            UIView first = UIManager.Open(basePrefab);
            UIView second = UIManager.Open(basePrefab);
            UIView third = UIManager.Open(basePrefab);

            Assert.AreNotSame(prefab, first);
            Assert.AreSame(first, second);
            Assert.AreSame(first, third);
            Assert.AreSame(first, UIManager.Find<UIManagerPrimaryTestPanel>());
            Assert.AreEqual(1, first.transform.parent.childCount);
        }

        [Test]
        public void Open_ConcreteAndBaseTypedPrefab_ShareConcreteInstance()
        {
            UIManagerPrimaryTestPanel prefab = CreatePrefab<UIManagerPrimaryTestPanel>("Primary Prefab");

            UIManagerPrimaryTestPanel concreteResult = UIManager.Open(prefab);
            UIView baseResult = UIManager.Open((UIView)prefab);

            Assert.AreSame(concreteResult, baseResult);
            Assert.AreSame(concreteResult, UIManager.Open(prefab));
        }

        [Test]
        public void Find_ExactOverloadAndPredicateOverload_KeepDistinctTypeSemantics()
        {
            UIManagerPrimaryTestPanel prefab = CreatePrefab<UIManagerPrimaryTestPanel>("Primary Prefab");
            UIManagerPrimaryTestPanel opened = UIManager.Open(prefab);

            Assert.AreSame(opened, UIManager.Find<UIManagerPrimaryTestPanel>());
            Assert.IsNull(UIManager.Find<UIView>());
            Assert.AreSame(opened, UIManager.Find<UIView>(view => view == opened));

            UIView[] allViews = UIManager.FindAll<UIView>(view => true);
            Assert.AreEqual(1, allViews.Length);
            Assert.AreSame(opened, allViews[0]);
            Assert.Throws<InvalidOperationException>(() => UIManager.Open<UIView>());
        }

        [Test]
        public void Close_ExactOverloadAndPredicateOverload_KeepDistinctTypeSemantics()
        {
            UIManagerPrimaryTestPanel prefab = CreatePrefab<UIManagerPrimaryTestPanel>("Primary Prefab");
            UIManagerPrimaryTestPanel opened = UIManager.Open(prefab);

            UIManager.Close<UIView>();

            Assert.IsTrue(opened.gameObject.activeSelf);
            Assert.AreEqual(0, opened.CloseCount);

            UIManager.Close<UIView>(view => true);

            Assert.IsFalse(opened.gameObject.activeSelf);
            Assert.AreEqual(1, opened.CloseCount);
        }

        [Test]
        public void CloseAll_ClosesSnapshot_RespectsIgnoredTypesAndPreventsReentry()
        {
            UIManagerPrimaryTestPanel primaryPrefab = CreatePrefab<UIManagerPrimaryTestPanel>("Primary Prefab");
            UIManagerSecondaryTestPanel secondaryPrefab = CreatePrefab<UIManagerSecondaryTestPanel>("Secondary Prefab");
            UIManagerPrimaryTestPanel primary = UIManager.Open(primaryPrefab);
            UIManagerSecondaryTestPanel secondary = UIManager.Open(secondaryPrefab);
            primary.Closing = delegate { UIManager.CloseAll(); };

            UIManager.CloseAll(typeof(UIManagerSecondaryTestPanel));

            Assert.IsFalse(primary.gameObject.activeSelf);
            Assert.AreEqual(1, primary.CloseCount);
            Assert.IsTrue(secondary.gameObject.activeSelf);

            UIManager.CloseAll();

            Assert.IsFalse(secondary.gameObject.activeSelf);
            Assert.AreEqual(1, secondary.CloseCount);
        }

        private T CreatePrefab<T>(string objectName) where T : UIView
        {
            GameObject prefabObject = new GameObject(objectName, typeof(RectTransform));
            T prefab = prefabObject.AddComponent<T>();
            prefabObject.SetActive(false);
            prefabObjects.Add(prefabObject);
            return prefab;
        }
    }

    public sealed class UIManagerPrimaryTestPanel : UIView
    {
        public Action Closing { get; set; }

        public int CloseCount { get; private set; }

        protected override void OnClose()
        {
            CloseCount++;
            if (Closing != null)
            {
                Closing();
            }
        }
    }

    public sealed class UIManagerSecondaryTestPanel : UIView
    {
        public int CloseCount { get; private set; }

        protected override void OnClose()
        {
            CloseCount++;
        }
    }
}
