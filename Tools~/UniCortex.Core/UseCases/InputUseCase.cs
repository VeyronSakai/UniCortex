using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class InputUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> SendKeyEventAsync(string key, string eventType,
        CancellationToken cancellationToken)
    {
        var request = new SendKeyEventRequest { key = key, eventType = eventType };
        await client.PostAsync(ApiRoutes.InputKey, request, cancellationToken);
        return $"Key event sent: {key} ({eventType})";
    }

    public async ValueTask<string> SendMouseEventAsync(float x, float y, string button, string eventType,
        CancellationToken cancellationToken)
    {
        var request = new SendMouseEventRequest { x = x, y = y, button = button, eventType = eventType };
        await client.PostAsync(ApiRoutes.InputMouse, request, cancellationToken);
        return $"Mouse event sent: ({x}, {y}) button={button} ({eventType})";
    }
}
