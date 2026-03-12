#if UNICORTEX_INPUT_SYSTEM
using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class InputSystemSimulationOperationsAdapter : IInputSystemSimulationOperations
    {
        public void SendKeyEvent(string key, string eventType)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Input simulation is only available in Play Mode. Enter Play Mode first.");
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                throw new InvalidOperationException("No Keyboard device is available.");
            }

            if (!Enum.TryParse<Key>(key, true, out var keyEnum) || keyEnum == Key.None)
            {
                throw new ArgumentException(
                    $"Invalid key name: {key}. Use Input System Key enum names (e.g. Space, A, LeftArrow, Return).");
            }

            var isRelease = string.Equals(eventType, "release", StringComparison.OrdinalIgnoreCase);
            InputState.Change(keyboard[keyEnum], isRelease ? 0f : 1f);
        }

        public void SendMouseEvent(float x, float y, int button, string eventType)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Input simulation is only available in Play Mode. Enter Play Mode first.");
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                throw new InvalidOperationException("No Mouse device is available.");
            }

            // Always update position
            InputState.Change(mouse.position, new Vector2(x, y));

            var isMove = string.Equals(eventType, "move", StringComparison.OrdinalIgnoreCase);
            if (!isMove)
            {
                ButtonControl buttonControl;
                if (button == 1)
                    buttonControl = mouse.rightButton;
                else if (button == 2)
                    buttonControl = mouse.middleButton;
                else
                    buttonControl = mouse.leftButton;

                var isRelease = string.Equals(eventType, "release", StringComparison.OrdinalIgnoreCase);
                InputState.Change(buttonControl, isRelease ? 0f : 1f);
            }
        }
    }
}
#endif
