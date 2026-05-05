using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageManagerClient = UnityEditor.PackageManager.Client;
using PackageManagerError = UnityEditor.PackageManager.Error;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class PackageManagerOperationsAdapter : IPackageManagerOperations
    {
        private static readonly SemaphoreSlim s_packageManagerSemaphore = new(1, 1);

        public async Task<PackageEntry[]> ListAsync(
            bool offlineMode,
            bool includeIndirectDependencies,
            CancellationToken cancellationToken = default)
        {
            await s_packageManagerSemaphore.WaitAsync(cancellationToken);
            try
            {
                var request = PackageManagerClient.List(offlineMode, includeIndirectDependencies);
                await WaitForCompletionAsync(request, cancellationToken);
                return request.Result.Select(PackageInfoMapper.ToEntry).ToArray();
            }
            finally
            {
                s_packageManagerSemaphore.Release();
            }
        }

        public async Task<PackageEntry[]> SearchAsync(
            string packageIdOrName,
            bool offlineMode,
            CancellationToken cancellationToken = default)
        {
            await s_packageManagerSemaphore.WaitAsync(cancellationToken);
            try
            {
                var request = PackageManagerClient.Search(packageIdOrName, offlineMode);
                await WaitForCompletionAsync(request, cancellationToken);
                return request.Result.Select(PackageInfoMapper.ToEntry).ToArray();
            }
            finally
            {
                s_packageManagerSemaphore.Release();
            }
        }

        public async Task<PackageEntry> AddAsync(string identifier, CancellationToken cancellationToken = default)
        {
            await s_packageManagerSemaphore.WaitAsync(cancellationToken);
            try
            {
                var request = PackageManagerClient.Add(identifier);
                await WaitForCompletionAsync(request, cancellationToken);
                return PackageInfoMapper.ToEntry(request.Result);
            }
            finally
            {
                s_packageManagerSemaphore.Release();
            }
        }

        public async Task RemoveAsync(string packageName, CancellationToken cancellationToken = default)
        {
            await s_packageManagerSemaphore.WaitAsync(cancellationToken);
            try
            {
                var listRequest = PackageManagerClient.List(true, false);
                await WaitForCompletionAsync(listRequest, cancellationToken);

                var package = listRequest.Result.FirstOrDefault(p => p.name == packageName);
                if (package == null || !package.isDirectDependency)
                {
                    return;
                }

                var request = PackageManagerClient.Remove(packageName);
                await WaitForCompletionAsync(request, cancellationToken);
            }
            finally
            {
                s_packageManagerSemaphore.Release();
            }
        }

        public async Task ResolveAsync(CancellationToken cancellationToken = default)
        {
            await s_packageManagerSemaphore.WaitAsync(cancellationToken);
            try
            {
                PackageManagerClient.Resolve();
            }
            catch (Exception ex)
            {
                throw new PackageManagerOperationException(ex.Message);
            }
            finally
            {
                s_packageManagerSemaphore.Release();
            }
        }

        private static Task WaitForCompletionAsync(Request request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request.IsCompleted)
            {
                return request.Status == StatusCode.Failure
                    ? Task.FromException(CreateRequestException(request))
                    : Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            CancellationTokenRegistration cancellationRegistration = default;

            void Cleanup()
            {
                EditorApplication.update -= Tick;
            }

            void Tick()
            {
                if (!request.IsCompleted)
                {
                    return;
                }

                Cleanup();
                cancellationRegistration.Dispose();

                if (request.Status == StatusCode.Failure)
                {
                    tcs.TrySetException(CreateRequestException(request));
                    return;
                }

                tcs.TrySetResult(true);
            }

            cancellationRegistration = cancellationToken.Register(() =>
            {
                Cleanup();
                tcs.TrySetCanceled(cancellationToken);
            });

            EditorApplication.update += Tick;
            Tick();

            return tcs.Task;
        }

        private static PackageManagerOperationException CreateRequestException(Request request)
        {
            var error = request.Error;
            var message = error != null && !string.IsNullOrEmpty(error.message)
                ? error.message
                : "Package Manager request failed.";
            return new PackageManagerOperationException(message);
        }

        private static class PackageInfoMapper
        {
            public static PackageEntry ToEntry(PackageInfo packageInfo)
            {
                return new PackageEntry(
                    packageInfo.name ?? string.Empty,
                    packageInfo.displayName ?? string.Empty,
                    packageInfo.version ?? string.Empty,
                    packageInfo.packageId ?? string.Empty,
                    packageInfo.source.ToString(),
                    GetStatus(packageInfo),
                    packageInfo.isDirectDependency,
                    packageInfo.resolvedPath ?? string.Empty,
                    packageInfo.assetPath ?? string.Empty,
                    packageInfo.description ?? string.Empty,
                    ToDependencies(packageInfo.dependencies),
                    ToErrors(packageInfo.errors));
            }

            private static string GetStatus(PackageInfo packageInfo)
            {
                if (packageInfo.errors != null && packageInfo.errors.Length > 0)
                {
                    return "Error";
                }

                return packageInfo.isDeprecated ? "Deprecated" : string.Empty;
            }

            private static PackageDependencyEntry[] ToDependencies(DependencyInfo[] dependencies)
            {
                if (dependencies == null)
                {
                    return Array.Empty<PackageDependencyEntry>();
                }

                return dependencies
                    .Select(dependency => new PackageDependencyEntry(
                        dependency.name ?? string.Empty,
                        dependency.version ?? string.Empty))
                    .ToArray();
            }

            private static string[] ToErrors(PackageManagerError[] errors)
            {
                if (errors == null)
                {
                    return Array.Empty<string>();
                }

                return errors
                    .Select(error => error.message ?? string.Empty)
                    .Where(message => !string.IsNullOrEmpty(message))
                    .ToArray();
            }
        }
    }
}
