namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IInputOperations
    {
        // Presses all the keys in one event, so they are pressed in the same frame.
        void PressKeys(string[] keys);

        // Releases all the keys in one event, so they are released in the same frame.
        void ReleaseKeys(string[] keys);

        void PressMouseButton(float x, float y, string button);
        void ReleaseMouseButton(float x, float y, string button);
        void MoveMouse(float x, float y);

        // Ignores input from the physical keyboard until UnblockPhysicalKeyboard is called, so that it does not
        // overwrite the simulated keys while they are held. Calls may be nested. Can be called from any thread.
        void BlockPhysicalKeyboard();
        void UnblockPhysicalKeyboard();

        // Ignores input from the physical pointers (mouse, pen and touchscreen) until UnblockPhysicalMouse is
        // called, so that they do not move the simulated pointer during a click or a drag. Calls may be nested.
        // Can be called from any thread.
        void BlockPhysicalMouse();
        void UnblockPhysicalMouse();
    }
}
