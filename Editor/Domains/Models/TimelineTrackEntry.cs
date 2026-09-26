using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TimelineTrackEntry
    {
        /// <summary>
        /// Index used as trackIndex by the other Timeline tools (order of output tracks, groups excluded).
        /// </summary>
        public int index;

        public string name;
        public string type;

        /// <summary>
        /// Name of the parent group track. Empty when the track is at the root of the timeline.
        /// </summary>
        public string groupName;

        public bool muted;
        public bool locked;

        /// <summary>
        /// instanceId of the bound object. 0 when the track is unbound or the timeline was read by asset path.
        /// </summary>
        public int bindingInstanceId;

        public string bindingName;
        public string bindingType;

        public List<TimelineClipEntry> clips;
    }
}
