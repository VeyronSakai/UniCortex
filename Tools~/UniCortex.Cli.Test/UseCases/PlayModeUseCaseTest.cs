using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;

namespace UniCortex.Cli.Test.UseCases;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class PlayModeUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();

        // Ensure not in play mode before tests
        try
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
        catch
        {
            // Already not in play mode
        }
    }

    [Test, Order(1)]
    public async ValueTask EnterPlayMode_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("started"));
    }

    [Test, Order(2)]
    public async ValueTask ExitPlayMode_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("stopped"));
    }

    [OneTimeTearDown]
    public async ValueTask OneTimeTearDown()
    {
        // Safety: ensure play mode is stopped
        try
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
        catch
        {
            // Best effort cleanup
        }
    }
}
