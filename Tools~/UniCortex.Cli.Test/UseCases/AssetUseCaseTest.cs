using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;

namespace UniCortex.Cli.Test.UseCases;

[TestFixture]
public class AssetUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Refresh_ReturnsSuccess()
    {
        var message = await _fixture.AssetUseCase.RefreshAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("refreshed"));
    }
}
