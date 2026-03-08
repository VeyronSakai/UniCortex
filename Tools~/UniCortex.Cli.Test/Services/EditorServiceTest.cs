using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;

namespace UniCortex.Cli.Test.Services;

[TestFixture]
public class EditorServiceTest
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
        var message = await _fixture.EditorService.PingAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("pong"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Undo_ReturnsSuccess()
    {
        var message = await _fixture.EditorService.UndoAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Redo_ReturnsSuccess()
    {
        var message = await _fixture.EditorService.RedoAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("successfully"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ReloadDomain_ReturnsSuccess()
    {
        var message = await _fixture.EditorService.ReloadDomainAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("completed"));
    }

    [Test, CancelAfter(120_000), Order(2)]
    public async ValueTask Ping_SucceedsAfterDomainReload()
    {
        var message = await _fixture.EditorService.PingAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("pong"));
    }
}
