using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

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

    [Test, CancelAfter(120_000)]
    public async ValueTask GetLogs_ReturnsEntriesRegardlessOfConsoleVisibilityFilters()
    {
        await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

        await _fixture.MenuItemUseCase.ExecuteAsync("UniCortex/Debug/Log Info", CancellationToken.None);
        await _fixture.MenuItemUseCase.ExecuteAsync("UniCortex/Debug/Log Warning", CancellationToken.None);
        await _fixture.MenuItemUseCase.ExecuteAsync("UniCortex/Debug/Log Error", CancellationToken.None);
        await Task.Delay(200);

        var infoLogs = await _fixture.ConsoleUseCase.GetLogsAsync(count: 20, log: true, warning: false, error: false,
            cancellationToken: CancellationToken.None);
        var warningLogs = await _fixture.ConsoleUseCase.GetLogsAsync(count: 20, log: false, warning: true, error: false,
            cancellationToken: CancellationToken.None);
        var errorLogs = await _fixture.ConsoleUseCase.GetLogsAsync(count: 20, log: false, warning: false, error: true,
            cancellationToken: CancellationToken.None);

        Assert.That(infoLogs, Does.Contain("[UniCortex] Sample info message"));
        Assert.That(warningLogs, Does.Contain("[UniCortex] Sample warning message"));
        Assert.That(errorLogs, Does.Contain("[UniCortex] Sample error message"));
    }
}
