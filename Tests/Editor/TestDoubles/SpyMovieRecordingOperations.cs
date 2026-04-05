using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyMovieRecordingOperations : IMovieRecordingOperations
    {
        private readonly Dictionary<string, MovieRecorderEntry> _recorders = new Dictionary<string, MovieRecorderEntry>();

        public int AddCallCount { get; private set; }
        public string LastAddName { get; private set; }
        public string LastAddOutputPath { get; private set; }
        public string LastAddEncoder { get; private set; }
        public string LastAddEncodingQuality { get; private set; }

        public int RemoveCallCount { get; private set; }
        public int LastRemoveIndex { get; private set; }

        public int StartMovieRecordingCallCount { get; private set; }
        public int LastStartIndex { get; private set; }
        public int LastFps { get; private set; }

        public int StopMovieRecordingCallCount { get; private set; }
        public string StopMovieRecordingResult { get; set; } = "/tmp/test_recording.mp4";

        public string AddMovieRecorder(string name, string outputPath, string encoder,
            string encodingQuality)
        {
            AddCallCount++;
            LastAddName = name;
            LastAddOutputPath = outputPath;
            LastAddEncoder = encoder;
            LastAddEncodingQuality = encodingQuality;
            var quality = string.IsNullOrEmpty(encodingQuality) ? MovieRecorderEncodingQuality.Low : encodingQuality;
            _recorders[name] = new MovieRecorderEntry(_recorders.Count, name, true, outputPath,
                MovieRecorderEncoderType.UnityMediaEncoder, quality, System.Array.Empty<string>());
            return name;
        }

        public MovieRecorderEntry[] GetMovieRecorderList()
        {
            return _recorders.Values.ToArray();
        }

        public void RemoveMovieRecorder(int index)
        {
            RemoveCallCount++;
            LastRemoveIndex = index;
            var key = _recorders.Keys.ElementAt(index);
            _recorders.Remove(key);
        }

        public void StartMovieRecording(int index, int fps)
        {
            StartMovieRecordingCallCount++;
            LastStartIndex = index;
            LastFps = fps;
        }

        public string StopMovieRecording()
        {
            StopMovieRecordingCallCount++;
            return StopMovieRecordingResult;
        }
    }
}
