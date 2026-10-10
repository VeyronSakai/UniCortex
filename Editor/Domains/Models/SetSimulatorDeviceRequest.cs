using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetSimulatorDeviceRequest
    {
        // Index of the device from GET /game-view/simulator/devices. -1 keeps the current device.
        public int index = -1;

        // Clockwise rotation of the device in degrees (0, 90, 180 or 270). -1 keeps the current rotation.
        public int rotation = -1;
    }
}
