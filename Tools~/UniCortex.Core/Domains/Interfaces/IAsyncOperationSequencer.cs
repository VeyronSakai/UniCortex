namespace UniCortex.Core.Domains.Interfaces;

public interface IAsyncOperationSequencer
{
    ValueTask<T> EnqueueAsync<T>(Func<CancellationToken, ValueTask<T>> operation,
        CancellationToken cancellationToken = default);
}
 