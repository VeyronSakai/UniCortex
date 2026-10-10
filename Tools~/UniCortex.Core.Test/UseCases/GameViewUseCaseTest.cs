using System.Buffers.Binary;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class GameViewUseCaseTest
{
    private static readonly byte[] s_pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Focus_Succeeds()
    {
        var result = await _fixture.GameViewUseCase.FocusAsync(CancellationToken.None);

        Assert.That(result, Does.Contain("successfully"));
    }

    [Test]
    public async ValueTask Capture_InPlayMode_ReturnsPngData()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);

            // Act
            var pngData = await _fixture.GameViewUseCase.CaptureAsync(false, false, CancellationToken.None);

            // Assert
            Assert.That(pngData.Take(s_pngSignature.Length), Is.EqualTo(s_pngSignature));
            // The IHDR chunk stores the width and height as big-endian integers at offsets 16 and 20.
            // get_game_view_size truncates fractional sizes (e.g. Free Aspect), so allow a difference of 1 pixel.
            Assert.That(BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(16, 4)),
                Is.EqualTo(size.screenWidth).Within(1));
            Assert.That(BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(20, 4)),
                Is.EqualTo(size.screenHeight).Within(1));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test]
    public void Capture_InEditMode_Throws()
    {
        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.GameViewUseCase.CaptureAsync(false, false, CancellationToken.None));
    }

    [Test]
    public async ValueTask GetSize_ReturnsScreenSize()
    {
        var result = await _fixture.GameViewUseCase.GetSizeAsync(CancellationToken.None);

        Assert.That(result, Does.Contain("Game View size:"));
        Assert.That(result, Does.Match(@"\d+x\d+"));
    }

    [Test]
    public async ValueTask GetSizeList_ReturnsSizes()
    {
        var result = await _fixture.GameViewUseCase.GetSizeListAsync(CancellationToken.None);

        Assert.That(result, Does.Contain("Game View sizes"));
        Assert.That(result, Does.Contain("[0]"));
    }

    [Test]
    public async ValueTask SetSize_Succeeds()
    {
        // Get the list first to find a valid index
        var listResponse = await _fixture.GameViewUseCase.GetSizeListResponseAsync(CancellationToken.None);
        Assert.That(listResponse.sizes.Length, Is.GreaterThan(0));

        var result = await _fixture.GameViewUseCase.SetSizeAsync(0, CancellationToken.None);

        Assert.That(result, Does.Contain("successfully"));
    }

    [Test]
    public async ValueTask GetScale_ReturnsScaleWithRange()
    {
        var response = await _fixture.GameViewUseCase.GetScaleResponseAsync(CancellationToken.None);

        Assert.That(response.minScale, Is.GreaterThan(0f));
        Assert.That(response.maxScale, Is.GreaterThanOrEqualTo(response.minScale));

        var message = await _fixture.GameViewUseCase.GetScaleAsync(CancellationToken.None);
        Assert.That(message, Does.Contain("Game View scale:"));
    }

    [Test]
    public async ValueTask SetScale_Succeeds_And_ClampsToRange()
    {
        var original = await _fixture.GameViewUseCase.GetScaleResponseAsync(CancellationToken.None);

        try
        {
            var result = await _fixture.GameViewUseCase.SetScaleAsync(2.0f, CancellationToken.None);
            Assert.That(result, Does.Contain("successfully"));

            // A value far above the valid range must be clamped to maxScale.
            await _fixture.GameViewUseCase.SetScaleAsync(1000f, CancellationToken.None);
            var clamped = await _fixture.GameViewUseCase.GetScaleResponseAsync(CancellationToken.None);
            Assert.That(clamped.scale, Is.LessThanOrEqualTo(clamped.maxScale + 0.001f));
        }
        finally
        {
            // Restore the original scale.
            await _fixture.GameViewUseCase.SetScaleAsync(original.scale, CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask GetSafeArea_InGameView_ReturnsWholeScreen()
    {
        // Arrange
        await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);

        // Act
        var response = await _fixture.GameViewUseCase.GetSafeAreaResponseAsync(CancellationToken.None);

        // Assert
        Assert.That(response.viewType, Is.EqualTo(PlayModeViewTypes.GameView));
        Assert.That(response.safeArea.width, Is.EqualTo((float)response.screenWidth));
        Assert.That(response.safeArea.height, Is.EqualTo((float)response.screenHeight));
        Assert.That(response.cutouts, Is.Empty);
    }

    [Test]
    public async ValueTask GetSimulatorDeviceList_InGameView_Throws()
    {
        // Arrange
        await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.GameViewUseCase.GetSimulatorDeviceListAsync(CancellationToken.None));
    }

    [Test]
    public async ValueTask SetViewType_SwitchesBetweenGameViewAndSimulatorView()
    {
        try
        {
            // Act
            var result = await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.SimulatorView,
                CancellationToken.None);

            // Assert
            Assert.That(result, Does.Contain("successfully"));
            var viewType = await _fixture.GameViewUseCase.GetViewTypeAsync(CancellationToken.None);
            Assert.That(viewType, Does.Contain(PlayModeViewTypes.SimulatorView));
        }
        finally
        {
            await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);
        }

        var restored = await _fixture.GameViewUseCase.GetViewTypeAsync(CancellationToken.None);
        Assert.That(restored, Does.Contain(PlayModeViewTypes.GameView));
    }

    [Test]
    public void SetViewType_UnknownViewType_Throws()
    {
        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.GameViewUseCase.SetViewTypeAsync("Unknown", CancellationToken.None));
    }

    [Test]
    public async ValueTask SetSimulatorDevice_ChangesDeviceRotationAndSafeArea()
    {
        // Arrange
        await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.SimulatorView, CancellationToken.None);
        var original = await _fixture.GameViewUseCase.GetSimulatorDeviceListResponseAsync(CancellationToken.None);
        try
        {
            var device = original.devices.First(d => d.screenHeight > d.screenWidth);

            // Act
            var result = await _fixture.GameViewUseCase.SetSimulatorDeviceAsync(device.index, 0,
                CancellationToken.None);
            var portrait = await _fixture.GameViewUseCase.GetSafeAreaResponseAsync(CancellationToken.None);
            await _fixture.GameViewUseCase.SetSimulatorDeviceAsync(null, 90, CancellationToken.None);
            var landscape = await _fixture.GameViewUseCase.GetSafeAreaResponseAsync(CancellationToken.None);
            var list = await _fixture.GameViewUseCase.GetSimulatorDeviceListResponseAsync(CancellationToken.None);

            // Assert
            Assert.That(result, Does.Contain(device.name));
            Assert.That(list.selectedIndex, Is.EqualTo(device.index));
            Assert.That(list.rotation, Is.EqualTo(90));
            Assert.That(portrait.deviceName, Is.EqualTo(device.name));
            Assert.That(portrait.orientation, Is.EqualTo("Portrait"));
            Assert.That(portrait.screenHeight, Is.GreaterThan(portrait.screenWidth));
            Assert.That(landscape.orientation, Does.StartWith("Landscape"));
            Assert.That(landscape.screenWidth, Is.GreaterThan(landscape.screenHeight));
        }
        finally
        {
            await _fixture.GameViewUseCase.SetSimulatorDeviceAsync(original.selectedIndex, original.rotation,
                CancellationToken.None);
            await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask SetSimulatorDevice_InvalidRotation_Throws()
    {
        // Arrange
        await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.SimulatorView, CancellationToken.None);
        try
        {
            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.GameViewUseCase.SetSimulatorDeviceAsync(null, 45, CancellationToken.None));
        }
        finally
        {
            await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask Capture_InSimulatorView_WithSafeArea_ReturnsPngAtDeviceResolution()
    {
        // Arrange
        await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.SimulatorView, CancellationToken.None);
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var safeArea = await _fixture.GameViewUseCase.GetSafeAreaResponseAsync(CancellationToken.None);

            // Act
            var pngData = await _fixture.GameViewUseCase.CaptureAsync(true, false, CancellationToken.None);

            // Assert
            Assert.That(pngData.Take(s_pngSignature.Length), Is.EqualTo(s_pngSignature));
            Assert.That(BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(16, 4)),
                Is.EqualTo(safeArea.screenWidth));
            Assert.That(BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(20, 4)),
                Is.EqualTo(safeArea.screenHeight));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
            await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask Capture_InSimulatorView_WithDeviceFrame_ReturnsPngIncludingFrame()
    {
        // Arrange
        await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.SimulatorView, CancellationToken.None);
        var original = await _fixture.GameViewUseCase.GetSimulatorDeviceListResponseAsync(CancellationToken.None);
        var device = original.devices.First(d => d.screenHeight > d.screenWidth);
        await _fixture.GameViewUseCase.SetSimulatorDeviceAsync(device.index, 90, CancellationToken.None);
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            // Act
            var pngData = await _fixture.GameViewUseCase.CaptureAsync(true, true, CancellationToken.None);

            // Assert
            // The device is rotated by 90 degrees, so the framed image is landscape and larger than the
            // native screen resolution of the device by the frame thickness.
            Assert.That(pngData.Take(s_pngSignature.Length), Is.EqualTo(s_pngSignature));
            var width = BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(16, 4));
            var height = BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(20, 4));
            Assert.That(width, Is.GreaterThan(device.screenHeight));
            Assert.That(height, Is.GreaterThan(device.screenWidth));
            Assert.That(width, Is.GreaterThan(height));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
            await _fixture.GameViewUseCase.SetSimulatorDeviceAsync(original.selectedIndex, original.rotation,
                CancellationToken.None);
            await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask Capture_InGameView_WithoutDeviceFrame_ReturnsPngAtGameViewSize()
    {
        // Arrange
        await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var size = await _fixture.GameViewUseCase.GetSizeResponseAsync(CancellationToken.None);

            // Act
            var pngData = await _fixture.GameViewUseCase.CaptureAsync(false, null, CancellationToken.None);

            // Assert
            // get_game_view_size truncates fractional sizes (e.g. Free Aspect), so allow a difference of 1 pixel.
            Assert.That(BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(16, 4)),
                Is.EqualTo(size.screenWidth).Within(1));
            Assert.That(BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(20, 4)),
                Is.EqualTo(size.screenHeight).Within(1));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask Capture_InGameView_WithDeviceFrame_Throws()
    {
        // Arrange
        await _fixture.GameViewUseCase.SetViewTypeAsync(PlayModeViewTypes.GameView, CancellationToken.None);
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.GameViewUseCase.CaptureAsync(false, true, CancellationToken.None));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }
}
