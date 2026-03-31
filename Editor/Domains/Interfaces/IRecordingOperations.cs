namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IRecordingOperations
    {
        void StartRecording(int fps, string outputPath);
        string StopRecording();
    }
}
