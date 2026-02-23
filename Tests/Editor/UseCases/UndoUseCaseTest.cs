using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class UndoUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsPerformUndo_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var undo = new SpyUndo();
            var useCase = new UndoUseCase(dispatcher, undo);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, undo.PerformUndoCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
