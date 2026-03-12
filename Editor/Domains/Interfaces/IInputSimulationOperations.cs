namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IInputSimulationOperations
    {
        void SendKeyEvent(string keyName, string eventType);
        void SendMouseEvent(float x, float y, int button, string eventType);
    }
}
