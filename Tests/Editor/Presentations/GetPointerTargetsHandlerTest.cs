using System;
using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Input;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class GetPointerTargetsHandlerTest
    {
        [Test]
        public void Handle_Returns200_WithTargets()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyPointerTargetOperations
            {
                PointerTargetsToReturn = new List<PointerTarget>
                {
                    new PointerTarget("Canvas/Button", 100, new ScreenRect(50f, 180f, 100f, 40f))
                }
            };
            var handler = new GetPointerTargetsHandler(new GetPointerTargetsUseCase(dispatcher, ops));
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.InputPointerTargets);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("\"targets\"", context.ResponseBody);
            StringAssert.Contains("\"path\":\"Canvas/Button\"", context.ResponseBody);
            StringAssert.Contains("\"instanceId\":100", context.ResponseBody);
            StringAssert.Contains("\"rect\":{\"x\":50.0,\"y\":180.0,\"width\":100.0,\"height\":40.0}",
                context.ResponseBody);
        }

        [Test]
        public void Handle_Returns400_WhenNotInPlayMode()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyPointerTargetOperations
            {
                ExceptionToThrow = new InvalidOperationException("Pointer targets are only available in Play Mode.")
            };
            var handler = new GetPointerTargetsHandler(new GetPointerTargetsUseCase(dispatcher, ops));
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.InputPointerTargets);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Play Mode", context.ResponseBody);
        }

        [Test]
        public void Handle_Returns400_WhenUguiNotInstalled()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new PointerTargetNotSupportedAdapter();
            var handler = new GetPointerTargetsHandler(new GetPointerTargetsUseCase(dispatcher, ops));
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.InputPointerTargets);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("com.unity.ugui", context.ResponseBody);
        }
    }
}
