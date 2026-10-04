using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class InputUseCaseTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
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
        // Click at screen center which is inside the button regardless of Game View resolution.
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);
            var centerX = size.screenWidth / 2f;
            var centerY = size.screenHeight / 2f;

            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.SendMouseEventAsync(centerX, centerY, MouseButton.Left, InputEventType.Press,
                CancellationToken.None);
            await Task.Delay(100);
            await _fixture.InputUseCase.SendMouseEventAsync(centerX, centerY, MouseButton.Left, InputEventType.Release,
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
    public async ValueTask SendMouseEvent_InPlayMode_ClicksUIButton_WithClickEventType()
    {
        // Same scenario as SendMouseEvent_InPlayMode_ClicksUIButton_WhenInsideButton,
        // but using a single "click" eventType instead of separate press/release calls.
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);
            var centerX = size.screenWidth / 2f;
            var centerY = size.screenHeight / 2f;

            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.SendMouseEventAsync(centerX, centerY, MouseButton.Left, InputEventType.Click,
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

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_InPlayMode_ClicksTopLeftButton_UsingGameViewSize()
    {
        // TopLeftButton (200x80) is anchored at top-left corner of the screen.
        // Its center in Input System coordinates (origin bottom-left, Y up) is (100, screenHeight - 40).
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);
            var x = 100f;
            var y = size.screenHeight - 40f;

            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.SendMouseEventAsync(x, y, MouseButton.Left, InputEventType.Click,
                CancellationToken.None);
            await Task.Delay(500);

            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);

            Assert.That(logs, Does.Contain("[ButtonClickDebug] Button clicked: TopLeftButton"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_InPlayMode_ClicksBottomRightButton_UsingGameViewSize()
    {
        // BottomRightButton (200x80) is anchored at bottom-right corner of the screen.
        // Its center in Input System coordinates (origin bottom-left, Y up) is (screenWidth - 100, 40).
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);
            var x = size.screenWidth - 100f;
            var y = 40f;

            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.SendMouseEventAsync(x, y, MouseButton.Left, InputEventType.Click,
                CancellationToken.None);
            await Task.Delay(500);

            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);

            Assert.That(logs, Does.Contain("[ButtonClickDebug] Button clicked: BottomRightButton"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetPointerTargets_ReturnsError_WhenNotInPlayMode()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.GetPointerTargetsAsync(CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode").Or.Contain("com.unity.ugui"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetPointerTargets_InPlayMode_ReturnsButtonsInGameViewCoordinates()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);

            // Act
            var targets = await GetPointerTargetsAsync();

            // Assert
            // TopLeftButton (200x80) is anchored at the top-left corner of the screen.
            var topLeft = targets.Single(t => t.path == "Canvas/TopLeftButton");
            Assert.That(topLeft.centerX, Is.EqualTo(100f).Within(1f));
            Assert.That(topLeft.centerY, Is.EqualTo(size.screenHeight - 40f).Within(1f));
            Assert.That(topLeft.rect.width, Is.EqualTo(200f).Within(1f));
            Assert.That(topLeft.rect.height, Is.EqualTo(80f).Within(1f));
            Assert.That(topLeft.events, Does.Contain(PointerEventName.Click));
            Assert.That(topLeft.activeInHierarchy, Is.True);
            Assert.That(topLeft.interactable, Is.True);
            Assert.That(topLeft.blocked, Is.False);

            // BottomRightButton is at the bottom-right corner, which is checked against the Game View size
            // by the EventSystem raycast.
            var bottomRight = targets.Single(t => t.path == "Canvas/BottomRightButton");
            Assert.That(bottomRight.centerX, Is.EqualTo(size.screenWidth - 100f).Within(1f));
            Assert.That(bottomRight.centerY, Is.EqualTo(40f).Within(1f));
            Assert.That(bottomRight.blocked, Is.False);
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetPointerTargets_InPlayMode_ReportsBlocked_WhenCoveredByOtherUI()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            // An Image added as the last child of the Canvas is drawn on top of TestButton at the screen center.
            await CreateOverlayAsync("Overlay");

            // Act
            var targets = await GetPointerTargetsAsync();

            // Assert
            var testButton = targets.Single(t => t.path == "Canvas/TestButton");
            Assert.That(testButton.blocked, Is.True);
            Assert.That(testButton.blockedBy, Is.EqualTo("Canvas/Overlay"));
            Assert.That(targets.Single(t => t.path == "Canvas/TopLeftButton").blocked, Is.False);
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_InPlayMode_ClicksUIButton_WithTargetPath()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            var message = await _fixture.InputUseCase.SendMouseEventAsync(null, null, null,
                "Canvas/BottomRightButton", MouseButton.Left, InputEventType.Click, CancellationToken.None);
            await Task.Delay(500);

            // Assert
            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);
            Assert.That(logs, Does.Contain("[ButtonClickDebug] Button clicked: BottomRightButton"));
            Assert.That(message, Does.Not.Contain("Warning"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_InPlayMode_ClicksUIButton_WithTargetInstanceId()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var targets = await GetPointerTargetsAsync();
            var topLeft = targets.Single(t => t.path == "Canvas/TopLeftButton");
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            await _fixture.InputUseCase.SendMouseEventAsync(null, null, topLeft.instanceId, null,
                MouseButton.Left, InputEventType.Click, CancellationToken.None);
            await Task.Delay(500);

            // Assert
            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);
            Assert.That(logs, Does.Contain("[ButtonClickDebug] Button clicked: TopLeftButton"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_InPlayMode_WarnsAndDoesNotClick_WhenTargetIsBlocked()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await CreateOverlayAsync("Overlay");
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            var message = await _fixture.InputUseCase.SendMouseEventAsync(null, null, null, "Canvas/TestButton",
                MouseButton.Left, InputEventType.Click, CancellationToken.None);
            await Task.Delay(500);

            // Assert
            Assert.That(message, Does.Contain("Warning").And.Contain("Canvas/Overlay"));
            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);
            Assert.That(logs, Does.Not.Contain("[ButtonClickDebug] Button clicked"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_ReturnsError_WhenTargetPathNotFound()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.InputUseCase.SendMouseEventAsync(null, null, null, "Canvas/Missing",
                    MouseButton.Left, InputEventType.Click, CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("not found"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_ReturnsError_WhenBothCoordinatesAndTargetAreGiven()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.SendMouseEventAsync(100f, 200f, null, "Canvas/TestButton",
                MouseButton.Left, InputEventType.Click, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Specify either x and y"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SendMouseEvent_ReturnsError_WhenNeitherCoordinatesNorTargetAreGiven()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.SendMouseEventAsync(null, null, null, null,
                MouseButton.Left, InputEventType.Click, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Specify either x and y"));
    }

    private async ValueTask<List<PointerTarget>> GetPointerTargetsAsync()
    {
        var json = await _fixture.InputUseCase.GetPointerTargetsAsync(CancellationToken.None);
        return JsonSerializer.Deserialize<GetPointerTargetsResponse>(json, s_jsonOptions)!.targets;
    }

    private async ValueTask CreateOverlayAsync(string name)
    {
        var findJson = await _fixture.GameObjectUseCase.FindAsync("t:Canvas", CancellationToken.None);
        var canvas = JsonSerializer.Deserialize<FindGameObjectsResponse>(findJson, s_jsonOptions)!.gameObjects
            .Single(g => g.name == "Canvas");

        var createJson = await _fixture.GameObjectUseCase.CreateAsync(name, canvas.instanceId,
            useRectTransform: true, cancellationToken: CancellationToken.None);
        var overlay = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;
        await _fixture.ComponentUseCase.AddAsync(overlay.instanceId, "UnityEngine.UI.Image", "UnityEngine.UI",
            CancellationToken.None);

        // Wait for the Canvas to update so the new Image is registered for raycasts.
        await Task.Delay(200);
    }
}
