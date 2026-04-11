using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Handlers.CustomTool
{
    internal sealed class CustomToolListHandler
    {
        private readonly CustomToolRegistry _registry;

        public CustomToolListHandler(CustomToolRegistry registry)
        {
            _registry = registry;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.CustomToolList, HandleListAsync);
        }

        private Task HandleListAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var tools = new List<CustomToolInfo>();
            foreach (var pair in _registry.Handlers)
            {
                var handler = pair.Value;
                tools.Add(new CustomToolInfo
                {
                    name = handler.ToolName,
                    description = handler.Description,
                    readOnly = handler.ReadOnly,
                    inputSchema = CustomToolSchemaSerializer.ToJsonSchema(handler.InputSchema)
                });
            }

            var response = new CustomToolListResponse { tools = tools };
            var json = JsonUtility.ToJson(response);
            return context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
