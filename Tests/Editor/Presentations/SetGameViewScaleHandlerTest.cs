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
    internal sealed class SetGameViewScaleHandlerTest
    {
        [Test]
        public void HandleSetGameViewScale_Returns200WithAppliedScale()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations { MinScale = 0.5f, MaxScale = 5.0f };
            var useCase = new SetGameViewScaleUseCase(dispatcher, operations);
            var handler = new SetGameViewScaleHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new SetGameViewScaleRequest { scale = 2.0f };
            var body = JsonUtility.ToJson(request);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewScale, body);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2.0f, operations.LastSetScale);
            Assert.AreEqual(1, operations.SetGameViewScaleCallCount);
            StringAssert.Contains("true", context.ResponseBody);
        }
    }
}
