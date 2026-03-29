using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class FocusGameViewResponse
    {
        public bool success;

        public FocusGameViewResponse(bool success)
        {
            this.success = success;
        }
    }
}
