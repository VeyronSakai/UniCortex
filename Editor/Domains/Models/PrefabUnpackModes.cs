namespace UniCortex.Editor.Domains.Models
{
    // Unpack modes of a prefab instance, matching the names of UnityEditor.PrefabUnpackMode.
    public static class PrefabUnpackModes
    {
        // Unpacks only the outermost prefab instance. Nested prefab instances inside it stay linked to their prefabs.
        public const string OutermostRoot = "OutermostRoot";

        // Unpacks the prefab instance and every nested prefab instance inside it.
        public const string Completely = "Completely";
    }
}
