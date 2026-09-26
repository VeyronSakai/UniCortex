using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetAnimationCurvesResponse
    {
        public float frameRate;
        public bool loop;
        public float length;
        public List<AnimationCurveEntry> curves;

        public GetAnimationCurvesResponse(float frameRate, bool loop, float length, List<AnimationCurveEntry> curves)
        {
            this.frameRate = frameRate;
            this.loop = loop;
            this.length = length;
            this.curves = curves;
        }
    }
}
