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
         "Capture the Game View as a PNG image at the Game View resolution. " +
         "Only available in Play Mode; use capture_scene_view in Edit Mode. " +
         "The Game View is opened if needed and focused before capturing. " +
         "When the Play Mode window is in the Simulator view, the simulated device screen is captured " +
         "with the device frame unless deviceFrame is false. In the Game view, or with deviceFrame set to false, " +
         "image pixel (px, py) corresponds to screen coordinates x = px, y = imageHeight - py."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CaptureGameViewAsync(
        [Description(
            "Draw the safe area (Screen.safeArea) as a yellow outline and the cutouts (Screen.cutouts, " +
            "e.g. notches and camera holes) as translucent red areas on the image. " +
            "Useful with the Simulator view to check whether UI overlaps the notch. Defaults to false.")]
        bool? drawSafeArea = null,
        [Description(
            "Draw the device frame of the Simulator view (bezel, rounded corners, notch and camera hole) " +
            "around the image, rotated like the simulated device, to see whether UI overlaps the notch or " +
            "the rounded corners. When omitted, the frame is drawn in the Simulator view and not in the " +
            "Game view. true is an error in the Game view. The image then includes the frame, so its pixels " +
            "no longer match screen coordinates; set to false to capture only the screen.")]
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
     Description("Get the current Game View size (width and height in pixels) in the Unity Editor."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetGameViewSizeAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSizeAsync, cancellationToken);

    [McpServerTool(Name = "get_game_view_size_list", ReadOnly = true),
     Description(
         "Get the list of available Game View sizes (built-in and custom) in the Unity Editor. " +
         "Returns each size with its index, name, width, height, and type. " +
         "Use the index with set_game_view_size to select a resolution."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetGameViewSizeListAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSizeListAsync, cancellationToken);

    [McpServerTool(Name = "set_game_view_size", ReadOnly = false),
     Description(
         "Set the Game View resolution in the Unity Editor by selecting a size from the available list. " +
         "Use get_game_view_size_list to get available sizes and their indices."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetGameViewSizeAsync(
        [Description("Index of the size from get_game_view_size_list.")]
        int index,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.SetSizeAsync(index, ct), cancellationToken);

    [McpServerTool(Name = "get_game_view_scale", ReadOnly = true),
     Description(
         "Get the current Game View scale (zoom factor) in the Unity Editor, " +
         "along with the valid minimum and maximum scale. 1.0 means 100%."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetGameViewScaleAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetScaleAsync, cancellationToken);

    [McpServerTool(Name = "set_game_view_scale", ReadOnly = false),
     Description(
         "Set the Game View scale (zoom factor) in the Unity Editor. 1.0 means 100%. " +
         "The value is clamped to the Game View's valid range and the applied value is returned. " +
         "Use get_game_view_scale to see the current value and valid range."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetGameViewScaleAsync(
        [Description("Scale (zoom) factor. 1.0 = 100%. Clamped to the Game View's valid range.")]
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
         "Use the Simulator view to simulate a mobile device, including its safe area and notch " +
         "(Screen.safeArea and Screen.cutouts), which the Game view does not simulate."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetPlayModeViewTypeAsync(
        [Description("View type: \"GameView\" or \"SimulatorView\".")]
        string viewType,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.SetViewTypeAsync(viewType, ct), cancellationToken);

    [McpServerTool(Name = "get_simulator_device_list", ReadOnly = true),
     Description(
         "Get the list of devices available in the Device Simulator view, with each device's index, name and " +
         "native screen resolution (portrait), together with the selected device and its rotation. " +
         "The Play Mode window must be in the Simulator view (see set_play_mode_view_type)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetSimulatorDeviceListAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSimulatorDeviceListAsync,
            cancellationToken);

    [McpServerTool(Name = "set_simulator_device", ReadOnly = false),
     Description(
         "Select the simulated device and/or its rotation in the Device Simulator view. " +
         "Specify index, rotation, or both. The Play Mode window must be in the Simulator view " +
         "(see set_play_mode_view_type). Use get_screen_safe_area afterwards to get the resulting " +
         "screen orientation, size and safe area."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetSimulatorDeviceAsync(
        [Description("Index of the device from get_simulator_device_list. Omit to keep the current device.")]
        int? index = null,
        [Description(
            "Clockwise rotation of the device in degrees: 0 (portrait), 90, 180 or 270, " +
            "same as the rotate buttons of the Simulator view. Omit to keep the current rotation.")]
        int? rotation = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.SetSimulatorDeviceAsync(index, rotation, ct), cancellationToken);

    [McpServerTool(Name = "get_screen_safe_area", ReadOnly = true),
     Description(
         "Get the screen size, the safe area (Screen.safeArea) and the cutouts (Screen.cutouts) of the " +
         "Play Mode window. In the Simulator view, these are the values of the simulated device and " +
         "orientation; in the Game view, the safe area is the whole screen. Rects are in screen coordinates " +
         "(origin at the bottom-left), the same as get_ui_pointer_targets, so UI rects can be compared " +
         "with the safe area directly."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetScreenSafeAreaAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSafeAreaAsync, cancellationToken);
}
