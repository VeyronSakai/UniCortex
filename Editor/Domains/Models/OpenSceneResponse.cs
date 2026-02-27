using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class OpenSceneResponse
    {
        public bool success;

        public OpenSceneResponse(bool success)
        {
            this.success = success;
        }
    }
}
