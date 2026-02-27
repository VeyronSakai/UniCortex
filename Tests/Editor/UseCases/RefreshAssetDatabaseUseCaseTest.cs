using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RefreshAssetDatabaseUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsRefresh_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new RefreshAssetDatabaseUseCase(dispatcher, operations);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.RefreshCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
