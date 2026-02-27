using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetAssetPropertyResponse
    {
        public bool success;

        public SetAssetPropertyResponse(bool success)
        {
            this.success = success;
        }
    }
}
