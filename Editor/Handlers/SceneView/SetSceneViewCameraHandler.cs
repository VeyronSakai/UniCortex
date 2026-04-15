using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.SceneView
{
    internal sealed class SetSceneViewCameraHandler
    {
        private readonly SetSceneViewCameraUseCase _useCase;

        public SetSceneViewCameraHandler(SetSceneViewCameraUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.SceneViewCamera, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = JsonUtility.FromJson<SetSceneViewCameraRequest>(body);
            var result = await _useCase.ExecuteAsync(request, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
