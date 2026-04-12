using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Extension
{
    internal sealed class ExtensionListHandler
    {
        private readonly ExtensionRegistry _registry;

        public ExtensionListHandler(ExtensionRegistry registry)
        {
            _registry = registry;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.ExtensionList, HandleListAsync);
        }

        private Task HandleListAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var items = new List<ExtensionInfo>();
            foreach (var handler in _registry.Handlers.Values)
            {
                try
                {
                    items.Add(new ExtensionInfo
                    {
                        name = handler.Name,
                        description = handler.Description,
                        readOnly = handler.ReadOnly,
                        inputSchema = ExtensionSchemaSerializer.ToJsonSchema(handler.InputSchema)
                    });
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(
                        $"[UniCortex] Skipping extension during list because metadata extraction failed: {ex}");
                }
            }

            items = items.OrderBy(item => item.name).ToList();

            var response = new ExtensionListResponse { extensions = items };
            var json = JsonUtility.ToJson(response);
            return context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
