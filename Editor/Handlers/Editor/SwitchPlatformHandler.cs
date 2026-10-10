using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
{
    internal sealed class SwitchPlatformHandler
    {
        private readonly SwitchPlatformUseCase _useCase;

        public SwitchPlatformHandler(SwitchPlatformUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.PlatformSwitch, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = string.IsNullOrEmpty(body) ? null : JsonUtility.FromJson<SwitchPlatformRequest>(body);

            if (request == null || string.IsNullOrEmpty(request.buildTarget))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("buildTarget is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            SwitchPlatformResponse result;
            try
            {
                result = await _useCase.ExecuteAsync(request.buildTarget, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
