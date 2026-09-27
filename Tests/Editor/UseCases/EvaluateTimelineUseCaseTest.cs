using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class EvaluateTimelineUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsEvaluate_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new EvaluateTimelineUseCase(dispatcher, ops);

            // Act
            useCase.ExecuteAsync(12345, 1.5, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, ops.EvaluateCallCount);
            Assert.AreEqual(12345, ops.LastEvaluateInstanceId);
            Assert.AreEqual(1.5, ops.LastEvaluateTime);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
