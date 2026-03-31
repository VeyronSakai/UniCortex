using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IRecordingOperations
    {
        void ConfigureRecorder(string outputPath, string source, string cameraSource,
            string cameraTag, bool captureUI, string outputFormat);

        GetRecorderSettingsResponse GetRecorderSettings();

        void StartRecording(int fps, string frameRatePlayback, string recordMode,
            float startTime, float endTime, int startFrame, int endFrame, int frameNumber);

        string StopRecording();
    }
}
