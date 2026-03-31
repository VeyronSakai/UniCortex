using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Unity Recorder package (com.unity.recorder) is not installed.
    internal sealed class RecordingNotSupportedAdapter : IRecordingOperations
    {
        private const string Message =
            "Unity Recorder package (com.unity.recorder) is not installed. " +
            "Install it via Unity Package Manager to use this feature.";

        public void ConfigureRecorder(string outputPath, string source, string cameraSource,
            string cameraTag, bool captureUI, string outputFormat)
        {
            throw new NotSupportedException(Message);
        }

        public GetRecorderSettingsResponse GetRecorderSettings()
        {
            throw new NotSupportedException(Message);
        }

        public void StartRecording(int fps, string frameRatePlayback, string recordMode,
            float startTime, float endTime, int startFrame, int endFrame, int frameNumber)
        {
            throw new NotSupportedException(Message);
        }

        public string StopRecording()
        {
            throw new NotSupportedException(Message);
        }
    }
}
