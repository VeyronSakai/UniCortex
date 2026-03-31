#if UNICORTEX_RECORDER
using System;
using System.IO;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class RecordingOperationsAdapter : IRecordingOperations
    {
        private RecorderController _controller;
        private RecorderControllerSettings _controllerSettings;
        private MovieRecorderSettings _recorderSettings;
        private string _outputPath;

        public RecordingOperationsAdapter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void StartRecording(int fps, string outputPath)
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

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.Combine(Path.GetTempPath(),
                    $"UniCortex_Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
            }

            _outputPath = outputPath;

            _controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _controllerSettings.FrameRate = fps;
            _controllerSettings.FrameRatePlayback = FrameRatePlayback.Constant;
            _controllerSettings.CapFrameRate = true;
            _controllerSettings.SetRecordModeToManual();

            _recorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            _recorderSettings.Enabled = true;
            _recorderSettings.OutputFile = Path.ChangeExtension(outputPath, null);
            _recorderSettings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
            _recorderSettings.ImageInputSettings = new GameViewInputSettings();

            _controllerSettings.AddRecorderSettings(_recorderSettings);

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
            var outputPath = _outputPath;
            Cleanup();
            return outputPath;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode && _controller != null)
            {
                _controller.StopRecording();
                Cleanup();
            }
        }

        private void Cleanup()
        {
            _controller = null;

            if (_controllerSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(_controllerSettings);
                _controllerSettings = null;
            }

            if (_recorderSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(_recorderSettings);
                _recorderSettings = null;
            }

            _outputPath = null;
        }
    }
}
#endif
