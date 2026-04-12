#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UniCortex.Editor.Handlers.Extension;

internal class SampleCountGameObjectsExtension : ExtensionHandler
{
    public override string Name => "sample_count_gameobjects";
    public override string Description => "Count GameObjects in the current scene, optionally filtered by name.";
    public override bool ReadOnly => true;

    public override ExtensionSchema InputSchema => new ExtensionSchema(
        new ExtensionProperty("nameFilter", ExtensionPropertyType.String,
            "Optional name filter. Only GameObjects whose name contains this string are counted.")
    );

    public override string Execute(string argumentsJson)
    {
        var filter = "";
        if (!string.IsNullOrEmpty(argumentsJson))
        {
            var args = JsonUtility.FromJson<Args>(argumentsJson);
            if (!string.IsNullOrEmpty(args.nameFilter))
            {
                filter = args.nameFilter;
            }
        }

        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var count = 0;
        foreach (var obj in allObjects)
        {
            if (string.IsNullOrEmpty(filter) || obj.name.Contains(filter))
            {
                count++;
            }
        }

        if (string.IsNullOrEmpty(filter))
        {
            return $"Found {count} GameObject(s) in the current scene.";
        }

        return $"Found {count} GameObject(s) matching \"{filter}\" in the current scene.";
    }

    [System.Serializable]
    private class Args
    {
        public string nameFilter;
    }
}
#endif
