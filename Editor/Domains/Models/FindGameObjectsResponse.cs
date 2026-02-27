using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class FindGameObjectsResponse
    {
        public List<GameObjectBasicInfo> gameObjects;

        public FindGameObjectsResponse(List<GameObjectBasicInfo> gameObjects)
        {
            this.gameObjects = gameObjects;
        }
    }
}
