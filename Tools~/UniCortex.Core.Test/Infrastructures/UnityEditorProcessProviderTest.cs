using NUnit.Framework;
using UniCortex.Core.Infrastructures;
using UnityProcess = UniCortex.Core.Infrastructures.UnityEditorProcessProvider.UnityProcess;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class UnityEditorProcessProviderTest
{
    private const string EditorPath = "/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/MacOS/Unity";
    private const int EditorProcessId = 1234;
    private const int NotRunningProcessId = int.MaxValue;

    private string _tempPath = "";
    private string _projectPath = "";
    private string _currentDirectory = "";

    [SetUp]
    public void SetUp()
    {
        _currentDirectory = Directory.GetCurrentDirectory();
        _tempPath = Path.Combine(Path.GetTempPath(), $"UniCortexTest_{Guid.NewGuid():N}");
        _projectPath = Path.Combine(_tempPath, "Project");
        Directory.CreateDirectory(Path.Combine(_projectPath, "Library"));
    }

    [TearDown]
    public void TearDown()
    {
        Directory.SetCurrentDirectory(_currentDirectory);
        Directory.Delete(_tempPath, recursive: true);
    }

    [Test]
    public void GetProcessId_ReturnsProcessIdInEditorInstanceFile()
    {
        // Arrange
        var processId = Environment.ProcessId;
        WriteEditorInstance($"{{\"process_id\" : {processId}, \"version\" : \"6000.3.10f1\"}}");
        UnityProcess[] processes = [CreateEditorProcess(EditorProcessId, _projectPath)];

        // Act
        var result = UnityEditorProcessProvider.GetProcessId(_projectPath, () => processes);

        // Assert
        Assert.That(result, Is.EqualTo(processId));
    }

    [Test]
    public void GetProcessId_Throws_WhenEditorInstanceFileDoesNotExist()
    {
        // Act
        var ex = Assert.Throws<InvalidOperationException>(
            () => UnityEditorProcessProvider.GetProcessId(_projectPath, () => []));

        // Assert
        Assert.That(ex!.Message, Does.Contain("EditorInstance.json"));
    }

    [Test]
    public void GetProcessId_Throws_WhenProcessIsNotRunning()
    {
        // Arrange
        WriteEditorInstance($"{{\"process_id\" : {NotRunningProcessId}}}");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(
            () => UnityEditorProcessProvider.GetProcessId(_projectPath, () => []));

        // Assert
        Assert.That(ex!.Message, Does.Contain("is not running"));
    }

    [Test]
    public void GetProcessId_Throws_WhenEditorInstanceFileIsNotJson()
    {
        // Arrange
        WriteEditorInstance("{\"process_id\" : ");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(
            () => UnityEditorProcessProvider.GetProcessId(_projectPath, () => []));

        // Assert
        Assert.That(ex!.Message, Does.Contain("Failed to read"));
    }

    [Test]
    public void GetProcessId_ReturnsEditorProcessWithProjectPath_WhenProcessIsNotRunning()
    {
        // Arrange
        WriteEditorInstance($"{{\"process_id\" : {NotRunningProcessId}}}");
        UnityProcess[] processes =
        [
            CreateEditorProcess(EditorProcessId - 1, Path.Combine(_tempPath, "OtherProject")),
            CreateEditorProcess(EditorProcessId, _projectPath)
        ];

        // Act
        var result = UnityEditorProcessProvider.GetProcessId(_projectPath, () => processes);

        // Assert
        Assert.That(result, Is.EqualTo(EditorProcessId));
    }

    [Test]
    public void GetProcessId_ReturnsEditorProcessWithProjectPath_WhenEditorInstanceFileDoesNotExist()
    {
        // Arrange
        UnityProcess[] processes = [CreateEditorProcess(EditorProcessId, _projectPath)];

        // Act
        var result = UnityEditorProcessProvider.GetProcessId(_projectPath, () => processes);

        // Assert
        Assert.That(result, Is.EqualTo(EditorProcessId));
    }

    [Test]
    public void GetProcessId_Throws_WhenSeveralEditorProcessesHaveProjectPath()
    {
        // Arrange
        WriteEditorInstance($"{{\"process_id\" : {NotRunningProcessId}}}");
        UnityProcess[] processes =
        [
            CreateEditorProcess(EditorProcessId, _projectPath),
            CreateEditorProcess(EditorProcessId + 1, _projectPath)
        ];

        // Act
        var ex = Assert.Throws<InvalidOperationException>(
            () => UnityEditorProcessProvider.GetProcessId(_projectPath, () => processes));

        // Assert
        Assert.That(ex!.Message, Does.Contain("is not running"));
    }

    [Test]
    public void FindEditorProcessIds_IgnoresAssetImportWorkers()
    {
        // Arrange
        UnityProcess[] processes =
        [
            CreateEditorProcess(EditorProcessId, _projectPath),
            new(EditorProcessId + 1,
            [
                EditorPath, "-adb2", "-batchMode", "-noUpm", "-name", "AssetImportWorker0",
                "-projectPath", _projectPath, "-logFile", "Logs/AssetImportWorker0.log"
            ])
        ];

        // Act
        var result = UnityEditorProcessProvider.FindEditorProcessIds(_projectPath, processes);

        // Assert
        Assert.That(result, Is.EqualTo(new[] { EditorProcessId }));
    }

    [Test]
    public void FindEditorProcessIds_IgnoresProcessesWithoutProjectPath()
    {
        // Arrange
        UnityProcess[] processes =
        [
            new(EditorProcessId, ["/Applications/Unity Hub.app/Contents/Resources/unity", "serve"]),
            new(EditorProcessId + 1, [EditorPath, "-projectPath"])
        ];

        // Act
        var result = UnityEditorProcessProvider.FindEditorProcessIds(_projectPath, processes);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void FindEditorProcessIds_IgnoresRelativeProjectPath()
    {
        // Arrange
        Directory.SetCurrentDirectory(_tempPath);
        UnityProcess[] processes = [CreateEditorProcess(EditorProcessId, "Project")];

        // Act
        var result = UnityEditorProcessProvider.FindEditorProcessIds(_projectPath, processes);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void FindEditorProcessIds_MatchesProjectPathWithTrailingSeparator()
    {
        // Arrange
        UnityProcess[] processes = [CreateEditorProcess(EditorProcessId, _projectPath + Path.DirectorySeparatorChar)];

        // Act
        var result = UnityEditorProcessProvider.FindEditorProcessIds(_projectPath, processes);

        // Assert
        Assert.That(result, Is.EqualTo(new[] { EditorProcessId }));
    }

    [Test]
    public void FindEditorProcessIds_MatchesProjectPathOptionIgnoringCase()
    {
        // Arrange
        UnityProcess[] processes = [new(EditorProcessId, [EditorPath, "-projectpath", _projectPath, "-useHub"])];

        // Act
        var result = UnityEditorProcessProvider.FindEditorProcessIds(_projectPath, processes);

        // Assert
        Assert.That(result, Is.EqualTo(new[] { EditorProcessId }));
    }

    [Test]
    public void FindEditorProcessIds_MatchesProjectPathThroughSymbolicLink()
    {
        // Arrange
        var linkPath = Path.Combine(_tempPath, "Link");
        CreateSymbolicLink(linkPath, _projectPath);
        UnityProcess[] processes = [CreateEditorProcess(EditorProcessId, linkPath)];

        // Act
        var result = UnityEditorProcessProvider.FindEditorProcessIds(_projectPath, processes);

        // Assert
        Assert.That(result, Is.EqualTo(new[] { EditorProcessId }));
    }

    [Test]
    public void NormalizePath_ResolvesSymbolicLinkInParentDirectory()
    {
        // Arrange
        var linkPath = Path.Combine(_tempPath, "Link");
        CreateSymbolicLink(linkPath, _projectPath);

        // Act
        var result = UnityEditorProcessProvider.NormalizePath(Path.Combine(linkPath, "Library"));

        // Assert
        Assert.That(result, Is.EqualTo(UnityEditorProcessProvider.NormalizePath(Path.Combine(_projectPath, "Library"))));
    }

    [Test]
    public void NormalizePath_KeepsPathThatDoesNotExist()
    {
        // Arrange
        var path = Path.Combine(_projectPath, "Missing", "Child");

        // Act
        var result = UnityEditorProcessProvider.NormalizePath(path + Path.DirectorySeparatorChar);

        // Assert
        Assert.That(result,
            Is.EqualTo(Path.Combine(UnityEditorProcessProvider.NormalizePath(_projectPath), "Missing", "Child")));
    }

    [Test]
    public void NormalizePath_ReturnsPath_WhenSymbolicLinksMakeLoop()
    {
        // Arrange
        var linkPathA = Path.Combine(_tempPath, "LinkA");
        var linkPathB = Path.Combine(_tempPath, "LinkB");
        CreateSymbolicLink(linkPathA, linkPathB);
        CreateSymbolicLink(linkPathB, linkPathA);

        // Act
        var result = UnityEditorProcessProvider.NormalizePath(Path.Combine(linkPathA, "Project"));

        // Assert
        Assert.That(result, Does.EndWith("Project"));
    }

    private static void CreateSymbolicLink(string linkPath, string targetPath)
    {
        try
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Windows requires the Developer Mode or the administrator privilege to create a symbolic link.
            Assert.Ignore($"Cannot create a symbolic link: {ex.Message}");
        }
    }

    private static UnityProcess CreateEditorProcess(int processId, string projectPath)
    {
        return new UnityProcess(processId, [EditorPath, "-projectPath", projectPath]);
    }

    private void WriteEditorInstance(string json)
    {
        File.WriteAllText(Path.Combine(_projectPath, "Library", "EditorInstance.json"), json);
    }
}
