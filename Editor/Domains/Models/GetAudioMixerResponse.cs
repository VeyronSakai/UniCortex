using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetAudioMixerResponse
    {
        public List<AudioMixerGroupEntry> groups;
        public List<AudioMixerSnapshotEntry> snapshots;
        public List<AudioMixerExposedParameterEntry> exposedParameters;

        public GetAudioMixerResponse(List<AudioMixerGroupEntry> groups, List<AudioMixerSnapshotEntry> snapshots,
            List<AudioMixerExposedParameterEntry> exposedParameters)
        {
            this.groups = groups;
            this.snapshots = snapshots;
            this.exposedParameters = exposedParameters;
        }
    }
}
