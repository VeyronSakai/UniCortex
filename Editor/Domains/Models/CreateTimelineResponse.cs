using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateTimelineResponse
    {
        public string assetPath;
        public int instanceId;

        public CreateTimelineResponse(string assetPath, int instanceId)
        {
            this.assetPath = assetPath;
            this.instanceId = instanceId;
        }
    }
}
