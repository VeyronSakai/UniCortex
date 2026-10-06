using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyInputOperations : IInputOperations
    {
        public int SendKeyEventCallCount { get; private set; }
        public string LastKey { get; private set; }
        public string LastKeyEventType { get; private set; }

        public enum MouseAction
        {
            Press,
            Release,
            Move,
        }

        public readonly struct MouseEventRecord
        {
            public readonly MouseAction Action;
            public readonly float X;
            public readonly float Y;

            // null for Move.
            public readonly string Button;

            public MouseEventRecord(MouseAction action, float x, float y, string button)
            {
                Action = action;
                X = x;
                Y = y;
                Button = button;
            }
        }

        public List<MouseEventRecord> MouseEventHistory { get; } = new();

        public void SendKeyEvent(string key, string eventType)
        {
            SendKeyEventCallCount++;
            LastKey = key;
            LastKeyEventType = eventType;
        }

        public void PressMouseButton(float x, float y, string button)
        {
            MouseEventHistory.Add(new MouseEventRecord(MouseAction.Press, x, y, button));
        }

        public void ReleaseMouseButton(float x, float y, string button)
        {
            MouseEventHistory.Add(new MouseEventRecord(MouseAction.Release, x, y, button));
        }

        public void MoveMouse(float x, float y)
        {
            MouseEventHistory.Add(new MouseEventRecord(MouseAction.Move, x, y, null));
        }
    }
}
