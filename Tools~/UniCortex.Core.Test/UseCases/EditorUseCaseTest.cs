using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class EditorUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Ping_ReturnsSuccessWithPong()
    {
        var message = await _fixture.EditorUseCase.PingAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("pong"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Undo_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.UndoAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Redo_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.RedoAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ReloadDomain_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.ReloadDomainAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("completed"));
    }

    [Test, CancelAfter(120_000), Order(2)]
    public async ValueTask Ping_SucceedsAfterDomainReload()
    {
        var message = await _fixture.EditorUseCase.PingAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("pong"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetEditorStatus_ReturnsEditMode_WhenNotPlaying()
    {
        var message = await _fixture.EditorUseCase.GetEditorStatusAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("edit mode"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Step_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.StepAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Unpause_ReturnsSuccess()
    {
        var message = await _fixture.EditorUseCase.UnpauseAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(60_000)]
    public async ValueTask GetEditorStatus_ReturnsPaused_DuringPlayModeAndPause(CancellationToken cancellationToken)
    {
        var editor = _fixture.EditorUseCase;

        await editor.EnterPlayModeAsync(cancellationToken);
        try
        {
            await editor.PauseAsync(cancellationToken);

            var message = await editor.GetEditorStatusAsync(cancellationToken);
            Assert.That(message, Does.Contain("paused"));
        }
        finally
        {
            await editor.UnpauseAsync(CancellationToken.None);
            await editor.ExitPlayModeAsync(CancellationToken.None);
        }
    }
}
