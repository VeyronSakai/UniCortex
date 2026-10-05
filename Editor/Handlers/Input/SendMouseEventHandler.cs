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
        private const int DefaultDragFrames = 10;

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
            public float toX;
            public float toY;
            public int toInstanceId;
            public int frames;
            public int holdFrames;
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

            var startError = ValidatePosition(hasX, hasY, hasTarget, request.instanceId, "x", "y", "instanceId");
            if (startError != null)
            {
                await WriteErrorAsync(context, startError);
                return;
            }

            var button = string.IsNullOrEmpty(request.button) ? MouseButton.Left : request.button;
            var eventType = string.IsNullOrEmpty(request.eventType) ? InputEventType.Click : request.eventType;
            var isDrag = string.Equals(eventType, InputEventType.Drag, StringComparison.OrdinalIgnoreCase);

            var hasToX = HasField(body, "toX");
            var hasToY = HasField(body, "toY");
            var hasToTarget = HasField(body, "toInstanceId");
            var hasFrames = HasField(body, "frames");
            var hasHoldFrames = HasField(body, "holdFrames");

            if (!isDrag && (hasToX || hasToY || hasToTarget || hasFrames || hasHoldFrames))
            {
                await WriteErrorAsync(context,
                    $"toX, toY, toInstanceId, frames and holdFrames are only valid with eventType \"{InputEventType.Drag}\".");
                return;
            }

            SendMouseEventResponse response;
            try
            {
                if (isDrag)
                {
                    var endError = ValidatePosition(hasToX, hasToY, hasToTarget, request.toInstanceId,
                        "toX", "toY", "toInstanceId");
                    if (endError != null)
                    {
                        await WriteErrorAsync(context, endError);
                        return;
                    }

                    MousePosition start = hasTarget
                        ? new MousePosition.Target(request.instanceId)
                        : new MousePosition.Coordinates(request.x, request.y);
                    MousePosition end = hasToTarget
                        ? new MousePosition.Target(request.toInstanceId)
                        : new MousePosition.Coordinates(request.toX, request.toY);
                    var frames = hasFrames ? request.frames : DefaultDragFrames;
                    var holdFrames = hasHoldFrames ? request.holdFrames : 0;
                    response = await _useCase.DragAsync(start, end, button, frames, holdFrames, cancellationToken);
                }
                else
                {
                    response = hasTarget
                        ? await _useCase.ExecuteAsync(request.instanceId, button, eventType, cancellationToken)
                        : await _useCase.ExecuteAsync(request.x, request.y, button, eventType, cancellationToken);
                }
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

        // Returns an error message when the position is not given by exactly one of the coordinates or a target.
        private static string ValidatePosition(bool hasX, bool hasY, bool hasTarget, int instanceId,
            string xName, string yName, string targetName)
        {
            if (hasX != hasY)
            {
                return $"{xName} and {yName} must be specified together.";
            }

            if (hasX == hasTarget)
            {
                return $"Specify either {xName} and {yName}, or {targetName}.";
            }

            if (hasTarget && instanceId == 0)
            {
                return $"{targetName} must not be 0.";
            }

            return null;
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
