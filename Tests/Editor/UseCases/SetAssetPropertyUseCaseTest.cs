using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetAssetPropertyUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetProperty_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new SetAssetPropertyUseCase(dispatcher, operations);

            useCase.ExecuteAsync("Assets/Test.mat", "m_Name", "NewName", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.SetPropertyCallCount);
            Assert.AreEqual("Assets/Test.mat", operations.LastSetPropertyAssetPath);
            Assert.AreEqual("m_Name", operations.LastSetPropertyPath);
            Assert.AreEqual("NewName", operations.LastSetPropertyValue);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
