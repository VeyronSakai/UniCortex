using UniCortex.Core.Infrastructures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class InputUseCase(UnityEditorClient client)
{
    public ValueTask<string> SendKeyEventAsync(string key, string eventType,
        CancellationToken cancellationToken)
    {
        var request = new SendKeyEventRequest { key = key, eventType = eventType };
        return client.PostAsync(ApiRoutes.InputKey, request, $"Key event sent: {key} ({eventType})",
            cancellationToken);
    }

    public ValueTask<string> SendMouseEventAsync(float x, float y, string button, string eventType,
        CancellationToken cancellationToken)
    {
        var request = new SendMouseEventRequest { x = x, y = y, button = button, eventType = eventType };
        return client.PostAsync(ApiRoutes.InputMouse, request,
            $"Mouse event sent: ({x}, {y}) button={button} ({eventType})", cancellationToken);
    }
}
