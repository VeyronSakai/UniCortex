using System;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Input System package (com.unity.inputsystem) is not installed.
    internal sealed class InputNotSupportedAdapter : IInputOperations
    {
        public void PressKeys(string[] keys) => throw CreateException();

        public void ReleaseKeys(string[] keys) => throw CreateException();

        public void TypeText(string text) => throw CreateException();

        public void PressMouseButton(float x, float y, string button) => throw CreateException();

        public void ReleaseMouseButton(float x, float y, string button) => throw CreateException();

        public void MoveMouse(float x, float y) => throw CreateException();

        public void BlockPhysicalKeyboard() => throw CreateException();

        // Does nothing so that it can be called after a failed operation.
        public void UnblockPhysicalKeyboard()
        {
        }

        public void BlockPhysicalMouse() => throw CreateException();

        // Does nothing so that it can be called after a failed operation.
        public void UnblockPhysicalMouse()
        {
        }

        private static NotSupportedException CreateException()
        {
            return new NotSupportedException(
                "Input System package (com.unity.inputsystem) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
    }
}
