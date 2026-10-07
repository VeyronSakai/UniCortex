using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetUIPointerTargetsResponse
    {
        public List<UIPointerTargetEntry> targets;

        public GetUIPointerTargetsResponse(List<UIPointerTargetEntry> targets)
        {
            this.targets = targets;
        }
    }
}
