using System.Text.Json;
using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Test.Services;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class PlayModeServiceTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;
    private HttpClient _rawClient = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        _rawClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        // Ensure not in play mode before tests
        try
        {
            var statusResponse = await _rawClient.GetAsync($"{_fixture.BaseUrl}{ApiRoutes.Status}");
            var json = await statusResponse.Content.ReadAsStringAsync();
            var status = JsonSerializer.Deserialize<EditorStatusResponse>(json, s_jsonOptions);
            if (status is { isPlaying: true })
            {
                await _rawClient.PostAsync($"{_fixture.BaseUrl}{ApiRoutes.Stop}", null);
                await WaitForPlayModeState(false);
            }
        }
        catch
        {
            // Already handled by fixture's connection check
        }
    }

    [Test, Order(1)]
    public async ValueTask EnterPlayMode_ReturnsSuccess()
    {
        var message = await _fixture.EditorService.EnterPlayModeAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("started"));
    }

    [Test, Order(2)]
    public async ValueTask ExitPlayMode_ReturnsSuccess()
    {
        var message = await _fixture.EditorService.ExitPlayModeAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("stopped"));
    }

    [OneTimeTearDown]
    public async ValueTask OneTimeTearDown()
    {
        // Safety: ensure play mode is stopped
        try
        {
            var statusResponse = await _rawClient.GetAsync($"{_fixture.BaseUrl}{ApiRoutes.Status}");
            var json = await statusResponse.Content.ReadAsStringAsync();
            var status = JsonSerializer.Deserialize<EditorStatusResponse>(json, s_jsonOptions);
            if (status is { isPlaying: true })
            {
                await _rawClient.PostAsync($"{_fixture.BaseUrl}{ApiRoutes.Stop}", null);
            }
        }
        catch
        {
            // Best effort cleanup
        }
        finally
        {
            _rawClient.Dispose();
        }
    }

    private async ValueTask WaitForPlayModeState(bool expectedPlaying)
    {
        for (var i = 0; i < 30; i++)
        {
            await Task.Delay(500);
            try
            {
                var response = await _rawClient.GetAsync($"{_fixture.BaseUrl}{ApiRoutes.Status}");
                var json = await response.Content.ReadAsStringAsync();
                var status = JsonSerializer.Deserialize<EditorStatusResponse>(json, s_jsonOptions);
                if (status?.isPlaying == expectedPlaying)
                {
                    return;
                }
            }
            catch
            {
                // Retry
            }
        }
    }
}
