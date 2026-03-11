using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PlayResponse
    {
        public bool success;

        public PlayResponse(bool success)
        {
            this.success = success;
        }
    }
}
