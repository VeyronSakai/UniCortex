using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RemoveComponentUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsRemoveComponent_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new RemoveComponentUseCase(dispatcher, ops);

            useCase.ExecuteAsync(456, "Rigidbody", 0, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.RemoveComponentCallCount);
            Assert.AreEqual(456, ops.LastRemoveComponentInstanceId);
            Assert.AreEqual("Rigidbody", ops.LastRemoveComponentType);
            Assert.AreEqual(0, ops.LastRemoveComponentIndex);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
