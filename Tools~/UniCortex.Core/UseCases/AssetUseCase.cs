using UniCortex.Core.Infrastructures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class AssetUseCase(UnityEditorClient client)
{
    public ValueTask<string> RefreshAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.AssetDatabaseRefresh, "Asset database refreshed.", cancellationToken);
    }
}
