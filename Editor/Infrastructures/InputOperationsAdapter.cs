#if UNICORTEX_INPUT_SYSTEM
using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using MouseButtonConst = UniCortex.Editor.Domains.Models.MouseButton;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class InputOperationsAdapter : IInputOperations
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

            var isRelease = string.Equals(eventType, InputEventType.Release, StringComparison.OrdinalIgnoreCase);
            InputState.Change(keyboard[keyEnum], isRelease ? 0f : 1f);
        }

        public void SendMouseEvent(float x, float y, string button, string eventType)
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

            var isMove = string.Equals(eventType, InputEventType.Move, StringComparison.OrdinalIgnoreCase);
            if (!isMove)
            {
                ButtonControl buttonControl;
                if (string.Equals(button, MouseButtonConst.Right, StringComparison.OrdinalIgnoreCase))
                    buttonControl = mouse.rightButton;
                else if (string.Equals(button, MouseButtonConst.Middle, StringComparison.OrdinalIgnoreCase))
                    buttonControl = mouse.middleButton;
                else
                    buttonControl = mouse.leftButton;

                var isRelease = string.Equals(eventType, InputEventType.Release, StringComparison.OrdinalIgnoreCase);
                InputState.Change(buttonControl, isRelease ? 0f : 1f);
            }
        }
    }
}
#endif
