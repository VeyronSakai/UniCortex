using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CreateScriptableObjectUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsCreate_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations();
            var useCase = new CreateScriptableObjectUseCase(dispatcher, operations);

            useCase.ExecuteAsync("TestConfig", "Assets/TestConfig.asset", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.CreateCallCount);
            Assert.AreEqual("TestConfig", operations.LastCreateType);
            Assert.AreEqual("Assets/TestConfig.asset", operations.LastCreateAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
