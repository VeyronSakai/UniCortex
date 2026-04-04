using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IRecordingOperations
    {
        string AddRecorder(string name, string outputPath, string encoder, string encodingQuality);

        GetRecorderListResponse GetRecorderList();

        void RemoveRecorder(int index);

        void StartRecording(int index, int fps);

        string StopRecording();
    }
}
