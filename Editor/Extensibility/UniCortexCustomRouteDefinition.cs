using System;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Extensibility
{
    [Serializable]
    public class UniCortexCustomRouteDefinition
    {
        public HttpMethodType method;
        public string path;
        public string description;

        public UniCortexCustomRouteDefinition(HttpMethodType method, string path, string description = "")
        {
            this.method = method;
            this.path = path;
            this.description = description;
        }
    }
}
