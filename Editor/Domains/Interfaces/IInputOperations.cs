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
    }
}
