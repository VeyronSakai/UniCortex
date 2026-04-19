using UniCortex.Core.Domains.Interfaces;

namespace UniCortex.Core.Infrastructures;

public sealed class AsyncOperationSequencer : IAsyncOperationSequencer
{
    private readonly Lock _syncRoot = new();
    private Task _tail = Task.CompletedTask;

    public async ValueTask<T> EnqueueAsync<T>(Func<CancellationToken, ValueTask<T>> operation,
        CancellationToken cancellationToken = default)
    {
        Task previous;
        TaskCompletionSource<bool> completion;

        lock (_syncRoot)
        {
            previous = _tail;
            completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _tail = completion.Task;
        }

        try
        {
            await previous.WaitAsync(cancellationToken);
            return await operation(cancellationToken);
        }
        finally
        {
            completion.TrySetResult(true);
        }
    }
}
 