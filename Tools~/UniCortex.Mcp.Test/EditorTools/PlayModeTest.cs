using NUnit.Framework;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.EditorTools;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[NonParallelizable]
public class PlayModeTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;
    private HttpClient _rawClient = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
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
    public async Task EnterPlayMode_ReturnsSuccess()
    {
        // Arrange
        var editorTools = _fixture.EditorTools;

        // Act
        var result = await editorTools.EnterPlayMode(CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("started"));
    }

    [Test, Order(2)]
    public async Task ExitPlayMode_ReturnsSuccess()
    {
        // Arrange
        var editorTools = _fixture.EditorTools;

        // Act
        var result = await editorTools.ExitPlayMode(CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("stopped"));
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
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

    private async Task WaitForPlayModeState(bool expectedPlaying)
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
