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
}
