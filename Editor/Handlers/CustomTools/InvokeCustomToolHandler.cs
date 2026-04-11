using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UnityEngine;

namespace UniCortex.Editor.Handlers.CustomTools
{
    internal sealed class InvokeCustomToolHandler
    {
        private readonly CustomExtensionRegistry _registry;

        public InvokeCustomToolHandler(CustomExtensionRegistry registry)
        {
            _registry = registry;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.CustomToolsInvoke, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                await context.WriteResponseAsync(
                    HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse("toolName is required.")));
                return;
            }

            var request = JsonUtility.FromJson<InvokeCustomToolRequest>(body);
            if (request == null || string.IsNullOrWhiteSpace(request.toolName))
            {
                await context.WriteResponseAsync(
                    HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse("toolName is required.")));
                return;
            }

            if (!_registry.HasTool(request.toolName))
            {
                await context.WriteResponseAsync(
                    HttpStatusCodes.NotFound,
                    JsonUtility.ToJson(new ErrorResponse($"Custom tool '{request.toolName}' was not found.")));
                return;
            }

            try
            {
                var content = await _registry.InvokeToolAsync(
                    request.toolName,
                    request.argumentsJson,
                    cancellationToken);

                await context.WriteResponseAsync(
                    HttpStatusCodes.Ok,
                    JsonUtility.ToJson(new InvokeCustomToolResponse(content)));
            }
            catch (ArgumentException ex)
            {
                await context.WriteResponseAsync(
                    HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse(ex.Message)));
            }
            catch (InvalidOperationException ex)
            {
                await context.WriteResponseAsync(
                    HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse(ex.Message)));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UniCortex] Custom tool '{request.toolName}' failed: {ex}");
                await context.WriteResponseAsync(
                    HttpStatusCodes.InternalServerError,
                    JsonUtility.ToJson(new ErrorResponse(ex.Message)));
            }
        }
    }
}
