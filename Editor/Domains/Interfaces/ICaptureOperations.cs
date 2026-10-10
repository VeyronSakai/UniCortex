using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ICaptureOperations
    {
        // Draws the safe area and the cutouts onto the image when safeAreaToDraw is not null.
        // Draws the device frame of the Simulator view around the image when drawDeviceFrame is true.
        byte[] CaptureGameView(GetScreenSafeAreaResponse safeAreaToDraw, bool drawDeviceFrame);
        byte[] CaptureSceneView();
    }
}
