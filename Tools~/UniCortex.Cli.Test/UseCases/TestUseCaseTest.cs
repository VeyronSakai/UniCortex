using System.Text.Json;
using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Test.UseCases;

[TestFixture]
public class TestUseCaseTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask Run_EditMode_ReturnsJsonWithResults()
    {
        var json = await _fixture.TestUseCase.RunAsync(testMode: TestModes.EditMode,
            cancellationToken: CancellationToken.None);

        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, s_jsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.passed + response.failed + response.skipped, Is.GreaterThan(0));
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask Run_PlayMode_ReturnsJson()
    {
        var json = await _fixture.TestUseCase.RunAsync(testMode: TestModes.PlayMode,
            cancellationToken: CancellationToken.None);

        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, s_jsonOptions);
        Assert.That(response, Is.Not.Null);
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask Run_WithTestNames_NonExistent_ReturnsZeroResults()
    {
        var json = await _fixture.TestUseCase.RunAsync(
            testMode: TestModes.EditMode,
            testNames: ["NonExistentTest_A", "NonExistentTest_B"],
            cancellationToken: CancellationToken.None);

        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, s_jsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.results, Has.Count.EqualTo(0));
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask Run_WithCategoryNames_NonExistent_ReturnsZeroResults()
    {
        var json = await _fixture.TestUseCase.RunAsync(
            testMode: TestModes.EditMode,
            categoryNames: ["NonExistentCategory_99999"],
            cancellationToken: CancellationToken.None);

        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, s_jsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.results, Has.Count.EqualTo(0));
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask Run_WithAssemblyNames_NonExistent_ReturnsZeroResults()
    {
        var json = await _fixture.TestUseCase.RunAsync(
            testMode: TestModes.EditMode,
            assemblyNames: ["NonExistentAssembly_99999"],
            cancellationToken: CancellationToken.None);

        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, s_jsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.results, Has.Count.EqualTo(0));
    }
}
