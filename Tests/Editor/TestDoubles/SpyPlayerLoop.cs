using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyPlayerLoop : IPlayerLoop
    {
        private readonly List<(Type phase, Type type, Action update)> _systems = new();

        public int InsertCallCount { get; private set; }

        public IReadOnlyList<(Type phase, Type type, Action update)> Systems => _systems;

        public bool Contains<TPhase, TSystem>(Action update)
        {
            return _systems.Any(system =>
                system.phase == typeof(TPhase) && system.type == typeof(TSystem) && system.update.Equals(update));
        }

        public void Insert<TPhase, TSystem>(Action update)
        {
            InsertCallCount++;
            _systems.RemoveAll(system => system.phase == typeof(TPhase) && system.type == typeof(TSystem));
            _systems.Add((typeof(TPhase), typeof(TSystem), update));
        }

        // Simulates one player loop update by calling every inserted system.
        public void Update()
        {
            foreach (var (_, _, update) in _systems.ToList())
            {
                update();
            }
        }
    }
}
