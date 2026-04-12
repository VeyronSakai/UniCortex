using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IProfilerOperations
    {
        void FocusProfilerWindow();
        GetProfilerStatusResponse GetStatus();
        void StartRecording(bool profileEditor);
        void StopRecording();
    }
}
