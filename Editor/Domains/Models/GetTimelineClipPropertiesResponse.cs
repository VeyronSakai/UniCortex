using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetTimelineClipPropertiesResponse
    {
        /// <summary>
        /// Full type name of the clip's PlayableAsset (e.g. UnityEngine.Timeline.AnimationPlayableAsset).
        /// </summary>
        public string typeName;

        public List<SerializedPropertyEntry> properties;

        public GetTimelineClipPropertiesResponse(string typeName, List<SerializedPropertyEntry> properties)
        {
            this.typeName = typeName;
            this.properties = properties;
        }
    }
}
