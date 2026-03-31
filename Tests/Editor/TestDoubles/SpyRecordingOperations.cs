using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyRecordingOperations : IRecordingOperations
    {
        public int StartRecordingCallCount { get; private set; }
        public int LastFps { get; private set; }
        public string LastOutputPath { get; private set; }

        public int StopRecordingCallCount { get; private set; }
        public string StopRecordingResult { get; set; } = "/tmp/test_recording.mp4";

        public void StartRecording(int fps, string outputPath)
        {
            StartRecordingCallCount++;
            LastFps = fps;
            LastOutputPath = outputPath;
        }

        public string StopRecording()
        {
            StopRecordingCallCount++;
            return StopRecordingResult;
        }
    }
}
