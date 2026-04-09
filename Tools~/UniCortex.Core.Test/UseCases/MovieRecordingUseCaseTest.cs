using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class MovieRecordingUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [TearDown]
    public async ValueTask TearDown()
    {
        await RemoveAllRecordersAsync();
    }

    [Test]
    public async ValueTask AddAndGetList_ReturnsAddedRecorder()
    {
        var name = await _fixture.MovieRecordingUseCase.AddAsync(
            "TestRecorder", Path.Combine(Path.GetTempPath(), "UniCortex_test.mp4"),
            cancellationToken: CancellationToken.None);

        var response = await _fixture.MovieRecordingUseCase.GetListAsync(CancellationToken.None);
        var entry = response.recorders.First(r => r.name == name);
        Assert.That(entry.enabled, Is.True);

    }

    [Test]
    public async ValueTask Remove_RemovesRecorderFromList()
    {
        var name = await _fixture.MovieRecordingUseCase.AddAsync(
            "RemoveTestRecorder", Path.Combine(Path.GetTempPath(), "UniCortex_remove_test.mp4"),
            cancellationToken: CancellationToken.None);

        var listBefore = await _fixture.MovieRecordingUseCase.GetListAsync(CancellationToken.None);
        var entry = listBefore.recorders.First(r => r.name == name);

        await _fixture.MovieRecordingUseCase.RemoveAsync(entry.index, CancellationToken.None);

        var response = await _fixture.MovieRecordingUseCase.GetListAsync(CancellationToken.None);
        Assert.That(response.recorders.Any(r => r.name == name), Is.False);
    }

    [Test]
    public async ValueTask StartAndStop_InPlayMode_ReturnsOutputPath()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);

        // Pick a fixed-resolution Game View size with even dimensions to avoid MP4 odd-resolution errors
        var sizeList = await _fixture.GameViewUseCase.GetSizeListResponseAsync(CancellationToken.None);
        var matchingSizes = sizeList.sizes
            .Where(s => s.sizeType == "FixedResolution" && s.width % 2 == 0 && s.height % 2 == 0)
            .ToList();
        Assert.That(
            matchingSizes,
            Is.Not.Empty,
            "Expected at least one FixedResolution Game View size with even width and height.");
        var evenSize = matchingSizes[0];
        await _fixture.GameViewUseCase.SetSizeAsync(evenSize.index, CancellationToken.None);

        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        var outputPath = string.Empty;
        try
        {
            // Add recorder after entering Play Mode (domain reload resets global settings)
            await _fixture.MovieRecordingUseCase.AddAsync(
                "RecordingTest",
                Path.Combine(Path.GetTempPath(), $"UniCortex_Test_{DateTime.Now:yyyyMMdd_HHmmss}.mp4"),
                cancellationToken: CancellationToken.None);

            var list = await _fixture.MovieRecordingUseCase.GetListAsync(CancellationToken.None);
            var entry = list.recorders.First(r => r.name == "RecordingTest");

            await _fixture.MovieRecordingUseCase.StartAsync(entry.index, 30, CancellationToken.None);

            await Task.Delay(1000);

            outputPath = await _fixture.MovieRecordingUseCase.StopAsync(CancellationToken.None);
            Assert.That(outputPath, Is.Not.Empty);
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
            DeleteRecordingFiles();
        }
    }

    private async Task RemoveAllRecordersAsync()
    {
        var list = await _fixture.MovieRecordingUseCase.GetListAsync(CancellationToken.None);
        // Remove only Movie recorders in descending index order to keep indices stable.
        var movieRecorders = list.recorders.Where(r => r.type == "Movie").OrderByDescending(r => r.index);
        foreach (var entry in movieRecorders)
        {
            await _fixture.MovieRecordingUseCase.RemoveAsync(entry.index, CancellationToken.None);
        }
    }

    private static void DeleteRecordingFiles()
    {
        var projectPath = Environment.GetEnvironmentVariable("UNICORTEX_PROJECT_PATH");
        if (string.IsNullOrEmpty(projectPath))
            return;

        foreach (var file in Directory.GetFiles(projectPath, "UniCortex_Test_*.mp4"))
        {
            File.Delete(file);
        }
    }
}
