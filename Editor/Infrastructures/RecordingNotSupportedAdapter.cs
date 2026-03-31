using System;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Unity Recorder package (com.unity.recorder) is not installed.
    internal sealed class RecordingNotSupportedAdapter : IRecordingOperations
    {
        public void StartRecording(int fps, string outputPath)
        {
            throw new NotSupportedException(
                "Unity Recorder package (com.unity.recorder) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public string StopRecording()
        {
            throw new NotSupportedException(
                "Unity Recorder package (com.unity.recorder) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
    }
}
