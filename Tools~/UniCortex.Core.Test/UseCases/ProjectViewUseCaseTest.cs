using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class ProjectViewUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Select_ReturnsSuccess_WhenAssetExists()
    {
        var message = await _fixture.ProjectViewUseCase.SelectAsync(TestConstants.SampleScenePath, CancellationToken.None);

        Assert.That(message, Does.Contain(TestConstants.SampleScenePath));
    }

    [Test]
    public void Select_ThrowsException_WhenAssetDoesNotExist()
    {
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.ProjectViewUseCase.SelectAsync("Assets/DoesNotExist.asset", CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Asset not found"));
    }
}
