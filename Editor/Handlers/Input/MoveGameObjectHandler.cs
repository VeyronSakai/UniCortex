using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class MoveGameObjectHandler
    {
        private readonly MovePointerUseCase _useCase;

        public MoveGameObjectHandler(MovePointerUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputGameObjectMove, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            var request = string.IsNullOrEmpty(body)
                ? new MoveGameObjectRequest()
                : JsonUtility.FromJson<MoveGameObjectRequest>(body);
            var error = PointerRequestParser.ValidateInstanceId(request.instanceId, "instanceId");
            if (error != null)
            {
                await WriteErrorAsync(context, error);
                return;
            }

            var position = new PointerPosition.Target(request.instanceId);

            PointerResponse response;
            try
            {
                response = await _useCase.ExecuteAsync(position, cancellationToken);
            }
            catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException
                                           or ArgumentException)
            {
                await WriteErrorAsync(context, ex.Message);
                return;
            }

            await context.WriteResponseAsync(HttpStatusCodes.Ok, JsonUtility.ToJson(response));
        }

        private static Task WriteErrorAsync(IRequestContext context, string message)
        {
            var errorJson = JsonUtility.ToJson(new ErrorResponse(message));
            return context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
        }
    }
}
