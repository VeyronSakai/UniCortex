using System;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UnityEngine.LowLevel;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class PlayerLoopAdapter : IPlayerLoop
    {
        public bool Contains<TPhase, TSystem>(Action update)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var phase = loop.subSystemList[FindPhaseIndex<TPhase>(loop)];
            return (phase.subSystemList ?? Array.Empty<PlayerLoopSystem>()).Any(system =>
                system.type == typeof(TSystem)
                // Insert wraps the function with update.Invoke, so the wrapped function is the target.
                && system.updateDelegate?.Target is Action installed
                && installed.Equals(update));
        }

        public void Insert<TPhase, TSystem>(Action update)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var index = FindPhaseIndex<TPhase>(loop);
            var phase = loop.subSystemList[index];

            var systems = (phase.subSystemList ?? Array.Empty<PlayerLoopSystem>())
                .Where(system => system.type != typeof(TSystem))
                .Append(new PlayerLoopSystem { type = typeof(TSystem), updateDelegate = update.Invoke })
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
