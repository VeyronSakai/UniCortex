using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Scene
{
    internal sealed class OpenSceneHandler
    {
        private readonly OpenSceneUseCase _useCase;

        public OpenSceneHandler(OpenSceneUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.SceneOpen, HandleOpenSceneAsync);
        }

        private async Task HandleOpenSceneAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("scenePath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<OpenSceneRequest>(body);

            if (string.IsNullOrEmpty(request.scenePath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("scenePath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            await _useCase.ExecuteAsync(request.scenePath, cancellationToken);
            var json = JsonUtility.ToJson(new OpenSceneResponse(true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
