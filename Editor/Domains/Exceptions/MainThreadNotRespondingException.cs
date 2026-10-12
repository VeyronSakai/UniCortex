using System;

namespace UniCortex.Editor.Domains.Exceptions
{
    /// <summary>
    /// Thrown when a request cannot start on the main thread because <c>EditorApplication.update</c> has not run
    /// for a while, e.g. while a modal dialog is open. The request has not been run.
    /// </summary>
    // Not an InvalidOperationException, which many handlers turn into a 400 response.
    internal sealed class MainThreadNotRespondingException : Exception
    {
        public MainThreadNotRespondingException(TimeSpan unresponsiveTime)
            : base($"The Unity Editor's main thread has not responded for {unresponsiveTime.TotalSeconds:0} seconds, " +
                   "so the request was not run. A modal dialog (e.g. a scene reload confirmation) may be open, " +
                   "or the Editor may be busy (e.g. importing assets).")
        {
        }
    }
}
