#if UNICORTEX_INPUT_SYSTEM
using System;
using System.Collections.Generic;
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
    internal enum InputAction
    {
        Press,
        Release,
        Move,
    }

    internal sealed class InputOperationsAdapter : IInputOperations
    {
        // Track queued key/button state ourselves instead of calling InputSystem.Update()
        // between events. Forcing InputSystem.Update() from an HTTP handler would process
        // all pending events outside the normal player loop, causing input to be consumed
        // at unpredictable times and potentially breaking frame-dependent logic such as
        // wasPressedThisFrame / wasReleasedThisFrame in MonoBehaviour.Update().
        private static readonly HashSet<Key> s_pressedKeys = new();
        private static readonly HashSet<string> s_pressedMouseButtons = new(StringComparer.OrdinalIgnoreCase);

        static InputOperationsAdapter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    // Unity resets device state on exit, so clear our tracking to match.
                    s_pressedKeys.Clear();
                    s_pressedMouseButtons.Clear();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    s_pressedKeys.Clear();
                    s_pressedMouseButtons.Clear();
                    ConfigureInputSettingsForSimulation();
                    break;
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
        /// Changes revert automatically when Play Mode ends because Unity restores
        /// the original InputSettings asset and runtime Application settings.
        /// </summary>
        private static void ConfigureInputSettingsForSimulation()
        {
            if (!EditorApplication.isPlaying) return;

            var settings = InputSystem.settings;

            // Only write when the value differs to avoid unnecessary OnChange()/ApplySettings() calls.
            if (settings.editorInputBehaviorInPlayMode !=
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView)
            {
                settings.editorInputBehaviorInPlayMode =
                    InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            }

            if (settings.backgroundBehavior != InputSettings.BackgroundBehavior.IgnoreFocus)
            {
                settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            }

            if (!Application.runInBackground)
            {
                Application.runInBackground = true;
            }

        }

        /// <summary>
        /// Verifies that Input System settings are correctly configured for simulation.
        /// If settings have drifted (e.g. Unity restored defaults after a Play Mode
        /// transition), reapplies them. The check is lightweight (three property reads)
        /// so it adds negligible overhead when settings are already correct.
        /// </summary>
        private static void EnsureInputSettingsConfigured()
        {
            var settings = InputSystem.settings;
            if (settings.editorInputBehaviorInPlayMode !=
                    InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView
                || settings.backgroundBehavior != InputSettings.BackgroundBehavior.IgnoreFocus
                || !Application.runInBackground)
            {
                ConfigureInputSettingsForSimulation();
            }
        }

        public void SendKeyEvent(string key, string eventType)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Input simulation is only available in Play Mode. Enter Play Mode first.");
            }

            EnsureInputSettingsConfigured();

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                throw new InvalidOperationException("No Keyboard device is available.");
            }

            if (!Enum.TryParse<Key>(key, true, out var keyEnum) || keyEnum == Key.None)
            {
                throw new ArgumentException(
                    $"Invalid key name: {key}. Use Input System Key enum names (e.g. Space, A, LeftArrow, Enter).");
            }

            var keyControl = keyboard[keyEnum];
            var action = ParseButtonAction(eventType);
            var targetValue = action == InputAction.Press ? 1f : 0f;

            // Use self-tracked state instead of keyControl.isPressed because
            // isPressed only reflects the last processed state, not queued events.
            // Without tracking, consecutive press events within the same update
            // would skip the reset and fail to trigger wasPressedThisFrame.
            var alreadyPressed = s_pressedKeys.Contains(keyEnum);

            if (action == InputAction.Press && alreadyPressed
                || action == InputAction.Release && !alreadyPressed)
            {
                using (StateEvent.From(keyboard, out var resetPtr))
                {
                    keyControl.WriteValueIntoEvent(1f - targetValue, resetPtr);
                    InputSystem.QueueEvent(resetPtr);
                }
            }

            using (StateEvent.From(keyboard, out var eventPtr))
            {
                keyControl.WriteValueIntoEvent(targetValue, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }

            if (action == InputAction.Press)
            {
                s_pressedKeys.Add(keyEnum);
            }
            else
            {
                s_pressedKeys.Remove(keyEnum);
            }
        }

        public void SendMouseEvent(float x, float y, string button, string eventType)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Input simulation is only available in Play Mode. Enter Play Mode first.");
            }

            EnsureInputSettingsConfigured();

            var mouse = Mouse.current;
            if (mouse == null)
            {
                throw new InvalidOperationException("No Mouse device is available.");
            }

            var action = ParseMouseAction(eventType);
            if (action == InputAction.Move)
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
                {
                    buttonControl = mouse.rightButton;
                }
                else if (string.Equals(button, MouseButtonConst.Middle, StringComparison.OrdinalIgnoreCase))
                {
                    buttonControl = mouse.middleButton;
                }
                else
                {
                    buttonControl = mouse.leftButton;
                }

                var targetValue = action == InputAction.Press ? 1f : 0f;

                // Resolve the button name used as the tracking key.
                var buttonName = button ?? MouseButtonConst.Left;

                // Use self-tracked state instead of buttonControl.isPressed because
                // isPressed only reflects the last processed state, not queued events.
                var alreadyPressed = s_pressedMouseButtons.Contains(buttonName);
                if (action == InputAction.Press && alreadyPressed
                    || action == InputAction.Release && !alreadyPressed)
                {
                    using (StateEvent.From(mouse, out var resetPtr))
                    {
                        mouse.position.WriteValueIntoEvent(new Vector2(x, y), resetPtr);
                        buttonControl.WriteValueIntoEvent(1f - targetValue, resetPtr);
                        InputSystem.QueueEvent(resetPtr);
                    }
                }

                using (StateEvent.From(mouse, out var eventPtr))
                {
                    mouse.position.WriteValueIntoEvent(new Vector2(x, y), eventPtr);
                    buttonControl.WriteValueIntoEvent(targetValue, eventPtr);
                    InputSystem.QueueEvent(eventPtr);
                }

                if (action == InputAction.Press)
                {
                    s_pressedMouseButtons.Add(buttonName);
                }
                else
                {
                    s_pressedMouseButtons.Remove(buttonName);
                }
            }
        }

        private static InputAction ParseButtonAction(string eventType)
        {
            return string.Equals(eventType, InputEventType.Release, StringComparison.OrdinalIgnoreCase)
                ? InputAction.Release
                : InputAction.Press;
        }

        private static InputAction ParseMouseAction(string eventType)
        {
            return string.Equals(eventType, InputEventType.Move, StringComparison.OrdinalIgnoreCase)
                ? InputAction.Move
                : ParseButtonAction(eventType);
        }
    }
}
#endif
