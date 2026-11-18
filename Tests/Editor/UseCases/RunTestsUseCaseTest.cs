using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RunTestsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_StartsRunWithParameters()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp, new SpyEditorSceneManager());

            useCase.ExecuteAsync(new RunTestsRequest(TestModes.PlayMode), CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, spy.StartCallCount);
            Assert.AreEqual(TestModes.PlayMode, spy.LastTestMode);
        }

        [Test]
        public void ExecuteAsync_PassesNewFilterFieldsToTestRunner()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp, new SpyEditorSceneManager());

            var request = new RunTestsRequest(
                TestModes.EditMode,
                testNames: new List<string> { "TestA", "TestB" },
                groupNames: new List<string> { "Group1" },
                categoryNames: new List<string> { "Smoke" },
                assemblyNames: new List<string> { "MyAssembly" });

            useCase.ExecuteAsync(request, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, spy.StartCallCount);
            Assert.AreEqual(new List<string> { "TestA", "TestB" }, spy.LastRequest.testNames);
            Assert.AreEqual(new List<string> { "Group1" }, spy.LastRequest.groupNames);
            Assert.AreEqual(new List<string> { "Smoke" }, spy.LastRequest.categoryNames);
            Assert.AreEqual(new List<string> { "MyAssembly" }, spy.LastRequest.assemblyNames);
        }

        [Test]
        public void ExecuteAsync_ThrowsInvalidOperationException_WhenInPlayMode()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp, new SpyEditorSceneManager());

            var ex = Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync(new RunTestsRequest(TestModes.EditMode), CancellationToken.None)
                    .GetAwaiter().GetResult());

            StringAssert.Contains("Cannot run tests during play mode", ex.Message);
            Assert.AreEqual(0, spy.StartCallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsUnsavedSceneChangesException_WhenSceneHasUnsavedChanges()
        {
            // Arrange
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager
            {
                DirtyScenePaths = new List<string> { "Assets/Scenes/Main.unity" }
            };
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp, sceneManager);

            // Act
            var ex = Assert.Throws<UnsavedSceneChangesException>(() =>
                useCase.ExecuteAsync(new RunTestsRequest(TestModes.EditMode), CancellationToken.None)
                    .GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Cannot run tests", ex.Message);
            StringAssert.Contains("Assets/Scenes/Main.unity", ex.Message);
            StringAssert.Contains("save (CLI: editor save)", ex.Message);
            StringAssert.Contains("discard_scene_changes", ex.Message);
            Assert.AreEqual(0, spy.StartCallCount);
            Assert.AreEqual(0, sceneManager.DiscardChangesCallCount);
        }

        [Test]
        public void ExecuteAsync_SuggestsSaveSceneAs_WhenUntitledSceneHasUnsavedChanges()
        {
            // Arrange
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager { DirtyScenePaths = new List<string> { "" } };
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp, sceneManager);

            // Act
            var ex = Assert.Throws<UnsavedSceneChangesException>(() =>
                useCase.ExecuteAsync(new RunTestsRequest(TestModes.EditMode), CancellationToken.None)
                    .GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Untitled", ex.Message);
            StringAssert.Contains("save_scene_as (CLI: scene save-as)", ex.Message);
            StringAssert.DoesNotContain("discard_scene_changes", ex.Message);
            StringAssert.DoesNotContain("save (CLI: editor save)", ex.Message);
            Assert.AreEqual(0, spy.StartCallCount);
        }
    }
}
