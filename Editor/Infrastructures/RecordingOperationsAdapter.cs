#if UNICORTEX_RECORDER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class RecordingOperationsAdapter : IRecordingOperations
    {
        private RecorderController _controller;
        private RecorderControllerSettings _controllerSettings;
        private string _activeOutputPath;

        public RecordingOperationsAdapter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public string AddRecorder(string name, string outputPath, string encoder,
            string encodingQuality)
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

        public RecorderEntry[] GetRecorderList()
        {
            var settings = RecorderControllerSettings.GetGlobalSettings();
            return settings.RecorderSettings
                .Select((r, i) =>
                {
                    var outputPath = string.Empty;
                    var encoder = string.Empty;
                    var quality = string.Empty;

                    if (r is MovieRecorderSettings movie)
                    {
                        outputPath = movie.OutputFile ?? string.Empty;
                        encoder = movie.EncoderSettings switch
                        {
                            CoreEncoderSettings => RecorderEncoderType.UnityMediaEncoder,
                            ProResEncoderSettings => RecorderEncoderType.ProRes,
                            GifEncoderSettings => RecorderEncoderType.Gif,
                            _ => movie.EncoderSettings.GetType().Name
                        };
                        if (movie.EncoderSettings is CoreEncoderSettings core)
                        {
                            quality = core.EncodingQuality.ToString();
                        }
                    }

                    var recorderErrors = GetRecorderErrors(r).ToArray();
                    return new RecorderEntry(i, r.name, r.Enabled, outputPath, encoder, quality, recorderErrors);
                })
                .ToArray();
        }

        public void RemoveRecorder(int index)
        {
            var settings = RecorderControllerSettings.GetGlobalSettings();
            var recorders = settings.RecorderSettings.ToArray();
            if (index < 0 || index >= recorders.Length)
            {
                throw new InvalidOperationException(
                    $"Recorder index {index} is out of range (0..{recorders.Length - 1}).");
            }

            settings.RemoveRecorder(recorders[index]);
        }

        public void StartRecording(int index, int fps)
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

            var globalSettings = RecorderControllerSettings.GetGlobalSettings();
            var recorders = globalSettings.RecorderSettings.ToArray();
            if (index < 0 || index >= recorders.Length)
            {
                throw new InvalidOperationException(
                    $"Recorder index {index} is out of range (0..{recorders.Length - 1}).");
            }

            var recorder = recorders[index];
            if (recorder is not MovieRecorderSettings movie)
            {
                throw new InvalidOperationException(
                    $"Recorder at index {index} is not a Movie recorder.");
            }

            var errors = GetRecorderErrors(recorder);
            // Exclude resolution errors since they are corrected to even values at recording time
            errors.RemoveAll(e => e.Contains("resolution"));
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Recorder at index {index} has errors: {string.Join("; ", errors)}");
            }

            _controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _controllerSettings.FrameRate = fps;
            _controllerSettings.FrameRatePlayback = FrameRatePlayback.Constant;
            _controllerSettings.CapFrameRate = true;
            _controllerSettings.SetRecordModeToManual();

            // Resolve output path from the recorder.
            // FileNameGenerator may strip the directory from absolute paths,
            // so fall back to a temp path if the directory is not usable.
            var ext = "." + ((IEncoderSettings)movie.EncoderSettings).Extension;
            var rawPath = movie.OutputFile ?? string.Empty;
            var rawDir = Path.GetDirectoryName(rawPath);
            var needsFallback = string.IsNullOrEmpty(rawPath) || rawPath.Contains("<")
                || !Path.IsPathRooted(rawPath)
                || string.IsNullOrEmpty(rawDir) || rawDir == "/"
                || !Directory.Exists(rawDir);
            if (needsFallback)
            {
                rawPath = Path.Combine(Path.GetTempPath(),
                    $"UniCortex_{movie.name}_{DateTime.Now:yyyyMMdd_HHmmss}");
            }

            _activeOutputPath = Path.ChangeExtension(rawPath, ext);

            // Clone the recorder settings so we don't mutate the global settings
            var clone = UnityEngine.Object.Instantiate(movie);
            clone.name = movie.name;
            clone.OutputFile = rawPath;

            // H.264 (MP4) requires even width and height.
            var input = clone.ImageInputSettings;
            input.OutputWidth = input.OutputWidth & ~1;
            input.OutputHeight = input.OutputHeight & ~1;

            _controllerSettings.AddRecorderSettings(clone);
            _controller = new RecorderController(_controllerSettings);
            _controller.PrepareRecording();
            _controller.StartRecording();
        }

        public string StopRecording()
        {
            if (_controller == null)
            {
                throw new InvalidOperationException("No recording is in progress.");
            }

            _controller.StopRecording();
            var outputPath = _activeOutputPath;
            CleanupRecordingState();
            return outputPath;
        }

        private static IEncoderSettings CreateEncoderSettings(string encoder, string encodingQuality)
        {
            if (string.IsNullOrEmpty(encoder) ||
                string.Equals(encoder, RecorderEncoderType.UnityMediaEncoder, StringComparison.OrdinalIgnoreCase))
            {
                return new CoreEncoderSettings
                {
                    Codec = CoreEncoderSettings.OutputCodec.MP4,
                    EncodingQuality = ParseEncodingQuality(encodingQuality)
                };
            }

            if (string.Equals(encoder, RecorderEncoderType.ProRes, StringComparison.OrdinalIgnoreCase))
                return new ProResEncoderSettings();

            if (string.Equals(encoder, RecorderEncoderType.Gif, StringComparison.OrdinalIgnoreCase))
                return new GifEncoderSettings();

            throw new InvalidOperationException(
                $"Unknown encoder: '{encoder}'. Available: {RecorderEncoderType.UnityMediaEncoder}, {RecorderEncoderType.ProRes}, {RecorderEncoderType.Gif}");
        }

        private static CoreEncoderSettings.VideoEncodingQuality ParseEncodingQuality(string quality)
        {
            if (string.IsNullOrEmpty(quality) ||
                string.Equals(quality, RecorderEncodingQuality.Low, StringComparison.OrdinalIgnoreCase))
                return CoreEncoderSettings.VideoEncodingQuality.Low;
            if (string.Equals(quality, RecorderEncodingQuality.Medium, StringComparison.OrdinalIgnoreCase))
                return CoreEncoderSettings.VideoEncodingQuality.Medium;
            if (string.Equals(quality, RecorderEncodingQuality.High, StringComparison.OrdinalIgnoreCase))
                return CoreEncoderSettings.VideoEncodingQuality.High;
            throw new InvalidOperationException(
                $"Unknown encoding quality: '{quality}'. Available: {RecorderEncodingQuality.Low}, {RecorderEncodingQuality.Medium}, {RecorderEncodingQuality.High}");
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
            _controller = null;

            if (_controllerSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(_controllerSettings);
                _controllerSettings = null;
            }

            _activeOutputPath = null;
        }

        private static List<string> GetRecorderErrors(RecorderSettings recorder)
        {
            var errors = new List<string>();
            var method = typeof(RecorderSettings).GetMethod("GetErrors",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            method?.Invoke(recorder, new object[] { errors });
            return errors;
        }
    }
}
#endif
