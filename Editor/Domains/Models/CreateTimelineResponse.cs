using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateTimelineResponse
    {
        public bool success;
        public string assetPath;

        public CreateTimelineResponse(bool success, string assetPath)
        {
            this.success = success;
            this.assetPath = assetPath;
        }
    }
}
