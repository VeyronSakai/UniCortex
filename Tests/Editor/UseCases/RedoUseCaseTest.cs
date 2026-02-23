using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RedoUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsPerformRedo_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var undo = new SpyUndo();
            var useCase = new RedoUseCase(dispatcher, undo);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, undo.PerformRedoCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
