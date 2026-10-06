using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class PointerPositionResolverTest
    {
        [Test]
        public void ResolveAsync_ReturnsCoordinates_AsTheyAre()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var uiPointerTargetOps = new SpyUiPointerTargetOperations();
            var resolver = new PointerPositionResolver(dispatcher, uiPointerTargetOps);

            // Act
            var (x, y) = resolver.ResolveAsync(new PointerPosition.Coordinates(100f, 200f), CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(100f, x);
            Assert.AreEqual(200f, y);
            Assert.AreEqual(0, uiPointerTargetOps.GetTargetCenterCallCount);
            Assert.AreEqual(0, dispatcher.CallCount);
        }

        [Test]
        public void ResolveAsync_ReturnsTargetCenter_OnMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var uiPointerTargetOps = new SpyUiPointerTargetOperations { TargetCenterToReturn = (320f, 180f) };
            var resolver = new PointerPositionResolver(dispatcher, uiPointerTargetOps);

            // Act
            var (x, y) = resolver.ResolveAsync(new PointerPosition.Target(12345), CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(320f, x);
            Assert.AreEqual(180f, y);
            Assert.AreEqual(12345, uiPointerTargetOps.LastInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
