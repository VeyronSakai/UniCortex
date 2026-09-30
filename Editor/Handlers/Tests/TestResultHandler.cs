using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Handlers.Tests
{
    internal sealed class TestResultHandler
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITestResultStore _testResultStore;

        public TestResultHandler(IMainThreadDispatcher dispatcher, ITestResultStore testResultStore)
        {
            _dispatcher = dispatcher;
            _testResultStore = testResultStore;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.TestsResult, HandleGetResultAsync);
        }

        private async Task HandleGetResultAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var json = await _dispatcher.RunOnMainThreadAsync(
                _testResultStore.GetResult, cancellationToken);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
