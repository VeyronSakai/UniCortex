using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Handlers.CustomTool
{
    internal sealed class CustomToolExecuteHandler
    {
        private readonly CustomToolRegistry _registry;
        private readonly IMainThreadDispatcher _dispatcher;

        public CustomToolExecuteHandler(CustomToolRegistry registry, IMainThreadDispatcher dispatcher)
        {
            _registry = registry;
            _dispatcher = dispatcher;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.CustomToolExecute, HandleExecuteAsync);
        }

        private async Task HandleExecuteAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = JsonUtility.FromJson<CustomToolExecuteRequest>(body);

            if (string.IsNullOrEmpty(request.name))
            {
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse("Missing required field: name")));
                return;
            }

            if (!_registry.TryGetHandler(request.name, out var handler))
            {
                await context.WriteResponseAsync(HttpStatusCodes.NotFound,
                    JsonUtility.ToJson(new ErrorResponse($"Custom tool not found: {request.name}")));
                return;
            }

            var arguments = request.arguments ?? "";

            var result = await _dispatcher.RunOnMainThreadAsync(() => handler.Execute(arguments), cancellationToken);

            var response = new CustomToolExecuteResponse { result = result };
            await context.WriteResponseAsync(HttpStatusCodes.Ok, JsonUtility.ToJson(response));
        }
    }
}
