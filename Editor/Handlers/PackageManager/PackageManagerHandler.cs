using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.PackageManager
{
    internal sealed class PackageManagerHandler
    {
        private readonly PackageManagerUseCase _useCase;

        public PackageManagerHandler(PackageManagerUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.PackageManagerList, HandleListAsync);
            router.Register(HttpMethodType.Get, ApiRoutes.PackageManagerSearch, HandleSearchAsync);
            router.Register(HttpMethodType.Post, ApiRoutes.PackageManagerAdd, HandleAddAsync);
            router.Register(HttpMethodType.Post, ApiRoutes.PackageManagerRemove, HandleRemoveAsync);
            router.Register(HttpMethodType.Post, ApiRoutes.PackageManagerResolve, HandleResolveAsync);
        }

        private async Task HandleListAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var offlineMode = ParseBool(context.GetQueryParameter("offlineMode"), true, "offlineMode");
                var includeIndirectDependencies = ParseBool(
                    context.GetQueryParameter("includeIndirectDependencies"),
                    false,
                    "includeIndirectDependencies");

                var packages = await _useCase.ListAsync(
                    offlineMode,
                    includeIndirectDependencies,
                    cancellationToken);
                var json = JsonUtility.ToJson(new PackageManagerPackagesResponse(packages));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (PackageManagerOperationException ex)
            {
                await WritePackageManagerErrorAsync(context, ex);
            }
        }

        private async Task HandleSearchAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var packageIdOrName = context.GetQueryParameter("packageIdOrName");
            if (string.IsNullOrEmpty(packageIdOrName))
            {
                await WriteBadRequestAsync(context, "packageIdOrName is required.");
                return;
            }

            try
            {
                var offlineMode = ParseBool(context.GetQueryParameter("offlineMode"), false, "offlineMode");
                var packages = await _useCase.SearchAsync(packageIdOrName, offlineMode, cancellationToken);
                var json = JsonUtility.ToJson(new PackageManagerPackagesResponse(packages));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (PackageManagerOperationException ex)
            {
                await WritePackageManagerErrorAsync(context, ex);
            }
        }

        private async Task HandleAddAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            if (string.IsNullOrEmpty(body))
            {
                await WriteBadRequestAsync(context, "identifier is required.");
                return;
            }

            var request = JsonUtility.FromJson<PackageManagerAddRequest>(body);
            if (string.IsNullOrEmpty(request.identifier))
            {
                await WriteBadRequestAsync(context, "identifier is required.");
                return;
            }

            try
            {
                var package = await _useCase.AddAsync(request.identifier, cancellationToken);
                var json = JsonUtility.ToJson(package);
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (PackageManagerOperationException ex)
            {
                await WritePackageManagerErrorAsync(context, ex);
            }
        }

        private async Task HandleRemoveAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            if (string.IsNullOrEmpty(body))
            {
                await WriteBadRequestAsync(context, "packageName is required.");
                return;
            }

            var request = JsonUtility.FromJson<PackageManagerRemoveRequest>(body);
            if (string.IsNullOrEmpty(request.packageName))
            {
                await WriteBadRequestAsync(context, "packageName is required.");
                return;
            }

            try
            {
                await _useCase.RemoveAsync(request.packageName, cancellationToken);
                var json = JsonUtility.ToJson(new PackageManagerRemoveResponse(true));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (PackageManagerOperationException ex)
            {
                await WritePackageManagerErrorAsync(context, ex);
            }
        }

        private async Task HandleResolveAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                await _useCase.ResolveAsync(cancellationToken);
                var json = JsonUtility.ToJson(new PackageManagerResolveResponse(true));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (PackageManagerOperationException ex)
            {
                await WritePackageManagerErrorAsync(context, ex);
            }
        }

        private static bool ParseBool(string value, bool defaultValue, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1")
            {
                return true;
            }

            if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) || value == "0")
            {
                return false;
            }

            throw new ArgumentException($"{parameterName} must be true or false.");
        }

        private static Task WritePackageManagerErrorAsync(
            IRequestContext context,
            PackageManagerOperationException exception)
        {
            return WriteBadRequestAsync(context, exception.Message);
        }

        private static Task WriteBadRequestAsync(IRequestContext context, string message)
        {
            var errorJson = JsonUtility.ToJson(new ErrorResponse(message));
            return context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
        }
    }
}
