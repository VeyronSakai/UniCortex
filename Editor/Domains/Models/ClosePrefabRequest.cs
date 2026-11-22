using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ClosePrefabRequest
    {
        // Drops the unsaved changes of the Prefab instead of failing while it has any.
        public bool discardChanges;
    }
}
