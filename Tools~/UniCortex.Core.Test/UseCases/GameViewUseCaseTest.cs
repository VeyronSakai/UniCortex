using System.Buffers.Binary;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

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
            var pngData = await _fixture.GameViewUseCase.CaptureAsync(CancellationToken.None);

            // Assert
            Assert.That(pngData.Take(s_pngSignature.Length), Is.EqualTo(s_pngSignature));
            // The IHDR chunk stores the width and height as big-endian integers at offsets 16 and 20.
            Assert.That(BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(16, 4)), Is.EqualTo(size.screenWidth));
            Assert.That(BinaryPrimitives.ReadInt32BigEndian(pngData.AsSpan(20, 4)), Is.EqualTo(size.screenHeight));
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
            await _fixture.GameViewUseCase.CaptureAsync(CancellationToken.None));
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
}
