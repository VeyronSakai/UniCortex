using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetUiPointerTargetsResponse
    {
        public List<UiPointerTargetEntry> targets;

        public GetUiPointerTargetsResponse(List<UiPointerTargetEntry> targets)
        {
            this.targets = targets;
        }
    }
}
