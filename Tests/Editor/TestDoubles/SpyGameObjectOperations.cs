using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyGameObjectOperations : IGameObjectOperations
    {
        public int FindCallCount { get; private set; }
        public string LastFindName { get; private set; }
        public string LastFindTag { get; private set; }
        public string LastFindComponentType { get; private set; }
        public List<GameObjectBasicInfo> FindResult { get; set; } = new List<GameObjectBasicInfo>();

        public int CreateCallCount { get; private set; }
        public string LastCreateName { get; private set; }
        public string LastCreatePrimitive { get; private set; }
        public CreateGameObjectResponse CreateResult { get; set; } = new CreateGameObjectResponse("New", 1);

        public int DeleteCallCount { get; private set; }
        public int LastDeleteInstanceId { get; private set; }

        public int GetInfoCallCount { get; private set; }
        public int LastGetInfoInstanceId { get; private set; }
        public GameObjectInfoResponse GetInfoResult { get; set; }

        public int ModifyCallCount { get; private set; }
        public int LastModifyInstanceId { get; private set; }
        public string LastModifyName { get; private set; }
        public bool? LastModifyActiveSelf { get; private set; }
        public string LastModifyTag { get; private set; }
        public int? LastModifyLayer { get; private set; }
        public int? LastModifyParentInstanceId { get; private set; }

        public List<GameObjectBasicInfo> Find(string name, string tag, string componentType)
        {
            FindCallCount++;
            LastFindName = name;
            LastFindTag = tag;
            LastFindComponentType = componentType;
            return FindResult;
        }

        public CreateGameObjectResponse Create(string name, string primitive)
        {
            CreateCallCount++;
            LastCreateName = name;
            LastCreatePrimitive = primitive;
            return CreateResult;
        }

        public void Delete(int instanceId)
        {
            DeleteCallCount++;
            LastDeleteInstanceId = instanceId;
        }

        public GameObjectInfoResponse GetInfo(int instanceId)
        {
            GetInfoCallCount++;
            LastGetInfoInstanceId = instanceId;
            return GetInfoResult;
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
