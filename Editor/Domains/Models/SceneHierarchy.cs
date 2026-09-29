using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SceneHierarchy
    {
        public string sceneName;
        public string scenePath;
        public bool isActive;
        public List<GameObjectNode> gameObjects;

        public SceneHierarchy(string sceneName, string scenePath, bool isActive, List<GameObjectNode> gameObjects)
        {
            this.sceneName = sceneName;
            this.scenePath = scenePath;
            this.isActive = isActive;
            this.gameObjects = gameObjects;
        }
    }
}
