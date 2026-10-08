using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TypeTextResponse
    {
        public bool success;

        public TypeTextResponse(bool success)
        {
            this.success = success;
        }
    }
}
