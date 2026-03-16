using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class EditorUseCaseTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };

    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Ping_ReturnsSuccessWithPong()
    {
        var message = await _fixture.EditorUseCase.PingAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("pong"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Undo_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.UndoAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Redo_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.RedoAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ReloadDomain_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.ReloadDomainAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("completed"));
    }

    [Test, CancelAfter(120_000), Order(2)]
    public async ValueTask Ping_SucceedsAfterDomainReload()
    {
        var message = await _fixture.EditorUseCase.PingAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("pong"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetEditorStatus_ReturnsEditMode_WhenNotPlaying()
    {
        var message = await _fixture.EditorUseCase.GetEditorStatusAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("edit mode"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Unpause_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.UnpauseAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(60_000)]
    public async ValueTask GetEditorStatus_ReturnsPaused_DuringPlayModeAndPause(CancellationToken cancellationToken)
    {
        var baseUrl = _fixture.BaseUrl;

        // Step 1: Enter play mode via raw POST
        using (var playClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
        using (var playResponse =
               await playClient.PostAsync($"{baseUrl}{ApiRoutes.Play}", null, cancellationToken))
        {
            playResponse.EnsureSuccessStatusCode();
        }

        // Step 2: Wait for domain reload to complete by polling /editor/status directly.
        // Each poll uses a new HttpClient to avoid Mono HttpListener keep-alive issues.
        EditorStatusResponse? playState = null;
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(1000, cancellationToken);
            try
            {
                using var pollClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                using var pollResponse =
                    await pollClient.GetAsync($"{baseUrl}{ApiRoutes.Status}", cancellationToken);
                if (pollResponse.IsSuccessStatusCode)
                {
                    var body = await pollResponse.Content.ReadAsStringAsync(cancellationToken);
                    if (!string.IsNullOrEmpty(body))
                    {
                        playState = JsonSerializer.Deserialize<EditorStatusResponse>(body, s_jsonOptions);
                        if (playState is { isPlaying: true })
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // Server still down during domain reload, keep polling
            }
        }

        try
        {
            // Step 3: Verify play mode is active
            Assert.That(playState, Is.Not.Null, "Never got a valid status response after entering play mode");
            Assert.That(playState!.isPlaying, Is.True, "Expected isPlaying to be true after entering play mode");

            // Step 4: Pause the editor (fresh client to avoid stale connection pool)
            using (var pauseClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
            using (var pauseResponse =
                   await pauseClient.PostAsync($"{baseUrl}{ApiRoutes.Pause}", null, cancellationToken))
            {
                pauseResponse.EnsureSuccessStatusCode();
            }

            // Wait for pause to take effect
            await Task.Delay(1000, cancellationToken);

            // Step 5: Verify paused state via raw HTTP (the core scenario)
            using (var statusClient2 = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
            using (var statusResponse =
                   await statusClient2.GetAsync($"{baseUrl}{ApiRoutes.Status}", cancellationToken))
            {
                statusResponse.EnsureSuccessStatusCode();
                var json = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
                var state = JsonSerializer.Deserialize<EditorStatusResponse>(json, s_jsonOptions)!;
                Assert.That(state.isPlaying, Is.True, "Expected isPlaying to be true");
                Assert.That(state.isPaused, Is.True, "Expected isPaused to be true");
            }

            // Step 6: Also verify via EditorUseCase (goes through HttpRequestHandler)
            var message = await _fixture.EditorUseCase.GetEditorStatusAsync(cancellationToken);
            Assert.That(message, Does.Contain("paused"));
        }
        finally
        {
            // Cleanup: unpause then exit play mode (fresh clients each time)
            try
            {
                using var c = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                await c.PostAsync($"{baseUrl}{ApiRoutes.Unpause}", null, CancellationToken.None);
            }
            catch
            {
                // best effort
            }

            await Task.Delay(500);

            try
            {
                using var c = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                await c.PostAsync($"{baseUrl}{ApiRoutes.Stop}", null, CancellationToken.None);
            }
            catch
            {
                // best effort
            }
        }
    }
}
