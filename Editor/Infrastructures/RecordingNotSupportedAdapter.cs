using System;
using System.Collections.Generic;
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

        public string AddRecorder(string name, string outputPath, string encoder, string encodingQuality)
        {
            throw new NotSupportedException(Message);
        }

        public IReadOnlyList<RecorderEntry> GetRecorderList()
        {
            throw new NotSupportedException(Message);
        }

        public void RemoveRecorder(int index)
        {
            throw new NotSupportedException(Message);
        }

        public void StartRecording(int index, int fps)
        {
            throw new NotSupportedException(Message);
        }

        public string StopRecording()
        {
            throw new NotSupportedException(Message);
        }
    }
}
