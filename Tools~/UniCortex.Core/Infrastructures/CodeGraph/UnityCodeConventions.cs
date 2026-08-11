namespace UniCortex.Core.Infrastructures.CodeGraph;

/// <summary>Unity-specific naming knowledge used to classify indexed symbols.</summary>
internal static class UnityCodeConventions
{
    internal const string MonoBehaviourRole = "MonoBehaviour";
    internal const string ScriptableObjectRole = "ScriptableObject";
    internal const string EditorWindowRole = "EditorWindow";
    internal const string EditorRole = "Editor";

    /// <summary>Fully qualified Unity base classes mapped to their role, checked closest-first along the base chain.</summary>
    internal static readonly IReadOnlyDictionary<string, string> RolesByBaseClass =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["UnityEngine.MonoBehaviour"] = MonoBehaviourRole,
            ["UnityEditor.EditorWindow"] = EditorWindowRole,
            ["UnityEditor.Editor"] = EditorRole,
            ["UnityEngine.ScriptableObject"] = ScriptableObjectRole
        };

    /// <summary>Simple-name fallback used when the Unity assemblies could not be resolved semantically.</summary>
    internal static readonly IReadOnlyDictionary<string, string> RolesByBaseClassSimpleName =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["MonoBehaviour"] = MonoBehaviourRole,
            ["EditorWindow"] = EditorWindowRole,
            ["Editor"] = EditorRole,
            ["ScriptableObject"] = ScriptableObjectRole
        };

    private static readonly HashSet<string> s_monoBehaviourMessages = new(StringComparer.Ordinal)
    {
        "Awake", "Start", "Update", "FixedUpdate", "LateUpdate", "OnEnable", "OnDisable", "OnDestroy",
        "OnGUI", "OnValidate", "Reset", "OnApplicationFocus", "OnApplicationPause", "OnApplicationQuit",
        "OnBecameInvisible", "OnBecameVisible", "OnCollisionEnter", "OnCollisionExit", "OnCollisionStay",
        "OnCollisionEnter2D", "OnCollisionExit2D", "OnCollisionStay2D", "OnControllerColliderHit",
        "OnDrawGizmos", "OnDrawGizmosSelected", "OnJointBreak", "OnJointBreak2D",
        "OnMouseDown", "OnMouseDrag", "OnMouseEnter", "OnMouseExit", "OnMouseOver", "OnMouseUp",
        "OnMouseUpAsButton", "OnParticleCollision", "OnParticleSystemStopped", "OnParticleTrigger",
        "OnPostRender", "OnPreCull", "OnPreRender", "OnRenderImage", "OnRenderObject",
        "OnTransformChildrenChanged", "OnTransformParentChanged",
        "OnTriggerEnter", "OnTriggerExit", "OnTriggerStay",
        "OnTriggerEnter2D", "OnTriggerExit2D", "OnTriggerStay2D", "OnWillRenderObject",
        "OnAnimatorIK", "OnAnimatorMove", "OnAudioFilterRead",
        "OnRectTransformDimensionsChange", "OnBeforeTransformParentChanged", "OnCanvasGroupChanged"
    };

    private static readonly HashSet<string> s_scriptableObjectMessages = new(StringComparer.Ordinal)
    {
        "Awake", "OnEnable", "OnDisable", "OnDestroy", "OnValidate", "Reset"
    };

    private static readonly HashSet<string> s_editorWindowMessages = new(StringComparer.Ordinal)
    {
        "Awake", "OnEnable", "OnDisable", "OnDestroy", "OnGUI", "CreateGUI", "OnFocus", "OnLostFocus",
        "OnHierarchyChange", "OnInspectorUpdate", "OnProjectChange", "OnSelectionChange", "Update",
        "ModifierKeysChanged", "ShowButton", "OnValidate", "Reset"
    };

    private static readonly HashSet<string> s_editorMessages = new(StringComparer.Ordinal)
    {
        "Awake", "OnEnable", "OnDisable", "OnDestroy", "OnSceneGUI", "OnValidate", "Reset"
    };

    internal static bool IsUnityMessage(string role, string methodName)
    {
        var messages = role switch
        {
            MonoBehaviourRole => s_monoBehaviourMessages,
            ScriptableObjectRole => s_scriptableObjectMessages,
            EditorWindowRole => s_editorWindowMessages,
            EditorRole => s_editorMessages,
            _ => null
        };

        return messages is not null && messages.Contains(methodName);
    }
}
