using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CaptureGameViewRequest
    {
        // Draw the outline of the safe area (Screen.safeArea) on the captured image.
        public bool drawSafeArea;

        // Draw the device frame of the Simulator view (bezel, rounded corners, notch) around the image.
        // When omitted, the frame is drawn in the Simulator view only. true is an error in the Game view.
        public bool? deviceFrame;
    }
}
