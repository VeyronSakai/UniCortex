using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetGameObjectsResponse
    {
        public List<GameObjectData> gameObjects;

        public GetGameObjectsResponse(List<GameObjectData> gameObjects)
        {
            this.gameObjects = gameObjects;
        }
    }
}
