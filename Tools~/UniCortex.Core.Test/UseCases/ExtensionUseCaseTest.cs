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
}
