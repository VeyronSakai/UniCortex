using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using MouseAction = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.MouseAction;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class ClickPointerUseCaseTest
    {
        [Test]
        public void ExecuteAsync_PressesAndReleases_AtCoordinates()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var resolver = new PointerPositionResolver(dispatcher, new SpyPointerTargetOperations());
            var useCase = new ClickPointerUseCase(dispatcher, resolver, ops);

            // Act
            var response = useCase.ExecuteAsync(new PointerPosition.Coordinates(150f, 250f), MouseButton.Right,
                CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseAction.Press, ops.MouseEventHistory[0].Action);
            Assert.AreEqual(MouseAction.Release, ops.MouseEventHistory[1].Action);
            foreach (var record in ops.MouseEventHistory)
            {
                Assert.AreEqual(150f, record.X);
                Assert.AreEqual(250f, record.Y);
                Assert.AreEqual(MouseButton.Right, record.Button);
            }

            Assert.AreEqual(2, dispatcher.CallCount);
            Assert.IsTrue(response.success);
            Assert.AreEqual(150f, response.x);
            Assert.AreEqual(250f, response.y);
        }

        [Test]
        public void ExecuteAsync_PressesAndReleases_AtTargetCenter()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations { TargetCenterToReturn = (40f, 60f) };
            var resolver = new PointerPositionResolver(dispatcher, pointerTargetOps);
            var useCase = new ClickPointerUseCase(dispatcher, resolver, ops);

            // Act
            var response = useCase.ExecuteAsync(new PointerPosition.Target(12345), MouseButton.Left,
                CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(12345, pointerTargetOps.LastInstanceId);
            Assert.AreEqual(2, ops.MouseEventHistory.Count);
            Assert.AreEqual(40f, ops.MouseEventHistory[1].X);
            Assert.AreEqual(60f, ops.MouseEventHistory[1].Y);
            Assert.AreEqual(40f, response.x);
            Assert.AreEqual(60f, response.y);
        }
    }
}
