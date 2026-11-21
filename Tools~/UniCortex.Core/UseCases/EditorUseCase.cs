using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class EditorUseCase(IUnityEditorClient client)
{
    internal TimeSpan PlayModePollInterval { get; init; } = TimeSpan.FromSeconds(1);

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
        var state = await GetStatusAsync(cancellationToken);

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

    public async ValueTask<GetEditorStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        return await client.GetAsync<GetEditorStatusRequest, GetEditorStatusResponse>(ApiRoutes.Status,
            cancellationToken: cancellationToken);
    }

    public async ValueTask<string> PauseAsync(CancellationToken cancellationToken)
    {
        await client.WaitForServerAsync(cancellationToken);
        await client.PostAsync<PauseRequest, PauseResponse>(ApiRoutes.Pause, cancellationToken: cancellationToken);
        return "Editor paused successfully.";
    }

    public async ValueTask<string> UnpauseAsync(CancellationToken cancellationToken)
    {
        await client.WaitForServerAsync(cancellationToken);
        await client.PostAsync<UnpauseRequest, UnpauseResponse>(ApiRoutes.Unpause, cancellationToken: cancellationToken);
        return "Editor unpaused successfully.";
    }

    public async ValueTask<string> StepAsync(CancellationToken cancellationToken)
    {
        await client.WaitForServerAsync(cancellationToken);
        await client.PostAsync<StepRequest, StepResponse>(ApiRoutes.Step, cancellationToken: cancellationToken);
        return "Editor stepped one frame successfully.";
    }

    public async ValueTask<string> UndoAsync(CancellationToken cancellationToken)
    {
        await client.WaitForServerAsync(cancellationToken);
        await client.PostAsync<UndoRequest, UndoResponse>(ApiRoutes.Undo, cancellationToken: cancellationToken);
        return "Undo performed successfully.";
    }

    public async ValueTask<string> RedoAsync(CancellationToken cancellationToken)
    {
        await client.WaitForServerAsync(cancellationToken);
        await client.PostAsync<RedoRequest, RedoResponse>(ApiRoutes.Redo, cancellationToken: cancellationToken);
        return "Redo performed successfully.";
    }

    public async ValueTask<string> SaveAsync(CancellationToken cancellationToken)
    {
        await client.WaitForServerAsync(cancellationToken);
        await client.PostAsync<SaveRequest, SaveResponse>(ApiRoutes.EditorSave, cancellationToken: cancellationToken);
        return "Saved successfully.";
    }

    public async ValueTask<string> ReloadDomainAsync(CancellationToken cancellationToken)
    {
        // Wait for the server to become available before triggering domain reload.
        // If Unity is already auto-recompiling (e.g. after a .cs file change),
        // the server will be unavailable; this prevents a double RequestScriptCompilation() call
        // that can freeze Unity.
        await client.WaitForServerAsync(cancellationToken);

        // The server responds when the compilation has finished: right before the domain reload starts,
        // or with an error (thrown here) when the compilation failed and the domain is not reloaded.
        await client.PostAsync<DomainReloadRequest, DomainReloadResponse>(ApiRoutes.DomainReload,
            cancellationToken: cancellationToken);

        // /editor/status runs on the main thread, which is busy with the reload until the old server has stopped,
        // so it is answered by the server of the new domain. Requests sent meanwhile are resent by HttpRequestHandler.
        await GetStatusAsync(cancellationToken);

        return "Domain reload completed successfully.";
    }

    public async ValueTask<string> GetActivePlatformAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetActivePlatformRequest, GetActivePlatformResponse>(
            ApiRoutes.Platform, cancellationToken: cancellationToken);
        return $"Active platform: {response.activeBuildTarget}";
    }

    public async ValueTask<string> SwitchPlatformAsync(string buildTarget, CancellationToken cancellationToken)
    {
        // Same as ReloadDomainAsync: avoid starting a switch while Unity is recompiling.
        await client.WaitForServerAsync(cancellationToken);

        // The server responds after assets have been reimported and scripts recompiled for the new platform,
        // right before the domain reload starts.
        var response = await client.PostAsync<SwitchPlatformRequest, SwitchPlatformResponse>(
            ApiRoutes.PlatformSwitch, new SwitchPlatformRequest { buildTarget = buildTarget }, cancellationToken);

        if (response.previousBuildTarget == response.activeBuildTarget)
        {
            return $"Platform is already {response.activeBuildTarget}.";
        }

        // Wait until the server of the new domain answers, as ReloadDomainAsync does.
        await GetStatusAsync(cancellationToken);

        return $"Platform switched from {response.previousBuildTarget} to {response.activeBuildTarget} successfully.";
    }

    private async ValueTask<bool> GetIsPlayingAsync(CancellationToken cancellationToken)
    {
        var status = await GetStatusAsync(cancellationToken);
        return status.isPlaying;
    }

    // There is no time limit, since entering play mode can take long: scripts that are still compiling are compiled
    // first, and the Editor answers requests meanwhile. isPlayingOrWillChangePlaymode tells whether the Editor is still
    // heading for the expected state, so that a change it refused (e.g. play mode with compile errors) fails at once.
    private async ValueTask WaitForPlayModeStateAsync(bool expectedPlaying,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(PlayModePollInterval, cancellationToken);
            var status = await GetStatusAsync(cancellationToken);
            if (status.isPlaying == expectedPlaying)
            {
                return;
            }

            if (status.isPlayingOrWillChangePlaymode != expectedPlaying)
            {
                throw new InvalidOperationException(expectedPlaying
                    ? "The Unity Editor did not enter play mode. Scripts may have compile errors: check them with " +
                      "get_console_logs (CLI: console logs)."
                    : "The Unity Editor did not exit play mode.");
            }
        }
    }
}
