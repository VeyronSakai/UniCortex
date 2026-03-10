using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;

namespace UniCortex.Cli.Test.UseCases;

[TestFixture]
public class ConsoleUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask GetLogs_ReturnsJsonWithLogs()
    {
        var json = await _fixture.ConsoleUseCase.GetLogsAsync(cancellationToken: CancellationToken.None);

        Assert.That(json, Does.Contain("logs"));
    }

    [Test]
    public async ValueTask GetLogs_WithCount_ReturnsLimitedEntries()
    {
        var json = await _fixture.ConsoleUseCase.GetLogsAsync(count: 5,
            cancellationToken: CancellationToken.None);

        Assert.That(json, Does.Contain("logs"));
    }

    [Test]
    public async ValueTask Clear_ReturnsSuccess()
    {
        var message = await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("cleared successfully"));
    }
}
