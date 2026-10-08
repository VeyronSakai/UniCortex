using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    public async ValueTask PressKey_ReturnsError_WhenNotInPlayMode()
    {
        // The error message varies depending on whether Input System is installed:
        // - "Play Mode" when installed but not in Play Mode
        // - "Input System package" when not installed
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.PressKeyAsync([KeyName.Space], null, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode").Or.Contain("Input System"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask TypeText_ReturnsError_WhenNotInPlayMode()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.TypeTextAsync("a", CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode").Or.Contain("Input System"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask TypeText_InPlayMode_TypesIntoFocusedInputField()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            // The InputField is created in Play Mode, so the scene is not changed.
            await _fixture.MenuItemUseCase.ExecuteAsync("GameObject/UI/Legacy/Input Field", CancellationToken.None);
            var targets = await GetUIPointerTargetsAsync();
            var inputField = targets.Single(t => t.path == "Canvas/InputField (Legacy)");
            await _fixture.InputUseCase.ClickMouseAsync(null, null, inputField.instanceId, MouseButton.Left, null,
                CancellationToken.None);

            // Act
            // No delay before reading the text: typing returns after the text has been processed.
            await _fixture.InputUseCase.TypeTextAsync("Hello, World! あ", CancellationToken.None);

            // Assert
            var json = await _fixture.ComponentUseCase.GetPropertiesAsync(inputField.instanceId,
                "UnityEngine.UI.InputField", "UnityEngine.UI", cancellationToken: CancellationToken.None);
            var properties = JsonSerializer.Deserialize<GetComponentPropertiesResponse>(json, s_jsonOptions)!
                .properties;
            Assert.That(properties.Single(p => p.path == "m_Text").value, Is.EqualTo("Hello, World! あ"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask TypeText_InPlayMode_RaisesKeyboardTextInput()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            await _fixture.InputUseCase.TypeTextAsync("Ab", CancellationToken.None);

            // Assert
            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);
            Assert.That(logs, Does.Contain("[InputSystemDebug] Text input: A"));
            Assert.That(logs, Does.Contain("[InputSystemDebug] Text input: b"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ClickMouse_ReturnsError_WhenNotInPlayMode()
    {
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.ClickMouseAsync(100f, 200f, null, MouseButton.Left, null,
                CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode").Or.Contain("Input System"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask PressKey_InPlayMode_TriggersKeyboardInput()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // No delay before reading the logs: the press returns after the release has been processed.
            await _fixture.InputUseCase.PressKeyAsync([KeyName.LeftShift, KeyName.A], 0.2f,
                CancellationToken.None);

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
    public async ValueTask ClickMouse_InPlayMode_TriggersMouseInput()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // No delay before reading the logs: the click returns after the release has been processed.
            await _fixture.InputUseCase.ClickMouseAsync(400f, 300f, null, MouseButton.Left, null,
                CancellationToken.None);

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
    public async ValueTask ClickMouse_InPlayMode_ClicksUIButton_WithHoldDuration()
    {
        // TestButton is 200x80, anchored at center of screen.
        // Long-press at screen center which is inside the button regardless of Game View resolution.
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);
            var centerX = size.screenWidth / 2f;
            var centerY = size.screenHeight / 2f;

            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.ClickMouseAsync(centerX, centerY, null, MouseButton.Left, 0.3f,
                CancellationToken.None);

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
    public async ValueTask ClickMouse_InPlayMode_ClicksUIButton()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);
            var centerX = size.screenWidth / 2f;
            var centerY = size.screenHeight / 2f;

            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.ClickMouseAsync(centerX, centerY, null, MouseButton.Left, null,
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
    public async ValueTask ClickMouse_InPlayMode_DoesNotClickUIButton_WhenOutsideButton()
    {
        // Click at top-left corner (10, 10) which is far outside the centered 200x80 button.
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            await _fixture.InputUseCase.ClickMouseAsync(10f, 10f, null, MouseButton.Left, null,
                CancellationToken.None);

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
    public async ValueTask ClickMouse_InPlayMode_ClicksTopLeftButton_UsingGameViewSize()
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

            await _fixture.InputUseCase.ClickMouseAsync(x, y, null, MouseButton.Left, null,
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
    public async ValueTask ClickMouse_InPlayMode_ClicksBottomRightButton_UsingGameViewSize()
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

            await _fixture.InputUseCase.ClickMouseAsync(x, y, null, MouseButton.Left, null,
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
    public async ValueTask GetUIPointerTargets_ReturnsError_WhenNotInPlayMode()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.GetUIPointerTargetsAsync(CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Play Mode").Or.Contain("com.unity.ugui"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetUIPointerTargets_InPlayMode_ReturnsButtonsInGameViewCoordinates()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);

            // Act
            var targets = await GetUIPointerTargetsAsync();

            // Assert
            Assert.That(targets.Select(t => t.path),
                Is.EquivalentTo(new[] { "Canvas/TestButton", "Canvas/TopLeftButton", "Canvas/BottomRightButton" }));

            // TopLeftButton (200x80) is anchored at the top-left corner of the screen.
            var topLeft = targets.Single(t => t.path == "Canvas/TopLeftButton");
            Assert.That(topLeft.rect.x, Is.EqualTo(0f).Within(1f));
            Assert.That(topLeft.rect.y, Is.EqualTo(size.screenHeight - 80f).Within(1f));
            Assert.That(topLeft.rect.width, Is.EqualTo(200f).Within(1f));
            Assert.That(topLeft.rect.height, Is.EqualTo(80f).Within(1f));

            // BottomRightButton is at the bottom-right corner. It is listed only when the EventSystem raycast
            // is checked against the Game View size.
            var bottomRight = targets.Single(t => t.path == "Canvas/BottomRightButton");
            Assert.That(bottomRight.rect.x, Is.EqualTo(size.screenWidth - 200f).Within(1f));
            Assert.That(bottomRight.rect.y, Is.EqualTo(0f).Within(1f));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetUIPointerTargets_InPlayMode_ExcludesButtonCoveredByOtherUI()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            // An Image added as the last child of the Canvas is drawn on top of TestButton at the screen center.
            await CreateOverlayAsync("Overlay");

            // Act
            var targets = await GetUIPointerTargetsAsync();

            // Assert
            Assert.That(targets.Select(t => t.path), Does.Not.Contain("Canvas/TestButton"));
            Assert.That(targets.Select(t => t.path), Does.Contain("Canvas/TopLeftButton"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetUIPointerTargets_InPlayMode_ExcludesInactiveAndNonInteractableButtons()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var before = await GetUIPointerTargetsAsync();
            var topLeft = before.Single(t => t.path == "Canvas/TopLeftButton");
            var bottomRight = before.Single(t => t.path == "Canvas/BottomRightButton");

            await _fixture.GameObjectUseCase.ModifyAsync(topLeft.instanceId, activeSelf: false,
                cancellationToken: CancellationToken.None);
            await _fixture.ComponentUseCase.SetPropertyAsync(bottomRight.instanceId, "UnityEngine.UI.Button",
                "UnityEngine.UI", "m_Interactable", "false", CancellationToken.None);

            // Act
            var targets = await GetUIPointerTargetsAsync();

            // Assert
            Assert.That(targets.Select(t => t.path), Is.EquivalentTo(new[] { "Canvas/TestButton" }));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ClickMouse_InPlayMode_ClicksUIButton_WithInstanceId()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var targets = await GetUIPointerTargetsAsync();
            var bottomRight = targets.Single(t => t.path == "Canvas/BottomRightButton");
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            await _fixture.InputUseCase.ClickMouseAsync(null, null, bottomRight.instanceId,
                MouseButton.Left, null, CancellationToken.None);
            await Task.Delay(500);

            // Assert
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
    public async ValueTask ClickMouse_InPlayMode_DoesNotClick_WhenTargetIsCovered()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var targets = await GetUIPointerTargetsAsync();
            var testButton = targets.Single(t => t.path == "Canvas/TestButton");
            await CreateOverlayAsync("Overlay");
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            await _fixture.InputUseCase.ClickMouseAsync(null, null, testButton.instanceId,
                MouseButton.Left, null, CancellationToken.None);
            await Task.Delay(500);

            // Assert
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
    public async ValueTask ClickMouse_InPlayMode_ClicksUIButton_AfterEventSystemIsRecreatedWithMouseAtSamePosition()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var targets = await GetUIPointerTargetsAsync();
            var testButton = targets.Single(t => t.path == "Canvas/TestButton");
            // Leave the mouse at the button's center, then recreate the EventSystem as a scene load would.
            await _fixture.InputUseCase.ClickMouseAsync(null, null, testButton.instanceId,
                MouseButton.Left, null, CancellationToken.None);
            await RecreateEventSystemAsync();
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            await _fixture.InputUseCase.ClickMouseAsync(null, null, testButton.instanceId,
                MouseButton.Left, null, CancellationToken.None);
            await Task.Delay(500);

            // Assert
            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);
            Assert.That(logs, Does.Contain("[ButtonClickDebug] Button clicked: TestButton"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ClickMouse_ReturnsError_WhenInstanceIdNotFound()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.InputUseCase.ClickMouseAsync(null, null, int.MaxValue,
                    MouseButton.Left, null, CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("not found"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ClickMouse_ReturnsError_WhenBothCoordinatesAndTargetAreGiven()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.ClickMouseAsync(100f, 200f, 12345,
                MouseButton.Left, null, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Specify either x and y"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ClickMouse_ReturnsError_WhenNeitherCoordinatesNorTargetAreGiven()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.ClickMouseAsync(null, null, null,
                MouseButton.Left, null, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Specify either x and y"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask DragMouse_InPlayMode_PressesAtStartAndReleasesAtEnd()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            var message = await _fixture.InputUseCase.DragMouseAsync(10f, 20f, null, 110f, 70f, null,
                MouseButton.Left, 0.2f, CancellationToken.None);

            // Assert
            // No delay before reading the logs: the drag returns after the release has been processed.
            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);
            Assert.That(message, Does.Contain("from (10, 20) to (110, 70)"));
            Assert.That(logs, Does.Contain("[InputSystemDebug] Left mouse pressed at (10"));
            Assert.That(logs, Does.Contain("[InputSystemDebug] Left mouse released at (110"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask MoveMouse_InPlayMode_MovesToTargetCenter()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var targets = await GetUIPointerTargetsAsync();
            var rect = targets.Single(t => t.path == "Canvas/TestButton").rect;
            var instanceId = targets.Single(t => t.path == "Canvas/TestButton").instanceId;

            // Act
            var message = await _fixture.InputUseCase.MoveMouseAsync(null, null, instanceId,
                CancellationToken.None);

            // Assert
            var match = Regex.Match(message, @"\((?<x>[-\d.]+), (?<y>[-\d.]+)\)");
            Assert.That(match.Success, Is.True, message);
            var x = float.Parse(match.Groups["x"].Value, CultureInfo.InvariantCulture);
            var y = float.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture);
            Assert.That(x, Is.InRange(rect.x, rect.x + rect.width));
            Assert.That(y, Is.InRange(rect.y, rect.y + rect.height));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask DragMouse_InPlayMode_DragsBetweenTargets()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var targets = await GetUIPointerTargetsAsync();
            var topLeft = targets.Single(t => t.path == "Canvas/TopLeftButton");
            var bottomRight = targets.Single(t => t.path == "Canvas/BottomRightButton");
            await _fixture.ConsoleUseCase.ClearAsync(CancellationToken.None);

            // Act
            var message = await _fixture.InputUseCase.DragMouseAsync(null, null, topLeft.instanceId,
                null, null, bottomRight.instanceId, MouseButton.Left, 0.2f, CancellationToken.None);

            // Assert
            // No delay before reading the logs: the drag returns after the release has been processed.
            var logs = await _fixture.ConsoleUseCase.GetLogsAsync(log: true, warning: false, error: false,
                cancellationToken: CancellationToken.None);
            Assert.That(message, Does.StartWith("Dragged pointer from ("));
            Assert.That(logs, Does.Contain("[InputSystemDebug] Left mouse pressed"));
            Assert.That(logs, Does.Contain("[InputSystemDebug] Left mouse released"));
            // The press and the release are on different buttons, so neither is clicked.
            Assert.That(logs, Does.Not.Contain("[ButtonClickDebug] Button clicked"));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask DragMouse_ReturnsError_WhenEndIsMissing()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.InputUseCase.DragMouseAsync(100f, 200f, null, null, null, null,
                MouseButton.Left, null, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Specify either toX and toY"));
    }

    private async ValueTask<List<UIPointerTargetEntry>> GetUIPointerTargetsAsync()
    {
        var json = await _fixture.InputUseCase.GetUIPointerTargetsAsync(CancellationToken.None);
        return JsonSerializer.Deserialize<GetUIPointerTargetsResponse>(json, s_jsonOptions)!.targets;
    }

    // Replaces the EventSystem with a copy, so that a new InputSystemUIInputModule handles the pointer.
    // The copy is enabled while the original is still enabled, as with an additively loaded scene. The UI actions
    // are then already enabled, so the copy does not get the current pointer position from their initial state
    // check.
    private async ValueTask RecreateEventSystemAsync()
    {
        var findJson = await _fixture.GameObjectUseCase.FindAsync("t:EventSystem", CancellationToken.None);
        var original = JsonSerializer.Deserialize<FindGameObjectsResponse>(findJson, s_jsonOptions)!.gameObjects
            .Single(g => g.name == "EventSystem");

        await _fixture.GameObjectUseCase.DuplicateAsync(original.instanceId, "EventSystem (New)",
            CancellationToken.None);
        await _fixture.GameObjectUseCase.DeleteAsync(original.instanceId, CancellationToken.None);

        // Wait for EventSystem.current to switch to the copy.
        await Task.Delay(200);
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
