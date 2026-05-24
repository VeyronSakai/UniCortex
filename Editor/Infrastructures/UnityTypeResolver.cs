using System;

namespace UniCortex.Editor.Infrastructures
{
    internal static class UnityTypeResolver
    {
        /// <summary>
        /// Resolves a Unity type given a fully-qualified type name and the name of the
        /// assembly that defines it (e.g. "UnityEngine.Rigidbody" + "UnityEngine.PhysicsModule").
        /// Returns null when the type cannot be found or is not assignable to TBase.
        /// </summary>
        public static Type Resolve<TBase>(string typeName, string assemblyName)
        {
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(assemblyName))
            {
                return null;
            }

            var type = Type.GetType($"{typeName}, {assemblyName}", throwOnError: false);
            return type != null && typeof(TBase).IsAssignableFrom(type) ? type : null;
        }
    }
}
