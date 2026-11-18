using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SaveSceneAsResponse
    {
        public bool success;

        public SaveSceneAsResponse(bool success)
        {
            this.success = success;
        }
    }
}
