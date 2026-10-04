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
        return await SendMouseEventAsync(x, y, null, null, button, eventType, cancellationToken);
    }

    public async ValueTask<string> SendMouseEventAsync(float? x, float? y, int? targetInstanceId, string? targetPath,
        string button, string eventType, CancellationToken cancellationToken)
    {
        var request = new SendMouseEventRequest
        {
            x = x,
            y = y,
            targetInstanceId = targetInstanceId,
            targetPath = targetPath,
            button = button,
            eventType = eventType
        };
        var response = await client.PostAsync<SendMouseEventRequest, SendMouseEventResponse>(ApiRoutes.InputMouse,
            request, cancellationToken);

        if (targetInstanceId == null && string.IsNullOrEmpty(targetPath))
        {
            return $"Mouse event sent: ({response.x}, {response.y}) button={button} ({eventType})";
        }

        var target = targetInstanceId?.ToString() ?? targetPath;
        var message = $"Mouse event sent to target {target} at ({response.x}, {response.y}) button={button} ({eventType})";
        if (!response.targetBlocked)
        {
            return message;
        }

        return string.IsNullOrEmpty(response.blockedBy)
            ? $"{message}. Warning: nothing receives pointer input at the target's center " +
              "(it may be off-screen, inactive, or not a raycast target), so the target may not receive the event."
            : $"{message}. Warning: the target's center is covered by '{response.blockedBy}', " +
              "so the event may go to that object instead.";
    }

    public async ValueTask<string> GetPointerTargetsAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetPointerTargetsRequest, GetPointerTargetsResponse>(
            ApiRoutes.InputPointerTargets, cancellationToken: cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
