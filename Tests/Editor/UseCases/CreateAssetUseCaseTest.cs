using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CreateAssetUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsCreateAsset_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new CreateAssetUseCase(dispatcher, operations);

            useCase.ExecuteAsync("Material", "Assets/Materials/Test.mat", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.CreateAssetCallCount);
            Assert.AreEqual("Material", operations.LastCreateType);
            Assert.AreEqual("Assets/Materials/Test.mat", operations.LastCreateAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
