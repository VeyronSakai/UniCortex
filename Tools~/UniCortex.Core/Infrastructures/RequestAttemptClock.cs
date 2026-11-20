using System.Diagnostics;

namespace UniCortex.Core.Infrastructures;

/// <summary>
/// Measures how long the current attempt of a request has been waiting for the Unity Editor.
/// HttpRequestHandler resends a request until the Editor's server answers, and many attempts end at once even while
/// the request as a whole waits for minutes: GET /tests/result answers with an empty body while tests run, and the
/// connection is refused while the domain reloads. Only an attempt that gets no answer at all shows that the Editor's
/// main thread does not run, e.g. because a modal dialog blocks it.
/// </summary>
internal sealed class RequestAttemptClock
{
    public static readonly HttpRequestOptionsKey<RequestAttemptClock> Key = new(nameof(RequestAttemptClock));

    private long _lastAttemptEnd = Stopwatch.GetTimestamp();

    /// <summary>
    /// The time since the request was sent or its last attempt ended.
    /// </summary>
    public TimeSpan CurrentAttemptDuration => Stopwatch.GetElapsedTime(Interlocked.Read(ref _lastAttemptEnd));

    public void MarkAttemptEnded()
    {
        Interlocked.Exchange(ref _lastAttemptEnd, Stopwatch.GetTimestamp());
    }
}
