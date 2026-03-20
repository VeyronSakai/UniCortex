using System.Text.Json;
using UniCortex.Core.Extensions;
using UniCortex.Core.Infrastructures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class EditorUseCase(UnityEditorClient client)
{
    private static readonly TimeSpan s_pollInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan s_pollTimeout = TimeSpan.FromSeconds(30);

    public async ValueTask<string> PingAsync(CancellationToken cancellationToken)
    {
        var baseUrl = client.BaseUrl;
        await client.WaitForServerAsync(cancellationToken);

        using var response = await client.HttpClient.GetAsync(
            $"{baseUrl}{ApiRoutes.Ping}?{QueryParameterNames.Verbose}=true", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var ping = JsonSerializer.Deserialize<PingResponse>(json, UnityEditorClient.JsonOptions)!;
        return ping.message;
    }

    public async ValueTask<string> EnterPlayModeAsync(CancellationToken cancellationToken)
    {
        var baseUrl = client.BaseUrl;
        await client.WaitForServerAsync(cancellationToken);

        if (await GetIsPlayingAsync(baseUrl, cancellationToken))
        {
            return "Editor is already in play mode.";
        }

        using var response = await client.HttpClient.PostAsync($"{baseUrl}{ApiRoutes.Play}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        await WaitForPlayModeStateAsync(baseUrl, expectedPlaying: true, cancellationToken);
        return "Play mode started successfully.";
    }

    public async ValueTask<string> ExitPlayModeAsync(CancellationToken cancellationToken)
    {
        var baseUrl = client.BaseUrl;

        if (!await GetIsPlayingAsync(baseUrl, cancellationToken))
        {
            return "Editor is not in play mode.";
        }

        using var response = await client.HttpClient.PostAsync($"{baseUrl}{ApiRoutes.Stop}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        await WaitForPlayModeStateAsync(baseUrl, expectedPlaying: false, cancellationToken);
        return "Play mode stopped successfully.";
    }

    public async ValueTask<string> GetEditorStatusAsync(CancellationToken cancellationToken)
    {
        var baseUrl = client.BaseUrl;

        using var response = await client.HttpClient.GetAsync($"{baseUrl}{ApiRoutes.Status}", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var state = JsonSerializer.Deserialize<EditorStatusResponse>(json, UnityEditorClient.JsonOptions)!;

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

    public ValueTask<string> PauseAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.Pause, "Editor paused successfully.", cancellationToken);
    }

    public ValueTask<string> UnpauseAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.Unpause, "Editor unpaused successfully.", cancellationToken);
    }

    public ValueTask<string> StepAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.Step, "Editor stepped one frame successfully.", cancellationToken);
    }

    public ValueTask<string> UndoAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.Undo, "Undo performed successfully.", cancellationToken);
    }

    public ValueTask<string> RedoAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.Redo, "Redo performed successfully.", cancellationToken);
    }

    public async ValueTask<string> ReloadDomainAsync(CancellationToken cancellationToken)
    {
        var baseUrl = client.BaseUrl;

        // Wait for the server to become available before triggering domain reload.
        // If Unity is already auto-recompiling (e.g. after a .cs file change),
        // the server will be unavailable; this prevents a double RequestScriptCompilation() call
        // that can freeze Unity.
        await client.WaitForServerAsync(cancellationToken);

        using var response = await client.HttpClient.PostAsync($"{baseUrl}{ApiRoutes.DomainReload}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        // RequestScriptCompilation() is dispatched asynchronously on the Unity main thread.
        // Wait briefly so that compilation starts and the server becomes unavailable
        // before we begin polling /ping.
        await Task.Delay(100, cancellationToken);

        await client.WaitForServerAsync(cancellationToken);

        return "Domain reload completed successfully.";
    }

    private async ValueTask<bool> GetIsPlayingAsync(string baseUrl, CancellationToken cancellationToken)
    {
        using var statusResponse = await client.HttpClient.GetAsync($"{baseUrl}{ApiRoutes.Status}", cancellationToken);
        await statusResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        var statusJson = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
        var status = JsonSerializer.Deserialize<EditorStatusResponse>(statusJson, UnityEditorClient.JsonOptions)!;
        return status.isPlaying;
    }

    private async ValueTask WaitForPlayModeStateAsync(string baseUrl, bool expectedPlaying,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + s_pollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(s_pollInterval, cancellationToken);
            if (await GetIsPlayingAsync(baseUrl, cancellationToken) == expectedPlaying)
            {
                return;
            }
        }

        throw new TimeoutException(
            $"Timed out waiting for Editor to {(expectedPlaying ? "enter" : "exit")} play mode.");
    }
}
