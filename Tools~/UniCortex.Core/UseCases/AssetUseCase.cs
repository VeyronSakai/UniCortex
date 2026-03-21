using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class AssetUseCase(IUnityEditorClient client)
{
    public ValueTask<string> RefreshAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.AssetDatabaseRefresh, "Asset database refreshed.", cancellationToken);
    }
}
