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
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);

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
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);

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
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);

            var ex = Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync(new RunTestsRequest(TestModes.EditMode), CancellationToken.None)
                    .GetAwaiter().GetResult());

            StringAssert.Contains("Cannot run tests during play mode", ex.Message);
            Assert.AreEqual(0, spy.StartCallCount);
        }
    }
}
