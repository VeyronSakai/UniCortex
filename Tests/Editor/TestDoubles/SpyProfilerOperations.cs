using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyProfilerOperations : IProfilerOperations
    {
        public int FocusProfilerWindowCallCount { get; private set; }
        public int GetStatusCallCount { get; private set; }
        public int StartRecordingCallCount { get; private set; }
        public int StopRecordingCallCount { get; private set; }
        public bool LastProfileEditor { get; private set; }

        public GetProfilerStatusResponse StatusResponse { get; set; } =
            new GetProfilerStatusResponse(false, false, false, false);

        public void FocusProfilerWindow()
        {
            FocusProfilerWindowCallCount++;
        }

        public GetProfilerStatusResponse GetStatus()
        {
            GetStatusCallCount++;
            return StatusResponse;
        }

        public void StartRecording(bool profileEditor)
        {
            StartRecordingCallCount++;
            LastProfileEditor = profileEditor;
        }

        public void StopRecording()
        {
            StopRecordingCallCount++;
        }
    }
}
