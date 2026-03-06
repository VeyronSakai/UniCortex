using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Tests
{
    internal sealed class RunTestsHandler
    {
        private readonly RunTestsUseCase _useCase;

        public RunTestsHandler(RunTestsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.TestsRun, HandleRunTestsAsync);
        }

        private async Task HandleRunTestsAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            var request = new RunTestsRequest();

            if (!string.IsNullOrEmpty(body))
            {
                var parsed = JsonUtility.FromJson<RunTestsRequest>(body);
                if (parsed != null)
                {
                    request = parsed;
                }
            }

            if (string.IsNullOrEmpty(request.testMode))
            {
                request.testMode = TestModes.EditMode;
            }

            var response = await _useCase.ExecuteAsync(request, cancellationToken);
            var json = JsonUtility.ToJson(response);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
