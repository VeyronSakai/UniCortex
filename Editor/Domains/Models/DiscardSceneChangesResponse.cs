using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DiscardSceneChangesResponse
    {
        public bool success;

        public DiscardSceneChangesResponse(bool success)
        {
            this.success = success;
        }
    }
}
