using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyAnimationClipOperations : IAnimationClipOperations
    {
        public int CreateCallCount { get; private set; }
        public string LastCreateAssetPath { get; private set; }
        public bool LastCreateLoop { get; private set; }
        public float LastCreateFrameRate { get; private set; }

        public int GetCurvesCallCount { get; private set; }
        public string LastGetCurvesAssetPath { get; private set; }
        public GetAnimationCurvesResponse GetCurvesResult { get; set; }

        public int SetCurveCallCount { get; private set; }
        public string LastSetCurveAssetPath { get; private set; }
        public string LastSetCurveAnimatorRelativePath { get; private set; }
        public string LastSetCurveComponentType { get; private set; }
        public string LastSetCurveAssemblyName { get; private set; }
        public string LastSetCurvePropertyName { get; private set; }
        public List<AnimationCurveKeyInput> LastSetCurveKeys { get; private set; }

        public int RemoveCurveCallCount { get; private set; }
        public string LastRemoveCurveAssetPath { get; private set; }
        public string LastRemoveCurveAnimatorRelativePath { get; private set; }
        public string LastRemoveCurveComponentType { get; private set; }
        public string LastRemoveCurveAssemblyName { get; private set; }
        public string LastRemoveCurvePropertyName { get; private set; }

        public CreateAnimationClipResponse Create(string assetPath, bool loop, float frameRate)
        {
            CreateCallCount++;
            LastCreateAssetPath = assetPath;
            LastCreateLoop = loop;
            LastCreateFrameRate = frameRate;
            return new CreateAnimationClipResponse(true, assetPath);
        }

        public GetAnimationCurvesResponse GetCurves(string assetPath)
        {
            GetCurvesCallCount++;
            LastGetCurvesAssetPath = assetPath;
            return GetCurvesResult;
        }

        public void SetCurve(string assetPath, string animatorRelativePath, string componentType, string assemblyName,
            string propertyName, List<AnimationCurveKeyInput> keys)
        {
            SetCurveCallCount++;
            LastSetCurveAssetPath = assetPath;
            LastSetCurveAnimatorRelativePath = animatorRelativePath;
            LastSetCurveComponentType = componentType;
            LastSetCurveAssemblyName = assemblyName;
            LastSetCurvePropertyName = propertyName;
            LastSetCurveKeys = keys;
        }

        public void RemoveCurve(string assetPath, string animatorRelativePath, string componentType,
            string assemblyName, string propertyName)
        {
            RemoveCurveCallCount++;
            LastRemoveCurveAssetPath = assetPath;
            LastRemoveCurveAnimatorRelativePath = animatorRelativePath;
            LastRemoveCurveComponentType = componentType;
            LastRemoveCurveAssemblyName = assemblyName;
            LastRemoveCurvePropertyName = propertyName;
        }
    }
}
