using System.Collections.Generic;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IAnimationClipOperations
    {
        CreateAnimationClipResponse Create(string assetPath, bool loop, float frameRate);
        GetAnimationCurvesResponse GetCurves(string assetPath);

        void SetCurve(string assetPath, string path, string componentType, string assemblyName,
            string propertyName, List<AnimationCurveKeyInput> keys);

        void RemoveCurve(string assetPath, string path, string componentType, string assemblyName,
            string propertyName);
    }
}
