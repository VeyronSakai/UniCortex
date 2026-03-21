namespace UniCortex.Core.Domains.Interfaces;

public interface IUnityEditorClient
{
    ValueTask WaitForServerAsync(CancellationToken cancellationToken);

    ValueTask<string> PostAsync<T>(string route, T request, CancellationToken cancellationToken);

    ValueTask PostAsync(string route, CancellationToken cancellationToken);

    ValueTask<string> GetStringAsync(string route, CancellationToken cancellationToken);

    ValueTask<byte[]> GetBytesAsync(string route, CancellationToken cancellationToken);

    ValueTask<HttpResponseMessage> SendGetAsync(string route, CancellationToken cancellationToken);

    ValueTask<HttpResponseMessage> SendPostAsync(string route, HttpContent? content,
        CancellationToken cancellationToken);
}
