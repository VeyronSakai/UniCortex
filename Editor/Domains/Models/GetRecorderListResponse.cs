using System;
using System.Collections.Generic;
using System.Linq;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetRecorderListResponse
    {
        public RecorderEntry[] recorders;

        public GetRecorderListResponse(IReadOnlyList<RecorderEntry> recorders)
        {
            this.recorders = recorders.ToArray();
        }
    }
}
