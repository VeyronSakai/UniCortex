using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.GameView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SimulatorDeviceHandlerTest
    {
        private SpyPlayModeViewOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyPlayModeViewOperations { ViewType = PlayModeViewTypes.SimulatorView };
            _router = new RequestRouter();
            new GetSimulatorDeviceListHandler(new GetSimulatorDeviceListUseCase(dispatcher, _operations))
                .Register(_router);
            new SetSimulatorDeviceHandler(new SetSimulatorDeviceUseCase(dispatcher, _operations))
                .Register(_router);
        }

        [Test]
        public void HandleGetList_Returns200_WithDevices()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.SimulatorDevices);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<GetSimulatorDeviceListResponse>(context.ResponseBody);
            Assert.AreEqual(2, response.devices.Length);
            Assert.AreEqual("Google Pixel 5", response.devices[1].name);
            Assert.AreEqual(2340, response.devices[1].screenHeight);
        }

        [Test]
        public void HandleGetList_Returns400_WhenGameView()
        {
            // Arrange
            _operations.ViewType = PlayModeViewTypes.GameView;
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.SimulatorDevices);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Simulator view", context.ResponseBody);
        }

        [Test]
        public void HandleSet_Returns200_WithDeviceAndRotation()
        {
            // Arrange
            var body = JsonUtility.ToJson(new SetSimulatorDeviceRequest { index = 1, rotation = 90 });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SimulatorDevice, body);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<SetSimulatorDeviceResponse>(context.ResponseBody);
            Assert.AreEqual("Google Pixel 5", response.deviceName);
            Assert.AreEqual(90, response.rotation);
        }

        [Test]
        public void HandleSet_Returns400_WhenIndexIsOutOfRange()
        {
            // Arrange
            var body = JsonUtility.ToJson(new SetSimulatorDeviceRequest { index = 10 });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SimulatorDevice, body);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("out of range", context.ResponseBody);
        }

        [Test]
        public void HandleSet_Returns400_WhenNothingIsSpecified()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SimulatorDevice, "");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _operations.SetSimulatorDeviceCallCount);
            Assert.AreEqual(0, _operations.SetSimulatorRotationCallCount);
        }
    }
}
