using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Services;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class TestTools(TestService testService)
{
    [McpServerTool(Name = "run_tests", ReadOnly = true),
     Description("Run Unity Test Runner tests and wait for completion."), UsedImplicitly]
    public async ValueTask<CallToolResult> RunTestsAsync(
        [Description("Test mode: '" + TestModes.EditMode + "' or '" + TestModes.PlayMode + "'. Defaults to '" + TestModes.EditMode + "'.")]
        string? testMode = null,
        [Description("Array of full test names to run (e.g. [\"MyTests.TestA\", \"MyTests.TestB\"]).")]
        string[]? testNames = null,
        [Description("Array of test group names to filter by.")]
        string[]? groupNames = null,
        [Description("Array of test category names to filter by (e.g. [\"Smoke\", \"Integration\"]).")]
        string[]? categoryNames = null,
        [Description("Array of test assembly names to filter by (e.g. [\"MyTests\"]).")]
        string[]? assemblyNames = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await testService.RunAsync(testMode, testNames, groupNames, categoryNames,
                assemblyNames, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
