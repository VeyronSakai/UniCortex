using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class SceneViewUseCaseTest
{
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
    public async ValueTask SetCamera_Succeeds()
    {
        var result = await _fixture.SceneViewUseCase.SetCameraAsync(
            0f, 1f, -10f,
            0f, 0f, 0f, 1f,
            8f, false,
            CancellationToken.None);

        Assert.That(result, Does.Contain("successfully"));
    }

    [Test]
    public async ValueTask GetCamera_ReturnsCurrentCameraState()
    {
        await _fixture.SceneViewUseCase.SetCameraAsync(
            2f, 3f, -12f,
            0f, 0f, 0f, 1f,
            9f, true,
            CancellationToken.None);

        var response = await _fixture.SceneViewUseCase.GetCameraResponseAsync(CancellationToken.None);

        Assert.That(response.position.x, Is.EqualTo(2f).Within(0.001f));
        Assert.That(response.position.y, Is.EqualTo(3f).Within(0.001f));
        Assert.That(response.position.z, Is.EqualTo(-12f).Within(0.001f));
        Assert.That(response.rotation.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(response.rotation.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(response.rotation.z, Is.EqualTo(0f).Within(0.001f));
        Assert.That(response.rotation.w, Is.EqualTo(1f).Within(0.001f));
        Assert.That(response.size, Is.GreaterThan(0f));
        Assert.That(response.orthographic, Is.True);
    }
}
