using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class DeleteGameObjectUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsDelete_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new DeleteGameObjectUseCase(dispatcher, ops);

            useCase.ExecuteAsync(456, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.DeleteCallCount);
            Assert.AreEqual(456, ops.LastDeleteInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
