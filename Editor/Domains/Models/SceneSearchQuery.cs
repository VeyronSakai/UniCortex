using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SceneSearchQuery
    {
        public string namePattern;
        public string componentTypePattern;
        public string tagPartial;
        public string tagExact;
        public int? instanceId;
        public int? layer;
        public string pathPattern;
        public List<string> stateFilters;

        public SceneSearchQuery()
        {
            namePattern = null;
            componentTypePattern = null;
            tagPartial = null;
            tagExact = null;
            instanceId = null;
            layer = null;
            pathPattern = null;
            stateFilters = new List<string>();
        }
    }
}
