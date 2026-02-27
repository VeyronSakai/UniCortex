using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class AddComponentUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsAddComponent_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new AddComponentUseCase(dispatcher, ops);

            useCase.ExecuteAsync(123, "Rigidbody", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.AddComponentCallCount);
            Assert.AreEqual(123, ops.LastAddComponentInstanceId);
            Assert.AreEqual("Rigidbody", ops.LastAddComponentType);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
