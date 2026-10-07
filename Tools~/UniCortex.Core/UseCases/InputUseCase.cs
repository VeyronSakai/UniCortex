using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class InputUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> PressKeyAsync(string[] keys, float? holdDuration,
        CancellationToken cancellationToken)
    {
        var request = new PressKeyRequest { keys = keys, holdDuration = holdDuration };
        await client.PostAsync<PressKeyRequest, PressKeyResponse>(ApiRoutes.InputKeyPress, request,
            cancellationToken);
        return $"Pressed keys: {string.Join("+", keys)}";
    }

    public async ValueTask<string> ClickMouseAsync(float? x, float? y, int? instanceId, string button,
        float? holdDuration, CancellationToken cancellationToken)
    {
        var request = new ClickMouseRequest
        {
            x = x,
            y = y,
            instanceId = instanceId,
            button = button,
            holdDuration = holdDuration
        };
        var response = await client.PostAsync<ClickMouseRequest, MouseResponse>(ApiRoutes.InputMouseClick,
            request, cancellationToken);
        return $"Clicked pointer at ({response.x}, {response.y}) button={button}";
    }

    public async ValueTask<string> MoveMouseAsync(float? x, float? y, int? instanceId,
        CancellationToken cancellationToken)
    {
        var request = new MoveMouseRequest { x = x, y = y, instanceId = instanceId };
        var response = await client.PostAsync<MoveMouseRequest, MouseResponse>(ApiRoutes.InputMouseMove,
            request, cancellationToken);
        return $"Moved pointer to ({response.x}, {response.y})";
    }

    public async ValueTask<string> DragMouseAsync(float? fromX, float? fromY, int? fromInstanceId,
        float? toX, float? toY, int? toInstanceId, string button, float? duration,
        CancellationToken cancellationToken)
    {
        var request = new DragMouseRequest
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
        var response = await client.PostAsync<DragMouseRequest, DragMouseResponse>(ApiRoutes.InputMouseDrag,
            request, cancellationToken);
        return $"Dragged pointer from ({response.fromX}, {response.fromY}) " +
               $"to ({response.toX}, {response.toY}) button={button}";
    }

    public async ValueTask<string> GetUIPointerTargetsAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetUIPointerTargetsRequest, GetUIPointerTargetsResponse>(
            ApiRoutes.InputUIPointerTargets, cancellationToken: cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
