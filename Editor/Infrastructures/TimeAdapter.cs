using UniCortex.Editor.Domains.Interfaces;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class TimeAdapter : ITime
    {
        public double UnscaledTime => Time.unscaledTimeAsDouble;
    }
}
