using NUnit.Framework;
using UniCortex.Core.Infrastructures;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class UnityEditorProcessProviderTest
{
    private string _projectPath = "";

    [SetUp]
    public void SetUp()
    {
        _projectPath = Path.Combine(Path.GetTempPath(), $"UniCortexTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_projectPath, "Library"));
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_projectPath, recursive: true);
    }

    [Test]
    public void GetProcessId_ReturnsProcessIdInEditorInstanceFile()
    {
        // Arrange
        var processId = Environment.ProcessId;
        WriteEditorInstance($"{{\"process_id\" : {processId}, \"version\" : \"6000.3.10f1\"}}");

        // Act
        var result = UnityEditorProcessProvider.GetProcessId(_projectPath);

        // Assert
        Assert.That(result, Is.EqualTo(processId));
    }

    [Test]
    public void GetProcessId_Throws_WhenEditorInstanceFileDoesNotExist()
    {
        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => UnityEditorProcessProvider.GetProcessId(_projectPath));

        // Assert
        Assert.That(ex!.Message, Does.Contain("EditorInstance.json"));
    }

    [Test]
    public void GetProcessId_Throws_WhenProcessIsNotRunning()
    {
        // Arrange
        WriteEditorInstance($"{{\"process_id\" : {int.MaxValue}}}");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => UnityEditorProcessProvider.GetProcessId(_projectPath));

        // Assert
        Assert.That(ex!.Message, Does.Contain("is not running"));
    }

    private void WriteEditorInstance(string json)
    {
        File.WriteAllText(Path.Combine(_projectPath, "Library", "EditorInstance.json"), json);
    }
}
