using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Unity Recorder package (com.unity.recorder) is not installed.
    internal sealed class AllRecorderNotSupportedAdapter : IAllRecorderOperations
    {
        public RecorderEntry[] GetRecorderList()
        {
            throw new NotSupportedException(
                "Unity Recorder package (com.unity.recorder) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
    }
}
