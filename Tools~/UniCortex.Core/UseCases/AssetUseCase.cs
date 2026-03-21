using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class AssetUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> RefreshAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync(ApiRoutes.AssetDatabaseRefresh, cancellationToken);
        return "Asset database refreshed.";
    }
}
