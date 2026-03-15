using System;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Input System package (com.unity.inputsystem) is not installed.
    internal sealed class InputNotSupportedAdapter : IInputOperations
    {
        public void SendKeyEvent(string key, string eventType)
        {
            throw new NotSupportedException(
                "Input System package (com.unity.inputsystem) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void SendMouseEvent(float x, float y, string button, string eventType)
        {
            throw new NotSupportedException(
                "Input System package (com.unity.inputsystem) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
    }
}
