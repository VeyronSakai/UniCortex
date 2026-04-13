using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ProfilerOperationsAdapter : IProfilerOperations
    {
        public void FocusProfilerWindow()
        {
            var window = EditorWindow.GetWindow<ProfilerWindow>();
            window.Show();
            window.Focus();
        }

        public GetProfilerStatusResponse GetStatus()
        {
            var window = GetProfilerWindow();
            return new GetProfilerStatusResponse(
                isWindowOpen: window != null,
                hasFocus: window != null && window.hasFocus,
                isRecording: ProfilerDriver.enabled,
                profileEditor: ProfilerDriver.profileEditor);
        }

        public void StartRecording(bool profileEditor)
        {
            ProfilerDriver.profileEditor = profileEditor;
            ProfilerDriver.enabled = true;
        }

        public void StopRecording()
        {
            ProfilerDriver.enabled = false;
        }

        private static ProfilerWindow GetProfilerWindow()
        {
            return Resources.FindObjectsOfTypeAll<ProfilerWindow>().FirstOrDefault();
        }
    }
}
