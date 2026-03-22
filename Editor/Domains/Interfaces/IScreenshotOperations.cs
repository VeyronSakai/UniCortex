namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IScreenshotOperations
    {
        byte[] CaptureGameView();
        byte[] CaptureSceneView();
    }
}
