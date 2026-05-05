using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyPackageManagerOperations : IPackageManagerOperations
    {
        public PackageEntry[] ListResult { get; set; } = Array.Empty<PackageEntry>();
        public PackageEntry[] SearchResult { get; set; } = Array.Empty<PackageEntry>();
        public PackageEntry AddResult { get; set; } = new PackageEntry(
            "com.test.package",
            "Test Package",
            "1.0.0",
            "com.test.package@1.0.0",
            "Registry",
            string.Empty,
            true,
            string.Empty,
            string.Empty,
            string.Empty,
            Array.Empty<PackageDependencyEntry>(),
            Array.Empty<string>());
        public Exception ExceptionToThrow { get; set; }

        public int ListCallCount { get; private set; }
        public bool LastListOfflineMode { get; private set; }
        public bool LastListIncludeIndirectDependencies { get; private set; }

        public int SearchCallCount { get; private set; }
        public string LastSearchPackageIdOrName { get; private set; }
        public bool LastSearchOfflineMode { get; private set; }

        public int AddCallCount { get; private set; }
        public string LastAddIdentifier { get; private set; }

        public int RemoveCallCount { get; private set; }
        public string LastRemovePackageName { get; private set; }

        public int ResolveCallCount { get; private set; }

        public Task<PackageEntry[]> ListAsync(
            bool offlineMode,
            bool includeIndirectDependencies,
            CancellationToken cancellationToken = default)
        {
            ListCallCount++;
            LastListOfflineMode = offlineMode;
            LastListIncludeIndirectDependencies = includeIndirectDependencies;
            if (ExceptionToThrow != null)
            {
                return Task.FromException<PackageEntry[]>(ExceptionToThrow);
            }

            return Task.FromResult(ListResult);
        }

        public Task<PackageEntry[]> SearchAsync(
            string packageIdOrName,
            bool offlineMode,
            CancellationToken cancellationToken = default)
        {
            SearchCallCount++;
            LastSearchPackageIdOrName = packageIdOrName;
            LastSearchOfflineMode = offlineMode;
            if (ExceptionToThrow != null)
            {
                return Task.FromException<PackageEntry[]>(ExceptionToThrow);
            }

            return Task.FromResult(SearchResult);
        }

        public Task<PackageEntry> AddAsync(string identifier, CancellationToken cancellationToken = default)
        {
            AddCallCount++;
            LastAddIdentifier = identifier;
            if (ExceptionToThrow != null)
            {
                return Task.FromException<PackageEntry>(ExceptionToThrow);
            }

            return Task.FromResult(AddResult);
        }

        public Task RemoveAsync(string packageName, CancellationToken cancellationToken = default)
        {
            RemoveCallCount++;
            LastRemovePackageName = packageName;
            if (ExceptionToThrow != null)
            {
                return Task.FromException(ExceptionToThrow);
            }

            return Task.CompletedTask;
        }

        public Task ResolveAsync(CancellationToken cancellationToken = default)
        {
            ResolveCallCount++;
            if (ExceptionToThrow != null)
            {
                return Task.FromException(ExceptionToThrow);
            }

            return Task.CompletedTask;
        }
    }
}
