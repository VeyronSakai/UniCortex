using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyInputOperations : IInputOperations
    {
        public enum KeyAction
        {
            Press,
            Release,
        }

        public readonly struct KeyEventRecord
        {
            public readonly KeyAction Action;
            public readonly string[] Keys;

            public KeyEventRecord(KeyAction action, string[] keys)
            {
                Action = action;
                Keys = keys;
            }
        }

        public List<KeyEventRecord> KeyEventHistory { get; } = new();

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

        public void PressKeys(string[] keys)
        {
            KeyEventHistory.Add(new KeyEventRecord(KeyAction.Press, keys));
        }

        public void ReleaseKeys(string[] keys)
        {
            KeyEventHistory.Add(new KeyEventRecord(KeyAction.Release, keys));
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
