using System.Threading.Tasks;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IRequestContext
    {
        HttpMethodType HttpMethod { get; }
        string Path { get; }
        string GetQueryParameter(string name);
        Task<string> ReadBodyAsync();
        Task WriteResponseAsync(int statusCode, string json);
    }
}
