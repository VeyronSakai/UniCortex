using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ProjectWindowUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> SelectAsync(string assetPath, CancellationToken cancellationToken)
    {
        await client.PostAsync<SelectProjectWindowAssetRequest, SelectProjectWindowAssetResponse>(
            ApiRoutes.ProjectWindowSelect, new SelectProjectWindowAssetRequest { assetPath = assetPath }, cancellationToken);
        return $"Project Window asset selected: {assetPath}";
    }
}
