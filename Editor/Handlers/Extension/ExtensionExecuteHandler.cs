using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Extension
{
    internal sealed class ExtensionExecuteHandler
    {
        private readonly ExtensionRegistry _registry;
        private readonly IMainThreadDispatcher _dispatcher;

        public ExtensionExecuteHandler(ExtensionRegistry registry, IMainThreadDispatcher dispatcher)
        {
            _registry = registry;
            _dispatcher = dispatcher;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ExtensionExecute, HandleExecuteAsync);
        }

        private async Task HandleExecuteAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = JsonUtility.FromJson<ExtensionExecuteRequest>(body);

            if (string.IsNullOrEmpty(request.name))
            {
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse("Missing required field: name")));
                return;
            }

            if (!_registry.TryGetHandler(request.name, out var handler))
            {
                await context.WriteResponseAsync(HttpStatusCodes.NotFound,
                    JsonUtility.ToJson(new ErrorResponse($"Extension not found: {request.name}")));
                return;
            }

            var arguments = request.arguments ?? "";

            var result = await _dispatcher.RunOnMainThreadAsync(() => handler.Execute(arguments), cancellationToken);

            var response = new ExtensionExecuteResponse { result = result };
            await context.WriteResponseAsync(HttpStatusCodes.Ok, JsonUtility.ToJson(response));
        }
    }
}
