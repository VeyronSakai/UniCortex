using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Prefab
{
    internal sealed class ClosePrefabHandler
    {
        private readonly ClosePrefabUseCase _useCase;

        public ClosePrefabHandler(ClosePrefabUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.PrefabClose, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            // The body is optional, so that a request without one keeps closing as before.
            var body = await context.ReadBodyAsync();
            var request = string.IsNullOrEmpty(body)
                ? new ClosePrefabRequest()
                : JsonUtility.FromJson<ClosePrefabRequest>(body);

            try
            {
                await _useCase.ExecuteAsync(request.discardChanges, cancellationToken);
                var json = JsonUtility.ToJson(new ClosePrefabResponse(true));
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
