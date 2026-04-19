using NUnit.Framework;
using UniCortex.Core.Infrastructures;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class AsyncOperationSequencerTest
{
    [Test]
    public async ValueTask EnqueueAsync_RunsOperationsInSubmissionOrder()
    {
        var sequencer = new AsyncOperationSequencer();
        var started = new List<int>();
        var completed = new List<int>();
        var firstEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var first = sequencer.EnqueueAsync(async _ =>
        {
            started.Add(1);
            firstEntered.TrySetResult(true);
            await releaseFirst.Task;
            completed.Add(1);
            return 1;
        });

        await firstEntered.Task;

        var second = sequencer.EnqueueAsync(_ =>
        {
            started.Add(2);
            completed.Add(2);
            return ValueTask.FromResult(2);
        });

        var third = sequencer.EnqueueAsync(_ =>
        {
            started.Add(3);
            completed.Add(3);
            return ValueTask.FromResult(3);
        });

        Assert.That(started, Is.EqualTo(new[] { 1 }));

        releaseFirst.TrySetResult(true);

        Assert.That(await first, Is.EqualTo(1));
        Assert.That(await second, Is.EqualTo(2));
        Assert.That(await third, Is.EqualTo(3));
        Assert.That(started, Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(completed, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async ValueTask EnqueueAsync_ContinuesAfterFailure()
    {
        var sequencer = new AsyncOperationSequencer();

        var first = sequencer.EnqueueAsync<int>(_ => throw new InvalidOperationException("boom"));
        var second = sequencer.EnqueueAsync(_ => ValueTask.FromResult(2));

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await first);

        Assert.That(ex!.Message, Is.EqualTo("boom"));
        Assert.That(await second, Is.EqualTo(2));
    }

    [Test]
    public async ValueTask EnqueueAsync_CanceledWhileWaiting_DoesNotBlockFollowingOperation()
    {
        var sequencer = new AsyncOperationSequencer();
        var order = new List<int>();
        var firstEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thirdStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var first = sequencer.EnqueueAsync(async _ =>
        {
            order.Add(1);
            firstEntered.TrySetResult(true);
            await releaseFirst.Task;
            return 1;
        });

        await firstEntered.Task;

        using var secondCts = new CancellationTokenSource();
        var second = sequencer.EnqueueAsync(_ =>
        {
            order.Add(2);
            return ValueTask.FromResult(2);
        }, secondCts.Token);

        var third = sequencer.EnqueueAsync(_ =>
        {
            order.Add(3);
            thirdStarted.TrySetResult(true);
            return ValueTask.FromResult(3);
        });

        secondCts.Cancel();

        // A canceled waiter must not release its slot early and allow a later
        // operation to start before the currently running operation completes.
        var startedBeforeFirstCompleted = await Task.WhenAny(
            thirdStarted.Task,
            Task.Delay(TimeSpan.FromMilliseconds(100)));

        Assert.That(startedBeforeFirstCompleted, Is.Not.SameAs(thirdStarted.Task));

        releaseFirst.TrySetResult(true);

        Assert.That(await first, Is.EqualTo(1));
        Assert.ThrowsAsync<OperationCanceledException>(async () => await second);
        Assert.That(await third, Is.EqualTo(3));
        Assert.That(order, Is.EqualTo(new[] { 1, 3 }));
    }
}
 