using System;

namespace UniCortex.Editor.Domains.Exceptions
{
    internal sealed class PackageManagerOperationException : Exception
    {
        public PackageManagerOperationException(string message)
            : base(message)
        {
        }
    }
}
