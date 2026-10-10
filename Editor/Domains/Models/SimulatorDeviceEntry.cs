using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SimulatorDeviceEntry
    {
        public int index;
        public string name = "";

        // Native screen resolution of the device in portrait orientation.
        public int screenWidth;
        public int screenHeight;
    }
}
