namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IUtilityOperations
    {
        bool ExecuteMenuItem(string menuPath);
        byte[] CaptureScreenshot();
    }
}
