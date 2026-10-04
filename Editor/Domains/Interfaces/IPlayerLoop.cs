using System;

namespace UniCortex.Editor.Domains.Interfaces
{
    // Abstraction over UnityEngine.LowLevel.PlayerLoop.
    internal interface IPlayerLoop
    {
        // True when the PreLateUpdate phase has a system of the given type that calls the given function.
        bool Contains(Type type, Action update);

        // Adds a system of the given type that calls the given function to the end of the PreLateUpdate phase.
        // A system of the same type that is already there is replaced.
        void Insert(Type type, Action update);
    }
}
