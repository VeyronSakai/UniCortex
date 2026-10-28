using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class AudioMixerCommands(AudioMixerUseCase audioMixerUseCase)
{
    /// <summary>Create a new AudioMixer asset (.mixer) with a Master group and one snapshot.</summary>
    /// <param name="assetPath">Asset path ending with ".mixer" (e.g. "Assets/Audio/Main.mixer").</param>
    [Command("create")]
    public async Task Create([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var json = await audioMixerUseCase.CreateAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Show the groups, snapshots and exposed parameters of an AudioMixer as JSON.</summary>
    /// <param name="assetPath">Asset path of the AudioMixer.</param>
    [Command("get")]
    public async Task Get([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var json = await audioMixerUseCase.GetAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }
}

public class AudioMixerGroupCommands(AudioMixerUseCase audioMixerUseCase)
{
    /// <summary>Add a child group to an AudioMixer.</summary>
    /// <param name="assetPath">Asset path of the AudioMixer.</param>
    /// <param name="groupName">Name of the new group.</param>
    /// <param name="parent">Parent group path (e.g. "Master/Music") or name. Default: the master group.</param>
    [Command("add")]
    public async Task Add([Argument] string assetPath, [Argument] string groupName, string? parent = null,
        CancellationToken cancellationToken = default)
    {
        var message = await audioMixerUseCase.AddGroupAsync(assetPath, groupName, parent, cancellationToken);
        Console.WriteLine(message);
    }
}

public class AudioMixerParameterCommands(AudioMixerUseCase audioMixerUseCase)
{
    /// <summary>Expose a group's volume or pitch under the given name (renames it if already exposed).</summary>
    /// <param name="assetPath">Asset path of the AudioMixer.</param>
    /// <param name="group">Group path (e.g. "Master/Music") or name.</param>
    /// <param name="parameter">"volume" or "pitch".</param>
    /// <param name="exposedName">Name of the exposed parameter (e.g. "MusicVolume").</param>
    [Command("expose")]
    public async Task Expose([Argument] string assetPath, [Argument] string group, [Argument] string parameter,
        [Argument] string exposedName, CancellationToken cancellationToken = default)
    {
        var message = await audioMixerUseCase.ExposeParameterAsync(assetPath, group, parameter, exposedName,
            cancellationToken);
        Console.WriteLine(message);
    }
}

public class AudioSourceCommands(AudioMixerUseCase audioMixerUseCase)
{
    /// <summary>Route an AudioSource to an AudioMixer group.</summary>
    /// <param name="instanceId">Instance ID of the GameObject that has the AudioSource.</param>
    /// <param name="assetPath">Asset path of the AudioMixer.</param>
    /// <param name="group">Group path (e.g. "Master/Music") or name.</param>
    /// <param name="componentIndex">Index of the AudioSource when the GameObject has several.</param>
    [Command("set-output")]
    public async Task SetOutput([Argument] int instanceId, [Argument] string assetPath, [Argument] string group,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var message = await audioMixerUseCase.SetAudioSourceOutputAsync(instanceId, assetPath, group,
            componentIndex, cancellationToken);
        Console.WriteLine(message);
    }
}
