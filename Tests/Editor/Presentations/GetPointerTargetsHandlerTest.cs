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
                    SpyPointerTargetOperations.CreateTarget(100f, 200f, true, "Canvas/Modal")
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
            StringAssert.Contains("\"centerX\":100.0", context.ResponseBody);
            StringAssert.Contains("\"events\":[\"click\"]", context.ResponseBody);
            StringAssert.Contains("\"blocked\":true", context.ResponseBody);
            StringAssert.Contains("\"blockedBy\":\"Canvas/Modal\"", context.ResponseBody);
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
