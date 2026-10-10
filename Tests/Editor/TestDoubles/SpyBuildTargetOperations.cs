using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyBuildTargetOperations : IBuildTargetOperations
    {
        public string ActiveBuildTarget { get; set; } = "StandaloneOSX";
        public string[] KnownBuildTargets { get; set; } = { "StandaloneOSX", "StandaloneWindows64", "iOS", "Android" };
        public string[] SupportedBuildTargets { get; set; } = { "StandaloneOSX", "Android" };
        public bool SwitchSucceeds { get; set; } = true;
        public List<string> SwitchedBuildTargets { get; } = new List<string>();

        public string GetActiveBuildTarget()
        {
            return ActiveBuildTarget;
        }

        public string ResolveBuildTarget(string name)
        {
            return KnownBuildTargets.FirstOrDefault(target =>
                string.Equals(target, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsBuildTargetSupported(string buildTarget)
        {
            return SupportedBuildTargets.Contains(buildTarget);
        }

        public string[] GetSupportedBuildTargets()
        {
            return SupportedBuildTargets;
        }

        public bool SwitchActiveBuildTarget(string buildTarget)
        {
            SwitchedBuildTargets.Add(buildTarget);
            if (SwitchSucceeds)
            {
                ActiveBuildTarget = buildTarget;
            }

            return SwitchSucceeds;
        }
    }
}
