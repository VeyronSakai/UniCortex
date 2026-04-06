#if UNICORTEX_RECORDER
using System;
using System.IO;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class MovieRecordingOperationsAdapter : IMovieRecordingOperations
    {
        private RecorderController _controller;
        private RecorderSettings _activeRecorderSettings;
        private RecorderControllerSettings _activeControllerSettings;
        private float _savedFrameRate;
        private FrameRatePlayback _savedFrameRatePlayback;
        private bool _savedCapFrameRate;

        public MovieRecordingOperationsAdapter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public string AddMovieRecorder(string name, string outputPath,
            string encoder = MovieRecorderEncoderType.UnityMediaEncoder,
            string encodingQuality = MovieRecorderEncodingQuality.Low)
        {
            var settings = RecorderControllerSettings.GetGlobalSettings();
            var recorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            recorder.Enabled = true;
            recorder.name = name;

            recorder.OutputFile = Path.ChangeExtension(outputPath, null);
            recorder.CaptureAudio = true;
            recorder.ImageInputSettings = new GameViewInputSettings();
            recorder.EncoderSettings = CreateEncoderSettings(encoder, encodingQuality);

            settings.AddRecorderSettings(recorder);

            return recorder.name;
        }

        public void RemoveMovieRecorder(int index)
        {
            var settings = RecorderControllerSettings.GetGlobalSettings();
            var allRecorders = settings.RecorderSettings.ToArray();
            if (index < 0 || index >= allRecorders.Length)
            {
                throw new InvalidOperationException(
                    $"Recorder index {index} is out of range (0..{allRecorders.Length - 1}).");
            }

            if (allRecorders[index] is not MovieRecorderSettings)
            {
                throw new InvalidOperationException(
                    $"Recorder at index {index} is not a MovieRecorderSettings (actual: {allRecorders[index].GetType().Name}).");
            }

            settings.RemoveRecorder(allRecorders[index]);
        }

        public void StartMovieRecording(int index, int fps = RecorderFps.Default)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Recording is only available in Play Mode. Enter Play Mode first.");
            }

            if (_controller != null)
            {
                throw new InvalidOperationException(
                    "A recording is already in progress. Stop the current recording first.");
            }

            // Use a single GetGlobalSettings() call to avoid instance mismatch
            // (each call may return a different deserialized instance).
            var settings = RecorderControllerSettings.GetGlobalSettings();
            var allRecorders = settings.RecorderSettings.ToArray();
            if (index < 0 || index >= allRecorders.Length)
            {
                throw new InvalidOperationException(
                    $"Recorder index {index} is out of range (0..{allRecorders.Length - 1}).");
            }

            if (allRecorders[index] is not MovieRecorderSettings movie)
            {
                throw new InvalidOperationException(
                    $"Recorder at index {index} is not a MovieRecorderSettings (actual: {allRecorders[index].GetType().Name}).");
            }

            var errors = RecorderErrorHelper.GetErrors(movie);
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Recorder at index {index} has errors: {string.Join("; ", errors)}");
            }

            _activeRecorderSettings = movie;
            _activeControllerSettings = settings;

            _savedFrameRate = settings.FrameRate;
            _savedFrameRatePlayback = settings.FrameRatePlayback;
            _savedCapFrameRate = settings.CapFrameRate;

            settings.FrameRate = fps;
            settings.FrameRatePlayback = FrameRatePlayback.Constant;
            settings.CapFrameRate = true;
            settings.SetRecordModeToManual();

            var controller = new RecorderController(settings);
            try
            {
                controller.PrepareRecording();
                controller.StartRecording();
            }
            catch
            {
                CleanupRecordingState();
                throw;
            }

            _controller = controller;
        }

        public string StopMovieRecording()
        {
            if (_controller == null)
            {
                throw new InvalidOperationException("No recording is in progress.");
            }

            var movie = (MovieRecorderSettings)_activeRecorderSettings;
            var outputPath = $"{movie.OutputFile}.{movie.EncoderSettings.Extension}";

            _controller.StopRecording();
            CleanupRecordingState();
            return outputPath;
        }

        private static IEncoderSettings CreateEncoderSettings(string encoder, string encodingQuality)
        {
            if (string.IsNullOrEmpty(encoder) ||
                string.Equals(encoder, MovieRecorderEncoderType.UnityMediaEncoder, StringComparison.OrdinalIgnoreCase))
            {
                return new CoreEncoderSettings
                {
                    Codec = CoreEncoderSettings.OutputCodec.MP4,
                    EncodingQuality = ParseEncodingQuality(encodingQuality)
                };
            }

            if (string.Equals(encoder, MovieRecorderEncoderType.ProRes, StringComparison.OrdinalIgnoreCase))
                return new ProResEncoderSettings();

            if (string.Equals(encoder, MovieRecorderEncoderType.Gif, StringComparison.OrdinalIgnoreCase))
                return new GifEncoderSettings();

            throw new InvalidOperationException(
                $"Unknown encoder: '{encoder}'. Available: {MovieRecorderEncoderType.UnityMediaEncoder}, {MovieRecorderEncoderType.ProRes}, {MovieRecorderEncoderType.Gif}");
        }

        private static CoreEncoderSettings.VideoEncodingQuality ParseEncodingQuality(string quality)
        {
            if (string.IsNullOrEmpty(quality) ||
                string.Equals(quality, MovieRecorderEncodingQuality.Low, StringComparison.OrdinalIgnoreCase))
                return CoreEncoderSettings.VideoEncodingQuality.Low;
            if (string.Equals(quality, MovieRecorderEncodingQuality.Medium, StringComparison.OrdinalIgnoreCase))
                return CoreEncoderSettings.VideoEncodingQuality.Medium;
            if (string.Equals(quality, MovieRecorderEncodingQuality.High, StringComparison.OrdinalIgnoreCase))
                return CoreEncoderSettings.VideoEncodingQuality.High;
            throw new InvalidOperationException(
                $"Unknown encoding quality: '{quality}'. Available: {MovieRecorderEncodingQuality.Low}, {MovieRecorderEncodingQuality.Medium}, {MovieRecorderEncodingQuality.High}");
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode && _controller != null)
            {
                _controller.StopRecording();
                CleanupRecordingState();
            }
        }

        private void CleanupRecordingState()
        {
            RestoreGlobalSettings();
            _controller = null;
            _activeRecorderSettings = null;
            _activeControllerSettings = null;
        }

        private void RestoreGlobalSettings()
        {
            if (_activeControllerSettings == null) return;
            _activeControllerSettings.FrameRate = _savedFrameRate;
            _activeControllerSettings.FrameRatePlayback = _savedFrameRatePlayback;
            _activeControllerSettings.CapFrameRate = _savedCapFrameRate;
        }

    }
}
#endif
