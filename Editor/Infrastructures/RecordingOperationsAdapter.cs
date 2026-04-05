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

        public RecordingOperationsAdapter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public string AddRecorder(string name, string outputPath,
            string encoder = RecorderEncoderType.UnityMediaEncoder,
            string encodingQuality = RecorderEncodingQuality.Low)
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

        public void StartRecording(int index, int fps = RecorderFps.Default)
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
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Recorder at index {index} has errors: {string.Join("; ", errors)}");
            }

            var settings = RecorderControllerSettings.GetGlobalSettings();
            settings.FrameRate = fps;
            settings.FrameRatePlayback = FrameRatePlayback.Constant;
            settings.CapFrameRate = true;
            settings.SetRecordModeToManual();

            _controller = new RecorderController(settings);
            _controller.PrepareRecording();
            _controller.StartRecording();
        }

        public string StopRecording()
        {
            if (_controller == null)
            {
                throw new InvalidOperationException("No recording is in progress.");
            }

            var outputPath = GetSessionOutputPath();
            _controller.StopRecording();
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
        }

        private string GetSessionOutputPath()
        {
            var method = typeof(RecorderController).GetMethod("GetRecordingSessions",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                throw new InvalidOperationException(
                    "Failed to find RecorderController.GetRecordingSessions via reflection.");
            }

            var sessions = method.Invoke(_controller, null) as IEnumerable<RecordingSession>;
            var session = sessions?.FirstOrDefault()
                ?? throw new InvalidOperationException("No active recording session found.");

            return session.settings.FileNameGenerator.BuildAbsolutePath(session);
        }

        private static List<string> GetRecorderErrors(RecorderSettings recorder)
        {
            var errors = new List<string>();
            var method = typeof(RecorderSettings).GetMethod("GetErrors",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                throw new InvalidOperationException(
                    "Failed to find RecorderSettings.GetErrors via reflection.");
            }

            method.Invoke(recorder, new object[] { errors });
            return errors;
        }
    }
}
#endif
