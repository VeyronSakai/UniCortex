using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class ClosePrefabUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsClosePrefab_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new ClosePrefabUseCase(dispatcher, operations);

            useCase.ExecuteAsync(CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.ClosePrefabCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
