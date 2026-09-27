using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class SceneViewUseCaseTest
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
        var result = await _fixture.SceneViewUseCase.FocusAsync(CancellationToken.None);

        Assert.That(result, Does.Contain("successfully"));
    }

    [Test]
    public async ValueTask Capture_InEditMode_ReturnsPngData()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);

        // Act
        var pngData = await _fixture.SceneViewUseCase.CaptureAsync(CancellationToken.None);

        // Assert
        Assert.That(pngData.Take(s_pngSignature.Length), Is.EqualTo(s_pngSignature));
    }
}
