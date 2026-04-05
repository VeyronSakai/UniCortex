using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class RecordingUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask AddAndGetList_ReturnsAddedRecorder()
    {
        var name = await _fixture.RecordingUseCase.AddAsync(
            "TestRecorder", Path.Combine(Path.GetTempPath(), "UniCortex_test.mp4"),
            cancellationToken: CancellationToken.None);
        try
        {
            var response = await _fixture.RecordingUseCase.GetListAsync(CancellationToken.None);
            var entry = response.recorders.First(r => r.name == name);
            Assert.That(entry.enabled, Is.True);
        }
        finally
        {
            var list = await _fixture.RecordingUseCase.GetListAsync(CancellationToken.None);
            var target = System.Array.Find(list.recorders, r => r.name == name);
            if (target != null)
                await _fixture.RecordingUseCase.RemoveAsync(target.index, CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask Remove_RemovesRecorderFromList()
    {
        var name = await _fixture.RecordingUseCase.AddAsync(
            "RemoveTestRecorder", Path.Combine(Path.GetTempPath(), "UniCortex_remove_test.mp4"),
            cancellationToken: CancellationToken.None);

        var listBefore = await _fixture.RecordingUseCase.GetListAsync(CancellationToken.None);
        var entry = listBefore.recorders.First(r => r.name == name);

        await _fixture.RecordingUseCase.RemoveAsync(entry.index, CancellationToken.None);

        var response = await _fixture.RecordingUseCase.GetListAsync(CancellationToken.None);
        Assert.That(response.recorders.Any(r => r.name == name), Is.False);
    }

    [Test]
    public async ValueTask StartAndStop_InPlayMode_ReturnsOutputPath()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            // Add recorder after entering Play Mode (domain reload resets global settings)
            await _fixture.RecordingUseCase.AddAsync(
                "RecordingTest",
                Path.Combine(Path.GetTempPath(), $"UniCortex_Test_{DateTime.Now:yyyyMMdd_HHmmss}.mp4"),
                cancellationToken: CancellationToken.None);

            var list = await _fixture.RecordingUseCase.GetListAsync(CancellationToken.None);
            var entry = list.recorders.First(r => r.name == "RecordingTest");

            await _fixture.RecordingUseCase.StartAsync(entry.index, 30, CancellationToken.None);

            await Task.Delay(1000);

            var outputPath = await _fixture.RecordingUseCase.StopAsync(CancellationToken.None);
            Assert.That(outputPath, Is.Not.Empty);
            Assert.That(File.Exists(outputPath), Is.True,
                $"Recording file should exist at: {outputPath}");
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }
}
