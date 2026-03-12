using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyInputSimulationOperations : IInputSimulationOperations
    {
        public int SendKeyEventCallCount { get; private set; }
        public string LastKeyName { get; private set; }
        public string LastKeyEventType { get; private set; }

        public int SendMouseEventCallCount { get; private set; }
        public float LastMouseX { get; private set; }
        public float LastMouseY { get; private set; }
        public int LastMouseButton { get; private set; }
        public string LastMouseEventType { get; private set; }

        public void SendKeyEvent(string keyName, string eventType)
        {
            SendKeyEventCallCount++;
            LastKeyName = keyName;
            LastKeyEventType = eventType;
        }

        public void SendMouseEvent(float x, float y, int button, string eventType)
        {
            SendMouseEventCallCount++;
            LastMouseX = x;
            LastMouseY = y;
            LastMouseButton = button;
            LastMouseEventType = eventType;
        }
    }
}
