using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests
{
    internal static class DebugMenu
    {
        [MenuItem("UniCortex/Debug/Log Info")]
        private static void LogInfo()
        {
            Debug.Log("[UniCortex] Sample info message");
        }

        [MenuItem("UniCortex/Debug/Log Warning")]
        private static void LogWarning()
        {
            Debug.LogWarning("[UniCortex] Sample warning message");
        }

        [MenuItem("UniCortex/Debug/Log Error")]
        private static void LogError()
        {
            Debug.LogError("[UniCortex] Sample error message");
        }
    }
}
