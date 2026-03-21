using System;
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

        // JsonUtility does not support Nullable<T>, so use a non-nullable helper for deserialization.
        // Field presence is detected via string matching; this class is only used for value extraction.
        [Serializable]
        private class RawModifyRequest
        {
            public int instanceId;
            public string name;
            public bool activeSelf;
            public string tag;
            public int layer;
            public int parentInstanceId;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var raw = JsonUtility.FromJson<RawModifyRequest>(body);

            if (raw.instanceId == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            // Determine which fields were provided in the JSON body
            var modifyName = body.Contains("\"name\"") ? raw.name : null;
            var modifyActiveSelf = body.Contains("\"activeSelf\"") ? (bool?)raw.activeSelf : null;
            var modifyTag = body.Contains("\"tag\"") ? raw.tag : null;
            var modifyLayer = body.Contains("\"layer\"") ? (int?)raw.layer : null;
            var modifyParent = body.Contains("\"parentInstanceId\"") ? (int?)raw.parentInstanceId : null;

            await _useCase.ExecuteAsync(raw.instanceId, modifyName, modifyActiveSelf, modifyTag, modifyLayer,
                modifyParent, cancellationToken);
            var json = JsonUtility.ToJson(new ModifyGameObjectResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
