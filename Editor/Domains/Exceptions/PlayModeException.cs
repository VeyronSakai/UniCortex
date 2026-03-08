using System;

namespace UniCortex.Editor.Domains.Exceptions
{
    internal sealed class PlayModeException : InvalidOperationException
    {
        public PlayModeException(string message) : base(message)
        {
        }
    }
}
