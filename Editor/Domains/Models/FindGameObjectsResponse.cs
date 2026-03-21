using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class FindGameObjectsResponse
    {
        public List<GameObjectSearchResult> gameObjects;

        public FindGameObjectsResponse(List<GameObjectSearchResult> gameObjects)
        {
            this.gameObjects = gameObjects;
        }
    }
}
