using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class InputSimulationOperationsAdapter : IInputSimulationOperations
    {
        private static readonly System.Type GameViewType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");

        public void SendKeyEvent(string keyName, string eventType)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Input simulation is only available in Play Mode. Enter Play Mode first.");
            }

            var evt = Event.KeyboardEvent(keyName);

            if (string.Equals(eventType, "keyUp", StringComparison.OrdinalIgnoreCase))
            {
                evt.type = EventType.KeyUp;
            }

            var gameView = EditorWindow.GetWindow(GameViewType);
            gameView.SendEvent(evt);
        }

        public void SendMouseEvent(float x, float y, int button, string eventType)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Input simulation is only available in Play Mode. Enter Play Mode first.");
            }

            var isMouseUp = string.Equals(eventType, "mouseUp", StringComparison.OrdinalIgnoreCase);

            var evt = new Event
            {
                type = isMouseUp ? EventType.MouseUp : EventType.MouseDown,
                mousePosition = new Vector2(x, y),
                button = button
            };

            var gameView = EditorWindow.GetWindow(GameViewType);
            gameView.SendEvent(evt);
        }
    }
}
