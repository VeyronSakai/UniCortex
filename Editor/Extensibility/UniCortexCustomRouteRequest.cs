using System;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Extensibility
{
    [Serializable]
    public class UniCortexCustomRouteRequest
    {
        public string path;
        public HttpMethodType httpMethod;
        public string body;
        public UniCortexQueryParameter[] queryParameters;

        public UniCortexCustomRouteRequest(
            string path,
            HttpMethodType httpMethod,
            string body,
            UniCortexQueryParameter[] queryParameters)
        {
            this.path = path;
            this.httpMethod = httpMethod;
            this.body = body;
            this.queryParameters = queryParameters ?? Array.Empty<UniCortexQueryParameter>();
        }

        public string GetQueryParameter(string name)
        {
            foreach (var queryParameter in queryParameters)
            {
                if (string.Equals(queryParameter.name, name, StringComparison.Ordinal))
                {
                    return queryParameter.value;
                }
            }

            return string.Empty;
        }
    }
}
