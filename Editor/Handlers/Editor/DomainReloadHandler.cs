using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
{
    internal sealed class DomainReloadHandler
    {
        private const string CompilationFailedMessage =
            "Script compilation failed, so the domain was not reloaded. Check the compile errors in the Console.";

        private readonly RequestDomainReloadUseCase _useCase;

        public DomainReloadHandler(RequestDomainReloadUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.DomainReload, HandleDomainReloadAsync);
        }

        private async Task HandleDomainReloadAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            // Responds when the compilation has finished: 200 right before the domain reload starts
            // (the server waits for this response before it stops), or 400 when the compilation failed.
            bool reloading;
            try
            {
                reloading = await _useCase.ExecuteAsync(cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            if (!reloading)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(CompilationFailedMessage));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var json = JsonUtility.ToJson(new DomainReloadResponse(success: true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
