using System;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback adapter used when the Input System package (com.unity.inputsystem) is not installed.
    // When the package is present, UniCortex.Editor.InputSystem assembly provides the real adapter
    // which is loaded via reflection in EntryPoint.
    internal sealed class InputSystemNotSupportedAdapter : IInputSystemSimulationOperations
    {
        public void SendKeyEvent(string key, string eventType)
        {
            throw new NotSupportedException(
                "Input System package (com.unity.inputsystem) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void SendMouseEvent(float x, float y, int button, string eventType)
        {
            throw new NotSupportedException(
                "Input System package (com.unity.inputsystem) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
    }
}
