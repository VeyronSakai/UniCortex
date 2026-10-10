using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetPlayModeViewTypeResponse
    {
        public string viewType;

        public SetPlayModeViewTypeResponse(string viewType)
        {
            this.viewType = viewType;
        }
    }
}
