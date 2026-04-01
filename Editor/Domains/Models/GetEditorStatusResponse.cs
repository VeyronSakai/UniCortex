using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetEditorStatusResponse
    {
        public bool isPlaying;
        public bool isPaused;

        public GetEditorStatusResponse(bool isPlaying, bool isPaused)
        {
            this.isPlaying = isPlaying;
            this.isPaused = isPaused;
        }
    }
}
