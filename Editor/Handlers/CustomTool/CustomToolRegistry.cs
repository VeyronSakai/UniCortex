using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Handlers.CustomTool
{
    internal sealed class CustomToolRegistry
    {
        private readonly Dictionary<string, CustomToolHandler> _handlers = new();

        internal CustomToolRegistry()
        {
            DiscoverHandlers();
        }

        internal bool TryGetHandler(string toolName, out CustomToolHandler handler)
        {
            return _handlers.TryGetValue(toolName, out handler);
        }

        internal IReadOnlyDictionary<string, CustomToolHandler> Handlers => _handlers;

        internal void RegisterForTest(CustomToolHandler handler)
        {
            _handlers[handler.ToolName] = handler;
        }

        private void DiscoverHandlers()
        {
            var types = TypeCache.GetTypesDerivedFrom<CustomToolHandler>();
            foreach (var type in types)
            {
                if (type.IsAbstract) continue;

                CustomToolHandler handler;
                try
                {
                    handler = (CustomToolHandler)Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[UniCortex] Failed to instantiate custom tool handler {type.FullName}: {ex.Message}");
                    continue;
                }

                if (string.IsNullOrEmpty(handler.ToolName))
                {
                    Debug.LogWarning(
                        $"[UniCortex] Custom tool handler {type.FullName} has an empty ToolName, skipping.");
                    continue;
                }

                if (_handlers.ContainsKey(handler.ToolName))
                {
                    Debug.LogWarning(
                        $"[UniCortex] Duplicate custom tool name '{handler.ToolName}' from {type.FullName}, skipping.");
                    continue;
                }

                _handlers[handler.ToolName] = handler;
            }

            if (_handlers.Count > 0)
            {
                Debug.Log($"[UniCortex] Registered {_handlers.Count} custom tool(s).");
            }
        }
    }
}
