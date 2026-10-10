using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetScreenSafeAreaResponse
    {
        public string viewType = "";

        // Name of the simulated device. Empty in the Game view.
        public string deviceName = "";

        // Simulated screen orientation (e.g. "Portrait", "LandscapeLeft"). Empty in the Game view.
        public string orientation = "";

        public int screenWidth;
        public int screenHeight;

        // Screen.safeArea and Screen.cutouts in screen coordinates (origin at the bottom-left),
        // the same coordinate system as get_ui_pointer_targets and the mouse tools.
        public ScreenRect safeArea = new ScreenRect(0, 0, 0, 0);
        public ScreenRect[] cutouts = Array.Empty<ScreenRect>();
    }
}
