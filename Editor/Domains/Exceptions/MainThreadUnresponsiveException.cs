using System;

namespace UniCortex.Editor.Domains.Exceptions
{
    /// <summary>
    /// Thrown when the Unity Editor main thread does not process a dispatched request in time,
    /// typically because a modal dialog is blocking <c>EditorApplication.update</c>.
    /// </summary>
    internal sealed class MainThreadUnresponsiveException : Exception
    {
        public MainThreadUnresponsiveException(TimeSpan unresponsiveDuration)
            : base(BuildMessage(unresponsiveDuration))
        {
        }

        private static string BuildMessage(TimeSpan unresponsiveDuration)
        {
            return $"The Unity Editor main thread has not responded for {unresponsiveDuration.TotalSeconds:F0} seconds. " +
                   "A modal dialog (e.g. \"The open scene(s) have been modified externally\") may be open in the Editor, " +
                   "or the Editor may be busy with its own long-running operation (e.g. an automatic asset import). " +
                   "Please check the Unity Editor. This request was not executed.";
        }
    }
}
