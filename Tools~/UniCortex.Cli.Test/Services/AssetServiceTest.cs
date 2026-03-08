using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;

namespace UniCortex.Cli.Test.Services;

[TestFixture]
public class AssetServiceTest
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
        var message = await _fixture.AssetService.RefreshAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("refreshed"));
    }
}
