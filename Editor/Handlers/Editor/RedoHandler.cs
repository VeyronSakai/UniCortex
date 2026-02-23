using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
{
    internal sealed class RedoHandler
    {
        private readonly RedoUseCase _useCase;

        public RedoHandler(RedoUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.Redo, HandleRedoAsync);
        }

        private async Task HandleRedoAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(new RedoResponse(success: true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
