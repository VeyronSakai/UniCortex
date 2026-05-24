using System;
using System.Collections.Generic;
using System.Linq;

namespace UniCortex.Editor.Infrastructures
{
    internal static class UnityTypeResolver
    {
        /// <summary>
        /// Resolves a Unity type by name, accepting either a namespace-qualified name
        /// (e.g. "UnityEngine.Rigidbody") or an assembly-qualified name
        /// (e.g. "UnityEngine.Rigidbody, UnityEngine.PhysicsModule").
        /// When a non-assembly-qualified name matches multiple loaded assemblies,
        /// throws <see cref="ArgumentException"/> instructing the caller to disambiguate.
        /// </summary>
        public static Type Resolve<TBase>(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            // Assembly-qualified name (contains a comma): resolve directly.
            if (typeName.Contains(','))
            {
                var type = Type.GetType(typeName, throwOnError: false);
                return type != null && typeof(TBase).IsAssignableFrom(type) ? type : null;
            }

            // Namespace-qualified name: search every loaded assembly and collect matches.
            // Deduplicate by assembly simple-name because Unity occasionally surfaces the same
            // logical assembly twice in AppDomain.CurrentDomain.GetAssemblies() (e.g. reload
            // artifacts), which would otherwise raise a false ambiguity error.
            var matches = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);
                if (type != null && typeof(TBase).IsAssignableFrom(type))
                {
                    matches.Add(type);
                }
            }

            var uniqueByAssembly = matches
                .GroupBy(t => t.Assembly.GetName().Name)
                .Select(g => g.First())
                .ToList();

            if (uniqueByAssembly.Count > 1)
            {
                var assemblies = string.Join(", ", uniqueByAssembly.Select(t => t.Assembly.GetName().Name));
                throw new ArgumentException(
                    $"Type '{typeName}' is ambiguous. Found in assemblies: {assemblies}. " +
                    $"Specify an assembly-qualified name (e.g. \"{typeName}, {uniqueByAssembly[0].Assembly.GetName().Name}\").");
            }

            return uniqueByAssembly.Count == 1 ? uniqueByAssembly[0] : null;
        }
    }
}
