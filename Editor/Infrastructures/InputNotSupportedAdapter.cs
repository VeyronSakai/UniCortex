using System;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Input System package (com.unity.inputsystem) is not installed.
    internal sealed class InputNotSupportedAdapter : IInputOperations
    {
        public void SendKeyEvent(string key, string eventType) => throw CreateException();

        public void PressMouseButton(float x, float y, string button) => throw CreateException();

        public void ReleaseMouseButton(float x, float y, string button) => throw CreateException();

        public void MoveMouse(float x, float y) => throw CreateException();

        private static NotSupportedException CreateException()
        {
            return new NotSupportedException(
                "Input System package (com.unity.inputsystem) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
    }
}
