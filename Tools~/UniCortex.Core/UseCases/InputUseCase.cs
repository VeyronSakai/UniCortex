using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class InputUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> SendKeyEventAsync(string key, string eventType,
        CancellationToken cancellationToken)
    {
        var request = new SendKeyEventRequest { key = key, eventType = eventType };
        await client.PostAsync<SendKeyEventRequest, SendKeyEventResponse>(ApiRoutes.InputKey, request, cancellationToken);
        return $"Key event sent: {key} ({eventType})";
    }

    public async ValueTask<string> ClickPointerAsync(float? x, float? y, int? instanceId, string button,
        float? holdDuration, CancellationToken cancellationToken)
    {
        var request = new ClickPointerRequest
        {
            x = x,
            y = y,
            instanceId = instanceId,
            button = button,
            holdDuration = holdDuration
        };
        var response = await client.PostAsync<ClickPointerRequest, PointerResponse>(ApiRoutes.InputPointerClick,
            request, cancellationToken);
        return $"Clicked pointer at ({response.x}, {response.y}) button={button}";
    }

    public async ValueTask<string> MovePointerAsync(float? x, float? y, int? instanceId,
        CancellationToken cancellationToken)
    {
        var request = new MovePointerRequest { x = x, y = y, instanceId = instanceId };
        var response = await client.PostAsync<MovePointerRequest, PointerResponse>(ApiRoutes.InputPointerMove,
            request, cancellationToken);
        return $"Moved pointer to ({response.x}, {response.y})";
    }

    public async ValueTask<string> DragPointerAsync(float? fromX, float? fromY, int? fromInstanceId,
        float? toX, float? toY, int? toInstanceId, string button, float? duration,
        CancellationToken cancellationToken)
    {
        var request = new DragPointerRequest
        {
            fromX = fromX,
            fromY = fromY,
            fromInstanceId = fromInstanceId,
            toX = toX,
            toY = toY,
            toInstanceId = toInstanceId,
            button = button,
            duration = duration
        };
        var response = await client.PostAsync<DragPointerRequest, DragPointerResponse>(ApiRoutes.InputPointerDrag,
            request, cancellationToken);
        return $"Dragged pointer from ({response.fromX}, {response.fromY}) " +
               $"to ({response.toX}, {response.toY}) button={button}";
    }

    public async ValueTask<string> GetPointerTargetsAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetPointerTargetsRequest, GetPointerTargetsResponse>(
            ApiRoutes.InputPointerTargets, cancellationToken: cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
