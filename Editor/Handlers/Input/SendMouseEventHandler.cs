using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class SendMouseEventHandler
    {
        private const string PositionRequiredMessage = "Specify either x and y, or instanceId.";

        private readonly SendMouseEventUseCase _useCase;

        public SendMouseEventHandler(SendMouseEventUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputMouse, HandleAsync);
        }

        // JsonUtility does not support Nullable<T>, so use a non-nullable helper for deserialization.
        // Field presence is detected via string matching because 0 is a valid coordinate.
        [Serializable]
        private class RawSendMouseEventRequest
        {
            public float x;
            public float y;
            public int instanceId;
            public string button;
            public string eventType;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                await WriteErrorAsync(context, PositionRequiredMessage);
                return;
            }

            var request = JsonUtility.FromJson<RawSendMouseEventRequest>(body);

            var hasX = HasField(body, "x");
            var hasY = HasField(body, "y");
            var hasTarget = HasField(body, "instanceId");

            if (hasX != hasY)
            {
                await WriteErrorAsync(context, "x and y must be specified together.");
                return;
            }

            if (hasX == hasTarget)
            {
                await WriteErrorAsync(context, PositionRequiredMessage);
                return;
            }

            if (hasTarget && request.instanceId == 0)
            {
                await WriteErrorAsync(context, "instanceId must not be 0.");
                return;
            }

            var button = string.IsNullOrEmpty(request.button) ? MouseButton.Left : request.button;
            var eventType = string.IsNullOrEmpty(request.eventType) ? InputEventType.Click : request.eventType;

            SendMouseEventResponse response;
            try
            {
                response = hasTarget
                    ? await _useCase.ExecuteAsync(request.instanceId, button, eventType, cancellationToken)
                    : await _useCase.ExecuteAsync(request.x, request.y, button, eventType, cancellationToken);
            }
            catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException
                                           or ArgumentException)
            {
                await WriteErrorAsync(context, ex.Message);
                return;
            }

            var json = JsonUtility.ToJson(response);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }

        // Matches the key followed by a colon so that a string value such as "x" is not taken as the key.
        private static bool HasField(string body, string name)
        {
            return Regex.IsMatch(body, $"\"{name}\"\\s*:");
        }

        private static Task WriteErrorAsync(IRequestContext context, string message)
        {
            var errorJson = JsonUtility.ToJson(new ErrorResponse(message));
            return context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
        }
    }
}
