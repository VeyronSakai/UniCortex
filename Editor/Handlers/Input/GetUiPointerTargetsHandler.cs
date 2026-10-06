using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class GetUiPointerTargetsHandler
    {
        private readonly GetUiPointerTargetsUseCase _useCase;

        public GetUiPointerTargetsHandler(GetUiPointerTargetsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.InputUiPointerTargets, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var targets = await _useCase.ExecuteAsync(cancellationToken);
                var json = JsonUtility.ToJson(new GetUiPointerTargetsResponse(targets));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
