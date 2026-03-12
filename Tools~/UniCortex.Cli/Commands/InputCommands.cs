using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class InputCommands(InputUseCase inputService)
{
    /// <summary>Send a keyboard event to the Game View in Play Mode.</summary>
    [Command("send-key")]
    public async Task SendKey(string keyName, string eventType = "keyDown",
        CancellationToken cancellationToken = default)
    {
        var message = await inputService.SendKeyEventAsync(keyName, eventType, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Send a mouse event to the Game View in Play Mode.</summary>
    [Command("send-mouse")]
    public async Task SendMouse(float x, float y, int button = 0, string eventType = "mouseDown",
        CancellationToken cancellationToken = default)
    {
        var message = await inputService.SendMouseEventAsync(x, y, button, eventType, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Send a keyboard event via Input System in Play Mode. Requires com.unity.inputsystem.</summary>
    [Command("send-system-key")]
    public async Task SendSystemKey(string key, string eventType = "press",
        CancellationToken cancellationToken = default)
    {
        var message = await inputService.SendInputSystemKeyEventAsync(key, eventType, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Send a mouse event via Input System in Play Mode. Requires com.unity.inputsystem.</summary>
    [Command("send-system-mouse")]
    public async Task SendSystemMouse(float x, float y, int button = 0, string eventType = "press",
        CancellationToken cancellationToken = default)
    {
        var message =
            await inputService.SendInputSystemMouseEventAsync(x, y, button, eventType, cancellationToken);
        Console.WriteLine(message);
    }
}
