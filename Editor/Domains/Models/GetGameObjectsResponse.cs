using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetGameObjectsResponse
    {
        public List<GameObjectSearchResult> gameObjects;

        public GetGameObjectsResponse(List<GameObjectSearchResult> gameObjects)
        {
            this.gameObjects = gameObjects;
        }
    }
}
