using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Scene
{
    internal sealed class SaveSceneAsHandler
    {
        private readonly SaveSceneAsUseCase _useCase;

        public SaveSceneAsHandler(SaveSceneAsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.SceneSaveAs, HandleSaveSceneAsAsync);
        }

        private async Task HandleSaveSceneAsAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = string.IsNullOrEmpty(body) ? null : JsonUtility.FromJson<SaveSceneAsRequest>(body);

            if (request == null || string.IsNullOrEmpty(request.scenePath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("scenePath is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            try
            {
                await _useCase.ExecuteAsync(request.scenePath, cancellationToken);
                var json = JsonUtility.ToJson(new SaveSceneAsResponse(true));
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
