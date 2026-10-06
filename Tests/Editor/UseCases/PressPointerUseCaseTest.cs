using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using MouseAction = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.MouseAction;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class PressPointerUseCaseTest
    {
        [Test]
        public void ExecuteAsync_Presses_AtResolvedPosition_OnMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations { TargetCenterToReturn = (320f, 180f) };
            var resolver = new PointerPositionResolver(dispatcher, pointerTargetOps);
            var useCase = new PressPointerUseCase(dispatcher, resolver, ops);

            // Act
            var response = useCase.ExecuteAsync(new PointerPosition.Target(12345), MouseButton.Middle,
                CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            var record = ops.MouseEventHistory[0];
            Assert.AreEqual(1, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseAction.Press, record.Action);
            Assert.AreEqual(320f, record.X);
            Assert.AreEqual(180f, record.Y);
            Assert.AreEqual(MouseButton.Middle, record.Button);
            Assert.AreEqual(2, dispatcher.CallCount);
            Assert.IsTrue(response.success);
            Assert.AreEqual(320f, response.x);
            Assert.AreEqual(180f, response.y);
        }
    }
}
