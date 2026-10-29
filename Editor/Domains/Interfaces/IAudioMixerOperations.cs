using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IAudioMixerOperations
    {
        CreateAudioMixerResponse Create(string assetPath);
        GetAudioMixerResponse Get(string assetPath);
        AddAudioMixerGroupResponse AddGroup(string assetPath, string groupName, string parentGroup);
        void ExposeParameter(string assetPath, string group, string parameter, string exposedName);
        void SetAudioSourceOutput(int instanceId, int componentIndex, string assetPath, string group);
    }
}
