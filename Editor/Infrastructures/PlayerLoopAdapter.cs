using System;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace UniCortex.Editor.Infrastructures
{
    // Systems are inserted at the end of PostLateUpdate, after Canvas layout updates and rendering,
    // so that UI positions and raycasts match what the Game View shows for the frame. Before rendering,
    // UI changed in the frame is not laid out yet, and Graphics shown for the first time have no depth,
    // so GraphicRaycaster does not hit them.
    internal sealed class PlayerLoopAdapter : IPlayerLoop
    {
        public bool ContainsInPostLateUpdate(Type type, Action update)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var phase = loop.subSystemList[FindPhaseIndex<PostLateUpdate>(loop)];
            return (phase.subSystemList ?? Array.Empty<PlayerLoopSystem>()).Any(system =>
                system.type == type
                // InsertIntoPostLateUpdate wraps the function with update.Invoke, so the wrapped function is the target.
                && system.updateDelegate?.Target is Action installed
                && installed.Equals(update));
        }

        public void InsertIntoPostLateUpdate(Type type, Action update)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var index = FindPhaseIndex<PostLateUpdate>(loop);
            var phase = loop.subSystemList[index];

            var systems = (phase.subSystemList ?? Array.Empty<PlayerLoopSystem>())
                .Where(system => system.type != type)
                .Append(new PlayerLoopSystem { type = type, updateDelegate = update.Invoke })
                .ToArray();

            phase.subSystemList = systems;
            loop.subSystemList[index] = phase;
            PlayerLoop.SetPlayerLoop(loop);
        }

        private static int FindPhaseIndex<TPhase>(PlayerLoopSystem loop)
        {
            for (var i = 0; i < loop.subSystemList.Length; i++)
            {
                if (loop.subSystemList[i].type == typeof(TPhase))
                {
                    return i;
                }
            }

            throw new InvalidOperationException($"{typeof(TPhase).Name} phase was not found in the player loop.");
        }
    }
}
