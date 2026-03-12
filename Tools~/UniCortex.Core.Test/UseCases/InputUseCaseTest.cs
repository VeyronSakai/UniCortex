using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class InputUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendKeyEvent_RequiresPlayMode()
    {
        // This test verifies that the endpoint rejects requests when not in Play Mode.
        // When Play Mode is not active, the server returns 500 with an error message.
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.SendKeyEventAsync("space", "keyDown", CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_RequiresPlayMode()
    {
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.SendMouseEventAsync(100f, 200f, 0, "mouseDown", CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode"));
    }
}
