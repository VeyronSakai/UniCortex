using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Scene
{
    internal sealed class CreateSceneHandler
    {
        private readonly CreateSceneUseCase _useCase;

        public CreateSceneHandler(CreateSceneUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.SceneCreate, HandleCreateSceneAsync);
        }

        private async Task HandleCreateSceneAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("scenePath is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<CreateSceneRequest>(body);

            if (string.IsNullOrEmpty(request.scenePath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("scenePath is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            try
            {
                var success = await _useCase.ExecuteAsync(request.scenePath, cancellationToken);
                if (!success)
                {
                    var errorJson = JsonUtility.ToJson(new ErrorResponse("Failed to create scene."));
                    await context.WriteResponseAsync(HttpStatusCodes.InternalServerError, errorJson);
                    return;
                }

                var json = JsonUtility.ToJson(new CreateSceneResponse(true));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
