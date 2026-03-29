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
}
