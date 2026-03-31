#if UNICORTEX_RECORDER
using System;
using System.IO;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
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
        private string _activeOutputPath;

        // Persisted settings (survive across recordings)
        private string _outputPath = "";
        private string _source = "GameView";
        private string _cameraSource = "";
        private string _cameraTag = "";
        private bool _captureUI;
        private int _outputWidth;
        private int _outputHeight;
        private string _outputFormat = "MP4";

        public RecordingOperationsAdapter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void ConfigureRecorder(string outputPath, string source, string cameraSource,
            string cameraTag, bool captureUI, int outputWidth, int outputHeight, string outputFormat)
        {
            _outputPath = outputPath ?? "";
            _source = string.IsNullOrEmpty(source) ? "GameView" : source;
            _cameraSource = cameraSource ?? "";
            _cameraTag = cameraTag ?? "";
            _captureUI = captureUI;
            _outputWidth = outputWidth;
            _outputHeight = outputHeight;
            _outputFormat = string.IsNullOrEmpty(outputFormat) ? "MP4" : outputFormat;
        }

        public GetRecorderSettingsResponse GetRecorderSettings()
        {
            return new GetRecorderSettingsResponse(
                _outputPath, _source, _cameraSource, _cameraTag,
                _captureUI, _outputWidth, _outputHeight, _outputFormat);
        }

        public void StartRecording(int fps, string frameRatePlayback, string recordMode,
            float startTime, float endTime, int startFrame, int endFrame, int frameNumber)
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

            var resolvedOutputPath = string.IsNullOrEmpty(_outputPath)
                ? Path.Combine(Path.GetTempPath(),
                    $"UniCortex_Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4")
                : _outputPath;
            _activeOutputPath = resolvedOutputPath;

            _controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _controllerSettings.FrameRate = fps;
            _controllerSettings.FrameRatePlayback =
                string.Equals(frameRatePlayback, "Variable", StringComparison.OrdinalIgnoreCase)
                    ? FrameRatePlayback.Variable
                    : FrameRatePlayback.Constant;
            _controllerSettings.CapFrameRate = true;

            ApplyRecordMode(recordMode, startTime, endTime, startFrame, endFrame, frameNumber);

            _recorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            _recorderSettings.Enabled = true;
            _recorderSettings.OutputFile = Path.ChangeExtension(resolvedOutputPath, null);

            ApplyOutputFormat();
            ApplyImageInputSettings();

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
            var outputPath = _activeOutputPath;
            CleanupRecordingState();
            return outputPath;
        }

        private void ApplyRecordMode(string recordMode,
            float startTime, float endTime, int startFrame, int endFrame, int frameNumber)
        {
            if (string.Equals(recordMode, "SingleFrame", StringComparison.OrdinalIgnoreCase))
            {
                _controllerSettings.SetRecordModeToSingleFrame(frameNumber);
            }
            else if (string.Equals(recordMode, "FrameInterval", StringComparison.OrdinalIgnoreCase))
            {
                _controllerSettings.SetRecordModeToFrameInterval(startFrame, endFrame);
            }
            else if (string.Equals(recordMode, "TimeInterval", StringComparison.OrdinalIgnoreCase))
            {
                _controllerSettings.SetRecordModeToTimeInterval(startTime, endTime);
            }
            else
            {
                _controllerSettings.SetRecordModeToManual();
            }
        }

        private void ApplyOutputFormat()
        {
            if (string.Equals(_outputFormat, "WebM", StringComparison.OrdinalIgnoreCase))
            {
                _recorderSettings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.WebM;
            }
            else
            {
                _recorderSettings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
            }
        }

        private void ApplyImageInputSettings()
        {
            ImageInputSettings inputSettings;

            if (string.Equals(_source, "Camera", StringComparison.OrdinalIgnoreCase))
            {
                var cameraInput = new CameraInputSettings();
                if (string.Equals(_cameraSource, "TaggedCamera", StringComparison.OrdinalIgnoreCase))
                {
                    cameraInput.Source = ImageSource.TaggedCamera;
                    cameraInput.CameraTag = _cameraTag;
                }
                else if (string.Equals(_cameraSource, "MainCamera", StringComparison.OrdinalIgnoreCase))
                {
                    cameraInput.Source = ImageSource.MainCamera;
                }
                else
                {
                    cameraInput.Source = ImageSource.ActiveCamera;
                }

                cameraInput.CaptureUI = _captureUI;
                inputSettings = cameraInput;
            }
            else
            {
                inputSettings = new GameViewInputSettings();
            }

            if (_outputWidth > 0)
            {
                inputSettings.OutputWidth = _outputWidth;
            }

            if (_outputHeight > 0)
            {
                inputSettings.OutputHeight = _outputHeight;
            }

            // H.264 (MP4) requires even width and height.
            if (string.Equals(_outputFormat, "MP4", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrEmpty(_outputFormat))
            {
                inputSettings.OutputWidth = inputSettings.OutputWidth & ~1;
                inputSettings.OutputHeight = inputSettings.OutputHeight & ~1;
            }

            _recorderSettings.ImageInputSettings = inputSettings;
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

            if (_recorderSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(_recorderSettings);
                _recorderSettings = null;
            }

            _activeOutputPath = null;
        }
    }
}
#endif
