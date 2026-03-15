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
        static InputOperationsAdapter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                ConfigureInputSettingsForSimulation();
            }
        }

        /// <summary>
        /// Configures Input System settings so that queued events are routed to the
        /// player update context regardless of Game View focus.
        ///
        /// Without this, the default PointersAndKeyboardsRespectGameViewFocus setting
        /// causes keyboard/mouse events to be processed only in editor updates when
        /// Game View is unfocused, making wasPressedThisFrame invisible to
        /// MonoBehaviour.Update().
        ///
        /// Called once on EnteredPlayMode so that the settings are already active
        /// before any input events are queued. Changing the settings and queuing
        /// events in the same call is unreliable because InputSystem.settings.OnChange()
        /// triggers ApplySettings() which may not fully propagate within the same
        /// editor update tick.
        ///
        /// Changes revert automatically when Play Mode ends because Unity restores
        /// the original InputSettings asset and runtime Application settings.
        /// </summary>
        private static void ConfigureInputSettingsForSimulation()
        {
            var settings = InputSystem.settings;

            // Route all device input to the game view regardless of focus.
            settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

            // Prevent Input System from disabling devices when the application loses focus.
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;

            // Prevent the event buffer from being flushed when the game is unfocused.
            Application.runInBackground = true;
        }

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
            var keyControl = keyboard[keyEnum];

            // Queue the opposite state first to guarantee a state transition.
            // Without this, consecutive identical events (e.g. press → press) would not
            // be recognized as a new press by Input System's wasPressedThisFrame.
            using (StateEvent.From(keyboard, out var resetPtr))
            {
                keyControl.WriteValueIntoEvent(isRelease ? 1f : 0f, resetPtr);
                InputSystem.QueueEvent(resetPtr);
            }

            using (StateEvent.From(keyboard, out var eventPtr))
            {
                keyControl.WriteValueIntoEvent(isRelease ? 0f : 1f, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }
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

            var isMove = string.Equals(eventType, InputEventType.Move, StringComparison.OrdinalIgnoreCase);

            if (isMove)
            {
                using (StateEvent.From(mouse, out var eventPtr))
                {
                    mouse.position.WriteValueIntoEvent(new Vector2(x, y), eventPtr);
                    InputSystem.QueueEvent(eventPtr);
                }
            }
            else
            {
                ButtonControl buttonControl;
                if (string.Equals(button, MouseButtonConst.Right, StringComparison.OrdinalIgnoreCase))
                    buttonControl = mouse.rightButton;
                else if (string.Equals(button, MouseButtonConst.Middle, StringComparison.OrdinalIgnoreCase))
                    buttonControl = mouse.middleButton;
                else
                    buttonControl = mouse.leftButton;

                var isRelease =
                    string.Equals(eventType, InputEventType.Release, StringComparison.OrdinalIgnoreCase);

                // Queue the opposite state first to guarantee a state transition.
                using (StateEvent.From(mouse, out var resetPtr))
                {
                    mouse.position.WriteValueIntoEvent(new Vector2(x, y), resetPtr);
                    buttonControl.WriteValueIntoEvent(isRelease ? 1f : 0f, resetPtr);
                    InputSystem.QueueEvent(resetPtr);
                }

                using (StateEvent.From(mouse, out var eventPtr))
                {
                    mouse.position.WriteValueIntoEvent(new Vector2(x, y), eventPtr);
                    buttonControl.WriteValueIntoEvent(isRelease ? 0f : 1f, eventPtr);
                    InputSystem.QueueEvent(eventPtr);
                }
            }
        }
    }
}
#endif
