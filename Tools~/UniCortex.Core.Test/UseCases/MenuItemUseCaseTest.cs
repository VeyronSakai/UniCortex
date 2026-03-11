using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class MenuItemUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Execute_ReturnsSuccess()
    {
        var message = await _fixture.MenuItemUseCase.ExecuteAsync("Edit/Select All",
            CancellationToken.None);

        Assert.That(message, Does.Contain("executed"));
    }
}
