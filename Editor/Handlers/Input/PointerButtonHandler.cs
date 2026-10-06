using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    // Handles the requests that take a position and a button: click, press and release.
    internal sealed class PointerButtonHandler
    {
        private readonly string _route;
        private readonly Func<PointerPosition, string, CancellationToken, Task<PointerResponse>> _execute;

        public PointerButtonHandler(string route,
            Func<PointerPosition, string, CancellationToken, Task<PointerResponse>> execute)
        {
            _route = route;
            _execute = execute;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, _route, HandleAsync);
        }

        // Non-nullable fields for JsonUtility (see PointerRequestParser).
        [Serializable]
        private class RawPointerButtonRequest
        {
            public float x;
            public float y;
            public int instanceId;
            public string button;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                await WriteErrorAsync(context, PointerRequestParser.PositionRequiredMessage);
                return;
            }

            var request = JsonUtility.FromJson<RawPointerButtonRequest>(body);
            var position = PointerRequestParser.ParsePosition(body, request.x, request.y, request.instanceId,
                "x", "y", "instanceId", out var error);
            if (position == null)
            {
                await WriteErrorAsync(context, error);
                return;
            }

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
