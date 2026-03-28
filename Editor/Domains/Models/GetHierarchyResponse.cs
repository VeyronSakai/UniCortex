using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetHierarchyResponse
    {
        public string sceneName;
        public string scenePath;
        public List<GameObjectNode> gameObjects;

        public GetHierarchyResponse(string sceneName, string scenePath, List<GameObjectNode> gameObjects)
        {
            this.sceneName = sceneName;
            this.scenePath = scenePath;
            this.gameObjects = gameObjects;
        }
    }
}
