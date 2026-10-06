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

    public ValueTask<string> ClickPointerAsync(float x, float y, string button,
        CancellationToken cancellationToken)
        => SendPointerButtonAsync(ApiRoutes.InputPointerClick, "Clicked", x, y, button, cancellationToken);

    public ValueTask<string> PressPointerAsync(float x, float y, string button,
        CancellationToken cancellationToken)
        => SendPointerButtonAsync(ApiRoutes.InputPointerPress, "Pressed", x, y, button, cancellationToken);

    public ValueTask<string> ReleasePointerAsync(float x, float y, string button,
        CancellationToken cancellationToken)
        => SendPointerButtonAsync(ApiRoutes.InputPointerRelease, "Released", x, y, button, cancellationToken);

    public async ValueTask<string> MovePointerAsync(float x, float y, CancellationToken cancellationToken)
    {
        var request = new MovePointerRequest { x = x, y = y };
        var response = await client.PostAsync<MovePointerRequest, PointerResponse>(ApiRoutes.InputPointerMove,
            request, cancellationToken);
        return $"Moved pointer to ({response.x}, {response.y})";
    }

    public async ValueTask<string> DragPointerAsync(float fromX, float fromY, float toX, float toY, string button,
        int? frames, int? holdFrames, CancellationToken cancellationToken)
    {
        var request = new DragPointerRequest
        {
            fromX = fromX,
            fromY = fromY,
            toX = toX,
            toY = toY,
            button = button,
            frames = frames,
            holdFrames = holdFrames
        };
        var response = await client.PostAsync<DragPointerRequest, DragPointerResponse>(ApiRoutes.InputPointerDrag,
            request, cancellationToken);
        return FormatDrag(response, button);
    }

    public ValueTask<string> ClickGameObjectAsync(int instanceId, string button,
        CancellationToken cancellationToken)
        => SendGameObjectButtonAsync(ApiRoutes.InputGameObjectClick, "Clicked", instanceId, button,
            cancellationToken);

    public ValueTask<string> PressGameObjectAsync(int instanceId, string button,
        CancellationToken cancellationToken)
        => SendGameObjectButtonAsync(ApiRoutes.InputGameObjectPress, "Pressed", instanceId, button,
            cancellationToken);

    public ValueTask<string> ReleaseGameObjectAsync(int instanceId, string button,
        CancellationToken cancellationToken)
        => SendGameObjectButtonAsync(ApiRoutes.InputGameObjectRelease, "Released", instanceId, button,
            cancellationToken);

    public async ValueTask<string> MoveGameObjectAsync(int instanceId, CancellationToken cancellationToken)
    {
        var request = new MoveGameObjectRequest { instanceId = instanceId };
        var response = await client.PostAsync<MoveGameObjectRequest, PointerResponse>(ApiRoutes.InputGameObjectMove,
            request, cancellationToken);
        return $"Moved pointer to instanceId {instanceId} at ({response.x}, {response.y})";
    }

    public async ValueTask<string> DragGameObjectAsync(int fromInstanceId, int toInstanceId, string button,
        int? frames, int? holdFrames, CancellationToken cancellationToken)
    {
        var request = new DragGameObjectRequest
        {
            fromInstanceId = fromInstanceId,
            toInstanceId = toInstanceId,
            button = button,
            frames = frames,
            holdFrames = holdFrames
        };
        var response = await client.PostAsync<DragGameObjectRequest, DragPointerResponse>(
            ApiRoutes.InputGameObjectDrag, request, cancellationToken);
        return FormatDrag(response, button);
    }

    private async ValueTask<string> SendPointerButtonAsync(string route, string verb, float x, float y,
        string button, CancellationToken cancellationToken)
    {
        var request = new PointerButtonRequest { x = x, y = y, button = button };
        var response = await client.PostAsync<PointerButtonRequest, PointerResponse>(route, request,
            cancellationToken);
        return $"{verb} pointer at ({response.x}, {response.y}) button={button}";
    }

    private async ValueTask<string> SendGameObjectButtonAsync(string route, string verb, int instanceId,
        string button, CancellationToken cancellationToken)
    {
        var request = new GameObjectButtonRequest { instanceId = instanceId, button = button };
        var response = await client.PostAsync<GameObjectButtonRequest, PointerResponse>(route, request,
            cancellationToken);
        return $"{verb} pointer on instanceId {instanceId} at ({response.x}, {response.y}) button={button}";
    }

    private static string FormatDrag(DragPointerResponse response, string button)
    {
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
