using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.Handlers.Tests
{
    internal sealed class TestResultHandler
    {
        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.TestsResult, HandleGetResultAsync);
        }

        private async Task HandleGetResultAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var json = TestResultStore.GetResult();
            await context.WriteResponseAsync(200, json);
        }
    }
}
