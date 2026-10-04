using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetPointerTargetsResponse
    {
        public List<PointerTarget> targets;

        public GetPointerTargetsResponse(List<PointerTarget> targets)
        {
            this.targets = targets;
        }
    }
}
