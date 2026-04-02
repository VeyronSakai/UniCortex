using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetGameViewSizeResponse
    {
        public bool success;

        public SetGameViewSizeResponse(bool success)
        {
            this.success = success;
        }
    }
}
