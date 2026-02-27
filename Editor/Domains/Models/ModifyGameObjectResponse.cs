using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ModifyGameObjectResponse
    {
        public bool success;

        public ModifyGameObjectResponse(bool success)
        {
            this.success = success;
        }
    }
}
