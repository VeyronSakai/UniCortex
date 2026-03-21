namespace UniCortex.Core.Domains.Interfaces;

public interface IUnityEditorClient
{
    HttpClient HttpClient { get; }
    string BaseUrl { get; }

    ValueTask WaitForServerAsync(CancellationToken cancellationToken);

    ValueTask<string> PostAsync<T>(string route, T request, string successMessage,
        CancellationToken cancellationToken);

    ValueTask<string> PostAndReadAsync<T>(string route, T request,
        CancellationToken cancellationToken);

    ValueTask<string> PostEmptyAsync(string route, string successMessage,
        CancellationToken cancellationToken);

    ValueTask<string> GetStringAsync(string route, CancellationToken cancellationToken);

    ValueTask<byte[]> GetBytesAsync(string route, CancellationToken cancellationToken);
}
