using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Utility
{
    internal sealed class ExecuteMenuItemHandler
    {
        private readonly ExecuteMenuItemUseCase _useCase;

        public ExecuteMenuItemHandler(ExecuteMenuItemUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.MenuExecute, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("menuPath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<ExecuteMenuItemRequest>(body);

            if (string.IsNullOrEmpty(request.menuPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("menuPath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var success = await _useCase.ExecuteAsync(request.menuPath, cancellationToken);
            var json = JsonUtility.ToJson(new ExecuteMenuItemResponse(success));
            await context.WriteResponseAsync(200, json);
        }
    }
}
