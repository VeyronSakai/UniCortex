using System;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class PlayerLoopAdapter : IPlayerLoop
    {
        public bool Contains(Type type, Action update)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var phase = loop.subSystemList[FindPreLateUpdateIndex(loop)];
            return (phase.subSystemList ?? Array.Empty<PlayerLoopSystem>()).Any(system =>
                system.type == type
                // Insert wraps the function with update.Invoke, so the wrapped function is the target.
                && system.updateDelegate?.Target is Action installed
                && installed.Equals(update));
        }

        public void Insert(Type type, Action update)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var index = FindPreLateUpdateIndex(loop);
            var phase = loop.subSystemList[index];

            var systems = (phase.subSystemList ?? Array.Empty<PlayerLoopSystem>())
                .Where(system => system.type != type)
                .Append(new PlayerLoopSystem { type = type, updateDelegate = update.Invoke })
                .ToArray();

            phase.subSystemList = systems;
            loop.subSystemList[index] = phase;
            PlayerLoop.SetPlayerLoop(loop);
        }

        private static int FindPreLateUpdateIndex(PlayerLoopSystem loop)
        {
            for (var i = 0; i < loop.subSystemList.Length; i++)
            {
                if (loop.subSystemList[i].type == typeof(PreLateUpdate))
                {
                    return i;
                }
            }

            throw new InvalidOperationException("PreLateUpdate phase was not found in the player loop.");
        }
    }
}
