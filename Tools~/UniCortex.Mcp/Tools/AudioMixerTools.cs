using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class AudioMixerTools(AudioMixerUseCase audioMixerUseCase, IAsyncOperationSequencer sequencer)
{
    private const string AssetPathDescription =
        "The asset path of the AudioMixer (e.g. \"Assets/Audio/Main.mixer\").";

    private const string GroupDescription =
        "The mixer group: a path from the master group (e.g. \"Master/Music\") or a group name (e.g. \"Music\"). " +
        "Use the path when several groups share the name. Use get_audio_mixer to list groups.";

    [McpServerTool(Name = "create_audio_mixer", ReadOnly = false),
     Description("Create a new AudioMixer asset (.mixer) with a \"Master\" group and one snapshot, " +
                 "the same as Assets > Create > Audio Mixer."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreateAudioMixerAsync(
        [Description("Asset path where the AudioMixer will be saved. Must end with \".mixer\" " +
                     "(e.g. \"Assets/Audio/Main.mixer\"). Missing folders are created.")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => audioMixerUseCase.CreateAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "get_audio_mixer", ReadOnly = true),
     Description("Get the groups (with paths like \"Master/Music\"), snapshots and exposed parameters of an " +
                 "AudioMixer. Each exposed parameter lists its name, group path and parameter " +
                 "(volume, pitch, or other for effect parameters)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetAudioMixerAsync(
        [Description(AssetPathDescription)] string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => audioMixerUseCase.GetAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "add_audio_mixer_group", ReadOnly = false),
     Description("Add a child group to an AudioMixer (e.g. \"Music\" or \"Effect\" under Master). Supports Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> AddAudioMixerGroupAsync(
        [Description(AssetPathDescription)] string assetPath,
        [Description("Name of the new group. Must not contain '/' and must be unique among its siblings.")]
        string groupName,
        [Description("The parent group: a path from the master group (e.g. \"Master/Music\") or a group name. " +
                     "Default: the master group.")]
        string? parentGroup = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => audioMixerUseCase.AddGroupAsync(assetPath, groupName, parentGroup, ct), cancellationToken);

    [McpServerTool(Name = "expose_audio_mixer_parameter", ReadOnly = false),
     Description("Expose a group's volume or pitch as an exposed parameter with the given name, so runtime " +
                 "code can call AudioMixer.SetFloat / GetFloat with it (volume is in dB). " +
                 "If the parameter is already exposed, it is renamed. Supports Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ExposeAudioMixerParameterAsync(
        [Description(AssetPathDescription)] string assetPath,
        [Description(GroupDescription)] string group,
        [Description("The group parameter to expose: \"volume\" or \"pitch\".")]
        string parameter,
        [Description("Name of the exposed parameter (e.g. \"MusicVolume\"). Must be unique in the mixer.")]
        string exposedName,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => audioMixerUseCase.ExposeParameterAsync(assetPath, group, parameter, exposedName, ct),
            cancellationToken);

    [McpServerTool(Name = "set_audio_source_output", ReadOnly = false),
     Description("Route an AudioSource (in a scene or in Prefab Mode) to an AudioMixer group by setting its " +
                 "Output (outputAudioMixerGroup). Supports Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetAudioSourceOutputAsync(
        [Description("The instance ID of the GameObject that has the AudioSource.")]
        int instanceId,
        [Description(AssetPathDescription)] string assetPath,
        [Description(GroupDescription)] string group,
        [Description("Index of the AudioSource when the GameObject has several. Default: 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => audioMixerUseCase.SetAudioSourceOutputAsync(instanceId, assetPath, group, componentIndex, ct),
            cancellationToken);
}
