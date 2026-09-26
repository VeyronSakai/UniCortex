using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetTimelineTrackPropertiesResponse
    {
        /// <summary>
        /// Full type name of the track (e.g. UnityEngine.Timeline.AnimationTrack).
        /// </summary>
        public string typeName;

        public List<SerializedPropertyEntry> properties;

        public GetTimelineTrackPropertiesResponse(string typeName, List<SerializedPropertyEntry> properties)
        {
            this.typeName = typeName;
            this.properties = properties;
        }
    }
}
