using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetSimulatorDeviceResponse
    {
        public string deviceName;
        public int rotation;

        public SetSimulatorDeviceResponse(string deviceName, int rotation)
        {
            this.deviceName = deviceName;
            this.rotation = rotation;
        }
    }
}
