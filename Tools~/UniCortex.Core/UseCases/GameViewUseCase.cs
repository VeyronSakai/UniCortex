using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class GameViewUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> FocusAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<FocusGameViewRequest, FocusGameViewResponse>(
            ApiRoutes.FocusGameView, cancellationToken: cancellationToken);
        return "Game View focused successfully.";
    }

    // deviceFrame: null draws the device frame in the Simulator view only.
    public async ValueTask<byte[]> CaptureAsync(bool drawSafeArea, bool? deviceFrame,
        CancellationToken cancellationToken)
    {
        var request = new CaptureGameViewRequest { drawSafeArea = drawSafeArea, deviceFrame = deviceFrame };
        var response = await client.GetAsync<CaptureGameViewRequest, CaptureGameViewResponse>(
            ApiRoutes.GameViewCapture, request, cancellationToken);
        return Convert.FromBase64String(response.pngDataBase64);
    }

    public async ValueTask<string> GetSizeAsync(CancellationToken cancellationToken)
    {
        var response = await GetSizeResponseAsync(cancellationToken);
        return $"Game View size: {response.screenWidth}x{response.screenHeight}";
    }

    public async ValueTask<GetGameViewSizeResponse> GetSizeResponseAsync(CancellationToken cancellationToken)
    {
        return await client.GetAsync<GetGameViewSizeRequest, GetGameViewSizeResponse>(
            ApiRoutes.GameViewSize, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> GetSizeListAsync(CancellationToken cancellationToken)
    {
        var response = await GetSizeListResponseAsync(cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine($"Game View sizes (selected: {response.selectedIndex}):");
        foreach (var entry in response.sizes)
        {
            var marker = entry.index == response.selectedIndex ? " *" : "";
            sb.AppendLine($"  [{entry.index}] {entry.name} ({entry.width}x{entry.height}, {entry.sizeType}){marker}");
        }

        return sb.ToString().TrimEnd();
    }

    public async ValueTask<GetGameViewSizeListResponse> GetSizeListResponseAsync(
        CancellationToken cancellationToken)
    {
        return await client.GetAsync<GetGameViewSizeListRequest, GetGameViewSizeListResponse>(
            ApiRoutes.GameViewSizeList, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> SetSizeAsync(int index, CancellationToken cancellationToken)
    {
        await client.PostAsync<SetGameViewSizeRequest, SetGameViewSizeResponse>(
            ApiRoutes.GameViewSize, new SetGameViewSizeRequest { index = index },
            cancellationToken);
        return $"Game View size set to index {index} successfully.";
    }

    public async ValueTask<string> GetScaleAsync(CancellationToken cancellationToken)
    {
        var response = await GetScaleResponseAsync(cancellationToken);
        return $"Game View scale: {response.scale} (min: {response.minScale}, max: {response.maxScale})";
    }

    public async ValueTask<GetGameViewScaleResponse> GetScaleResponseAsync(CancellationToken cancellationToken)
    {
        return await client.GetAsync<GetGameViewScaleRequest, GetGameViewScaleResponse>(
            ApiRoutes.GameViewScale, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> SetScaleAsync(float scale, CancellationToken cancellationToken)
    {
        var response = await client.PostAsync<SetGameViewScaleRequest, SetGameViewScaleResponse>(
            ApiRoutes.GameViewScale, new SetGameViewScaleRequest { scale = scale },
            cancellationToken);
        return $"Game View scale set to {response.scale} successfully.";
    }

    public async ValueTask<string> GetViewTypeAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetPlayModeViewTypeRequest, GetPlayModeViewTypeResponse>(
            ApiRoutes.GameViewViewType, cancellationToken: cancellationToken);
        return $"Play Mode view type: {response.viewType}";
    }

    public async ValueTask<string> SetViewTypeAsync(string viewType, CancellationToken cancellationToken)
    {
        var response = await client.PostAsync<SetPlayModeViewTypeRequest, SetPlayModeViewTypeResponse>(
            ApiRoutes.GameViewViewType, new SetPlayModeViewTypeRequest { viewType = viewType },
            cancellationToken);
        return $"Play Mode view type set to {response.viewType} successfully.";
    }

    public async ValueTask<string> GetSimulatorDeviceListAsync(CancellationToken cancellationToken)
    {
        var response = await GetSimulatorDeviceListResponseAsync(cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine($"Simulator devices (selected: {response.selectedIndex}, rotation: {response.rotation}):");
        foreach (var device in response.devices)
        {
            var marker = device.index == response.selectedIndex ? " *" : "";
            sb.AppendLine($"  [{device.index}] {device.name} ({device.screenWidth}x{device.screenHeight}){marker}");
        }

        return sb.ToString().TrimEnd();
    }

    public async ValueTask<GetSimulatorDeviceListResponse> GetSimulatorDeviceListResponseAsync(
        CancellationToken cancellationToken)
    {
        return await client.GetAsync<GetSimulatorDeviceListRequest, GetSimulatorDeviceListResponse>(
            ApiRoutes.SimulatorDevices, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> SetSimulatorDeviceAsync(int? index, int? rotation,
        CancellationToken cancellationToken)
    {
        // -1 keeps the current device or rotation.
        var request = new SetSimulatorDeviceRequest { index = index ?? -1, rotation = rotation ?? -1 };
        var response = await client.PostAsync<SetSimulatorDeviceRequest, SetSimulatorDeviceResponse>(
            ApiRoutes.SimulatorDevice, request, cancellationToken);
        return $"Simulator device set to {response.deviceName} (rotation: {response.rotation}) successfully.";
    }

    public async ValueTask<string> GetSafeAreaAsync(CancellationToken cancellationToken)
    {
        var response = await GetSafeAreaResponseAsync(cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<GetScreenSafeAreaResponse> GetSafeAreaResponseAsync(CancellationToken cancellationToken)
    {
        return await client.GetAsync<GetScreenSafeAreaRequest, GetScreenSafeAreaResponse>(
            ApiRoutes.GameViewSafeArea, cancellationToken: cancellationToken);
    }
}
