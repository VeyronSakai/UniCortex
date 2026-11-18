using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Scene
{
    internal sealed class DiscardSceneChangesHandler
    {
        private readonly DiscardSceneChangesUseCase _useCase;

        public DiscardSceneChangesHandler(DiscardSceneChangesUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.SceneDiscard, HandleDiscardSceneChangesAsync);
        }

        private async Task HandleDiscardSceneChangesAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                await _useCase.ExecuteAsync(cancellationToken);
                var json = JsonUtility.ToJson(new DiscardSceneChangesResponse(true));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (PlayModeException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
