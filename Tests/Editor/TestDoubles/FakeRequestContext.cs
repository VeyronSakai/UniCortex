using System.Collections.Generic;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    // IMPORTANT: Async methods must return synchronously completed tasks
    // (Task.CompletedTask / Task.FromResult). Do NOT use Task.Yield() or any
    // truly asynchronous construct here. Tests call .GetAwaiter().GetResult()
    // which blocks the thread. Under Unity's UnitySynchronizationContext the
    // continuation from Task.Yield() would be posted back to the blocked
    // thread, causing a deadlock.
    //
    // Async test methods ([Test] async Task) are not reliably supported until
    // Unity Test Framework 1.3+ (Unity 2023.1+). With Test Framework 1.1.x,
    // the synchronous-blocking + completed-task pattern used here is the
    // safest approach.
    internal sealed class FakeRequestContext : IRequestContext
    {
        public string HttpMethod { get; }
        public string Path { get; }
        public string Body { get; }

        public int ResponseStatusCode { get; private set; }
        public string ResponseBody { get; private set; }

        public FakeRequestContext(string httpMethod, string path, string body = "")
        {
            HttpMethod = httpMethod;
            Path = path;
            Body = body;
        }

        private readonly Dictionary<string, string> _queryParameters = new();

        public void SetQueryParameter(string name, string value)
        {
            _queryParameters[name] = value;
        }

        public string GetQueryParameter(string name)
        {
            _queryParameters.TryGetValue(name, out var value);
            return value;
        }

        public Task<string> ReadBodyAsync()
        {
            return Task.FromResult(Body);
        }

        public Task WriteResponseAsync(int statusCode, string json)
        {
            ResponseStatusCode = statusCode;
            ResponseBody = json;
            return Task.CompletedTask;
        }
    }
}
