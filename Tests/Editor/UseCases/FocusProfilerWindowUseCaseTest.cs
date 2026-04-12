using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class FocusProfilerWindowUseCaseTest
    {
        [Test]
        public void ExecuteAsync_FocusesProfilerWindow_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProfilerOperations();
            var useCase = new FocusProfilerWindowUseCase(dispatcher, operations);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.FocusProfilerWindowCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
