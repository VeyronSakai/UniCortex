using System.Diagnostics;
using NUnit.Framework;
using UniCortex.Core.Infrastructures;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class ProcessArgumentsReaderTest
{
    private string _tempPath = "";
    private Process? _process;

    [SetUp]
    public void SetUp()
    {
        if (!OperatingSystem.IsMacOS() && !OperatingSystem.IsWindows())
        {
            Assert.Ignore("Reading the arguments of another process is supported only on macOS and Windows.");
        }

        _tempPath = Path.Combine(Path.GetTempPath(), $"UniCortexTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempPath);
    }

    [TearDown]
    public void TearDown()
    {
        if (_process != null)
        {
            if (!_process.HasExited)
            {
                _process.Kill();
                _process.WaitForExit();
            }

            _process.Dispose();
            _process = null;
        }

        if (_tempPath.Length > 0)
        {
            Directory.Delete(_tempPath, recursive: true);
        }
    }

    [Test]
    public void Read_ReturnsArgumentsOfProcess()
    {
        // Arrange
        var startInfo = CreateLongRunningProcessStartInfo();
        _process = Process.Start(startInfo)!;

        // Act
        var result = ProcessArgumentsReader.Read(_process.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Skip(1), Is.EqualTo(startInfo.ArgumentList));
    }

    [Test]
    public void Read_ReturnsNull_WhenProcessDoesNotExist()
    {
        // Act
        var result = ProcessArgumentsReader.Read(int.MaxValue);

        // Assert
        Assert.That(result, Is.Null);
    }

    private ProcessStartInfo CreateLongRunningProcessStartInfo()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessStartInfo("ping.exe")
            {
                ArgumentList = { "-n", "60", "127.0.0.1" },
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
        }

        // A path with a space and non-ASCII characters checks how the arguments are split and decoded.
        var filePath = Path.Combine(_tempPath, "Project Path プロジェクト.txt");
        File.WriteAllText(filePath, "");
        return new ProcessStartInfo("/usr/bin/tail")
        {
            ArgumentList = { "-f", filePath },
            RedirectStandardOutput = true
        };
    }
}
