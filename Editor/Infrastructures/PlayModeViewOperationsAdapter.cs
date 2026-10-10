using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class PlayModeViewOperationsAdapter : IPlayModeViewOperations
    {
        public string GetViewType()
        {
            return PlayModeWindow.GetViewType() == PlayModeWindow.PlayModeViewTypes.SimulatorView
                ? PlayModeViewTypes.SimulatorView
                : PlayModeViewTypes.GameView;
        }

        public void SetViewType(string viewType)
        {
            PlayModeWindow.SetViewType(viewType == PlayModeViewTypes.SimulatorView
                ? PlayModeWindow.PlayModeViewTypes.SimulatorView
                : PlayModeWindow.PlayModeViewTypes.GameView);
        }

        public GetSimulatorDeviceListResponse GetSimulatorDeviceList()
        {
            var main = PlayModeViewUtility.GetSimulatorMain(out _);
            var devices = (Array)PlayModeViewUtility.GetMemberValue(main, "devices");

            var entries = new SimulatorDeviceEntry[devices.Length];
            for (var i = 0; i < devices.Length; i++)
            {
                var deviceInfo = PlayModeViewUtility.GetMemberValue(devices.GetValue(i), "deviceInfo");
                var screens = (Array)PlayModeViewUtility.GetMemberValue(deviceInfo, "screens");
                var screen = screens.Length > 0 ? screens.GetValue(0) : null;
                entries[i] = new SimulatorDeviceEntry
                {
                    index = i,
                    name = (string)PlayModeViewUtility.GetMemberValue(deviceInfo, "friendlyName"),
                    screenWidth = screen != null ? (int)PlayModeViewUtility.GetMemberValue(screen, "width") : 0,
                    screenHeight = screen != null ? (int)PlayModeViewUtility.GetMemberValue(screen, "height") : 0
                };
            }

            return new GetSimulatorDeviceListResponse
            {
                devices = entries,
                selectedIndex = (int)PlayModeViewUtility.GetMemberValue(main, "deviceIndex"),
                rotation = PlayModeViewUtility.GetRotation(main)
            };
        }

        public void SetSimulatorDevice(int index)
        {
            var main = PlayModeViewUtility.GetSimulatorMain(out var window);
            var count = ((Array)PlayModeViewUtility.GetMemberValue(main, "devices")).Length;
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Index {index} is out of range. Valid range: 0 to {count - 1}.");
            }

            // Setting deviceIndex restarts the simulation with the selected device, as the device list popup does.
            if ((int)PlayModeViewUtility.GetMemberValue(main, "deviceIndex") != index)
            {
                PlayModeViewUtility.SetMemberValue(main, "deviceIndex", index);
            }

            window.Repaint();
        }

        public void SetSimulatorRotation(int rotation)
        {
            var main = PlayModeViewUtility.GetSimulatorMain(out var window);

            // UserInterfaceController.Rotation is what the rotate buttons of the Simulator toolbar change.
            // Its setter also rotates the simulated screen and updates the device view.
            var userInterface = PlayModeViewUtility.GetMemberValue(main, "userInterface");
            PlayModeViewUtility.SetMemberValue(userInterface, "Rotation", rotation);

            window.Repaint();
        }

        public (string deviceName, int rotation) GetSimulatorDevice()
        {
            var main = PlayModeViewUtility.GetSimulatorMain(out _);
            return (PlayModeViewUtility.GetCurrentDeviceName(main), PlayModeViewUtility.GetRotation(main));
        }

        public GetScreenSafeAreaResponse GetScreenSafeArea()
        {
            if (GetViewType() != PlayModeViewTypes.SimulatorView)
            {
                // The Game view does not simulate a safe area, so it is the whole screen.
                var size = Handles.GetMainGameViewSize();
                var width = (int)size.x;
                var height = (int)size.y;
                return new GetScreenSafeAreaResponse
                {
                    viewType = PlayModeViewTypes.GameView,
                    screenWidth = width,
                    screenHeight = height,
                    safeArea = new ScreenRect(0, 0, width, height)
                };
            }

            var main = PlayModeViewUtility.GetSimulatorMain(out _);
            var screenSimulation = PlayModeViewUtility.GetScreenSimulation(main);

            var safeArea = (Rect)PlayModeViewUtility.GetMemberValue(screenSimulation, "safeArea");
            var cutouts = (Rect[])PlayModeViewUtility.GetMemberValue(screenSimulation, "cutouts") ??
                          Array.Empty<Rect>();
            var cutoutRects = new ScreenRect[cutouts.Length];
            for (var i = 0; i < cutouts.Length; i++)
            {
                cutoutRects[i] = ToScreenRect(cutouts[i]);
            }

            return new GetScreenSafeAreaResponse
            {
                viewType = PlayModeViewTypes.SimulatorView,
                deviceName = PlayModeViewUtility.GetCurrentDeviceName(main),
                orientation = PlayModeViewUtility.GetMemberValue(screenSimulation, "orientation").ToString(),
                screenWidth = (int)PlayModeViewUtility.GetMemberValue(screenSimulation, "width"),
                screenHeight = (int)PlayModeViewUtility.GetMemberValue(screenSimulation, "height"),
                safeArea = ToScreenRect(safeArea),
                cutouts = cutoutRects
            };
        }

        private static ScreenRect ToScreenRect(Rect rect)
        {
            return new ScreenRect(rect.x, rect.y, rect.width, rect.height);
        }
    }
}
