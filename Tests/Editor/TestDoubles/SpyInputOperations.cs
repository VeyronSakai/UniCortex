using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyInputOperations : IInputOperations
    {
        public int SendKeyEventCallCount { get; private set; }
        public string LastKey { get; private set; }
        public string LastKeyEventType { get; private set; }

        public int SendMouseEventCallCount { get; private set; }
        public float LastMouseX { get; private set; }
        public float LastMouseY { get; private set; }
        public string LastMouseButton { get; private set; }
        public string LastMouseEventType { get; private set; }

        public readonly struct MouseEventRecord
        {
            public readonly float X;
            public readonly float Y;
            public readonly string Button;
            public readonly string EventType;

            public MouseEventRecord(float x, float y, string button, string eventType)
            {
                X = x;
                Y = y;
                Button = button;
                EventType = eventType;
            }
        }

        public List<MouseEventRecord> MouseEventHistory { get; } = new();

        public void SendKeyEvent(string key, string eventType)
        {
            SendKeyEventCallCount++;
            LastKey = key;
            LastKeyEventType = eventType;
        }

        public void SendMouseEvent(float x, float y, string button, string eventType)
        {
            SendMouseEventCallCount++;
            LastMouseX = x;
            LastMouseY = y;
            LastMouseButton = button;
            LastMouseEventType = eventType;
            MouseEventHistory.Add(new MouseEventRecord(x, y, button, eventType));
        }
    }
}
