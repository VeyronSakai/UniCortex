using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Utility
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

            var testMode = "EditMode";
            var nameFilter = "";

            if (!string.IsNullOrEmpty(body))
            {
                var request = JsonUtility.FromJson<RunTestsRequest>(body);
                if (request != null)
                {
                    if (!string.IsNullOrEmpty(request.testMode))
                    {
                        testMode = request.testMode;
                    }

                    nameFilter = request.nameFilter ?? "";
                }
            }

            var response = await _useCase.ExecuteAsync(testMode, nameFilter, cancellationToken);
            var json = JsonUtility.ToJson(response);
            await context.WriteResponseAsync(200, json);
        }
    }
}
