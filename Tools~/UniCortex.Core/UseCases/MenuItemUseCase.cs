using UniCortex.Core.Infrastructures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class MenuItemUseCase(UnityEditorClient client)
{
    public ValueTask<string> ExecuteAsync(string menuPath, CancellationToken cancellationToken)
    {
        var request = new ExecuteMenuItemRequest { menuPath = menuPath };
        return client.PostAsync(ApiRoutes.MenuItemExecute, request, $"Menu item executed: {menuPath}",
            cancellationToken);
    }
}
