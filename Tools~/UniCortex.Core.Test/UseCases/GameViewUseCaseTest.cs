using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class GameViewUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Focus_Succeeds()
    {
        var result = await _fixture.GameViewUseCase.FocusAsync(CancellationToken.None);

        Assert.That(result, Does.Contain("successfully"));
    }

    [Test]
    public async ValueTask GetSize_ReturnsScreenSize()
    {
        var result = await _fixture.GameViewUseCase.GetSizeAsync(CancellationToken.None);

        Assert.That(result, Does.Contain("Game View size:"));
        Assert.That(result, Does.Match(@"\d+x\d+"));
    }
}
