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
}
