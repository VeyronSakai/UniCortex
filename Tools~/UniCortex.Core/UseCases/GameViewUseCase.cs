using System.Text;
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
}
