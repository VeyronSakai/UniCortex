using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class AudioMixerUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(string assetPath, CancellationToken cancellationToken = default)
    {
        var request = new CreateAudioMixerRequest { assetPath = assetPath };
        var response = await client.PostAsync<CreateAudioMixerRequest, CreateAudioMixerResponse>(
            ApiRoutes.AudioMixerCreate, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> GetAsync(string assetPath, CancellationToken cancellationToken = default)
    {
        var request = new GetAudioMixerRequest { assetPath = assetPath };
        var response = await client.GetAsync<GetAudioMixerRequest, GetAudioMixerResponse>(
            ApiRoutes.AudioMixer, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> AddGroupAsync(string assetPath, string groupName, string? parentGroup,
        CancellationToken cancellationToken = default)
    {
        var request = new AddAudioMixerGroupRequest
        {
            assetPath = assetPath,
            groupName = groupName,
            parentGroup = parentGroup ?? string.Empty
        };
        var response = await client.PostAsync<AddAudioMixerGroupRequest, AddAudioMixerGroupResponse>(
            ApiRoutes.AudioMixerAddGroup, request, cancellationToken);
        return $"Group '{response.path}' added.";
    }

    public async ValueTask<string> ExposeParameterAsync(string assetPath, string group, string parameter,
        string exposedName, CancellationToken cancellationToken = default)
    {
        var request = new ExposeAudioMixerParameterRequest
        {
            assetPath = assetPath,
            group = group,
            parameter = parameter,
            exposedName = exposedName
        };
        await client.PostAsync<ExposeAudioMixerParameterRequest, ExposeAudioMixerParameterResponse>(
            ApiRoutes.AudioMixerExposeParameter, request, cancellationToken);
        return $"The {parameter} of group '{group}' is exposed as '{exposedName}'.";
    }

    public async ValueTask<string> SetAudioSourceOutputAsync(int instanceId, string assetPath, string group,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var request = new SetAudioSourceOutputRequest
        {
            instanceId = instanceId,
            componentIndex = componentIndex,
            assetPath = assetPath,
            group = group
        };
        await client.PostAsync<SetAudioSourceOutputRequest, SetAudioSourceOutputResponse>(
            ApiRoutes.AudioSourceSetOutput, request, cancellationToken);
        return $"AudioSource output of GameObject {instanceId} set to group '{group}' of '{assetPath}'.";
    }
}
