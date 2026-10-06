namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IInputOperations
    {
        void SendKeyEvent(string key, string eventType);
        void PressMouseButton(float x, float y, string button);
        void ReleaseMouseButton(float x, float y, string button);
        void MoveMouse(float x, float y);
    }
}
