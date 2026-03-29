using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class FocusSceneViewUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsFocusSceneView_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new FocusSceneViewUseCase(dispatcher, operations);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.FocusSceneViewCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
