using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class MenuItemUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> ExecuteAsync(string menuPath, CancellationToken cancellationToken)
    {
        var request = new ExecuteMenuItemRequest { menuPath = menuPath };
        await client.PostAsync<ExecuteMenuItemRequest, ExecuteMenuItemResponse>(ApiRoutes.MenuItemExecute, request,
            cancellationToken);
        return $"Menu item executed: {menuPath}";
    }
}
