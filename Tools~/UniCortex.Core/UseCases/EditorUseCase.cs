using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class EditorUseCase(IUnityEditorClient client)
{
    private static readonly TimeSpan s_pollInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan s_pollTimeout = TimeSpan.FromSeconds(30);

    public async ValueTask<string> PingAsync(CancellationToken cancellationToken)
    {
        var ping = await client.GetAsync<PingRequest, PingResponse>(
            ApiRoutes.Ping, new PingRequest { verbose = true }, cancellationToken);
        return ping.message;
    }

    public async ValueTask<string> EnterPlayModeAsync(CancellationToken cancellationToken)
    {
        await client.WaitForServerAsync(cancellationToken);

        if (await GetIsPlayingAsync(cancellationToken))
        {
            return "Editor is already in play mode.";
        }

        await client.PostAsync<PlayRequest, PlayResponse>(ApiRoutes.Play, cancellationToken: cancellationToken);

        await WaitForPlayModeStateAsync(expectedPlaying: true, cancellationToken);
        return "Play mode started successfully.";
    }

    public async ValueTask<string> ExitPlayModeAsync(CancellationToken cancellationToken)
    {
        if (!await GetIsPlayingAsync(cancellationToken))
        {
            return "Editor is not in play mode.";
        }

        await client.PostAsync<StopRequest, StopResponse>(ApiRoutes.Stop, cancellationToken: cancellationToken);

        await WaitForPlayModeStateAsync(expectedPlaying: false, cancellationToken);
        return "Play mode stopped successfully.";
    }

    public async ValueTask<string> GetEditorStatusAsync(CancellationToken cancellationToken)
    {
        var state = await client.GetAsync<GetEditorStatusRequest, GetEditorStatusResponse>(ApiRoutes.Status,
            cancellationToken: cancellationToken);

        if (state.isPlaying && state.isPaused)
        {
            return "Editor is in play mode and paused.";
        }

        if (state.isPlaying)
        {
            return "Editor is in play mode.";
        }

        return "Editor is in edit mode.";
    }

    public async ValueTask<string> PauseAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<PauseRequest, PauseResponse>(ApiRoutes.Pause, cancellationToken: cancellationToken);
        return "Editor paused successfully.";
    }

    public async ValueTask<string> UnpauseAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<UnpauseRequest, UnpauseResponse>(ApiRoutes.Unpause, cancellationToken: cancellationToken);
        return "Editor unpaused successfully.";
    }

    public async ValueTask<string> StepAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<StepRequest, StepResponse>(ApiRoutes.Step, cancellationToken: cancellationToken);
        return "Editor stepped one frame successfully.";
    }

    public async ValueTask<string> UndoAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<UndoRequest, UndoResponse>(ApiRoutes.Undo, cancellationToken: cancellationToken);
        return "Undo performed successfully.";
    }

    public async ValueTask<string> RedoAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<RedoRequest, RedoResponse>(ApiRoutes.Redo, cancellationToken: cancellationToken);
        return "Redo performed successfully.";
    }

    public async ValueTask<string> ReloadDomainAsync(CancellationToken cancellationToken)
    {
        // Wait for the server to become available before triggering domain reload.
        // If Unity is already auto-recompiling (e.g. after a .cs file change),
        // the server will be unavailable; this prevents a double RequestScriptCompilation() call
        // that can freeze Unity.
        await client.WaitForServerAsync(cancellationToken);

        await client.PostAsync<DomainReloadRequest, DomainReloadResponse>(ApiRoutes.DomainReload,
            cancellationToken: cancellationToken);

        // RequestScriptCompilation() is dispatched asynchronously on the Unity main thread.
        // Wait briefly so that compilation starts and the server becomes unavailable
        // before we begin polling /ping.
        await Task.Delay(100, cancellationToken);

        await client.WaitForServerAsync(cancellationToken);

        return "Domain reload completed successfully.";
    }

    public async ValueTask<string> SetTimeScaleAsync(float timeScale, CancellationToken cancellationToken)
    {
        var response = await client.PostAsync<SetTimeScaleRequest, SetTimeScaleResponse>(
            ApiRoutes.TimeScale, new SetTimeScaleRequest { timeScale = timeScale }, cancellationToken);
        return $"Time scale set to {response.timeScale} successfully.";
    }

    public async ValueTask<string> GetTimeScaleAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetTimeScaleRequest, GetTimeScaleResponse>(
            ApiRoutes.TimeScale, cancellationToken: cancellationToken);
        return $"Current time scale: {response.timeScale}";
    }

    private async ValueTask<bool> GetIsPlayingAsync(CancellationToken cancellationToken)
    {
        var status = await client.GetAsync<GetEditorStatusRequest, GetEditorStatusResponse>(ApiRoutes.Status,
            cancellationToken: cancellationToken);
        return status.isPlaying;
    }

    private async ValueTask WaitForPlayModeStateAsync(bool expectedPlaying,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + s_pollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(s_pollInterval, cancellationToken);
            if (await GetIsPlayingAsync(cancellationToken) == expectedPlaying)
            {
                return;
            }
        }

        throw new TimeoutException(
            $"Timed out waiting for Editor to {(expectedPlaying ? "enter" : "exit")} play mode.");
    }
}
