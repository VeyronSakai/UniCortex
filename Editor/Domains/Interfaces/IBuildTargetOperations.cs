namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IBuildTargetOperations
    {
        string GetActiveBuildTarget();

        // Returns the canonical build target name that matches the given name (case-insensitive),
        // or null when no build target has that name.
        string ResolveBuildTarget(string name);

        // Returns whether the platform module of the build target is installed in this Editor.
        bool IsBuildTargetSupported(string buildTarget);

        // Returns the names of the build targets whose platform modules are installed.
        string[] GetSupportedBuildTargets();

        // Switches the active build target synchronously. Returns false when the switch failed.
        bool SwitchActiveBuildTarget(string buildTarget);
    }
}
