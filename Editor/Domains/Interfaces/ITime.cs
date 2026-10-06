namespace UniCortex.Editor.Domains.Interfaces
{
    // Abstraction over UnityEngine.Time.
    internal interface ITime
    {
        // Time at the beginning of the current frame in seconds, not affected by Time.timeScale.
        double UnscaledTime { get; }
    }
}
