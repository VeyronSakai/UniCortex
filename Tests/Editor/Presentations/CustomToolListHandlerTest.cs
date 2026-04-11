using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.CustomTool;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class CustomToolListHandlerTest
    {
        [Test]
        public void HandleList_Returns200WithEmptyList_WhenNoCustomTools()
        {
            var registry = new CustomToolRegistry();
            var handler = new CustomToolListHandler(registry);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.CustomToolList);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("\"tools\":", context.ResponseBody);
        }
    }
}
