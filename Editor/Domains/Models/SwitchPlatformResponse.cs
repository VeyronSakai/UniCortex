using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SwitchPlatformResponse
    {
        public string previousBuildTarget;
        public string activeBuildTarget;

        public SwitchPlatformResponse(string previousBuildTarget, string activeBuildTarget)
        {
            this.previousBuildTarget = previousBuildTarget;
            this.activeBuildTarget = activeBuildTarget;
        }
    }
}
