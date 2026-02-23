using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
{
    internal sealed class UndoHandler
    {
        private readonly UndoUseCase _useCase;

        public UndoHandler(UndoUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.Undo, HandleUndoAsync);
        }

        private async Task HandleUndoAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(new UndoResponse(success: true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
