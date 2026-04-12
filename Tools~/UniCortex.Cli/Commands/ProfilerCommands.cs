using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ProfilerCommands(ProfilerUseCase profilerUseCase)
{
    /// <summary>Open or focus the Profiler window in the Unity Editor.</summary>
    [Command("focus")]
    public async Task Focus(CancellationToken cancellationToken = default)
    {
        var message = await profilerUseCase.FocusWindowAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the current Profiler window and recording state as JSON.</summary>
    [Command("status")]
    public async Task Status(CancellationToken cancellationToken = default)
    {
        var message = await profilerUseCase.GetStatusAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Start Profiler recording in the Unity Editor.</summary>
    /// <param name="profileEditor">When true, enable Editor profiling. Default: false.</param>
    [Command("start")]
    public async Task Start(bool profileEditor = false, CancellationToken cancellationToken = default)
    {
        var message = await profilerUseCase.StartRecordingAsync(profileEditor, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Stop Profiler recording in the Unity Editor.</summary>
    [Command("stop")]
    public async Task Stop(CancellationToken cancellationToken = default)
    {
        var message = await profilerUseCase.StopRecordingAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
