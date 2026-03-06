using System.Text.Json;
using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.TestTools;

[TestFixture]
public class RunTestsTest
{
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };

    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask RunTests_EditMode_ReturnsJsonWithResults()
    {
        // Arrange
        var testTools = _fixture.TestTools;

        // Act
        var result = await testTools.RunTestsAsync(testMode: TestModes.EditMode, cancellationToken: CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var json = ((TextContentBlock)result.Content[0]).Text;
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, JsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.passed + response.failed + response.skipped, Is.GreaterThan(0));
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask RunTests_PlayMode_ReturnsJson()
    {
        // Arrange
        var testTools = _fixture.TestTools;

        // Act
        var result = await testTools.RunTestsAsync(testMode: TestModes.PlayMode, cancellationToken: CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var json = ((TextContentBlock)result.Content[0]).Text;
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, JsonOptions);
        Assert.That(response, Is.Not.Null);
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask RunTests_WithTestNames_NonExistent_ReturnsZeroResults()
    {
        // Arrange
        var testTools = _fixture.TestTools;

        // Act
        var result = await testTools.RunTestsAsync(
            testMode: TestModes.EditMode,
            testNames: ["NonExistentTest_A", "NonExistentTest_B"],
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        var json = ((TextContentBlock)result.Content[0]).Text;
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, JsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.results, Has.Count.EqualTo(0));
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask RunTests_WithCategoryNames_NonExistent_ReturnsZeroResults()
    {
        // Arrange
        var testTools = _fixture.TestTools;

        // Act
        var result = await testTools.RunTestsAsync(
            testMode: TestModes.EditMode,
            categoryNames: ["NonExistentCategory_99999"],
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        var json = ((TextContentBlock)result.Content[0]).Text;
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, JsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.results, Has.Count.EqualTo(0));
    }

    [Test, CancelAfter(300_000)]
    public async ValueTask RunTests_WithAssemblyNames_NonExistent_ReturnsZeroResults()
    {
        // Arrange
        var testTools = _fixture.TestTools;

        // Act
        var result = await testTools.RunTestsAsync(
            testMode: TestModes.EditMode,
            assemblyNames: ["NonExistentAssembly_99999"],
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        var json = ((TextContentBlock)result.Content[0]).Text;
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, JsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.results, Has.Count.EqualTo(0));
    }
}
