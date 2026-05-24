using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class ProjectSettingsUseCaseTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask ListCategories_ContainsProjectSettings()
    {
        var json = await _fixture.ProjectSettingsUseCase.GetCategoriesAsync(CancellationToken.None);

        Assert.That(json, Does.Contain("ProjectSettings"));
        Assert.That(json, Does.Contain("ProjectSettings/ProjectSettings.asset"));
    }

    [Test]
    public async ValueTask GetSettings_ProjectSettings_ReturnsProperties()
    {
        var json = await _fixture.ProjectSettingsUseCase.GetAsync("ProjectSettings", CancellationToken.None);
        var response = JsonSerializer.Deserialize<GetProjectSettingsResponse>(json, s_jsonOptions)!;

        Assert.That(response.category, Is.EqualTo("ProjectSettings"));
        Assert.That(response.properties, Is.Not.Empty);
    }

    [Test]
    public async ValueTask Get_UnknownCategory_Throws()
    {
        Assert.That(async () => await _fixture.ProjectSettingsUseCase.GetAsync("NotARealCategory",
            CancellationToken.None), Throws.Exception);
    }

    [Test]
    public async ValueTask SetSetting_UpdatesValue_And_Restores()
    {
        var ct = CancellationToken.None;

        // Capture the original m_TimeScale so the change can be reverted (no side effects).
        var beforeJson = await _fixture.ProjectSettingsUseCase.GetAsync("TimeManager", ct);
        var before = JsonSerializer.Deserialize<GetProjectSettingsResponse>(beforeJson, s_jsonOptions)!;
        var timeScale = before.properties.Find(p => p.path == "m_TimeScale");
        Assert.That(timeScale, Is.Not.Null, "m_TimeScale property should exist in Time settings.");
        var original = timeScale!.value;

        try
        {
            var message = await _fixture.ProjectSettingsUseCase.SetAsync("TimeManager", "m_TimeScale", "2", ct);
            Assert.That(message, Does.Contain("successfully"));

            var afterJson = await _fixture.ProjectSettingsUseCase.GetAsync("TimeManager", ct);
            var after = JsonSerializer.Deserialize<GetProjectSettingsResponse>(afterJson, s_jsonOptions)!;
            var updated = after.properties.Find(p => p.path == "m_TimeScale");
            Assert.That(updated!.value, Is.EqualTo("2"));
        }
        finally
        {
            await _fixture.ProjectSettingsUseCase.SetAsync("TimeManager", "m_TimeScale", original, ct);
        }
    }
}
