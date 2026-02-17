using EditorBridge.Editor.Domains.Interfaces;

namespace EditorBridge.Editor.Tests.TestDoubles
{
    internal sealed class FakeRequestContext : IRequestContext
    {
        public string HttpMethod { get; set; } = "GET";
        public string Path { get; set; } = "/";
        public string Body { get; set; } = "";

        public int ResponseStatusCode { get; private set; }
        public string ResponseBody { get; private set; }

        public string ReadBody() => Body;

        public void WriteResponse(int statusCode, string json)
        {
            ResponseStatusCode = statusCode;
            ResponseBody = json;
        }
    }
}
