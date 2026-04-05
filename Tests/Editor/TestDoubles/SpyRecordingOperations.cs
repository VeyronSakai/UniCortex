using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyRecordingOperations : IRecordingOperations
    {
        private readonly Dictionary<string, RecorderEntry> _recorders = new Dictionary<string, RecorderEntry>();

        public int AddCallCount { get; private set; }
        public string LastAddName { get; private set; }
        public string LastAddOutputPath { get; private set; }
        public string LastAddEncoder { get; private set; }
        public string LastAddEncodingQuality { get; private set; }

        public int RemoveCallCount { get; private set; }
        public int LastRemoveIndex { get; private set; }

        public int StartRecordingCallCount { get; private set; }
        public int LastStartIndex { get; private set; }
        public int LastFps { get; private set; }

        public int StopRecordingCallCount { get; private set; }
        public string StopRecordingResult { get; set; } = "/tmp/test_recording.mp4";

        public string AddRecorder(string name, string outputPath, string encoder,
            string encodingQuality)
        {
            AddCallCount++;
            LastAddName = name;
            LastAddOutputPath = outputPath;
            LastAddEncoder = encoder;
            LastAddEncodingQuality = encodingQuality;
            var quality = string.IsNullOrEmpty(encodingQuality) ? RecorderDefaults.QualityLow : encodingQuality;
            _recorders[name] = new RecorderEntry(_recorders.Count, name, true, outputPath,
                RecorderDefaults.EncoderUnityMedia, quality, System.Array.Empty<string>());
            return name;
        }

        public RecorderEntry[] GetRecorderList()
        {
            return _recorders.Values.ToArray();
        }

        public void RemoveRecorder(int index)
        {
            RemoveCallCount++;
            LastRemoveIndex = index;
            var key = _recorders.Keys.ElementAt(index);
            _recorders.Remove(key);
        }

        public void StartRecording(int index, int fps)
        {
            StartRecordingCallCount++;
            LastStartIndex = index;
            LastFps = fps;
        }

        public string StopRecording()
        {
            StopRecordingCallCount++;
            return StopRecordingResult;
        }
    }
}
