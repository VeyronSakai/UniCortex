using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    // Operations on the Play Mode window, which shows either the Game view or the Simulator view.
    internal interface IPlayModeViewOperations
    {
        // Returns one of PlayModeViewTypes.
        string GetViewType();

        // viewType is one of PlayModeViewTypes.
        void SetViewType(string viewType);

        GetSimulatorDeviceListResponse GetSimulatorDeviceList();
        void SetSimulatorDevice(int index);

        // rotation is the clockwise rotation of the device in degrees (0, 90, 180 or 270).
        void SetSimulatorRotation(int rotation);

        // Returns the selected device name and its rotation.
        (string deviceName, int rotation) GetSimulatorDevice();

        GetScreenSafeAreaResponse GetScreenSafeArea();
    }
}
