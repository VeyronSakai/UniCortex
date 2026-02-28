using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Scene
{
    internal sealed class SaveSceneHandler
    {
        private readonly SaveSceneUseCase _useCase;

        public SaveSceneHandler(SaveSceneUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.SceneSave, HandleSaveSceneAsync);
        }

        private async Task HandleSaveSceneAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var success = await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(new SaveSceneResponse(success));
            await context.WriteResponseAsync(200, json);
        }
    }
}
