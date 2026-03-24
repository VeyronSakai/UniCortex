using UniCortex.Editor.Domains.Interfaces;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class TimeOperationsAdapter : ITimeOperations
    {
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }
    }
}
