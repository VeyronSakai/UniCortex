using System;
using System.Threading;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SelectProjectWindowAssetUseCaseTest
    {
        private static readonly Type s_projectBrowserType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");

        [Test]
        public void ExecuteAsync_CallsSelectAsset_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProjectWindowOperations();
            var useCase = new SelectProjectWindowAssetUseCase(dispatcher, operations);

            useCase.ExecuteAsync("Assets/Scenes/SampleScene.unity", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.SelectAssetCallCount);
            Assert.AreEqual("Assets/Scenes/SampleScene.unity", operations.LastSelectedAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_OpensProjectWindow_WhenClosed()
        {
            Assert.That(s_projectBrowserType, Is.Not.Null);

            CloseAllProjectBrowsers();
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new ProjectWindowOperationsAdapter();
            var useCase = new SelectProjectWindowAssetUseCase(dispatcher, operations);

            useCase.ExecuteAsync("Assets/Scenes/SampleScene.unity", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.That(Resources.FindObjectsOfTypeAll(s_projectBrowserType), Is.Not.Empty);
            Assert.That(Selection.activeObject, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(Selection.activeObject), Is.EqualTo("Assets/Scenes/SampleScene.unity"));
        }

        private static void CloseAllProjectBrowsers()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll(s_projectBrowserType))
            {
                ((EditorWindow)window).Close();
            }
        }
    }
}
