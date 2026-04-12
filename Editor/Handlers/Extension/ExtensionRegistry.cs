using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Extension
{
    internal sealed class ExtensionRegistry
    {
        private readonly SortedDictionary<string, ExtensionHandler> _handlers = new(StringComparer.Ordinal);

        internal ExtensionRegistry() : this(discover: true)
        {
        }

        internal ExtensionRegistry(bool discover)
        {
            if (discover) DiscoverHandlers();
        }

        internal bool TryGetHandler(string name, out ExtensionHandler handler)
        {
            return _handlers.TryGetValue(name, out handler);
        }

        internal IReadOnlyDictionary<string, ExtensionHandler> Handlers => _handlers;

        internal void RegisterForTest(ExtensionHandler handler)
        {
            _handlers[handler.Name] = handler;
        }

        private void DiscoverHandlers()
        {
            var types = TypeCache.GetTypesDerivedFrom<ExtensionHandler>();
            foreach (var type in types)
            {
                if (type.IsAbstract) continue;

                ExtensionHandler handler;
                try
                {
                    handler = (ExtensionHandler)Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[UniCortex] Failed to instantiate extension handler {type.FullName}: {ex.Message}");
                    continue;
                }

                string handlerName;
                try
                {
                    handlerName = handler.Name;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[UniCortex] Failed to read Name from extension handler {type.FullName}: {ex.Message}");
                    continue;
                }

                if (string.IsNullOrEmpty(handlerName))
                {
                    Debug.LogWarning(
                        $"[UniCortex] Extension handler {type.FullName} has an empty Name, skipping.");
                    continue;
                }

                if (_handlers.ContainsKey(handlerName))
                {
                    Debug.LogWarning(
                        $"[UniCortex] Duplicate extension name '{handlerName}' from {type.FullName}, skipping.");
                    continue;
                }

                _handlers[handlerName] = handler;
            }

            if (_handlers.Count > 0)
            {
                Debug.Log($"[UniCortex] Registered {_handlers.Count} extension(s).");
            }
        }
    }
}
