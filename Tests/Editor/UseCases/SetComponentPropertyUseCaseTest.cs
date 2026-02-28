using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetComponentPropertyUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetProperty_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new SetComponentPropertyUseCase(dispatcher, ops);

            useCase.ExecuteAsync(123, "UnityEngine.Transform", "m_LocalPosition.x", "1.5", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SetPropertyCallCount);
            Assert.AreEqual(123, ops.LastSetPropertyInstanceId);
            Assert.AreEqual("UnityEngine.Transform", ops.LastSetPropertyComponentType);
            Assert.AreEqual("m_LocalPosition.x", ops.LastSetPropertyPath);
            Assert.AreEqual("1.5", ops.LastSetPropertyValue);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
