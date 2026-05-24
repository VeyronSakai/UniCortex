using System;

namespace UniCortex.Editor.Infrastructures
{
    internal static class UnityTypeResolver
    {
        /// <summary>
        /// Resolves a Unity type from an assembly-qualified name
        /// (e.g. "UnityEngine.Rigidbody, UnityEngine.PhysicsModule").
        /// Returns null when the type cannot be found or is not assignable to TBase.
        /// </summary>
        public static Type Resolve<TBase>(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                return null;
            }

            var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
            return type != null && typeof(TBase).IsAssignableFrom(type) ? type : null;
        }
    }
}
