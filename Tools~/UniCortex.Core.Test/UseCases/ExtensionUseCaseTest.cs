using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class ExtensionUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask List_ReturnsSampleExtensions()
    {
        var response = await _fixture.ExtensionUseCase.ListAsync(CancellationToken.None);

        Assert.That(response.extensions, Is.Not.Null);

        var names = response.extensions.Select(e => e.name).ToList();
        Assert.That(names, Does.Contain("sample_count_gameobjects"));
        Assert.That(names, Does.Contain("sample_list_components"));
    }

    [Test]
    public async ValueTask List_ReturnsMetadataForSampleCountGameObjects()
    {
        var response = await _fixture.ExtensionUseCase.ListAsync(CancellationToken.None);

        var info = response.extensions.First(e => e.name == "sample_count_gameobjects");

        Assert.That(info.description, Does.Contain("Count GameObjects"));
        Assert.That(info.readOnly, Is.True);
        Assert.That(info.inputSchema, Does.Contain("nameFilter"));
    }

    [Test]
    public async ValueTask List_ReturnsMetadataForSampleListComponents()
    {
        var response = await _fixture.ExtensionUseCase.ListAsync(CancellationToken.None);

        var info = response.extensions.First(e => e.name == "sample_list_components");

        Assert.That(info.description, Does.Contain("components"));
        Assert.That(info.readOnly, Is.True);
        Assert.That(info.inputSchema, Does.Contain("gameObjectName"));
        Assert.That(info.inputSchema, Does.Contain("\"required\":[\"gameObjectName\"]"));
    }

    [Test]
    public async ValueTask Execute_SampleCountGameObjects_ReturnsCount()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_count_gameobjects", null, CancellationToken.None);

        Assert.That(result, Does.Match(@"Found \d+ GameObject\(s\) in the current scene\."));
    }

    [Test]
    public async ValueTask Execute_SampleCountGameObjects_WithNameFilter_ReturnsFilteredCount()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_count_gameobjects",
            "{\"nameFilter\":\"Camera\"}",
            CancellationToken.None);

        Assert.That(result, Does.Contain("matching \"Camera\""));
    }

    [Test]
    public async ValueTask Execute_SampleCountGameObjects_WithEmptyArguments_ReturnsCount()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_count_gameobjects", "", CancellationToken.None);

        Assert.That(result, Does.Contain("GameObject"));
    }

    [Test]
    public async ValueTask Execute_SampleListComponents_WithExistingGameObject_ReturnsComponents()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_list_components",
            "{\"gameObjectName\":\"Camera\"}",
            CancellationToken.None);

        Assert.That(result, Does.Contain("Components on \"Camera\""));
        Assert.That(result, Does.Contain("UnityEngine.Camera"));
    }

    [Test]
    public async ValueTask Execute_SampleListComponents_WithUnknownGameObject_ReturnsNotFoundMessage()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_list_components",
            "{\"gameObjectName\":\"__DoesNotExist__\"}",
            CancellationToken.None);

        Assert.That(result, Does.Contain("not found"));
    }

    [Test]
    public async ValueTask Execute_SampleListComponents_WithoutArguments_ReturnsErrorMessage()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_list_components", null, CancellationToken.None);

        Assert.That(result, Does.Contain("gameObjectName is required"));
    }
}
