using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyAudioMixerOperations : IAudioMixerOperations
    {
        public int CreateCallCount { get; private set; }
        public string LastCreateAssetPath { get; private set; }

        public int GetCallCount { get; private set; }
        public string LastGetAssetPath { get; private set; }

        public GetAudioMixerResponse GetResult { get; set; } = new GetAudioMixerResponse(
            new List<AudioMixerGroupEntry> { new AudioMixerGroupEntry("Master", "Master") },
            new List<AudioMixerSnapshotEntry> { new AudioMixerSnapshotEntry("Snapshot", true) },
            new List<AudioMixerExposedParameterEntry>());

        public int AddGroupCallCount { get; private set; }
        public string LastAddGroupAssetPath { get; private set; }
        public string LastAddGroupName { get; private set; }
        public string LastAddGroupParent { get; private set; }

        public int ExposeParameterCallCount { get; private set; }
        public string LastExposeAssetPath { get; private set; }
        public string LastExposeGroup { get; private set; }
        public string LastExposeParameter { get; private set; }
        public string LastExposeName { get; private set; }

        public int SetOutputCallCount { get; private set; }
        public int LastSetOutputInstanceId { get; private set; }
        public int LastSetOutputComponentIndex { get; private set; }
        public string LastSetOutputAssetPath { get; private set; }
        public string LastSetOutputGroup { get; private set; }

        public CreateAudioMixerResponse Create(string assetPath)
        {
            CreateCallCount++;
            LastCreateAssetPath = assetPath;
            return new CreateAudioMixerResponse(true, assetPath);
        }

        public GetAudioMixerResponse Get(string assetPath)
        {
            GetCallCount++;
            LastGetAssetPath = assetPath;
            return GetResult;
        }

        public AddAudioMixerGroupResponse AddGroup(string assetPath, string groupName, string parentGroup)
        {
            AddGroupCallCount++;
            LastAddGroupAssetPath = assetPath;
            LastAddGroupName = groupName;
            LastAddGroupParent = parentGroup;
            return new AddAudioMixerGroupResponse(true, "Master/" + groupName);
        }

        public void ExposeParameter(string assetPath, string group, string parameter, string exposedName)
        {
            ExposeParameterCallCount++;
            LastExposeAssetPath = assetPath;
            LastExposeGroup = group;
            LastExposeParameter = parameter;
            LastExposeName = exposedName;
        }

        public void SetAudioSourceOutput(int instanceId, int componentIndex, string assetPath, string group)
        {
            SetOutputCallCount++;
            LastSetOutputInstanceId = instanceId;
            LastSetOutputComponentIndex = componentIndex;
            LastSetOutputAssetPath = assetPath;
            LastSetOutputGroup = group;
        }
    }
}
