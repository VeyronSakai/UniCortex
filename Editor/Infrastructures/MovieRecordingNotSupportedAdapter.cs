using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Unity Recorder package (com.unity.recorder) is not installed.
    internal sealed class MovieRecordingNotSupportedAdapter : IMovieRecordingOperations
    {
        private const string Message =
            "Unity Recorder package (com.unity.recorder) is not installed. " +
            "Install it via Unity Package Manager to use this feature.";

        public string AddMovieRecorder(string name, string outputPath,
            string encoder = MovieRecorderEncoderType.UnityMediaEncoder,
            string encodingQuality = MovieRecorderEncodingQuality.Low,
            bool captureAudio = false)
        {
            throw new NotSupportedException(Message);
        }

        public void RemoveMovieRecorder(int index)
        {
            throw new NotSupportedException(Message);
        }

        public void StartMovieRecording(int index, int fps = RecorderFps.Default)
        {
            throw new NotSupportedException(Message);
        }

        public string StopMovieRecording()
        {
            throw new NotSupportedException(Message);
        }
    }
}
