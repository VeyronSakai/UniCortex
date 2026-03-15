namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IInputOperations
    {
        void SendKeyEvent(string key, string eventType);
        void SendMouseEvent(float x, float y, string button, string eventType);
    }
}
