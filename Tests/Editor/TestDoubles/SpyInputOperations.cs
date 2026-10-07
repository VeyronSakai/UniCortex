using System;
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

            // Whether the physical keyboard was blocked when the event was sent.
            public readonly bool PhysicalKeyboardBlocked;

            public KeyEventRecord(KeyAction action, string[] keys, bool physicalKeyboardBlocked)
            {
                Action = action;
                Keys = keys;
                PhysicalKeyboardBlocked = physicalKeyboardBlocked;
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

            // Whether the physical mouse was blocked when the event was sent.
            public readonly bool PhysicalMouseBlocked;

            public MouseEventRecord(MouseAction action, float x, float y, string button, bool physicalMouseBlocked)
            {
                Action = action;
                X = x;
                Y = y;
                Button = button;
                PhysicalMouseBlocked = physicalMouseBlocked;
            }
        }

        public List<MouseEventRecord> MouseEventHistory { get; } = new();

        // Number of BlockPhysical* calls not yet followed by UnblockPhysical*.
        public int PhysicalKeyboardBlockCount { get; private set; }
        public int PhysicalMouseBlockCount { get; private set; }

        // When set, ReleaseKeys / ReleaseMouseButton throw it, e.g. to test that the physical devices are unblocked
        // after a failure.
        public Exception ExceptionOnRelease { get; set; }

        public void PressKeys(string[] keys)
        {
            KeyEventHistory.Add(new KeyEventRecord(KeyAction.Press, keys, PhysicalKeyboardBlockCount > 0));
        }

        public void ReleaseKeys(string[] keys)
        {
            if (ExceptionOnRelease != null)
                throw ExceptionOnRelease;

            KeyEventHistory.Add(new KeyEventRecord(KeyAction.Release, keys, PhysicalKeyboardBlockCount > 0));
        }

        public void PressMouseButton(float x, float y, string button)
        {
            MouseEventHistory.Add(new MouseEventRecord(MouseAction.Press, x, y, button, PhysicalMouseBlockCount > 0));
        }

        public void ReleaseMouseButton(float x, float y, string button)
        {
            if (ExceptionOnRelease != null)
                throw ExceptionOnRelease;

            MouseEventHistory.Add(
                new MouseEventRecord(MouseAction.Release, x, y, button, PhysicalMouseBlockCount > 0));
        }

        public void MoveMouse(float x, float y)
        {
            MouseEventHistory.Add(new MouseEventRecord(MouseAction.Move, x, y, null, PhysicalMouseBlockCount > 0));
        }

        public void BlockPhysicalKeyboard()
        {
            PhysicalKeyboardBlockCount++;
        }

        public void UnblockPhysicalKeyboard()
        {
            PhysicalKeyboardBlockCount--;
        }

        public void BlockPhysicalMouse()
        {
            PhysicalMouseBlockCount++;
        }

        public void UnblockPhysicalMouse()
        {
            PhysicalMouseBlockCount--;
        }
    }
}
