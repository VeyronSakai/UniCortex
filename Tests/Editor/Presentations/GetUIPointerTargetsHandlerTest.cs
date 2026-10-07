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
    internal sealed class GetUIPointerTargetsHandlerTest
    {
        [Test]
        public void Handle_Returns200_WithTargets()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyUIPointerTargetOperations
            {
                UIPointerTargetsToReturn = new List<UIPointerTargetEntry>
                {
                    new UIPointerTargetEntry("Canvas/Button", 100, new ScreenRect(50f, 180f, 100f, 40f))
                }
            };
            var handler = new GetUIPointerTargetsHandler(new GetUIPointerTargetsUseCase(dispatcher, ops));
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.InputUIPointerTargets);

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
            var ops = new SpyUIPointerTargetOperations
            {
                ExceptionToThrow = new InvalidOperationException("Pointer targets are only available in Play Mode.")
            };
            var handler = new GetUIPointerTargetsHandler(new GetUIPointerTargetsUseCase(dispatcher, ops));
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.InputUIPointerTargets);

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
            var ops = new UIPointerTargetNotSupportedAdapter();
            var handler = new GetUIPointerTargetsHandler(new GetUIPointerTargetsUseCase(dispatcher, ops));
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.InputUIPointerTargets);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("com.unity.ugui", context.ResponseBody);
        }
    }
}
