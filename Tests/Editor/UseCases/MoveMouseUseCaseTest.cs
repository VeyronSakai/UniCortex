using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using MouseAction = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.MouseAction;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class MoveMouseUseCaseTest
    {
        [Test]
        public void ExecuteAsync_Moves_OnMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var resolver = new PointerPositionResolver(dispatcher, new SpyUiPointerTargetOperations());
            var useCase = new MoveMouseUseCase(dispatcher, resolver, ops);

            // Act
            var response = useCase.ExecuteAsync(new PointerPosition.Coordinates(0f, 0f), CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            var record = ops.MouseEventHistory[0];
            Assert.AreEqual(1, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseAction.Move, record.Action);
            Assert.AreEqual(0f, record.X);
            Assert.AreEqual(0f, record.Y);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.IsTrue(response.success);
        }
    }
}
