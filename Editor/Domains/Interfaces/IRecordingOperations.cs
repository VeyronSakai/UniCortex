using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IRecordingOperations
    {
        string AddRecorder(string name, string outputPath,
            string encoder = RecorderEncoderType.UnityMediaEncoder,
            string encodingQuality = RecorderEncodingQuality.Low);

        RecorderEntry[] GetRecorderList();

        void RemoveRecorder(int index);

        void StartRecording(int index, int fps = RecorderFps.Default);

        string StopRecording();
    }
}
