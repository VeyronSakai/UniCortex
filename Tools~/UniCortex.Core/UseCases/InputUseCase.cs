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

    public async ValueTask<string> SendMouseEventAsync(float x, float y, string button, string eventType,
        CancellationToken cancellationToken)
    {
        return await SendMouseEventAsync(x, y, null, button, eventType, cancellationToken);
    }

    public async ValueTask<string> SendMouseEventAsync(float? x, float? y, int? instanceId,
        string button, string eventType, CancellationToken cancellationToken)
    {
        var request = new SendMouseEventRequest
        {
            x = x,
            y = y,
            instanceId = instanceId,
            button = button,
            eventType = eventType
        };
        var response = await client.PostAsync<SendMouseEventRequest, SendMouseEventResponse>(ApiRoutes.InputMouse,
            request, cancellationToken);

        if (instanceId == null)
        {
            return $"Mouse event sent: ({response.x}, {response.y}) button={button} ({eventType})";
        }

        var message =
            $"Mouse event sent to instanceId {instanceId} at ({response.x}, {response.y}) button={button} ({eventType})";
        return response.blocked
            ? $"{message}. Warning: the target is covered by other UI or off-screen at its center, " +
              "so it may not receive the event."
            : message;
    }

    public async ValueTask<string> GetPointerTargetsAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetPointerTargetsRequest, GetPointerTargetsResponse>(
            ApiRoutes.InputPointerTargets, cancellationToken: cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
