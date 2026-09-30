namespace UniCortex.Core.Domains.Interfaces;

public interface IUnityEditorClient
{
    ValueTask WaitForServerAsync(CancellationToken cancellationToken = default);

    ValueTask<TRes> PostAsync<TReq, TRes>(string route, TReq? request = null,
        CancellationToken cancellationToken = default) where TReq : class;

    ValueTask<TRes> GetAsync<TReq, TRes>(string route, TReq? request = null,
        CancellationToken cancellationToken = default) where TReq : class;
}
