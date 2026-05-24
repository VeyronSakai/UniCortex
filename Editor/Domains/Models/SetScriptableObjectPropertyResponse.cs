using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetScriptableObjectPropertyResponse
    {
        public bool success;

        public SetScriptableObjectPropertyResponse(bool success)
        {
            this.success = success;
        }
    }
}
