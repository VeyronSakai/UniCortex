using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    // Accesses the main Play Mode view (the Game view or the Simulator view) through internal Unity APIs.
    internal static class PlayModeViewUtility
    {
        public static readonly Type PlayModeViewType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PlayModeView");

        private const string SimulatorWindowTypeName = "SimulatorWindow";

        private const BindingFlags AllInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly MethodInfo s_getMainPlayModeView =
            PlayModeViewType?.GetMethod("GetMainPlayModeView", BindingFlags.NonPublic | BindingFlags.Static);

        // Returns the Play Mode view that the game renders to, or null when none is open.
        public static EditorWindow GetMainPlayModeView()
        {
            if (s_getMainPlayModeView == null)
            {
                throw new InvalidOperationException(
                    "PlayModeView internals not found. This Unity version may not be supported.");
            }

            return s_getMainPlayModeView.Invoke(null, null) as EditorWindow;
        }

        // Returns the DeviceSimulatorMain of the Simulator view, which holds the simulated device and screen.
        public static object GetSimulatorMain(out EditorWindow window)
        {
            window = GetMainPlayModeView();
            if (window == null || window.GetType().Name != SimulatorWindowTypeName)
            {
                throw new InvalidOperationException(
                    "The Play Mode window is not in the Simulator view. Switch it to the Simulator view first.");
            }

            return GetMemberValue(window, "main")
                   ?? throw new InvalidOperationException("The Simulator view is not initialized yet.");
        }

        public static string GetCurrentDeviceName(object main)
        {
            var deviceInfo = GetMemberValue(GetMemberValue(main, "currentDevice"), "deviceInfo");
            return (string)GetMemberValue(deviceInfo, "friendlyName");
        }

        public static int GetRotation(object main)
        {
            return (int)GetMemberValue(GetMemberValue(main, "userInterface"), "Rotation");
        }

        // Returns the simulated screen (ScreenSimulation) after applying pending changes. The Simulator view
        // applies orientation and resolution changes only when it repaints, so apply them now.
        public static object GetScreenSimulation(object main)
        {
            var screenSimulation = GetMemberValue(main, "ScreenSimulation");
            var applyChanges = screenSimulation.GetType().GetMethod("ApplyChanges", AllInstance, null,
                Type.EmptyTypes, null) ?? throw CreateMemberNotFoundException(screenSimulation.GetType(),
                "ApplyChanges");
            applyChanges.Invoke(screenSimulation, null);
            return screenSimulation;
        }

        // Returns what the Simulator view draws around the game image: the device frame and its layout.
        public static DeviceFrame GetDeviceFrame()
        {
            var main = GetSimulatorMain(out _);
            var device = GetMemberValue(main, "currentDevice");
            var screenIndex = (int)GetMemberValue(main, "m_ScreenIndex");
            var screen = GetMemberValue(main, "currentScreen");
            var presentation = GetMemberValue(screen, "presentation");
            var screenSimulation = GetScreenSimulation(main);

            // DeviceLoader.LoadOverlay is what the Simulator view uses to load the frame image of the device.
            var deviceLoaderType = device.GetType().Assembly.GetType("UnityEditor.DeviceSimulation.DeviceLoader")
                                   ?? throw CreateMemberNotFoundException(device.GetType(), "DeviceLoader");
            var loadOverlay = deviceLoaderType.GetMethod("LoadOverlay", BindingFlags.Public | BindingFlags.NonPublic |
                                                                         BindingFlags.Static)
                              ?? throw CreateMemberNotFoundException(deviceLoaderType, "LoadOverlay");

            return new DeviceFrame(
                (Texture)loadOverlay.Invoke(null, new[] { device, screenIndex }),
                (Vector4)GetMemberValue(presentation, "borderSize"),
                (int)GetMemberValue(screen, "width"),
                (int)GetMemberValue(screen, "height"),
                (Vector4)GetMemberValue(screenSimulation, "Insets"),
                (ScreenOrientation)GetMemberValue(screenSimulation, "orientation"),
                GetRotation(main));
        }

        public static object GetMemberValue(object target, string name)
        {
            var type = target.GetType();
            var property = type.GetProperty(name, AllInstance);
            if (property != null)
            {
                return property.GetValue(target);
            }

            var field = type.GetField(name, AllInstance);
            if (field != null)
            {
                return field.GetValue(target);
            }

            throw CreateMemberNotFoundException(type, name);
        }

        public static void SetMemberValue(object target, string name, object value)
        {
            var type = target.GetType();

            // PropertyInfo.SetValue also invokes non-public setters.
            var property = type.GetProperty(name, AllInstance);
            if (property == null || property.GetSetMethod(true) == null)
            {
                throw CreateMemberNotFoundException(type, name);
            }

            property.SetValue(target, value);
        }

        private static InvalidOperationException CreateMemberNotFoundException(Type type, string name)
        {
            return new InvalidOperationException(
                $"Unity internal API changed: member '{name}' was not found on {type.Name}. " +
                "This Unity version may not be supported.");
        }
    }
}
