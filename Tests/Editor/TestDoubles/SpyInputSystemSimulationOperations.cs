using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyInputSystemSimulationOperations : IInputSystemSimulationOperations
    {
        public int SendKeyEventCallCount { get; private set; }
        public string LastKey { get; private set; }
        public string LastKeyEventType { get; private set; }

        public int SendMouseEventCallCount { get; private set; }
        public float LastMouseX { get; private set; }
        public float LastMouseY { get; private set; }
        public int LastMouseButton { get; private set; }
        public string LastMouseEventType { get; private set; }

        public void SendKeyEvent(string key, string eventType)
        {
            SendKeyEventCallCount++;
            LastKey = key;
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
