using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class AnimationClipCommands(AnimationClipUseCase animationClipUseCase)
{
    /// <summary>Create a new empty AnimationClip (.anim file).</summary>
    /// <param name="assetPath">Asset path ending with ".anim" (e.g. "Assets/Animations/FadeIn.anim").</param>
    /// <param name="loop">Enable Loop Time.</param>
    /// <param name="frameRate">Sample rate in frames per second.</param>
    [Command("create")]
    public async Task Create([Argument] string assetPath, bool loop = false, float frameRate = 60f,
        CancellationToken cancellationToken = default)
    {
        var json = await animationClipUseCase.CreateAsync(assetPath, loop, frameRate, cancellationToken);
        Console.WriteLine(json);
    }
}

public class AnimationCurveCommands(AnimationClipUseCase animationClipUseCase)
{
    /// <summary>List settings and all float curves (with keys) of an AnimationClip as JSON.</summary>
    /// <param name="assetPath">Asset path of the AnimationClip.</param>
    [Command("list")]
    public async Task List([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var json = await animationClipUseCase.GetCurvesAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set one float curve of an AnimationClip, replacing all of its keys.</summary>
    /// <param name="assetPath">Asset path of the AnimationClip.</param>
    /// <param name="componentType">Fully-qualified component type name (e.g. "UnityEngine.UI.Image").</param>
    /// <param name="assemblyName">Assembly that defines the type (e.g. "UnityEngine.UI").</param>
    /// <param name="propertyName">Property name to animate (e.g. "m_Color.a").</param>
    /// <param name="keys">JSON array of keys, e.g. '[{"time":0,"value":0},{"time":1,"value":1,"tangentMode":"Linear"}]'.</param>
    /// <param name="path">Path relative to the Animator root (e.g. "Root/Child"). Empty for the root.</param>
    [Command("set")]
    public async Task Set([Argument] string assetPath, [Argument] string componentType,
        [Argument] string assemblyName, [Argument] string propertyName, [Argument] string keys,
        string path = "", CancellationToken cancellationToken = default)
    {
        var message = await animationClipUseCase.SetCurveAsync(assetPath, path, componentType, assemblyName,
            propertyName, AnimationClipUseCase.ParseKeys(keys), cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove one float curve from an AnimationClip.</summary>
    /// <param name="assetPath">Asset path of the AnimationClip.</param>
    /// <param name="componentType">Fully-qualified component type name.</param>
    /// <param name="assemblyName">Assembly that defines the type.</param>
    /// <param name="propertyName">Animated property name.</param>
    /// <param name="path">Path relative to the Animator root. Empty for the root.</param>
    [Command("remove")]
    public async Task Remove([Argument] string assetPath, [Argument] string componentType,
        [Argument] string assemblyName, [Argument] string propertyName, string path = "",
        CancellationToken cancellationToken = default)
    {
        var message = await animationClipUseCase.RemoveCurveAsync(assetPath, path, componentType, assemblyName,
            propertyName, cancellationToken);
        Console.WriteLine(message);
    }
}
