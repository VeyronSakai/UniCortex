using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class GameViewTools(GameViewUseCase gameViewUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "focus_game_view", ReadOnly = false),
     Description(
         "Switch focus to the Game View window in the Unity Editor."),
     UsedImplicitly]
    public ValueTask<CallToolResult> FocusGameViewAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.FocusAsync, cancellationToken);

    [McpServerTool(Name = "capture_game_view", ReadOnly = true),
     Description(
         "Capture the Game View as a PNG at its resolution (focuses it first). " +
         "Play Mode only; use capture_scene_view in Edit Mode. " +
         "In the Simulator view the device frame is included unless deviceFrame is false. " +
         "Without the frame, image pixel (px, py) is screen x = px, y = imageHeight - py."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CaptureGameViewAsync(
        [Description(
            "Draw the safe area (Screen.safeArea) as a yellow outline and the cutouts (Screen.cutouts, " +
            "e.g. notches) in translucent red, e.g. to check UI against the notch. Default false.")]
        bool? drawSafeArea = null,
        [Description(
            "Draw the Simulator view's device frame (bezel, rounded corners, notch) around the image. " +
            "Default: true in the Simulator view, false in the Game view (true is an error there). " +
            "With the frame, pixels no longer match screen coordinates.")]
        bool? deviceFrame = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var pngData = await gameViewUseCase.CaptureAsync(drawSafeArea ?? false, deviceFrame, ct);
            return new CallToolResult
            {
                Content = [ImageContentBlock.FromBytes(pngData, "image/png")]
            };
        }, cancellationToken);

    [McpServerTool(Name = "get_game_view_size", ReadOnly = true),
     Description("Get the current Game View size in pixels."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetGameViewSizeAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSizeAsync, cancellationToken);

    [McpServerTool(Name = "get_game_view_size_list", ReadOnly = true),
     Description(
         "List the Game View sizes (index, name, width, height, type). Pass an index to set_game_view_size."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetGameViewSizeListAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSizeListAsync, cancellationToken);

    [McpServerTool(Name = "set_game_view_size", ReadOnly = false),
     Description(
         "Select a Game View size by its index from get_game_view_size_list. " +
         "Use add_game_view_size for a resolution not in the list."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetGameViewSizeAsync(
        [Description("Index from get_game_view_size_list.")]
        int index,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.SetSizeAsync(index, ct), cancellationToken);

    [McpServerTool(Name = "add_game_view_size", ReadOnly = false),
     Description(
         "Add a fixed resolution to the current platform's Game View size list (saved to Unity preferences) " +
         "and return its index; it is not selected, so pass the index to set_game_view_size. " +
         "An existing size with the same name and size is reused. A smaller size with the device's aspect " +
         "ratio (e.g. 642x1389 for 1284x2778) keeps the layout and makes capture_game_view images smaller."),
     UsedImplicitly]
    public ValueTask<CallToolResult> AddGameViewSizeAsync(
        [Description("Label shown in the resolution menu (up to 40 characters).")]
        string name,
        [Description("Width in pixels (10 to 99999).")]
        int width,
        [Description("Height in pixels (10 to 99999).")]
        int height,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.AddSizeAsync(name, width, height, ct), cancellationToken);

    [McpServerTool(Name = "get_game_view_scale", ReadOnly = true),
     Description(
         "Get the Game View zoom scale (1.0 = 100%) and its valid minimum and maximum."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetGameViewScaleAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetScaleAsync, cancellationToken);

    [McpServerTool(Name = "set_game_view_scale", ReadOnly = false),
     Description(
         "Set the Game View zoom scale (1.0 = 100%). It is clamped to the valid range and the applied value is returned."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetGameViewScaleAsync(
        [Description("Zoom scale; 1.0 = 100%.")]
        float scale,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.SetScaleAsync(scale, ct), cancellationToken);

    [McpServerTool(Name = "get_play_mode_view_type", ReadOnly = true),
     Description(
         "Get whether the Play Mode window shows the Game view (\"GameView\") or " +
         "the Device Simulator view (\"SimulatorView\")."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetPlayModeViewTypeAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetViewTypeAsync, cancellationToken);

    [McpServerTool(Name = "set_play_mode_view_type", ReadOnly = false),
     Description(
         "Switch the Play Mode window between the Game view and the Device Simulator view. " +
         "Only the Simulator view simulates a device's safe area and notch (Screen.safeArea, Screen.cutouts)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetPlayModeViewTypeAsync(
        [Description("View type: \"GameView\" or \"SimulatorView\".")]
        string viewType,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.SetViewTypeAsync(viewType, ct), cancellationToken);

    [McpServerTool(Name = "get_simulator_device_list", ReadOnly = true),
     Description(
         "List the Device Simulator devices (index, name, native portrait resolution) with the selected device " +
         "and rotation. Requires the Simulator view (see set_play_mode_view_type)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetSimulatorDeviceListAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSimulatorDeviceListAsync,
            cancellationToken);

    [McpServerTool(Name = "set_simulator_device", ReadOnly = false),
     Description(
         "Select the simulated device and/or its rotation. Requires the Simulator view " +
         "(see set_play_mode_view_type). Use get_screen_safe_area afterwards for the resulting screen and safe area."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetSimulatorDeviceAsync(
        [Description("Device index from get_simulator_device_list. Omit to keep the current device.")]
        int? index = null,
        [Description("Clockwise rotation in degrees: 0 (portrait), 90, 180 or 270. Omit to keep the current rotation.")]
        int? rotation = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.SetSimulatorDeviceAsync(index, rotation, ct), cancellationToken);

    [McpServerTool(Name = "get_screen_safe_area", ReadOnly = true),
     Description(
         "Get the screen size, safe area (Screen.safeArea) and cutouts (Screen.cutouts) of the Play Mode window: " +
         "the simulated device's in the Simulator view, the whole screen in the Game view. " +
         "Rects are in screen coordinates (bottom-left origin), the same as get_ui_pointer_targets."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetScreenSafeAreaAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSafeAreaAsync, cancellationToken);
}
