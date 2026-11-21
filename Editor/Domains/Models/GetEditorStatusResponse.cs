using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetEditorStatusResponse
    {
        public bool isPlaying;
        public bool isPaused;

        // Where the Editor is heading: true while it enters play mode (e.g. while scripts compile first), and false
        // once it is asked to exit play mode, while isPlaying is still true.
        public bool isPlayingOrWillChangePlaymode;

        public GetEditorStatusResponse(bool isPlaying, bool isPaused, bool isPlayingOrWillChangePlaymode)
        {
            this.isPlaying = isPlaying;
            this.isPaused = isPaused;
            this.isPlayingOrWillChangePlaymode = isPlayingOrWillChangePlaymode;
        }
    }
}
