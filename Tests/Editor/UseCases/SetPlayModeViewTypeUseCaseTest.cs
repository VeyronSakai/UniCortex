using System;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetPlayModeViewTypeUseCaseTest
    {
        [TestCase(PlayModeViewTypes.SimulatorView)]
        [TestCase(PlayModeViewTypes.GameView)]
        public void ExecuteAsync_SetsViewType_And_ReturnsCurrentViewType(string viewType)
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPlayModeViewOperations();
            var useCase = new SetPlayModeViewTypeUseCase(dispatcher, operations);

            // Act
            var result = useCase.ExecuteAsync(viewType, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(viewType, result.viewType);
            Assert.AreEqual(1, operations.SetViewTypeCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_Throws_WithoutSetting_WhenViewTypeIsUnknown()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPlayModeViewOperations();
            var useCase = new SetPlayModeViewTypeUseCase(dispatcher, operations);

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                useCase.ExecuteAsync("Simulator", CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains(PlayModeViewTypes.SimulatorView, ex.Message);
            Assert.AreEqual(0, operations.SetViewTypeCallCount);
            Assert.AreEqual(0, dispatcher.CallCount);
        }
    }
}
