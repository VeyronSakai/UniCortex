using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal static class GameObjectNodeBuilder
    {
        public static GameObjectNode BuildNode(Transform transform)
        {
            var go = transform.gameObject;
            var components = go.GetComponents<Component>()
                .Where(c => c != null)
                .Select(c => c.GetType().Name)
                .ToList();

            var children = new List<GameObjectNode>();
            for (var i = 0; i < transform.childCount; i++)
            {
                children.Add(BuildNode(transform.GetChild(i)));
            }

            var isLocked = (go.hideFlags & HideFlags.NotEditable) != 0;
            return new GameObjectNode(go.name, go.GetInstanceID(), go.activeSelf, go.tag, go.layer, go.isStatic,
                isLocked, components, children);
        }
    }
}
