namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ICaptureOperations
    {
        byte[] CaptureGameView();
        byte[] CaptureSceneView();
    }
}
