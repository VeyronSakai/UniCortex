using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameObject
{
    internal sealed class ModifyGameObjectHandler
    {
        private readonly ModifyGameObjectUseCase _useCase;

        public ModifyGameObjectHandler(ModifyGameObjectUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.GameObjectModify, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<ModifyGameObjectRequest>(body);

            if (request.instanceId == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            // Determine which fields were provided in the JSON body
            string modifyName = body.Contains("\"name\"") ? request.name : null;
            bool? modifyActiveSelf = body.Contains("\"activeSelf\"") ? (bool?)request.activeSelf : null;
            string modifyTag = body.Contains("\"tag\"") ? request.tag : null;
            int? modifyLayer = body.Contains("\"layer\"") ? (int?)request.layer : null;
            int? modifyParent = body.Contains("\"parentInstanceId\"") ? (int?)request.parentInstanceId : null;

            await _useCase.ExecuteAsync(request.instanceId, modifyName, modifyActiveSelf, modifyTag, modifyLayer,
                modifyParent, cancellationToken);
            var json = JsonUtility.ToJson(new ModifyGameObjectResponse(true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
