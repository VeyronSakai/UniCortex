using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetEditorStatusResponse
    {
        public bool isPlaying;
        public bool isPaused;
        public int screenWidth;
        public int screenHeight;

        public GetEditorStatusResponse(bool isPlaying, bool isPaused, int screenWidth, int screenHeight)
        {
            this.isPlaying = isPlaying;
            this.isPaused = isPaused;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
        }
    }
}
