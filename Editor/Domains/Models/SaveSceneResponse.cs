using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SaveSceneResponse
    {
        public bool success;

        public SaveSceneResponse(bool success)
        {
            this.success = success;
        }
    }
}
