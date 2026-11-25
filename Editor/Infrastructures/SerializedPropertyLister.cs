using System;
using System.Collections.Generic;
using UniCortex.Editor.Domains.Models;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal static class SerializedPropertyLister
    {
        // Lists the visible properties of the whole object, or of the subtree at propertyPath (the property itself
        // first). The children of arrays and nested serializable values are listed after them, with the full paths
        // that FindProperty accepts, down to depth levels below the top-level properties (or below propertyPath).
        // Without depth, every level is listed. Throws ArgumentException when propertyPath is not found.
        public static List<SerializedPropertyEntry> List(SerializedObject serializedObject, string propertyPath,
            int? depth)
        {
            var properties = new List<SerializedPropertyEntry>();
            var wholeObject = string.IsNullOrEmpty(propertyPath);
            var iterator = wholeObject ? serializedObject.GetIterator() : serializedObject.FindProperty(propertyPath);
            if (iterator == null)
            {
                throw new ArgumentException($"Property '{propertyPath}' not found.");
            }

            // The top-level properties have depth 0. The iteration ends at the first property that is not below
            // the start: a sibling of propertyPath, or nothing for the whole object (the root has depth -1).
            var baseDepth = wholeObject ? 0 : iterator.depth;
            var endDepth = wholeObject ? -1 : iterator.depth;
            var listedManagedReferences = new HashSet<long>();
            var enterChildren = true;
            if (!wholeObject)
            {
                properties.Add(SerializedPropertyEntryFactory.Create(iterator));
                enterChildren = ShouldListChildren(iterator, baseDepth, depth, listedManagedReferences);
            }

            while (iterator.NextVisible(enterChildren) && iterator.depth > endDepth)
            {
                properties.Add(SerializedPropertyEntryFactory.Create(iterator));
                enterChildren = ShouldListChildren(iterator, baseDepth, depth, listedManagedReferences);
            }

            return properties;
        }

        // Only arrays and nested serializable values (Generic) and [SerializeReference] values have children worth
        // listing. Strings, vectors, colors and the like already show their whole value.
        private static bool ShouldListChildren(SerializedProperty property, int baseDepth, int? depth,
            HashSet<long> listedManagedReferences)
        {
            if (depth != null && property.depth - baseDepth >= depth.Value)
            {
                return false;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return true;
                case SerializedPropertyType.ManagedReference:
                    // A [SerializeReference] value can be referenced from several places, and from itself (a cycle),
                    // which the iterator would follow forever. List its children only the first time.
                    return listedManagedReferences.Add(property.managedReferenceId);
                default:
                    return false;
            }
        }
    }
}
