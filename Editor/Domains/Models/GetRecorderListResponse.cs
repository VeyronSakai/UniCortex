using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetRecorderListResponse
    {
        public RecorderEntry[] recorders;

        public GetRecorderListResponse(RecorderEntry[] recorders)
        {
            this.recorders = recorders;
        }
    }
}
