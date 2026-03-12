namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IInputSystemSimulationOperations
    {
        void SendKeyEvent(string key, string eventType);
        void SendMouseEvent(float x, float y, int button, string eventType);
    }
}
