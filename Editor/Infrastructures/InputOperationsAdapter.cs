#if UNICORTEX_INPUT_SYSTEM
using System;
using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using InputMouseButton = UnityEngine.InputSystem.LowLevel.MouseButton;
using MouseButtonConst = UniCortex.Editor.Domains.Models.MouseButton;

namespace UniCortex.Editor.Infrastructures
{
    // Sends the simulated input to virtual devices instead of the physical ones, so that events from the physical
    // devices do not overwrite the simulated state (see BlockPhysicalMouse / BlockPhysicalKeyboard).
    internal sealed class InputOperationsAdapter : IInputOperations
    {
        // Names of the virtual devices. The devices are found by name, because the Input System keeps them over a
        // domain reload while this adapter is created again.
        private const string VirtualMouseName = "UniCortexMouse";
        private const string VirtualKeyboardName = "UniCortexKeyboard";

        // Track queued key/button state ourselves instead of calling InputSystem.Update()
        // between events. Forcing InputSystem.Update() from an HTTP handler would process
        // all pending events outside the normal player loop, causing input to be consumed
        // at unpredictable times and potentially breaking frame-dependent logic such as
        // wasPressedThisFrame / wasReleasedThisFrame in MonoBehaviour.Update().
        private readonly HashSet<Key> _pressedKeys = new();
        private readonly HashSet<string> _pressedMouseButtons = new(StringComparer.OrdinalIgnoreCase);

        // Number of operations blocking the physical devices. Changed from any thread with Interlocked.
        // Not kept over a domain reload, so the physical devices cannot stay blocked after one.
        private int _physicalKeyboardBlockCount;
        private int _physicalMouseBlockCount;

        public InputOperationsAdapter()
        {
            InputSystem.onEvent += OnInputEvent;
        }

        // Registered to EditorApplication.playModeStateChanged by EntryPoint.
        internal void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    // Unity resets device state on exit, so clear our tracking to match.
                    _pressedKeys.Clear();
                    _pressedMouseButtons.Clear();
                    RemoveVirtualDevice<Keyboard>(VirtualKeyboardName);
                    RemoveVirtualDevice<Mouse>(VirtualMouseName);
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    _pressedKeys.Clear();
                    _pressedMouseButtons.Clear();
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

        private void SendKeys(string[] keys, bool targetPressed)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Input simulation is only available in Play Mode. Enter Play Mode first.");
            }

            EnsureInputSettingsConfigured();

            var keyboard = GetOrAddVirtualDevice<Keyboard>(VirtualKeyboardName);

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
            var keysInTargetState = keyEnums.FindAll(key => _pressedKeys.Contains(key) == targetPressed);
            if (keysInTargetState.Count > 0)
            {
                InputSystem.QueueStateEvent(keyboard, BuildKeyboardState(keysInTargetState, !targetPressed));
            }

            InputSystem.QueueStateEvent(keyboard, BuildKeyboardState(keyEnums, targetPressed));

            foreach (var key in keyEnums)
            {
                if (targetPressed)
                {
                    _pressedKeys.Add(key);
                }
                else
                {
                    _pressedKeys.Remove(key);
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

        private void SendMouseButton(float x, float y, string button, bool targetPressed)
        {
            var mouse = GetMouse();

            var buttonName = button ?? MouseButtonConst.Left;
            var buttonEnum = ToInputMouseButton(buttonName);

            // Track button state ourselves because isPressed only reflects
            // the last *processed* state, not events still sitting in the queue.
            var alreadyPressed = _pressedMouseButtons.Contains(buttonName);

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

            // UI input modules learn the pointer position from changes of the position control (the "Point"
            // action). An InputSystemUIInputModule enabled while its actions are already enabled by another one
            // (e.g. an EventSystem in an additively loaded scene) does not get the current position from the
            // initial state check either, so a press where the mouse already is hits nothing. Moving away and
            // back within the same update guarantees a position change; both events are processed before the UI
            // module runs, so it only sees the final position.
            if (targetPressed)
            {
                // Nudge toward the inside of the screen: x - 1 is on the screen unless x is at the left edge.
                var nudgeX = x >= 1f ? x - 1f : x + 1f;
                // Keep the button released here so that the press still happens at (x, y).
                var nudgeState = BuildMouseState(nudgeX, y).WithButton(buttonEnum, false);
                InputSystem.QueueStateEvent(mouse, nudgeState);
            }

            var mainState = BuildMouseState(x, y);
            mainState = mainState.WithButton(buttonEnum, targetPressed);
            InputSystem.QueueStateEvent(mouse, mainState);

            if (targetPressed)
            {
                _pressedMouseButtons.Add(buttonName);
            }
            else
            {
                _pressedMouseButtons.Remove(buttonName);
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

            return GetOrAddVirtualDevice<Mouse>(VirtualMouseName);
        }

        public void BlockPhysicalKeyboard()
        {
            Interlocked.Increment(ref _physicalKeyboardBlockCount);
        }

        public void UnblockPhysicalKeyboard()
        {
            Interlocked.Decrement(ref _physicalKeyboardBlockCount);
        }

        public void BlockPhysicalMouse()
        {
            Interlocked.Increment(ref _physicalMouseBlockCount);
        }

        public void UnblockPhysicalMouse()
        {
            Interlocked.Decrement(ref _physicalMouseBlockCount);
        }

        // Drops the state events of the physical devices while they are blocked. The physical devices are not
        // disabled with InputSystem.DisableDevice, because a disabled device would stay disabled when a domain
        // reload happens before it is enabled again.
        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Devices added by code (our virtual devices, and those of the game such as VirtualMouseInput) are
            // not native.
            if (device == null || !device.native)
            {
                return;
            }

            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            {
                return;
            }

            // Pen and Touchscreen are dropped too: uGUI treats them as the same pointer as the mouse by default
            // (UIPointerBehavior.SingleMouseOrPenButMultiTouchAndTrack).
            if (device is Pointer && Volatile.Read(ref _physicalMouseBlockCount) > 0
                || device is Keyboard && Volatile.Read(ref _physicalKeyboardBlockCount) > 0)
            {
                eventPtr.handled = true;
            }
        }

        // Returns the virtual device, adding it when it does not exist yet, and makes it current so that code
        // reading Mouse.current / Keyboard.current sees the simulated state.
        private static TDevice GetOrAddVirtualDevice<TDevice>(string name) where TDevice : InputDevice
        {
            var device = FindVirtualDevice<TDevice>(name) ?? InputSystem.AddDevice<TDevice>(name);
            device.MakeCurrent();
            return device;
        }

        private static void RemoveVirtualDevice<TDevice>(string name) where TDevice : InputDevice
        {
            var device = FindVirtualDevice<TDevice>(name);
            if (device != null)
            {
                InputSystem.RemoveDevice(device);
            }
        }

        private static TDevice FindVirtualDevice<TDevice>(string name) where TDevice : InputDevice
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is TDevice typed && !device.native && device.name == name)
                {
                    return typed;
                }
            }

            return null;
        }

        /// <summary>
        /// Builds a MouseState from scratch with the given position and all tracked
        /// button states.
        /// </summary>
        private MouseState BuildMouseState(float x, float y)
        {
            var state = new MouseState { position = new Vector2(x, y) };
            foreach (var btn in _pressedMouseButtons)
                state = state.WithButton(ToInputMouseButton(btn));
            return state;
        }

        /// <summary>
        /// Builds a KeyboardState from scratch with all tracked key states.
        /// The target keys are set to the specified state, overriding any tracked value.
        /// </summary>
        private KeyboardState BuildKeyboardState(List<Key> targetKeys, bool targetPressed)
        {
            var state = new KeyboardState();
            foreach (var key in _pressedKeys)
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
