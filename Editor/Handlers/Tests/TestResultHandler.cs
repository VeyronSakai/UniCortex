using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.Handlers.Tests
{
    internal sealed class TestResultHandler
    {
        private readonly IMainThreadDispatcher _dispatcher;

        public TestResultHandler(IMainThreadDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.TestsResult, HandleGetResultAsync);
        }

        private async Task HandleGetResultAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var json = await _dispatcher.RunOnMainThreadAsync(
                () => TestResultStore.GetResult(), cancellationToken);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
