using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetSimulatorDeviceListResponse
    {
        public SimulatorDeviceEntry[] devices = Array.Empty<SimulatorDeviceEntry>();
        public int selectedIndex;

        // Clockwise rotation of the simulated device in degrees (0, 90, 180 or 270).
        public int rotation;
    }
}
