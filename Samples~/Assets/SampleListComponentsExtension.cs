#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UniCortex.Editor.Handlers.Extension;

internal class SampleListComponentsExtension : ExtensionHandler
{
    public override string Name => "sample_list_components";
    public override string Description =>
        "List all components attached to a GameObject specified by name.";
    public override bool ReadOnly => true;

    public override ExtensionSchema InputSchema => new ExtensionSchema(
        new ExtensionProperty("gameObjectName", ExtensionPropertyType.String,
            "The name of the GameObject to inspect.", required: true)
    );

    public override string Execute(string argumentsJson)
    {
        var args = JsonUtility.FromJson<Args>(argumentsJson);
        if (string.IsNullOrEmpty(args.gameObjectName))
        {
            return "Error: gameObjectName is required.";
        }

        var go = GameObject.Find(args.gameObjectName);
        if (go == null)
        {
            return $"GameObject \"{args.gameObjectName}\" not found.";
        }

        var components = go.GetComponents<Component>();
        var sb = new StringBuilder();
        sb.AppendLine($"Components on \"{go.name}\" ({components.Length}):");
        foreach (var component in components)
        {
            sb.AppendLine($"  - {component.GetType().FullName}");
        }

        return sb.ToString().TrimEnd();
    }

    [System.Serializable]
    private class Args
    {
        public string gameObjectName;
    }
}
#endif
