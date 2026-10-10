using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class BuildTargetOperationsAdapter : IBuildTargetOperations
    {
        // Obsolete aliases (e.g. iPhone) share values with current names, and Enum.ToString() may return
        // either name for such a value, so build targets are named only by their non-obsolete names.
        private static readonly KeyValuePair<string, BuildTarget>[] s_buildTargets = typeof(BuildTarget)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => !field.IsDefined(typeof(ObsoleteAttribute), false))
            .Select(field => new KeyValuePair<string, BuildTarget>(field.Name, (BuildTarget)field.GetValue(null)))
            .Where(pair => pair.Value != BuildTarget.NoTarget)
            .ToArray();

        public string GetActiveBuildTarget()
        {
            var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            foreach (var pair in s_buildTargets)
            {
                if (pair.Value == activeBuildTarget)
                {
                    return pair.Key;
                }
            }

            return activeBuildTarget.ToString();
        }

        public string ResolveBuildTarget(string name)
        {
            foreach (var pair in s_buildTargets)
            {
                if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Key;
                }
            }

            return null;
        }

        public bool IsBuildTargetSupported(string buildTarget)
        {
            return IsSupported(Parse(buildTarget));
        }

        public string[] GetSupportedBuildTargets()
        {
            return s_buildTargets
                .Where(pair => IsSupported(pair.Value))
                .Select(pair => pair.Key)
                .ToArray();
        }

        public bool SwitchActiveBuildTarget(string buildTarget)
        {
            var target = Parse(buildTarget);
            return EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
        }

        private static bool IsSupported(BuildTarget target)
        {
            return BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target);
        }

        private static BuildTarget Parse(string buildTarget)
        {
            return (BuildTarget)Enum.Parse(typeof(BuildTarget), buildTarget);
        }
    }
}
