using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Editor;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SetTimeScaleHandlerTest
    {
        [Test]
        public void HandleSetTimeScale_Returns200WithUpdatedTimeScale()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var timeOps = new SpyTimeOperations();
            var useCase = new SetTimeScaleUseCase(dispatcher, timeOps);
            var handler = new SetTimeScaleHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new SetTimeScaleRequest { timeScale = 0.5f };
            var body = JsonUtility.ToJson(request);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimeScale, body);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(0.5f, timeOps.TimeScale);
            StringAssert.Contains("0.5", context.ResponseBody);
        }
    }
}
