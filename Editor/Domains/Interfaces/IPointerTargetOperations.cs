using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    // Methods are called on the main thread and complete in the player loop,
    // where EventSystem raycasts see the Game View's screen size.
    internal interface IPointerTargetOperations
    {
        Task<List<PointerTarget>> GetPointerTargetsAsync(CancellationToken cancellationToken);

        // Returns the center of a UI object in Game View coordinates, and whether other UI covers it there.
        Task<(float x, float y, bool blocked)> GetTargetCenterAsync(int instanceId,
            CancellationToken cancellationToken);
    }
}
