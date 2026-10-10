using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class SetSimulatorDeviceHandler
    {
        private readonly SetSimulatorDeviceUseCase _useCase;

        public SetSimulatorDeviceHandler(SetSimulatorDeviceUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.SimulatorDevice, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = string.IsNullOrEmpty(body)
                ? new SetSimulatorDeviceRequest()
                : JsonUtility.FromJson<SetSimulatorDeviceRequest>(body);

            SetSimulatorDeviceResponse result;
            try
            {
                result = await _useCase.ExecuteAsync(request.index, request.rotation, cancellationToken);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
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
