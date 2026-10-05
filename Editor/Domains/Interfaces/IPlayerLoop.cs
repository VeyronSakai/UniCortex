using System;

namespace UniCortex.Editor.Domains.Interfaces
{
    // Abstraction over UnityEngine.LowLevel.PlayerLoop.
    internal interface IPlayerLoop
    {
        // True when the PostLateUpdate phase has a system of the given type that calls the given function.
        bool ContainsInPostLateUpdate(Type type, Action update);

        // Adds a system of the given type that calls the given function to the end of the PostLateUpdate phase.
        // A system of the same type that is already there is replaced.
        void InsertIntoPostLateUpdate(Type type, Action update);
    }
}
