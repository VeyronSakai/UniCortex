using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetHierarchyResponse
    {
        public List<SceneHierarchy> scenes;

        public GetHierarchyResponse(List<SceneHierarchy> scenes)
        {
            this.scenes = scenes;
        }
    }
}
