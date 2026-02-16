using System.Diagnostics.CodeAnalysis;
using EditorBridge.Editor.Models;

namespace UnityEditorBridge.CLI.Commands;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class EditorCommands
{
    /// <summary>Check server connectivity.</summary>
    public async Task Ping()
    {
        await BridgeClient.GetAsync(ApiRoutes.Ping, CancellationToken.None);
    }
}
