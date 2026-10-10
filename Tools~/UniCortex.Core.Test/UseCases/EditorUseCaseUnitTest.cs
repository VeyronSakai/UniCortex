using System.Net;
using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class EditorUseCaseUnitTest
{
    [Test]
    public async ValueTask ReloadDomainAsync_WaitsForStatusAfterDomainReloadRequest()
    {
        // Arrange
        var client = new FakeUnityEditorClient();
        var useCase = new EditorUseCase(client);

        // Act
        var message = await useCase.ReloadDomainAsync(CancellationToken.None);

        // Assert
        Assert.That(message, Does.Contain("completed"));
        Assert.That(client.Calls, Is.EqualTo(new[] { ApiRoutes.DomainReload, ApiRoutes.Status }));
    }

    [Test]
    public void ReloadDomainAsync_Throws_WhenDomainReloadRequestFails()
    {
        // Arrange
        var client = new FakeUnityEditorClient
        {
            DomainReloadException = new HttpRequestException("Script compilation failed", null,
                HttpStatusCode.BadRequest)
        };
        var useCase = new EditorUseCase(client);

        // Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await useCase.ReloadDomainAsync(CancellationToken.None));

        // Assert
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(client.Calls, Is.EqualTo(new[] { ApiRoutes.DomainReload }));
    }

    [Test]
    public async ValueTask GetActivePlatformAsync_ReturnsActiveBuildTarget()
    {
        // Arrange
        var client = new FakeUnityEditorClient { ActiveBuildTarget = "Android" };
        var useCase = new EditorUseCase(client);

        // Act
        var message = await useCase.GetActivePlatformAsync(CancellationToken.None);

        // Assert
        Assert.That(message, Is.EqualTo("Active platform: Android"));
        Assert.That(client.Calls, Is.EqualTo(new[] { ApiRoutes.Platform }));
    }

    [Test]
    public async ValueTask SwitchPlatformAsync_WaitsForStatusAfterSwitch()
    {
        // Arrange
        var client = new FakeUnityEditorClient { ActiveBuildTarget = "StandaloneOSX" };
        var useCase = new EditorUseCase(client);

        // Act
        var message = await useCase.SwitchPlatformAsync("Android", CancellationToken.None);

        // Assert
        Assert.That(message, Does.Contain("from StandaloneOSX to Android"));
        Assert.That(client.LastSwitchPlatformRequest?.buildTarget, Is.EqualTo("Android"));
        Assert.That(client.Calls, Is.EqualTo(new[] { ApiRoutes.PlatformSwitch, ApiRoutes.Status }));
    }

    [Test]
    public async ValueTask SwitchPlatformAsync_DoesNotWaitForStatus_WhenPlatformIsAlreadyActive()
    {
        // Arrange
        var client = new FakeUnityEditorClient { ActiveBuildTarget = "Android" };
        var useCase = new EditorUseCase(client);

        // Act
        var message = await useCase.SwitchPlatformAsync("Android", CancellationToken.None);

        // Assert
        Assert.That(message, Does.Contain("already Android"));
        Assert.That(client.Calls, Is.EqualTo(new[] { ApiRoutes.PlatformSwitch }));
    }

    private sealed class FakeUnityEditorClient : IUnityEditorClient
    {
        public Exception? DomainReloadException { get; init; }
        public string ActiveBuildTarget { get; set; } = "StandaloneOSX";
        public SwitchPlatformRequest? LastSwitchPlatformRequest { get; private set; }
        public List<string> Calls { get; } = [];

        public ValueTask WaitForServerAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<TRes> PostAsync<TReq, TRes>(string route, TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            Calls.Add(route);
            if (route == ApiRoutes.PlatformSwitch)
            {
                LastSwitchPlatformRequest = request as SwitchPlatformRequest;
                var previousBuildTarget = ActiveBuildTarget;
                ActiveBuildTarget = LastSwitchPlatformRequest!.buildTarget;
                object switchResponse = new SwitchPlatformResponse(previousBuildTarget, ActiveBuildTarget);
                return new ValueTask<TRes>((TRes)switchResponse);
            }

            if (route != ApiRoutes.DomainReload)
            {
                throw new InvalidOperationException("Unexpected PostAsync call.");
            }

            if (DomainReloadException != null)
            {
                throw DomainReloadException;
            }

            object response = new DomainReloadResponse(true);
            return new ValueTask<TRes>((TRes)response);
        }

        public ValueTask<TRes> GetAsync<TReq, TRes>(string route, TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            Calls.Add(route);
            if (route == ApiRoutes.Platform)
            {
                object platformResponse = new GetActivePlatformResponse(ActiveBuildTarget);
                return new ValueTask<TRes>((TRes)platformResponse);
            }

            if (route != ApiRoutes.Status)
            {
                throw new InvalidOperationException("Unexpected GetAsync call.");
            }

            object response = new GetEditorStatusResponse(false, false);
            return new ValueTask<TRes>((TRes)response);
        }
    }
}
