using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetSceneHierarchyResponse
    {
        public string sceneName;
        public string scenePath;
        public List<GameObjectNode> gameObjects;

        public GetSceneHierarchyResponse(string sceneName, string scenePath, List<GameObjectNode> gameObjects)
        {
            this.sceneName = sceneName;
            this.scenePath = scenePath;
            this.gameObjects = gameObjects;
        }
    }
}
