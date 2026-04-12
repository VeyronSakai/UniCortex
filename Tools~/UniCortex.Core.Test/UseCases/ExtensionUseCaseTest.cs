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
    public async ValueTask List_ReturnsResponse()
    {
        var response = await _fixture.ExtensionUseCase.ListAsync(CancellationToken.None);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.extensions, Is.Not.Null);
    }

    [Test]
    public async ValueTask Execute_ReturnsResult()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_count_gameobjects", null, CancellationToken.None);

        Assert.That(result, Does.Contain("GameObject"));
    }

    [Test]
    public async ValueTask Execute_WithArguments_ReturnsFilteredResult()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_count_gameobjects", "{\"nameFilter\":\"Camera\"}", CancellationToken.None);

        Assert.That(result, Does.Contain("Camera"));
    }

    [Test]
    public async ValueTask Execute_WithEmptyArguments_ReturnsResult()
    {
        var result = await _fixture.ExtensionUseCase.ExecuteAsync(
            "sample_count_gameobjects", "", CancellationToken.None);

        Assert.That(result, Does.Contain("GameObject"));
    }
}
