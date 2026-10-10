using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetActivePlatformResponse
    {
        public string activeBuildTarget;

        public GetActivePlatformResponse(string activeBuildTarget)
        {
            this.activeBuildTarget = activeBuildTarget;
        }
    }
}
