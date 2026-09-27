using System.Collections.Generic;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IGameObjectOperations
    {
        List<GameObjectSearchResult> Get(string query);
        CreateGameObjectResponse Create(string name, int parentInstanceId, int? siblingIndex, bool useRectTransform);
        void Delete(int instanceId);
        void Modify(int instanceId, string name, bool? activeSelf, string tag, int? layer, int? parentInstanceId,
            int? siblingIndex, bool worldPositionStays);
        DuplicateGameObjectResponse Duplicate(int instanceId, string name);
    }
}
