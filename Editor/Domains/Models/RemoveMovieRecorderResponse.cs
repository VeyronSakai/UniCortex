using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveMovieRecorderResponse
    {
        public bool success;

        public RemoveMovieRecorderResponse(bool success)
        {
            this.success = success;
        }
    }
}
