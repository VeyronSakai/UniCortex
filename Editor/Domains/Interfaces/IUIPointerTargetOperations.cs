using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    // Methods are called on the main thread and complete in the player loop,
    // where EventSystem raycasts see the Game View's screen size.
    internal interface IUIPointerTargetOperations
    {
        Task<List<UIPointerTargetEntry>> GetUIPointerTargetsAsync(CancellationToken cancellationToken);

        // Returns the center of a UI object in Game View coordinates.
        Task<(float x, float y)> GetTargetCenterAsync(int instanceId,
            CancellationToken cancellationToken);
    }
}
