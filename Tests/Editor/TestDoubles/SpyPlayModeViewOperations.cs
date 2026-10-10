using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyPlayModeViewOperations : IPlayModeViewOperations
    {
        public string ViewType { get; set; } = PlayModeViewTypes.GameView;
        public int SetViewTypeCallCount { get; private set; }

        public SimulatorDeviceEntry[] Devices { get; set; } = new[]
        {
            new SimulatorDeviceEntry { index = 0, name = "Apple iPhone 13", screenWidth = 1170, screenHeight = 2532 },
            new SimulatorDeviceEntry { index = 1, name = "Google Pixel 5", screenWidth = 1080, screenHeight = 2340 }
        };

        public int SelectedIndex { get; set; }
        public int Rotation { get; set; }
        public int SetSimulatorDeviceCallCount { get; private set; }
        public int SetSimulatorRotationCallCount { get; private set; }

        public GetScreenSafeAreaResponse SafeArea { get; set; } = new GetScreenSafeAreaResponse
        {
            viewType = PlayModeViewTypes.SimulatorView,
            deviceName = "Apple iPhone 13",
            orientation = "Portrait",
            screenWidth = 1170,
            screenHeight = 2532,
            safeArea = new ScreenRect(0, 102, 1170, 2289),
            cutouts = new[] { new ScreenRect(312, 2442, 546, 90) }
        };

        public int GetScreenSafeAreaCallCount { get; private set; }

        public string GetViewType()
        {
            return ViewType;
        }

        public void SetViewType(string viewType)
        {
            SetViewTypeCallCount++;
            ViewType = viewType;
        }

        public GetSimulatorDeviceListResponse GetSimulatorDeviceList()
        {
            ThrowIfNotSimulatorView();
            return new GetSimulatorDeviceListResponse
            {
                devices = Devices,
                selectedIndex = SelectedIndex,
                rotation = Rotation
            };
        }

        public void SetSimulatorDevice(int index)
        {
            ThrowIfNotSimulatorView();
            SetSimulatorDeviceCallCount++;
            if (index < 0 || index >= Devices.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Index {index} is out of range. Valid range: 0 to {Devices.Length - 1}.");
            }

            SelectedIndex = index;
        }

        public void SetSimulatorRotation(int rotation)
        {
            ThrowIfNotSimulatorView();
            SetSimulatorRotationCallCount++;
            Rotation = rotation;
        }

        public (string deviceName, int rotation) GetSimulatorDevice()
        {
            ThrowIfNotSimulatorView();
            return (Devices[SelectedIndex].name, Rotation);
        }

        public GetScreenSafeAreaResponse GetScreenSafeArea()
        {
            GetScreenSafeAreaCallCount++;
            return SafeArea;
        }

        private void ThrowIfNotSimulatorView()
        {
            if (ViewType != PlayModeViewTypes.SimulatorView)
            {
                throw new InvalidOperationException(
                    "The Play Mode window is not in the Simulator view. Switch it to the Simulator view first.");
            }
        }
    }
}
