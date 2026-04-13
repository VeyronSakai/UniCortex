using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetProfilerStatusResponse
    {
        public bool isWindowOpen;
        public bool hasFocus;
        public bool isRecording;
        public bool profileEditor;

        public GetProfilerStatusResponse(bool isWindowOpen, bool hasFocus, bool isRecording, bool profileEditor)
        {
            this.isWindowOpen = isWindowOpen;
            this.hasFocus = hasFocus;
            this.isRecording = isRecording;
            this.profileEditor = profileEditor;
        }
    }
}
