using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UnityEngine;

namespace UniCortex.Editor.Handlers.CustomTools
{
    internal sealed class GetCustomToolsManifestHandler
    {
        private readonly CustomExtensionRegistry _registry;

        public GetCustomToolsManifestHandler(CustomExtensionRegistry registry)
        {
            _registry = registry;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.CustomToolsManifest, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var json = JsonUtility.ToJson(_registry.GetManifest());
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
