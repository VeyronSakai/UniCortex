using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class PackageManagerUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPackageManagerOperations _operations;

        public PackageManagerUseCase(IMainThreadDispatcher dispatcher, IPackageManagerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public Task<PackageEntry[]> ListAsync(
            bool offlineMode,
            bool includeIndirectDependencies,
            CancellationToken cancellationToken = default)
        {
            return RunAsync(
                () => _operations.ListAsync(offlineMode, includeIndirectDependencies, cancellationToken),
                cancellationToken);
        }

        public Task<PackageEntry[]> SearchAsync(
            string packageIdOrName,
            bool offlineMode,
            CancellationToken cancellationToken = default)
        {
            return RunAsync(
                () => _operations.SearchAsync(packageIdOrName, offlineMode, cancellationToken),
                cancellationToken);
        }

        public Task<PackageEntry> AddAsync(string identifier, CancellationToken cancellationToken = default)
        {
            return RunAsync(() => _operations.AddAsync(identifier, cancellationToken), cancellationToken);
        }

        public Task RemoveAsync(string packageName, CancellationToken cancellationToken = default)
        {
            return RunAsync(() => _operations.RemoveAsync(packageName, cancellationToken), cancellationToken);
        }

        public Task ResolveAsync(CancellationToken cancellationToken = default)
        {
            return RunAsync(() => _operations.ResolveAsync(cancellationToken), cancellationToken);
        }

        private async Task<T> RunAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(operation, cancellationToken);
            return await task;
        }

        private async Task RunAsync(Func<Task> operation, CancellationToken cancellationToken)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(operation, cancellationToken);
            await task;
        }
    }
}
