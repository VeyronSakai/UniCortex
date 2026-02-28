using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyGameObjectOperations : IGameObjectOperations
    {
        public int GetCallCount { get; private set; }
        public string LastGetQuery { get; private set; }
        public List<GameObjectData> GetResult { get; set; } = new List<GameObjectData>();

        public int CreateCallCount { get; private set; }
        public string LastCreateName { get; private set; }
        public CreateGameObjectResponse CreateResult { get; set; } = new CreateGameObjectResponse("New", 1);

        public int DeleteCallCount { get; private set; }
        public int LastDeleteInstanceId { get; private set; }

        public int ModifyCallCount { get; private set; }
        public int LastModifyInstanceId { get; private set; }
        public string LastModifyName { get; private set; }
        public bool? LastModifyActiveSelf { get; private set; }
        public string LastModifyTag { get; private set; }
        public int? LastModifyLayer { get; private set; }
        public int? LastModifyParentInstanceId { get; private set; }

        public List<GameObjectData> Get(string query)
        {
            GetCallCount++;
            LastGetQuery = query;
            return GetResult;
        }

        public CreateGameObjectResponse Create(string name)
        {
            CreateCallCount++;
            LastCreateName = name;
            return CreateResult;
        }

        public void Delete(int instanceId)
        {
            DeleteCallCount++;
            LastDeleteInstanceId = instanceId;
        }

        public void Modify(int instanceId, string name, bool? activeSelf, string tag, int? layer,
            int? parentInstanceId)
        {
            ModifyCallCount++;
            LastModifyInstanceId = instanceId;
            LastModifyName = name;
            LastModifyActiveSelf = activeSelf;
            LastModifyTag = tag;
            LastModifyLayer = layer;
            LastModifyParentInstanceId = parentInstanceId;
        }
    }
}
