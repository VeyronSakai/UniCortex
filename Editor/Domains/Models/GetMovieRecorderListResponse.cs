using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetMovieRecorderListResponse
    {
        public MovieRecorderEntry[] recorders;

        public GetMovieRecorderListResponse(MovieRecorderEntry[] recorders)
        {
            this.recorders = recorders;
        }
    }
}
