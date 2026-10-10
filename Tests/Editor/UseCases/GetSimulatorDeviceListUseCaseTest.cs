using System;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetSimulatorDeviceListUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsDevices_WhenSimulatorView()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPlayModeViewOperations
            {
                ViewType = PlayModeViewTypes.SimulatorView,
                SelectedIndex = 1,
                Rotation = 90
            };
            var useCase = new GetSimulatorDeviceListUseCase(dispatcher, operations);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, result.devices.Length);
            Assert.AreEqual("Apple iPhone 13", result.devices[0].name);
            Assert.AreEqual(1, result.selectedIndex);
            Assert.AreEqual(90, result.rotation);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_Throws_WhenGameView()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPlayModeViewOperations { ViewType = PlayModeViewTypes.GameView };
            var useCase = new GetSimulatorDeviceListUseCase(dispatcher, operations);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }
    }
}
