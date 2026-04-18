using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class PlayModeUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);

        // Ensure not in play mode before tests (idempotent: no-op if already stopped)
        await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
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
        // Safety: ensure play mode is stopped (idempotent: no-op if already stopped)
        await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
    }
}
