using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameViewCommands(GameViewUseCase gameViewUseCase)
{
    /// <summary>Switch focus to the Game View window.</summary>
    [Command("focus")]
    public async Task Focus(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.FocusAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Capture the Game View (or the Simulator view) as a PNG file. Only available in Play Mode.</summary>
    /// <param name="outputPath">File path to save the PNG image.</param>
    /// <param name="drawSafeArea">Draw the safe area outline and the cutouts on the image.</param>
    /// <param name="noDeviceFrame">Capture only the screen without the device frame of the Simulator view.</param>
    [Command("capture")]
    public async Task Capture([Argument] string outputPath, bool drawSafeArea = false, bool noDeviceFrame = false,
        CancellationToken cancellationToken = default)
    {
        var pngData = await gameViewUseCase.CaptureAsync(drawSafeArea, noDeviceFrame ? false : null,
            cancellationToken);
        await File.WriteAllBytesAsync(outputPath, pngData, cancellationToken);
        Console.WriteLine($"Game View captured to: {outputPath}");
    }

    /// <summary>Get the screen size, safe area and cutouts of the Game View or the Simulator view.</summary>
    [Command("safe-area")]
    public async Task SafeArea(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.GetSafeAreaAsync(cancellationToken);
        Console.WriteLine(message);
    }
}

public class GameViewSizeCommands(GameViewUseCase gameViewUseCase)
{
    /// <summary>Get the current Game View size (width and height in pixels).</summary>
    [Command("get")]
    public async Task Get(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.GetSizeAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>List all available Game View sizes (built-in and custom).</summary>
    [Command("list")]
    public async Task List(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.GetSizeListAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Set the Game View resolution by index from the size list.</summary>
    /// <param name="index">Index of the size from the size list.</param>
    [Command("set")]
    public async Task Set([Argument] int index, CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.SetSizeAsync(index, cancellationToken);
        Console.WriteLine(message);
    }
}

public class GameViewScaleCommands(GameViewUseCase gameViewUseCase)
{
    /// <summary>Get the current Game View scale (zoom factor) and its valid range.</summary>
    [Command("get")]
    public async Task Get(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.GetScaleAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Set the Game View scale (zoom factor). The value is clamped to the valid range.</summary>
    /// <param name="scale">Scale (zoom) factor. 1.0 = 100%.</param>
    [Command("set")]
    public async Task Set([Argument] float scale, CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.SetScaleAsync(scale, cancellationToken);
        Console.WriteLine(message);
    }
}

public class GameViewViewTypeCommands(GameViewUseCase gameViewUseCase)
{
    /// <summary>Get whether the Play Mode window shows the Game view or the Simulator view.</summary>
    [Command("get")]
    public async Task Get(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.GetViewTypeAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Switch the Play Mode window between the Game view and the Simulator view.</summary>
    /// <param name="viewType">View type: GameView or SimulatorView.</param>
    [Command("set")]
    public async Task Set([Argument] string viewType, CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.SetViewTypeAsync(viewType, cancellationToken);
        Console.WriteLine(message);
    }
}

public class SimulatorDeviceCommands(GameViewUseCase gameViewUseCase)
{
    /// <summary>List the devices of the Simulator view with the selected device and rotation.</summary>
    [Command("list")]
    public async Task List(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.GetSimulatorDeviceListAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Select the simulated device and/or its rotation in the Simulator view.</summary>
    /// <param name="index">Index of the device from the device list. Omit to keep the current device.</param>
    /// <param name="rotation">Clockwise rotation in degrees (0, 90, 180 or 270). Omit to keep the current rotation.</param>
    [Command("set")]
    public async Task Set(int? index = null, int? rotation = null, CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.SetSimulatorDeviceAsync(index, rotation, cancellationToken);
        Console.WriteLine(message);
    }
}
