using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class ModifyTimelineClipUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsModifyClip_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new ModifyTimelineClipUseCase(dispatcher, ops);
            var request = new ModifyTimelineClipRequest { instanceId = 12345, trackIndex = 1, clipIndex = 2, start = 0.5 };

            // Act
            useCase.ExecuteAsync(request, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, ops.ModifyClipCallCount);
            Assert.AreSame(request, ops.LastModifyClipRequest);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
