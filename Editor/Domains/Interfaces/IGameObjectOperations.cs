using System.Collections.Generic;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IGameObjectOperations
    {
        List<GameObjectBasicInfo> Find(string name, string tag, string componentType);
        CreateGameObjectResponse Create(string name, string primitive);
        void Delete(int instanceId);
        GameObjectInfoResponse GetInfo(int instanceId);
        void Modify(int instanceId, string name, bool? activeSelf, string tag, int? layer, int? parentInstanceId);
    }
}
