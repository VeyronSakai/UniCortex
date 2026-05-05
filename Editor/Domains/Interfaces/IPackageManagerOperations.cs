using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IPackageManagerOperations
    {
        Task<PackageEntry[]> ListAsync(
            bool offlineMode,
            bool includeIndirectDependencies,
            CancellationToken cancellationToken = default);

        Task<PackageEntry[]> SearchAsync(
            string packageIdOrName,
            bool offlineMode,
            CancellationToken cancellationToken = default);

        Task<PackageEntry> AddAsync(string identifier, CancellationToken cancellationToken = default);

        Task RemoveAsync(string packageName, CancellationToken cancellationToken = default);

        Task ResolveAsync(CancellationToken cancellationToken = default);
    }
}
