using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ProfilerTools(ProfilerUseCase profilerUseCase)
{
    [McpServerTool(Name = "focus_profiler_window", ReadOnly = false),
     Description("Open or focus the Profiler window in the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> FocusProfilerWindowAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await profilerUseCase.FocusWindowAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "get_profiler_status", ReadOnly = true),
     Description("Get the current Profiler window and recording state in the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> GetProfilerStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            var json = await profilerUseCase.GetStatusAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "start_profiler_recording", ReadOnly = false),
     Description(
         "Start Profiler recording in the Unity Editor. " +
         "Set profileEditor to true to profile the Editor itself; false keeps player/play mode profiling."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StartProfilerRecordingAsync(
        [Description("When true, enable Editor profiling; when false, keep player/play mode profiling.")]
        bool profileEditor = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await profilerUseCase.StartRecordingAsync(profileEditor, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "stop_profiler_recording", ReadOnly = false),
     Description("Stop Profiler recording in the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StopProfilerRecordingAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await profilerUseCase.StopRecordingAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
