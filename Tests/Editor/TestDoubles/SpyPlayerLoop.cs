using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyPlayerLoop : IPlayerLoop
    {
        private readonly List<(Type type, Action update)> _systems = new();

        public int InsertCallCount { get; private set; }

        public IReadOnlyList<(Type type, Action update)> Systems => _systems;

        public bool Contains(Type type, Action update)
        {
            return _systems.Any(system => system.type == type && system.update.Equals(update));
        }

        public void Insert(Type type, Action update)
        {
            InsertCallCount++;
            _systems.RemoveAll(system => system.type == type);
            _systems.Add((type, update));
        }

        // Simulates one player loop update by calling every inserted system.
        public void Update()
        {
            foreach (var (_, update) in _systems.ToList())
            {
                update();
            }
        }
    }
}
