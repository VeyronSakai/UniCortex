using System;

namespace UniCortex.Editor.Extensibility
{
    [Serializable]
    public class UniCortexCustomRouteResponse
    {
        public int statusCode;
        public string bodyJson;

        public UniCortexCustomRouteResponse(int statusCode, string bodyJson)
        {
            this.statusCode = statusCode;
            this.bodyJson = bodyJson;
        }
    }
}
