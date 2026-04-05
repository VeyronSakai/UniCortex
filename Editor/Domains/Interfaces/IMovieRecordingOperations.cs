using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IMovieRecordingOperations
    {
        string AddMovieRecorder(string name, string outputPath,
            string encoder = MovieRecorderEncoderType.UnityMediaEncoder,
            string encodingQuality = MovieRecorderEncodingQuality.Low);

        MovieRecorderEntry[] GetMovieRecorderList();

        void RemoveMovieRecorder(int index);

        void StartMovieRecording(int index, int fps = RecorderFps.Default);

        string StopMovieRecording();
    }
}
