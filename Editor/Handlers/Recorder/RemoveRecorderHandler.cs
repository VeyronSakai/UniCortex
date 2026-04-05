using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Recorder
{
    internal sealed class RemoveRecorderHandler
    {
        private readonly RemoveRecorderUseCase _useCase;

        public RemoveRecorderHandler(RemoveRecorderUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.RecorderRemove, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var body = await context.ReadBodyAsync();
                if (string.IsNullOrEmpty(body))
                {
                    var errorJson = JsonUtility.ToJson(new ErrorResponse("Request body is required."));
                    await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                    return;
                }

                var request = JsonUtility.FromJson<RemoveRecorderRequest>(body);
                await _useCase.ExecuteAsync(request.index, cancellationToken);
                var json = JsonUtility.ToJson(new RemoveRecorderResponse(true));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
            catch (NotSupportedException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
