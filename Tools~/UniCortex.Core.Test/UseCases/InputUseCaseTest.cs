using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

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
    public async ValueTask SendKeyEvent_ReturnsError_WhenNotInPlayMode()
    {
        // The error message varies depending on whether Input System is installed:
        // - "Play Mode" when installed but not in Play Mode
        // - "Input System package" when not installed
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.SendKeyEventAsync(KeyName.Space, InputEventType.Press, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode").Or.Contain("Input System"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_ReturnsError_WhenNotInPlayMode()
    {
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.SendMouseEventAsync(100f, 200f, MouseButton.Left, InputEventType.Press,
                CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode").Or.Contain("Input System"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendKeyEvent_InPlayMode_TriggersKeyboardInput()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.SendKeyEventAsync(KeyName.A, InputEventType.Press, CancellationToken.None);
            await Task.Delay(500);
            await _fixture.InputUseCase.SendKeyEventAsync(KeyName.A, InputEventType.Release, CancellationToken.None);
            await Task.Delay(500);

            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);

            Assert.That(logs, Does.Contain("[InputSystemDebug] A key pressed"));
            Assert.That(logs, Does.Contain("[InputSystemDebug] A key released"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_InPlayMode_TriggersMouseInput()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.SendMouseEventAsync(400f, 300f, MouseButton.Left, InputEventType.Press,
                CancellationToken.None);
            await Task.Delay(500);
            await _fixture.InputUseCase.SendMouseEventAsync(400f, 300f, MouseButton.Left, InputEventType.Release,
                CancellationToken.None);
            await Task.Delay(500);

            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);

            Assert.That(logs, Does.Contain("[InputSystemDebug] Left mouse pressed"));
            Assert.That(logs, Does.Contain("[InputSystemDebug] Left mouse released"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_InPlayMode_ClicksUIButton_WhenInsideButton()
    {
        // TestButton is 200x80, anchored at center of screen.
        // Game View is 800x600, so button occupies x:300-500, y:260-340 (screen coords).
        // Click at screen center (400, 300) which is inside the button.
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.SendMouseEventAsync(400f, 300f, MouseButton.Left, InputEventType.Press,
                CancellationToken.None);
            await Task.Delay(100);
            await _fixture.InputUseCase.SendMouseEventAsync(400f, 300f, MouseButton.Left, InputEventType.Release,
                CancellationToken.None);
            await Task.Delay(500);

            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);

            Assert.That(logs, Does.Contain("[ButtonClickDebug] Button clicked"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_InPlayMode_DoesNotClickUIButton_WhenOutsideButton()
    {
        // Click at top-left corner (10, 10) which is far outside the centered 200x80 button.
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.SendMouseEventAsync(10f, 10f, MouseButton.Left, InputEventType.Press,
                CancellationToken.None);
            await Task.Delay(100);
            await _fixture.InputUseCase.SendMouseEventAsync(10f, 10f, MouseButton.Left, InputEventType.Release,
                CancellationToken.None);
            await Task.Delay(500);

            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);

            Assert.That(logs, Does.Not.Contain("[ButtonClickDebug] Button clicked"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }
}
