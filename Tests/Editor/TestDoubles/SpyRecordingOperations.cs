using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyRecordingOperations : IRecordingOperations
    {
        public int ConfigureCallCount { get; private set; }
        public string LastConfigOutputPath { get; private set; }
        public string LastConfigSource { get; private set; }
        public string LastConfigCameraSource { get; private set; }
        public string LastConfigCameraTag { get; private set; }
        public bool LastConfigCaptureUI { get; private set; }
        public int LastConfigOutputWidth { get; private set; }
        public int LastConfigOutputHeight { get; private set; }
        public string LastConfigOutputFormat { get; private set; }

        public GetRecorderSettingsResponse SettingsResult { get; set; }
            = new GetRecorderSettingsResponse("", "GameView", "", "", false, 0, 0, "MP4");

        public int StartRecordingCallCount { get; private set; }
        public int LastFps { get; private set; }
        public string LastFrameRatePlayback { get; private set; }
        public string LastRecordMode { get; private set; }
        public float LastStartTime { get; private set; }
        public float LastEndTime { get; private set; }
        public int LastStartFrame { get; private set; }
        public int LastEndFrame { get; private set; }
        public int LastFrameNumber { get; private set; }

        public int StopRecordingCallCount { get; private set; }
        public string StopRecordingResult { get; set; } = "/tmp/test_recording.mp4";

        public void ConfigureRecorder(string outputPath, string source, string cameraSource,
            string cameraTag, bool captureUI, int outputWidth, int outputHeight, string outputFormat)
        {
            ConfigureCallCount++;
            LastConfigOutputPath = outputPath;
            LastConfigSource = source;
            LastConfigCameraSource = cameraSource;
            LastConfigCameraTag = cameraTag;
            LastConfigCaptureUI = captureUI;
            LastConfigOutputWidth = outputWidth;
            LastConfigOutputHeight = outputHeight;
            LastConfigOutputFormat = outputFormat;
        }

        public GetRecorderSettingsResponse GetRecorderSettings()
        {
            return SettingsResult;
        }

        public void StartRecording(int fps, string frameRatePlayback, string recordMode,
            float startTime, float endTime, int startFrame, int endFrame, int frameNumber)
        {
            StartRecordingCallCount++;
            LastFps = fps;
            LastFrameRatePlayback = frameRatePlayback;
            LastRecordMode = recordMode;
            LastStartTime = startTime;
            LastEndTime = endTime;
            LastStartFrame = startFrame;
            LastEndFrame = endFrame;
            LastFrameNumber = frameNumber;
        }

        public string StopRecording()
        {
            StopRecordingCallCount++;
            return StopRecordingResult;
        }
    }
}
