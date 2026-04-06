using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyMovieRecordingOperations : IAllRecorderOperations, IMovieRecordingOperations
    {
        private readonly List<RecorderEntry> _recorders = new List<RecorderEntry>();

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
            var resolvedEncoder = string.IsNullOrEmpty(encoder) ? MovieRecorderEncoderType.UnityMediaEncoder : encoder;
            var resolvedQuality = string.IsNullOrEmpty(encodingQuality) ? MovieRecorderEncodingQuality.Low : encodingQuality;
            _recorders.Add(new RecorderEntry(_recorders.Count, name, "Movie", true, outputPath,
                resolvedEncoder, resolvedQuality, System.Array.Empty<string>()));
            return name;
        }

        public RecorderEntry[] GetRecorderList()
        {
            return _recorders.Select((r, i) =>
                new RecorderEntry(i, r.name, r.type, r.enabled, r.outputPath, r.encoder, r.encodingQuality, r.errors))
                .ToArray();
        }

        public void RemoveMovieRecorder(int index)
        {
            RemoveCallCount++;
            LastRemoveIndex = index;
            _recorders.RemoveAt(index);
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
