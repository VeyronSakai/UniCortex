using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class PackageManagerUseCase(IUnityEditorClient client)
{
    public async ValueTask<PackageManagerPackagesResponse> ListAsync(
        bool offlineMode = true,
        bool includeIndirectDependencies = false,
        CancellationToken cancellationToken = default)
    {
        var request = new PackageManagerListRequest
        {
            offlineMode = offlineMode,
            includeIndirectDependencies = includeIndirectDependencies
        };
        return await client.GetAsync<PackageManagerListRequest, PackageManagerPackagesResponse>(
            ApiRoutes.PackageManagerList, request, cancellationToken);
    }

    public async ValueTask<PackageManagerPackagesResponse> SearchAsync(
        string packageIdOrName,
        bool offlineMode = false,
        CancellationToken cancellationToken = default)
    {
        var request = new PackageManagerSearchRequest
        {
            packageIdOrName = packageIdOrName,
            offlineMode = offlineMode
        };
        return await client.GetAsync<PackageManagerSearchRequest, PackageManagerPackagesResponse>(
            ApiRoutes.PackageManagerSearch, request, cancellationToken);
    }

    public async ValueTask<PackageEntry> AddAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        var request = new PackageManagerAddRequest { identifier = identifier };
        return await client.PostAsync<PackageManagerAddRequest, PackageEntry>(
            ApiRoutes.PackageManagerAdd, request, cancellationToken);
    }

    public async ValueTask RemoveAsync(string packageName, CancellationToken cancellationToken = default)
    {
        var request = new PackageManagerRemoveRequest { packageName = packageName };
        await client.PostAsync<PackageManagerRemoveRequest, PackageManagerRemoveResponse>(
            ApiRoutes.PackageManagerRemove, request, cancellationToken);
    }

    public async ValueTask ResolveAsync(CancellationToken cancellationToken = default)
    {
        await client.PostAsync<PackageManagerResolveRequest, PackageManagerResolveResponse>(
            ApiRoutes.PackageManagerResolve, cancellationToken: cancellationToken);
    }
}
