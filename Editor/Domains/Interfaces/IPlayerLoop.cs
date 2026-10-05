using System;

namespace UniCortex.Editor.Domains.Interfaces
{
    // Abstraction over UnityEngine.LowLevel.PlayerLoop.
    // TPhase is a player loop phase type (e.g. UnityEngine.PlayerLoop.PostLateUpdate), and TSystem is the type
    // that identifies the inserted system.
    internal interface IPlayerLoop
    {
        // True when the TPhase phase has a system of TSystem that calls the given function.
        bool Contains<TPhase, TSystem>(Action update);

        // Adds a system of TSystem that calls the given function to the end of the TPhase phase.
        // A system of TSystem that is already there is replaced.
        void Insert<TPhase, TSystem>(Action update);
    }
}
