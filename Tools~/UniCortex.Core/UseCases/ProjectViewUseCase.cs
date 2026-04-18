using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ProjectViewUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> SelectAsync(string assetPath, CancellationToken cancellationToken)
    {
        await client.PostAsync<SelectProjectViewAssetRequest, SelectProjectViewAssetResponse>(
            ApiRoutes.ProjectViewSelect, new SelectProjectViewAssetRequest { assetPath = assetPath }, cancellationToken);
        return $"Project View asset selected: {assetPath}";
    }
}
