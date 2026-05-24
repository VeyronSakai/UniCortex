using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetScriptableObjectPropertyUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetProperty_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyScriptableObjectOperations();
            var useCase = new SetScriptableObjectPropertyUseCase(dispatcher, ops);

            useCase.ExecuteAsync("Assets/Data/MyData.asset", "m_Speed", "2.5", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SetPropertyCallCount);
            Assert.AreEqual("Assets/Data/MyData.asset", ops.LastSetPropertyAssetPath);
            Assert.AreEqual("m_Speed", ops.LastSetPropertyPath);
            Assert.AreEqual("2.5", ops.LastSetPropertyValue);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
