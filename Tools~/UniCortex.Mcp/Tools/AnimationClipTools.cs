using System.ComponentModel;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class AnimationClipTools(AnimationClipUseCase animationClipUseCase, IAsyncOperationSequencer sequencer)
{
    private const string AssetPathDescription =
        "The asset path of the AnimationClip (e.g. \"Assets/Animations/FadeIn.anim\").";

    private const string AnimatorRelativePathDescription =
        "Path of the animated object relative to the Animator root (e.g. \"Root/Child\"). " +
        "Use an empty string for the Animator's own GameObject.";

    private const string ComponentTypeDescription =
        "Fully qualified type name of the animated component " +
        "(e.g. \"UnityEngine.Transform\", \"UnityEngine.RectTransform\", \"UnityEngine.UI.Image\", \"UnityEngine.GameObject\").";

    private const string AssemblyNameDescription =
        "Name of the assembly that defines the component type " +
        "(e.g. \"UnityEngine.CoreModule\" for Transform/RectTransform/GameObject, \"UnityEngine.UI\" for Image).";

    private const string PropertyNameDescription =
        "Serialized property name of the float value to animate " +
        "(e.g. \"m_Color.a\", \"m_AnchoredPosition.y\", \"m_LocalScale.x\", \"m_IsActive\").";

    [McpServerTool(Name = "create_animation_clip", ReadOnly = false),
     Description("Create a new empty AnimationClip (.anim file) at the specified asset path."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreateAnimationClipAsync(
        [Description("Asset path where the AnimationClip will be saved. Must end with \".anim\" " +
                     "(e.g. \"Assets/Animations/FadeIn.anim\").")]
        string assetPath,
        [Description("Whether the clip loops (Loop Time). Default: false.")]
        bool loop = false,
        [Description("Sample rate of the clip in frames per second. Default: 60.")]
        float frameRate = 60f,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => animationClipUseCase.CreateAsync(assetPath, loop, frameRate, ct), cancellationToken);

    [McpServerTool(Name = "get_animation_curves", ReadOnly = true),
     Description(
         "Get the settings (frameRate, loop, length) and all float curves with their keys of an AnimationClip. " +
         "Infinite tangents (stepped keys) are reported as ±3.4028235E+38 (float max)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetAnimationCurvesAsync(
        [Description(AssetPathDescription)] string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => animationClipUseCase.GetCurvesAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "set_animation_curve", ReadOnly = false),
     Description(
         "Set one float curve of an AnimationClip, replacing all of its existing keys. " +
         "Creates the curve if it does not exist. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetAnimationCurveAsync(
        [Description(AssetPathDescription)] string assetPath,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        [Description(PropertyNameDescription)] string propertyName,
        [Description("Keys of the curve. Must contain at least one key; times must be unique.")]
        AnimationKeyParameter[] keys,
        [Description(AnimatorRelativePathDescription)] string animatorRelativePath = "",
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => animationClipUseCase.SetCurveAsync(assetPath, animatorRelativePath, componentType, assemblyName,
                propertyName, keys.Select(k => k.ToInput()).ToList(), ct),
            cancellationToken);

    [McpServerTool(Name = "remove_animation_curve", ReadOnly = false),
     Description("Remove one float curve from an AnimationClip. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> RemoveAnimationCurveAsync(
        [Description(AssetPathDescription)] string assetPath,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        [Description(PropertyNameDescription)] string propertyName,
        [Description(AnimatorRelativePathDescription)] string animatorRelativePath = "",
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => animationClipUseCase.RemoveCurveAsync(assetPath, animatorRelativePath, componentType,
                assemblyName, propertyName, ct),
            cancellationToken);
}

[UsedImplicitly]
public sealed class AnimationKeyParameter
{
    [JsonPropertyName("time"), Description("Key time in seconds.")]
    public float Time { get; init; }

    [JsonPropertyName("value"), Description("Key value.")]
    public float Value { get; init; }

    [JsonPropertyName("inTangent"),
     Description("Incoming tangent (slope). Used only when tangentMode is Free. " +
                 "±3.4028235E+38 (float max) means an infinite slope (value held until the next key).")]
    public float InTangent { get; init; }

    [JsonPropertyName("outTangent"),
     Description("Outgoing tangent (slope). Used only when tangentMode is Free. " +
                 "±3.4028235E+38 (float max) means an infinite slope (value held until the next key).")]
    public float OutTangent { get; init; }

    [JsonPropertyName("tangentMode"),
     Description("Tangent mode applied to both sides of the key: Free (default, uses inTangent/outTangent), " +
                 "Auto, ClampedAuto, Linear, or Constant. " +
                 "Free with tangents omitted (0) gives flat tangents, i.e. a smooth ease in / ease out.")]
    public string? TangentMode { get; init; }

    internal AnimationCurveKeyInput ToInput() => new()
    {
        time = Time,
        value = Value,
        inTangent = InTangent,
        outTangent = OutTangent,
        tangentMode = TangentMode ?? string.Empty
    };
}
