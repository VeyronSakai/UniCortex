using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Console
{
    internal sealed class ConsoleClearHandler
    {
        private readonly ClearConsoleLogsUseCase _useCase;

        public ConsoleClearHandler(ClearConsoleLogsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ConsoleClear, HandleConsoleClearAsync);
        }

        private async Task HandleConsoleClearAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(new ConsoleClearResponse(success: true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
