using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class TimelineUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetInfo_ReturnsError_WhenInvalidInstanceId()
    {
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.TimelineUseCase.GetInfoAsync(-1, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("instanceId").Or.Contain("Timeline"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Play_ReturnsError_WhenInvalidInstanceId()
    {
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.TimelineUseCase.PlayAsync(-1, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("instanceId").Or.Contain("Timeline"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask AddTrack_ReturnsError_WhenInvalidInstanceId()
    {
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.TimelineUseCase.AddTrackAsync(-1, TimelineTrackType.AnimationTrack, "Test",
                CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("instanceId").Or.Contain("Timeline"));
    }
}
