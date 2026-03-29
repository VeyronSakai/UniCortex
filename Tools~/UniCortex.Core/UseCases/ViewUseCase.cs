using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ViewUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> FocusSceneViewAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<FocusSceneViewRequest, FocusSceneViewResponse>(
            ApiRoutes.FocusSceneView, cancellationToken: cancellationToken);
        return "Scene View focused successfully.";
    }

    public async ValueTask<string> FocusGameViewAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<FocusGameViewRequest, FocusGameViewResponse>(
            ApiRoutes.FocusGameView, cancellationToken: cancellationToken);
        return "Game View focused successfully.";
    }
}
