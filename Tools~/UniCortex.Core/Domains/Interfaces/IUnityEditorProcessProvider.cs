namespace UniCortex.Core.Domains.Interfaces;

public interface IUnityEditorProcessProvider
{
    /// <summary>
    /// Returns the process ID of the running Unity Editor.
    /// </summary>
    int GetProcessId();
}
