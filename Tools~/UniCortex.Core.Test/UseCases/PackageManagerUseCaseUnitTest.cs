using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class PackageManagerUseCaseUnitTest
{
    [Test]
    public async ValueTask ListAsync_CallsListRouteWithQuery()
    {
        var client = new FakeUnityEditorClient();
        var useCase = new PackageManagerUseCase(client);

        await useCase.ListAsync(offlineMode: false, includeIndirectDependencies: true, CancellationToken.None);

        Assert.That(client.LastGetRoute, Is.EqualTo(ApiRoutes.PackageManagerList));
        var request = (PackageManagerListRequest)client.LastGetRequest!;
        Assert.That(request.offlineMode, Is.False);
        Assert.That(request.includeIndirectDependencies, Is.True);
    }

    [Test]
    public async ValueTask SearchAsync_CallsSearchRouteWithQuery()
    {
        var client = new FakeUnityEditorClient();
        var useCase = new PackageManagerUseCase(client);

        await useCase.SearchAsync("com.unity.timeline", offlineMode: true, CancellationToken.None);

        Assert.That(client.LastGetRoute, Is.EqualTo(ApiRoutes.PackageManagerSearch));
        var request = (PackageManagerSearchRequest)client.LastGetRequest!;
        Assert.That(request.packageIdOrName, Is.EqualTo("com.unity.timeline"));
        Assert.That(request.offlineMode, Is.True);
    }

    [Test]
    public async ValueTask AddAsync_CallsAddRouteWithBody()
    {
        var client = new FakeUnityEditorClient();
        var useCase = new PackageManagerUseCase(client);

        await useCase.AddAsync("com.unity.timeline@1.8.7", CancellationToken.None);

        Assert.That(client.LastPostRoute, Is.EqualTo(ApiRoutes.PackageManagerAdd));
        var request = (PackageManagerAddRequest)client.LastPostRequest!;
        Assert.That(request.identifier, Is.EqualTo("com.unity.timeline@1.8.7"));
    }

    [Test]
    public async ValueTask RemoveAsync_CallsRemoveRouteWithBody()
    {
        var client = new FakeUnityEditorClient();
        var useCase = new PackageManagerUseCase(client);

        await useCase.RemoveAsync("com.unity.timeline", CancellationToken.None);

        Assert.That(client.LastPostRoute, Is.EqualTo(ApiRoutes.PackageManagerRemove));
        var request = (PackageManagerRemoveRequest)client.LastPostRequest!;
        Assert.That(request.packageName, Is.EqualTo("com.unity.timeline"));
    }

    [Test]
    public async ValueTask ResolveAsync_CallsResolveRoute()
    {
        var client = new FakeUnityEditorClient();
        var useCase = new PackageManagerUseCase(client);

        await useCase.ResolveAsync(CancellationToken.None);

        Assert.That(client.LastPostRoute, Is.EqualTo(ApiRoutes.PackageManagerResolve));
        Assert.That(client.LastPostRequest, Is.Null);
    }

    private sealed class FakeUnityEditorClient : IUnityEditorClient
    {
        public string? LastGetRoute { get; private set; }
        public object? LastGetRequest { get; private set; }
        public string? LastPostRoute { get; private set; }
        public object? LastPostRequest { get; private set; }

        public ValueTask WaitForServerAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<TRes> PostAsync<TReq, TRes>(
            string route,
            TReq? request = null,
            CancellationToken cancellationToken = default)
            where TReq : class
        {
            LastPostRoute = route;
            LastPostRequest = request;

            object response = typeof(TRes) == typeof(PackageEntry)
                ? CreatePackage()
                : new PackageManagerResolveResponse(true);

            if (typeof(TRes) == typeof(PackageManagerRemoveResponse))
            {
                response = new PackageManagerRemoveResponse(true);
            }

            return new ValueTask<TRes>((TRes)response);
        }

        public ValueTask<TRes> GetAsync<TReq, TRes>(
            string route,
            TReq? request = null,
            CancellationToken cancellationToken = default)
            where TReq : class
        {
            LastGetRoute = route;
            LastGetRequest = request;
            object response = new PackageManagerPackagesResponse(new[] { CreatePackage() });
            return new ValueTask<TRes>((TRes)response);
        }

        private static PackageEntry CreatePackage()
        {
            return new PackageEntry(
                "com.unity.timeline",
                "Timeline",
                "1.8.7",
                "com.unity.timeline@1.8.7",
                "Registry",
                string.Empty,
                true,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<PackageDependencyEntry>(),
                Array.Empty<string>());
        }
    }
}
