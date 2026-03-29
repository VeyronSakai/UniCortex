using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class SceneViewUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> FocusAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<FocusSceneViewRequest, FocusSceneViewResponse>(
            ApiRoutes.FocusSceneView, cancellationToken: cancellationToken);
        return "Scene View focused successfully.";
    }
}
