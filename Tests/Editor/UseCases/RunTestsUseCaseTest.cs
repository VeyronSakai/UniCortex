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
        public void ExecuteAsync_AggregatesResults()
        {
            var spy = new SpyTestRunner(new List<TestResultItem>
            {
                new TestResultItem("Test1", "Passed", 0.1f),
                new TestResultItem("Test2", "Failed", 0.2f, "assertion error"),
                new TestResultItem("Test3", "Passed", 0.05f),
                new TestResultItem("Test4", "Skipped", 0f),
            });
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);

            var result = useCase.ExecuteAsync(new RunTestsRequest(TestModes.EditMode), CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(2, result.passed);
            Assert.AreEqual(1, result.failed);
            Assert.AreEqual(1, result.skipped);
            Assert.AreEqual(4, result.results.Count);
        }

        [Test]
        public void ExecuteAsync_PassesParametersToTestRunner()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);

            useCase.ExecuteAsync(new RunTestsRequest(TestModes.PlayMode), CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, spy.RunTestsCallCount);
            Assert.AreEqual(TestModes.PlayMode, spy.LastTestMode);
        }

        [Test]
        public void ExecuteAsync_WithEmptyResults_ReturnsZeroCounts()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);

            var result = useCase.ExecuteAsync(new RunTestsRequest(TestModes.EditMode), CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(0, result.passed);
            Assert.AreEqual(0, result.failed);
            Assert.AreEqual(0, result.skipped);
            Assert.AreEqual(0, result.results.Count);
        }

        [Test]
        public void ExecuteAsync_PassesNewFilterFieldsToTestRunner()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);

            var request = new RunTestsRequest(
                TestModes.EditMode,
                testNames: new List<string> { "TestA", "TestB" },
                groupNames: new List<string> { "Group1" },
                categoryNames: new List<string> { "Smoke" },
                assemblyNames: new List<string> { "MyAssembly" });

            useCase.ExecuteAsync(request, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, spy.RunTestsCallCount);
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
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);

            var ex = Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync(new RunTestsRequest(TestModes.EditMode), CancellationToken.None)
                    .GetAwaiter().GetResult());

            StringAssert.Contains("Cannot run tests during play mode", ex.Message);
            Assert.AreEqual(0, spy.RunTestsCallCount);
        }
    }
}
