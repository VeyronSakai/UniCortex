using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class ProfilerUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask FocusWindow_Succeeds()
    {
        var result = await _fixture.ProfilerUseCase.FocusWindowAsync(CancellationToken.None);

        Assert.That(result, Does.Contain("successfully"));
    }

    [Test]
    public async ValueTask GetStatus_ReturnsJson()
    {
        var result = await _fixture.ProfilerUseCase.GetStatusAsync(CancellationToken.None);

        Assert.That(result, Does.Contain("isWindowOpen"));
        Assert.That(result, Does.Contain("isRecording"));
    }

    [Test]
    public async ValueTask StartAndStopRecording_TogglesStatus()
    {
        await _fixture.ProfilerUseCase.FocusWindowAsync(CancellationToken.None);

        await _fixture.ProfilerUseCase.StartRecordingAsync(profileEditor: true, cancellationToken: CancellationToken.None);
        var started = await _fixture.ProfilerUseCase.GetStatusResponseAsync(CancellationToken.None);
        Assert.That(started.isRecording, Is.True);
        Assert.That(started.profileEditor, Is.True);

        await _fixture.ProfilerUseCase.StopRecordingAsync(CancellationToken.None);
        var stopped = await _fixture.ProfilerUseCase.GetStatusResponseAsync(CancellationToken.None);
        Assert.That(stopped.isRecording, Is.False);
    }
}
