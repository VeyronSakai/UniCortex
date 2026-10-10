using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetPlayModeViewTypeResponse
    {
        public string viewType;

        public GetPlayModeViewTypeResponse(string viewType)
        {
            this.viewType = viewType;
        }
    }
}
