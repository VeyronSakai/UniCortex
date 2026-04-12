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
            foreach (var handler in _registry.Handlers.Values.OrderBy(h => h.Name))
            {
                items.Add(new ExtensionInfo
                {
                    name = handler.Name,
                    description = handler.Description,
                    readOnly = handler.ReadOnly,
                    inputSchema = ExtensionSchemaSerializer.ToJsonSchema(handler.InputSchema)
                });
            }

            var response = new ExtensionListResponse { extensions = items };
            var json = JsonUtility.ToJson(response);
            return context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
