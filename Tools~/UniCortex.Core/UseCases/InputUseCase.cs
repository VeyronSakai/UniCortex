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

    public ValueTask<string> ClickPointerAsync(float? x, float? y, int? instanceId, string button,
        CancellationToken cancellationToken)
        => SendPointerButtonAsync(ApiRoutes.InputPointerClick, "Clicked", x, y, instanceId, button,
            cancellationToken);

    public ValueTask<string> PressPointerAsync(float? x, float? y, int? instanceId, string button,
        CancellationToken cancellationToken)
        => SendPointerButtonAsync(ApiRoutes.InputPointerPress, "Pressed", x, y, instanceId, button,
            cancellationToken);

    public ValueTask<string> ReleasePointerAsync(float? x, float? y, int? instanceId, string button,
        CancellationToken cancellationToken)
        => SendPointerButtonAsync(ApiRoutes.InputPointerRelease, "Released", x, y, instanceId, button,
            cancellationToken);

    public async ValueTask<string> MovePointerAsync(float? x, float? y, int? instanceId,
        CancellationToken cancellationToken)
    {
        var request = new MovePointerRequest { x = x, y = y, instanceId = instanceId };
        var response = await client.PostAsync<MovePointerRequest, PointerResponse>(ApiRoutes.InputPointerMove,
            request, cancellationToken);
        return $"Moved pointer to ({response.x}, {response.y})";
    }

    public async ValueTask<string> DragPointerAsync(float? x, float? y, int? instanceId,
        float? toX, float? toY, int? toInstanceId, string button, int? frames, int? holdFrames,
        CancellationToken cancellationToken)
    {
        var request = new DragPointerRequest
        {
            x = x,
            y = y,
            instanceId = instanceId,
            toX = toX,
            toY = toY,
            toInstanceId = toInstanceId,
            button = button,
            frames = frames,
            holdFrames = holdFrames
        };
        var response = await client.PostAsync<DragPointerRequest, DragPointerResponse>(ApiRoutes.InputPointerDrag,
            request, cancellationToken);
        return $"Dragged pointer from ({response.x}, {response.y}) to ({response.toX}, {response.toY}) button={button}";
    }

    private async ValueTask<string> SendPointerButtonAsync(string route, string verb, float? x, float? y,
        int? instanceId, string button, CancellationToken cancellationToken)
    {
        var request = new PointerButtonRequest { x = x, y = y, instanceId = instanceId, button = button };
        var response = await client.PostAsync<PointerButtonRequest, PointerResponse>(route, request,
            cancellationToken);
        return $"{verb} pointer at ({response.x}, {response.y}) button={button}";
    }

    public async ValueTask<string> GetPointerTargetsAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetPointerTargetsRequest, GetPointerTargetsResponse>(
            ApiRoutes.InputPointerTargets, cancellationToken: cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
