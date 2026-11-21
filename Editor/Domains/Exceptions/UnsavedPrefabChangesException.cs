using System;

namespace UniCortex.Editor.Domains.Exceptions
{
    // Thrown when an operation would leave Prefab Mode while the Prefab has unsaved changes. With Auto Save off,
    // Unity would ask whether to save them in a dialog that blocks the Editor.
    internal sealed class UnsavedPrefabChangesException : InvalidOperationException
    {
        public UnsavedPrefabChangesException(string operation, string prefabAssetPath)
            : base($"{operation} while the Prefab open in Prefab Mode ({prefabAssetPath}) has unsaved changes. " +
                   "Save them with save (CLI: editor save), or discard them with close_prefab with " +
                   "discardChanges: true (CLI: prefab close --discard-changes), which leaves Prefab Mode.")
        {
        }
    }
}
