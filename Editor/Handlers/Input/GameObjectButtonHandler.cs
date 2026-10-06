using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    // Handles click, press and release at the center of a GameObject.
    internal sealed class GameObjectButtonHandler
    {
        private readonly string _route;
        private readonly Func<PointerPosition, string, CancellationToken, Task<PointerResponse>> _execute;

        public GameObjectButtonHandler(string route,
            Func<PointerPosition, string, CancellationToken, Task<PointerResponse>> execute)
        {
            _route = route;
            _execute = execute;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, _route, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            var request = string.IsNullOrEmpty(body)
                ? new GameObjectButtonRequest()
                : JsonUtility.FromJson<GameObjectButtonRequest>(body);
            var error = PointerRequestParser.ValidateInstanceId(request.instanceId, "instanceId");
            if (error != null)
            {
                await WriteErrorAsync(context, error);
                return;
            }

            var position = new PointerPosition.Target(request.instanceId);
            var button = string.IsNullOrEmpty(request.button) ? MouseButton.Left : request.button;

            PointerResponse response;
            try
            {
                response = await _execute(position, button, cancellationToken);
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
