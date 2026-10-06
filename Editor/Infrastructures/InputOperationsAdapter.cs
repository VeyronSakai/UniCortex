#if UNICORTEX_INPUT_SYSTEM
using System;
using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using InputMouseButton = UnityEngine.InputSystem.LowLevel.MouseButton;
using MouseButtonConst = UniCortex.Editor.Domains.Models.MouseButton;

namespace UniCortex.Editor.Infrastructures
{
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

        public void PressKeys(string[] keys)
        {
            SendKeys(keys, true);
        }

        public void ReleaseKeys(string[] keys)
        {
            SendKeys(keys, false);
        }

        private static void SendKeys(string[] keys, bool targetPressed)
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

            // Parse all the keys first, so that nothing is sent when one of them is invalid.
            var keyEnums = new List<Key>(keys.Length);
            foreach (var key in keys)
            {
                if (!Enum.TryParse<Key>(key, true, out var keyEnum) || keyEnum == Key.None)
                {
                    throw new ArgumentException(
                        $"Invalid key name: {key}. Use Input System Key enum names (e.g. Space, A, LeftArrow, Enter).");
                }

                keyEnums.Add(keyEnum);
            }

            // Track key state ourselves because isPressed only reflects
            // the last *processed* state, not events still sitting in the queue.
            // wasPressedThisFrame / wasReleasedThisFrame detect a *change* from the
            // previous frame. If a key is already in the target state, queuing the
            // same state again would be a no-op. To guarantee a detectable transition
            // we first queue the opposite state for such keys, then queue the desired state.
            var keysInTargetState = keyEnums.FindAll(key => s_pressedKeys.Contains(key) == targetPressed);
            if (keysInTargetState.Count > 0)
            {
                InputSystem.QueueStateEvent(keyboard, BuildKeyboardState(keysInTargetState, !targetPressed));
            }

            InputSystem.QueueStateEvent(keyboard, BuildKeyboardState(keyEnums, targetPressed));

            foreach (var key in keyEnums)
            {
                if (targetPressed)
                {
                    s_pressedKeys.Add(key);
                }
                else
                {
                    s_pressedKeys.Remove(key);
                }
            }
        }

        public void PressMouseButton(float x, float y, string button)
        {
            SendMouseButton(x, y, button, true);
        }

        public void ReleaseMouseButton(float x, float y, string button)
        {
            SendMouseButton(x, y, button, false);
        }

        public void MoveMouse(float x, float y)
        {
            var mouse = GetMouse();
            InputSystem.QueueStateEvent(mouse, BuildMouseState(x, y));
        }

        private static void SendMouseButton(float x, float y, string button, bool targetPressed)
        {
            var mouse = GetMouse();

            var buttonName = button ?? MouseButtonConst.Left;
            var buttonEnum = ToInputMouseButton(buttonName);

            // Track button state ourselves because isPressed only reflects
            // the last *processed* state, not events still sitting in the queue.
            var alreadyPressed = s_pressedMouseButtons.Contains(buttonName);

            // wasPressedThisFrame / wasReleasedThisFrame detect a *change* from the
            // previous frame. If the button is already in the target state (e.g.
            // pressing an already-pressed button), queuing the same state again would
            // be a no-op. To guarantee a detectable transition we first queue the
            // opposite state, then queue the desired state — producing a
            // released→pressed (or pressed→released) edge within one update.
            if (targetPressed && alreadyPressed
                || !targetPressed && !alreadyPressed)
            {
                var resetState = BuildMouseState(x, y);
                resetState = resetState.WithButton(buttonEnum, !targetPressed);
                InputSystem.QueueStateEvent(mouse, resetState);
            }

            var mainState = BuildMouseState(x, y);
            mainState = mainState.WithButton(buttonEnum, targetPressed);
            InputSystem.QueueStateEvent(mouse, mainState);

            if (targetPressed)
            {
                s_pressedMouseButtons.Add(buttonName);
            }
            else
            {
                s_pressedMouseButtons.Remove(buttonName);
            }
        }

        private static Mouse GetMouse()
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

            return mouse;
        }

        /// <summary>
        /// Builds a MouseState from scratch with the given position and all tracked
        /// button states. Because the state is constructed rather than copied from the
        /// physical device, no physical mouse state can leak into simulated events.
        /// </summary>
        private static MouseState BuildMouseState(float x, float y)
        {
            var state = new MouseState { position = new Vector2(x, y) };
            foreach (var btn in s_pressedMouseButtons)
                state = state.WithButton(ToInputMouseButton(btn));
            return state;
        }

        /// <summary>
        /// Builds a KeyboardState from scratch with all tracked key states.
        /// The target keys are set to the specified state, overriding any tracked value.
        /// </summary>
        private static KeyboardState BuildKeyboardState(List<Key> targetKeys, bool targetPressed)
        {
            var state = new KeyboardState();
            foreach (var key in s_pressedKeys)
                state.Set(key, true);
            foreach (var key in targetKeys)
                state.Set(key, targetPressed);
            return state;
        }

        private static InputMouseButton ToInputMouseButton(string button)
        {
            if (string.Equals(button, MouseButtonConst.Right, StringComparison.OrdinalIgnoreCase))
                return InputMouseButton.Right;
            if (string.Equals(button, MouseButtonConst.Middle, StringComparison.OrdinalIgnoreCase))
                return InputMouseButton.Middle;
            return InputMouseButton.Left;
        }
    }
}
#endif
