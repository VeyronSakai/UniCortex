using System.Collections.Generic;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IRequestContext
    {
        HttpMethodType HttpMethod { get; }
        string Path { get; }
        string GetQueryParameter(string name);
        IReadOnlyList<KeyValuePair<string, string>> GetQueryParameters();
        Task<string> ReadBodyAsync();
        Task WriteResponseAsync(int statusCode, string json);
    }
}
