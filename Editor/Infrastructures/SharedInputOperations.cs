using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    // The IInputOperations shared by the MCP handlers and the public API for tests (PointerInput),
    // so that both track the same pressed state of the simulated devices.
    internal static class SharedInputOperations
    {
        private static IInputOperations s_instance;

        public static IInputOperations Instance => s_instance ??= Create();

        private static IInputOperations Create()
        {
#if UNICORTEX_INPUT_SYSTEM
            var adapter = new InputOperationsAdapter();
            EditorApplication.playModeStateChanged += adapter.OnPlayModeStateChanged;
            return adapter;
#else
            return new InputNotSupportedAdapter();
#endif
        }
    }
}
